using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate.Core;

public interface IPersistingInterpreterState
{
	public Bucket StateBucket { get; }

	public ValueTask CheckPoint(int level);
}

public class StateMachinePersistingInterpreterBase3 : StateMachinePersistingInterpreterBase2
{
	private readonly IPersistingInterpreterState _persistingInterpreterState;

	[SetsRequiredMembers]
	public StateMachinePersistingInterpreterBase3(IPersistingInterpreterState persistingInterpreterState,
												  IInterpreterModel interpreterModel,
												  IEventQueueReader eventQueueReader,
												  IDataModelHandler dataModelHandler,
												  IStateMachineContext stateMachineContext,
												  INotifyStateChanged? notifyStateChanged,
												  IUnhandledErrorBehaviour? unhandledErrorBehaviour,
												  IResourceLoader resourceLoader,
												  IStateMachineLocation? stateMachineLocation,
												  IExternalCommunication? externalCommunication,
												  ILogger<IStateMachineInterpreter> logger) : base(
		persistingInterpreterState, 
		interpreterModel, 
		eventQueueReader, 
		dataModelHandler, 
		stateMachineContext, 
		notifyStateChanged, 
		unhandledErrorBehaviour, 
		resourceLoader, 
		stateMachineLocation,
		externalCommunication, 
		logger)
	{
		_persistingInterpreterState = persistingInterpreterState;
	}
}

public class StateMachinePersistingInterpreterBase2 : StateMachinePersistingInterpreterBase
{
	private readonly IPersistingInterpreterState _persistingInterpreterState;

	[SetsRequiredMembers]
	public StateMachinePersistingInterpreterBase2(IPersistingInterpreterState persistingInterpreterState,
												  IInterpreterModel interpreterModel,
												  IEventQueueReader eventQueueReader,
												  IDataModelHandler dataModelHandler,
												  IStateMachineContext stateMachineContext,
												  INotifyStateChanged? notifyStateChanged,
												  IUnhandledErrorBehaviour? unhandledErrorBehaviour,
												  IResourceLoader resourceLoader,
												  IStateMachineLocation? stateMachineLocation,
												  IExternalCommunication? externalCommunication,
												  ILogger<IStateMachineInterpreter> logger) : base(
		persistingInterpreterState, 
		interpreterModel, 
		eventQueueReader, 
		dataModelHandler, 
		stateMachineContext, 
		notifyStateChanged, 
		unhandledErrorBehaviour, 
		resourceLoader, 
		stateMachineLocation,
		externalCommunication, 
		logger)
	{
		_persistingInterpreterState = persistingInterpreterState;
	}

	protected override async ValueTask<IEvent> ReadExternalEvent()
	{
		await _persistingInterpreterState.CheckPoint(16).ConfigureAwait(false);

		return await base.ReadExternalEvent().ConfigureAwait(false);
	}
}

public class StateMachinePersistingInterpreterBase : StateMachineInterpreter
{
	private const int KeyIndex             = 0;
	private const int MethodCompletedIndex = 1;
	private const int ValueIndex           = 2;
	private const int ReturnCallIndex      = 0;

	private readonly IPersistingInterpreterState _persistingInterpreterState;
	private readonly IInterpreterModel           _interpreterModel;
	private readonly Bucket                      _stateBucket;
	private          Bucket                      _methodBucket;
	private          Bucket                      _callBucket;
	private          int                         _methodIndex  = 1;
	private          int                         _callIndex    = 1;
	private          bool                         _suspending;

	//public required IStateMachineInterpreterTracer1? StateMachineInterpreterTracer { private get; init; }
	//public required ITmpLogger2<IStateMachineInterpreter>? Logger { private get; init; }

	[SetsRequiredMembers]
	public StateMachinePersistingInterpreterBase(IPersistingInterpreterState persistingInterpreterState,
												 IInterpreterModel interpreterModel,
												 IEventQueueReader eventQueueReader,
												 IDataModelHandler dataModelHandler,
												 IStateMachineContext stateMachineContext,
												 INotifyStateChanged? notifyStateChanged,
												 IUnhandledErrorBehaviour? unhandledErrorBehaviour,
												 IResourceLoader resourceLoader,
												 IStateMachineLocation? stateMachineLocation,
												 IExternalCommunication? externalCommunication,
												 ILogger<IStateMachineInterpreter> logger)
		: base(
			interpreterModel,
			eventQueueReader,
			dataModelHandler,
			stateMachineContext,
			notifyStateChanged,
			unhandledErrorBehaviour,
			resourceLoader,
			stateMachineLocation,
			externalCommunication,
			logger)
	{
		Infra.Requires(persistingInterpreterState);

		_persistingInterpreterState = persistingInterpreterState;
		_interpreterModel = interpreterModel;
		_stateBucket = persistingInterpreterState.StateBucket;
		_methodBucket = _stateBucket.Nested(_methodIndex);
		_callBucket = _methodBucket.Nested(_callIndex);
	}

	public virtual void TriggerSuspendSignal()
	{
		_suspending = true;
		StopWaitingExternalEvents();
	}

	public override async ValueTask<DataModelValue> RunAsync()
	{
		try
		{
			return await base.RunAsync().ConfigureAwait(false);
		}
		catch (StateMachineSuspendedException)
		{
			await TraceInterpreterState(StateMachineInterpreterState.Suspended).ConfigureAwait(false);

			throw;
		}
	}

	protected override async ValueTask EnterSteps()
	{
		if (Enter(StateBagKey.EnterSteps, out _))
		{
			return;
		}

		await base.EnterSteps().ConfigureAwait(false);

		Complete(StateBagKey.EnterSteps);
	}

	protected override async ValueTask MainEventLoop()
	{
		if (Enter(StateBagKey.MainEventLoop, out _))
		{
			return;
		}

		await base.MainEventLoop().ConfigureAwait(false);

		Complete(StateBagKey.MainEventLoop);
	}

	protected override async ValueTask<bool> MainEventLoopIteration()
	{
		Enter(StateBagKey.MainEventLoopIteration, out _);

		var result = await base.MainEventLoopIteration().ConfigureAwait(false);

		Complete(StateBagKey.MainEventLoopIteration, iteration: true);

		return result;
	}

	protected override async ValueTask<bool> StartInvokeLoop()
	{
		if (Enter(StateBagKey.StartInvokeLoop, out var valueBucket))
		{
			return valueBucket.GetBoolean(Bucket.RootKey);
		}

		var result = await base.StartInvokeLoop().ConfigureAwait(false);

		Complete(StateBagKey.StartInvokeLoop);
		valueBucket.Add(Bucket.RootKey, result);

		return result;
	}

	protected override async ValueTask<bool> ExternalQueueProcess()
	{
		if (Enter(StateBagKey.ExternalQueueProcess, out var valueBucket))
		{
			return valueBucket.GetBoolean(Bucket.RootKey);
		}

		var result = await base.ExternalQueueProcess().ConfigureAwait(false);

		Complete(StateBagKey.ExternalQueueProcess);
		valueBucket.Add(Bucket.RootKey, result);

		return result;
	}

	protected override async ValueTask<List<TransitionNode>> ExternalEventTransitions()
	{
		if (Enter(StateBagKey.ExternalEventTransitions, out var valueBucket))
		{
			return GetTransitionNodes(valueBucket);
		}

		var result = await base.ExternalEventTransitions().ConfigureAwait(false);

		Complete(StateBagKey.ExternalEventTransitions);
		AddTransitionNodes(valueBucket, result);

		return result;
	}

	protected List<TransitionNode> GetTransitionNodes(Bucket bucket)
	{
		var stateMachineNode = _interpreterModel.Root;
		var length = bucket.GetInt32(Bucket.RootKey);
		var list = new List<TransitionNode>(length);
		
		for (var i = 0; i < length; i++)
		{
			list.Add(FindTransitionNode(stateMachineNode, bucket.GetInt32(i)));
		}

		return list;
	}

	private static TransitionNode FindTransitionNode(StateEntityNode node, int documentId)
	{
		if (node.Transitions is { IsDefaultOrEmpty: false } transitions && transitions[0].DocumentId <= documentId)
		{
			foreach (var transition in transitions)
			{
				if (transition.DocumentId == documentId)
				{
					return transition;
				}
			}
		}
		else if (node.States is { IsDefaultOrEmpty: false } states)
		{
			for (var i = 1; i < states.Length; i++)
			{
				if (documentId < states[i].DocumentId)
				{
					return FindTransitionNode(states[i - 1], documentId);
				}
			}

			return FindTransitionNode(states[^1], documentId);
		}

		throw new KeyNotFoundException(Res.Format(Resources.Exception_TransitionNodeWithDocumentIdNotFound, documentId));
	}

	protected static void AddTransitionNodes(Bucket bucket, List<TransitionNode> list)
	{
		Infra.Requires(list);

		bucket.Add(Bucket.RootKey, list.Count);
		for (var i = 0; i < list.Count; i++)
		{
			bucket.Add(i, list[i].DocumentId);
		}
	}

	protected override async ValueTask<IEvent> WaitForExternalEvent()
	{
		await _persistingInterpreterState.CheckPoint(0).ConfigureAwait(false);

		return await base.WaitForExternalEvent().ConfigureAwait(false);
	}

	protected override ValueTask ExternalQueueCompleted()
	{
		if (_suspending)
		{
			throw new StateMachineSuspendedException(Resources.Exception_StateMachineHasBeenSuspended);
		}

		return base.ExternalQueueCompleted();
	}

	protected bool Enter(StateBagKey key, out Bucket valueBucket)
	{
		if (_callBucket.TryGet(KeyIndex, out StateBagKey savedKey))
		{
			Infra.Assert(savedKey == key);
		}

		if (!_callBucket.TryGet(MethodCompletedIndex, out bool methodCompleted))
		{
			_callBucket.Add(MethodCompletedIndex, value: false);
			_callBucket.Add(KeyIndex, key);
		}

		if (methodCompleted)
		{
			_callIndex ++;
		}
		else
		{
			_methodBucket = _stateBucket.Nested(++ _methodIndex);
			_methodBucket.Nested(0).Add(ReturnCallIndex, _callIndex);
			_callIndex = 1;
		}

		_callBucket = _methodBucket.Nested(_callIndex);
		valueBucket = _callBucket.Nested(ValueIndex);

		return methodCompleted;
	}

	protected void Complete(StateBagKey key, bool iteration = false)
	{
		var returnCallIndex = _methodBucket.Nested(0).GetInt32(ReturnCallIndex);

		_methodBucket.RemoveSubtree(Bucket.RootKey);
		_methodBucket = _stateBucket.Nested(-- _methodIndex);
		_callIndex = returnCallIndex;
		_callBucket = _methodBucket.Nested(_callIndex);

		Infra.Assert(_callBucket.GetEnum(KeyIndex).As<StateBagKey>() == key);
		Infra.Assert(!_callBucket.GetBoolean(MethodCompletedIndex));
		
		if (iteration)
		{
			_callBucket.RemoveSubtree(Bucket.RootKey);
		}
		else
		{
			_callBucket.Add(MethodCompletedIndex, value: true);
			_callBucket = _methodBucket.Nested(++ _callIndex);
		}
	}

	protected enum StateBagKey
	{
		None,
		ExecuteGlobalScript,
		InitialEnterStates,
		ExitStates,
		EnterStates,
		NotifyAccepted,
		NotifyStarted,
		NotifyExited,
		NotifyWaiting,
		Interpret,
		ExitSteps,
		InitializeDataModels,
		ExternalEventTransitions,
		MainEventLoopIteration,
		StartInvokeLoop,
		Microstep,
		InternalQueueProcess,
		IsInternalQueueEmpty,
		MacrostepIteration,
		Macrostep,
		ExternalQueueProcess,
		SelectTransitions,
		MainEventLoop,
		ExitInterpreter,
		ExecuteTransitionContent,
		RunExecutableEntity,
		Invoke,
		CancelInvoke,
		InitializeDataModel,
		InitializeData,
		EnterSteps
	}
}