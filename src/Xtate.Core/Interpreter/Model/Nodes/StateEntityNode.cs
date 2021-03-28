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
using System.Collections.Generic;
using System.Collections.Immutable;
using Xtate.Persistence;

namespace Xtate.Core
{
	internal abstract class StateEntityNode : IStateEntity, IStoreSupport, IDocumentId
	{
		public static readonly IComparer<StateEntityNode> EntryOrder = new DocumentOrderComparer(reverseOrder: false);
		public static readonly IComparer<StateEntityNode> ExitOrder  = new DocumentOrderComparer(reverseOrder: true);

		private DocumentIdSlot _documentIdSlot;

		protected StateEntityNode(DocumentIdNode documentIdNode, IEnumerable<StateEntityNode>? children)
		{
			documentIdNode.SaveToSlot(out _documentIdSlot);

			if (children is not null)
			{
				foreach (var stateEntity in children)
				{
					stateEntity.Parent = this;
				}
			}
		}

		public StateEntityNode? Parent { get; private set; }

		public virtual bool                            IsAtomicState => throw GetNotSupportedException();
		public virtual IIdentifier                     Id            => throw GetNotSupportedException();
		public virtual ImmutableArray<TransitionNode>  Transitions   => throw GetNotSupportedException();
		public virtual ImmutableArray<OnEntryNode>     OnEntry       => throw GetNotSupportedException();
		public virtual ImmutableArray<OnExitNode>      OnExit        => throw GetNotSupportedException();
		public virtual ImmutableArray<InvokeNode>      Invoke        => throw GetNotSupportedException();
		public virtual ImmutableArray<StateEntityNode> States        => throw GetNotSupportedException();
		public virtual ImmutableArray<HistoryNode>     HistoryStates => throw GetNotSupportedException();
		public virtual DataModelNode?                  DataModel     => throw GetNotSupportedException();

	#region Interface IDocumentId

		public int DocumentId => _documentIdSlot.Value;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket) => Store(bucket);

	#endregion

		private NotSupportedException GetNotSupportedException() => new(Res.Format(Resources.Exception_SpecifiedMethodIsNotSupportedInType, GetType().Name));

		protected static IEnumerable<StateEntityNode> GetChildNodes(IInitial? initial, ImmutableArray<IStateEntity> states, ImmutableArray<IHistory> historyStates = default)
		{
			if (initial is not null)
			{
				yield return initial.As<InitialNode>();
			}

			if (!states.IsDefaultOrEmpty)
			{
				foreach (var node in states)
				{
					yield return node.As<StateEntityNode>();
				}
			}

			if (!historyStates.IsDefaultOrEmpty)
			{
				foreach (var node in historyStates)
				{
					yield return node.As<StateEntityNode>();
				}
			}
		}

		protected abstract void Store(Bucket bucket);

		private sealed class DocumentOrderComparer : IComparer<StateEntityNode>
		{
			private readonly bool _reverseOrder;

			public DocumentOrderComparer(bool reverseOrder) => _reverseOrder = reverseOrder;

		#region Interface IComparer<StateEntityNode>

			public int Compare(StateEntityNode? x, StateEntityNode? y) => _reverseOrder ? InternalCompare(y, x) : InternalCompare(x, y);

		#endregion

			private static int InternalCompare(StateEntityNode? x, StateEntityNode? y)
			{
				if (x == y) return 0;
				if (y is null) return 1;
				if (x is null) return -1;

				return x.DocumentId.CompareTo(y.DocumentId);
			}
		}
	}
}