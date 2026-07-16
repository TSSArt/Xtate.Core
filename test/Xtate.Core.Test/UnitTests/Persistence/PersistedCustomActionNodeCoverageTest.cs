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

using System.Reflection;
using Xtate.Ancestor;
using Xtate.DataModel;
using Xtate.Interpreter.Model;
using Xtate.Persistence;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;
using Xtate.StateMachine;
using TypeInfo = Xtate.Persistence.Internal.TypeInfo;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class PersistedCustomActionNodeCoverageTest
{
	[TestMethod]
	public async Task PersistedCustomActionNodeForwardsAndStoresActionProperties()
	{
		var source = new CustomActionSource
					 {
						 XmlNamespace = "urn:test-actions",
						 XmlName = "action",
						 Xml = "<action xmlns='urn:test-actions'/>",
						 Locations = default,
						 Values = default
					 };
		var documentIds = new LinkedList<int>();
		var node = new PersistedCustomActionNode(new DocumentIdNode(documentIds), source);
		documentIds.First!.Value = 42;

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreEqual(expected: "urn:test-actions", node.XmlNamespace);
		Assert.AreEqual(expected: "action", node.XmlName);
		Assert.AreEqual(expected: "<action xmlns='urn:test-actions'/>", node.Xml);
		Assert.IsTrue(node.Locations.IsDefault);
		Assert.IsTrue(node.Values.IsDefault);
		Assert.AreEqual(expected: 42, node.DocumentId);
		Assert.AreEqual(expected: 42, node.DocumentId);

		await node.Execute();
		Assert.AreEqual(expected: 1, source.ExecuteCount);

		var bucket = new Bucket(new InMemoryStorage(writeOnly: false));
		((IStoreSupport) node).Store(bucket);

		Assert.IsTrue(bucket.TryGet(Key.TypeInfo, out TypeInfo typeInfo));
		Assert.AreEqual(TypeInfo.CustomActionNode, typeInfo);
		Assert.AreEqual(expected: 42, bucket.GetInt32(Key.DocumentId));
		Assert.AreEqual(expected: "urn:test-actions", bucket.GetString(Key.Namespace));
		Assert.AreEqual(expected: "action", bucket.GetString(Key.Name));
		Assert.AreEqual(expected: "<action xmlns='urn:test-actions'/>", bucket.GetString(Key.Content));
		Assert.IsFalse(bucket.TryGet(Key.LocationList, out int _));
		Assert.IsFalse(bucket.TryGet(Key.ValueList, out int _));
	}

	[TestMethod]
	public void StateMachineReaderRestoresCustomActionAndItsExpressionLists()
	{
		var source = new CustomActionSource
					 {
						 XmlNamespace = "urn:test-actions",
						 XmlName = "action",
						 Xml = "<action xmlns='urn:test-actions'/>",
						 Locations = [new PersistedLocationExpressionNode(new LocationExpression { Expression = "target" })],
						 Values = [new PersistedValueExpressionNode(new ValueExpression { Expression = "value" })]
					 };
		var documentIds = new LinkedList<int>();
		var node = new PersistedCustomActionNode(new DocumentIdNode(documentIds), source);
		documentIds.First!.Value = 43;
		var bucket = new Bucket(new InMemoryStorage(writeOnly: false));
		((IStoreSupport) node).Store(bucket);
		var restore = typeof(StateMachineReader).GetMethod(name: "RestoreCustomAction", BindingFlags.Static | BindingFlags.NonPublic)!;

		var restored = (ICustomAction?) restore.Invoke(obj: null, [bucket]);

		Assert.IsNotNull(restored);
		var restoredNotNull = restored!;
		Assert.AreEqual(expected: "urn:test-actions", restoredNotNull.XmlNamespace);
		Assert.AreEqual(expected: "action", restoredNotNull.XmlName);
		Assert.AreEqual(expected: "<action xmlns='urn:test-actions'/>", restoredNotNull.Xml);
		Assert.AreEqual(expected: "target", restoredNotNull.Locations.Single().Expression);
		Assert.AreEqual(expected: "value", restoredNotNull.Values.Single().Expression);
		var persistedDocument = ((IAncestorProvider) restoredNotNull).Ancestor as IPersistedDocumentId;
		Assert.IsNotNull(persistedDocument);
		Assert.AreEqual(expected: 43, persistedDocument.DocumentId);

		var emptyBucket = new Bucket(new InMemoryStorage(writeOnly: false));
		Assert.IsNull(restore.Invoke(obj: null, [emptyBucket]));
	}

	private sealed class CustomActionSource : ICustomAction, IExecEvaluator
	{
		public int ExecuteCount { get; private set; }

	#region Interface ICustomAction

		public string? XmlNamespace { get; init; }

		public string? XmlName { get; init; }

		public string? Xml { get; init; }

		public ImmutableArray<ILocationExpression> Locations { get; init; }

		public ImmutableArray<IValueExpression> Values { get; init; }

	#endregion

	#region Interface IExecEvaluator

		public ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}

	#endregion
	}
}