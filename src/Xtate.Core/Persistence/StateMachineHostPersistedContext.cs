#region Copyright © 2019-2020 Sergii Artemenko

// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Service;

namespace Xtate.Persistence
{
	internal sealed class StateMachineHostPersistedContext : StateMachineHostContext
	{
		private const    string   HostPartition      = "StateMachineHost";
		private const    string   ContextKey         = "context";
		private const    int      StateMachinesKey   = 0;
		private const    int      InvokedServicesKey = 1;
		private readonly Uri?     _baseUri;
		private readonly TimeSpan _idlePeriod;

		private readonly Dictionary<(SessionId SessionId, InvokeId InvokeId), InvokedServiceMeta> _invokedServices = new();

		private readonly SemaphoreSlim                           _lockInvokedServices = new(initialCount: 1, maxCount: 1);
		private readonly SemaphoreSlim                           _lockStateMachines   = new(initialCount: 1, maxCount: 1);
		private readonly IStateMachineHost                       _stateMachineHost;
		private readonly Dictionary<SessionId, StateMachineMeta> _stateMachines = new();
		private readonly IStorageProvider                        _storageProvider;
		private          bool                                    _disposed;
		private          int                                     _invokedServiceRecordId;
		private          int                                     _stateMachineRecordId;

		private ITransactionalStorage? _storage;

		public StateMachineHostPersistedContext(IStateMachineHost stateMachineHost, StateMachineHostOptions options) : base(stateMachineHost, options)
		{
			Infrastructure.NotNull(options.StorageProvider);

			_stateMachineHost = stateMachineHost;
			_storageProvider = options.StorageProvider;
			_idlePeriod = options.SuspendIdlePeriod;
			_baseUri = options.BaseUri;
		}

		public override async ValueTask InitializeAsync(CancellationToken token)
		{
			try
			{
				_storage = await _storageProvider.GetTransactionalStorage(HostPartition, ContextKey, token).ConfigureAwait(false);

				await LoadStateMachines(token).ConfigureAwait(false);
				await LoadInvokedServices(token).ConfigureAwait(false);

				await base.InitializeAsync(token).ConfigureAwait(false);
			}
			catch (OperationCanceledException ex) when (ex.CancellationToken == token)
			{
				Stop();

				throw;
			}
		}

		public override async ValueTask DisposeAsync()
		{
			if (_disposed)
			{
				return;
			}

			Stop();

			if (_storage is { } storage)
			{
				await storage.DisposeAsync().ConfigureAwait(false);
			}

			_lockInvokedServices.Dispose();
			_lockStateMachines.Dispose();

			_disposed = true;

			await base.DisposeAsync().ConfigureAwait(false);
		}

		public override async ValueTask AddService(SessionId sessionId, InvokeId invokeId, IService service, CancellationToken token)
		{
			Infrastructure.NotNull(_storage);

			await _lockInvokedServices.WaitAsync(token).ConfigureAwait(false);
			try
			{
				await base.AddService(sessionId, invokeId, service, token).ConfigureAwait(false);

				var bucket = new Bucket(_storage).Nested(InvokedServicesKey);
				var recordId = _invokedServiceRecordId ++;

				var invokedSessionId = service is StateMachineController stateMachineController ? stateMachineController.SessionId : null;
				var invokedService = new InvokedServiceMeta(sessionId, invokeId, invokedSessionId) { RecordId = recordId };
				_invokedServices.Add((sessionId, invokeId), invokedService);

				bucket.Add(Bucket.RootKey, _invokedServiceRecordId);

				invokedService.Store(bucket.Nested(recordId));

				await _storage.CheckPoint(level: 0, StopToken).ConfigureAwait(false);
			}
			finally
			{
				_lockInvokedServices.Release();
			}
		}

		private async ValueTask RemoveInvokedService(SessionId sessionId, InvokeId invokeId)
		{
			Infrastructure.NotNull(_storage);

			if (!_invokedServices.Remove((sessionId, invokeId)))
			{
				return;
			}

			var bucket = new Bucket(_storage).Nested(InvokedServicesKey);
			if (bucket.TryGet(sessionId, out int recordId))
			{
				bucket.RemoveSubtree(recordId);

				await _storage.CheckPoint(level: 0, StopToken).ConfigureAwait(false);
			}

			await ShrinkInvokedServices().ConfigureAwait(false);
		}

		public override async ValueTask<IService?> TryRemoveService(SessionId sessionId, InvokeId invokeId)
		{
			await _lockInvokedServices.WaitAsync(StopToken).ConfigureAwait(false);
			try
			{
				await RemoveInvokedService(sessionId, invokeId).ConfigureAwait(false);

				return await base.TryRemoveService(sessionId, invokeId).ConfigureAwait(false);
			}
			finally
			{
				_lockInvokedServices.Release();
			}
		}

		public override async ValueTask<IService?> TryCompleteService(SessionId sessionId, InvokeId invokeId)
		{
			await _lockInvokedServices.WaitAsync(StopToken).ConfigureAwait(false);
			try
			{
				await RemoveInvokedService(sessionId, invokeId).ConfigureAwait(false);

				return await base.TryCompleteService(sessionId, invokeId).ConfigureAwait(false);
			}
			finally
			{
				_lockInvokedServices.Release();
			}
		}

		protected override StateMachineController CreateStateMachineController(SessionId sessionId, IStateMachine? stateMachine, IStateMachineOptions? stateMachineOptions,
																			   Uri? stateMachineLocation, in InterpreterOptions defaultOptions) =>
				stateMachineOptions.IsStateMachinePersistable()
						? new StateMachinePersistedController(sessionId, stateMachineOptions, stateMachine, stateMachineLocation, _stateMachineHost, _storageProvider, _idlePeriod, in defaultOptions)
						: base.CreateStateMachineController(sessionId, stateMachine, stateMachineOptions, stateMachineLocation, in defaultOptions);

		public override async ValueTask<StateMachineController> CreateAndAddStateMachine(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters, IErrorProcessor errorProcessor,
																						 CancellationToken token)
		{
			Infrastructure.NotNull(_storage);

			var (stateMachine, location) = await LoadStateMachine(origin, _baseUri, errorProcessor, token).ConfigureAwait(false);

			stateMachine.Is<StateMachineOptions>(out var options);

			if (!options.IsStateMachinePersistable())
			{
				return await base.CreateAndAddStateMachine(sessionId, origin, parameters, errorProcessor, token).ConfigureAwait(false);
			}

			await _lockStateMachines.WaitAsync(token).ConfigureAwait(false);
			try
			{
				var stateMachineController = await base.CreateAndAddStateMachine(sessionId, origin, parameters, errorProcessor, token).ConfigureAwait(false);

				var bucket = new Bucket(_storage).Nested(StateMachinesKey);
				var recordId = _stateMachineRecordId ++;

				var stateMachineMeta = new StateMachineMeta(sessionId, options, location) { RecordId = recordId, Controller = stateMachineController };
				_stateMachines.Add(sessionId, stateMachineMeta);

				bucket.Add(Bucket.RootKey, _stateMachineRecordId);

				stateMachineMeta.Store(bucket.Nested(recordId));

				await _storage.CheckPoint(level: 0, StopToken).ConfigureAwait(false);

				return stateMachineController;
			}
			finally
			{
				_lockStateMachines.Release();
			}
		}

		public override async ValueTask RemoveStateMachine(SessionId sessionId)
		{
			Infrastructure.NotNull(_storage);

			await _lockStateMachines.WaitAsync(StopToken).ConfigureAwait(false);
			try
			{
				_stateMachines.Remove(sessionId);

				var bucket = new Bucket(_storage).Nested(StateMachinesKey);
				if (bucket.TryGet(sessionId, out int recordId))
				{
					bucket.RemoveSubtree(recordId);

					await _storage.CheckPoint(level: 0, StopToken).ConfigureAwait(false);
				}

				await ShrinkStateMachines().ConfigureAwait(false);

				await base.RemoveStateMachine(sessionId).ConfigureAwait(false);
			}
			finally
			{
				_lockStateMachines.Release();
			}
		}

		private async ValueTask ShrinkStateMachines()
		{
			Infrastructure.NotNull(_storage);

			if (_stateMachines.Count * 2 > _stateMachineRecordId)
			{
				return;
			}

			_stateMachineRecordId = 0;
			var rootBucket = new Bucket(_storage).Nested(StateMachinesKey);
			rootBucket.RemoveSubtree(Bucket.RootKey);

			foreach (var stateMachine in _stateMachines.Values)
			{
				stateMachine.RecordId = _stateMachineRecordId ++;
				stateMachine.Store(rootBucket.Nested(stateMachine.RecordId));
			}

			if (_stateMachineRecordId > 0)
			{
				rootBucket.Add(Bucket.RootKey, _stateMachineRecordId);
			}

			await _storage.CheckPoint(level: 0, StopToken).ConfigureAwait(false);
			await _storage.Shrink(StopToken).ConfigureAwait(false);
		}

		private async ValueTask LoadStateMachines(CancellationToken token)
		{
			Infrastructure.NotNull(_storage);

			var bucket = new Bucket(_storage).Nested(StateMachinesKey);

			bucket.TryGet(Bucket.RootKey, out _stateMachineRecordId);

			if (_stateMachineRecordId == 0)
			{
				return;
			}

			await _lockStateMachines.WaitAsync(token).ConfigureAwait(false);
			try
			{
				for (var i = 0; i < _stateMachineRecordId; i ++)
				{
					var eventBucket = bucket.Nested(i);
					if (eventBucket.TryGet(Key.TypeInfo, out TypeInfo typeInfo) && typeInfo == TypeInfo.StateMachine)
					{
						var meta = new StateMachineMeta(bucket) { RecordId = i };
						var stateMachineController = AddSavedStateMachine(meta.SessionId, meta.Location, meta, DefaultErrorProcessor.Instance);
						meta.Controller = stateMachineController;

						_stateMachines.Add(meta.SessionId, meta);
					}
				}
			}
			finally
			{
				_lockStateMachines.Release();
			}
		}

		private async ValueTask ShrinkInvokedServices()
		{
			Infrastructure.NotNull(_storage);

			if (_invokedServices.Count * 2 > _invokedServiceRecordId)
			{
				return;
			}

			_invokedServiceRecordId = 0;
			var rootBucket = new Bucket(_storage).Nested(InvokedServicesKey);
			rootBucket.RemoveSubtree(Bucket.RootKey);

			foreach (var invokedService in _invokedServices.Values)
			{
				invokedService.RecordId = _invokedServiceRecordId ++;
				invokedService.Store(rootBucket.Nested(invokedService.RecordId));
			}

			if (_invokedServiceRecordId > 0)
			{
				rootBucket.Add(Bucket.RootKey, _invokedServiceRecordId);
			}

			await _storage.CheckPoint(level: 0, StopToken).ConfigureAwait(false);
			await _storage.Shrink(StopToken).ConfigureAwait(false);
		}

		private async ValueTask LoadInvokedServices(CancellationToken token)
		{
			Infrastructure.NotNull(_storage);

			var bucket = new Bucket(_storage).Nested(InvokedServicesKey);

			bucket.TryGet(Bucket.RootKey, out _invokedServiceRecordId);

			if (_invokedServiceRecordId == 0)
			{
				return;
			}

			await _lockInvokedServices.WaitAsync(token).ConfigureAwait(false);
			try
			{
				for (var i = 0; i < _invokedServiceRecordId; i ++)
				{
					var eventBucket = bucket.Nested(i);
					if (eventBucket.TryGet(Key.TypeInfo, out TypeInfo typeInfo) && typeInfo == TypeInfo.InvokedService)
					{
						var invokedService = new InvokedServiceMeta(bucket) { RecordId = i };

						if (invokedService.SessionId is not null)
						{
							var stateMachine = _stateMachines[invokedService.SessionId];
							Infrastructure.NotNull(stateMachine.Controller);
							await base.AddService(invokedService.ParentSessionId, invokedService.InvokeId, stateMachine.Controller, token).ConfigureAwait(false);

							_invokedServices.Add((invokedService.ParentSessionId, invokedService.InvokeId), invokedService);
						}
						else if (_stateMachines.TryGetValue(invokedService.ParentSessionId, out var invokingStateMachine))
						{
							Infrastructure.NotNull(invokingStateMachine.Controller);
							var evt = new EventObject(EventType.External, EventName.ErrorExecution, data: default, sendId: null, invokedService.InvokeId);
							await invokingStateMachine.Controller.Send(evt, token).ConfigureAwait(false);
						}
					}
				}
			}
			finally
			{
				_lockInvokedServices.Release();
			}
		}

		private class StateMachineMeta : IStoreSupport, IStateMachineOptions
		{
			public StateMachineMeta(SessionId sessionId, IStateMachineOptions? options, Uri? stateMachineLocation)
			{
				SessionId = sessionId;
				Location = stateMachineLocation;

				if (options is not null)
				{
					PersistenceLevel = options.PersistenceLevel;
					SynchronousEventProcessing = options.SynchronousEventProcessing;
					ExternalQueueSize = options.ExternalQueueSize;
					UnhandledErrorBehaviour = options.UnhandledErrorBehaviour;
				}
			}

			public StateMachineMeta(Bucket bucket)
			{
				SessionId = bucket.GetSessionId(Key.SessionId) ?? throw new PersistenceException(Resources.Exception_Missed_SessionId);
				Location = bucket.GetUri(Key.Location);
				Name = bucket.GetString(Key.Name);

				if (bucket.TryGet(Key.OptionPersistenceLevel, out PersistenceLevel persistenceLevel))
				{
					PersistenceLevel = persistenceLevel;
				}

				if (bucket.TryGet(Key.OptionSynchronousEventProcessing, out bool synchronousEventProcessing))
				{
					SynchronousEventProcessing = synchronousEventProcessing;
				}

				if (bucket.TryGet(Key.OptionExternalQueueSize, out int externalQueueSize))
				{
					ExternalQueueSize = externalQueueSize;
				}

				if (bucket.TryGet(Key.UnhandledErrorBehaviour, out UnhandledErrorBehaviour unhandledErrorBehaviour))
				{
					UnhandledErrorBehaviour = unhandledErrorBehaviour;
				}
			}

			public SessionId               SessionId  { get; }
			public Uri?                    Location   { get; }
			public int                     RecordId   { get; set; }
			public StateMachineController? Controller { get; set; }

		#region Interface IStateMachineOptions

			public string?                  Name                       { get; }
			public PersistenceLevel?        PersistenceLevel           { get; }
			public bool?                    SynchronousEventProcessing { get; }
			public int?                     ExternalQueueSize          { get; }
			public UnhandledErrorBehaviour? UnhandledErrorBehaviour    { get; }

		#endregion

		#region Interface IStoreSupport

			public void Store(Bucket bucket)
			{
				bucket.Add(Key.TypeInfo, TypeInfo.StateMachine);
				bucket.AddId(Key.SessionId, SessionId);
				bucket.Add(Key.Location, Location);

				if (Name is { } name)
				{
					bucket.Add(Key.Name, name);
				}

				if (PersistenceLevel is { } persistenceLevel)
				{
					bucket.Add(Key.OptionPersistenceLevel, persistenceLevel);
				}

				if (SynchronousEventProcessing is { } synchronousEventProcessing)
				{
					bucket.Add(Key.OptionSynchronousEventProcessing, synchronousEventProcessing);
				}

				if (ExternalQueueSize is { } externalQueueSize)
				{
					bucket.Add(Key.OptionExternalQueueSize, externalQueueSize);
				}

				if (UnhandledErrorBehaviour is { } unhandledErrorBehaviour)
				{
					bucket.Add(Key.UnhandledErrorBehaviour, unhandledErrorBehaviour);
				}
			}

		#endregion
		}

		private class InvokedServiceMeta : IStoreSupport
		{
			public InvokedServiceMeta(SessionId parentSessionId, InvokeId invokeId, SessionId? sessionId)
			{
				ParentSessionId = parentSessionId;
				InvokeId = invokeId;
				SessionId = sessionId;
			}

			public InvokedServiceMeta(Bucket bucket)
			{
				ParentSessionId = bucket.GetSessionId(Key.ParentSessionId) ?? throw new PersistenceException(Resources.Exception_Missed_ParentSessionId);
				InvokeId = bucket.GetInvokeId(Key.InvokeId) ?? throw new PersistenceException(Resources.Exception_InvokedServiceMeta_Missed_InvokeId);
				SessionId = bucket.GetSessionId(Key.SessionId);
			}

			public SessionId  ParentSessionId { get; }
			public InvokeId   InvokeId        { get; }
			public SessionId? SessionId       { get; }
			public int        RecordId        { get; set; }

		#region Interface IStoreSupport

			public void Store(Bucket bucket)
			{
				bucket.Add(Key.TypeInfo, TypeInfo.InvokedService);
				bucket.AddId(Key.ParentSessionId, ParentSessionId);
				bucket.AddId(Key.InvokeId, InvokeId);
				bucket.AddId(Key.InvokeUniqueId, InvokeId);
				bucket.AddId(Key.SessionId, SessionId);
			}

		#endregion
		}
	}
}