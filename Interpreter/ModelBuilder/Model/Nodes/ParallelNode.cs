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
public class ParallelNode : StateEntityNode, IParallel, IAncestorProvider, IDebugEntityId
{
	private readonly IParallel _parallel;

	public ParallelNode(DocumentIdNode documentIdNode, IParallel parallel) : base(documentIdNode)
	{
		_parallel = parallel;

		var transitions = parallel.Transitions.UseAncestor.ItemsAs<TransitionNode>(true);
		var invokeList = parallel.Invoke.UseAncestor.ItemsAs<InvokeNode>(true);
		var states = parallel.States.UseAncestor.ItemsAs<StateEntityNode>(true);
		var historyStates = parallel.HistoryStates.UseAncestor.ItemsAs<HistoryNode>(true);

		Register(states);
		Register(historyStates);
		Register(transitions);

		States = states;
		HistoryStates = historyStates;
		Transitions = transitions;
		Invoke = invokeList;
		OnEntry = parallel.OnEntry.UseAncestor.ItemsAs<OnEntryNode>(true);
		OnExit = parallel.OnExit.UseAncestor.ItemsAs<OnExitNode>(true);
		DataModel = parallel.DataModel?.UseAncestor.As<DataModelNode>();
	}

	public override bool IsAtomicState => false;

	public override DataModelNode? DataModel { get; }

	public override ImmutableArray<InvokeNode> Invoke { get; }

	public override ImmutableArray<TransitionNode> Transitions { get; }

	public override ImmutableArray<HistoryNode> HistoryStates { get; }

	public override ImmutableArray<StateEntityNode> States { get; }

	public override ImmutableArray<OnEntryNode> OnEntry { get; }

	public override ImmutableArray<OnExitNode> OnExit { get; }

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _parallel;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"{Id}(#{DocumentId})";

#endregion

#region Interface IParallel

	IDataModel? IParallel.DataModel => DataModel;

	ImmutableArray<IInvoke> IParallel.Invoke => ImmutableArray<IInvoke>.CastUp(Invoke);

	ImmutableArray<IStateEntity> IParallel.States => ImmutableArray<IStateEntity>.CastUp(States);

	ImmutableArray<IHistory> IParallel.HistoryStates => ImmutableArray<IHistory>.CastUp(HistoryStates);

	ImmutableArray<ITransition> IParallel.Transitions => ImmutableArray<ITransition>.CastUp(Transitions);

	ImmutableArray<IOnEntry> IParallel.OnEntry => ImmutableArray<IOnEntry>.CastUp(OnEntry);

	ImmutableArray<IOnExit> IParallel.OnExit => ImmutableArray<IOnExit>.CastUp(OnExit);

#endregion

#region Interface IStateEntity

	public override IIdentifier Id => _parallel.Id!;

#endregion
}