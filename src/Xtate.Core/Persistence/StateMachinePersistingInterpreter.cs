// Copyright © 2019-2025 Sergii Artemenko
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

using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate.Core;

public interface ISuspendEventDispatcher
{
    event Action OnSuspend;
}

public interface IPersistenceOptions
{
    PersistenceLevel PersistenceLevel { get; }
}

public class StateMachinePersistingInterpreter : StateMachineInterpreter
{
    private const int KeyIndex = 0;

    private const int ValueIndex = 1;

    public required IInterpreterModel InterpreterModel { private get; [UsedImplicitly] init; }

    public required IStorage StateStorage
    {
        [UsedImplicitly]
        init
        {
            _stateBucket = new Bucket(value);
            _storage = value;
        }
    }

    private bool _suspending;

    private int _index;

    private int _subIndex;

    private readonly IStorage _storage;

    private readonly Bucket _stateBucket;

    [UsedImplicitly]
    public required ISuspendEventDispatcher SuspendEventDispatcher
    {
        init => value.OnSuspend += Suspend;
    }

    public required IPersistenceOptions PersistenceOptions { private get; [UsedImplicitly] init; }

    private void Suspend()
    {
        _suspending = true;

        StopWaitingExternalEvents();
    }

    protected override async ValueTask ExternalQueueCompleted()
    {
        if (_suspending)
        {
            throw new StateMachineSuspendedException(Resources.Exception_StateMachineHasBeenSuspended);
        }

        await base.ExternalQueueCompleted().ConfigureAwait(false);
    }

    protected override ValueTask HandleMainLoopException(Exception ex) =>
        _suspending && ex is StateMachineSuspendedException ? NotifyInterpreterState(StateMachineInterpreterState.Suspended) : base.HandleMainLoopException(ex);

    private bool Enter(StateBagKey key) => Enter(key, out _, iteration: false);

    private void Exit(StateBagKey key) => Exit(key, out _, iteration: false);

    private bool Enter(StateBagKey key, out bool result)
    {
        if (!Enter(key, out var bucket, iteration: false))
        {
            result = false;

            return false;
        }

        result = bucket.TryGet(ValueIndex, out bool value) && value;

        return true;
    }

    private void Exit(StateBagKey key, bool result)
    {
        Exit(key, out var bucket, iteration: false);

        if (result)
        {
            bucket.Add(ValueIndex, value: true);
        }
    }

    private bool Enter(StateBagKey key, [NotNullWhen(true)] out List<TransitionNode>? result)
    {
        if (!Enter(key, out var bucket, iteration: false))
        {
            result = null;

            return false;
        }

        bucket = bucket.Nested(ValueIndex);

        var length = bucket.GetInt32(Bucket.RootKey);

        var entityMap = InterpreterModel.EntityMap;
        result = new List<TransitionNode>(length);

        for (var i = 0; i < length; i ++)
        {
            var documentId = bucket.GetInt32(i);

            if (!entityMap.TryGetEntityByDocumentId(documentId, out var entity) || entity is not TransitionNode transitionNode)
            {
                throw new KeyNotFoundException(Res.Format(Resources.Exception_TransitionNodeWithDocumentIdNotFound, documentId));
            }

            result.Add(transitionNode);
        }

        return true;
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
            for (var i = 1; i < states.Length; i ++)
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

    private void Exit(StateBagKey key, List<TransitionNode> result)
    {
        Exit(key, out var bucket, iteration: false);
        bucket = bucket.Nested(ValueIndex);

        bucket.Add(Bucket.RootKey, result.Count);

        for (var i = 0; i < result.Count; i ++)
        {
            bucket.Add(i, result[i].DocumentId);
        }
    }

    private void EnterIteration(StateBagKey key) => Enter(key, out _, iteration: true);

    private void ExitIteration(StateBagKey key) => Exit(key, out _, iteration: true);

    private bool Enter(StateBagKey key, out Bucket bucket, bool iteration)
    {
        var subBucket = _stateBucket.Nested(_index).Nested(_subIndex);

        if (subBucket.TryGet(KeyIndex, out StateBagKey savedKey) && savedKey != key)
        {
            Infra.Fail();
        }

        if (!subBucket.TryGet(Bucket.RootKey, out MethodState methodState))
        {
            subBucket.Add(Bucket.RootKey, MethodState.Executing);
            subBucket.Add(KeyIndex, key);

            methodState = MethodState.Executing;
        }

        switch (methodState)
        {
            case MethodState.Executing:
                _index ++;
                _stateBucket.Add(_index, _subIndex);
                _subIndex = 0;
                bucket = default;

                return false;

            case MethodState.Completed:
                if (iteration)
                {
                    Infra.Fail();
                }

                _subIndex ++;
                bucket = subBucket;

                return true;

            default:
                throw Infra.Unmatched(methodState);
        }
    }

    private void Exit(StateBagKey key, out Bucket bucket, bool iteration)
    {
        var topBucket = _stateBucket.Nested(_index);
        var getStatus = topBucket.TryGet(Bucket.RootKey, out int subIndex);
        Infra.Assert(getStatus);
        _index --;
        _subIndex = subIndex;

        var subBucket = _stateBucket.Nested(_index).Nested(_subIndex);

        if (!subBucket.TryGet(KeyIndex, out StateBagKey savedKey) || savedKey != key)
        {
            Infra.Fail();
        }

        if (!subBucket.TryGet(Bucket.RootKey, out MethodState methodState) || methodState != MethodState.Executing)
        {
            Infra.Fail();
        }

        topBucket.RemoveSubtree(Bucket.RootKey);

        if (iteration)
        {
            subBucket.RemoveSubtree(Bucket.RootKey);
        }
        else
        {
            subBucket.Add(Bucket.RootKey, MethodState.Completed);
            _subIndex ++;
        }

        bucket = subBucket;
    }

    protected virtual async ValueTask CheckPoint(PersistenceLevel level)
    {
        if (level <= PersistenceOptions.PersistenceLevel && _storage is ITransactionalStorage transactionalStorage)
        {
            await transactionalStorage.CheckPoint((int)level).ConfigureAwait(false);

            if (level == PersistenceLevel.StableState)
            {
                await transactionalStorage.Shrink().ConfigureAwait(false);
            }
        }
    }

    protected override async ValueTask Interpret()
    {
        if (Enter(StateBagKey.Interpret))
        {
            return;
        }

        await base.Interpret().ConfigureAwait(false);

        Exit(StateBagKey.Interpret);
    }

    protected override async ValueTask MainEventLoop()
    {
        if (Enter(StateBagKey.MainEventLoop))
        {
            return;
        }

        await base.MainEventLoop().ConfigureAwait(false);

        Exit(StateBagKey.MainEventLoop);
    }

    protected override async ValueTask ExitSteps()
    {
        if (Enter(StateBagKey.ExitSteps))
        {
            return;
        }

        await base.ExitSteps().ConfigureAwait(false);

        Exit(StateBagKey.ExitSteps);
    }

    protected override async ValueTask InitializeDataModels()
    {
        if (Enter(StateBagKey.InitializeDataModels))
        {
            return;
        }

        await base.InitializeDataModels().ConfigureAwait(false);

        Exit(StateBagKey.InitializeDataModels);
    }

    protected override async ValueTask InitialEnterStates()
    {
        if (Enter(StateBagKey.InitialEnterStates))
        {
            return;
        }

        await base.InitialEnterStates().ConfigureAwait(false);

        Exit(StateBagKey.InitialEnterStates);
    }

    protected override async ValueTask<bool> MainEventLoopIteration()
    {
        EnterIteration(StateBagKey.MainEventLoopIteration);

        var result = await base.MainEventLoopIteration().ConfigureAwait(false);

        ExitIteration(StateBagKey.MainEventLoopIteration);

        return result;
    }

    protected override async ValueTask<bool> StartInvokeLoop()
    {
        if (Enter(StateBagKey.StartInvokeLoop, out bool result))
        {
            return result;
        }

        result = await base.StartInvokeLoop().ConfigureAwait(false);

        Exit(StateBagKey.StartInvokeLoop, result);

        return result;
    }

    protected override async ValueTask<bool> ExternalQueueProcess()
    {
        if (Enter(StateBagKey.ExternalQueueProcess, out bool result))
        {
            return result;
        }

        result = await base.ExternalQueueProcess().ConfigureAwait(false);

        Exit(StateBagKey.ExternalQueueProcess, result);

        await CheckPoint(PersistenceLevel.Event).ConfigureAwait(false);

        return result;
    }

    protected override async ValueTask<bool> Macrostep()
    {
        if (Enter(StateBagKey.Macrostep, out bool result))
        {
            return result;
        }

        result = await base.Macrostep().ConfigureAwait(false);

        Exit(StateBagKey.Macrostep, result);

        return result;
    }

    protected override async ValueTask<bool> MacrostepIteration()
    {
        EnterIteration(StateBagKey.MacrostepIteration);

        var result = await base.MacrostepIteration().ConfigureAwait(false);

        ExitIteration(StateBagKey.MacrostepIteration);

        return result;
    }

    protected override async ValueTask<bool> IsInternalQueueEmpty()
    {
        if (Enter(StateBagKey.IsInternalQueueEmpty, out bool result))
        {
            return result;
        }

        result = await base.IsInternalQueueEmpty().ConfigureAwait(false);

        Exit(StateBagKey.IsInternalQueueEmpty, result);

        return result;
    }

    protected override async ValueTask<bool> InternalQueueProcess()
    {
        if (Enter(StateBagKey.InternalQueueProcess, out bool result))
        {
            return result;
        }

        result = await base.InternalQueueProcess().ConfigureAwait(false);

        Exit(StateBagKey.InternalQueueProcess, result);

        return result;
    }

    protected override async ValueTask<List<TransitionNode>> SelectInternalEventTransitions()
    {
        if (Enter(StateBagKey.InternalQueueProcess, out List<TransitionNode>? result))
        {
            return result;
        }

        result = await base.SelectInternalEventTransitions().ConfigureAwait(false);

        Exit(StateBagKey.InternalQueueProcess, result);

        return result;
    }

    protected override async ValueTask<List<TransitionNode>> SelectTransitions(IIncomingEvent? incomingEvent)
    {
        if (Enter(StateBagKey.SelectTransitions, out List<TransitionNode>? result))
        {
            return result;
        }

        result = await base.SelectTransitions(incomingEvent).ConfigureAwait(false);

        Exit(StateBagKey.SelectTransitions, result);

        return result;
    }

    protected override async ValueTask<bool> Microstep(List<TransitionNode> enabledTransitions)
    {
        if (Enter(StateBagKey.Microstep, out bool result))
        {
            return result;
        }

        result = await base.Microstep(enabledTransitions).ConfigureAwait(false);

        await CheckPoint(PersistenceLevel.Transition).ConfigureAwait(false);

        Exit(StateBagKey.Microstep, result);

        return result;
    }

    protected override async ValueTask ExitStates(List<TransitionNode> enabledTransitions)
    {
        if (Enter(StateBagKey.ExitStates))
        {
            return;
        }

        await base.ExitStates(enabledTransitions).ConfigureAwait(false);

        Exit(StateBagKey.ExitStates);
    }

    protected override async ValueTask EnterStates(List<TransitionNode> enabledTransitions)
    {
        if (Enter(StateBagKey.EnterStates))
        {
            return;
        }

        await base.EnterStates(enabledTransitions).ConfigureAwait(false);

        Exit(StateBagKey.EnterStates);
    }

    protected override async ValueTask ExecuteGlobalScript()
    {
        if (Enter(StateBagKey.ExecuteGlobalScript))
        {
            return;
        }

        await base.ExecuteGlobalScript().ConfigureAwait(false);

        Exit(StateBagKey.ExecuteGlobalScript);
    }

    protected override async ValueTask ExitInterpreter()
    {
        if (Enter(StateBagKey.ExitInterpreter))
        {
            return;
        }

        await base.ExitInterpreter().ConfigureAwait(false);

        Exit(StateBagKey.ExitInterpreter);
    }

    protected override async ValueTask ExecuteTransitionContent(List<TransitionNode> transitions)
    {
        if (Enter(StateBagKey.ExecuteTransitionContent))
        {
            return;
        }

        await base.ExecuteTransitionContent(transitions).ConfigureAwait(false);

        Exit(StateBagKey.ExecuteTransitionContent);
    }

    protected override async ValueTask RunExecutableEntity(ImmutableArray<IExecEvaluator> action)
    {
        if (Enter(StateBagKey.RunExecutableEntity))
        {
            return;
        }

        await base.RunExecutableEntity(action).ConfigureAwait(false);

        await CheckPoint(PersistenceLevel.ExecutableAction).ConfigureAwait(false);

        Exit(StateBagKey.RunExecutableEntity);
    }

    protected override async ValueTask StartInvoke(InvokeId invokeId, InvokeNode invoke)
    {
        if (Enter(StateBagKey.Invoke))
        {
            return;
        }

        await base.StartInvoke(invokeId, invoke).ConfigureAwait(false);

        Exit(StateBagKey.Invoke);
    }

    protected override async ValueTask CancelInvoke(InvokeNode invoke)
    {
        if (Enter(StateBagKey.CancelInvoke))
        {
            return;
        }

        await base.CancelInvoke(invoke).ConfigureAwait(false);

        Exit(StateBagKey.CancelInvoke);
    }

    protected override async ValueTask InitializeDataModel(DataModelNode dataModel, DataModelList? defaultValues = default)
    {
        if (Enter(StateBagKey.InitializeDataModel))
        {
            return;
        }

        await base.InitializeDataModel(dataModel, defaultValues).ConfigureAwait(false);

        Exit(StateBagKey.InitializeDataModel);
    }

    protected override async ValueTask InitializeData(DataNode data, DataModelList? defaultValues)
    {
        if (Enter(StateBagKey.InitializeData))
        {
            return;
        }

        await base.InitializeData(data, defaultValues).ConfigureAwait(false);

        Exit(StateBagKey.InitializeData);
    }

    protected override async ValueTask<IIncomingEvent> WaitForExternalEvent()
    {
        await CheckPoint(PersistenceLevel.StableState).ConfigureAwait(false);

        return await base.WaitForExternalEvent().ConfigureAwait(false);
    }

    private enum MethodState
    {
        Executing,

        Completed
    }

    private enum StateBagKey
    {
        ExecuteGlobalScript,

        InitialEnterStates,

        ExitStates,

        EnterStates,

        Interpret,

        ExitSteps,

        InitializeDataModels,

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

        InitializeData
    }
    /*
    protected override async ValueTask DoOperation(StateBagKey key, Func<ValueTask> func)
    {
        if (_persistenceContext.GetState((int) key) == 0)
        {
            await func().ConfigureAwait(false);

            _persistenceContext.SetState((int) key, value: 1);
        }
    }

    protected override async ValueTask DoOperation<TArg>(StateBagKey key, Func<TArg, ValueTask> func, TArg arg)
    {
        if (_persistenceContext.GetState((int) key) == 0)
        {
            await func(arg).ConfigureAwait(false);

            _persistenceContext.SetState((int) key, value: 1);
        }
    }

    protected override async ValueTask DoOperation<TArg>(StateBagKey key,
                                                         IEntity entity,
                                                         Func<TArg, ValueTask> func,
                                                         TArg arg)
    {
        var documentId = entity.As<IDocumentId>().DocumentId;

        if (_persistenceContext.GetState((int) key, documentId) == 0)
        {
            await func(arg).ConfigureAwait(false);

            _persistenceContext.SetState((int) key, documentId, value: 1);
        }
    }

    protected override void Complete(StateBagKey key)
    {
        _persistenceContext.ClearState((int) key);
    }

    protected override bool Capture(StateBagKey key, bool value)
    {
        if (_persistenceContext.GetState((int) key) == 1)
        {
            return _persistenceContext.GetState((int) key, subKey: 0) == 1;
        }

        _persistenceContext.SetState((int) key, subKey: 0, value ? 1 : 0);
        _persistenceContext.SetState((int) key, value: 1);

        return result;
    }

    protected override async ValueTask<List<TransitionNode>> Capture(StateBagKey key, Func<ValueTask<List<TransitionNode>>> value)
    {
        if (_persistenceContext.GetState((int) key) == 0)
        {
            var list = await value().ConfigureAwait(false);
            _persistenceContext.SetState((int) key, subKey: 0, list.Count);

            for (var i = 0; i < list.Count; i++)
            {
                _persistenceContext.SetState((int) key, i + 1, list[i].As<IDocumentId>().DocumentId);
            }

            _persistenceContext.SetState((int) key, value: 1);

            return list;
        }

        var length = _persistenceContext.GetState((int) key, subKey: 0);
        var capturedSet = new List<TransitionNode>(length);

        for (var i = 0; i < length; i++)
        {
            var documentId = _persistenceContext.GetState((int) key, i + 1);
            capturedSet.Add(_interpreterModel.EntityMap[documentId].As<TransitionNode>());
        }

        return capturedSet;
    }

    protected override async ValueTask CheckPoint(PersistenceLevel level)
    {
        if (_persistenceContext is not { } persistenceContext || _options.options.PersistenceLevel < level)
        {
            return;
        }

        await persistenceContext.CheckPoint((int) level, CancellationToken.None).ConfigureAwait(false);

        if (level == PersistenceLevel.StableState)
        {
            await persistenceContext.Shrink(CancellationToken.None).ConfigureAwait(false);
        }
    }

    protected override bool _running
    {
        get => base._running && _persistenceContext.GetState((int) StateBagKey.Stop) == 0;
        set
        {
            base._running = value;

            _persistenceContext.SetState((int) StateBagKey.Stop, value ? 0 : 1);
        }
    }

    protected override async ValueTask ExitSteps()
    {
        try
        {
            await base.ExitSteps().ConfigureAwait(false);
        }
        finally
        {
            await _options.options.StorageProvider.RemoveTransactionalStorage(partition: default, StateStorageKey, CancellationToken.None).ConfigureAwait(false);
            await _options.options.StorageProvider.RemoveTransactionalStorage(partition: default, StateMachineDefinitionStorageKey, CancellationToken.None).ConfigureAwait(false);
        }
    }*/
}