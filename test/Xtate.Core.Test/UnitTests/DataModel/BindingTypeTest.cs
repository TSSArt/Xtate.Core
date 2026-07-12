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
public class BindingTypeTest
{
	[TestMethod]
	public void BindingType_ShouldHaveEarlyValue()
	{
		// Act
		var bindingType = BindingType.Early;

		// Assert
		Assert.AreEqual(BindingType.Early, bindingType);
	}

	[TestMethod]
	public void BindingType_ShouldHaveLateValue()
	{
		// Act
		var bindingType = BindingType.Late;

		// Assert
		Assert.AreEqual(BindingType.Late, bindingType);
	}

	[TestMethod]
	public void BindingType_ValuesShouldBeDifferent()
	{
		// Act & Assert
		Assert.AreNotEqual(BindingType.Early, BindingType.Late);
	}

	[TestMethod]
	public void BindingType_EqualityComparison_ShouldWork()
	{
		// Act
		var type1 = BindingType.Late;
		var type2 = BindingType.Late;
		var type3 = BindingType.Early;

		// Assert
		Assert.IsTrue(type1 == type2);
		Assert.IsFalse(type1 == type3);
		Assert.IsFalse(type1 != type2);
		Assert.IsTrue(type1 != type3);
	}

	[TestMethod]
	public void BindingType_ToString_ShouldReturnValidString()
	{
		// Act
		var earlyStr = BindingType.Early.ToString();
		var lateStr = BindingType.Late.ToString();

		// Assert
		Assert.AreEqual(expected: "Early", earlyStr);
		Assert.AreEqual(expected: "Late", lateStr);
	}

	[TestMethod]
	public void BindingType_GetValues_ShouldReturnAllValues()
	{
		// Act
		var values = Enum.GetValues(typeof(BindingType));

		// Assert
		Assert.AreEqual(expected: 2, values.Length);
	}

	[TestMethod]
	public void BindingType_GetNames_ShouldReturnAllNames()
	{
		// Act
		var names = Enum.GetNames(typeof(BindingType));

		// Assert
		Assert.AreEqual(expected: 2, names.Length);
		Assert.IsTrue(names.Contains("Early"));
		Assert.IsTrue(names.Contains("Late"));
	}

	[TestMethod]
	public void BindingType_GetHashCode_ShouldBeConsistent()
	{
		// Arrange
		var type1 = BindingType.Early;

		// Act
		var hash1 = type1.GetHashCode();
		var hash2 = type1.GetHashCode();

		// Assert
		Assert.AreEqual(hash1, hash2);
	}

	[TestMethod]
	public void BindingType_CastToInt_ShouldWork()
	{
		// Act
		var earlyInt = (int) BindingType.Early;
		var lateInt = (int) BindingType.Late;

		// Assert
		Assert.AreEqual(expected: 0, earlyInt);
		Assert.AreEqual(expected: 1, lateInt);
	}
}