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

using System.Collections;
using System.Globalization;
using System.Reflection;
using Xtate.DataModel.Services;
using Xtate.DataTypes;

namespace Xtate.Core.Test.UnitTests.DataModel;

[TestClass]
public class DataModelListCoverageTest
{
	[TestMethod]
	public void EnumerablesExposeKeysValuesPairsEntriesAndSparseUndefinedSlots()
	{
		var list = new DataModelList
				   {
					   { "alpha", "one" },
					   { "beta", 2 },
					   DataModelValue.Null
				   };
		list.SetLength(5);

		CollectionAssert.AreEqual(new[] { "alpha", "beta" }, list.Keys.ToArray());
		CollectionAssert.AreEqual(new[] { "one", "2", "null", "undefined", "undefined" }, list.Values.Select(FormatValue).ToArray());
		CollectionAssert.AreEqual(new[] { "alpha=one", "beta=2" }, list.KeyValuePairs.Select(pair => $"{pair.Key}={FormatValue(pair.Value)}").ToArray());
		AssertSequence(["0:alpha:one", "1:beta:2", "2::null", "3::undefined", "0::undefined"], [.. list.Entries.Select(FormatEntry)]);

		Assert.IsTrue(list.TryGet(index: 4, out var sparseEntry));
		Assert.AreEqual(DataModelValueType.Undefined, sparseEntry.Value.Type);
		Assert.IsFalse(list.TryGet(index: 5, out _));
	}

	[TestMethod]
	public void ByKeyEnumerablesFindCaseSensitiveAndCaseInsensitiveMatches()
	{
		var list = new DataModelList
				   {
					   { "item", "lower" },
					   { "ITEM", "upper" },
					   { "other", "ignored" }
				   };

		CollectionAssert.AreEqual(new[] { "lower" }, list.ListValues(key: "item", caseInsensitive: false).Select(value => value.AsString()).ToArray());
		CollectionAssert.AreEqual(new[] { "lower", "upper" }, list.ListValues(key: "item", caseInsensitive: true).Select(value => value.AsString()).ToArray());
		CollectionAssert.AreEqual(new[] { "item=lower" }, list.ListKeyValues(key: "item", caseInsensitive: false).Select(value => $"{value.Key}={value.Value.AsString()}").ToArray());
		CollectionAssert.AreEqual(new[] { "item=lower", "ITEM=upper" }, list.ListKeyValues(key: "item", caseInsensitive: true).Select(value => $"{value.Key}={value.Value.AsString()}").ToArray());
		CollectionAssert.AreEqual(new[] { "0:item:lower" }, list.ListEntries(key: "item", caseInsensitive: false).Select(entry => $"{entry.Index}:{entry.Key}:{entry.Value.AsString()}").ToArray());
		CollectionAssert.AreEqual(
			new[] { "0:item:lower", "1:item:upper" }, list.ListEntries(key: "item", caseInsensitive: true).Select(entry => $"{entry.Index}:{entry.Key}:{entry.Value.AsString()}").ToArray());
	}

	[TestMethod]
	public void NullKeyEnumeratorsTraverseStoredAndSparseIndexesAndSupportReset()
	{
		var list = new DataModelList { "first", "second" };
		list.SetLength(3);

		using var values = list.ListValues(null!, caseInsensitive: true).GetEnumerator();
		Assert.AreEqual(DataModelValueType.Undefined, values.Current.Type);
		Assert.IsTrue(values.MoveNext());
		Assert.AreEqual(expected: "first", values.Current.AsString());
		Assert.AreEqual(expected: "first", ((DataModelValue)((IEnumerator)values).Current).AsString());
		Assert.IsTrue(values.MoveNext());
		Assert.AreEqual(expected: "second", values.Current.AsString());
		Assert.IsTrue(values.MoveNext());
		Assert.AreEqual(DataModelValueType.Undefined, values.Current.Type);
		Assert.IsFalse(values.MoveNext());
		values.Reset();
		Assert.IsTrue(values.MoveNext());

		using var keyValues = list.ListKeyValues(null!, caseInsensitive: true).GetEnumerator();
		Assert.IsTrue(keyValues.MoveNext());
		Assert.IsNull(keyValues.Current.Key);
		Assert.IsNull(((DataModelList.KeyValue)((IEnumerator)keyValues).Current).Key);
		Assert.IsTrue(keyValues.MoveNext());
		Assert.IsNull(keyValues.Current.Key);
		Assert.IsTrue(keyValues.MoveNext());
		Assert.IsNull(keyValues.Current.Key);
		Assert.IsFalse(keyValues.MoveNext());
		keyValues.Reset();
		Assert.IsTrue(keyValues.MoveNext());

		using var entries = list.ListEntries(null!, caseInsensitive: true).GetEnumerator();
		Assert.IsTrue(entries.MoveNext());
		Assert.IsNull(entries.Current.Key);
		Assert.IsNull(((DataModelList.Entry)((IEnumerator)entries).Current).Key);
		Assert.IsTrue(entries.MoveNext());
		Assert.IsNull(entries.Current.Key);
		Assert.IsTrue(entries.MoveNext());
		Assert.AreEqual(DataModelValueType.Undefined, entries.Current.Value.Type);
		Assert.IsFalse(entries.MoveNext());
		entries.Reset();
		Assert.IsTrue(entries.MoveNext());

		var sparse = new DataModelList();
		sparse.SetLength(2);

		CollectionAssert.AreEqual(
			new[] { DataModelValueType.Undefined, DataModelValueType.Undefined },
			sparse.ListValues(null!, caseInsensitive: true).Select(value => value.Type).ToArray());
		Assert.IsTrue(sparse.ListKeyValues(null!, caseInsensitive: true).All(item => item.Key is null && item.Value.Type == DataModelValueType.Undefined));
		Assert.IsTrue(sparse.ListEntries(null!, caseInsensitive: true).All(item => item.Key is null && item.Value.Type == DataModelValueType.Undefined));
	}

	[TestMethod]
	public void RemoveFirstAndRemoveAllUseKeyLookupAndPreserveRemainingItems()
	{
		var list = new DataModelList
				   {
					   { "dup", "first" },
					   { "keep", "middle" },
					   { "dup", "second" },
					   { "DUP", "third" }
				   };

		Assert.IsTrue(list.RemoveFirst(key: "dup", caseInsensitive: false));
		CollectionAssert.AreEqual(new[] { "middle", "second", "third" }, list.Values.Select(value => value.AsString()).ToArray());

		Assert.IsTrue(list.RemoveAll(key: "dup", caseInsensitive: true));
		Assert.AreEqual(expected: 1, list.Count);
		Assert.AreEqual(expected: "middle", list[0].AsString());
		Assert.IsFalse(list.RemoveAll("missing"));
	}

	[TestMethod]
	public void EntryAndKeyValueSupportEqualityHashingOperatorsAndAllDistinctFields()
	{
		var metadata = new DataModelList { ["kind"] = "metadata" };
		var value = new DataModelValue("value");
		var entry = new DataModelList.Entry(index: 1, key: "key", value, DataModelAccess.Writable, metadata);
		var sameEntry = new DataModelList.Entry(index: 1, key: "key", value, DataModelAccess.Writable, metadata);

		Assert.IsTrue(entry.Equals(sameEntry));
		Assert.IsTrue(entry.Equals((object)sameEntry));
		Assert.AreEqual(entry.GetHashCode(), sameEntry.GetHashCode());
		Assert.IsTrue(entry == sameEntry);
		Assert.IsFalse(entry != sameEntry);
		Assert.IsFalse(entry.Equals(new DataModelList.Entry(index: 2, key: "key", value, DataModelAccess.Writable, metadata)));
		Assert.IsFalse(entry.Equals(new DataModelList.Entry(index: 1, key: "other", value, DataModelAccess.Writable, metadata)));
		Assert.IsFalse(entry.Equals(new DataModelList.Entry(index: 1, key: "key", value, DataModelAccess.ReadOnly, metadata)));
		Assert.IsFalse(entry.Equals(new DataModelList.Entry(index: 1, key: "key", value, DataModelAccess.Writable, [])));
		Assert.IsFalse(entry.Equals(new DataModelList.Entry(index: 1, key: "key", new DataModelValue("other"), DataModelAccess.Writable, metadata)));
		Assert.IsFalse(entry.Equals(TestHelper.Opaque<object>("not an entry")));
		Assert.IsFalse(entry.Equals(obj: null));

		var keyValue = new DataModelList.KeyValue(key: "key", value);
		var sameKeyValue = new DataModelList.KeyValue(key: "key", value);

		Assert.IsTrue(keyValue.Equals(sameKeyValue));
		Assert.IsTrue(keyValue.Equals((object)sameKeyValue));
		Assert.AreEqual(keyValue.GetHashCode(), sameKeyValue.GetHashCode());
		Assert.IsTrue(keyValue == sameKeyValue);
		Assert.IsFalse(keyValue != sameKeyValue);
		Assert.IsFalse(keyValue.Equals(new DataModelList.KeyValue(key: "other", value)));
		Assert.IsFalse(keyValue.Equals(new DataModelList.KeyValue(key: "key", new DataModelValue("other"))));
		Assert.IsFalse(keyValue.Equals(TestHelper.Opaque<object>("not a key value")));
		Assert.IsFalse(keyValue.Equals(obj: null));
	}

	[TestMethod]
	public void KeyValuePairEnumeratorSupportsNonGenericCurrentAndReset()
	{
		var list = new DataModelList { ["first"] = 1, ["second"] = 2 };
		using var enumerator = list.KeyValuePairs.GetEnumerator();

		Assert.IsTrue(enumerator.MoveNext());
		Assert.AreEqual(expected: "first", enumerator.Current.Key);
		Assert.AreEqual(expected: "first", ((KeyValuePair<string, DataModelValue>)((IEnumerator)enumerator).Current).Key);
		enumerator.Reset();
		Assert.IsNull(enumerator.Current.Key);
		Assert.IsTrue(enumerator.MoveNext());
		Assert.AreEqual(expected: "first", enumerator.Current.Key);
	}

	[TestMethod]
	public void EnumeratorsSupportDirectAndNonGenericEntryPointsCurrentAndReset()
	{
		var list = new DataModelList { ["key"] = "value" };

		using var keys = list.Keys.GetEnumerator();
		Assert.IsTrue(keys.MoveNext());
		Assert.AreEqual(expected: "key", keys.Current);
		Assert.AreEqual(expected: "key", ((IEnumerator)keys).Current);
		keys.Reset();
		Assert.IsTrue(keys.MoveNext());
		Assert.AreEqual(expected: "key", GetFirst(list.Keys));

		using var values = list.Values.GetEnumerator();
		Assert.IsTrue(values.MoveNext());
		Assert.AreEqual(expected: "value", values.Current.AsString());
		Assert.AreEqual(expected: "value", ((DataModelValue)((IEnumerator)values).Current).AsString());
		values.Reset();
		Assert.IsTrue(values.MoveNext());
		Assert.AreEqual(expected: "value", ((DataModelValue)GetFirst(list.Values)).AsString());

		using var keyValues = list.KeyValues.GetEnumerator();
		Assert.IsTrue(keyValues.MoveNext());
		Assert.AreEqual(expected: "key", ((DataModelList.KeyValue)((IEnumerator)keyValues).Current).Key);
		keyValues.Reset();
		Assert.IsTrue(keyValues.MoveNext());
		Assert.AreEqual(expected: "key", ((DataModelList.KeyValue)GetFirst(list.KeyValues)).Key);

		using var entries = list.Entries.GetEnumerator();
		Assert.IsTrue(entries.MoveNext());
		Assert.AreEqual(expected: "key", ((DataModelList.Entry)((IEnumerator)entries).Current).Key);
		entries.Reset();
		Assert.IsTrue(entries.MoveNext());
		Assert.AreEqual(expected: "key", ((DataModelList.Entry)GetFirst(list.Entries)).Key);

		Assert.AreEqual(expected: "value", ((DataModelValue)GetFirst(list.ListValues(key: "key", caseInsensitive: false))).AsString());
		Assert.AreEqual(expected: "key", ((DataModelList.KeyValue)GetFirst(list.ListKeyValues(key: "key", caseInsensitive: false))).Key);
		Assert.AreEqual(expected: "key", ((DataModelList.Entry)GetFirst(list.ListEntries(key: "key", caseInsensitive: false))).Key);
		Assert.AreEqual(expected: "key", ((KeyValuePair<string, DataModelValue>)GetFirst(list.KeyValuePairs)).Key);
	}

	[TestMethod]
	public void ListOperationsCopyFindSliceAndRemoveValues()
	{
		var list = new DataModelList { "first", "second", "third" };
		var copy = new DataModelValue[3];

		list.CopyTo(copy, index: 0);

		CollectionAssert.AreEqual(new[] { "first", "second", "third" }, copy.Select(value => value.AsString()).ToArray());
		Assert.IsTrue(list.Contains("second"));
		Assert.IsFalse(list.Contains("missing"));
		Assert.AreEqual(expected: 1, list.IndexOf("second"));
		Assert.AreEqual(expected: -1, list.IndexOf("missing"));
		CollectionAssert.AreEqual(new[] { "second", "third" }, list.Slice(start: 1, length: 2).Select(value => value.AsString()).ToArray());
		Assert.AreEqual(expected: 0, list.Slice(start: 3, length: 0).Length);
		Assert.IsTrue(list.Remove("second"));
		Assert.IsFalse(list.Remove("missing"));
		Assert.IsFalse(list.IsReadOnly);
		Assert.IsTrue(list.CloneAsReadOnly().IsReadOnly);
	}

	[TestMethod]
	public void CloneAndAccessHelpersPreserveCaseSensitivityMetadataAndMutationRules()
	{
		var metadata = new DataModelList { ["kind"] = "meta" };
		var list = new DataModelList(caseInsensitive: true)
				   {
					   { "Name", "value", metadata }
				   };
		list.SetMetadata(new DataModelList { ["root"] = "metadata" });

		var clone = list.CloneAsReadOnly();

		Assert.IsTrue(clone.CaseInsensitive);
		Assert.IsTrue(clone.ContainsKey("name"));
		Assert.AreEqual(expected: "value", clone["name"].AsString());
		Assert.AreEqual(expected: "value", clone.Values.Single().AsString());
		Assert.AreEqual(expected: "meta", clone.Entries.Single().Metadata!["kind"].AsString());
		Assert.AreEqual(expected: "metadata", clone.GetMetadata()!["root"].AsString());
		Assert.IsFalse(clone.CanSet(key: "name", caseInsensitive: true));
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => clone["name"] = "changed");
	}

	[TestMethod]
	public void WritableCloneCapabilitiesIndexedMetadataArrayFormattingAndConvenienceRemovalAreCovered()
	{
		var metadata = new DataModelList { ["kind"] = "indexed" };
		var array = new DataModelList { "one", "two" };
		array.SetMetadata(index: 0, metadata);

		Assert.AreSame(metadata, array.GetMetadata(index: 0));
		Assert.IsNull(array.GetMetadata(index: 3));
		Assert.AreEqual(expected: "[one,two]", array.ToString(format: null, CultureInfo.InvariantCulture));

		var writable = array.CloneAsWritable();
		Assert.AreNotSame(array, writable);
		Assert.IsTrue(writable.CanAdd());
		Assert.IsTrue(writable.CanClear());
		Assert.IsTrue(writable.CanInsert(index: 1));
		Assert.IsTrue(writable.CanSetLength(length: 4));
		Assert.IsTrue(writable.CanSetMetadata());
		Assert.IsTrue(writable.ClearInternal(throwOnDeny: false));
		Assert.AreEqual(expected: 0, writable.Count);

		var keyed = new DataModelList { ["remove"] = "first", ["keep"] = "second" };
		Assert.IsTrue(keyed.RemoveFirst("remove"));
		Assert.IsFalse(keyed.RemoveFirst("missing"));
		Assert.AreEqual(expected: "second", keyed["keep"].AsString());
	}

	[TestMethod]
	public void PrivateAdaptersCreateEmptyAndSizedArraysAndRejectUnavailableAccess()
	{
		var listType = typeof(DataModelList);
		var argsType = listType.GetNestedType(name: "Args", BindingFlags.NonPublic)!;

		foreach (var adapterName in new[] { "ValueAdapter", "KeyValueAdapter", "MetaValueAdapter", "KeyMetaValueAdapter" })
		{
			var adapter = listType.GetField($"{adapterName}Instance", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(obj: null)!;
			var createArray = adapter.GetType().GetMethod(name: "CreateArray", BindingFlags.Instance | BindingFlags.Public)!;
			object?[] emptyArguments = [Activator.CreateInstance(argsType), 0];
			object?[] sizedArguments = [Activator.CreateInstance(argsType), 2];

			Assert.AreEqual(expected: 0, ((Array)createArray.Invoke(adapter, emptyArguments)!).Length);
			Assert.AreEqual(expected: 2, ((Array)createArray.Invoke(adapter, sizedArguments)!).Length);
		}

		foreach (var adapterName in new[] { "ValueAdapter", "KeyValueAdapter" })
		{
			var adapter = listType.GetField($"{adapterName}Instance", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(obj: null)!;
			var getAccess = adapter.GetType().GetMethod(name: "GetAccessByIndex", BindingFlags.Instance | BindingFlags.Public)!;

			try
			{
				_ = getAccess.Invoke(adapter, [Activator.CreateInstance(argsType)]);
				Assert.Fail($"{adapterName} unexpectedly exposed item access metadata.");
			}
			catch (TargetInvocationException exception)
			{
				Assert.IsNotNull(exception.InnerException);
			}
		}
	}

	[TestMethod]
	public void ListBoundaryValidationMetadataAndFormattingBranchesAreCovered()
	{
		var list = new DataModelList { "one", "two" };
		Assert.ThrowsExactly<ArgumentNullException>(() => list.CopyTo(null!, index: 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => list.CopyTo(new DataModelValue[2], index: -1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => list.CopyTo(new DataModelValue[2], index: 2));
		var destination = new DataModelValue[3];
		list.CopyTo(destination, index: 1);
		Assert.AreEqual(DataModelValue.Undefined, destination[0]);
		Assert.AreEqual((DataModelValue)"one", destination[1]);
		Assert.AreEqual((DataModelValue)"two", destination[2]);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => list.Slice(start: -1, length: 1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => list.Slice(start: 3, length: 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => list.Slice(start: 1, length: 2));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => list.TryGet(index: -1, out _));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => list.SetLength(length: -1));
		Assert.ThrowsExactly<ArgumentNullException>(() => list.Add(null!, value: "value"));

		Assert.IsFalse(new DataModelList().HasKeys);
		Assert.IsFalse(list.HasKeys);
		Assert.IsTrue(new DataModelList { ["key"] = "value" }.HasKeys);

		var metadata = new DataModelList { ["kind"] = "missing index" };
		var missingMetadata = new DataModelList();
		missingMetadata.SetMetadata(index: 3, metadata);
		Assert.AreSame(metadata, missingMetadata.GetMetadata(index: 0));

		var constant = list.AsConstant();
		Assert.ThrowsExactly<InvalidOperationException>(() => constant.Access = DataModelAccess.Writable);

		Assert.AreEqual(expected: "{}", DataModelConverter.CreateAsArray().ToString());
		Assert.IsFalse(DataModelConverter.CreateAsArray().TryFormat([], out var charsWritten, format: default, CultureInfo.InvariantCulture));
		Assert.AreEqual(expected: 0, charsWritten);
		var oneValueArray = DataModelConverter.CreateAsArray();
		oneValueArray.Add("x");
		Assert.IsFalse(oneValueArray.TryFormat(new char[1], out charsWritten, format: default, CultureInfo.InvariantCulture));
		Assert.AreEqual(expected: 1, charsWritten);
		Assert.IsFalse(oneValueArray.TryFormat(new char[2], out charsWritten, format: default, CultureInfo.InvariantCulture));
		Assert.AreEqual(expected: 2, charsWritten);
		var twoValueArray = DataModelConverter.CreateAsArray();
		twoValueArray.Add("x");
		twoValueArray.Add("y");
		Assert.IsFalse(twoValueArray.TryFormat(new char[2], out charsWritten, format: default, CultureInfo.InvariantCulture));
		Assert.AreEqual(expected: 2, charsWritten);
	}

	[TestMethod]
	public void CopyToRejectsDestinationThatCannotHoldTheList()
	{
		var list = new DataModelList { "one", "two" };

		Assert.ThrowsExactly<ArgumentException>(() => list.CopyTo(new DataModelValue[1], index: 0));
	}

	private static string FormatEntry(DataModelList.Entry entry) => $"{entry.Index}:{entry.Key}:{FormatValue(entry.Value)}";

	private static object GetFirst(IEnumerable enumerable)
	{
		var enumerator = enumerable.GetEnumerator();
		using var enumeratorScope = enumerator as IDisposable;
		Assert.IsTrue(enumerator.MoveNext());

		return enumerator.Current!;
	}

	private static void AssertSequence(string[] expected, string[] actual) =>
		Assert.IsTrue(expected.SequenceEqual(actual), $"Expected: [{string.Join(separator: ", ", expected)}]; Actual: [{string.Join(separator: ", ", actual)}]");

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
