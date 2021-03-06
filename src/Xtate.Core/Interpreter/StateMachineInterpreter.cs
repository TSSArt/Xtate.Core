﻿#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xtate.DataModel;
using Xtate.DataModel.Null;
using Xtate.DataModel.Runtime;
using Xtate.Persistence;

namespace Xtate.Core
{
	using DefaultHistoryContent = Dictionary<IIdentifier, ImmutableArray<IExecEvaluator>>;

	[PublicAPI]
	public sealed partial class StateMachineInterpreter
	{
		private const string StateStorageKey                  = "state";
		private const string StateMachineDefinitionStorageKey = "smd";

		private static readonly ImmutableArray<IDataModelHandlerFactory> PredefinedDataModelHandlerFactories =
			ImmutableArray.Create(NullDataModelHandler.Factory, RuntimeDataModelHandler.Factory);

		private readonly ChannelReader<IEvent>               _eventChannel;
		private readonly bool                                _isPersistingEnabled;
		private readonly InterpreterOptions                  _options;
		private readonly SessionId                           _sessionId;
		private readonly IStateMachineValidator              _stateMachineValidator = StateMachineValidator.Instance;
		private          CancellationToken                   _anyToken;
		private          IStateMachineContext                _context = default!;
		private          IDataModelHandler?                  _dataModelHandler;
		private          ImmutableDictionary<string, string> _dataModelVars = ImmutableDictionary<string, string>.Empty;
		private          DataModelValue                      _doneData;
		private          InterpreterModel?                   _model;
		private          IStateMachine?                      _stateMachine;
		private          bool                                _stop;

		private StateMachineInterpreter(SessionId sessionId,
										IStateMachine? stateMachine,
										ChannelReader<IEvent> eventChannel,
										InterpreterOptions options)
		{
			_sessionId = sessionId;
			_stateMachine = stateMachine;
			_eventChannel = eventChannel;
			_options = options;
			_isPersistingEnabled = _options.PersistenceLevel != PersistenceLevel.None && _options.StorageProvider is not null;
		}

		private bool Running
		{
			get => !_stop && (!_isPersistingEnabled || _context.PersistenceContext.GetState((int) StateBagKey.Stop) == 0);
			set
			{
				_stop = !value;

				if (_isPersistingEnabled)
				{
					_context.PersistenceContext.SetState((int) StateBagKey.Stop, value ? 0 : 1);
				}
			}
		}

		public static ValueTask<DataModelValue> RunAsync(IStateMachine? stateMachine, ChannelReader<IEvent> eventChannel, InterpreterOptions? options = default)
		{
			if (eventChannel is null) throw new ArgumentNullException(nameof(eventChannel));

			return new StateMachineInterpreter(SessionId.New(), stateMachine, eventChannel, options ?? InterpreterOptions.Default).Run();
		}

		public static ValueTask<DataModelValue> RunAsync(SessionId sessionId,
														 IStateMachine? stateMachine,
														 ChannelReader<IEvent> eventChannel,
														 InterpreterOptions? options = default)
		{
			if (sessionId is null) throw new ArgumentNullException(nameof(sessionId));
			if (eventChannel is null) throw new ArgumentNullException(nameof(eventChannel));

			return new StateMachineInterpreter(sessionId, stateMachine, eventChannel, options ?? InterpreterOptions.Default).Run();
		}

		private InterpreterModelBuilder.Parameters CreateInterpreterModelBuilderParameters()
		{
			Infra.NotNull(_stateMachine);
			Infra.NotNull(_dataModelHandler);

			return new InterpreterModelBuilder.Parameters(_stateMachine, _dataModelHandler)
				   {
					   BaseUri = _options.BaseUri,
					   CustomActionProviders = _options.CustomActionProviders,
					   ErrorProcessor = _options.ErrorProcessor,
					   ResourceLoaderFactories = _options.ResourceLoaderFactories,
					   SecurityContext = _options.SecurityContext,
					   Logger = _options.Logger,
					   LoggerContext = this
				   };
		}

		private async ValueTask<InterpreterModel> BuildInterpreterModel()
		{
			var interpreterModel = _isPersistingEnabled ? await TryRestoreInterpreterModel().ConfigureAwait(false) : null;

			if (interpreterModel is not null)
			{
				return interpreterModel;
			}

			Infra.NotNull(_stateMachine);

			_dataModelHandler = await CreateDataModelHandler(_stateMachine.DataModelType).ConfigureAwait(false);

			_stateMachineValidator.Validate(_stateMachine, _options.ErrorProcessor);

			var parameters = CreateInterpreterModelBuilderParameters();
			var interpreterModelBuilder = new InterpreterModelBuilder(parameters);

			try
			{
				interpreterModel = await interpreterModelBuilder.Build(_options.StopToken).ConfigureAwait(false);
			}
			finally
			{
				_options.ErrorProcessor?.ThrowIfErrors();
			}

			if (_isPersistingEnabled)
			{
				await SaveInterpreterModel(interpreterModel).ConfigureAwait(false);
			}

			return interpreterModel;
		}

		private async ValueTask<InterpreterModel?> TryRestoreInterpreterModel()
		{
			Infra.NotNull(_options.StorageProvider);

			var storage = await _options.StorageProvider.GetTransactionalStorage(partition: default, StateMachineDefinitionStorageKey, _options.StopToken).ConfigureAwait(false);
			await using (storage.ConfigureAwait(false))
			{
				var bucket = new Bucket(storage);

				if (bucket.TryGet(Key.Version, out int version) && version != 1)
				{
					throw new PersistenceException(Resources.Exception_PersistedStateCantBeReadUnsupportedVersion);
				}

				var storedSessionId = bucket.GetSessionId(Key.SessionId);
				if (storedSessionId is not null && storedSessionId != _sessionId)
				{
					throw new PersistenceException(Resources.Exception_PersistedStateCantBeReadStoredAndProvidedSessionIdsDoesNotMatch);
				}

				if (!bucket.TryGet(Key.StateMachineDefinition, out var memory))
				{
					return null;
				}

				var smdBucket = new Bucket(new InMemoryStorage(memory.Span));
				var dataModelType = smdBucket.GetString(Key.DataModelType);
				_dataModelHandler = await CreateDataModelHandler(dataModelType).ConfigureAwait(false);

				ImmutableDictionary<int, IEntity>? entityMap = default;

				if (_stateMachine is not null)
				{
					var parameters = CreateInterpreterModelBuilderParameters();
					var temporaryModelBuilder = new InterpreterModelBuilder(parameters);
					var model = await temporaryModelBuilder.Build(_options.StopToken).ConfigureAwait(false);
					entityMap = model.EntityMap;
				}

				var restoredStateMachine = new StateMachineReader().Build(smdBucket, entityMap);

				if (_stateMachine is not null)
				{
					//TODO: Validate stateMachine vs restoredStateMachine (number of elements should be the same and documentId should point to the same entity type)
				}

				_stateMachine = restoredStateMachine;

				try
				{
					var parameters = CreateInterpreterModelBuilderParameters();
					var interpreterModelBuilder = new InterpreterModelBuilder(parameters);

					return await interpreterModelBuilder.Build(_options.StopToken).ConfigureAwait(false);
				}
				finally
				{
					_options.ErrorProcessor?.ThrowIfErrors();
				}
			}
		}

		private async ValueTask SaveInterpreterModel(InterpreterModel interpreterModel)
		{
			Infra.NotNull(_options.StorageProvider);

			var storage = await _options.StorageProvider.GetTransactionalStorage(partition: default, StateMachineDefinitionStorageKey, _options.StopToken).ConfigureAwait(false);
			await using (storage.ConfigureAwait(false))
			{
				SaveToStorage(interpreterModel.Root.As<IStoreSupport>(), new Bucket(storage));

				await storage.CheckPoint(level: 0, _options.StopToken).ConfigureAwait(false);
			}
		}

		private void SaveToStorage(IStoreSupport root, in Bucket bucket)
		{
			var memoryStorage = new InMemoryStorage();
			root.Store(new Bucket(memoryStorage));

			Span<byte> span = stackalloc byte[memoryStorage.GetTransactionLogSize()];
			memoryStorage.WriteTransactionLogToSpan(span);

			bucket.Add(Key.Version, value: 1);
			bucket.AddId(Key.SessionId, _sessionId);
			bucket.Add(Key.StateMachineDefinition, span);
		}

		private async ValueTask<IDataModelHandler> CreateDataModelHandler(string? dataModelType)
		{
			dataModelType ??= NullDataModelHandler.DataModelType;
			var factoryContext = new FactoryContext(_options.ResourceLoaderFactories, _options.SecurityContext, _options.Logger, this);
			var activator = await FindDataModelHandlerFactoryActivator(dataModelType, factoryContext).ConfigureAwait(false);

			if (activator is not null)
			{
				return await activator.CreateHandler(factoryContext, dataModelType, _options.ErrorProcessor, _options.StopToken).ConfigureAwait(false);
			}

			_options.ErrorProcessor.AddError<StateMachineInterpreter>(entity: null, Res.Format(Resources.Exception_CantFindDataModelHandlerFactoryForDataModelType, dataModelType));

			return new NullDataModelHandler(_options.ErrorProcessor);
		}

		private async ValueTask<IDataModelHandlerFactoryActivator?> FindDataModelHandlerFactoryActivator(string dataModelType, IFactoryContext factoryContext)
		{
			if (!_options.DataModelHandlerFactories.IsDefaultOrEmpty)
			{
				foreach (var factory in _options.DataModelHandlerFactories)
				{
					var activator = await factory.TryGetActivator(factoryContext, dataModelType, _options.StopToken).ConfigureAwait(false);

					if (activator is not null)
					{
						return activator;
					}
				}
			}

			foreach (var factory in PredefinedDataModelHandlerFactories)
			{
				var activator = await factory.TryGetActivator(factoryContext, dataModelType ?? NullDataModelHandler.DataModelType, _options.StopToken).ConfigureAwait(false);

				if (activator is not null)
				{
					return activator;
				}
			}

			return null;
		}

		private ValueTask DoOperation(StateBagKey key, Func<ValueTask> func)
		{
			return _isPersistingEnabled ? DoOperationAsync() : func();

			async ValueTask DoOperationAsync()
			{
				var persistenceContext = _context.PersistenceContext;
				if (persistenceContext.GetState((int) key) == 0)
				{
					await func().ConfigureAwait(false);

					persistenceContext.SetState((int) key, value: 1);
				}
			}
		}

		private ValueTask DoOperation<TArg>(StateBagKey key, Func<TArg, ValueTask> func, TArg arg)
		{
			return _isPersistingEnabled ? DoOperationAsync() : func(arg);

			async ValueTask DoOperationAsync()
			{
				var persistenceContext = _context.PersistenceContext;
				if (persistenceContext.GetState((int) key) == 0)
				{
					await func(arg).ConfigureAwait(false);

					persistenceContext.SetState((int) key, value: 1);
				}
			}
		}

		private ValueTask DoOperation<TArg>(StateBagKey key,
											IEntity entity,
											Func<TArg, ValueTask> func,
											TArg arg)
		{
			return _isPersistingEnabled ? DoOperationAsync() : func(arg);

			async ValueTask DoOperationAsync()
			{
				var documentId = entity.As<IDocumentId>().DocumentId;

				var persistenceContext = _context.PersistenceContext;
				if (persistenceContext.GetState((int) key, documentId) == 0)
				{
					await func(arg).ConfigureAwait(false);

					persistenceContext.SetState((int) key, documentId, value: 1);
				}
			}
		}

		private void Complete(StateBagKey key)
		{
			if (_isPersistingEnabled)
			{
				_context.PersistenceContext.ClearState((int) key);
			}
		}

		private bool Capture(StateBagKey key, bool value)
		{
			if (_isPersistingEnabled)
			{
				var persistenceContext = _context.PersistenceContext;
				if (persistenceContext.GetState((int) key) == 1)
				{
					return persistenceContext.GetState((int) key, subKey: 0) == 1;
				}

				persistenceContext.SetState((int) key, subKey: 0, value ? 1 : 0);
				persistenceContext.SetState((int) key, value: 1);
			}

			return value;
		}

		private ValueTask<List<TransitionNode>> Capture(StateBagKey key, Func<ValueTask<List<TransitionNode>>> value)
		{
			return _isPersistingEnabled ? CaptureAsync() : value();

			async ValueTask<List<TransitionNode>> CaptureAsync()
			{
				var persistenceContext = _context.PersistenceContext;
				if (persistenceContext.GetState((int) key) == 0)
				{
					var list = await value().ConfigureAwait(false);
					persistenceContext.SetState((int) key, subKey: 0, list.Count);

					for (var i = 0; i < list.Count; i ++)
					{
						persistenceContext.SetState((int) key, i + 1, list[i].As<IDocumentId>().DocumentId);
					}

					persistenceContext.SetState((int) key, value: 1);

					return list;
				}

				var length = persistenceContext.GetState((int) key, subKey: 0);
				var capturedSet = new List<TransitionNode>(length);

				Infra.NotNull(_model);

				for (var i = 0; i < length; i ++)
				{
					var documentId = persistenceContext.GetState((int) key, i + 1);
					capturedSet.Add(_model.EntityMap[documentId].As<TransitionNode>());
				}

				return capturedSet;
			}
		}

		private ValueTask NotifyAccepted() => NotifyInterpreterState(StateMachineInterpreterState.Accepted);
		private ValueTask NotifyStarted()  => NotifyInterpreterState(StateMachineInterpreterState.Started);
		private ValueTask NotifyExited()   => NotifyInterpreterState(StateMachineInterpreterState.Exited);
		private ValueTask NotifyWaiting()  => NotifyInterpreterState(StateMachineInterpreterState.Waiting);

		private async ValueTask NotifyInterpreterState(StateMachineInterpreterState state)
		{
			await TraceInterpreterState(state).ConfigureAwait(false);

			if (_options.NotifyStateChanged is { } notifyStateChanged)
			{
				await notifyStateChanged.OnChanged(state).ConfigureAwait(false);
			}
		}

		private async ValueTask RunSteps()
		{
			try
			{
				await DoOperation(StateBagKey.NotifyAccepted, NotifyAccepted).ConfigureAwait(false);
				await DoOperation(StateBagKey.InitializeRootDataModel, InitializeRootDataModel).ConfigureAwait(false);
				await DoOperation(StateBagKey.EarlyInitializeDataModel, InitializeAllDataModels).ConfigureAwait(false);
				await DoOperation(StateBagKey.ExecuteGlobalScript, ExecuteGlobalScript).ConfigureAwait(false);
				await DoOperation(StateBagKey.NotifyStarted, NotifyStarted).ConfigureAwait(false);
				await DoOperation(StateBagKey.InitialEnterStates, InitialEnterStates).ConfigureAwait(false);
				await DoOperation(StateBagKey.MainEventLoop, MainEventLoop).ConfigureAwait(false);
			}
			catch (StateMachineLiveLockException ex)
			{
				throw await DestroyingSteps(ex).ConfigureAwait(false);
			}
			catch (OperationCanceledException ex) when (ex.CancellationToken == _options.DestroyToken || ex.CancellationToken == _anyToken && _options.DestroyToken.IsCancellationRequested)
			{
				throw await DestroyingSteps(ex).ConfigureAwait(false);
			}
			catch (StateMachineUnhandledErrorException ex) when (ex.UnhandledErrorBehaviour == UnhandledErrorBehaviour.DestroyStateMachine)
			{
				throw await DestroyingSteps(ex).ConfigureAwait(false);
			}

			await ExitSteps().ConfigureAwait(false);
		}

		private async ValueTask ExitSteps()
		{
			await DoOperation(StateBagKey.ExitInterpreter, ExitInterpreter).ConfigureAwait(false);
			await DoOperation(StateBagKey.NotifyExited, NotifyExited).ConfigureAwait(false);
			await CleanupPersistedData().ConfigureAwait(false);
		}

		private async ValueTask<StateMachineDestroyedException> DestroyingSteps(Exception destroyException)
		{
			await TraceInterpreterState(StateMachineInterpreterState.Destroying).ConfigureAwait(false);

			await ExitSteps().ConfigureAwait(false);

			return new StateMachineDestroyedException(Resources.Exception_StateMachineHasBeenDestroyed, destroyException);
		}

		private async ValueTask<DataModelValue> Run()
		{
			_model = await BuildInterpreterModel().ConfigureAwait(false);
			_context = await CreateContext().ConfigureAwait(false);
			var anyTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_options.SuspendToken, _options.DestroyToken, _options.StopToken);

			using (anyTokenSource)
			await using (_context.ConfigureAwait(false))
			{
				_anyToken = anyTokenSource.Token;

				if (_stateMachine is null)
				{
					await TraceInterpreterState(StateMachineInterpreterState.Resumed).ConfigureAwait(false);
				}

				try
				{
					await RunSteps().ConfigureAwait(false);
				}
				catch (ChannelClosedException ex)
				{
					await TraceInterpreterState(StateMachineInterpreterState.QueueClosed).ConfigureAwait(false);

					throw new StateMachineQueueClosedException(Resources.Exception_StateMachineExternalQueueHasBeenClosed, ex);
				}
				catch (OperationCanceledException ex) when (ex.CancellationToken == _options.StopToken || ex.CancellationToken == _anyToken && _options.StopToken.IsCancellationRequested)
				{
					await TraceInterpreterState(StateMachineInterpreterState.Halted).ConfigureAwait(false);

					throw new OperationCanceledException(Resources.Exception_StateMachineHasBeenHalted, ex, _options.StopToken);
				}
				catch (StateMachineUnhandledErrorException ex) when (ex.UnhandledErrorBehaviour == UnhandledErrorBehaviour.HaltStateMachine)
				{
					await TraceInterpreterState(StateMachineInterpreterState.Halted).ConfigureAwait(false);

					throw;
				}
				catch (OperationCanceledException ex) when (ex.CancellationToken == _options.SuspendToken ||
															ex.CancellationToken == _anyToken && _options.SuspendToken.IsCancellationRequested)
				{
					await TraceInterpreterState(StateMachineInterpreterState.Suspended).ConfigureAwait(false);

					throw new StateMachineSuspendedException(Resources.Exception_StateMachineHasBeenSuspended, ex);
				}

				return _doneData;
			}
		}

		private async ValueTask CleanupPersistedData()
		{
			if (_isPersistingEnabled)
			{
				Infra.NotNull(_options.StorageProvider);

				await _options.StorageProvider.RemoveTransactionalStorage(partition: default, StateStorageKey, _options.StopToken).ConfigureAwait(false);
				await _options.StorageProvider.RemoveTransactionalStorage(partition: default, StateMachineDefinitionStorageKey, _options.StopToken).ConfigureAwait(false);
			}
		}

		private async ValueTask InitializeAllDataModels()
		{
			Infra.NotNull(_model);

			if (_model.Root.Binding == BindingType.Early && !_model.DataModelList.IsDefaultOrEmpty)
			{
				foreach (var node in _model.DataModelList)
				{
					await DoOperation(StateBagKey.InitializeDataModel, node, InitializeDataModel, node).ConfigureAwait(false);
				}
			}
		}

		private ValueTask InitialEnterStates()
		{
			Infra.NotNull(_model);

			return EnterStates(new List<TransitionNode>(1) { _model.Root.Initial.Transition });
		}

		private async ValueTask MainEventLoop()
		{
			var exit = false;

			while (Capture(StateBagKey.Running, Running && !exit))
			{
				_anyToken.ThrowIfCancellationRequested();

				await DoOperation(StateBagKey.InternalQueueProcessing, InternalQueueProcessing).ConfigureAwait(false);

				exit = await ExternalQueueProcessing().ConfigureAwait(false);

				Complete(StateBagKey.InternalQueueProcessing);
				Complete(StateBagKey.Running);
			}

			Complete(StateBagKey.Running);
		}

		private async ValueTask InternalQueueProcessing()
		{
			var liveLockDetector = LiveLockDetector.Create();
			var exit = false;

			try
			{
				while (Capture(StateBagKey.Running, Running && !exit))
				{
					_anyToken.ThrowIfCancellationRequested();

					liveLockDetector.Iteration(_context.InternalQueue.Count);

					exit = await InternalQueueProcessingIteration().ConfigureAwait(false);

					Complete(StateBagKey.Running);
				}

				Complete(StateBagKey.Running);
			}
			finally
			{
				liveLockDetector.Dispose();
			}
		}

		private async ValueTask<List<TransitionNode>> SelectInternalEventTransitions()
		{
			Infra.NotNull(_dataModelHandler);

			var internalEvent = _context.InternalQueue.Dequeue();

			var eventModel = DataConverter.FromEvent(internalEvent, _dataModelHandler.CaseInsensitive);
			_context.DataModel.SetInternal(key: @"_event", _dataModelHandler.CaseInsensitive, eventModel, DataModelAccess.ReadOnly);

			await TraceProcessingEvent(internalEvent).ConfigureAwait(false);

			var transitions = await SelectTransitions(internalEvent).ConfigureAwait(false);

			if (transitions.Count == 0 && EventName.IsError(internalEvent.NameParts))
			{
				UnhandledErrorEvent(internalEvent);
			}

			return transitions;
		}

		private void UnhandledErrorEvent(IEvent evt)
		{
			switch (_options.UnhandledErrorBehaviour)
			{
				case UnhandledErrorBehaviour.IgnoreError:
					return;

				case UnhandledErrorBehaviour.HaltStateMachine:
				case UnhandledErrorBehaviour.DestroyStateMachine:

					evt.Is<Exception>(out var exception);

					throw new StateMachineUnhandledErrorException(Resources.Exception_UnhandledException, exception, _options.UnhandledErrorBehaviour);

				default:
					Infra.Unexpected(_options.UnhandledErrorBehaviour);
					break;
			}
		}

		private async ValueTask<bool> InternalQueueProcessMessage()
		{
			var exit = false;

			if (Capture(StateBagKey.InternalQueueNonEmpty, _context.InternalQueue.Count > 0))
			{
				var transitions = await Capture(StateBagKey.SelectInternalEventTransitions, SelectInternalEventTransitions).ConfigureAwait(false);

				if (transitions.Count > 0)
				{
					await Microstep(transitions).ConfigureAwait(false);

					await CheckPoint(PersistenceLevel.Transition).ConfigureAwait(false);
				}

				Complete(StateBagKey.SelectInternalEventTransitions);
			}
			else
			{
				exit = true;
			}

			Complete(StateBagKey.InternalQueueNonEmpty);

			return exit;
		}

		private async ValueTask<bool> InternalQueueProcessingIteration()
		{
			var exit = false;
			var transitions = await Capture(StateBagKey.EventlessTransitions, SelectEventlessTransitions).ConfigureAwait(false);

			if (transitions.Count > 0)
			{
				await Microstep(transitions).ConfigureAwait(false);

				await CheckPoint(PersistenceLevel.Transition).ConfigureAwait(false);
			}
			else
			{
				exit = await InternalQueueProcessMessage().ConfigureAwait(false);
			}

			Complete(StateBagKey.EventlessTransitions);

			return exit;
		}

		private async ValueTask<bool> ExternalQueueProcessing()
		{
			var exit = false;

			if (Capture(StateBagKey.Running2, Running))
			{
				foreach (var state in _context.StatesToInvoke.ToSortedList(StateEntityNode.EntryOrder))
				{
					foreach (var invoke in state.Invoke)
					{
						await DoOperation(StateBagKey.Invoke, invoke, Invoke, invoke).ConfigureAwait(false);
					}
				}

				_context.StatesToInvoke.Clear();
				Complete(StateBagKey.Invoke);

				if (Capture(StateBagKey.InternalQueueEmpty, _context.InternalQueue.Count == 0))
				{
					_anyToken.ThrowIfCancellationRequested();

					var transitions = await Capture(StateBagKey.ExternalEventTransitions, ExternalEventTransitions).ConfigureAwait(false);

					if (transitions.Count > 0)
					{
						await Microstep(transitions).ConfigureAwait(false);

						await CheckPoint(PersistenceLevel.Event).ConfigureAwait(false);
					}

					Complete(StateBagKey.ExternalEventTransitions);
				}

				Complete(StateBagKey.InternalQueueEmpty);
			}
			else
			{
				exit = true;
			}

			Complete(StateBagKey.Running2);

			return exit;
		}

		private async ValueTask<List<TransitionNode>> ExternalEventTransitions()
		{
			Infra.NotNull(_dataModelHandler);

			var externalEvent = await ReadExternalEvent().ConfigureAwait(false);

			var eventModel = DataConverter.FromEvent(externalEvent, _dataModelHandler.CaseInsensitive);
			_context.DataModel.SetInternal(key: @"_event", _dataModelHandler.CaseInsensitive, eventModel, DataModelAccess.ReadOnly);

			await TraceProcessingEvent(externalEvent).ConfigureAwait(false);

			foreach (var state in _context.Configuration)
			{
				foreach (var invoke in state.Invoke)
				{
					if (InvokeId.InvokeUniqueIdComparer.Equals(invoke.InvokeId, externalEvent.InvokeId))
					{
						await ApplyFinalize(invoke).ConfigureAwait(false);
					}

					if (invoke.AutoForward)
					{
						await ForwardEvent(invoke, externalEvent).ConfigureAwait(false);
					}
				}
			}

			return await SelectTransitions(externalEvent).ConfigureAwait(false);
		}

		private async ValueTask CheckPoint(PersistenceLevel level)
		{
			if (!_isPersistingEnabled || _options.PersistenceLevel < level)
			{
				return;
			}

			var persistenceContext = _context.PersistenceContext;
			await persistenceContext.CheckPoint((int) level, _options.StopToken).ConfigureAwait(false);

			if (level == PersistenceLevel.StableState)
			{
				await persistenceContext.Shrink(_options.StopToken).ConfigureAwait(false);
			}
		}

		private async ValueTask<IEvent> ReadExternalEvent()
		{
			while (true)
			{
				var evt = await ReadExternalEventUnfiltered().ConfigureAwait(false);

				if (evt.InvokeId is null)
				{
					return evt;
				}

				if (IsInvokeActive(evt.InvokeId))
				{
					return evt;
				}
			}
		}

		private bool IsInvokeActive(InvokeId invokeId) => _context.ActiveInvokes.Contains(invokeId);

		private async ValueTask<IEvent> ReadExternalEventUnfiltered()
		{
			if (_eventChannel.TryRead(out var evt))
			{
				return evt;
			}

			await CheckPoint(PersistenceLevel.StableState).ConfigureAwait(false);

			await NotifyWaiting().ConfigureAwait(false);

			return await _eventChannel.ReadAsync(_anyToken).ConfigureAwait(false);
		}

		private async ValueTask ExitInterpreter()
		{
			var statesToExit = _context.Configuration.ToSortedList(StateEntityNode.ExitOrder);

			foreach (var state in statesToExit)
			{
				foreach (var onExit in state.OnExit)
				{
					await DoOperation(StateBagKey.OnExit, onExit, RunExecutableEntity, onExit.ActionEvaluators).ConfigureAwait(false);
				}

				foreach (var invoke in state.Invoke)
				{
					await CancelInvoke(invoke).ConfigureAwait(false);
				}

				_context.Configuration.Delete(state);

				if (state is FinalNode { Parent: StateMachineNode } final)
				{
					await DoOperation(StateBagKey.ReturnDoneEvent, state, EvaluateDoneData, final).ConfigureAwait(false);
				}
			}

			Complete(StateBagKey.ReturnDoneEvent);
			Complete(StateBagKey.OnExit);
		}

		private ValueTask<List<TransitionNode>> SelectEventlessTransitions() => SelectTransitions(evt: null);

		private async ValueTask<List<TransitionNode>> SelectTransitions(IEvent? evt)
		{
			var transitions = new List<TransitionNode>();

			foreach (var state in _context.Configuration.ToFilteredSortedList(s => s.IsAtomicState, StateEntityNode.EntryOrder))
			{
				await FindTransitionForState(transitions, state, evt).ConfigureAwait(false);
			}

			return RemoveConflictingTransitions(transitions);
		}

		private async ValueTask FindTransitionForState(List<TransitionNode> transitionNodes, StateEntityNode state, IEvent? evt)
		{
			foreach (var transition in state.Transitions)
			{
				if (EventMatch(transition.EventDescriptors, evt) && await ConditionMatch(transition).ConfigureAwait(false))
				{
					transitionNodes.Add(transition);

					return;
				}
			}

			if (state.Parent is not StateMachineNode)
			{
				await FindTransitionForState(transitionNodes, state.Parent!, evt).ConfigureAwait(false);
			}
		}

		private static bool EventMatch(ImmutableArray<IEventDescriptor> eventDescriptors, IEvent? evt)
		{
			if (evt is null)
			{
				return eventDescriptors.IsDefaultOrEmpty;
			}

			if (eventDescriptors.IsDefaultOrEmpty)
			{
				return false;
			}

			foreach (var eventDescriptor in eventDescriptors)
			{
				if (eventDescriptor.IsEventMatch(evt))
				{
					return true;
				}
			}

			return false;
		}

		private async ValueTask<bool> ConditionMatch(TransitionNode transition)
		{
			var condition = transition.ConditionEvaluator;

			if (condition is null)
			{
				return true;
			}

			_options.StopToken.ThrowIfCancellationRequested();

			try
			{
				return await condition.EvaluateBoolean(_context.ExecutionContext, _options.StopToken).ConfigureAwait(false);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(transition, ex).ConfigureAwait(false);

				return false;
			}
		}

		private List<TransitionNode> RemoveConflictingTransitions(List<TransitionNode> enabledTransitions)
		{
			var filteredTransitions = new List<TransitionNode>();
			List<TransitionNode>? transitionsToRemove = default;
			List<TransitionNode>? tr1 = default;
			List<TransitionNode>? tr2 = default;

			foreach (var t1 in enabledTransitions)
			{
				var t1Preempted = false;
				transitionsToRemove?.Clear();

				foreach (var t2 in filteredTransitions)
				{
					(tr1 ??= new List<TransitionNode>(1) { default! })[0] = t1;
					(tr2 ??= new List<TransitionNode>(1) { default! })[0] = t2;

					if (HasIntersection(ComputeExitSet(tr1), ComputeExitSet(tr2)))
					{
						if (IsDescendant(t1.Source, t2.Source))
						{
							(transitionsToRemove ??= new List<TransitionNode>()).Add(t2);
						}
						else
						{
							t1Preempted = true;
							break;
						}
					}
				}

				if (!t1Preempted)
				{
					if (transitionsToRemove is not null)
					{
						foreach (var t3 in transitionsToRemove)
						{
							filteredTransitions.Remove(t3);
						}
					}

					filteredTransitions.Add(t1);
				}
			}

			return filteredTransitions;
		}

		private async ValueTask Microstep(List<TransitionNode> enabledTransitions)
		{
			await DoOperation(StateBagKey.ExitStates, ExitStates, enabledTransitions).ConfigureAwait(false);

			await DoOperation(StateBagKey.ExecuteTransitionContent, ExecuteTransitionContent, enabledTransitions).ConfigureAwait(false);

			await DoOperation(StateBagKey.EnterStates, EnterStates, enabledTransitions).ConfigureAwait(false);
		}

		private async ValueTask ExitStates(List<TransitionNode> enabledTransitions)
		{
			var statesToExit = ComputeExitSet(enabledTransitions);

			foreach (var state in statesToExit)
			{
				_context.StatesToInvoke.Delete(state);
			}

			var states = ToSortedList(statesToExit, StateEntityNode.ExitOrder);

			foreach (var state in states)
			{
				foreach (var history in state.HistoryStates)
				{
					static bool Deep(StateEntityNode node, StateEntityNode state)    => node.IsAtomicState && IsDescendant(node, state);
					static bool Shallow(StateEntityNode node, StateEntityNode state) => node.Parent == state;

					var list = history.Type == HistoryType.Deep
						? _context.Configuration.ToFilteredList(Deep, state)
						: _context.Configuration.ToFilteredList(Shallow, state);

					_context.HistoryValue.Set(history.Id, list);
				}
			}

			foreach (var state in states)
			{
				await TraceExitingState(state).ConfigureAwait(false);

				foreach (var onExit in state.OnExit)
				{
					await DoOperation(StateBagKey.OnExit, onExit, RunExecutableEntity, onExit.ActionEvaluators).ConfigureAwait(false);
				}

				foreach (var invoke in state.Invoke)
				{
					await CancelInvoke(invoke).ConfigureAwait(false);
				}

				_context.Configuration.Delete(state);

				await TraceExitedState(state).ConfigureAwait(false);
			}

			Complete(StateBagKey.OnExit);
		}

		private static void AddIfNotExists<T>(List<T> list, T item)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}

		private static List<T> ToSortedList<T>(List<T> list, IComparer<T> comparer)
		{
			var result = new List<T>(list);
			result.Sort(comparer);

			return result;
		}

		private static bool HasIntersection<T>(List<T> list1, List<T> list2)
		{
			foreach (var item in list1)
			{
				if (list2.Contains(item))
				{
					return true;
				}
			}

			return false;
		}

		private async ValueTask EnterStates(List<TransitionNode> enabledTransitions)
		{
			Infra.NotNull(_model);

			var statesToEnter = new List<StateEntityNode>();
			var statesForDefaultEntry = new List<CompoundNode>();
			var defaultHistoryContent = new DefaultHistoryContent();

			ComputeEntrySet(enabledTransitions, statesToEnter, statesForDefaultEntry, defaultHistoryContent);

			foreach (var state in ToSortedList(statesToEnter, StateEntityNode.EntryOrder))
			{
				await TraceEnteringState(state).ConfigureAwait(false);

				_context.Configuration.AddIfNotExists(state);
				_context.StatesToInvoke.AddIfNotExists(state);

				if (_model.Root.Binding == BindingType.Late && state.DataModel is { } dataModel)
				{
					await DoOperation(StateBagKey.InitializeDataModel, dataModel, InitializeDataModel, dataModel).ConfigureAwait(false);
				}

				foreach (var onEntry in state.OnEntry)
				{
					await DoOperation(StateBagKey.OnEntry, onEntry, RunExecutableEntity, onEntry.ActionEvaluators).ConfigureAwait(false);
				}

				if (state is CompoundNode compound && statesForDefaultEntry.Contains(compound))
				{
					await DoOperation(StateBagKey.DefaultEntry, state, RunExecutableEntity, compound.Initial.Transition.ActionEvaluators).ConfigureAwait(false);
				}

				if (defaultHistoryContent.TryGetValue(state.Id, out var action))
				{
					await DoOperation(StateBagKey.DefaultHistoryContent, state, RunExecutableEntity, action).ConfigureAwait(false);
				}

				if (state is FinalNode final)
				{
					if (final.Parent is StateMachineNode)
					{
						Running = false;
					}
					else
					{
						var parent = final.Parent;
						var grandparent = parent!.Parent;

						DataModelValue doneData = default;
						if (final.DoneData is not null)
						{
							doneData = await EvaluateDoneData(final.DoneData).ConfigureAwait(false);
						}

						_context.InternalQueue.Enqueue(new EventObject { Type = EventType.Internal, NameParts = EventName.GetDoneStateNameParts(parent.Id), Data = doneData });

						if (grandparent is ParallelNode)
						{
							if (grandparent.States.All(IsInFinalState))
							{
								_context.InternalQueue.Enqueue(new EventObject { Type = EventType.Internal, NameParts = EventName.GetDoneStateNameParts(grandparent.Id) });
							}
						}
					}
				}

				await TraceEnteredState(state).ConfigureAwait(false);
			}

			Complete(StateBagKey.OnEntry);
			Complete(StateBagKey.DefaultEntry);
			Complete(StateBagKey.DefaultHistoryContent);
		}

		private async ValueTask<DataModelValue> EvaluateDoneData(DoneDataNode doneData)
		{
			try
			{
				return await doneData.Evaluate(_context.ExecutionContext, _options.StopToken).ConfigureAwait(false);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(doneData, ex).ConfigureAwait(false);
			}

			return default;
		}

		private bool IsInFinalState(StateEntityNode state)
		{
			if (state is CompoundNode)
			{
				static bool Predicate(StateEntityNode s, OrderedSet<StateEntityNode> cfg) => s is FinalNode && cfg.IsMember(s);
				return state.States.Any(Predicate, _context.Configuration);
			}

			if (state is ParallelNode)
			{
				return state.States.All(IsInFinalState);
			}

			return false;
		}

		private void ComputeEntrySet(List<TransitionNode> transitions,
									 List<StateEntityNode> statesToEnter,
									 List<CompoundNode> statesForDefaultEntry,
									 DefaultHistoryContent defaultHistoryContent)
		{
			foreach (var transition in transitions)
			{
				foreach (var state in transition.TargetState)
				{
					AddDescendantStatesToEnter(state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
				}

				var ancestor = GetTransitionDomain(transition);

				foreach (var state in GetEffectiveTargetStates(transition))
				{
					AddAncestorStatesToEnter(state, ancestor, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
				}
			}
		}

		private List<StateEntityNode> ComputeExitSet(List<TransitionNode> transitions)
		{
			var statesToExit = new List<StateEntityNode>();
			foreach (var transition in transitions)
			{
				if (!transition.Target.IsDefaultOrEmpty)
				{
					var domain = GetTransitionDomain(transition);
					foreach (var state in _context.Configuration)
					{
						if (IsDescendant(state, domain))
						{
							AddIfNotExists(statesToExit, state);
						}
					}
				}
			}

			return statesToExit;
		}

		private void AddDescendantStatesToEnter(StateEntityNode state,
												List<StateEntityNode> statesToEnter,
												List<CompoundNode> statesForDefaultEntry,
												DefaultHistoryContent defaultHistoryContent)
		{
			if (state is HistoryNode history)
			{
				if (_context.HistoryValue.TryGetValue(history.Id, out var states))
				{
					foreach (var s in states)
					{
						AddDescendantStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
					}

					foreach (var s in states)
					{
						AddAncestorStatesToEnter(s, state.Parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
					}
				}
				else
				{
					defaultHistoryContent[state.Parent!.Id] = history.Transition.ActionEvaluators;

					foreach (var s in history.Transition.TargetState)
					{
						AddDescendantStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
					}

					foreach (var s in history.Transition.TargetState)
					{
						AddAncestorStatesToEnter(s, state.Parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
					}
				}
			}
			else
			{
				AddIfNotExists(statesToEnter, state);
				if (state is CompoundNode compound)
				{
					AddIfNotExists(statesForDefaultEntry, compound);

					foreach (var s in compound.Initial.Transition.TargetState)
					{
						AddDescendantStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
					}

					foreach (var s in compound.Initial.Transition.TargetState)
					{
						AddAncestorStatesToEnter(s, state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
					}
				}
				else
				{
					if (state is ParallelNode)
					{
						foreach (var child in state.States)
						{
							if (!statesToEnter.Exists(IsDescendant, child))
							{
								AddDescendantStatesToEnter(child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
							}
						}
					}
				}
			}
		}

		private void AddAncestorStatesToEnter(StateEntityNode state,
											  StateEntityNode? ancestor,
											  List<StateEntityNode> statesToEnter,
											  List<CompoundNode> statesForDefaultEntry,
											  DefaultHistoryContent defaultHistoryContent)
		{
			var ancestors = GetProperAncestors(state, ancestor);

			if (ancestors is null)
			{
				return;
			}

			foreach (var anc in ancestors)
			{
				AddIfNotExists(statesToEnter, anc);

				if (anc is ParallelNode)
				{
					foreach (var child in anc.States)
					{
						if (!statesToEnter.Exists(IsDescendant, child))
						{
							AddDescendantStatesToEnter(child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
						}
					}
				}
			}
		}

		private static bool IsDescendant(StateEntityNode state1, StateEntityNode? state2)
		{
			for (var s = state1.Parent; s is not null; s = s.Parent)
			{
				if (s == state2)
				{
					return true;
				}
			}

			return false;
		}

		private StateEntityNode? GetTransitionDomain(TransitionNode transition)
		{
			var tstates = GetEffectiveTargetStates(transition);

			if (tstates.Count == 0)
			{
				return null;
			}

			if (transition.Type == TransitionType.Internal && transition.Source is CompoundNode && tstates.TrueForAll(IsDescendant, transition.Source))
			{
				return transition.Source;
			}

			return FindLcca(transition.Source, tstates);
		}

		private static StateEntityNode? FindLcca(StateEntityNode headState, List<StateEntityNode> tailStates)
		{
			var ancestors = GetProperAncestors(headState, state2: null);

			if (ancestors is null)
			{
				return null;
			}

			foreach (var anc in ancestors)
			{
				if (tailStates.TrueForAll(IsDescendant, anc))
				{
					return anc;
				}
			}

			return null;
		}

		private static List<StateEntityNode>? GetProperAncestors(StateEntityNode state1, StateEntityNode? state2)
		{
			List<StateEntityNode>? states = default;

			for (var s = state1.Parent; s is not null; s = s.Parent)
			{
				if (s == state2)
				{
					return states;
				}

				(states ??= new List<StateEntityNode>()).Add(s);
			}

			return state2 is null ? states : null;
		}

		private List<StateEntityNode> GetEffectiveTargetStates(TransitionNode transition)
		{
			var targets = new List<StateEntityNode>();

			foreach (var state in transition.TargetState)
			{
				if (state is HistoryNode history)
				{
					if (!_context.HistoryValue.TryGetValue(history.Id, out var values))
					{
						values = GetEffectiveTargetStates(history.Transition);
					}

					foreach (var s in values)
					{
						AddIfNotExists(targets, s);
					}
				}
				else
				{
					AddIfNotExists(targets, state);
				}
			}

			return targets;
		}

		private async ValueTask ExecuteTransitionContent(List<TransitionNode> transitions)
		{
			foreach (var transition in transitions)
			{
				await DoOperation(StateBagKey.RunExecutableEntity, transition, ExecuteTransitionContent, transition).ConfigureAwait(false);
			}

			Complete(StateBagKey.RunExecutableEntity);
		}

		private async ValueTask ExecuteTransitionContent(TransitionNode transition)
		{
			await TracePerformingTransition(transition).ConfigureAwait(false);

			await RunExecutableEntity(transition.ActionEvaluators).ConfigureAwait(false);

			await TracePerformedTransition(transition).ConfigureAwait(false);
		}

		private async ValueTask RunExecutableEntity(ImmutableArray<IExecEvaluator> action)
		{
			if (!action.IsDefaultOrEmpty)
			{
				foreach (var executableEntity in action)
				{
					_options.StopToken.ThrowIfCancellationRequested();

					try
					{
						await executableEntity.Execute(_context.ExecutionContext, _options.StopToken).ConfigureAwait(false);
					}
					catch (Exception ex) when (IsError(ex))
					{
						await Error(executableEntity, ex).ConfigureAwait(false);

						break;
					}
				}
			}

			await CheckPoint(PersistenceLevel.ExecutableAction).ConfigureAwait(false);
		}

		private bool IsOperationCancelled(Exception exception)
		{
			return exception switch
				   {
					   OperationCanceledException ex => ex.CancellationToken == _options.StopToken,
					   _                             => false
				   };
		}

		private bool IsError(Exception ex) => !IsOperationCancelled(ex);

		private async ValueTask Error(object source, Exception exception, bool logLoggerErrors = true)
		{
			Infra.NotNull(_dataModelHandler);

			var sourceEntityId = (source as IEntity).Is(out IDebugEntityId? id) ? id.EntityId?.ToString(CultureInfo.InvariantCulture) : null;

			SendId? sendId = default;

			var errorType = IsPlatformError(exception)
				? ErrorType.Platform
				: IsCommunicationError(exception, out sendId)
					? ErrorType.Communication
					: ErrorType.Execution;

			var nameParts = errorType switch
							{
								ErrorType.Execution     => EventName.ErrorExecution,
								ErrorType.Communication => EventName.ErrorCommunication,
								ErrorType.Platform      => EventName.ErrorPlatform,
								_                       => throw Infra.Unexpected<Exception>(errorType)
							};

			var evt = new EventObject
					  {
						  Type = EventType.Platform,
						  NameParts = nameParts,
						  Data = DataConverter.FromException(exception, _dataModelHandler.CaseInsensitive),
						  SendId = sendId,
						  Ancestor = exception
					  };

			_context.InternalQueue.Enqueue(evt);

			try
			{
				await LogError(errorType, sourceEntityId, exception, _options.StopToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				if (logLoggerErrors)
				{
					try
					{
						await Error(source, ex, logLoggerErrors: false).ConfigureAwait(false);
					}
					catch
					{
						// ignored
					}
				}
			}
		}

		private async ValueTask ExecuteGlobalScript()
		{
			Infra.NotNull(_model);

			if (_model.Root.ScriptEvaluator is { } scriptEvaluator)
			{
				try
				{
					await scriptEvaluator.Execute(_context.ExecutionContext, _options.StopToken).ConfigureAwait(false);
				}
				catch (Exception ex) when (IsError(ex))
				{
					await Error(scriptEvaluator, ex).ConfigureAwait(false);
				}
			}
		}

		private async ValueTask EvaluateDoneData(FinalNode final)
		{
			if (final.DoneData is not null)
			{
				_doneData = await EvaluateDoneData(final.DoneData).ConfigureAwait(false);
			}
		}

		private async ValueTask ForwardEvent(InvokeNode invoke, IEvent evt)
		{
			try
			{
				Infra.NotNull(invoke.InvokeId);

				await ForwardEvent(evt, invoke.InvokeId, _options.StopToken).ConfigureAwait(false);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(invoke, ex).ConfigureAwait(false);
			}
		}

		private ValueTask ApplyFinalize(InvokeNode invoke) => invoke.Finalize is not null ? RunExecutableEntity(invoke.Finalize.ActionEvaluators) : default;

		private async ValueTask Invoke(InvokeNode invoke)
		{
			try
			{
				await invoke.Start(_context.ExecutionContext, _options.StopToken).ConfigureAwait(false);

				Infra.NotNull(invoke.InvokeId);

				_context.ActiveInvokes.Add(invoke.InvokeId);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(invoke, ex).ConfigureAwait(false);
			}
		}

		private async ValueTask CancelInvoke(InvokeNode invoke)
		{
			try
			{
				Infra.NotNull(invoke.InvokeId);

				_context.ActiveInvokes.Remove(invoke.InvokeId);

				await invoke.Cancel(_context.ExecutionContext, _options.StopToken).ConfigureAwait(false);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(invoke, ex).ConfigureAwait(false);
			}
		}

		private async ValueTask InitializeRootDataModel()
		{
			Infra.NotNull(_model);
			Infra.NotNull(_dataModelHandler);

			var rootDataModel = _model.Root.DataModel;

			if (rootDataModel is null)
			{
				return;
			}

			if (_options.Arguments.Type != DataModelValueType.List)
			{
				await InitializeDataModel(rootDataModel).ConfigureAwait(false);

				return;
			}

			var dictionary = _options.Arguments.AsList();
			var caseInsensitive = _dataModelHandler.CaseInsensitive;

			foreach (var node in rootDataModel.Data)
			{
				await InitializeData(node, dictionary[node.Id, caseInsensitive]).ConfigureAwait(false);
			}
		}

		private async ValueTask InitializeDataModel(DataModelNode dataModel)
		{
			foreach (var node in dataModel.Data)
			{
				await InitializeData(node).ConfigureAwait(false);
			}
		}

		private async ValueTask InitializeData(DataNode data, DataModelValue overrideValue = default)
		{
			Infra.NotNull(_dataModelHandler);

			try
			{
				_context.DataModel[data.Id, _dataModelHandler.CaseInsensitive] = await GetValue(data, overrideValue).ConfigureAwait(false);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(data, ex).ConfigureAwait(false);
			}
		}

		private async ValueTask<DataModelValue> GetValue(DataNode data, DataModelValue overrideValue)
		{
			if (!overrideValue.IsUndefined())
			{
				return overrideValue;
			}

			if (data.ResourceEvaluator is { } resourceEvaluator)
			{
				Infra.NotNull(data.Source);

				var resource = await LoadData(data.Source).ConfigureAwait(false);

				await using (resource.ConfigureAwait(false))
				{
					var obj = await resourceEvaluator.EvaluateObject(_context.ExecutionContext, resource, _options.StopToken).ConfigureAwait(false);

					return DataModelValue.FromObject(obj);
				}
			}

			if (data.ExpressionEvaluator is { } expressionEvaluator)
			{
				var obj = await expressionEvaluator.EvaluateObject(_context.ExecutionContext, _options.StopToken).ConfigureAwait(false);

				return DataModelValue.FromObject(obj);
			}

			if (data.InlineContentEvaluator is { } inlineContentEvaluator)
			{
				var obj = await inlineContentEvaluator.EvaluateObject(_context.ExecutionContext, _options.StopToken).ConfigureAwait(false);

				return DataModelValue.FromObject(obj);
			}

			return default;
		}

		private ValueTask<Resource> LoadData(IExternalDataExpression externalDataExpression)
		{
			var uri = _options.BaseUri.CombineWith(externalDataExpression.Uri!);
			var factoryContext = new FactoryContext(_options.ResourceLoaderFactories, _options.SecurityContext, _options.Logger, this);

			return factoryContext.GetResource(uri, _options.StopToken);
		}

		private StateMachineContext.Parameters CreateStateMachineContextParameters()
		{
			Infra.NotNull(_model);
			Infra.NotNull(_dataModelHandler);

			return new(_sessionId)
				   {
					   StateMachineName = _model.Root.Name,
					   ContextRuntimeItems = _options.ContextRuntimeItems,
					   Logger = _options.Logger,
					   LoggerContext = this,
					   ExternalCommunication = this,
					   SecurityContext = _options.SecurityContext,
					   DataModelCaseInsensitive = _dataModelHandler.CaseInsensitive,
					   DataModelInterpreter = new LazyValue(CreateInterpreterList),
					   DataModelHandlerData = new LazyValue(CreateDataModelHandlerList),
					   DataModelConfiguration = _options.Configuration,
					   DataModelHost = _options.Host,
					   DataModelArguments = _options.Arguments
				   };
		}

		private async ValueTask<IStateMachineContext> CreateContext()
		{
			Infra.NotNull(_model);
			Infra.NotNull(_dataModelHandler);

			IStateMachineContext context;
			var parameters = CreateStateMachineContextParameters();

			if (_isPersistingEnabled)
			{
				Infra.NotNull(_options.StorageProvider);

				var storage = await _options.StorageProvider.GetTransactionalStorage(partition: default, StateStorageKey, _options.StopToken).ConfigureAwait(false);
				context = new StateMachinePersistedContext(storage, _model.EntityMap, parameters);
			}
			else
			{
				context = new StateMachineContext(parameters);
			}

			_dataModelHandler.ExecutionContextCreated(context.ExecutionContext, out _dataModelVars);

			return context;
		}

		private DataModelValue CreateInterpreterList()
		{
			Infra.NotNull(_dataModelHandler);

			var typeInfo = TypeInfo<StateMachineInterpreter>.Instance;

			var interpreterList = new DataModelList(_dataModelHandler.CaseInsensitive)
								  {
									  { @"name", typeInfo.FullTypeName },
									  { @"version", typeInfo.AssemblyVersion }
								  };

			interpreterList.MakeDeepConstant();

			return new DataModelValue(interpreterList);
		}

		private DataModelValue CreateDataModelHandlerList()
		{
			Infra.NotNull(_dataModelHandler);

			var typeInfo = _dataModelHandler.TypeInfo;

			var dataModelHandlerList = new DataModelList(_dataModelHandler.CaseInsensitive)
									   {
										   { @"name", typeInfo.FullTypeName },
										   { @"assembly", typeInfo.AssemblyName },
										   { @"version", typeInfo.AssemblyVersion },
										   { @"vars", DataModelValue.FromObject(_dataModelVars) }
									   };

			dataModelHandlerList.MakeDeepConstant();

			return new DataModelValue(dataModelHandlerList);
		}

		private enum StateBagKey
		{
			Stop,
			Running,
			Running2,
			EventlessTransitions,
			InternalQueueNonEmpty,
			SelectInternalEventTransitions,
			InternalQueueEmpty,
			InitializeRootDataModel,
			EarlyInitializeDataModel,
			ExecuteGlobalScript,
			ExternalEventTransitions,
			InitialEnterStates,
			ExitInterpreter,
			InternalQueueProcessing,
			MainEventLoop,
			ExitStates,
			ExecuteTransitionContent,
			EnterStates,
			RunExecutableEntity,
			InitializeDataModel,
			OnExit,
			OnEntry,
			DefaultEntry,
			DefaultHistoryContent,
			Invoke,
			ReturnDoneEvent,
			NotifyAccepted,
			NotifyStarted,
			NotifyExited
		}

		private struct LiveLockDetector : IDisposable
		{
			private const int IterationCount = 36;

			private int[]? _data;
			private int    _index;
			private int    _internalQueueLength;
			private int    _sum;

		#region Interface IDisposable

			public void Dispose()
			{
				if (_data is { } data)
				{
					ArrayPool<int>.Shared.Return(data);

					_data = default;
				}
			}

		#endregion

			public static LiveLockDetector Create() => new() { _index = -1 };

			public void Iteration(int internalQueueCount)
			{
				if (_index == -1)
				{
					_internalQueueLength = internalQueueCount;
					_index = _sum = 0;

					return;
				}

				_data ??= ArrayPool<int>.Shared.Rent(IterationCount);

				if (_index >= IterationCount)
				{
					if (_sum >= 0)
					{
						throw new StateMachineLiveLockException();
					}

					_sum -= _data[_index % IterationCount];
				}

				var delta = internalQueueCount - _internalQueueLength;
				_internalQueueLength = internalQueueCount;
				_sum += delta;
				_data[_index ++ % IterationCount] = delta;
			}
		}
	}
}