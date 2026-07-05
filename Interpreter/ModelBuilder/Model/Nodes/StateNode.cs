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

using Xtate.Ancestor;
using Xtate.Ancestor.Extensions;
using Xtate.StateMachine;
using Xtate.StateMachine.Internal;

namespace Xtate.Interpreter.Model;

[InstantiatedByIoC]
public class StateNode : StateEntityNode, IState, IAncestorProvider, IDebugEntityId
{
	private readonly IState _state;

	public StateNode(DocumentIdNode documentIdNode, IState state) : base(documentIdNode)
	{
		_state = state;

		var initial = state.Initial?.UseAncestor.As<InitialNode>();
		var states = state.States.UseAncestor.ItemsAs<StateEntityNode>(true);
		var historyStates = state.HistoryStates.UseAncestor.ItemsAs<HistoryNode>(true);
		var transitions = state.Transitions.UseAncestor.ItemsAs<TransitionNode>(true);
		var invokeList = state.Invoke.UseAncestor.ItemsAs<InvokeNode>(true);

		Register(initial);
		Register(states);
		Register(historyStates);
		Register(transitions);

		Initial = initial;
		States = states;
		HistoryStates = historyStates;
		Transitions = transitions;
		Invoke = invokeList;
		OnEntry = state.OnEntry.UseAncestor.ItemsAs<OnEntryNode>(true);
		OnExit = state.OnExit.UseAncestor.ItemsAs<OnExitNode>(true);
		DataModel = state.DataModel?.UseAncestor.As<DataModelNode>();
	}

	public override bool IsAtomicState => true;

	public override DataModelNode? DataModel { get; }

	public override ImmutableArray<InvokeNode> Invoke { get; }

	public override ImmutableArray<TransitionNode> Transitions { get; }

	public override ImmutableArray<HistoryNode> HistoryStates { get; }

	public override ImmutableArray<StateEntityNode> States { get; }

	public override ImmutableArray<OnEntryNode> OnEntry { get; }

	public override ImmutableArray<OnExitNode> OnExit { get; }

	protected InitialNode? Initial { get; }

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _state;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"{Id}(#{DocumentId})";

#endregion

#region Interface IState

	IInitial? IState.Initial => Initial;

	IDataModel? IState.DataModel => DataModel;

	ImmutableArray<IInvoke> IState.Invoke => ImmutableArray<IInvoke>.CastUp(Invoke);

	ImmutableArray<IStateEntity> IState.States => ImmutableArray<IStateEntity>.CastUp(States);

	ImmutableArray<IHistory> IState.HistoryStates => ImmutableArray<IHistory>.CastUp(HistoryStates);

	ImmutableArray<ITransition> IState.Transitions => ImmutableArray<ITransition>.CastUp(Transitions);

	ImmutableArray<IOnEntry> IState.OnEntry => ImmutableArray<IOnEntry>.CastUp(OnEntry);

	ImmutableArray<IOnExit> IState.OnExit => ImmutableArray<IOnExit>.CastUp(OnExit);

#endregion

#region Interface IStateEntity

	public override IIdentifier Id => _state.Id;

#endregion
}