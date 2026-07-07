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

using Xtate.StateMachine;

namespace Xtate.Test;

[TestClass]
public class HistoryTypeTest
{
	[TestMethod]
	public void HistoryType_ShouldHaveShallowValue()
	{
		// Act
		var historyType = HistoryType.Shallow;

		// Assert
		Assert.AreEqual(HistoryType.Shallow, historyType);
	}

	[TestMethod]
	public void HistoryType_ShouldHaveDeepValue()
	{
		// Act
		var historyType = HistoryType.Deep;

		// Assert
		Assert.AreEqual(HistoryType.Deep, historyType);
	}

	[TestMethod]
	public void HistoryType_ValuesShouldBeDifferent()
	{
		// Act & Assert
		Assert.AreNotEqual(HistoryType.Shallow, HistoryType.Deep);
	}

	[TestMethod]
	public void HistoryType_EqualityComparison_ShouldWork()
	{
		// Act
		var type1 = HistoryType.Deep;
		var type2 = HistoryType.Deep;
		var type3 = HistoryType.Shallow;

		// Assert
		Assert.IsTrue(type1 == type2);
		Assert.IsFalse(type1 == type3);
		Assert.IsFalse(type1 != type2);
		Assert.IsTrue(type1 != type3);
	}

	[TestMethod]
	public void HistoryType_ToString_ShouldReturnValidString()
	{
		// Act
		var shallowStr = HistoryType.Shallow.ToString();
		var deepStr = HistoryType.Deep.ToString();

		// Assert
		Assert.AreEqual("Shallow", shallowStr);
		Assert.AreEqual("Deep", deepStr);
	}

	[TestMethod]
	public void HistoryType_GetValues_ShouldReturnAllValues()
	{
		// Act
		var values = Enum.GetValues(typeof(HistoryType));

		// Assert
		Assert.AreEqual(2, values.Length);
	}

	[TestMethod]
	public void HistoryType_GetNames_ShouldReturnAllNames()
	{
		// Act
		var names = Enum.GetNames(typeof(HistoryType));

		// Assert
		Assert.AreEqual(2, names.Length);
		Assert.IsTrue(names.Contains("Shallow"));
		Assert.IsTrue(names.Contains("Deep"));
	}

	[TestMethod]
	public void HistoryType_GetHashCode_ShouldBeConsistent()
	{
		// Arrange
		var type1 = HistoryType.Shallow;

		// Act
		var hash1 = type1.GetHashCode();
		var hash2 = type1.GetHashCode();

		// Assert
		Assert.AreEqual(hash1, hash2);
	}

	[TestMethod]
	public void HistoryType_CastToInt_ShouldWork()
	{
		// Act
		var shallowInt = (int) HistoryType.Shallow;
		var deepInt = (int) HistoryType.Deep;

		// Assert
		Assert.AreEqual(0, shallowInt);
		Assert.AreEqual(1, deepInt);
	}
}
