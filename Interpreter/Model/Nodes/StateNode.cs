#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Collections.Immutable;
using Xtate.Persistence;

namespace Xtate.Core
{
	internal class StateNode : StateEntityNode, IState, IAncestorProvider, IDebugEntityId
	{
		private readonly StateEntity _state;

		public StateNode(in DocumentIdRecord documentIdNode, in StateEntity state) : base(documentIdNode, GetChildNodes(state.Initial, state.States, state.HistoryStates))
		{
			_state = state;

			var id = state.Id ?? new IdentifierNode(Identifier.New());
			var transitions = state.Transitions.AsArrayOf<ITransition, TransitionNode>(true);
			var invokeList = state.Invoke.AsArrayOf<IInvoke, InvokeNode>(true);

			Id = id;
			States = state.States.AsArrayOf<IStateEntity, StateEntityNode>();
			HistoryStates = state.HistoryStates.AsArrayOf<IHistory, HistoryNode>(true);
			Transitions = transitions;
			OnEntry = state.OnEntry.AsArrayOf<IOnEntry, OnEntryNode>(true);
			OnExit = state.OnExit.AsArrayOf<IOnExit, OnExitNode>(true);
			Invoke = invokeList;
			Initial = state.Initial?.As<InitialNode>();
			DataModel = state.DataModel?.As<DataModelNode>();

			foreach (var transition in transitions)
			{
				transition.SetSource(this);
			}

			foreach (var invoke in invokeList)
			{
				invoke.SetStateId(id);
			}
		}

		public override bool                            IsAtomicState => true;
		public override ImmutableArray<InvokeNode>      Invoke        { get; }
		public override ImmutableArray<TransitionNode>  Transitions   { get; }
		public override ImmutableArray<HistoryNode>     HistoryStates { get; }
		public override ImmutableArray<StateEntityNode> States        { get; }
		public override ImmutableArray<OnEntryNode>     OnEntry       { get; }
		public override ImmutableArray<OnExitNode>      OnExit        { get; }
		public override DataModelNode?                  DataModel     { get; }

		protected InitialNode? Initial { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _state.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}(#{DocumentId})";

	#endregion

	#region Interface IState

		public override IIdentifier Id { get; }

		IInitial? IState.                   Initial       => Initial;
		IDataModel? IState.                 DataModel     => DataModel;
		ImmutableArray<IInvoke> IState.     Invoke        => ImmutableArray<IInvoke>.CastUp(Invoke)!;
		ImmutableArray<IStateEntity> IState.States        => ImmutableArray<IStateEntity>.CastUp(States)!;
		ImmutableArray<IHistory> IState.    HistoryStates => ImmutableArray<IHistory>.CastUp(HistoryStates)!;
		ImmutableArray<ITransition> IState. Transitions   => ImmutableArray<ITransition>.CastUp(Transitions)!;
		ImmutableArray<IOnEntry> IState.    OnEntry       => ImmutableArray<IOnEntry>.CastUp(OnEntry)!;
		ImmutableArray<IOnExit> IState.     OnExit        => ImmutableArray<IOnExit>.CastUp(OnExit)!;

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.StateNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Id, Id);
			bucket.AddEntity(Key.Initial, Initial);
			bucket.AddEntity(Key.DataModel, DataModel);
			bucket.AddEntityList(Key.HistoryStates, HistoryStates);
			bucket.AddEntityList(Key.Transitions, Transitions);
			bucket.AddEntityList(Key.OnEntry, OnEntry);
			bucket.AddEntityList(Key.OnExit, OnExit);
			bucket.AddEntityList(Key.Invoke, Invoke);
		}
	}
}