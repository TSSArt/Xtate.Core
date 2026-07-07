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

using Xtate.IoProcessors.NamedPipe;

namespace Xtate.Test;

[TestClass]
public class NamedPipeErrorTypeTest
{
	[TestMethod]
	public void NamedPipeErrorType_ShouldHaveNoneValue()
	{
		// Act
		var errorType = NamedPipeErrorType.None;

		// Assert
		Assert.AreEqual(NamedPipeErrorType.None, errorType);
	}

	[TestMethod]
	public void NamedPipeErrorType_ShouldHaveExceptionValue()
	{
		// Act
		var errorType = NamedPipeErrorType.Exception;

		// Assert
		Assert.AreEqual(NamedPipeErrorType.Exception, errorType);
	}

	[TestMethod]
	public void NamedPipeErrorType_ValuesShouldBeDifferent()
	{
		// Act & Assert
		Assert.AreNotEqual(NamedPipeErrorType.None, NamedPipeErrorType.Exception);
	}

	[TestMethod]
	public void NamedPipeErrorType_EqualityComparison_ShouldWork()
	{
		// Act
		var type1 = NamedPipeErrorType.Exception;
		var type2 = NamedPipeErrorType.Exception;
		var type3 = NamedPipeErrorType.None;

		// Assert
		Assert.IsTrue(type1 == type2);
		Assert.IsFalse(type1 == type3);
		Assert.IsFalse(type1 != type2);
		Assert.IsTrue(type1 != type3);
	}

	[TestMethod]
	public void NamedPipeErrorType_ToString_ShouldReturnValidString()
	{
		// Act
		var noneStr = NamedPipeErrorType.None.ToString();
		var exceptionStr = NamedPipeErrorType.Exception.ToString();

		// Assert
		Assert.AreEqual("None", noneStr);
		Assert.AreEqual("Exception", exceptionStr);
	}

	[TestMethod]
	public void NamedPipeErrorType_GetValues_ShouldReturnAllValues()
	{
		// Act
		var values = Enum.GetValues(typeof(NamedPipeErrorType));

		// Assert
		Assert.AreEqual(2, values.Length);
	}

	[TestMethod]
	public void NamedPipeErrorType_GetNames_ShouldReturnAllNames()
	{
		// Act
		var names = Enum.GetNames(typeof(NamedPipeErrorType));

		// Assert
		Assert.AreEqual(2, names.Length);
		Assert.IsTrue(names.Contains("None"));
		Assert.IsTrue(names.Contains("Exception"));
	}

	[TestMethod]
	public void NamedPipeErrorType_GetHashCode_ShouldBeConsistent()
	{
		// Arrange
		var errorType = NamedPipeErrorType.Exception;

		// Act
		var hash1 = errorType.GetHashCode();
		var hash2 = errorType.GetHashCode();

		// Assert
		Assert.AreEqual(hash1, hash2);
	}

	[TestMethod]
	public void NamedPipeErrorType_CastToInt_ShouldWork()
	{
		// Act
		var noneInt = (int) NamedPipeErrorType.None;
		var exceptionInt = (int) NamedPipeErrorType.Exception;

		// Assert
		Assert.AreEqual(0, noneInt);
		Assert.AreEqual(1, exceptionInt);
	}
}
