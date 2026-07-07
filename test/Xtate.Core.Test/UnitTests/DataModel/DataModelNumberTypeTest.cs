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
public class DataModelNumberTypeTest
{
	[TestMethod]
	public void DataModelNumberType_ShouldHaveInt32Value()
	{
		// Act
		var numberType = DataModelNumberType.Int32;

		// Assert
		Assert.AreEqual(DataModelNumberType.Int32, numberType);
	}

	[TestMethod]
	public void DataModelNumberType_ShouldHaveInt64Value()
	{
		// Act
		var numberType = DataModelNumberType.Int64;

		// Assert
		Assert.AreEqual(DataModelNumberType.Int64, numberType);
	}

	[TestMethod]
	public void DataModelNumberType_ShouldHaveDoubleValue()
	{
		// Act
		var numberType = DataModelNumberType.Double;

		// Assert
		Assert.AreEqual(DataModelNumberType.Double, numberType);
	}

	[TestMethod]
	public void DataModelNumberType_ShouldHaveDecimalValue()
	{
		// Act
		var numberType = DataModelNumberType.Decimal;

		// Assert
		Assert.AreEqual(DataModelNumberType.Decimal, numberType);
	}

	[TestMethod]
	public void DataModelNumberType_AllValuesShouldBeDifferent()
	{
		// Act & Assert
		Assert.AreNotEqual(DataModelNumberType.Int32, DataModelNumberType.Int64);
		Assert.AreNotEqual(DataModelNumberType.Int64, DataModelNumberType.Double);
		Assert.AreNotEqual(DataModelNumberType.Double, DataModelNumberType.Decimal);
	}

	[TestMethod]
	public void DataModelNumberType_EqualityComparison_ShouldWork()
	{
		// Act
		var type1 = DataModelNumberType.Double;
		var type2 = DataModelNumberType.Double;
		var type3 = DataModelNumberType.Decimal;

		// Assert
		Assert.IsTrue(type1 == type2);
		Assert.IsFalse(type1 == type3);
		Assert.IsFalse(type1 != type2);
		Assert.IsTrue(type1 != type3);
	}

	[TestMethod]
	public void DataModelNumberType_ToString_ShouldReturnValidString()
	{
		// Act
		var int32Str = DataModelNumberType.Int32.ToString();
		var int64Str = DataModelNumberType.Int64.ToString();
		var doubleStr = DataModelNumberType.Double.ToString();
		var decimalStr = DataModelNumberType.Decimal.ToString();

		// Assert
		Assert.AreEqual("Int32", int32Str);
		Assert.AreEqual("Int64", int64Str);
		Assert.AreEqual("Double", doubleStr);
		Assert.AreEqual("Decimal", decimalStr);
	}

	[TestMethod]
	public void DataModelNumberType_GetValues_ShouldReturnAllValues()
	{
		// Act
		var values = Enum.GetValues(typeof(DataModelNumberType));

		// Assert
		Assert.AreEqual(4, values.Length);
	}

	[TestMethod]
	public void DataModelNumberType_GetNames_ShouldReturnAllNames()
	{
		// Act
		var names = Enum.GetNames(typeof(DataModelNumberType));

		// Assert
		Assert.AreEqual(4, names.Length);
		Assert.IsTrue(names.Contains("Int32"));
		Assert.IsTrue(names.Contains("Int64"));
		Assert.IsTrue(names.Contains("Double"));
		Assert.IsTrue(names.Contains("Decimal"));
	}

	[TestMethod]
	public void DataModelNumberType_GetHashCode_ShouldBeConsistent()
	{
		// Arrange
		var type1 = DataModelNumberType.Int64;

		// Act
		var hash1 = type1.GetHashCode();
		var hash2 = type1.GetHashCode();

		// Assert
		Assert.AreEqual(hash1, hash2);
	}
}
