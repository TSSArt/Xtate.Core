// Copyright © 2019-2024 Sergii Artemenko
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

using Xtate.Core;
using Xtate.Persistence;

namespace Xtate.Test;

[TestClass]
public class PersistedDataModelTest
{
	private Bucket _bucket;

	private DataModelList _dataModelList = default!;

	private DataModelListPersistingController _listController = default!;

	private DataModelList _restoredDataModelList = default!;

	private DataModelReferenceTracker _restoredTracker = default!;

	private InMemoryStorage _storage = default!;

	private DataModelReferenceTracker _tracker = default!;

	[TestInitialize]
	public void Initialize()
	{
		_storage = new InMemoryStorage(false);
		var bucket = new Bucket(_storage);
		_bucket = bucket.Nested("root");

		_tracker = new DataModelReferenceTracker(bucket.Nested("refs"));
		_restoredTracker = new DataModelReferenceTracker(bucket.Nested("refs"));

		_dataModelList = [];
		_listController = new DataModelListPersistingController(_bucket, _tracker, _dataModelList);

		_restoredDataModelList = [];

		_dataModelList = [];
		_listController = new DataModelListPersistingController(_bucket, _tracker, _dataModelList);

		_restoredDataModelList = [];
	}

	[TestCleanup]
	public void Finalization()
	{
		_tracker.Dispose();
		_listController.Dispose();
		_listController.Dispose();
	}

	[TestMethod]
	public void EmptyObjectTest()
	{
		using var controller = new DataModelListPersistingController(_bucket, _tracker, _restoredDataModelList);

		Assert.AreEqual(expected: 0, _restoredDataModelList.Count);
		Assert.AreEqual(expected: 0, _storage.GetTransactionLogSize());
	}

	[TestMethod]
	public void AddObjectTest()
	{
		_dataModelList["b"] = new DataModelValue("ee");

		using var controller = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);

		Assert.AreEqual(expected: 1, _restoredDataModelList.Count);
		Assert.AreEqual(expected: "ee", _restoredDataModelList["b"].AsString());

		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 4, StorageTest.GetEntriesCount(_storage));
	}

	[TestMethod]
	public void RemoveObjectTest()
	{
		_dataModelList["a"] = new DataModelValue("qq");
		_dataModelList["b"] = new DataModelValue("ee");

		_dataModelList.RemoveAll("b");

		using var controller = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);

		Assert.AreEqual(expected: 1, _restoredDataModelList.Count);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 15, StorageTest.GetEntriesCount(_storage));
	}

	[TestMethod]
	public void RemoveObjectAllTest()
	{
		_dataModelList["a"] = new DataModelValue("qq");
		_dataModelList["b"] = new DataModelValue("ee");

		_dataModelList.RemoveAll("a");
		_dataModelList.RemoveAll("b");

		using var controller = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);

		Assert.AreEqual(expected: 0, _restoredDataModelList.Count);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 13, StorageTest.GetEntriesCount(_storage));
	}

	[TestMethod]
	public void SubObjectTest()
	{
		var list = new DataModelList { ["t"] = new DataModelValue("test") };
		_dataModelList["a"] = new DataModelValue(list);

		using var controller = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);

		Assert.AreEqual(expected: "test", _restoredDataModelList["a"].AsList()["t"].AsString());
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 10, StorageTest.GetEntriesCount(_storage));
	}

	[TestMethod]
	public void AddArrayTest()
	{
		_dataModelList[0] = new DataModelValue("qq");

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 1, _restoredDataModelList.Count);
		Assert.AreEqual(expected: "qq", _restoredDataModelList[0].AsString());
	}

	[TestMethod]
	public void RemoveArrayTest()
	{
		_dataModelList[0] = new DataModelValue("qq");
		_dataModelList[1] = new DataModelValue("e");

		_dataModelList.RemoveAt(1);

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 1, _restoredDataModelList.Count);
		Assert.AreEqual(expected: "qq", _restoredDataModelList[0].AsString());
	}

	[TestMethod]
	public void RemoveArrayAllTest()
	{
		_dataModelList[0] = new DataModelValue("qq");
		_dataModelList[1] = new DataModelValue("e");

		_dataModelList.RemoveAt(0);
		_dataModelList.RemoveAt(0);

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 0, _restoredDataModelList.Count);
	}

	[TestMethod]
	public void AddDecimalTest()
	{
		_dataModelList[0] = new DataModelValue(1.2M);

		_dataModelList.RemoveAt(1);

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 1, _restoredDataModelList.Count);
		Assert.AreEqual(expected: 1.2, _restoredDataModelList[0].AsNumber());
	}

	[TestMethod]
	public void AddDoubleTest()
	{
		_dataModelList[0] = new DataModelValue(1.2);

		_dataModelList.RemoveAt(1);

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 1, _restoredDataModelList.Count);
		Assert.AreEqual(expected: 1.2, _restoredDataModelList[0].AsNumber());
	}

	[TestMethod]
	public void AddDateTimeTest()
	{
		_dataModelList[0] = new DataModelValue(new DateTime(year: 2000, month: 1, day: 1));

		_dataModelList.RemoveAt(1);

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 1, _restoredDataModelList.Count);
		Assert.AreEqual(new DateTime(year: 2000, month: 1, day: 1), _restoredDataModelList[0].AsDateTime().ToDateTime());
	}

	[TestMethod]
	public void AddBooleanTest()
	{
		_dataModelList[0] = new DataModelValue(true);

		_dataModelList.RemoveAt(1);

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 1, _restoredDataModelList.Count);
		Assert.AreEqual(expected: true, _restoredDataModelList[0].AsBoolean());
	}

	[TestMethod]
	public void AddStringTest()
	{
		_dataModelList[0] = new DataModelValue("test");

		_dataModelList.RemoveAt(1);

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 1, _restoredDataModelList.Count);
		Assert.AreEqual(expected: "test", _restoredDataModelList[0].AsString());
	}

	[TestMethod]
	public void AddSubObjectTest()
	{
		var list = new DataModelList
				   {
					   ["prop"] = new DataModelValue("value")
				   };

		_dataModelList[0] = new DataModelValue(list);

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 1, _restoredDataModelList.Count);
		Assert.AreEqual(expected: "value", _restoredDataModelList[0].AsList()["prop"].AsString());
	}

	[TestMethod]
	public void AddSubArrayTest()
	{
		var list = new DataModelList
				   {
					   [0] = new DataModelValue("value")
				   };

		_dataModelList[0] = new DataModelValue(list);

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 1, _restoredDataModelList.Count);
		Assert.AreEqual(expected: "value", _restoredDataModelList[0].AsList()[0].AsString());
	}

	[TestMethod]
	public void ArrayClearTest()
	{
		_dataModelList.Add(new DataModelValue(5));
		_dataModelList.Clear();

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 0, _restoredDataModelList.Count);
	}

	[TestMethod]
	public void ArrayInsertTest()
	{
		_dataModelList.Add(new DataModelValue(5));
		_dataModelList.Insert(index: 0, new DataModelValue(4));

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 2, _restoredDataModelList.Count);
		Assert.AreEqual(expected: 4, _restoredDataModelList[0].AsNumber());
		Assert.AreEqual(expected: 5, _restoredDataModelList[1].AsNumber());
	}

	[TestMethod]
	public void ArraySetLengthTest()
	{
		_dataModelList.Add(new DataModelValue(5));
		_dataModelList.SetLength(5);

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 5, _restoredDataModelList.Count);
		Assert.AreEqual(expected: 5, _restoredDataModelList[0].AsNumber());
		Assert.AreEqual(DataModelValueType.Undefined, _restoredDataModelList[4].Type);
	}

	[TestMethod]
	public void ArrayReadonlyStringPropTest()
	{
		var list = new DataModelList();

		list.SetInternal(key: "t", caseInsensitive: false, new DataModelValue(value: "test"), DataModelAccess.ReadOnly);

		_dataModelList["a"] = new DataModelValue(list);

		using var controller = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);

		Assert.IsTrue(_restoredDataModelList["a"].AsList().TryGet(key: "t", caseInsensitive: false, out var entry) && entry.Access == DataModelAccess.ReadOnly);
		Assert.AreEqual(expected: false, _restoredDataModelList["a"].AsList().CanSet(key: "t", caseInsensitive: false));
		Assert.AreEqual(expected: true, _restoredDataModelList.CanSet(key: "a", caseInsensitive: false));
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 12, StorageTest.GetEntriesCount(_storage));
	}

	[TestMethod]
	public void ArrayReadonlyObjectTest()
	{
		var list = new DataModelList
				   {
					   ["t"] = new DataModelValue("test")
				   };
		list.MakeReadOnly();
		_dataModelList["a"] = new DataModelValue(list);

		using var controller = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);

		Assert.IsTrue(_restoredDataModelList["a"].AsList().TryGet(key: "t", caseInsensitive: false, out var entry) && entry.Access == DataModelAccess.Writable);
		Assert.AreEqual(expected: false, _restoredDataModelList["a"].AsList().CanSet(key: "t", caseInsensitive: false));
		Assert.AreEqual(expected: true, _restoredDataModelList.CanSet(key: "a", caseInsensitive: false));
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 12, StorageTest.GetEntriesCount(_storage));
	}

	[TestMethod]
	public void ArrayReadonlyArrayTest()
	{
		var list = new DataModelList
				   {
					   [0] = new DataModelValue("test")
				   };
		list.MakeReadOnly();
		_dataModelList["a"] = new DataModelValue(list);

		using var controller = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);

		Assert.IsTrue(_restoredDataModelList["a"].AsList().TryGet(index: 0, out var entry) && entry.Access == DataModelAccess.Writable);
		Assert.AreEqual(expected: false, _restoredDataModelList["a"].AsList().CanSet(index: 0));
		Assert.AreEqual(expected: true, _restoredDataModelList.CanSet(key: "a", caseInsensitive: false));
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 12, StorageTest.GetEntriesCount(_storage));
	}

	[TestMethod]
	public void ArrayReadonlyObjectPropertyTest()
	{
		var list = new DataModelList
				   {
					   ["t"] = new DataModelValue("test")
				   };
		_dataModelList.SetInternal(key: "a", caseInsensitive: false, new DataModelValue(list), DataModelAccess.ReadOnly);

		using var controller = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);

		Assert.IsTrue(_restoredDataModelList["a"].AsList().TryGet(key: "t", caseInsensitive: false, out var entry) && entry.Access == DataModelAccess.Writable);
		Assert.AreEqual(expected: true, _restoredDataModelList["a"].AsList().CanSet(key: "t", caseInsensitive: false));
		Assert.AreEqual(expected: false, _restoredDataModelList.CanSet(key: "a", caseInsensitive: false));
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreEqual(expected: 11, StorageTest.GetEntriesCount(_storage));
	}

	[TestMethod]
	public void ReferencesNewObjectTest()
	{
		var obj1 = new DataModelList { ["prop1-rw"] = new DataModelValue("val1") };
		var root = new DataModelList { ["obj1"] = new DataModelValue(obj1) };
		_ = new DataModelListPersistingController(_bucket, _restoredTracker, root);
	}

	[TestMethod]
	public void ReferencesObjectTest()
	{
		var obj1 = new DataModelList
				   {
					   ["prop1-rw"] = new DataModelValue("val1")
				   };
		obj1.SetInternal(key: "prop1-ro", caseInsensitive: false, new DataModelValue("val1"), DataModelAccess.ReadOnly);

		var obj2 = new DataModelList
				   {
					   ["prop2-rw"] = new DataModelValue("val1")
				   };
		obj1.SetInternal(key: "prop2-ro", caseInsensitive: false, new DataModelValue("val1"), DataModelAccess.ReadOnly);

		_dataModelList["numeric-rw"] = new DataModelValue(11);
		obj1.SetInternal(key: "numeric-ro", caseInsensitive: false, new DataModelValue(22), DataModelAccess.ReadOnly);

		_dataModelList["obj1a"] = new DataModelValue(obj1);
		_dataModelList["obj1b"] = new DataModelValue(obj1);
		_dataModelList["obj2a"] = new DataModelValue(obj2);
		_dataModelList["obj2b"] = new DataModelValue(obj2);

		_dataModelList["obj1c"] = new DataModelValue(obj1);
		_dataModelList["obj1c"] = DataModelValue.Null;

		obj1["extra1"] = new DataModelValue("value-extra1");
		obj2["extra2"] = new DataModelValue("value-extra2");

		Assert.AreSame(obj1, _dataModelList["obj1a"].AsList());
		Assert.AreSame(obj1, _dataModelList["obj1b"].AsList());
		Assert.AreSame(obj2, _dataModelList["obj2a"].AsList());
		Assert.AreSame(obj2, _dataModelList["obj2b"].AsList());
		Assert.AreEqual(expected: "value-extra1", _dataModelList["obj1a"].AsList()["extra1"].AsString());
		Assert.AreEqual(expected: "value-extra1", _dataModelList["obj1b"].AsList()["extra1"].AsString());
		Assert.AreEqual(expected: "value-extra2", _dataModelList["obj2a"].AsList()["extra2"].AsString());
		Assert.AreEqual(expected: "value-extra2", _dataModelList["obj2b"].AsList()["extra2"].AsString());

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreSame(_restoredDataModelList["obj1a"].AsList(), _restoredDataModelList["obj1b"].AsList());
		Assert.AreSame(_restoredDataModelList["obj2a"].AsList(), _restoredDataModelList["obj2b"].AsList());
		Assert.AreEqual(expected: "value-extra1", _restoredDataModelList["obj1a"].AsList()["extra1"].AsString());
		Assert.AreEqual(expected: "value-extra1", _restoredDataModelList["obj1b"].AsList()["extra1"].AsString());
		Assert.AreEqual(expected: "value-extra2", _restoredDataModelList["obj2a"].AsList()["extra2"].AsString());
		Assert.AreEqual(expected: "value-extra2", _restoredDataModelList["obj2b"].AsList()["extra2"].AsString());
	}

	[TestMethod]
	public void ReferencesArrayTest()
	{
		var obj1 = new DataModelList
				   {
					   [1] = new DataModelValue("val1")
				   };
		obj1.SetInternal(index: 0, key: null, new DataModelValue("val1"), DataModelAccess.ReadOnly);

		var obj2 = new DataModelList
				   {
					   [1] = new DataModelValue("val1")
				   };
		obj1.SetInternal(index: 0, key: null, new DataModelValue("val1"), DataModelAccess.ReadOnly);

		_dataModelList[0] = new DataModelValue(11);
		obj1.SetInternal(index: 1, key: null, new DataModelValue(22), DataModelAccess.ReadOnly);
		_dataModelList[2] = new DataModelValue(obj1);
		_dataModelList[3] = new DataModelValue(obj1);
		_dataModelList[4] = new DataModelValue(obj2);
		_dataModelList[5] = new DataModelValue(obj2);

		obj1[2] = new DataModelValue("value-extra1");
		obj2[3] = new DataModelValue("value-extra2");

		_dataModelList[6] = new DataModelValue(obj1);
		_dataModelList[6] = DataModelValue.Null;

		Assert.AreSame(obj1, _dataModelList[2].AsList());
		Assert.AreSame(obj1, _dataModelList[3].AsList());
		Assert.AreSame(obj2, _dataModelList[4].AsList());
		Assert.AreSame(obj2, _dataModelList[5].AsList());
		Assert.AreEqual(expected: "value-extra1", _dataModelList[2].AsList()[2].AsString());
		Assert.AreEqual(expected: "value-extra1", _dataModelList[3].AsList()[2].AsString());
		Assert.AreEqual(expected: "value-extra2", _dataModelList[4].AsList()[3].AsString());
		Assert.AreEqual(expected: "value-extra2", _dataModelList[5].AsList()[3].AsString());

		_ = new DataModelListPersistingController(_bucket, _restoredTracker, _restoredDataModelList);
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
		Assert.AreSame(_restoredDataModelList[2].AsList(), _restoredDataModelList[3].AsList());
		Assert.AreSame(_restoredDataModelList[4].AsList(), _restoredDataModelList[5].AsList());
		Assert.AreEqual(expected: "value-extra1", _restoredDataModelList[2].AsList()[2].AsString());
		Assert.AreEqual(expected: "value-extra1", _restoredDataModelList[3].AsList()[2].AsString());
		Assert.AreEqual(expected: "value-extra2", _restoredDataModelList[4].AsList()[3].AsString());
		Assert.AreEqual(expected: "value-extra2", _restoredDataModelList[5].AsList()[3].AsString());
	}

	[TestMethod]
	public void ReferencesRemovedTest()
	{
		var obj1 = new DataModelList { ["prop1-rw"] = new DataModelValue("val1") };

		_dataModelList["obj1a"] = new DataModelValue(obj1);

		Assert.AreEqual(expected: "prop1-rw", new Bucket(_storage).Nested("refs").Nested(0).Nested(0).GetString(Key.Key));
		Assert.IsTrue(new Bucket(_storage).Nested("refs").Nested(0).Nested(0).TryGet(Key.Key, out _));
		_dataModelList["obj1a"] = default;
		Assert.IsFalse(new Bucket(_storage).Nested("refs").Nested(0).Nested(0).TryGet(Key.Key, out _));
		Console.WriteLine(StorageTest.Dump(_storage, Environment.NewLine, hex: true));
	}
}