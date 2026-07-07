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
public class DataModelDateTimeTypeTest
{
	[TestMethod]
	public void DataModelDateTimeType_ShouldHaveDateTimeValue()
	{
		// Act
		var dateTimeType = DataModelDateTimeType.DateTime;

		// Assert
		Assert.AreEqual(DataModelDateTimeType.DateTime, dateTimeType);
	}

	[TestMethod]
	public void DataModelDateTimeType_ShouldHaveDateTimeOffsetValue()
	{
		// Act
		var dateTimeType = DataModelDateTimeType.DateTimeOffset;

		// Assert
		Assert.AreEqual(DataModelDateTimeType.DateTimeOffset, dateTimeType);
	}

	[TestMethod]
	public void DataModelDateTimeType_ValuesShouldBeDifferent()
	{
		// Act & Assert
		Assert.AreNotEqual(DataModelDateTimeType.DateTime, DataModelDateTimeType.DateTimeOffset);
	}

	[TestMethod]
	public void DataModelDateTimeType_EqualityComparison_ShouldWork()
	{
		// Act
		var type1 = DataModelDateTimeType.DateTimeOffset;
		var type2 = DataModelDateTimeType.DateTimeOffset;
		var type3 = DataModelDateTimeType.DateTime;

		// Assert
		Assert.IsTrue(type1 == type2);
		Assert.IsFalse(type1 == type3);
		Assert.IsFalse(type1 != type2);
		Assert.IsTrue(type1 != type3);
	}

	[TestMethod]
	public void DataModelDateTimeType_ToString_ShouldReturnValidString()
	{
		// Act
		var dateTimeStr = DataModelDateTimeType.DateTime.ToString();
		var dateTimeOffsetStr = DataModelDateTimeType.DateTimeOffset.ToString();

		// Assert
		Assert.AreEqual("DateTime", dateTimeStr);
		Assert.AreEqual("DateTimeOffset", dateTimeOffsetStr);
	}

	[TestMethod]
	public void DataModelDateTimeType_GetValues_ShouldReturnAllValues()
	{
		// Act
		var values = Enum.GetValues(typeof(DataModelDateTimeType));

		// Assert
		Assert.AreEqual(2, values.Length);
	}

	[TestMethod]
	public void DataModelDateTimeType_GetNames_ShouldReturnAllNames()
	{
		// Act
		var names = Enum.GetNames(typeof(DataModelDateTimeType));

		// Assert
		Assert.AreEqual(2, names.Length);
		Assert.IsTrue(names.Contains("DateTime"));
		Assert.IsTrue(names.Contains("DateTimeOffset"));
	}

	[TestMethod]
	public void DataModelDateTimeType_GetHashCode_ShouldBeConsistent()
	{
		// Arrange
		var type1 = DataModelDateTimeType.DateTime;

		// Act
		var hash1 = type1.GetHashCode();
		var hash2 = type1.GetHashCode();

		// Assert
		Assert.AreEqual(hash1, hash2);
	}

	[TestMethod]
	public void DataModelDateTimeType_CastToInt_ShouldWork()
	{
		// Act
		var dateTimeInt = (int) DataModelDateTimeType.DateTime;
		var dateTimeOffsetInt = (int) DataModelDateTimeType.DateTimeOffset;

		// Assert
		Assert.AreEqual(0, dateTimeInt);
		Assert.AreEqual(1, dateTimeOffsetInt);
	}
}
