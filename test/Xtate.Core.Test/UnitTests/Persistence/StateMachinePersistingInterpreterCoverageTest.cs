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
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class StateMachinePersistingInterpreterCoverageTest
{
	[TestMethod]
	public void FindTransitionNodeSearchesDirectAndNestedStateRanges()
	{
		var direct = CreateTransition(documentId: 5);
		var directRoot = CreateState(documentId: 1, transitions: [direct]);
		Assert.AreSame(direct, FindTransitionNode(directRoot, documentId: 5));

		var firstNested = CreateTransition(documentId: 15);
		var secondNested = CreateTransition(documentId: 20);
		var firstChild = CreateState(documentId: 10, transitions: [firstNested, secondNested]);
		var lastNested = CreateTransition(documentId: 35);
		var lastChild = CreateState(documentId: 30, transitions: [lastNested]);
		var nestedRoot = CreateState(documentId: 1, states: [firstChild, lastChild]);

		Assert.AreSame(secondNested, FindTransitionNode(nestedRoot, documentId: 20));
		Assert.AreSame(lastNested, FindTransitionNode(nestedRoot, documentId: 35));
	}

	[TestMethod]
	public void FindTransitionNodeRejectsUnknownDocumentIds()
	{
		var transition = CreateTransition(documentId: 5);
		var directRoot = CreateState(documentId: 1, transitions: [transition]);
		AssertFindThrows(directRoot, documentId: 6);

		AssertFindThrows(CreateState(documentId: 1), documentId: 9);
	}

	private static TransitionNode CreateTransition(int documentId)
	{
		var documentIds = new LinkedList<int>();
		var transition = new TransitionNode(new DocumentIdNode(documentIds), Mock.Of<ITransition>());
		documentIds.First!.Value = documentId;

		return transition;
	}

	private static TestStateNode CreateState(int documentId, ImmutableArray<TransitionNode> transitions = default, ImmutableArray<StateEntityNode> states = default)
	{
		var documentIds = new LinkedList<int>();
		var state = new TestStateNode(new DocumentIdNode(documentIds), transitions, states);
		documentIds.First!.Value = documentId;

		return state;
	}

	private static TransitionNode FindTransitionNode(StateEntityNode state, int documentId) =>
		(TransitionNode) typeof(StateMachinePersistingInterpreter)
			.GetMethod("FindTransitionNode", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
			.Invoke(obj: null, [state, documentId])!;

	private static void AssertFindThrows(StateEntityNode state, int documentId)
	{
		try
		{
			_ = FindTransitionNode(state, documentId);
			Assert.Fail("The missing transition was restored.");
		}
		catch (System.Reflection.TargetInvocationException exception)
		{
			Assert.IsInstanceOfType<KeyNotFoundException>(exception.InnerException);
		}
	}

	private sealed class TestStateNode(
		DocumentIdNode documentIdNode,
		ImmutableArray<TransitionNode> transitions,
		ImmutableArray<StateEntityNode> states) : StateEntityNode(documentIdNode)
	{
		public override ImmutableArray<TransitionNode> Transitions { get; } = transitions;

		public override ImmutableArray<StateEntityNode> States { get; } = states;
	}
}
