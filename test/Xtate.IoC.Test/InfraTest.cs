#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.IoC.Test;

[TestClass]
public class InfraTest
{
	[TestMethod]
	public void AssertFailureTest()
	{
		// Arrange

		// Act

		// Assert
		Assert.ThrowsException<InfrastructureException>([ExcludeFromCodeCoverage]() => Infra.Assert(false));
	}

	[TestMethod]
	public void RequiresFailureTest()
	{
		// Arrange
		[ExcludeFromCodeCoverage]
		static void Method(object argument1) => Infra.Requires(argument1);

		// Act

		// Assert
		var ex = Assert.ThrowsException<ArgumentNullException>([ExcludeFromCodeCoverage]() => Method(null!));
		Assert.AreEqual(expected: "argument1", ex.ParamName);
	}

	[TestMethod]
	public void NotNullFailureTest()
	{
		// Arrange

		// Act

		// Assert
		Assert.ThrowsException<InfrastructureException>([ExcludeFromCodeCoverage]() => Infra.NotNull(null!));
	}

	[TestMethod]
	public void UnexpectedNullTest()
	{
		// Arrange
		var ex = Infra.UnexpectedValueException((object?) null);

		// Act

		// Assert
		Assert.IsTrue(ex.Message.Contains("null"));
	}

	[TestMethod]
	public void UnexpectedPrimitiveTest()
	{
		// Arrange
		var ex = Infra.UnexpectedValueException(456789123);

		// Act

		// Assert
		Assert.IsTrue(ex.Message.Contains("456789123"));
	}

	[TestMethod]
	public void UnexpectedEnumTest()
	{
		// Arrange
		var ex = Infra.UnexpectedValueException(UnexpectedEnumTestEnum.Val1);

		// Act

		// Assert
		Assert.IsTrue(ex.Message.Contains("Val1"));
	}

	[TestMethod]
	public void UnexpectedDelegateTest()
	{
		// Arrange
		var ex = Infra.UnexpectedValueException([ExcludeFromCodeCoverage]() => { });

		// Act

		// Assert
		Assert.IsTrue(ex.Message.Contains("Delegate"));
	}

	[TestMethod]
	public void UnexpectedOtherTypeTest()
	{
		// Arrange
		var ex = Infra.UnexpectedValueException(new Version());

		// Act

		// Assert
		Assert.IsTrue(ex.Message.Contains("Version"));
	}

	private enum UnexpectedEnumTestEnum { Val1 }
}