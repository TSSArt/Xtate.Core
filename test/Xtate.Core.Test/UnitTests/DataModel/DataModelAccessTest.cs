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

namespace Xtate.Core.Test.UnitTests.DataModel;

[TestClass]
[SuppressMessage(category: "ReSharper", checkId: "ConvertToConstant.Local")]
public class DataModelAccessTest
{
	[TestMethod]
	public void DataModelAccess_ShouldHaveWritableValue()
	{
		// Act
		var access = DataModelAccess.Writable;

		// Assert
		Assert.AreEqual(DataModelAccess.Writable, access);
	}

	[TestMethod]
	public void DataModelAccess_ShouldHaveReadOnlyValue()
	{
		// Act
		var access = DataModelAccess.ReadOnly;

		// Assert
		Assert.AreEqual(DataModelAccess.ReadOnly, access);
	}

	[TestMethod]
	public void DataModelAccess_ShouldHaveConstantValue()
	{
		// Act
		var access = DataModelAccess.Constant;

		// Assert
		Assert.AreEqual(DataModelAccess.Constant, access);
	}

	[TestMethod]
	public void DataModelAccess_AllValuesShouldBeDifferent()
	{
		Assert.AreEqual(expected: 3, Enum.GetValues(typeof(DataModelAccess)).Cast<DataModelAccess>().Distinct().Count());
	}

	[TestMethod]
	[DataRow(DataModelAccess.ReadOnly, DataModelAccess.ReadOnly, true)]
	[DataRow(DataModelAccess.ReadOnly, DataModelAccess.Writable, false)]
	public void DataModelAccess_EqualityComparison_ShouldWork(DataModelAccess left, DataModelAccess right, bool expectedEqual)
	{
		Assert.AreEqual(expectedEqual, left == right);
		Assert.AreEqual(!expectedEqual, left != right);
	}

	[TestMethod]
	public void DataModelAccess_ToString_ShouldReturnValidString()
	{
		// Act
		var writableStr = TestHelper.EnumName(DataModelAccess.Writable);
		var readOnlyStr = TestHelper.EnumName(DataModelAccess.ReadOnly);
		var constantStr = TestHelper.EnumName(DataModelAccess.Constant);

		// Assert
		Assert.AreEqual(nameof(DataModelAccess.Writable), writableStr);
		Assert.AreEqual(nameof(DataModelAccess.ReadOnly), readOnlyStr);
		Assert.AreEqual(nameof(DataModelAccess.Constant), constantStr);
	}

	[TestMethod]
	public void DataModelAccess_GetValues_ShouldReturnAllValues()
	{
		// Act
		var values = Enum.GetValues(typeof(DataModelAccess));

		// Assert
		Assert.AreEqual(expected: 3, values.Length);
	}

	[TestMethod]
	public void DataModelAccess_GetNames_ShouldReturnAllNames()
	{
		// Act
		var names = Enum.GetNames(typeof(DataModelAccess));

		// Assert
		Assert.AreEqual(expected: 3, names.Length);
		Assert.IsTrue(names.Contains("Writable"));
		Assert.IsTrue(names.Contains("ReadOnly"));
		Assert.IsTrue(names.Contains("Constant"));
	}

	[TestMethod]
	public void DataModelAccess_GetHashCode_ShouldBeConsistent()
	{
		// Arrange
		var access = DataModelAccess.Constant;

		// Act
		var hash1 = access.GetHashCode();
		var hash2 = access.GetHashCode();

		// Assert
		Assert.AreEqual(hash1, hash2);
	}

	[TestMethod]
	public void DataModelAccess_CastToInt_ShouldWork()
	{
		// Act
		var writableInt = (int)DataModelAccess.Writable;
		var readOnlyInt = (int)DataModelAccess.ReadOnly;
		var constantInt = (int)DataModelAccess.Constant;

		// Assert
		Assert.AreEqual(expected: 0, writableInt);
		Assert.AreEqual(expected: 1, readOnlyInt);
		Assert.AreEqual(expected: 2, constantInt);
	}
}