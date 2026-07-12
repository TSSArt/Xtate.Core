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
public class TransitionTypeTest
{
	[TestMethod]
	public void TransitionType_ShouldHaveExternalValue()
	{
		// Act
		var transitionType = TransitionType.External;

		// Assert
		Assert.AreEqual(TransitionType.External, transitionType);
	}

	[TestMethod]
	public void TransitionType_ShouldHaveInternalValue()
	{
		// Act
		var transitionType = TransitionType.Internal;

		// Assert
		Assert.AreEqual(TransitionType.Internal, transitionType);
	}

	[TestMethod]
	public void TransitionType_ValuesShouldBeDifferent()
	{
		// Act & Assert
		Assert.AreNotEqual(TransitionType.External, TransitionType.Internal);
	}

	[TestMethod]
	public void TransitionType_EqualityComparison_ShouldWork()
	{
		// Act
		var type1 = TransitionType.Internal;
		var type2 = TransitionType.Internal;
		var type3 = TransitionType.External;

		// Assert
		Assert.IsTrue(type1 == type2);
		Assert.IsFalse(type1 == type3);
		Assert.IsFalse(type1 != type2);
		Assert.IsTrue(type1 != type3);
	}

	[TestMethod]
	public void TransitionType_ToString_ShouldReturnValidString()
	{
		// Act
		var externalStr = TransitionType.External.ToString();
		var internalStr = TransitionType.Internal.ToString();

		// Assert
		Assert.AreEqual(expected: "External", externalStr);
		Assert.AreEqual(expected: "Internal", internalStr);
	}

	[TestMethod]
	public void TransitionType_GetValues_ShouldReturnAllValues()
	{
		// Act
		var values = Enum.GetValues(typeof(TransitionType));

		// Assert
		Assert.AreEqual(expected: 2, values.Length);
	}

	[TestMethod]
	public void TransitionType_GetNames_ShouldReturnAllNames()
	{
		// Act
		var names = Enum.GetNames(typeof(TransitionType));

		// Assert
		Assert.AreEqual(expected: 2, names.Length);
		Assert.IsTrue(names.Contains("External"));
		Assert.IsTrue(names.Contains("Internal"));
	}

	[TestMethod]
	public void TransitionType_GetHashCode_ShouldBeConsistent()
	{
		// Arrange
		var type1 = TransitionType.Internal;

		// Act
		var hash1 = type1.GetHashCode();
		var hash2 = type1.GetHashCode();

		// Assert
		Assert.AreEqual(hash1, hash2);
	}

	[TestMethod]
	public void TransitionType_CastToInt_ShouldWork()
	{
		// Act
		var externalInt = (int) TransitionType.External;
		var internalInt = (int) TransitionType.Internal;

		// Assert
		Assert.AreEqual(expected: 0, externalInt);
		Assert.AreEqual(expected: 1, internalInt);
	}
}