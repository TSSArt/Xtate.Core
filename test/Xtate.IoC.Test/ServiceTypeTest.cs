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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.IoC.Test;

[TestClass]
public class ServiceTypeTest
{
	[TestMethod]
	public void ServiceTypeEmptyTest()
	{
		// Arrange
		var empty = new ServiceType();

		// Act

		// Assert
		Assert.ThrowsException<InvalidOperationException>([ExcludeFromCodeCoverage]() => empty.Type);
	}

	[TestMethod]
	public void ServiceTypeDefinitionTest()
	{
		// Arrange
		var intType = ServiceType.TypeOf<int>();

		// Act

		// Assert
		Assert.AreEqual(new ServiceType(), intType.Definition);
	}

	[TestMethod]
	public void ServiceTypeGenericDefinitionTest()
	{
		// Arrange
		var listIntType = ServiceType.TypeOf<List<int>>();
		var listLongType = ServiceType.TypeOf<List<long>>();

		// Act

		// Assert
		Assert.IsTrue(listIntType.Definition.Equals(listIntType.Definition));
		Assert.IsTrue(listIntType.Definition.Equals(listLongType.Definition));
		Assert.IsTrue(listIntType.Definition.Equals((object) listIntType.Definition));
		Assert.IsTrue(listIntType.Definition.Equals((object) listLongType.Definition));
		Assert.IsFalse(listIntType.Definition.Equals(new object()));
	}

	[TestMethod]
	public void ServiceTypeEmptyBaseMethodsTest()
	{
		// Arrange
		var empty = new ServiceType();

		// Act

		// Assert
		Assert.IsTrue(empty.Equals(empty));
		Assert.AreEqual(expected: "", empty.ToString());
		Assert.AreEqual(expected: 0, empty.GetHashCode());
	}

	[TestMethod]
	public void ServiceTypeBaseMethodsTest()
	{
		// Arrange
		var intType = ServiceType.TypeOf<int>();

		// Act

		// Assert
		Assert.IsTrue(intType.Equals(intType));
		Assert.IsFalse(string.IsNullOrEmpty(intType.ToString()));
		Assert.AreNotEqual(long.MaxValue, intType.GetHashCode());
	}

	[TestMethod]
	public void ArgumentTypeEmptyBaseMethodsTest()
	{
		// Arrange
		var empty = new ArgumentType();

		// Act

		// Assert
		Assert.AreEqual(expected: "(empty)", empty.ToString());
	}
}