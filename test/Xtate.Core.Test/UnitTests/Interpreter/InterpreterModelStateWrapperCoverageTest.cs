// Copyright © 2019-2026 Sergii Artemenko
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

using System.Globalization;
using Xtate.Ancestor;
using Xtate.DataModel;
using Xtate.Interpreter.Model;
using Xtate.StateMachine;
using Xtate.StateMachine.Internal;

namespace Xtate.Test.UnitTests.Interpreter;

[TestClass]
public class InterpreterModelStateWrapperCoverageTest
{
	[TestMethod]
	public async Task TransitionNodeForwardsEvaluatorsMapsTargetsAndDeconstructs()
	{
		var targetId = Identifier.FromString("target");
		var missingId = Identifier.FromString("missing");
		var action = new ActionSource();
		var condition = new ConditionSource { Expression = "true" };
		var source = new TransitionSource
					 {
						 EventDescriptors = ImmutableArray.Create<IEventDescriptor>(EventDescriptor.FromString("event")),
						 Condition = condition,
						 Target = ImmutableArray.Create<IIdentifier>(targetId, missingId),
						 Type = TransitionType.External,
						 Action = ImmutableArray.Create<IExecutableEntity>(action)
					 };
		var ids = new LinkedList<int>();
		var node = new TransitionNode(new DocumentIdNode(ids), source);
		ids.First!.Value = 21;
		source.Ancestor = node;
		var state = new TestStateNode(targetId);
		var missingState = new TestStateNode(missingId);

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreEqual(expected: 21, node.DocumentId);
		Assert.AreEqual("(#21)", node.EntityId.ToString(CultureInfo.InvariantCulture));
		Assert.AreEqual(TransitionType.External, node.Type);
		Assert.AreEqual("event", node.EventDescriptors[0].Value);
		Assert.AreSame(condition, node.Condition);
		Assert.AreEqual(expected: 2, node.Target.Count);
		Assert.AreSame(action, node.Action[0]);
		Assert.AreSame(action, node.ActionEvaluators[0]);
		Assert.AreSame(condition, node.ConditionEvaluator);
		await node.ActionEvaluators[0].Execute();
		Assert.AreEqual(expected: 1, action.ExecuteCount);
		Assert.IsTrue(await node.ConditionEvaluator!.EvaluateBoolean());

		Assert.IsFalse(node.TryMapTarget(new Dictionary<IIdentifier, StateEntityNode> { [targetId] = state }));
		Assert.IsTrue(node.TryMapTarget(new Dictionary<IIdentifier, StateEntityNode> { [targetId] = state, [missingId] = missingState }));
		CollectionAssert.AreEqual(new StateEntityNode[] { state, missingState }, node.TargetState.ToArray());
		node.SetSource(state);
		Assert.AreSame(state, node.Source);

		node.Deconstruct(out var self, out var type, out var target, out var descriptors);
		Assert.AreSame(node, self);
		Assert.AreEqual(TransitionType.External, type);
		Assert.AreEqual(expected: 2, target.Count);
		Assert.AreEqual("event", descriptors[0].Value);
	}

	[TestMethod]
	public void InitialNodeConnectsTransitionAndForwardsInitialEntity()
	{
		var transitionSource = CreateEmptyTransitionSource();
		var transition = new TransitionNode(new DocumentIdNode(list: null), transitionSource);
		transitionSource.Ancestor = transition;
		var source = new InitialSource { Transition = transitionSource };
		var ids = new LinkedList<int>();
		var node = new InitialNode(new DocumentIdNode(ids), source);
		ids.First!.Value = 22;

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreSame(transition, node.Transition);
		Assert.AreSame(node, transition.Source);
		Assert.AreSame(transitionSource, ((IInitial) node).Transition);
		Assert.AreSame(transitionSource, typeof(IInitial).GetProperty(nameof(IInitial.Transition))!.GetValue(node));
		Assert.AreEqual("(#22)", node.EntityId.ToString(CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void SyntheticInitialNodeRejectsSourceTransitionAccess()
	{
		var transitionSource = CreateEmptyTransitionSource();
		var transition = new TransitionNode(new DocumentIdNode(list: null), transitionSource);
		transitionSource.Ancestor = transition;
		IInitial initial = new TestInitialNode(new DocumentIdNode(list: null), transition);

		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage] () => _ = initial.Transition);
	}

	[TestMethod]
	public void HistoryNodeForwardsIdentityTypeTransitionAndDebugId()
	{
		var transitionSource = CreateEmptyTransitionSource();
		var transition = new TransitionNode(new DocumentIdNode(list: null), transitionSource);
		transitionSource.Ancestor = transition;
		var id = Identifier.FromString("history");
		var source = new HistorySource { Id = id, Type = HistoryType.Deep, Transition = transitionSource };
		var ids = new LinkedList<int>();
		var node = new HistoryNode(new DocumentIdNode(ids), source);
		ids.First!.Value = 23;

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreSame(id, node.Id);
		Assert.AreEqual(HistoryType.Deep, node.Type);
		Assert.AreSame(transition, node.Transition);
		Assert.AreSame(node, transition.Source);
		Assert.AreSame(transitionSource, ((IHistory) node).Transition);
		Assert.AreEqual("history(#23)", ((IDebugEntityId) node).EntityId.ToString(CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void FinalNodeForwardsIdentityAndExposesAtomicEmptyCollections()
	{
		var id = Identifier.FromString("final");
		var source = new FinalSource
					 {
						 Id = id,
						 OnEntry = [],
						 OnExit = [],
						 DoneData = null
					 };
		var ids = new LinkedList<int>();
		var node = new FinalNode(new DocumentIdNode(ids), source);
		ids.First!.Value = 24;
		var final = (IFinal) node;

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreSame(id, node.Id);
		Assert.IsTrue(node.IsAtomicState);
		Assert.IsEmpty(node.Transitions);
		Assert.IsEmpty(node.HistoryStates);
		Assert.IsEmpty(node.Invoke);
		Assert.IsEmpty(node.OnEntry);
		Assert.IsEmpty(node.OnExit);
		Assert.IsNull(node.DoneData);
		Assert.IsEmpty(final.OnEntry);
		Assert.IsEmpty(final.OnExit);
		Assert.IsNull(final.DoneData);
		Assert.AreEqual("final($24)", ((IDebugEntityId) node).EntityId.ToString(CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void StateNodeForwardsIdentityEmptyCollectionsAndAtomicSemantics()
	{
		var id = Identifier.FromString("atomic");
		var source = new StateSource { Id = id };
		var ids = new LinkedList<int>();
		var node = new StateNode(new DocumentIdNode(ids), source);
		ids.First!.Value = 25;
		var state = (IState) node;

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreSame(id, node.Id);
		Assert.IsTrue(node.IsAtomicState);
		Assert.IsNull(node.DataModel);
		Assert.IsNull(state.Initial);
		Assert.IsNull(state.DataModel);
		Assert.IsEmpty(state.States);
		Assert.IsEmpty(state.HistoryStates);
		Assert.IsEmpty(state.Transitions);
		Assert.IsEmpty(state.Invoke);
		Assert.IsEmpty(state.OnEntry);
		Assert.IsEmpty(state.OnExit);
		Assert.AreEqual("atomic(#25)", ((IDebugEntityId) node).EntityId.ToString(CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void CompoundNodeRequiresAndRegistersInitialAndChildState()
	{
		var transitionSource = CreateEmptyTransitionSource();
		var transition = new TransitionNode(new DocumentIdNode(list: null), transitionSource);
		transitionSource.Ancestor = transition;
		var initialSource = new InitialSource { Transition = transitionSource };
		var initial = new InitialNode(new DocumentIdNode(list: null), initialSource);
		initialSource.Ancestor = initial;
		var childSource = new StateSource { Id = Identifier.FromString("child") };
		var child = new StateNode(new DocumentIdNode(list: null), childSource);
		childSource.Ancestor = child;
		var source = new StateSource
					 {
						 Id = Identifier.FromString("compound"),
						 Initial = initialSource,
						 States = ImmutableArray.Create<IStateEntity>(childSource)
					 };
		var ids = new LinkedList<int>();
		var node = new CompoundNode(new DocumentIdNode(ids), source);
		ids.First!.Value = 26;

		Assert.IsFalse(node.IsAtomicState);
		Assert.AreSame(initial, node.Initial);
		Assert.AreSame(node, initial.Parent);
		Assert.AreSame(node, child.Parent);
		Assert.AreSame(child, node.States[0]);
		Assert.AreSame(initial, ((IState) node).Initial);
		Assert.AreEqual("compound(#26)", ((IDebugEntityId) node).EntityId.ToString(CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void ParallelNodeForwardsCollectionsAndRegistersChildState()
	{
		var childSource = new StateSource { Id = Identifier.FromString("region") };
		var child = new StateNode(new DocumentIdNode(list: null), childSource);
		childSource.Ancestor = child;
		var id = Identifier.FromString("parallel");
		var source = new ParallelSource
					 {
						 Id = id,
						 States = ImmutableArray.Create<IStateEntity>(childSource)
					 };
		var ids = new LinkedList<int>();
		var node = new ParallelNode(new DocumentIdNode(ids), source);
		ids.First!.Value = 27;
		var parallel = (IParallel) node;

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreSame(id, node.Id);
		Assert.IsFalse(node.IsAtomicState);
		Assert.AreSame(node, child.Parent);
		Assert.AreSame(child, node.States[0]);
		Assert.AreSame(child, parallel.States[0]);
		Assert.IsNull(parallel.DataModel);
		Assert.IsEmpty(parallel.HistoryStates);
		Assert.IsEmpty(parallel.Transitions);
		Assert.IsEmpty(parallel.Invoke);
		Assert.IsEmpty(parallel.OnEntry);
		Assert.IsEmpty(parallel.OnExit);
		Assert.AreEqual("parallel(#27)", ((IDebugEntityId) node).EntityId.ToString(CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void StateMachineNodeForwardsConfigurationAndRegistersRootNodes()
	{
		var transitionSource = CreateEmptyTransitionSource();
		var transition = new TransitionNode(new DocumentIdNode(list: null), transitionSource);
		transitionSource.Ancestor = transition;
		var initialSource = new InitialSource { Transition = transitionSource };
		var initial = new InitialNode(new DocumentIdNode(list: null), initialSource);
		initialSource.Ancestor = initial;
		var childSource = new StateSource { Id = Identifier.FromString("root") };
		var child = new StateNode(new DocumentIdNode(list: null), childSource);
		childSource.Ancestor = child;
		var source = new StateMachineSource
					 {
						 Name = "machine",
						 DataModelType = "null",
						 Binding = BindingType.Late,
						 Initial = initialSource,
						 States = ImmutableArray.Create<IStateEntity>(childSource)
					 };
		var ids = new LinkedList<int>();
		var node = new StateMachineNode(new DocumentIdNode(ids), source);
		ids.First!.Value = 28;
		var machine = (IStateMachine) node;

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreEqual("machine", node.Name);
		Assert.AreEqual("null", node.DataModelType);
		Assert.AreEqual(BindingType.Late, node.Binding);
		Assert.IsNull(node.Script);
		Assert.IsNull(node.ScriptEvaluator);
		Assert.IsNull(node.DataModel);
		Assert.IsNull(typeof(IStateMachine).GetProperty(nameof(IStateMachine.DataModel))!.GetValue(node));
		Assert.AreSame(initial, node.Initial);
		Assert.AreSame(node, initial.Parent);
		Assert.AreSame(node, child.Parent);
		Assert.AreSame(child, node.States[0]);
		Assert.AreSame(initial, machine.Initial);
		Assert.AreSame(child, machine.States[0]);
		Assert.AreEqual("machine(#28)", ((IDebugEntityId) node).EntityId.ToString(CultureInfo.InvariantCulture));
	}

	private static TransitionSource CreateEmptyTransitionSource() =>
		new()
		{
			EventDescriptors = ImmutableArray<IEventDescriptor>.Empty,
			Target = ImmutableArray<IIdentifier>.Empty,
			Type = TransitionType.Internal,
			Action = ImmutableArray<IExecutableEntity>.Empty
		};

	private sealed class TransitionSource : ITransition, IAncestorProvider
	{
		public object? Ancestor { get; set; }

		public EventDescriptors EventDescriptors { get; init; }

		public IConditionExpression? Condition { get; init; }

		public Target Target { get; init; }

		public TransitionType Type { get; init; }

		public ImmutableArray<IExecutableEntity> Action { get; init; }
	}

	private sealed class ActionSource : IExecutableEntity, IExecEvaluator
	{
		public int ExecuteCount { get; private set; }

		public ValueTask Execute()
		{
			ExecuteCount ++;
			return ValueTask.CompletedTask;
		}
	}

	private sealed class ConditionSource : IConditionExpression, IBooleanEvaluator
	{
		public string? Expression { get; init; }

		public ValueTask<bool> EvaluateBoolean() => new(true);
	}

	private sealed class InitialSource : IInitial, IAncestorProvider
	{
		public object? Ancestor { get; set; }

		public ITransition? Transition { get; init; }
	}

	private sealed class HistorySource : IHistory
	{
		public IIdentifier? Id { get; init; }

		public HistoryType Type { get; init; }

		public ITransition? Transition { get; init; }
	}

	private sealed class FinalSource : IFinal
	{
		public IIdentifier? Id { get; init; }

		public ImmutableArray<IOnEntry> OnEntry { get; init; }

		public ImmutableArray<IOnExit> OnExit { get; init; }

		public IDoneData? DoneData { get; init; }
	}

	private sealed class StateSource : IState, IAncestorProvider
	{
		public object? Ancestor { get; set; }

		public IIdentifier? Id { get; init; }

		public IInitial? Initial { get; init; }

		public ImmutableArray<IStateEntity> States { get; init; } = [];

		public ImmutableArray<IHistory> HistoryStates { get; init; } = [];

		public ImmutableArray<ITransition> Transitions { get; init; } = [];

		public IDataModel? DataModel { get; init; }

		public ImmutableArray<IOnEntry> OnEntry { get; init; } = [];

		public ImmutableArray<IOnExit> OnExit { get; init; } = [];

		public ImmutableArray<IInvoke> Invoke { get; init; } = [];
	}

	private sealed class ParallelSource : IParallel
	{
		public IIdentifier? Id { get; init; }

		public ImmutableArray<IStateEntity> States { get; init; } = [];

		public ImmutableArray<IHistory> HistoryStates { get; init; } = [];

		public ImmutableArray<ITransition> Transitions { get; init; } = [];

		public IDataModel? DataModel { get; init; }

		public ImmutableArray<IOnEntry> OnEntry { get; init; } = [];

		public ImmutableArray<IOnExit> OnExit { get; init; } = [];

		public ImmutableArray<IInvoke> Invoke { get; init; } = [];
	}

	private sealed class StateMachineSource : IStateMachine
	{
		public string? Name { get; init; }

		public string? DataModelType { get; init; }

		public BindingType Binding { get; init; }

		public IInitial? Initial { get; init; }

		public ImmutableArray<IStateEntity> States { get; init; } = [];

		public IDataModel? DataModel { get; init; }

		public IExecutableEntity? Script { get; init; }
	}

	private sealed class TestStateNode(IIdentifier id) : StateEntityNode(new DocumentIdNode(list: null))
	{
		[ExcludeFromCodeCoverage]
		public override IIdentifier Id => id;
	}

	private sealed class TestInitialNode(DocumentIdNode documentIdNode, TransitionNode transition) : InitialNode(documentIdNode, transition);
}
