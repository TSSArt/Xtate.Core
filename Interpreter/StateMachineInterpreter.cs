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

using System.Buffers;
using Xtate.DataModel;

namespace Xtate.Core;

using DefaultHistoryContent = Dictionary<IIdentifier, ImmutableArray<IExecEvaluator>>;

public class StateMachineInterpreter : IStateMachineInterpreter
{
    private const int PlatformErrorEventId = 1;

    private const int ExecutionErrorEventId = 2;

    private const int CommunicationErrorEventId = 3;

    private const int InterpreterStateEventId = 4;

    private const int EventProcessingEventId = 5;

    private const int EnteringStateEventId = 6;

    private const int EnteredStateEventId = 7;

    private const int ExitingStateEventId = 8;

    private const int ExitedStateEventId = 9;

    private const int ExecutingTransitionEventId = 10;

    private const int ExecutedTransitionEventId = 11;

    private bool _running = true;

    private StateMachineDestroyedException? _stateMachineDestroyedException;

    public required StateMachineRuntimeError StateMachineRuntimeError { private get; [UsedImplicitly] init; }

    public required IStateMachineArguments StateMachineArguments { private get; [UsedImplicitly] init; }

    public required DataConverter DataConverter { private get; [UsedImplicitly] init; }

    public required ICaseSensitivity CaseSensitivity { private get; [UsedImplicitly] init; }

    public required IEventQueueReader EventQueueReader { private get; [UsedImplicitly] init; }

    public required ILogger<IStateMachineInterpreter> Logger { private get; [UsedImplicitly] init; }

    public required IInterpreterModel Model { private get; [UsedImplicitly] init; }

    public required INotifyStateChanged NotifyStateChanged { private get; [UsedImplicitly] init; }

    public required IUnhandledErrorBehaviour UnhandledErrorBehaviour { private get; [UsedImplicitly] init; }

    public required IStateMachineContext StateMachineContext { private get; [UsedImplicitly] init; }

    public required IInvokeController InvokeController { private get; [UsedImplicitly] init; }

#region Interface IStateMachineInterpreter

    public virtual async ValueTask<DataModelValue> RunAsync()
    {
        await Interpret().ConfigureAwait(false);

        ProcessRemainingInternalQueue();

        return StateMachineContext.DoneData;
    }

#endregion

    private void ProcessRemainingInternalQueue()
    {
        var internalQueue = StateMachineContext.InternalQueue;

        while (internalQueue.Count > 0)
        {
            var internalEvent = internalQueue.Dequeue();

            if (internalEvent.Name.IsError())
            {
                ProcessUnhandledError(internalEvent);

                ThrowIfDestroying();
            }
        }
    }

    protected virtual async ValueTask NotifyInterpreterState(StateMachineInterpreterState state)
    {
        await Logger.Write(Level.Trace, InterpreterStateEventId, $@"Interpreter state has changed to '{state}'").ConfigureAwait(false);

        await NotifyStateChanged.OnChanged(state).ConfigureAwait(false);
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
            await NotifyInterpreterState(StateMachineInterpreterState.Destroying).ConfigureAwait(false);
            await ExitSteps().ConfigureAwait(false);

            throw;
        }
        catch (Exception ex)
        {
            await HandleMainLoopException(ex).ConfigureAwait(false);

            throw;
        }

        await ExitSteps().ConfigureAwait(false);
    }

    protected virtual ValueTask HandleMainLoopException(Exception ex) => NotifyInterpreterState(StateMachineInterpreterState.Terminated);

    protected virtual async ValueTask EnterSteps()
    {
        await NotifyInterpreterState(StateMachineInterpreterState.Accepted).ConfigureAwait(false);
        await InitializeDataModels().ConfigureAwait(false);
        await ExecuteGlobalScript().ConfigureAwait(false);
        await NotifyInterpreterState(StateMachineInterpreterState.Started).ConfigureAwait(false);
        await InitialEnterStates().ConfigureAwait(false);
    }

    protected virtual async ValueTask ExitSteps()
    {
        await ExitInterpreter().ConfigureAwait(false);
        await NotifyInterpreterState(StateMachineInterpreterState.Completed).ConfigureAwait(false);
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
            await InitializeDataModel(dataModel, StateMachineArguments.Arguments.AsListOrDefault()).ConfigureAwait(false);
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
        foreach (var state in StateMachineContext.StatesToInvoke.ToSortedList(StateEntityNode.EntryOrder))
        {
            foreach (var invoke in state.Invoke)
            {
                var invokeId = InvokeId.New(state.Id, invoke.Id);

                await StartInvoke(invokeId, invoke).ConfigureAwait(false);
            }
        }

        StateMachineContext.StatesToInvoke.Clear();

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
            if (liveLockDetector.IsLiveLockDetected(StateMachineContext.InternalQueue.Count))
            {
                throw new StateMachineDestroyedException(Resources.Exception_LivelockDetected);
            }
        }

        return _running;
    }

    protected virtual async ValueTask<bool> MacrostepIteration()
    {
        if (await SelectTransitions(incomingEvent: default).ConfigureAwait(false) is { Count: > 0 } transitions)
        {
            return await Microstep(transitions).ConfigureAwait(false);
        }

        return await InternalQueueProcess().ConfigureAwait(false);
    }

    protected virtual ValueTask<bool> IsInternalQueueEmpty() => new(StateMachineContext.InternalQueue.Count == 0);

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

    protected virtual async ValueTask<List<TransitionNode>> SelectInternalEventTransitions()
    {
        var internalEvent = StateMachineContext.InternalQueue.Dequeue();

        var eventModel = DataConverter.FromEvent(internalEvent);
        StateMachineContext.DataModel.SetInternal(key: @"_event", CaseSensitivity.CaseInsensitive, eventModel, DataModelAccess.ReadOnly);

        var eventType = internalEvent.Type;
        var eventName = internalEvent.Name;
        await Logger.Write(Level.Trace, EventProcessingEventId, $@"Processing {eventType} event '{eventName}'", internalEvent).ConfigureAwait(false);

        var transitions = await SelectTransitions(internalEvent).ConfigureAwait(false);

        if (transitions.Count == 0 && internalEvent.Name.IsError())
        {
            ProcessUnhandledError(internalEvent);
        }

        return transitions;
    }

    private void ProcessUnhandledError(IIncomingEvent incomingEvent)
    {
        switch (UnhandledErrorBehaviour.Behaviour)
        {
            case Xtate.UnhandledErrorBehaviour.IgnoreError:
                break;

            case Xtate.UnhandledErrorBehaviour.DestroyStateMachine:
                TriggerDestroySignal(GetUnhandledErrorException());

                break;

            case Xtate.UnhandledErrorBehaviour.TerminateStateMachine:
                throw GetUnhandledErrorException();

            default:
                throw Infra.Unmatched(UnhandledErrorBehaviour.Behaviour);
        }

        StateMachineUnhandledErrorException GetUnhandledErrorException()
        {
            incomingEvent.Is<Exception>(out var exception);

            return new StateMachineUnhandledErrorException(Resources.Exception_UnhandledException, exception);
        }
    }

    protected virtual async ValueTask<List<TransitionNode>> SelectTransitions(IIncomingEvent? incomingEvent)
    {
        var transitions = new List<TransitionNode>();

        foreach (var state in StateMachineContext.Configuration.ToFilteredSortedList(s => s.IsAtomicState, StateEntityNode.EntryOrder))
        {
            await FindTransitionForState(transitions, state, incomingEvent).ConfigureAwait(false);
        }

        return RemoveConflictingTransitions(transitions);
    }

    protected virtual async ValueTask<List<TransitionNode>> ExternalEventTransitions()
    {
        var externalEvent = await ReadExternalEventFiltered().ConfigureAwait(false);

        var eventModel = DataConverter.FromEvent(externalEvent);
        var eventType = externalEvent.Type;
        var eventName = externalEvent.Name;
        StateMachineContext.DataModel.SetInternal(key: @"_event", CaseSensitivity.CaseInsensitive, eventModel, DataModelAccess.ReadOnly);

        await Logger.Write(Level.Trace, EventProcessingEventId, $@"Processing {eventType} event '{eventName}'", externalEvent).ConfigureAwait(false);

        foreach (var state in StateMachineContext.Configuration)
        {
            foreach (var invoke in state.Invoke)
            {
                if (invoke.CurrentInvokeId == externalEvent.InvokeId)
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

    private async ValueTask<IIncomingEvent> ReadExternalEventFiltered()
    {
        while (true)
        {
            var incomingEvent = await ReadExternalEvent().ConfigureAwait(false);

            if (incomingEvent.InvokeId is null)
            {
                return incomingEvent;
            }

            if (IsInvokeActive(incomingEvent.InvokeId))
            {
                return incomingEvent;
            }
        }
    }

    private bool IsInvokeActive(InvokeId invokeId) => StateMachineContext.ActiveInvokes.Contains(invokeId);

    protected virtual async ValueTask<IIncomingEvent> ReadExternalEvent()
    {
        ThrowIfDestroying();

        if (EventQueueReader.TryReadEvent(out var incomingEvent))
        {
            return incomingEvent;
        }

        return await WaitForExternalEvent().ConfigureAwait(false);
    }

    protected virtual async ValueTask<IIncomingEvent> WaitForExternalEvent()
    {
        await NotifyInterpreterState(StateMachineInterpreterState.Waiting).ConfigureAwait(false);

        while (await EventQueueReader.WaitToEvent().ConfigureAwait(false))
        {
            if (EventQueueReader.TryReadEvent(out var incomingEvent))
            {
                return incomingEvent;
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
        var statesToExit = StateMachineContext.Configuration.ToSortedList(StateEntityNode.ExitOrder);

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

            StateMachineContext.Configuration.Delete(state);

            if (state is FinalNode { Parent: StateMachineNode } final)
            {
                await EvaluateDoneData(final).ConfigureAwait(false);
            }
        }
    }

    private async ValueTask FindTransitionForState(List<TransitionNode> transitionNodes, StateEntityNode state, IIncomingEvent? incomingEvent)
    {
        foreach (var transition in state.Transitions)
        {
            if (EventMatch(transition.EventDescriptors, incomingEvent) && await ConditionMatch(transition).ConfigureAwait(false))
            {
                transitionNodes.Add(transition);

                return;
            }
        }

        if (state.Parent is not StateMachineNode)
        {
            await FindTransitionForState(transitionNodes, state.Parent!, incomingEvent).ConfigureAwait(false);
        }
    }

    private static bool EventMatch(EventDescriptors eventDescriptors, IIncomingEvent? incomingEvent)
    {
        if (incomingEvent is null)
        {
            return eventDescriptors.IsDefault;
        }

        if (eventDescriptors.IsDefault)
        {
            return false;
        }

        foreach (var eventDescriptor in eventDescriptors)
        {
            if (eventDescriptor.IsEventMatch(incomingEvent))
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

        try
        {
            return await condition.EvaluateBoolean().ConfigureAwait(false);
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
                (tr1 ??= [default!])[0] = t1;
                (tr2 ??= [default!])[0] = t2;

                if (HasIntersection(ComputeExitSet(tr1), ComputeExitSet(tr2)))
                {
                    if (IsDescendant(t1.Source, t2.Source))
                    {
                        (transitionsToRemove ??= []).Add(t2);
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
            StateMachineContext.StatesToInvoke.Delete(state);
        }

        var states = ToSortedList(statesToExit, StateEntityNode.ExitOrder);

        foreach (var state in states)
        {
            foreach (var history in state.HistoryStates)
            {
                static bool Deep(StateEntityNode node, StateEntityNode state) => node.IsAtomicState && IsDescendant(node, state);

                static bool Shallow(StateEntityNode node, StateEntityNode state) => node.Parent == state;

                var list = history.Type == HistoryType.Deep
                    ? StateMachineContext.Configuration.ToFilteredList(Deep, state)
                    : StateMachineContext.Configuration.ToFilteredList(Shallow, state);

                StateMachineContext.HistoryValue.Set(history.Id, list);
            }
        }

        foreach (var state in states)
        {
            var stateId = state.Id;
            await Logger.Write(Level.Trace, ExitingStateEventId, $@"Exiting state [{stateId}]", state).ConfigureAwait(false);

            foreach (var onExit in state.OnExit)
            {
                await RunExecutableEntity(onExit.ActionEvaluators).ConfigureAwait(false);
            }

            foreach (var invoke in state.Invoke)
            {
                await CancelInvoke(invoke).ConfigureAwait(false);
            }

            StateMachineContext.Configuration.Delete(state);

            await Logger.Write(Level.Trace, ExitedStateEventId, $@"Exited state [{stateId}]", state).ConfigureAwait(false);
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
            var stateId = state.Id;
            await Logger.Write(Level.Trace, EnteringStateEventId, $@"Entering state [{stateId}]", state).ConfigureAwait(false);

            StateMachineContext.Configuration.AddIfNotExists(state);
            StateMachineContext.StatesToInvoke.AddIfNotExists(state);

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

            if (defaultHistoryContent.TryGetValue(stateId, out var action))
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

                    StateMachineContext.InternalQueue.Enqueue(new IncomingEvent { Type = EventType.Internal, Name = EventName.GetDoneStateName(parent.Id), Data = doneData });

                    if (grandparent is ParallelNode)
                    {
                        if (grandparent.States.All(IsInFinalState))
                        {
                            StateMachineContext.InternalQueue.Enqueue(new IncomingEvent { Type = EventType.Internal, Name = EventName.GetDoneStateName(grandparent.Id) });
                        }
                    }
                }
            }

            await Logger.Write(Level.Trace, EnteredStateEventId, $@"Entered state [{stateId}]", state).ConfigureAwait(false);
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

            return state.States.Any(Predicate, StateMachineContext.Configuration);
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
            if (!transition.Target.IsDefault)
            {
                var domain = GetTransitionDomain(transition);

                foreach (var state in StateMachineContext.Configuration)
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
            if (StateMachineContext.HistoryValue.TryGetValue(history.Id, out var states))
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
                if (!StateMachineContext.HistoryValue.TryGetValue(history.Id, out var values))
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
        foreach (var (node, type, target, @event) in transitions)
        {
            var traceEnabled = Logger.IsEnabled(Level.Trace);

            if (traceEnabled)
            {
                if (@event.IsDefault)
                {
                    await Logger.Write(Level.Trace, ExecutingTransitionEventId, $@"Executing eventless {type} transition to '{target}'", node).ConfigureAwait(false);
                }
                else
                {
                    await Logger.Write(Level.Trace, ExecutingTransitionEventId, $@"Executing {type} transition to '{target}'. Event descriptor '{@event}'", node).ConfigureAwait(false);
                }
            }

            await RunExecutableEntity(node.ActionEvaluators).ConfigureAwait(false);

            if (traceEnabled)
            {
                if (@event.IsDefault)
                {
                    await Logger.Write(Level.Trace, ExecutedTransitionEventId, $@"Executed eventless {type} transition to '{target}'", node).ConfigureAwait(false);
                }
                else
                {
                    await Logger.Write(Level.Trace, ExecutedTransitionEventId, $@"Executed {type} transition to '{target}'. Event descriptor '{@event}'", node).ConfigureAwait(false);
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

    private bool IsError(Exception _) => true; // TODO: Is not OperationCanceled or ObjectDisposed when SM terminated?

    private async ValueTask Error(object source, Exception exception, bool logLoggerErrors = true)
    {
        SendId? sendId = default;

        var errorType = StateMachineRuntimeError.IsPlatformError(exception)
            ? ErrorType.Platform
            : StateMachineRuntimeError.IsCommunicationError(exception, out sendId)
                ? ErrorType.Communication
                : ErrorType.Execution;

        var name = errorType switch
                   {
                       ErrorType.Execution     => EventName.ErrorExecution,
                       ErrorType.Communication => EventName.ErrorCommunication,
                       ErrorType.Platform      => EventName.ErrorPlatform,
                       _                       => throw Infra.Unmatched(errorType)
                   };

        var incomingEvent = new IncomingEvent
                            {
                                Type = EventType.Platform,
                                Name = name,
                                Data = DataConverter.FromException(exception),
                                SendId = sendId,
                                Ancestor = exception
                            };

        StateMachineContext.InternalQueue.Enqueue(incomingEvent);

        if (Logger.IsEnabled(Level.Error))
        {
            try
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

    private async ValueTask LogError(ErrorType errorType, object source, Exception exception)
    {
        try
        {
            var entityId = source.Is(out IDebugEntityId? id) ? id.EntityId : default;

            var eventId = errorType switch
                          {
                              ErrorType.Platform      => PlatformErrorEventId,
                              ErrorType.Execution     => ExecutionErrorEventId,
                              ErrorType.Communication => CommunicationErrorEventId,
                              _                       => throw Infra.Unmatched(errorType)
                          };

            await Logger.Write(Level.Error, eventId, $@"{errorType} error in entity [{entityId}].", exception).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw StateMachineRuntimeError.PlatformError(ex);
        }
    }

    protected virtual async ValueTask ExecuteGlobalScript()
    {
        if (Model.Root.ScriptEvaluator is { } scriptEvaluator)
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
    }

    private async ValueTask EvaluateDoneData(FinalNode final)
    {
        if (final.DoneData is not null)
        {
            StateMachineContext.DoneData = await EvaluateDoneData(final.DoneData).ConfigureAwait(false);
        }
    }

    private async ValueTask ForwardEvent(InvokeNode invoke, IIncomingEvent incomingEvent)
    {
        try
        {
            var invokeId = invoke.CurrentInvokeId;

            Infra.NotNull(invokeId);

            await InvokeController.Forward(invokeId, incomingEvent).ConfigureAwait(false);
        }
        catch (Exception ex) when (IsError(ex))
        {
            await Error(invoke, ex).ConfigureAwait(false);
        }
    }

    private ValueTask ApplyFinalize(InvokeNode invoke) => invoke.Finalize is not null ? RunExecutableEntity(invoke.Finalize.ActionEvaluators) : default;

    protected virtual async ValueTask StartInvoke(InvokeId invokeId, InvokeNode invoke)
    {
        try
        {
            Infra.Assert(invoke.CurrentInvokeId is null);

            var invokeData = await invoke.CreateInvokeData(invokeId).ConfigureAwait(false);

            invoke.CurrentInvokeId = invokeId;

            StateMachineContext.ActiveInvokes.Add(invokeId);

            await InvokeController.Start(invokeData).ConfigureAwait(false);
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
            var tmpInvokeId = invoke.CurrentInvokeId;
            Infra.NotNull(tmpInvokeId);

            StateMachineContext.ActiveInvokes.Remove(tmpInvokeId);

            invoke.CurrentInvokeId = null;

            await InvokeController.Cancel(tmpInvokeId).ConfigureAwait(false);
        }
        catch (Exception ex) when (IsError(ex))
        {
            await Error(invoke, ex).ConfigureAwait(false);
        }
    }

    protected virtual async ValueTask InitializeDataModel(DataModelNode dataModel, DataModelList? defaultValues = default)
    {
        foreach (var node in dataModel.Data)
        {
            await InitializeData(node, defaultValues).ConfigureAwait(false);
        }
    }

    protected virtual async ValueTask InitializeData(DataNode data, DataModelList? defaultValues)
    {
        Infra.Requires(data);

        var id = data.Id;
        Infra.NotNull(id);

        if (defaultValues?[id, CaseSensitivity.CaseInsensitive] is not { Type: not DataModelValueType.Undefined } value)
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

        StateMachineContext.DataModel[id] = value;
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

    //TODO:
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

        private int _index;

        private int _queueLength;

        private int _sum;

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
        }
    }
}