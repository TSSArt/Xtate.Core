<<<<<<< Updated upstream
﻿
#region Copyright © 2019-2021 Sergii Artemenko
=======
﻿#region Copyright © 2019-2023 Sergii Artemenko
>>>>>>> Stashed changes

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

using System.Buffers;
<<<<<<< Updated upstream
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
=======
>>>>>>> Stashed changes
using Xtate.DataModel;

namespace Xtate.Core;

using DefaultHistoryContent = Dictionary<IIdentifier, ImmutableArray<IExecEvaluator>>;

public partial class StateMachineInterpreter : IStateMachineInterpreter
{
	private readonly object                          _stateMachineToken = new();
	private          IStateMachineContext            _context = default!;
	private          bool                            _running = true;
	private          StateMachineDestroyedException? _stateMachineDestroyedException;

<<<<<<< Updated upstream
	public partial class StateMachineInterpreter : IStateMachineInterpreter
	{
		//TODO:
		//private static readonly ImmutableArray<IDataModelHandlerFactory> PredefinedDataModelHandlerFactories =
		//ImmutableArray.Create(NullDataModelHandler.Factory, RuntimeDataModelHandler.Factory);

		private readonly object _stateMachineToken = new();
		//private readonly ChannelReader<IEvent>               _eventChannel;
		//private readonly bool                                _isPersistingEnabled;
	//	private readonly InterpreterOptions                  _options;
		//private readonly SessionId                           _sessionId;
		[Obsolete]
		private readonly IStateMachineValidator              _stateMachineValidator;
		//private readonly IPersistenceContext?                _persistenceContext;
		private                   IStateMachineContext      _context;
		public required Func<ValueTask<IStateMachineContext     >> ContextFactory;
		public           required INotifyStateChanged?      _notifyStateChanged;
		public           required IUnhandledErrorBehaviour? _unhandledErrorBehaviour;
		public           required IResourceLoader?          _resourceLoader;
		public           required IStateMachineLocation?    _stateMachineLocation;
		public           required IExternalCommunication?   _externalCommunication;
		//public           required IStateMachineInterpreterLogger        _logger;
		public           required ILogger<IStateMachineInterpreter> _logger;
		//public           required IStateMachineInterpreterTracer        _tracer;
		public           required DataConverter                         _dataConverter;

		public required IDataModelHandler _dataModelHandler;
		//
		private         ImmutableDictionary<string, string> _dataModelVars = ImmutableDictionary<string, string>.Empty;
		private         DataModelValue                      _doneData; //TODO: move to context
		
		public required IInterpreterModel                   _model;

		public required IEventQueueReader _eventQueueReader;

		//private          IStateMachine?                      _stateMachine;
		
		//[Obsolete] private IExecutionContext _executionContext;

		private readonly IStateMachineArguments? _stateMachineArguments;

		private          StateMachineDestroyedException? _stateMachineDestroyedException;


		public StateMachineInterpreter() { }

		[Obsolete]
		[SetsRequiredMembers]
		public StateMachineInterpreter(IInterpreterModel interpreterModel,
									   IEventQueueReader eventQueueReader,
									   IDataModelHandler dataModelHandler,
									   IStateMachineContext stateMachineContext,
									   INotifyStateChanged? notifyStateChanged,
									   IUnhandledErrorBehaviour? unhandledErrorBehaviour,
									   IResourceLoader? resourceLoader,
									   IStateMachineLocation? stateMachineLocation,
									   IExternalCommunication? externalCommunication,
									   ILogger<IStateMachineInterpreter> logger)
		{
			_model = interpreterModel;
			_eventQueueReader = eventQueueReader;


			//	_sessionId = options.SessionId;
			//	_stateMachine = null;
			//_eventChannel = options.eventChannel;

			//	_options = options.options;
			_dataModelHandler = dataModelHandler;

			//	_isPersistingEnabled = _options.PersistenceLevel != PersistenceLevel.None && _options.StorageProvider is not null;
			//_persistenceContext = persistenceContext;

			_context = stateMachineContext;
			_notifyStateChanged = notifyStateChanged;
			_unhandledErrorBehaviour = unhandledErrorBehaviour;
			_resourceLoader = resourceLoader;
			_stateMachineLocation = stateMachineLocation;
			_externalCommunication = externalCommunication;
			_logger = logger;

			//_executionContext = executionContext;
		}

		private StateMachineInterpreter(IStateMachineInterpreterOptions options, 
										IStateMachineContext context,
										IExecutionContext executionContext,
										IDataModelHandler dataModelHandler,
										IStateMachineValidator stateMachineValidator,
										IPersistenceContext? persistenceContext,
										IStateMachineArguments? stateMachineArguments)
		{
			if (options is null) throw new ArgumentNullException(nameof(options));

		/*	_sessionId = options.SessionId;
			_stateMachine = null;
			_eventChannel = options.eventChannel;
			_options = options.options;
			_dataModelHandler = dataModelHandler;
			_isPersistingEnabled = _options.PersistenceLevel != PersistenceLevel.None && _options.StorageProvider is not null;
			_stateMachineValidator = stateMachineValidator;*/
			//_persistenceContext = persistenceContext;

		//	_model = options.model;
			_context = context;
			//_executionContext = executionContext;
			_stateMachineArguments = stateMachineArguments;
		}
=======
	public required IStateMachineArguments?               StateMachineArguments   { private get; [UsedImplicitly] init; }
	public required DataConverter                         DataConverter           { private get; [UsedImplicitly] init; }
	public required IDataModelHandler                     DataModelHandler        { private get; [UsedImplicitly] init; }
	public required IEventQueueReader                     EventQueueReader        { private get; [UsedImplicitly] init; }
	public required IExternalCommunication?               ExternalCommunication   { private get; [UsedImplicitly] init; }
	public required ILogger<IStateMachineInterpreter>     Logger                  { private get; [UsedImplicitly] init; }
	public required IInterpreterModel                     Model                   { private get; [UsedImplicitly] init; }
	public required INotifyStateChanged?                  NotifyStateChanged      { private get; [UsedImplicitly] init; }
	public required IUnhandledErrorBehaviour?             UnhandledErrorBehaviour { private get; [UsedImplicitly] init; }
	public required Func<ValueTask<IStateMachineContext>> ContextFactory          { private get; [UsedImplicitly] init; }

#region Interface IStateMachineInterpreter

	public virtual async ValueTask<DataModelValue> RunAsync()
	{
		_context = await ContextFactory().ConfigureAwait(false);

		await Interpret().ConfigureAwait(false);

		return _context.DoneData;
	}
>>>>>>> Stashed changes

#endregion

	protected virtual ValueTask NotifyAccepted() => NotifyInterpreterState(StateMachineInterpreterState.Accepted);
	protected virtual ValueTask NotifyStarted()  => NotifyInterpreterState(StateMachineInterpreterState.Started);
	protected virtual ValueTask NotifyExited()   => NotifyInterpreterState(StateMachineInterpreterState.Exited);
	protected virtual ValueTask NotifyWaiting()  => NotifyInterpreterState(StateMachineInterpreterState.Waiting);

	protected ValueTask TraceInterpreterState(StateMachineInterpreterState state) => Logger.Write(Level.Trace, $@"Interpreter state has changed to '{state}'");

	private async ValueTask NotifyInterpreterState(StateMachineInterpreterState state)
	{
		await TraceInterpreterState(state).ConfigureAwait(false);

		if (NotifyStateChanged is not null)
		{
<<<<<<< Updated upstream
			//TODO: to delete
			/*_sessionId = sessionId;
			_stateMachine = stateMachine;
			_eventChannel = eventChannel;
			_options = options;
			_isPersistingEnabled = _options.PersistenceLevel != PersistenceLevel.None && _options.StorageProvider is not null;
			_stateMachineValidator = _options.ServiceLocator.GetService<IStateMachineValidator>();*/
=======
			await NotifyStateChanged.OnChanged(state).ConfigureAwait(false);
>>>>>>> Stashed changes
		}
	}

<<<<<<< Updated upstream
		private bool _running = true;

		
		//TODO: to delete
		public static ValueTask<DataModelValue> RunAsync(IStateMachine? stateMachine, ChannelReader<IEvent>  eventChannel, InterpreterOptions? options = default)
		{
			if (eventChannel is null) throw new ArgumentNullException(nameof(eventChannel));

			return RunAsync(SessionId.New(), stateMachine, eventChannel, options);
		}

		//TODO: to delete
		public static ValueTask<DataModelValue> RunAsync(SessionId sessionId,
																  IStateMachine? stateMachine,
																  ChannelReader<IEvent> eventChannel,
																  InterpreterOptions? options = default)
		{
			if (sessionId is null) throw new ArgumentNullException(nameof(sessionId));
			if (eventChannel is null) throw new ArgumentNullException(nameof(eventChannel));

			Infra.NotNull(options);

			//var sp = options.ServiceLocator;

			
			//return new StateMachineInterpreter(sessionId, stateMachine, eventChannel, options ?? InterpreterOptions.Default).Run();
			return default;//sp.GetService<IStateMachineInterpreter>().RunAsync();
		}
		/*
		private InterpreterModelBuilder.Parameters CreateInterpreterModelBuilderParameters()
		{
			Infra.NotNull(_stateMachine);
			Infra.NotNull(_dataModelHandler);

			return new InterpreterModelBuilder.Parameters(_options.ServiceLocator, _stateMachine, _dataModelHandler)
				   {
					   BaseUri = _options.BaseUri,
					   CustomActionProviders = _options.CustomActionProviders,
					   ErrorProcessor = _options.ErrorProcessor,
					   ResourceLoaderFactories = _options.ResourceLoaderFactories,
					   SecurityContext = _options.SecurityContext,
					   Logger = _options.Logger,
					   LoggerContext = this
				   };
		}*/
		/*
		private async ValueTask<InterpreterModel> BuildInterpreterModel()
		{
			var interpreterModel = _isPersistingEnabled ? await TryRestoreInterpreterModel().ConfigureAwait(false) : null;

			if (interpreterModel is not null)
			{
				return interpreterModel;
			}

			Infra.NotNull(_stateMachine);

			_dataModelHandler = await CreateDataModelHandler(_stateMachine.DataModelType).ConfigureAwait(false);

			_stateMachineValidator.Validate(_stateMachine);

			var parameters = CreateInterpreterModelBuilderParameters();
			var interpreterModelBuilder = new InterpreterModelBuilder(parameters);

			try
			{
				interpreterModel = await interpreterModelBuilder.Build(CancellationToken.None).ConfigureAwait(false);
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

			var storage = await _options.StorageProvider.GetTransactionalStorage(partition: default, StateMachineDefinitionStorageKey, CancellationToken.None).ConfigureAwait(false);
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
					var model = await temporaryModelBuilder.Build(CancellationToken.None).ConfigureAwait(false);
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

					return await interpreterModelBuilder.Build(CancellationToken.None).ConfigureAwait(false);
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

			var storage = await _options.StorageProvider.GetTransactionalStorage(partition: default, StateMachineDefinitionStorageKey, CancellationToken.None).ConfigureAwait(false);
			await using (storage.ConfigureAwait(false))
			{
				SaveToStorage(interpreterModel.Root.As<IStoreSupport>(), new Bucket(storage));

				await storage.CheckPoint(level: 0, CancellationToken.None).ConfigureAwait(false);
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
		}*/
		/*
		private ValueTask<IDataModelHandler> CreateDataModelHandler(string? dataModelType)
		{
			var dataModelHandlerService = _options.ServiceLocator.GetService<IDataModelHandlerService>();

			return dataModelHandlerService.GetDataModelHandler(dataModelType, CancellationToken.None);
			
			dataModelType ??= NullDataModelHandler.DataModelType;
			var factoryContext = new FactoryContext(_options.ServiceLocator, _options.ResourceLoaderFactories, _options.SecurityContext, _options.Logger, this);
			var activator = await FindDataModelHandlerFactoryActivator(dataModelType, _options.ServiceLocator).ConfigureAwait(false);

			if (activator is not null)
			{
				return await activator.CreateHandler(_options.ServiceLocator, dataModelType, _options.ErrorProcessor, _stopCts.Token).ConfigureAwait(false);
			}

			_options.ErrorProcessor.AddError<StateMachineInterpreter>(entity: null, Res.Format(Resources.Exception_CantFindDataModelHandlerFactoryForDataModelType, dataModelType));

			return new NullDataModelHandler(_options.ServiceLocator);
			
		}*/
		/*
		private async ValueTask<IDataModelHandlerFactoryActivator?> FindDataModelHandlerFactoryActivator(string dataModelType, ServiceLocator serviceLocator)
		{
			if (!_options.DataModelHandlerFactories.IsDefaultOrEmpty)
			{
				foreach (var factory in _options.DataModelHandlerFactories)
				{
					var activator = await factory.TryGetActivator(serviceLocator, dataModelType, _stopCts.Token).ConfigureAwait(false);

					if (activator is not null)
					{
						return activator;
					}
				}
			}

			foreach (var factory in PredefinedDataModelHandlerFactories)
			{
				var activator = await factory.TryGetActivator(serviceLocator, dataModelType ?? NullDataModelHandler.DataModelType, _stopCts.Token).ConfigureAwait(false);

				if (activator is not null)
				{
					return activator;
				}
			}

			return null;
		}*/

		/*protected virtual ValueTask DoOperation<TArg>(StateBagKey key,
													  IEntity entity,
													  Func<TArg, ValueTask> func,
													  TArg arg) =>
			func(arg);

		protected virtual void Complete(StateBagKey key)            { }*/
		
		protected virtual ValueTask NotifyAccepted() => NotifyInterpreterState(StateMachineInterpreterState.Accepted);
		protected virtual ValueTask NotifyStarted()  => NotifyInterpreterState(StateMachineInterpreterState.Started);
		protected virtual ValueTask NotifyExited()   => NotifyInterpreterState(StateMachineInterpreterState.Exited);
		protected virtual ValueTask NotifyWaiting()  => NotifyInterpreterState(StateMachineInterpreterState.Waiting);

		protected ValueTask TraceInterpreterState(StateMachineInterpreterState state) => _logger.Write(Level.Trace, $@"Interpreter state has changed to '{state}'");

		private async ValueTask NotifyInterpreterState(StateMachineInterpreterState state)
		{
			await TraceInterpreterState(state).ConfigureAwait(false);

			if (_notifyStateChanged is { } notifyStateChanged)
			{
				await notifyStateChanged.OnChanged(state).ConfigureAwait(false);
			}
		}

		public virtual async ValueTask<DataModelValue> RunAsync()
		{
			_context = await ContextFactory().ConfigureAwait(false);

			if (false /*_stateMachine is null*/) //TODO: use correct condition
			{
				//await TraceInterpreterState(StateMachineInterpreterState.Resumed).ConfigureAwait(false);
			}

			//try
			//{
				await Interpret().ConfigureAwait(false);
			//}
			/*catch (StateMachineQueueClosedException)
			{
				await TraceInterpreterState(StateMachineInterpreterState.QueueClosed).ConfigureAwait(false);

				throw;
			}*/
			/*catch (StateMachineDestroyedException)
			{
				throw;
			}
			catch
			{
				await TraceInterpreterState(StateMachineInterpreterState.Halted).ConfigureAwait(false);

				throw;
			}*/ /*
			catch (StateMachineSuspendedException)
			{
				await TraceInterpreterState(StateMachineInterpreterState.Suspended).ConfigureAwait(false);

				throw;
			}*/
			/*catch (StateMachineInterpreterDisposedException)
			{
				await TraceInterpreterState(StateMachineInterpreterState.Halted).ConfigureAwait(false);

				throw;
			}*/

			return _doneData;
		}

		protected virtual async ValueTask Interpret()
		{
			try
			{
				await EnterSteps().ConfigureAwait(false);
				await MainEventLoop().ConfigureAwait(false);
			}
			catch (StateMachineDestroyedException)
			{
				await TraceInterpreterState(StateMachineInterpreterState.Destroying).ConfigureAwait(false);
				await ExitSteps().ConfigureAwait(false);

				throw;
			}
			catch
			{
				await TraceInterpreterState(StateMachineInterpreterState.Halted).ConfigureAwait(false);

				throw;
			}

			await ExitSteps().ConfigureAwait(false);
		}

		protected virtual async ValueTask EnterSteps()
		{
			await NotifyAccepted().ConfigureAwait(false);
			await InitializeDataModels().ConfigureAwait(false);
			await ExecuteGlobalScript().ConfigureAwait(false);
			await NotifyStarted().ConfigureAwait(false);
			await InitialEnterStates().ConfigureAwait(false);
		}

		protected virtual async ValueTask ExitSteps()
		{
			await ExitInterpreter().ConfigureAwait(false);
			await NotifyExited().ConfigureAwait(false);
		}
		/*
		private void ThrowIfTerminationRequested(bool throwOnDestroy)
		{
			var state = (State)_state;

			switch (state)
			{
				case State.Operate:                      return;
				case State.Destroy when !throwOnDestroy: return;
				case State.Destroy when throwOnDestroy:  throw new StateMachineDestroyedException(Resources.Exception_StateMachineHasBeenDestroyed);
				//case State.Suspend:                      throw new StateMachineSuspendedException(Resources.Exception_StateMachineHasBeenSuspended);
				case State.Disposed:                     throw new StateMachineInterpreterDisposedException(Resources.Exception_StateMachineInterpreterHasBeenDisposed);
				default:                                 throw Infra.Unexpected<Exception>(state);
=======
	protected virtual async ValueTask Interpret()
	{
		try
		{
			await EnterSteps().ConfigureAwait(false);
			await MainEventLoop().ConfigureAwait(false);
		}
		catch (StateMachineDestroyedException)
		{
			await TraceInterpreterState(StateMachineInterpreterState.Destroying).ConfigureAwait(false);
			await ExitSteps().ConfigureAwait(false);

			throw;
		}
		catch
		{
			await TraceInterpreterState(StateMachineInterpreterState.Halted).ConfigureAwait(false);

			throw;
		}

		await ExitSteps().ConfigureAwait(false);
	}

	protected virtual async ValueTask EnterSteps()
	{
		await NotifyAccepted().ConfigureAwait(false);
		await InitializeDataModels().ConfigureAwait(false);
		await ExecuteGlobalScript().ConfigureAwait(false);
		await NotifyStarted().ConfigureAwait(false);
		await InitialEnterStates().ConfigureAwait(false);
	}

	protected virtual async ValueTask ExitSteps()
	{
		await ExitInterpreter().ConfigureAwait(false);
		await NotifyExited().ConfigureAwait(false);
	}

	public virtual void TriggerDestroySignal(Exception? innerException = default)
	{
		_stateMachineDestroyedException = new StateMachineDestroyedException(Resources.Exception_StateMachineHasBeenDestroyed, innerException);

		StopWaitingExternalEvents();
	}

	protected void StopWaitingExternalEvents() => EventQueueReader.Complete();

	protected virtual async ValueTask InitializeDataModels()
	{
		if (Model.Root.DataModel is { } dataModel)
		{
			await InitializeDataModel(dataModel, StateMachineArguments?.Arguments.AsListOrDefault()).ConfigureAwait(false);
		}

		if (Model.Root is { Binding: BindingType.Early } stateMachineNode)
		{
			foreach (var stateNode in stateMachineNode.States)
			{
				await InitializeDataModelRecursive(stateNode).ConfigureAwait(false);
			}
		}
	}

	private async ValueTask InitializeDataModelRecursive(StateEntityNode stateEntityNode)
	{
		if (stateEntityNode is ParallelNode or StateNode)
		{
			if (stateEntityNode.DataModel is { } dataModelNode)
			{
				await InitializeDataModel(dataModelNode).ConfigureAwait(false);
			}

			if (stateEntityNode.States is { IsDefaultOrEmpty: false } states)
			{
				foreach (var stateNode in states)
				{
					await InitializeDataModelRecursive(stateNode).ConfigureAwait(false);
				}
			}
		}
	}

	protected virtual ValueTask InitialEnterStates() => EnterStates([Model.Root.Initial.Transition]);

	protected virtual async ValueTask MainEventLoop()
	{
		while (await MainEventLoopIteration().ConfigureAwait(false)) { }
	}

	protected virtual async ValueTask<bool> MainEventLoopIteration()
	{
		if (!await Macrostep().ConfigureAwait(false))
		{
			return false;
		}

		if (await StartInvokeLoop().ConfigureAwait(false))
		{
			return true;
		}

		return await ExternalQueueProcess().ConfigureAwait(false);
	}

	protected virtual async ValueTask<bool> StartInvokeLoop()
	{
		foreach (var state in _context.StatesToInvoke.ToSortedList(StateEntityNode.EntryOrder))
		{
			foreach (var invoke in state.Invoke)
			{
				await Invoke(invoke).ConfigureAwait(false);
>>>>>>> Stashed changes
			}
		}*/

		public virtual void TriggerDestroySignal(Exception? innerException = default)
		{
			_stateMachineDestroyedException = new StateMachineDestroyedException(Resources.Exception_StateMachineHasBeenDestroyed, innerException);

			StopWaitingExternalEvents();
		}

<<<<<<< Updated upstream
		protected void StopWaitingExternalEvents() => _eventQueueReader.Complete();

		protected virtual async ValueTask InitializeDataModels()
		{
			if (_model.Root.DataModel is { } dataModel)
			{
				await InitializeDataModel(dataModel, _stateMachineArguments?.Arguments.AsListOrDefault()).ConfigureAwait(false);
=======
		_context.StatesToInvoke.Clear();

		return !await IsInternalQueueEmpty().ConfigureAwait(false);
	}

	protected virtual async ValueTask<bool> ExternalQueueProcess()
	{
		if (await ExternalEventTransitions().ConfigureAwait(false) is { Count: > 0 } transitions)
		{
			return await Microstep(transitions).ConfigureAwait(false);
		}

		return _running;
	}

	protected virtual async ValueTask<bool> Macrostep()
	{
		using var liveLockDetector = LiveLockDetector.Create();

		while (await MacrostepIteration().ConfigureAwait(false))
		{
			if (liveLockDetector.IsLiveLockDetected(_context.InternalQueue.Count))
			{
				throw new StateMachineDestroyedException(Resources.Exception_LivelockDetected);
>>>>>>> Stashed changes
			}

<<<<<<< Updated upstream
			if (_model.Root is { Binding: BindingType.Early } stateMachineNode)
			{
				foreach (var stateNode in stateMachineNode.States)
				{
					await InitializeDataModelRecursive(stateNode).ConfigureAwait(false);
=======
		return _running;
	}

	protected virtual async ValueTask<bool> MacrostepIteration()
	{
		if (await SelectTransitions(evt: default).ConfigureAwait(false) is { Count: > 0 } transitions)
		{
			return await Microstep(transitions).ConfigureAwait(false);
		}

		return await InternalQueueProcess().ConfigureAwait(false);
	}

	protected virtual ValueTask<bool> IsInternalQueueEmpty() => new(_context.InternalQueue.Count == 0);

	protected virtual async ValueTask<bool> InternalQueueProcess()
	{
		if (await IsInternalQueueEmpty().ConfigureAwait(false))
		{
			return false;
		}

		if (await SelectInternalEventTransitions().ConfigureAwait(false) is { Count: > 0 } transitions)
		{
			return await Microstep(transitions).ConfigureAwait(false);
		}

		return _running;
	}

	private ValueTask TraceProcessingEvent(IEvent evt) => Logger.Write(Level.Trace, $@"Processing {evt.Type} event '{EventName.ToName(evt.NameParts)}'", evt);

	protected virtual async ValueTask<List<TransitionNode>> SelectInternalEventTransitions()
	{
		var internalEvent = _context.InternalQueue.Dequeue();

		var eventModel = DataConverter.FromEvent(internalEvent);
		_context.DataModel.SetInternal(key: @"_event", DataModelHandler.CaseInsensitive, eventModel, DataModelAccess.ReadOnly);

		await TraceProcessingEvent(internalEvent).ConfigureAwait(false);

		var transitions = await SelectTransitions(internalEvent).ConfigureAwait(false);

		if (transitions.Count == 0 && EventName.IsError(internalEvent.NameParts))
		{
			ProcessUnhandledError(internalEvent);
		}

		return transitions;
	}

	private void ProcessUnhandledError(IEvent evt)
	{
		var behaviour = UnhandledErrorBehaviour?.Behaviour ?? Xtate.UnhandledErrorBehaviour.DestroyStateMachine;

		switch (behaviour)
		{
			case Xtate.UnhandledErrorBehaviour.IgnoreError:
				break;

			case Xtate.UnhandledErrorBehaviour.DestroyStateMachine:
				TriggerDestroySignal(GetUnhandledErrorException());
				break;

			case Xtate.UnhandledErrorBehaviour.HaltStateMachine:
				throw GetUnhandledErrorException();

			default:
				throw Infra.Unexpected<Exception>(behaviour);
		}

		StateMachineUnhandledErrorException GetUnhandledErrorException()
		{
			evt.Is<Exception>(out var exception);

			return new StateMachineUnhandledErrorException(Resources.Exception_UnhandledException, exception);
		}
	}

	protected virtual async ValueTask<List<TransitionNode>> SelectTransitions(IEvent? evt)
	{
		var transitions = new List<TransitionNode>();

		foreach (var state in _context.Configuration.ToFilteredSortedList(s => s.IsAtomicState, StateEntityNode.EntryOrder))
		{
			await FindTransitionForState(transitions, state, evt).ConfigureAwait(false);
		}

		return RemoveConflictingTransitions(transitions);
	}

	protected virtual async ValueTask<List<TransitionNode>> ExternalEventTransitions()
	{
		var externalEvent = await ReadExternalEventFiltered().ConfigureAwait(false);

		var eventModel = DataConverter.FromEvent(externalEvent);
		_context.DataModel.SetInternal(key: @"_event", DataModelHandler.CaseInsensitive, eventModel, DataModelAccess.ReadOnly);

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
>>>>>>> Stashed changes
				}
			}
		}

<<<<<<< Updated upstream
		private async ValueTask InitializeDataModelRecursive(StateEntityNode stateEntityNode)
		{
			if (stateEntityNode is ParallelNode or StateNode)
			{
				if (stateEntityNode.DataModel is { } dataModelNode)
				{
					await InitializeDataModel(dataModelNode).ConfigureAwait(false);
				}

				if (stateEntityNode.States is { IsDefaultOrEmpty: false } states)
				{
					foreach (var stateNode in states)
					{
						await InitializeDataModelRecursive(stateNode).ConfigureAwait(false);
					}
				}
			}
		}

		protected virtual ValueTask InitialEnterStates()
		{
			return EnterStates(new List<TransitionNode>(1) { _model.Root.Initial.Transition });
		}

		protected virtual async ValueTask MainEventLoop()
		{
			while (await MainEventLoopIteration().ConfigureAwait(false)) { }
		}

		protected virtual async ValueTask<bool> MainEventLoopIteration()
		{
			if (!await Macrostep().ConfigureAwait(false))
			{
				return false;
			}

			if (await StartInvokeLoop().ConfigureAwait(false))
			{
				return true;
			}

			return await ExternalQueueProcess().ConfigureAwait(false);
		}

		protected virtual async ValueTask<bool> StartInvokeLoop()
		{
			foreach (var state in _context.StatesToInvoke.ToSortedList(StateEntityNode.EntryOrder))
			{
				foreach (var invoke in state.Invoke)
				{
					await Invoke(invoke).ConfigureAwait(false);
				}
			}

			_context.StatesToInvoke.Clear();

			return !await IsInternalQueueEmpty().ConfigureAwait(false);
		}

		protected virtual async ValueTask<bool> ExternalQueueProcess()
		{
			if (await ExternalEventTransitions().ConfigureAwait(false) is { Count: > 0 } transitions)
			{
				return await Microstep(transitions).ConfigureAwait(false);
			}

			return _running;
		}
		
		protected virtual async ValueTask<bool> Macrostep()
		{
			using var liveLockDetector = LiveLockDetector.Create();

			while (await MacrostepIteration().ConfigureAwait(false))
			{
				if (liveLockDetector.IsLiveLockDetected(_context.InternalQueue.Count))
				{
					throw new StateMachineDestroyedException(Resources.Exception_LivelockDetected);
				}
			}

			return _running;
		}

		protected virtual async ValueTask<bool> MacrostepIteration()
		{
			if (await SelectTransitions(evt: default).ConfigureAwait(false) is { Count: > 0 } transitions)
			{
				return await Microstep(transitions).ConfigureAwait(false);
			}

			return await InternalQueueProcess().ConfigureAwait(false);
		}

		protected virtual ValueTask<bool> IsInternalQueueEmpty() => new(_context.InternalQueue.Count == 0);

		protected virtual async ValueTask<bool> InternalQueueProcess()
		{
			if (await IsInternalQueueEmpty().ConfigureAwait(false))
			{
				return false;
			}

			if (await SelectInternalEventTransitions().ConfigureAwait(false) is { Count: > 0 } transitions)
			{
				return await Microstep(transitions).ConfigureAwait(false);
			}

			return _running;
		}

		private ValueTask TraceProcessingEvent(IEvent evt) => _logger.Write(Level.Trace, $@"Processing {evt.Type} event '{EventName.ToName(evt.NameParts)}'", evt);

		protected virtual async ValueTask<List<TransitionNode>> SelectInternalEventTransitions()
		{
			var internalEvent = _context.InternalQueue.Dequeue();

			var eventModel = _dataConverter.FromEvent(internalEvent);
			_context.DataModel.SetInternal(key: @"_event", _dataModelHandler.CaseInsensitive, eventModel, DataModelAccess.ReadOnly);

			await TraceProcessingEvent(internalEvent).ConfigureAwait(false);

			var transitions = await SelectTransitions(internalEvent).ConfigureAwait(false);

			if (transitions.Count == 0 && EventName.IsError(internalEvent.NameParts))
			{
				ProcessUnhandledError(internalEvent);
			}

			return transitions;
		}

		private void ProcessUnhandledError(IEvent evt)
		{
			var behaviour = _unhandledErrorBehaviour?.Behaviour ?? UnhandledErrorBehaviour.DestroyStateMachine;
			
			switch (behaviour)
			{
				case UnhandledErrorBehaviour.IgnoreError:
					break;

				case UnhandledErrorBehaviour.DestroyStateMachine:
					TriggerDestroySignal(GetUnhandledErrorException());
					break;

				case UnhandledErrorBehaviour.HaltStateMachine:
					throw GetUnhandledErrorException();

				default:
					throw Infra.Unexpected<Exception>(behaviour);
			}

			StateMachineUnhandledErrorException GetUnhandledErrorException()
			{
				evt.Is<Exception>(out var exception);

				return new StateMachineUnhandledErrorException(Resources.Exception_UnhandledException, exception);
			}
		}

		protected virtual async ValueTask<List<TransitionNode>> SelectTransitions(IEvent? evt)
		{
			var transitions = new List<TransitionNode>();

			foreach (var state in _context.Configuration.ToFilteredSortedList(s => s.IsAtomicState, StateEntityNode.EntryOrder))
			{
				await FindTransitionForState(transitions, state, evt).ConfigureAwait(false);
			}

			return RemoveConflictingTransitions(transitions);
		}

		protected virtual async ValueTask<List<TransitionNode>> ExternalEventTransitions()
		{
			var externalEvent = await ReadExternalEventFiltered().ConfigureAwait(false);

			var eventModel = _dataConverter.FromEvent(externalEvent);
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

		//protected virtual ValueTask CheckPoint(PersistenceLevel level) => default;

		private async ValueTask<IEvent> ReadExternalEventFiltered()
		{
			while (true)
			{
				var evt = await ReadExternalEvent().ConfigureAwait(false);

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

		protected virtual async ValueTask<IEvent> ReadExternalEvent()
		{
			ThrowIfDestroying();

			if (_eventQueueReader.TryReadEvent(out var evt))
=======
		return await SelectTransitions(externalEvent).ConfigureAwait(false);
	}

	//protected virtual ValueTask CheckPoint(PersistenceLevel level) => default;

	private async ValueTask<IEvent> ReadExternalEventFiltered()
	{
		while (true)
		{
			var evt = await ReadExternalEvent().ConfigureAwait(false);

			if (evt.InvokeId is null)
>>>>>>> Stashed changes
			{
				return evt;
			}

<<<<<<< Updated upstream
			return await WaitForExternalEvent().ConfigureAwait(false);
		}

		protected virtual async ValueTask<IEvent> WaitForExternalEvent()
		{
			await NotifyWaiting().ConfigureAwait(false);

			while (await _eventQueueReader.WaitToEvent().ConfigureAwait(false))
			{
				if (_eventQueueReader.TryReadEvent(out var evt))
				{
					return evt;
				}
			}

			await ExternalQueueCompleted().ConfigureAwait(false);
			
			throw new StateMachineQueueClosedException(Resources.Exception_StateMachineExternalQueueHasBeenClosed);
		}

		protected virtual ValueTask ExternalQueueCompleted()
		{
			ThrowIfDestroying();

			return default;
		}

		private void ThrowIfDestroying()
		{
			if (_stateMachineDestroyedException is { } exception)
			{
				throw exception;
			}
		}

		protected virtual async ValueTask ExitInterpreter()
=======
			if (IsInvokeActive(evt.InvokeId))
			{
				return evt;
			}
		}
	}

	private bool IsInvokeActive(InvokeId invokeId) => _context.ActiveInvokes.Contains(invokeId);

	protected virtual async ValueTask<IEvent> ReadExternalEvent()
	{
		ThrowIfDestroying();

		if (EventQueueReader.TryReadEvent(out var evt))
		{
			return evt;
		}

		return await WaitForExternalEvent().ConfigureAwait(false);
	}

	protected virtual async ValueTask<IEvent> WaitForExternalEvent()
	{
		await NotifyWaiting().ConfigureAwait(false);

		while (await EventQueueReader.WaitToEvent().ConfigureAwait(false))
>>>>>>> Stashed changes
		{
			if (EventQueueReader.TryReadEvent(out var evt))
			{
<<<<<<< Updated upstream
				foreach (var onExit in state.OnExit)
				{
					await RunExecutableEntity(onExit.ActionEvaluators).ConfigureAwait(false);
				}

				foreach (var invoke in state.Invoke)
				{
					await CancelInvoke(invoke).ConfigureAwait(false);
				}

				_context.Configuration.Delete(state);

				if (state is FinalNode { Parent: StateMachineNode } final)
				{
					await EvaluateDoneData(final).ConfigureAwait(false);
				}
			}
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
=======
				return evt;
>>>>>>> Stashed changes
			}
		}

		await ExternalQueueCompleted().ConfigureAwait(false);

		throw new StateMachineQueueClosedException(Resources.Exception_StateMachineExternalQueueHasBeenClosed);
	}

	protected virtual ValueTask ExternalQueueCompleted()
	{
		ThrowIfDestroying();

		return default;
	}

	private void ThrowIfDestroying()
	{
		if (_stateMachineDestroyedException is { } exception)
		{
			throw exception;
		}
	}

	protected virtual async ValueTask ExitInterpreter()
	{
		var statesToExit = _context.Configuration.ToSortedList(StateEntityNode.ExitOrder);

		foreach (var state in statesToExit)
		{
			foreach (var onExit in state.OnExit)
			{
				await RunExecutableEntity(onExit.ActionEvaluators).ConfigureAwait(false);
			}

			foreach (var invoke in state.Invoke)
			{
				await CancelInvoke(invoke).ConfigureAwait(false);
			}

			_context.Configuration.Delete(state);

			if (state is FinalNode { Parent: StateMachineNode } final)
			{
				await EvaluateDoneData(final).ConfigureAwait(false);
			}
		}
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
<<<<<<< Updated upstream

			try
			{
				return await condition.EvaluateBoolean().ConfigureAwait(false);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(transition, ex).ConfigureAwait(false);

				return false;
			}
=======
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
		protected virtual async ValueTask<bool> Microstep(List<TransitionNode> enabledTransitions)
		{
			await ExitStates(enabledTransitions).ConfigureAwait(false);
			await ExecuteTransitionContent(enabledTransitions).ConfigureAwait(false);
			await EnterStates(enabledTransitions).ConfigureAwait(false);

			return _running;
		}

		protected virtual async ValueTask ExitStates(List<TransitionNode> enabledTransitions)
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
				await _logger.Write(Level.Trace, $@"Exiting state '{state.Id}'", state).ConfigureAwait(false);

				foreach (var onExit in state.OnExit)
				{
					await RunExecutableEntity(onExit.ActionEvaluators).ConfigureAwait(false);
				}

				foreach (var invoke in state.Invoke)
				{
					await CancelInvoke(invoke).ConfigureAwait(false);
				}

				_context.Configuration.Delete(state);

				await _logger.Write(Level.Trace, $@"Exited state '{state.Id}'", state).ConfigureAwait(false);
			}
		}

		private static void AddIfNotExists<T>(List<T> list, T item)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}

		private static List<StateEntityNode> ToSortedList(List<StateEntityNode> list, IComparer<StateEntityNode> comparer)
		{
			var result = new List<StateEntityNode>(list);
			result.Sort(comparer);

			return result;
		}

		private static bool HasIntersection(List<StateEntityNode> list1, List<StateEntityNode> list2)
		{
			foreach (var item in list1)
			{
				if (list2.Contains(item))
				{
					return true;
				}
			}
=======
		try
		{
			return await condition.EvaluateBoolean().ConfigureAwait(false);
		}
		catch (Exception ex) when (IsError(ex))
		{
			await Error(transition, ex).ConfigureAwait(false);
>>>>>>> Stashed changes

			return false;
		}
	}

<<<<<<< Updated upstream
		protected virtual async ValueTask EnterStates(List<TransitionNode> enabledTransitions)
		{
			var statesToEnter = new List<StateEntityNode>();
			var statesForDefaultEntry = new List<CompoundNode>();
			var defaultHistoryContent = new DefaultHistoryContent();

			ComputeEntrySet(enabledTransitions, statesToEnter, statesForDefaultEntry, defaultHistoryContent);

			foreach (var state in ToSortedList(statesToEnter, StateEntityNode.EntryOrder))
			{
				await _logger.Write(Level.Trace, $@"Entering state '{state.Id}'", state).ConfigureAwait(false);
=======
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
				(tr1 ??= [default!])[0] = t1;
				(tr2 ??= [default!])[0] = t2;
>>>>>>> Stashed changes

				if (HasIntersection(ComputeExitSet(tr1), ComputeExitSet(tr2)))
				{
<<<<<<< Updated upstream
					await InitializeDataModel(dataModel).ConfigureAwait(false);
				}

				foreach (var onEntry in state.OnEntry)
				{
					await RunExecutableEntity(onEntry.ActionEvaluators).ConfigureAwait(false);
				}

				if (state is CompoundNode compound && statesForDefaultEntry.Contains(compound))
				{
					await RunExecutableEntity(compound.Initial.Transition.ActionEvaluators).ConfigureAwait(false);
				}

				if (defaultHistoryContent.TryGetValue(state.Id, out var action))
				{
					await RunExecutableEntity(action).ConfigureAwait(false);
				}

				if (state is FinalNode final)
				{
					if (final.Parent is StateMachineNode)
					{
						_running = false;
=======
					if (IsDescendant(t1.Source, t2.Source))
					{
						(transitionsToRemove ??= []).Add(t2);
>>>>>>> Stashed changes
					}
					else
					{
						t1Preempted = true;
						break;
					}
				}
<<<<<<< Updated upstream

				await _logger.Write(Level.Trace, $@"Entered state '{state.Id}'", state).ConfigureAwait(false);
			}
		}

		private async ValueTask<DataModelValue> EvaluateDoneData(DoneDataNode doneData)
		{
			try
			{
				return await doneData.Evaluate().ConfigureAwait(false);
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
=======
			}

			if (!t1Preempted)
			{
				if (transitionsToRemove is not null)
>>>>>>> Stashed changes
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

	protected virtual async ValueTask<bool> Microstep(List<TransitionNode> enabledTransitions)
	{
		await ExitStates(enabledTransitions).ConfigureAwait(false);
		await ExecuteTransitionContent(enabledTransitions).ConfigureAwait(false);
		await EnterStates(enabledTransitions).ConfigureAwait(false);

		return _running;
	}

	protected virtual async ValueTask ExitStates(List<TransitionNode> enabledTransitions)
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
			await Logger.Write(Level.Trace, $@"Exiting state '{state.Id}'", state).ConfigureAwait(false);

			foreach (var onExit in state.OnExit)
			{
				await RunExecutableEntity(onExit.ActionEvaluators).ConfigureAwait(false);
			}

			foreach (var invoke in state.Invoke)
			{
				await CancelInvoke(invoke).ConfigureAwait(false);
			}

			_context.Configuration.Delete(state);

			await Logger.Write(Level.Trace, $@"Exited state '{state.Id}'", state).ConfigureAwait(false);
		}
	}

	private static void AddIfNotExists<T>(List<T> list, T item)
	{
		if (!list.Contains(item))
		{
			list.Add(item);
		}
	}

	private static List<StateEntityNode> ToSortedList(List<StateEntityNode> list, IComparer<StateEntityNode> comparer)
	{
		var result = new List<StateEntityNode>(list);
		result.Sort(comparer);

		return result;
	}

	private static bool HasIntersection(List<StateEntityNode> list1, List<StateEntityNode> list2)
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

	protected virtual async ValueTask EnterStates(List<TransitionNode> enabledTransitions)
	{
		var statesToEnter = new List<StateEntityNode>();
		var statesForDefaultEntry = new List<CompoundNode>();
		var defaultHistoryContent = new DefaultHistoryContent();

		ComputeEntrySet(enabledTransitions, statesToEnter, statesForDefaultEntry, defaultHistoryContent);

		foreach (var state in ToSortedList(statesToEnter, StateEntityNode.EntryOrder))
		{
			await Logger.Write(Level.Trace, $@"Entering state '{state.Id}'", state).ConfigureAwait(false);

			_context.Configuration.AddIfNotExists(state);
			_context.StatesToInvoke.AddIfNotExists(state);

			if (Model.Root.Binding == BindingType.Late && state.DataModel is { } dataModel)
			{
				await InitializeDataModel(dataModel).ConfigureAwait(false);
			}

			foreach (var onEntry in state.OnEntry)
			{
				await RunExecutableEntity(onEntry.ActionEvaluators).ConfigureAwait(false);
			}

			if (state is CompoundNode compound && statesForDefaultEntry.Contains(compound))
			{
				await RunExecutableEntity(compound.Initial.Transition.ActionEvaluators).ConfigureAwait(false);
			}

			if (defaultHistoryContent.TryGetValue(state.Id, out var action))
			{
				await RunExecutableEntity(action).ConfigureAwait(false);
			}

			if (state is FinalNode final)
			{
				if (final.Parent is StateMachineNode)
				{
					_running = false;
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

			await Logger.Write(Level.Trace, $@"Entered state '{state.Id}'", state).ConfigureAwait(false);
		}
	}

	private async ValueTask<DataModelValue> EvaluateDoneData(DoneDataNode doneData)
	{
		try
		{
			return await doneData.Evaluate().ConfigureAwait(false);
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

			(states ??= []).Add(s);
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

	protected virtual async ValueTask ExecuteTransitionContent(List<TransitionNode> transitions)
	{
		foreach (var transition in transitions)
		{
			string? eventDescriptor = default;
			string? target = default;
			var traceEnabled = Logger.IsEnabled(Level.Trace);

			if (traceEnabled)
			{
				eventDescriptor = EventDescriptor.ToString(transition.EventDescriptors);
				target = Identifier.ToString(transition.Target);

				if (eventDescriptor is not null)
				{
					await Logger.Write(Level.Trace, $@"Performing eventless {transition.Type} transition to '{target}'", transition).ConfigureAwait(false);
				}
				else
				{
					await Logger.Write(Level.Trace, $@"Performing {transition.Type} transition to '{target}'. Event descriptor '{eventDescriptor}'", transition).ConfigureAwait(false);
				}
			}

<<<<<<< Updated upstream
			return targets;
		}

		protected virtual async ValueTask ExecuteTransitionContent(List<TransitionNode> transitions)
		{
			foreach (var transition in transitions)
			{
				string? eventDescriptor = default;
				string? target = default;
				var traceEnabled = _logger.IsEnabled(Level.Trace);

				if (traceEnabled)
				{
					eventDescriptor = EventDescriptor.ToString(transition.EventDescriptors);
					target = Identifier.ToString(transition.Target);

					if (eventDescriptor is not null)
					{
						await _logger.Write(Level.Trace, $@"Performing eventless {transition.Type} transition to '{target}'", transition).ConfigureAwait(false);
					}
					else
					{
						await _logger.Write(Level.Trace, $@"Performing {transition.Type} transition to '{target}'. Event descriptor '{eventDescriptor}'", transition).ConfigureAwait(false);
					}
				}

				await RunExecutableEntity(transition.ActionEvaluators).ConfigureAwait(false);

				if (traceEnabled)
				{
					if (eventDescriptor is not null)
					{
						await _logger.Write(Level.Trace, $@"Performing eventless {transition.Type} transition to '{target}'", transition).ConfigureAwait(false);
					}
					else
					{
						await _logger.Write(Level.Trace, $@"Performing {transition.Type} transition to '{target}'. Event descriptor '{eventDescriptor}'", transition).ConfigureAwait(false);
					}
				}
			}
		}

		protected virtual async ValueTask RunExecutableEntity(ImmutableArray<IExecEvaluator> action)
		{
			if (!action.IsDefaultOrEmpty)
=======
			await RunExecutableEntity(transition.ActionEvaluators).ConfigureAwait(false);

			if (traceEnabled)
>>>>>>> Stashed changes
			{
				if (eventDescriptor is not null)
				{
<<<<<<< Updated upstream
					try
					{
						await executableEntity.Execute().ConfigureAwait(false);
					}
					catch (Exception ex) when (IsError(ex))
					{
						await Error(executableEntity, ex).ConfigureAwait(false);

						break;
					}
				}
			}
		}

		private static bool IsError(Exception _) => true;// TODO: Is not OperationCanceled or ObjectDisposed when SM halted?

		private bool IsPlatformError(Exception exception)
		{
			for (var ex = exception; ex is not null; ex = ex.InnerException)
			{
				if (ex is PlatformException platformException)
				{
					if (platformException.Token == _stateMachineToken)
					{
						return true;
					}

					break;
				}
			}

			return false;
		}

		private bool IsCommunicationError(Exception? exception, out SendId? sendId)
		{
			for (var ex = exception; ex is not null; ex = ex.InnerException)
			{
				if (ex is CommunicationException communicationException)
				{
					if (communicationException.Token == _stateMachineToken)
					{
						sendId = communicationException.SendId;

						return true;
					}

					break;
				}
			}

			sendId = default;

			return false;
		}
=======
					await Logger.Write(Level.Trace, $@"Performing eventless {transition.Type} transition to '{target}'", transition).ConfigureAwait(false);
				}
				else
				{
					await Logger.Write(Level.Trace, $@"Performing {transition.Type} transition to '{target}'. Event descriptor '{eventDescriptor}'", transition).ConfigureAwait(false);
				}
			}
		}
	}

	protected virtual async ValueTask RunExecutableEntity(ImmutableArray<IExecEvaluator> action)
	{
		if (!action.IsDefaultOrEmpty)
		{
			foreach (var executableEntity in action)
			{
				try
				{
					await executableEntity.Execute().ConfigureAwait(false);
				}
				catch (Exception ex) when (IsError(ex))
				{
					await Error(executableEntity, ex).ConfigureAwait(false);

					break;
				}
			}
		}
	}

	private static bool IsError(Exception _) => true; // TODO: Is not OperationCanceled or ObjectDisposed when SM halted?

	private bool IsPlatformError(Exception exception)
	{
		for (var ex = exception; ex is not null; ex = ex.InnerException)
		{
			if (ex is PlatformException platformException)
			{
				if (platformException.Token == _stateMachineToken)
				{
					return true;
				}

				break;
			}
		}

		return false;
	}

	private bool IsCommunicationError(Exception? exception, out SendId? sendId)
	{
		for (var ex = exception; ex is not null; ex = ex.InnerException)
		{
			if (ex is CommunicationException communicationException)
			{
				if (communicationException.Token == _stateMachineToken)
				{
					sendId = communicationException.SendId;

					return true;
				}

				break;
			}
		}

		sendId = default;
>>>>>>> Stashed changes

		return false;
	}

	private async ValueTask Error(object source, Exception exception, bool logLoggerErrors = true)
	{
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
					  Data = DataConverter.FromException(exception),
					  SendId = sendId,
					  Ancestor = exception
				  };

		_context.InternalQueue.Enqueue(evt);

		if (Logger.IsEnabled(Level.Error))
		{
<<<<<<< Updated upstream
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
						  Data = _dataConverter.FromException(exception),
						  SendId = sendId,
						  Ancestor = exception
					  };

			_context.InternalQueue.Enqueue(evt);

			if (_logger.IsEnabled(Level.Error))
			{
				try
=======
			try
			{
				await LogError(errorType, source, exception).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				if (logLoggerErrors)
>>>>>>> Stashed changes
				{
					await LogError(errorType, source, exception).ConfigureAwait(false);
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
		}
	}

<<<<<<< Updated upstream
		private async ValueTask LogError(ErrorType errorType, object source, Exception exception)
		{
			try
			{
				var entityId = source.Is(out IDebugEntityId? id) ? id.EntityId : default;

				await _logger.Write(Level.Error, $@"{errorType} error in entity '{entityId}'.", exception).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new PlatformException(ex) { Token = _stateMachineToken };
			}
		}

		protected virtual async ValueTask ExecuteGlobalScript()
		{
			if (_model.Root.ScriptEvaluator is { } scriptEvaluator)
			{
				try
				{
					await scriptEvaluator.Execute().ConfigureAwait(false);
				}
				catch (Exception ex) when (IsError(ex))
				{
					await Error(scriptEvaluator, ex).ConfigureAwait(false);
				}
			}
=======
	private async ValueTask LogError(ErrorType errorType, object source, Exception exception)
	{
		try
		{
			var entityId = source.Is(out IDebugEntityId? id) ? id.EntityId : default;

			await Logger.Write(Level.Error, $@"{errorType} error in entity '{entityId}'.", exception).ConfigureAwait(false);
>>>>>>> Stashed changes
		}
		catch (Exception ex)
		{
			throw new PlatformException(ex) { Token = _stateMachineToken };
		}
	}

	protected virtual async ValueTask ExecuteGlobalScript()
	{
		if (Model.Root.ScriptEvaluator is { } scriptEvaluator)
		{
			try
			{
<<<<<<< Updated upstream
				Infra.NotNull(invoke.InvokeId);

				await ForwardEvent(evt, invoke.InvokeId).ConfigureAwait(false);
=======
				await scriptEvaluator.Execute().ConfigureAwait(false);
>>>>>>> Stashed changes
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
			_context.DoneData = await EvaluateDoneData(final.DoneData).ConfigureAwait(false);
		}
	}

<<<<<<< Updated upstream
		protected virtual async ValueTask Invoke(InvokeNode invoke)
		{
			try
			{
				await invoke.Start().ConfigureAwait(false);

				Infra.NotNull(invoke.InvokeId);

				_context.ActiveInvokes.Add(invoke.InvokeId);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(invoke, ex).ConfigureAwait(false);
			}
		}

		protected virtual async ValueTask CancelInvoke(InvokeNode invoke)
		{
			try
			{
				Infra.NotNull(invoke.InvokeId);

				_context.ActiveInvokes.Remove(invoke.InvokeId);

				await invoke.Cancel().ConfigureAwait(false);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(invoke, ex).ConfigureAwait(false);
			}
		}

		protected virtual async ValueTask InitializeDataModel(DataModelNode rootDataModel, DataModelList? defaultValues = default)
		{
			foreach (var node in rootDataModel.Data)
			{
				await InitializeData(node, defaultValues).ConfigureAwait(false);
			}
		}

		protected virtual async ValueTask InitializeData(DataNode data, DataModelList? defaultValues)
		{
			Infra.Requires(data);

			var caseInsensitive = _dataModelHandler.CaseInsensitive;

			if (defaultValues?[data.Id, caseInsensitive] is not { Type: not DataModelValueType.Undefined } value)
			{
				try
				{
					value = await GetValue(data).ConfigureAwait(false);
				}
				catch (Exception ex) when (IsError(ex))
				{
					await Error(data, ex).ConfigureAwait(false);

					return;
				}
			}

			_context.DataModel[data.Id, caseInsensitive] = value;
		}

		private async ValueTask<DataModelValue> GetValue(DataNode data)
		{
			if (data.ResourceEvaluator is { } resourceEvaluator)
=======
	private async ValueTask ForwardEvent(InvokeNode invoke, IEvent evt)
	{
		try
		{
			Infra.NotNull(invoke.InvokeId);

			await ForwardEvent(evt, invoke.InvokeId).ConfigureAwait(false);
		}
		catch (Exception ex) when (IsError(ex))
		{
			await Error(invoke, ex).ConfigureAwait(false);
		}
	}

	private ValueTask ApplyFinalize(InvokeNode invoke) => invoke.Finalize is not null ? RunExecutableEntity(invoke.Finalize.ActionEvaluators) : default;

	protected virtual async ValueTask Invoke(InvokeNode invoke)
	{
		try
		{
			await invoke.Start().ConfigureAwait(false);

			Infra.NotNull(invoke.InvokeId);

			_context.ActiveInvokes.Add(invoke.InvokeId);
		}
		catch (Exception ex) when (IsError(ex))
		{
			await Error(invoke, ex).ConfigureAwait(false);
		}
	}

	protected virtual async ValueTask CancelInvoke(InvokeNode invoke)
	{
		try
		{
			Infra.NotNull(invoke.InvokeId);

			_context.ActiveInvokes.Remove(invoke.InvokeId);

			await invoke.Cancel().ConfigureAwait(false);
		}
		catch (Exception ex) when (IsError(ex))
		{
			await Error(invoke, ex).ConfigureAwait(false);
		}
	}

	protected virtual async ValueTask InitializeDataModel(DataModelNode rootDataModel, DataModelList? defaultValues = default)
	{
		foreach (var node in rootDataModel.Data)
		{
			await InitializeData(node, defaultValues).ConfigureAwait(false);
		}
	}

	protected virtual async ValueTask InitializeData(DataNode data, DataModelList? defaultValues)
	{
		Infra.Requires(data);

		var id = data.Id;
		Infra.NotNull(id);

		if (defaultValues?[id, DataModelHandler.CaseInsensitive] is not { Type: not DataModelValueType.Undefined } value)
		{
			try
			{
				value = await GetValue(data).ConfigureAwait(false);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(data, ex).ConfigureAwait(false);

				return;
			}
		}

		_context.DataModel[id] = value;
	}

	private static async ValueTask<DataModelValue> GetValue(DataNode data)
	{
		if (data.SourceEvaluator is { } resourceEvaluator)
		{
			var obj = await resourceEvaluator.EvaluateObject().ConfigureAwait(false);

			return DataModelValue.FromObject(obj);
		}

		if (data.ExpressionEvaluator is { } expressionEvaluator)
		{
			var obj = await expressionEvaluator.EvaluateObject().ConfigureAwait(false);

			return DataModelValue.FromObject(obj);
		}

		if (data.InlineContentEvaluator is { } inlineContentEvaluator)
		{
			var obj = await inlineContentEvaluator.EvaluateObject().ConfigureAwait(false);

			return DataModelValue.FromObject(obj);
		}

		return default;
	}

	/*
	private async ValueTask<IStateMachineContext> CreateContext()
	{
		Infra.NotNull(_model);
		Infra.NotNull(_dataModelHandler);

		IStateMachineContext context;
		var parameters = CreateStateMachineContextParameters();

		if (_isPersistingEnabled)
		{
			Infra.NotNull(_options.StorageProvider);

			var storage = await _options.StorageProvider.GetTransactionalStorage(partition: default, StateStorageKey, _stopCts.Token).ConfigureAwait(false);
			context = new StateMachinePersistedContext(storage, _model.EntityMap, parameters);
		}
		else
		{
			context = new StateMachineContext(parameters);
		}

		_dataModelHandler.ExecutionContextCreated(_executionContext, out _dataModelVars);

		return context;
	}*/

	private struct LiveLockDetector : IDisposable
	{
		private const int IterationCount = 36;

		private int[]? _data;
		private int    _index;
		private int    _queueLength;
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

		public bool IsLiveLockDetected(int queueLength)
		{
			if (_index == -1)
			{
				_queueLength = queueLength;
				_index = _sum = 0;

				return false;
			}

			_data ??= ArrayPool<int>.Shared.Rent(IterationCount);

			if (_index >= IterationCount)
>>>>>>> Stashed changes
			{
				if (_sum >= 0)
				{
<<<<<<< Updated upstream
					var obj = await resourceEvaluator.EvaluateObject(resource).ConfigureAwait(false);

					return DataModelValue.FromObject(obj);
				}
			}

			if (data.ExpressionEvaluator is { } expressionEvaluator)
			{
				var obj = await expressionEvaluator.EvaluateObject().ConfigureAwait(false);

				return DataModelValue.FromObject(obj);
			}

			if (data.InlineContentEvaluator is { } inlineContentEvaluator)
			{
				var obj = await inlineContentEvaluator.EvaluateObject().ConfigureAwait(false);

				return DataModelValue.FromObject(obj);
			}

			return default;
		}

		private ValueTask<Resource> LoadData(IExternalDataExpression externalDataExpression)
		{
			var uri = _stateMachineLocation?.Location.CombineWith(externalDataExpression.Uri);
			//TODO:
			//var factoryContext = new FactoryContext(_options.ServiceLocator, _options.ResourceLoaderFactories, _options.SecurityContext, _options.Logger, this);
			//var resourceLoaderService = _options.ServiceLocator.GetService<IResourceLoaderService>();

			return _resourceLoader.Request(uri);
		}
		/*
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
		}*/
		/*
		private async ValueTask<IStateMachineContext> CreateContext()
		{
			Infra.NotNull(_model);
			Infra.NotNull(_dataModelHandler);

			IStateMachineContext context;
			var parameters = CreateStateMachineContextParameters();

			if (_isPersistingEnabled)
			{
				Infra.NotNull(_options.StorageProvider);

				var storage = await _options.StorageProvider.GetTransactionalStorage(partition: default, StateStorageKey, _stopCts.Token).ConfigureAwait(false);
				context = new StateMachinePersistedContext(storage, _model.EntityMap, parameters);
			}
			else
			{
				context = new StateMachineContext(parameters);
			}

			_dataModelHandler.ExecutionContextCreated(_executionContext, out _dataModelVars);

			return context;
		}*/

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

			var typeInfo = TypeInfo<int>.Instance;//_dataModelHandler.TypeInfo;

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

		private struct LiveLockDetector : IDisposable
		{
			private const int IterationCount = 36;

			private int[]? _data;
			private int    _index;
			private int    _queueLength;
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

			public bool IsLiveLockDetected(int queueLength)
			{
				if (_index == -1)
				{
					_queueLength = queueLength;
					_index = _sum = 0;

					return false;
				}

				_data ??= ArrayPool<int>.Shared.Rent(IterationCount);

				if (_index >= IterationCount)
				{
					if (_sum >= 0)
					{
						return true;
					}

					_sum -= _data[_index % IterationCount];
				}

				var delta = queueLength - _queueLength;
				_queueLength = queueLength;
				_sum += delta;
				_data[_index ++ % IterationCount] = delta;

				return false;
=======
					return true;
				}

				_sum -= _data[_index % IterationCount];
>>>>>>> Stashed changes
			}

			var delta = queueLength - _queueLength;
			_queueLength = queueLength;
			_sum += delta;
			_data[_index ++ % IterationCount] = delta;

			return false;
		}
	}
}