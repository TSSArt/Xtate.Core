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

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class ExtCollectionCoverageTest
{
	[TestMethod]
	public async Task ExtDictionaryCompletesPendingLookupWhenValueIsAddedUpdatedOrRemoved()
	{
		var dictionary = new ExtDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		Assert.IsTrue(dictionary.IsEmpty);
		Assert.IsTrue(dictionary.TryAddPending("alpha"));
		Assert.IsFalse(dictionary.TryAddPending("ALPHA"));

		var pendingAdd = dictionary.TryGetValueAsync("ALPHA").AsTask();
		Assert.IsFalse(pendingAdd.IsCompleted);

		dictionary["Alpha"] = 1;

		var added = await pendingAdd;
		Assert.IsTrue(added.Found);
		Assert.AreEqual(expected: 1, added.Value);
		Assert.IsTrue(dictionary.TryGetValue(key: "alpha", out var current));
		Assert.AreEqual(expected: 1, current);

		Assert.IsTrue(dictionary.TryAddPending("beta"));
		var pendingAddOrUpdate = dictionary.TryGetValueAsync("BETA").AsTask();
		Assert.AreEqual(expected: 3, dictionary.AddOrUpdate(key: "beta", static (_, arg) => arg, static (_, value, arg) => value + arg, factoryArgument: 3));

		var addedByAddOrUpdate = await pendingAddOrUpdate;
		Assert.IsTrue(addedByAddOrUpdate.Found);
		Assert.AreEqual(expected: 3, addedByAddOrUpdate.Value);
		Assert.AreEqual(expected: 3, dictionary.AddOrUpdate(key: "ALPHA", static (_, arg) => arg, static (_, value, arg) => value + arg, factoryArgument: 2));

		Assert.IsTrue(dictionary.TryAddPending("alpha"));
		var pendingRemove = dictionary.TryGetValueAsync("missing").AsTask();
		Assert.IsTrue(dictionary.TryRemovePair(key: "alpha", value: 3));
		Assert.IsFalse(dictionary.IsEmpty);
		Assert.IsFalse(dictionary.TryRemove(key: "missing", out _));

		var removed = await pendingRemove;
		Assert.IsFalse(removed.Found);
		Assert.AreEqual(expected: 0, removed.Value);
		Assert.IsTrue(dictionary.TryRemove(key: "beta", out _));
		Assert.IsTrue(dictionary.IsEmpty);
	}

	[TestMethod]
	public void ExtDictionaryCoversGetOrAddUpdateOrRemoveTakeAndEnumeration()
	{
		var dictionary = new ExtDictionary<string, int>();

		Assert.AreEqual(expected: 10, dictionary.GetOrAdd(key: "one", static (_, arg) => arg, factoryArgument: 10));
		Assert.AreEqual(expected: 10, dictionary.GetOrAdd(key: "one", static (_, arg) => arg, factoryArgument: 20));
		Assert.AreEqual(expected: 15, dictionary.UpdateOrRemove(key: "one", static (_, value, _) => value < 0, static (_, value, arg) => value + arg, factoryArgument: 5));
		Assert.AreEqual(expected: 1, dictionary.Count);
		CollectionAssert.AreEqual(new[] { "one" }, dictionary.Keys.ToArray());
		CollectionAssert.AreEqual(new[] { 15 }, dictionary.Values.ToArray());
		CollectionAssert.AreEqual(new[] { "one=15" }, dictionary.Select(pair => $"{pair.Key}={pair.Value}").ToArray());

		Assert.AreEqual(expected: 0, dictionary.UpdateOrRemove(key: "one", static (_, value, arg) => value == arg, static (_, value, _) => value, factoryArgument: 15));
		Assert.IsFalse(dictionary.TryGetValue(key: "one", out _));

		dictionary.TryAdd(key: "take", value: 99);
		Assert.IsTrue(dictionary.TryTake(out var key, out var value));
		Assert.AreEqual(expected: "take", key);
		Assert.AreEqual(expected: 99, value);
		Assert.IsFalse(dictionary.TryTake(out _, out _));
		Assert.ThrowsExactly<KeyNotFoundException>([ExcludeFromCodeCoverage]() => _ = dictionary["missing"]);
	}

	[TestMethod]
	public void ExtCollectionSupportsGroupedValuesRemoveTakeAndEnumeration()
	{
		var collection = new ExtCollection<string, string>(StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);

		collection.Add(value1: "group", value2: "one");
		collection.Add(value1: "GROUP", value2: "two");
		collection.Add(value1: "other", value2: "three");

		Assert.AreEqual(expected: 3, collection.Count);
		CollectionAssert.AreEquivalent(new[] { "group=one", "group=two", "other=three" }, collection.Select(pair => $"{pair.Item1}={pair.Item2}").ToArray());

		Assert.IsTrue(collection.Remove(value1: "group", value2: "TWO"));
		Assert.AreEqual(expected: 2, collection.Count);
		Assert.IsFalse(collection.Remove(value1: "group", value2: "missing"));

		Assert.IsTrue(collection.TryRemoveGroup(value: "GROUP", out var groupValues));
		CollectionAssert.AreEqual(new[] { "one" }, groupValues.ToArray());
		Assert.AreEqual(expected: 1, collection.Count);
		Assert.IsFalse(collection.TryRemoveGroup(value: "missing", out _));

		Assert.IsTrue(collection.TryTake(out var key, out var value));
		Assert.AreEqual(expected: "other", key);
		Assert.AreEqual(expected: "three", value);
		Assert.AreEqual(expected: 0, collection.Count);
		Assert.IsFalse(collection.TryTake(out _, out _));
	}
}