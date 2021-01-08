#region Copyright © 2019-2020 Sergii Artemenko

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
	internal sealed class FinalNode : StateEntityNode, IFinal, IAncestorProvider, IDebugEntityId
	{
		private readonly FinalEntity _final;

		public FinalNode(in DocumentIdRecord documentIdNode, in FinalEntity final) : base(documentIdNode, children: null)
		{
			_final = final;

			Id = final.Id ?? new IdentifierNode(Identifier.New());
			OnEntry = final.OnEntry.AsArrayOf<IOnEntry, OnEntryNode>(true);
			OnExit = final.OnExit.AsArrayOf<IOnExit, OnExitNode>(true);
			DoneData = final.DoneData?.As<DoneDataNode>();
		}

		public override bool                           IsAtomicState => true;
		public override ImmutableArray<TransitionNode> Transitions   => ImmutableArray<TransitionNode>.Empty;
		public override ImmutableArray<HistoryNode>    HistoryStates => ImmutableArray<HistoryNode>.Empty;
		public override ImmutableArray<InvokeNode>     Invoke        => ImmutableArray<InvokeNode>.Empty;
		public override ImmutableArray<OnEntryNode>    OnEntry       { get; }
		public override ImmutableArray<OnExitNode>     OnExit        { get; }
		public          DoneDataNode?                  DoneData      { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _final.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}(${DocumentId})";

	#endregion

	#region Interface IFinal

		public override IIdentifier Id { get; }

		ImmutableArray<IOnEntry> IFinal.OnEntry  => ImmutableArray<IOnEntry>.CastUp(OnEntry)!;
		ImmutableArray<IOnExit> IFinal. OnExit   => ImmutableArray<IOnExit>.CastUp(OnExit)!;
		IDoneData? IFinal.              DoneData => DoneData;

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
}