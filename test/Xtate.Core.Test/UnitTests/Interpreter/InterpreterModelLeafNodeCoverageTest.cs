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

using System.Globalization;
using Xtate.Ancestor;
using Xtate.DataModel;
using Xtate.Interpreter;
using Xtate.Interpreter.Internal;
using Xtate.Interpreter.Model;
using Xtate.Interpreter.Services;
using Xtate.StateMachine;
using Xtate.StateMachine.Internal;

namespace Xtate.Test.UnitTests.Interpreter;

[TestClass]
public class InterpreterModelLeafNodeCoverageTest
{
	[TestMethod]
	public void ExternalScriptExpressionNodeForwardsUriStoresAndPropagatesContent()
	{
		var source = new ExternalScriptSource { Uri = new Uri("https://example.test/action.js") };
		var node = new TestExternalScriptExpressionNode(source);

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreEqual(source.Uri, node.Uri);

		((IExternalScriptConsumer) node).SetContent("script body");

		Assert.AreEqual(expected: "script body", node.StoredContent);
		Assert.AreEqual(expected: "script body", source.Content);
	}

	[TestMethod]
	public async Task ScriptNodeForwardsScriptMembersDocumentIdAndExecution()
	{
		var content = Mock.Of<IScriptExpression>();
		var external = Mock.Of<IExternalScriptExpression>();
		var source = new ScriptSource { Content = content, Source = external };
		var documentIds = new LinkedList<int>();
		var node = new ScriptNode(new DocumentIdNode(documentIds), source);
		documentIds.First!.Value = 73;

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreSame(content, node.Content);
		Assert.AreSame(external, node.Source);
		Assert.AreEqual(expected: 73, node.DocumentId);

		await node.Execute();
		Assert.AreEqual(expected: 1, source.ExecuteCount);
	}

	[TestMethod]
	public void IdentifierNodeForwardsIdentityDebugAndObjectMembers()
	{
		var identifier = Identifier.FromString("state-id");
		var node = new IdentifierNode(identifier);

		Assert.AreSame(identifier, ((IAncestorProvider) node).Ancestor);
		Assert.AreEqual(expected: "state-id", node.Value);
		Assert.AreEqual(expected: "state-id", node.ToString());
		Assert.AreEqual(expected: "state-id", typeof(IdentifierNode).GetMethod(nameof(ToString), Type.EmptyTypes)!.Invoke(node, parameters: null));
		Assert.AreEqual(identifier.GetHashCode(), node.GetHashCode());
		Assert.IsTrue(node.Equals(identifier));
		Assert.IsFalse(node.Equals(Identifier.FromString("other")));
		Assert.IsFalse((bool) typeof(Identifier).GetMethod(nameof(Equals), [typeof(object)])!.Invoke(identifier, ["state-id"])!);
		Assert.IsTrue((bool) typeof(Identifier).GetMethod(nameof(Equals), [typeof(object)])!.Invoke(identifier, [Identifier.FromString("state-id")])!);
		Assert.IsFalse((bool) typeof(Identifier).GetMethod(nameof(Equals), [typeof(object)])!.Invoke(identifier, [null])!);
		Assert.AreEqual(expected: "state-id", ((IDebugEntityId) node).EntityId.ToString(CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void EventDescriptorNodeForwardsIdentityAndMatchesIncomingEvents()
	{
		var descriptor = EventDescriptor.FromString("order.*");
		var node = new EventDescriptorNode(descriptor);

		Assert.AreSame(descriptor, ((IAncestorProvider) node).Ancestor);
		Assert.AreEqual(expected: "order.*", node.Value);
		Assert.AreEqual(expected: "order.*", node.ToString());
		Assert.AreEqual(expected: "order.*", typeof(EventDescriptorNode).GetMethod(nameof(ToString), Type.EmptyTypes)!.Invoke(node, parameters: null));
		Assert.AreEqual(descriptor.GetHashCode(), node.GetHashCode());
		Assert.IsTrue(node.Equals(descriptor));
		Assert.IsFalse(node.Equals(EventDescriptor.FromString("payment.*")));
		Assert.AreEqual(expected: "order.*", ((IDebugEntityId) node).EntityId.ToString(CultureInfo.InvariantCulture));
		Assert.IsTrue(node.IsEventMatch(new IncomingEvent { Name = EventName.FromString("order.created") }));
		Assert.IsFalse(node.IsEventMatch(new IncomingEvent { Name = EventName.FromString("payment.created") }));
	}

	[TestMethod]
	public void IdentifierAndEventDescriptorNodesNormalizeNullSourceText()
	{
		var identifier = new Mock<IIdentifier>();
		var descriptor = new Mock<IEventDescriptor>();
		identifier.Setup(static value => value.ToString()).Returns((string) null!);
		descriptor.Setup(static value => value.ToString()).Returns((string) null!);

		Assert.AreEqual(string.Empty, new IdentifierNode(identifier.Object).ToString());
		Assert.AreEqual(string.Empty, new EventDescriptorNode(descriptor.Object).ToString());
	}

	[TestMethod]
	public void EventDescriptorSupportsCastValueEqualityHashingAndNullComparisons()
	{
		var descriptor = (EventDescriptor) "event.name";
		var equal = EventDescriptor.FromString("event.name");
		var different = EventDescriptor.FromString("other");

		Assert.AreEqual(expected: "event.name", descriptor.Value);
		Assert.AreEqual(expected: "event.name", descriptor.ToString());
		Assert.IsTrue(descriptor.Equals(descriptor));
		Assert.IsTrue(descriptor.Equals(equal));
		Assert.IsTrue(descriptor.Equals((object) equal));
		Assert.IsFalse(descriptor.Equals(different));
		Assert.IsFalse(descriptor.Equals(null));
		Assert.IsFalse(descriptor.Equals(new object()));
		Assert.AreEqual(equal.GetHashCode(), descriptor.GetHashCode());
	}

	[TestMethod]
	public void StateEntityDocumentOrderComparersUseDocumentIdsInBothDirections()
	{
		var documentIds = new LinkedList<int>();
		var first = new TestStateEntityNode(Identifier.FromString("first"), new DocumentIdNode(documentIds));
		var second = new TestStateEntityNode(Identifier.FromString("second"), new DocumentIdNode(documentIds));
		documentIds.First!.Value = 10;
		documentIds.Last!.Value = 20;

		Assert.IsLessThan(upperBound: 0, StateEntityNode.EntryOrder.Compare(first, second));
		Assert.IsGreaterThan(lowerBound: 0, StateEntityNode.ExitOrder.Compare(first, second));
		Assert.AreEqual(expected: 0, StateEntityNode.EntryOrder.Compare(first, first));
		var collection = new List<TestStateEntityNode> { second, first };
		collection.Sort(StateEntityNode.EntryOrder);
		CollectionAssert.AreEqual(new[] { first, second }, collection);
		var actual = new List<TestStateEntityNode> { first, second };
		actual.Sort(StateEntityNode.ExitOrder);
		CollectionAssert.AreEqual(new[] { second, first }, actual);
	}

	[TestMethod]
	public void StateEntityBasePropertiesReportUnsupportedDerivedType()
	{
		var node = new BareStateEntityNode();
		Func<object?>[] accessors =
		[
			() => node.IsAtomicState,
			() => node.Transitions,
			() => node.OnEntry,
			() => node.OnExit,
			() => node.Invoke,
			() => node.HistoryStates,
			() => node.States,
			() => node.DataModel,
			() => node.Id
		];

		foreach (var accessor in accessors)
		{
			var exception = Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => _ = accessor());
			StringAssert.Contains(exception.Message, nameof(BareStateEntityNode));
		}
	}

	[TestMethod]
	public void InStateControllerFindsOnlyIdentifiersInCurrentConfiguration()
	{
		var activeId = Identifier.FromString("active");
		var configuration = new OrderedSet<StateEntityNode> { new TestStateEntityNode(activeId) };
		var context = new Mock<IStateMachineContext>();
		context.SetupGet(static c => c.Configuration).Returns(configuration);
		var controller = new InStateController { StateMachineContext = context.Object };

		Assert.IsTrue(controller.InState(activeId));
		Assert.IsFalse(controller.InState(Identifier.FromString("inactive")));
	}

	private sealed class TestExternalScriptExpressionNode(IExternalScriptExpression source) : ExternalScriptExpressionNode(source)
	{
		public string? StoredContent => Content;
	}

	private sealed class ExternalScriptSource : IExternalScriptExpression, IExternalScriptConsumer
	{
		public string? Content { get; private set; }

	#region Interface IExternalScriptConsumer

		public void SetContent(string content) => Content = content;

	#endregion

	#region Interface IExternalScriptExpression

		public Uri? Uri { get; init; }

	#endregion
	}

	private sealed class ScriptSource : IScript, IExecEvaluator
	{
		public int ExecuteCount { get; private set; }

	#region Interface IExecEvaluator

		public ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}

	#endregion

	#region Interface IScript

		public IScriptExpression? Content { get; init; }

		public IExternalScriptExpression? Source { get; init; }

	#endregion
	}

	private sealed class TestStateEntityNode(IIdentifier id, DocumentIdNode documentIdNode) : StateEntityNode(documentIdNode)
	{
		public TestStateEntityNode(IIdentifier id) : this(id, new DocumentIdNode(list: null)) { }

		public override IIdentifier Id => id;
	}

	private sealed class BareStateEntityNode : StateEntityNode
	{
		public BareStateEntityNode() : base(new DocumentIdNode(list: null)) { }
	}
}