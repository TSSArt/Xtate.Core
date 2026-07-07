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

namespace Xtate.Test;

[TestClass]
public class DataModelValueTypeTest
{
	[TestMethod]
	public void DataModelValueType_ShouldHaveUndefinedValue()
	{
		// Act
		var valueType = DataModelValueType.Undefined;

		// Assert
		Assert.AreEqual(DataModelValueType.Undefined, valueType);
	}

	[TestMethod]
	public void DataModelValueType_ShouldHaveNullValue()
	{
		// Act
		var valueType = DataModelValueType.Null;

		// Assert
		Assert.AreEqual(DataModelValueType.Null, valueType);
	}

	[TestMethod]
	public void DataModelValueType_ShouldHaveStringValue()
	{
		// Act
		var valueType = DataModelValueType.String;

		// Assert
		Assert.AreEqual(DataModelValueType.String, valueType);
	}

	[TestMethod]
	public void DataModelValueType_ShouldHaveBooleanValue()
	{
		// Act
		var valueType = DataModelValueType.Boolean;

		// Assert
		Assert.AreEqual(DataModelValueType.Boolean, valueType);
	}

	[TestMethod]
	public void DataModelValueType_ShouldHaveNumberValue()
	{
		// Act
		var valueType = DataModelValueType.Number;

		// Assert
		Assert.AreEqual(DataModelValueType.Number, valueType);
	}

	[TestMethod]
	public void DataModelValueType_ShouldHaveDateTimeValue()
	{
		// Act
		var valueType = DataModelValueType.DateTime;

		// Assert
		Assert.AreEqual(DataModelValueType.DateTime, valueType);
	}

	[TestMethod]
	public void DataModelValueType_ShouldHaveListValue()
	{
		// Act
		var valueType = DataModelValueType.List;

		// Assert
		Assert.AreEqual(DataModelValueType.List, valueType);
	}

	[TestMethod]
	public void DataModelValueType_AllValuesShouldBeDifferent()
	{
		// Act & Assert
		Assert.AreNotEqual(DataModelValueType.Undefined, DataModelValueType.Null);
		Assert.AreNotEqual(DataModelValueType.Null, DataModelValueType.String);
		Assert.AreNotEqual(DataModelValueType.String, DataModelValueType.Boolean);
		Assert.AreNotEqual(DataModelValueType.Boolean, DataModelValueType.Number);
		Assert.AreNotEqual(DataModelValueType.Number, DataModelValueType.DateTime);
		Assert.AreNotEqual(DataModelValueType.DateTime, DataModelValueType.List);
	}

	[TestMethod]
	public void DataModelValueType_EqualityComparison_ShouldWork()
	{
		// Act
		var type1 = DataModelValueType.String;
		var type2 = DataModelValueType.String;
		var type3 = DataModelValueType.Boolean;

		// Assert
		Assert.IsTrue(type1 == type2);
		Assert.IsFalse(type1 == type3);
		Assert.IsFalse(type1 != type2);
		Assert.IsTrue(type1 != type3);
	}

	[TestMethod]
	public void DataModelValueType_ToString_ShouldReturnValidString()
	{
		// Act
		var undefinedStr = DataModelValueType.Undefined.ToString();
		var nullStr = DataModelValueType.Null.ToString();
		var stringStr = DataModelValueType.String.ToString();

		// Assert
		Assert.AreEqual("Undefined", undefinedStr);
		Assert.AreEqual("Null", nullStr);
		Assert.AreEqual("String", stringStr);
	}

	[TestMethod]
	public void DataModelValueType_GetValues_ShouldReturnAllValues()
	{
		// Act
		var values = Enum.GetValues(typeof(DataModelValueType));

		// Assert
		Assert.AreEqual(7, values.Length);
	}

	[TestMethod]
	public void DataModelValueType_GetNames_ShouldReturnAllNames()
	{
		// Act
		var names = Enum.GetNames(typeof(DataModelValueType));

		// Assert
		Assert.AreEqual(7, names.Length);
		Assert.IsTrue(names.Contains("Undefined"));
		Assert.IsTrue(names.Contains("Null"));
		Assert.IsTrue(names.Contains("String"));
		Assert.IsTrue(names.Contains("Boolean"));
		Assert.IsTrue(names.Contains("Number"));
		Assert.IsTrue(names.Contains("DateTime"));
		Assert.IsTrue(names.Contains("List"));
	}

	[TestMethod]
	public void DataModelValueType_GetHashCode_ShouldBeConsistent()
	{
		// Arrange
		var type1 = DataModelValueType.String;

		// Act
		var hash1 = type1.GetHashCode();
		var hash2 = type1.GetHashCode();

		// Assert
		Assert.AreEqual(hash1, hash2);
	}
}
