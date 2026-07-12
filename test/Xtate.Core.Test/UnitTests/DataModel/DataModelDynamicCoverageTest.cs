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
		dynamic dyn = list.AsDynamic();

		dyn.Name = "value";
		dyn[1] = 42;
		dyn.SetLength(4);

		var metadata = new DataModelList { ["kind"] = "field" };
		dyn.SetMetadata("Name", metadata);

		object name = dyn.Name;
		object length = dyn.GetLength();
		object fieldMetadata = dyn.GetMetadata("Name");
		object missingMetadata = dyn.GetMetadata("missing");

		Assert.AreEqual("value", name);
		Assert.AreEqual(42, list[1].AsNumber().ToInt32());
		Assert.AreEqual(4, length);
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
		dynamic dyn = list.AsDynamic();
		
		dyn[1] = 42;

		object indexValue = dyn[1];

		Assert.AreEqual(42, indexValue);
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

		Assert.AreEqual("hello", title);
		Assert.AreEqual(9.5D, indexValue);
		Assert.AreEqual("hello", list["Title"].AsString());

		dynamic stringValue = new DataModelValue("text").AsDynamic();
		dynamic boolValue = new DataModelValue(true).AsDynamic();
		dynamic numberValue = new DataModelValue(123).AsDynamic();
		dynamic dateValue = new DataModelValue(new DateTime(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc)).AsDynamic();

		string convertedString = stringValue;
		bool convertedBool = boolValue;
		int convertedInt = numberValue;
		DateTime convertedDate = dateValue;

		Assert.AreEqual("text", convertedString);
		Assert.IsTrue(convertedBool);
		Assert.AreEqual(123, convertedInt);
		Assert.AreEqual(new DateTime(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc), convertedDate);
	}
}
