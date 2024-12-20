﻿// Copyright © 2019-2024 Sergii Artemenko
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

public abstract class StateEntityNode : IStateEntity, IStoreSupport, IDocumentId
{
	public static readonly IComparer<StateEntityNode> EntryOrder = new DocumentOrderComparer(reverseOrder: false);

	public static readonly IComparer<StateEntityNode> ExitOrder = new DocumentOrderComparer(reverseOrder: true);

	private DocumentIdSlot _documentIdSlot;

	protected StateEntityNode(DocumentIdNode documentIdNode) => documentIdNode.SaveToSlot(out _documentIdSlot);

	public StateEntityNode? Parent { get; private set; }

	public virtual bool IsAtomicState => throw GetNotSupportedException();

	public virtual ImmutableArray<TransitionNode> Transitions => throw GetNotSupportedException();

	public virtual ImmutableArray<OnEntryNode> OnEntry => throw GetNotSupportedException();

	public virtual ImmutableArray<OnExitNode> OnExit => throw GetNotSupportedException();

	public virtual ImmutableArray<InvokeNode> Invoke => throw GetNotSupportedException();

	public virtual ImmutableArray<HistoryNode> HistoryStates => throw GetNotSupportedException();

	public virtual ImmutableArray<StateEntityNode> States => throw GetNotSupportedException();

	public virtual DataModelNode? DataModel => throw GetNotSupportedException();

#region Interface IDocumentId

	public int DocumentId => _documentIdSlot.CreateValue();

#endregion

#region Interface IStateEntity

	public virtual IIdentifier Id => throw GetNotSupportedException();

#endregion

#region Interface IStoreSupport

	void IStoreSupport.Store(Bucket bucket) => Store(bucket);

#endregion

	protected void Register(InitialNode? initialNode)
	{
		if (initialNode is not null)
		{
			initialNode.Parent = this;
		}
	}

	protected void Register(ImmutableArray<StateEntityNode> stateEntityNodes)
	{
		if (stateEntityNodes.IsDefaultOrEmpty)
		{
			return;
		}

		foreach (var stateEntityNode in stateEntityNodes)
		{
			if (stateEntityNode is not null)
			{
				stateEntityNode.Parent = this;
			}
		}
	}

	protected void Register(ImmutableArray<HistoryNode> historyNodes)
	{
		if (historyNodes.IsDefaultOrEmpty)
		{
			return;
		}

		foreach (var historyNode in historyNodes)
		{
			if (historyNode is not null)
			{
				historyNode.Parent = this;
			}
		}
	}

	protected void Register(ImmutableArray<TransitionNode> transitionNodes)
	{
		if (transitionNodes.IsDefaultOrEmpty)
		{
			return;
		}

		foreach (var transitionNode in transitionNodes)
		{
			transitionNode?.SetSource(this);
		}
	}

	protected void Register(ImmutableArray<InvokeNode> invokeNodes)
	{
		if (invokeNodes.IsDefaultOrEmpty)
		{
			return;
		}

		foreach (var invokeNode in invokeNodes)
		{
			invokeNode?.SetSource(this);
		}
	}

	private NotSupportedException GetNotSupportedException() => new(Res.Format(Resources.Exception_SpecifiedMethodIsNotSupportedInType, GetType().Name));

	protected abstract void Store(Bucket bucket);

	private sealed class DocumentOrderComparer(bool reverseOrder) : IComparer<StateEntityNode>
	{
	#region Interface IComparer<StateEntityNode>

		public int Compare(StateEntityNode? x, StateEntityNode? y) => reverseOrder ? InternalCompare(y, x) : InternalCompare(x, y);

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