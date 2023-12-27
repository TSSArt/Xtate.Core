// Copyright © 2019-2023 Sergii Artemenko
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

public class FinalNode(DocumentIdNode documentIdNode, IFinal final) : StateEntityNode(documentIdNode), IFinal, IAncestorProvider, IDebugEntityId
{
	public DoneDataNode? DoneData { get; } = final.DoneData?.As<DoneDataNode>();

	public override ImmutableArray<TransitionNode> Transitions   => [];
	public override ImmutableArray<HistoryNode>    HistoryStates => [];
	public override ImmutableArray<InvokeNode>     Invoke        => [];
	public override ImmutableArray<OnEntryNode>    OnEntry       { get; } = final.OnEntry.AsArrayOf<IOnEntry, OnEntryNode>(true);
	public override ImmutableArray<OnExitNode>     OnExit        { get; } = final.OnExit.AsArrayOf<IOnExit, OnExitNode>(true);
	public override bool                           IsAtomicState => true;

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => final;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"{Id}(${DocumentId})";

#endregion

#region Interface IFinal

	ImmutableArray<IOnEntry> IFinal.OnEntry  => ImmutableArray<IOnEntry>.CastUp(OnEntry);
	ImmutableArray<IOnExit> IFinal. OnExit   => ImmutableArray<IOnExit>.CastUp(OnExit);
	IDoneData? IFinal.              DoneData => DoneData;

#endregion

#region Interface IStateEntity

	public override IIdentifier Id { get; } = final.Id ?? new IdentifierNode(Identifier.New());

#endregion

	protected override void Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.FinalNode);
		bucket.Add(Key.DocumentId, DocumentId);
		bucket.AddEntity(Key.Id, Id);
		bucket.AddEntityList(Key.OnEntry, OnEntry);
		bucket.AddEntityList(Key.OnExit, OnExit);
		bucket.AddEntity(Key.DoneData, DoneData);
	}
}