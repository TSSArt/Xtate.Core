// Copyright © 2019-2024 Sergii Artemenko
// 
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

using Xtate.ExternalService;

namespace Xtate.Persistence;

internal sealed class StateMachineHostPersistedContext : StateMachineHostContext
{
	private const string HostPartition = "StateMachineHost";

	private const string ContextKey = "context";

	private const int StateMachinesKey = 0;

	private const int InvokedServicesKey = 1;

	private readonly Uri? _baseUri;

	private readonly TimeSpan? _idlePeriod;

	private readonly Dictionary<(SessionId SessionId, InvokeId InvokeId), InvokedServiceMeta> _invokedServices = [];

	private readonly SemaphoreSlim _lockInvokedServices = new(initialCount: 1, maxCount: 1);

	private readonly SemaphoreSlim _lockStateMachines = new(initialCount: 1, maxCount: 1);

	//private readonly IStateMachineHost                       _stateMachineHost;
	private readonly Dictionary<SessionId, StateMachineMeta> _stateMachines = [];

	private readonly IStorageProvider _storageProvider;

	private bool _disposed;

	private int _invokedServiceRecordId;

	private int _stateMachineRecordId;

	private ITransactionalStorage? _storage;

	public StateMachineHostPersistedContext(StateMachineHostOptions options) : base(options, new PersistedEventSchedulerFactory(options))
	{
		Infra.NotNull(options.StorageProvider);

		//_stateMachineHost = stateMachineHost;
		_storageProvider = options.StorageProvider;
		_idlePeriod = options.SuspendIdlePeriod;
		_baseUri = options.BaseUri;
	}

	public override async ValueTask InitializeAsync()
	{
		try
		{
			_storage = await _storageProvider.GetTransactionalStorage(HostPartition, ContextKey).ConfigureAwait(false);

			await LoadStateMachines(default).ConfigureAwait(false);
			await LoadInvokedServices(default).ConfigureAwait(false);

			await base.InitializeAsync().ConfigureAwait(false);
		}
		catch (OperationCanceledException ex) when (ex.CancellationToken == default) //TODO:
		{
			Stop();

			throw;
		}
	}

	protected override async ValueTask DisposeAsyncCore()
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

		await base.DisposeAsyncCore().ConfigureAwait(false);
	}

	public override async ValueTask AddService(SessionId sessionId,
											   InvokeId invokeId,
											   IExternalService externalService,
											   CancellationToken token)
	{
		Infra.NotNull(_storage);

		await _lockInvokedServices.WaitAsync(token).ConfigureAwait(false);

		try
		{
			await base.AddService(sessionId, invokeId, externalService, token).ConfigureAwait(false);

			var bucket = new Bucket(_storage).Nested(InvokedServicesKey);
			var recordId = _invokedServiceRecordId ++;

			var invokedSessionId = externalService is StateMachineControllerBase stateMachineController ? stateMachineController.SessionId : null;
			var invokedService = new InvokedServiceMeta(sessionId, invokeId, invokedSessionId) { RecordId = recordId };
			_invokedServices.Add((sessionId, invokeId), invokedService);

			bucket.Add(Bucket.RootKey, _invokedServiceRecordId);

			invokedService.Store(bucket.Nested(recordId));

			//TODO:
			await _storage.CheckPoint(level: 0 /*, StopToken*/).ConfigureAwait(false);
		}
		finally
		{
			_lockInvokedServices.Release();
		}
	}

	private async ValueTask RemoveInvokedService(SessionId sessionId, InvokeId invokeId)
	{
		Infra.NotNull(_storage);

		if (!_invokedServices.Remove((sessionId, invokeId)))
		{
			return;
		}

		var bucket = new Bucket(_storage).Nested(InvokedServicesKey);

		if (bucket.TryGet(sessionId, out int recordId))
		{
			bucket.RemoveSubtree(recordId);

			//TODO:
			await _storage.CheckPoint(level: 0 /*, StopToken*/).ConfigureAwait(false);
		}

		await ShrinkInvokedServices().ConfigureAwait(false);
	}

	public override async ValueTask<IExternalService?> TryRemoveService(InvokeId invokeId)
	{
		await _lockInvokedServices.WaitAsync(StopToken).ConfigureAwait(false);

		try
		{
			await RemoveInvokedService( /*sessionId, */ sessionId: null, invokeId).ConfigureAwait(false);

			return await base.TryRemoveService(invokeId).ConfigureAwait(false);
		}
		finally
		{
			_lockInvokedServices.Release();
		}
	}

	public override async ValueTask<IExternalService?> TryCompleteService(InvokeId invokeId)
	{
		await _lockInvokedServices.WaitAsync(StopToken).ConfigureAwait(false);

		try
		{
			await RemoveInvokedService( /*sessionId*/sessionId: null, invokeId).ConfigureAwait(false);

			return await base.TryCompleteService(invokeId).ConfigureAwait(false);
		}
		finally
		{
			_lockInvokedServices.Release();
		}
	}

	protected override StateMachineControllerBase CreateStateMachineController(SessionId sessionId,
																			   IStateMachine? stateMachine,
																			   IStateMachineOptions? stateMachineOptions,
																			   Uri? stateMachineLocation /*,
																			   InterpreterOptions defaultOptions*/

		// SecurityContext securityContext,
		// DeferredFinalizer finalizer
	) =>
		stateMachineOptions.IsStateMachinePersistable()
			? new StateMachinePersistedController(
				  sessionId, stateMachineOptions, stateMachine, stateMachineLocation/*_stateMachineHost*/,
				  _storageProvider, _idlePeriod /*, defaultOption*s*/)
			  {
				  EventQueueWriter = default!,
				  StateMachineInterpreter = default,
				  TaskMonitor = null,
				  StateMachineStatus = null
			  }
			: base.CreateStateMachineController(sessionId, stateMachine, stateMachineOptions, stateMachineLocation /*, defaultOptions*/);

	public override async ValueTask<StateMachineControllerBase> CreateAndAddStateMachine( //ServiceLocator serviceLocator,
		SessionId sessionId,
		StateMachineOrigin origin,
		DataModelValue parameters,
		SecurityContext securityContext,

		//DeferredFinalizer finalizer,
		IErrorProcessor errorProcessor,
		CancellationToken token)
	{
		Infra.NotNull(_storage);

		var (stateMachine, location) = await LoadStateMachine(origin, _baseUri, securityContext, errorProcessor, token).ConfigureAwait(false);

		stateMachine.Is<IStateMachineOptions>(out var options);

		if (!options.IsStateMachinePersistable())
		{
			return await base.CreateAndAddStateMachine(sessionId, origin, parameters, securityContext, errorProcessor, token).ConfigureAwait(false);
		}

		await _lockStateMachines.WaitAsync(token).ConfigureAwait(false);

		try
		{
			var stateMachineController = await base.CreateAndAddStateMachine(sessionId, origin, parameters, securityContext, errorProcessor, token).ConfigureAwait(false);

			var bucket = new Bucket(_storage).Nested(StateMachinesKey);
			var recordId = _stateMachineRecordId ++;

			var stateMachineMeta = new StateMachineMeta(sessionId, options, location, securityContext) { RecordId = recordId, Controller = stateMachineController };
			_stateMachines.Add(sessionId, stateMachineMeta);

			bucket.Add(Bucket.RootKey, _stateMachineRecordId);

			stateMachineMeta.Store(bucket.Nested(recordId));

			//TODO:
			await _storage.CheckPoint(level: 0 /*, StopToken*/).ConfigureAwait(false);

			return stateMachineController;
		}
		finally
		{
			_lockStateMachines.Release();
		}
	}

	public override void RemoveStateMachineController(SessionId sessionId)
	{
		//TODO:uncomment
		/*
		Infra.NotNull(_storage);

		var sessionId = stateMachineController.SessionId;

		await _lockStateMachines.WaitAsync(StopToken).ConfigureAwait(false);
		try
		{
			_stateMachines.Remove(sessionId);

			var bucket = new Bucket(_storage).Nested(StateMachinesKey);
			if (bucket.TryGet(sessionId, out int recordId))
			{
				bucket.RemoveSubtree(recordId);

				//TODO:
				await _storage.CheckPoint(level: 0).ConfigureAwait(false);
			}

			await ShrinkStateMachines().ConfigureAwait(false);

			await base.RemoveStateMachineController(stateMachineController).ConfigureAwait(false);
		}
		finally
		{
			_lockStateMachines.Release();
		}*/
	}

	private async ValueTask ShrinkStateMachines()
	{
		Infra.NotNull(_storage);

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

		//TODO:

		await _storage.CheckPoint(level: 0 /*, StopToken*/).ConfigureAwait(false);
		await _storage.Shrink( /*StopToken*/).ConfigureAwait(false);
	}

	private async ValueTask LoadStateMachines(CancellationToken token)
	{
		Infra.NotNull(_storage);

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
				var stateMachineBucket = bucket.Nested(i);

				if (stateMachineBucket.TryGet(Key.TypeInfo, out TypeInfo typeInfo) && typeInfo == TypeInfo.StateMachine)
				{
					var meta = new StateMachineMeta(stateMachineBucket) { RecordId = i };

					//var finalizer = new DeferredFinalizer();
					var securityContext = SecurityContext.Create(meta.SecurityContextType, meta.Permissions);

					var controller = AddSavedStateMachine(meta.SessionId, meta.Location, meta, securityContext, default! /*TODO*/);
					AddStateMachineController(meta.SessionId, controller);

					//TODO:
					//finalizer.Add(static (ctx, ctrl) => ((StateMachineHostContext) ctx).RemoveStateMachineController((StateMachineControllerBase) ctrl), this, controller);
					//finalizer.Add(controller);

					meta.Controller = controller;

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
		Infra.NotNull(_storage);

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

		//TODO:
		await _storage.CheckPoint(level: 0 /*, StopToken*/).ConfigureAwait(false);
		await _storage.Shrink( /*StopToken*/).ConfigureAwait(false);
	}

	private async ValueTask LoadInvokedServices(CancellationToken token)
	{
		Infra.NotNull(_storage);

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
						Infra.NotNull(stateMachine.Controller);
						await base.AddService(invokedService.ParentSessionId, invokedService.InvokeId, stateMachine.Controller, token).ConfigureAwait(false);

						_invokedServices.Add((invokedService.ParentSessionId, invokedService.InvokeId), invokedService);
					}
					else if (_stateMachines.TryGetValue(invokedService.ParentSessionId, out var invokingStateMachine))
					{
						Infra.NotNull(invokingStateMachine.Controller);
						var incomingEvent = new IncomingEvent { Type = EventType.External, Name = EventName.ErrorExecution, InvokeId = invokedService.InvokeId };
						await invokingStateMachine.Controller.Dispatch(incomingEvent, token).ConfigureAwait(false);
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
		public StateMachineMeta(SessionId sessionId,
								IStateMachineOptions? options,
								Uri? stateMachineLocation,
								SecurityContext securityContext)
		{
			SessionId = sessionId;
			Location = stateMachineLocation;
			SecurityContextType = securityContext.Type;
			Permissions = securityContext.Permissions;

			if (options is not null)
			{
				PersistenceLevel = options.PersistenceLevel;
				SynchronousEventProcessing = options.SynchronousEventProcessing;
				ExternalQueueSize = options.ExternalQueueSize;
				UnhandledErrorBehaviour = options.UnhandledErrorBehaviour;
			}
		}

		public StateMachineMeta(in Bucket bucket)
		{
			SessionId = bucket.GetSessionId(Key.SessionId) ?? throw new PersistenceException(Resources.Exception_MissedSessionId);
			Location = bucket.GetUri(Key.Location);
			Name = bucket.GetString(Key.Name);

			if (bucket.TryGet(Key.SecurityContextType, out SecurityContextType securityContextType))
			{
				SecurityContextType = securityContextType;
			}

			if (bucket.TryGet(Key.SecurityContextPermissions, out SecurityContextPermissions permissions))
			{
				Permissions = permissions;
			}

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

		public SessionId SessionId { get; }

		public Uri? Location { get; }

		public int RecordId { get; set; }

		public StateMachineControllerBase? Controller { get; set; }

		public SecurityContextType SecurityContextType { get; }

		public SecurityContextPermissions Permissions { get; }

	#region Interface IStateMachineOptions

		public string? Name { get; }

		public PersistenceLevel? PersistenceLevel { get; }

		public bool? SynchronousEventProcessing { get; }

		public int? ExternalQueueSize { get; }

		public UnhandledErrorBehaviour? UnhandledErrorBehaviour { get; }

	#endregion

	#region Interface IStoreSupport

		public void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.StateMachine);
			bucket.AddId(Key.SessionId, SessionId);
			bucket.Add(Key.Location, Location);
			bucket.Add(Key.SecurityContextType, SecurityContextType);
			bucket.Add(Key.SecurityContextPermissions, Permissions);

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

		public InvokedServiceMeta(in Bucket bucket)
		{
			ParentSessionId = bucket.GetSessionId(Key.ParentSessionId) ?? throw new PersistenceException(Resources.Exception_MissedParentSessionId);
			InvokeId = bucket.GetInvokeId(Key.InvokeId) ?? throw new PersistenceException(Resources.Exception_InvokedServiceMetaMissedInvokeId);
			SessionId = bucket.GetSessionId(Key.SessionId);
		}

		public SessionId ParentSessionId { get; }

		public InvokeId InvokeId { get; }

		public SessionId? SessionId { get; }

		public int RecordId { get; set; }

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