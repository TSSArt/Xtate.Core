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

using Xtate.DataTypes;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class DataModelListCoverageTest
{
	[TestMethod]
	public void EnumerablesExposeKeysValuesPairsEntriesAndSparseUndefinedSlots()
	{
		var list = new DataModelList();
		list.Add("alpha", "one");
		list.Add("beta", 2);
		list.Add(DataModelValue.Null);
		list.SetLength(5);

		CollectionAssert.AreEqual(new[] { "alpha", "beta" }, list.Keys.ToArray());
		CollectionAssert.AreEqual(new[] { "one", "2", "null", "undefined", "undefined" }, list.Values.Select(FormatValue).ToArray());
		CollectionAssert.AreEqual(new[] { "alpha=one", "beta=2" }, list.KeyValuePairs.Select(pair => $"{pair.Key}={FormatValue(pair.Value)}").ToArray());
		AssertSequence(new[] { "0:alpha:one", "1:beta:2", "2::null", "3::undefined", "0::undefined" }, list.Entries.Select(FormatEntry).ToArray());

		Assert.IsTrue(list.TryGet(index: 4, out var sparseEntry));
		Assert.AreEqual(DataModelValueType.Undefined, sparseEntry.Value.Type);
		Assert.IsFalse(list.TryGet(index: 5, out _));
	}

	[TestMethod]
	public void ByKeyEnumerablesFindCaseSensitiveAndCaseInsensitiveMatches()
	{
		var list = new DataModelList();
		list.Add("item", "lower");
		list.Add("ITEM", "upper");
		list.Add("other", "ignored");

		CollectionAssert.AreEqual(new[] { "lower" }, list.ListValues("item", caseInsensitive: false).Select(value => value.AsString()).ToArray());
		CollectionAssert.AreEqual(new[] { "lower", "upper" }, list.ListValues("item", caseInsensitive: true).Select(value => value.AsString()).ToArray());
		CollectionAssert.AreEqual(new[] { "item=lower", "ITEM=upper" }, list.ListKeyValues("item", caseInsensitive: true).Select(value => $"{value.Key}={value.Value.AsString()}").ToArray());
		CollectionAssert.AreEqual(new[] { "0:item:lower", "1:item:upper" }, list.ListEntries("item", caseInsensitive: true).Select(entry => $"{entry.Index}:{entry.Key}:{entry.Value.AsString()}").ToArray());
	}

	[TestMethod]
	public void RemoveFirstAndRemoveAllUseKeyLookupAndPreserveRemainingItems()
	{
		var list = new DataModelList();
		list.Add("dup", "first");
		list.Add("keep", "middle");
		list.Add("dup", "second");
		list.Add("DUP", "third");

		Assert.IsTrue(list.RemoveFirst("dup", caseInsensitive: false));
		CollectionAssert.AreEqual(new[] { "middle", "second", "third" }, list.Values.Select(value => value.AsString()).ToArray());

		Assert.IsTrue(list.RemoveAll("dup", caseInsensitive: true));
		Assert.AreEqual(1, list.Count);
		Assert.AreEqual("middle", list[0].AsString());
		Assert.IsFalse(list.RemoveAll("missing"));
	}

	[TestMethod]
	public void CloneAndAccessHelpersPreserveCaseSensitivityMetadataAndMutationRules()
	{
		var metadata = new DataModelList { ["kind"] = "meta" };
		var list = new DataModelList(caseInsensitive: true);
		list.Add("Name", "value", metadata);
		list.SetMetadata(new DataModelList { ["root"] = "metadata" });

		var clone = list.CloneAsReadOnly();

		Assert.IsTrue(clone.CaseInsensitive);
		Assert.IsTrue(clone.ContainsKey("name"));
		Assert.AreEqual("value", clone["name"].AsString());
		Assert.AreEqual("meta", clone.Entries.Single().Metadata!["kind"].AsString());
		Assert.AreEqual("metadata", clone.GetMetadata()!["root"].AsString());
		Assert.IsFalse(clone.CanSet("name", caseInsensitive: true));
		Assert.ThrowsExactly<InvalidOperationException>(() => clone["name"] = "changed");
	}

	private static string FormatEntry(DataModelList.Entry entry) => $"{entry.Index}:{entry.Key}:{FormatValue(entry.Value)}";

	private static void AssertSequence(string[] expected, string[] actual) =>
		Assert.IsTrue(expected.SequenceEqual(actual), $"Expected: [{string.Join(", ", expected)}]; Actual: [{string.Join(", ", actual)}]");

	private static string FormatValue(DataModelValue value) =>
		value.Type switch
		{
			DataModelValueType.String    => value.AsString(),
			DataModelValueType.Number    => value.AsNumber().ToString(),
			DataModelValueType.Null      => "null",
			DataModelValueType.Undefined => "undefined",
			_                            => value.Type.ToString().ToLowerInvariant()
		};
}
