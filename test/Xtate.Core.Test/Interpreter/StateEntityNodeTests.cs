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

using Xtate.Interpreter.Model;

namespace Xtate.Core.Test.Interpreter;

[TestClass]
public sealed class StateEntityNodeTests
{
	[TestMethod]
	public void EntryOrder_ShouldReturnZero_ForSameNode()
	{
		var node = CreateNode();

		Assert.AreEqual(expected: 0, StateEntityNode.EntryOrder.Compare(node, node));
	}

	[TestMethod]
	public void EntryOrder_ShouldPlaceParentBeforeDirectChild()
	{
		var parent = CreateNode();
		var child = CreateNode(parent);

		Assert.IsTrue(StateEntityNode.EntryOrder.Compare(parent, child) < 0);
		Assert.IsTrue(StateEntityNode.EntryOrder.Compare(child, parent) > 0);
	}

	[TestMethod]
	public void EntryOrder_ShouldPlaceAncestorBeforeDescendant()
	{
		var ancestor = CreateNode();
		var parent = CreateNode(ancestor);
		var descendant = CreateNode(parent);

		Assert.IsTrue(StateEntityNode.EntryOrder.Compare(ancestor, descendant) < 0);
		Assert.IsTrue(StateEntityNode.EntryOrder.Compare(descendant, ancestor) > 0);
	}

	[TestMethod]
	public void EntryOrder_ShouldPreserveSiblingDeclarationOrder()
	{
		var parent = CreateNode();
		var firstChild = CreateNode(parent);
		var secondChild = CreateNode(parent);

		Assert.IsTrue(StateEntityNode.EntryOrder.Compare(firstChild, secondChild) < 0);
		Assert.IsTrue(StateEntityNode.EntryOrder.Compare(secondChild, firstChild) > 0);
	}

	[TestMethod]
	public void EntryOrder_ShouldPlaceEarlierBranchBeforeLaterBranch()
	{
		var root = CreateNode();
		var firstBranch = CreateNode(root);
		var secondBranch = CreateNode(root);
		var firstBranchDescendant = CreateNode(firstBranch);
		var secondBranchDescendant = CreateNode(secondBranch);

		Assert.IsTrue(StateEntityNode.EntryOrder.Compare(firstBranchDescendant, secondBranchDescendant) < 0);
		Assert.IsTrue(StateEntityNode.EntryOrder.Compare(secondBranchDescendant, firstBranchDescendant) > 0);
	}

	[TestMethod]
	public void EntryOrder_ShouldOrderNodesAsHierarchyPreOrder()
	{
		var root = CreateNode();
		var firstChild = CreateNode(root);
		var secondChild = CreateNode(root);
		var firstGrandchild = CreateNode(firstChild);
		var secondGrandchild = CreateNode(firstChild);

		StateEntityNode[] nodes = [secondGrandchild, secondChild, root, firstGrandchild, firstChild];

		Array.Sort(nodes, StateEntityNode.EntryOrder);

		Assert.AreSequenceEqual([root, firstChild, firstGrandchild, secondGrandchild, secondChild], nodes);
	}

	private static Node CreateNode(Node? parent = null) => new(parent);

	private class Node : StateEntityNode
	{
		public Node(Node? parent) : base(new DocumentIdNode(null))
		{
			parent?.PublicStates.Add(this);
			parent?.Register([this]);
		}

		private List<StateEntityNode> PublicStates { get; } = [];

		public override ImmutableArray<StateEntityNode> States => [.. PublicStates];
	}
}
