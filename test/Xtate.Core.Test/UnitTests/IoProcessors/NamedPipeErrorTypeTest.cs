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

namespace Xtate.Core.Test.UnitTests.IoProcessors;

[TestClass]
[SuppressMessage(category: "ReSharper", checkId: "ConvertToConstant.Local")]
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
		Assert.AreEqual(expected: 2, Enum.GetValues(typeof(NamedPipeErrorType)).Cast<NamedPipeErrorType>().Distinct().Count());
	}

	[TestMethod]
	[DataRow(NamedPipeErrorType.Exception, NamedPipeErrorType.Exception, true)]
	[DataRow(NamedPipeErrorType.Exception, NamedPipeErrorType.None, false)]
	public void NamedPipeErrorType_EqualityComparison_ShouldWork(NamedPipeErrorType left, NamedPipeErrorType right, bool expectedEqual)
	{
		Assert.AreEqual(expectedEqual, left == right);
		Assert.AreEqual(!expectedEqual, left != right);
	}

	[TestMethod]
	public void NamedPipeErrorType_ToString_ShouldReturnValidString()
	{
		// Act
		var noneStr = TestHelper.EnumName(NamedPipeErrorType.None);
		var exceptionStr = TestHelper.EnumName(NamedPipeErrorType.Exception);

		// Assert
		Assert.AreEqual(nameof(NamedPipeErrorType.None), noneStr);
		Assert.AreEqual(nameof(NamedPipeErrorType.Exception), exceptionStr);
	}

	[TestMethod]
	public void NamedPipeErrorType_GetValues_ShouldReturnAllValues()
	{
		// Act
		var values = Enum.GetValues(typeof(NamedPipeErrorType));

		// Assert
		Assert.AreEqual(expected: 2, values.Length);
	}

	[TestMethod]
	public void NamedPipeErrorType_GetNames_ShouldReturnAllNames()
	{
		// Act
		var names = Enum.GetNames(typeof(NamedPipeErrorType));

		// Assert
		Assert.AreEqual(expected: 2, names.Length);
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
		var noneInt = (int)NamedPipeErrorType.None;
		var exceptionInt = (int)NamedPipeErrorType.Exception;

		// Assert
		Assert.AreEqual(expected: 0, noneInt);
		Assert.AreEqual(expected: 1, exceptionInt);
	}
}