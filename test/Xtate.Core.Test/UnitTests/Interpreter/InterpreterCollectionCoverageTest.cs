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

using System.IO;
using System.Xml;
using System.Xml.XPath;
using Xtate.Ancestor;
using Xtate.Ancestor.Extensions;
using Xtate.DataModel.XPath.Internal;
using Xtate.Interpreter.Internal;

namespace Xtate.Test.UnitTests.Interpreter;

[TestClass]
public class InterpreterCollectionCoverageTest
{
	[TestMethod]
	public void EntityQueueRaisesChangedEventsForEnqueueAndDequeue()
	{
		var queue = new EntityQueue<string>();
		var events = new List<(EntityQueue<string>.ChangedAction Action, string? Entity)>();
		queue.Changed += (action, entity) => events.Add((action, entity));

		queue.Enqueue("first");
		var value = queue.Dequeue();

		Assert.AreEqual(expected: "first", value);
		CollectionAssert.AreEqual(
			new[]
			{
				(EntityQueue<string>.ChangedAction.Enqueue, "first"),
				(EntityQueue<string>.ChangedAction.Dequeue, default(string))
			},
			events);
	}

	[TestMethod]
	public void OrderedSetMaintainsOrderUniquenessAndRaisesChangedEvents()
	{
		var set = new OrderedSet<int>();
		var events = new List<(OrderedSet<int>.ChangedAction Action, int Entity)>();
		set.Changed += (action, entity) => events.Add((action, entity));

		Assert.IsTrue(set.IsEmpty);

		set.AddIfNotExists(2);
		set.AddIfNotExists(2);
		set.Add(1);
		set.Delete(2);

		CollectionAssert.AreEqual(new[] { 1 }, set);
		Assert.IsFalse(set.IsEmpty);
		Assert.IsTrue(set.IsMember(1));
		Assert.IsFalse(set.IsMember(2));
		CollectionAssert.AreEqual(new[] { 1 }, set.ToSortedList(Comparer<int>.Default));
		CollectionAssert.AreEqual(new[] { 1 }, set.ToFilteredSortedList(static item => item > 0, Comparer<int>.Default));
		CollectionAssert.AreEqual(new[] { 1 }, set.ToFilteredList(static (item, min) => item >= min, arg: 1));
		CollectionAssert.AreEqual(
			new[]
			{
				(OrderedSet<int>.ChangedAction.Add, 2),
				(OrderedSet<int>.ChangedAction.Add, 1),
				(OrderedSet<int>.ChangedAction.Delete, 2)
			},
			events);

		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => set.ToFilteredList(null!, arg: 0));

		set.Clear();

		Assert.IsTrue(set.IsEmpty);
		Assert.AreEqual((OrderedSet<int>.ChangedAction.Clear, 0), events[^1]);
	}

	[TestMethod]
	public void AncestorArrayMapsItemsThroughAncestorChainAndHandlesDefaultArrays()
	{
		ImmutableArray<AncestorSource> defaultArray = default;
		var defaultResult = defaultArray.UseAncestor.ItemsAs<ITestAncestor>();
		var emptyResult = defaultArray.UseAncestor.ItemsAs<ITestAncestor>(emptyArrayIfDefault: true);
		var ancestor = new TestAncestor("ancestor");
		var array = ImmutableArray.Create(new AncestorSource(ancestor), null!);

		var result = array.UseAncestor.ItemsAs<ITestAncestor>();

		Assert.IsTrue(defaultResult.IsDefault);
		Assert.IsFalse(emptyResult.IsDefault);
		Assert.AreEqual(expected: 0, emptyResult.Length);
		Assert.AreSame(ancestor, result[0]);
		Assert.IsNull(result[1]);
	}

	[TestMethod]
	public void XPathSingleElementIteratorMovesOnceAndCloneStartsBeforeElement()
	{
		using var reader = XmlReader.Create(new StringReader("<root><child /></root>"));
		var document = new XPathDocument(reader);
		var navigator = document.CreateNavigator().SelectSingleNode("/root/child")!;
		var iterator = new XPathSingleElementIterator(navigator);

		Assert.AreEqual(expected: 0, iterator.CurrentPosition);
		Assert.IsNull(iterator.Current);
		Assert.IsTrue(iterator.MoveNext());
		Assert.AreEqual(expected: 1, iterator.CurrentPosition);
		Assert.AreEqual(expected: "child", iterator.Current!.LocalName);
		Assert.IsFalse(iterator.MoveNext());
		Assert.AreEqual(expected: 1, iterator.CurrentPosition);

		var clone = iterator.Clone();

		Assert.AreEqual(expected: 0, clone.CurrentPosition);
		Assert.IsTrue(clone.MoveNext());
		Assert.AreEqual(expected: "child", clone.Current!.LocalName);
	}

	private interface ITestAncestor;

	private sealed class TestAncestor(string value) : ITestAncestor
	{
		public string Value { get; } = value;
	}

	private sealed class AncestorSource(object ancestor) : IAncestorProvider
	{
	#region Interface IAncestorProvider

		public object Ancestor { get; } = ancestor;

	#endregion
	}
}