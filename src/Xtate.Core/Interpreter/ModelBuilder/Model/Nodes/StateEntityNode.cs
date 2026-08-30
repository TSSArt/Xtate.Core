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

using Xtate.Interpreter.Internal;
using Xtate.StateMachine;

namespace Xtate.Interpreter.Model;

public abstract class StateEntityNode : IStateEntity, IDocumentId
{
	public static readonly IComparer<StateEntityNode> EntryOrder = new OrderComparer(reverseOrder: false);

	public static readonly IComparer<StateEntityNode> ExitOrder = new OrderComparer(reverseOrder: true);

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

	protected void Register(InitialNode? initialNode) => initialNode?.Parent = this;

	protected void Register(ImmutableArray<StateEntityNode> stateEntityNodes)
	{
		if (!stateEntityNodes.IsDefaultOrEmpty)
		{
			foreach (var stateEntityNode in stateEntityNodes)
			{
				stateEntityNode?.Parent = this;
			}
		}
	}

	protected void Register(ImmutableArray<HistoryNode> historyNodes)
	{
		if (!historyNodes.IsDefaultOrEmpty)
		{
			foreach (var historyNode in historyNodes)
			{
				historyNode?.Parent = this;
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

	private NotSupportedException GetNotSupportedException() => new(Res.Format(Resources.Exception_SpecifiedMethodIsNotSupportedInType, GetType().Name));

	private sealed class OrderComparer(bool reverseOrder) : IComparer<StateEntityNode>
	{
	#region Interface IComparer<StateEntityNode>

		public int Compare(StateEntityNode? x, StateEntityNode? y) => reverseOrder ? InternalCompare(y, x) : InternalCompare(x, y);

	#endregion

		private static int InternalCompare(StateEntityNode? x, StateEntityNode? y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (y is null) return 1;
			if (x is null) return -1;

			var xId = x.DocumentId;
			var yId = y.DocumentId;

			return xId >= 0 && yId >= 0 ? xId.CompareTo(yId) : HierarchyCompare(x, y);
		}

		private static int HierarchyCompare(StateEntityNode x, StateEntityNode y)
		{
			var depth = 0;

			for (var node = x; node.Parent is not null; node = node.Parent)
			{
				depth++;
			}

			for (var node = y; node.Parent is not null; node = node.Parent)
			{
				depth--;
			}

			var xNode = x;
			var yNode = y;

			while (depth > 0)
			{
				xNode = xNode.Parent!;
				depth--;
			}

			while (depth < 0)
			{
				yNode = yNode.Parent!;
				depth++;
			}

			if (ReferenceEquals(xNode, yNode))
			{
				return ReferenceEquals(xNode, x) ? -1 : 1;
			}

			while (!ReferenceEquals(xNode.Parent, yNode.Parent))
			{
				xNode = xNode.Parent!;
				yNode = yNode.Parent!;
			}

			foreach (var state in xNode.Parent!.States)
			{
				if (ReferenceEquals(state, xNode))
				{
					return -1;
				}

				if (ReferenceEquals(state, yNode))
				{
					return 1;
				}
			}

			throw Infra.Fail<Exception>();
		}
	}
}
