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

using Xtate.Persistence;

namespace Xtate.Core;

public sealed class ParallelNode : StateEntityNode, IParallel, IAncestorProvider, IDebugEntityId
{
	private readonly IParallel _parallel;

	public ParallelNode(DocumentIdNode documentIdNode, IParallel parallel) : base(documentIdNode)
	{
		_parallel = parallel;

		var id = parallel.Id ?? new IdentifierNode(Identifier.New());
		var transitions = parallel.Transitions.AsArrayOf<ITransition, TransitionNode>(true);
		var invokeList = parallel.Invoke.AsArrayOf<IInvoke, InvokeNode>(true);
		var states = parallel.States.AsArrayOf<IStateEntity, StateEntityNode>(true);
		var historyStates = parallel.HistoryStates.AsArrayOf<IHistory, HistoryNode>(true);

		Register(states);
		Register(historyStates);
		Register(transitions);
		Register(invokeList);

		Id = id;
		States = states;
		HistoryStates = historyStates;
		Transitions = transitions;
		Invoke = invokeList;
		OnEntry = parallel.OnEntry.AsArrayOf<IOnEntry, OnEntryNode>(true);
		OnExit = parallel.OnExit.AsArrayOf<IOnExit, OnExitNode>(true);
		DataModel = parallel.DataModel?.As<DataModelNode>();
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

	public override IIdentifier Id { get; }

#endregion

	protected override void Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.ParallelNode);
		bucket.Add(Key.DocumentId, DocumentId);
		bucket.AddEntity(Key.Id, Id);
		bucket.AddEntity(Key.DataModel, DataModel);
		bucket.AddEntityList(Key.States, States);
		bucket.AddEntityList(Key.HistoryStates, HistoryStates);
		bucket.AddEntityList(Key.Transitions, Transitions);
		bucket.AddEntityList(Key.OnEntry, OnEntry);
		bucket.AddEntityList(Key.OnExit, OnExit);
		bucket.AddEntityList(Key.Invoke, Invoke);
	}
}