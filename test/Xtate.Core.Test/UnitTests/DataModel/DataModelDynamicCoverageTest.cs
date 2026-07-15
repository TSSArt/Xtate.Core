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
public class DataModelDynamicCoverageTest
{
	[TestMethod]
	public void DataModelListDynamicSupportsExactMemberIndexLengthMetadataAndConversions()
	{
		var list = new DataModelList(caseInsensitive: true);
		var dyn = list.AsDynamic();

		dyn.Name = "value";
		dyn[1] = 42;
		dyn.SetLength(4);

		var metadata = new DataModelList { ["kind"] = "field" };
		dyn.SetMetadata("Name", metadata);

		object name = dyn.Name;
		object length = dyn.GetLength();
		object fieldMetadata = dyn.GetMetadata("Name");
		object missingMetadata = dyn.GetMetadata("missing");

		Assert.AreEqual(expected: "value", name);
		Assert.AreEqual(expected: 42, list[1].AsNumber().ToInt32());
		Assert.AreEqual(expected: 4, length);
		Assert.AreSame(metadata, fieldMetadata);
		Assert.IsNull(missingMetadata);

		DataModelList convertedList = dyn;
		DataModelValue convertedValue = dyn;

		Assert.AreSame(list, convertedList);
		Assert.AreSame(list, convertedValue.AsList());
	}

	[TestMethod]
	public void DataModelListDynamicIndexedGetReturnsStoredValue()
	{
		var list = new DataModelList();
		var dyn = list.AsDynamic();

		dyn[1] = 42;

		object indexValue = dyn[1];

		Assert.AreEqual(expected: 42, indexValue);
	}

	[TestMethod]
	public void DataModelListDynamicSupportsMemberIndexLengthMetadataAndConversions()
	{
		var list = new DataModelList(caseInsensitive: true);
		dynamic dyn = list;

		dyn.Name = "value";
		dyn[1] = 42;
		dyn.SetLength(4);

		var metadata = new DataModelList { ["kind"] = "field" };
		dyn.SetMetadata("name", metadata);

		Assert.AreEqual("value", dyn.name);
		Assert.AreEqual(42, dyn[1]);
		Assert.AreEqual(4, dyn.GetLength());
		Assert.AreSame(metadata, dyn.GetMetadata("NAME"));
		Assert.IsNull(dyn.GetMetadata("missing"));

		DataModelList convertedList = dyn;
		DataModelValue convertedValue = dyn;

		Assert.AreSame(list, convertedList);
		Assert.AreSame(list, convertedValue.AsList());
	}

	[TestMethod]
	public void DataModelValueDynamicDelegatesToListAndConvertsScalarValues()
	{
		var list = new DataModelList();
		var value = new DataModelValue(list);
		dynamic dyn = value;

		dyn.Title = "hello";
		dyn[1] = 9.5D;

		object title = dyn.Title;
		object indexValue = dyn[1];

		Assert.AreEqual(expected: "hello", title);
		Assert.AreEqual(expected: 9.5D, indexValue);
		Assert.AreEqual(expected: "hello", list["Title"].AsString());

		var stringValue = new DataModelValue("text").AsDynamic();
		var boolValue = new DataModelValue(true).AsDynamic();
		var numberValue = new DataModelValue(123).AsDynamic();
		var dateValue = new DataModelValue(new DateTime(year: 2026, month: 7, day: 11, hour: 12, minute: 0, second: 0, DateTimeKind.Utc)).AsDynamic();

		string convertedString = stringValue;
		bool convertedBool = boolValue;
		int convertedInt = numberValue;
		DateTime convertedDate = dateValue;

		Assert.AreEqual(expected: "text", convertedString);
		Assert.IsTrue(convertedBool);
		Assert.AreEqual(expected: 123, convertedInt);
		Assert.AreEqual(new DateTime(year: 2026, month: 7, day: 11, hour: 12, minute: 0, second: 0, DateTimeKind.Utc), convertedDate);
	}

	[TestMethod]
	public void DynamicListConvertsNumericIndexesAndSupportsAllMetadataOverloads()
	{
		var list = new DataModelList { "zero", "one" };
		dynamic dyn = list;
		var rootMetadata = new DataModelList { ["scope"] = "root" };
		var indexMetadata = new DataModelList { ["scope"] = "index" };
		var keyMetadata = new DataModelList { ["scope"] = "key" };

		dyn.SetLength((short) 3);
		dyn[(short) 2] = "two";
		dyn.SetMetadata((short) 1, indexMetadata);
		dyn.SetMetadata("named", keyMetadata);
		list.SetMetadata(rootMetadata);

		object convertedIndex = dyn[(short) 2];
		object length = dyn.GetLength();
		object metadataByConvertedIndex = dyn.GetMetadata((short) 1);
		object metadataByIntIndex = dyn.GetMetadata(1);
		object metadataByKey = dyn.GetMetadata("named");
		object allMetadata = dyn.GetMetadata();

		Assert.AreEqual("two", convertedIndex);
		Assert.AreEqual(expected: 4, length);
		Assert.AreSame(indexMetadata, metadataByConvertedIndex);
		Assert.AreSame(indexMetadata, metadataByIntIndex);
		Assert.AreSame(keyMetadata, metadataByKey);
		Assert.AreSame(rootMetadata, allMetadata);
		Assert.AreEqual("one", list[1].AsString());
		Assert.IsTrue(list.ContainsKey("named"));
	}

	[TestMethod]
	public void DynamicListReportsWrongIndexCountAndUnknownMethod()
	{
		dynamic dyn = new DataModelList { 1, 2 };

		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => _ = dyn[0, 1]);
		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => dyn[0, 1] = 3);
		Assert.ThrowsExactly<MissingMethodException>([ExcludeFromCodeCoverage]() => dyn.UnknownMethod());
	}

}
