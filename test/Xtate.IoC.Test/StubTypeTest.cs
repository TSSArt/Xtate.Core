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

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.IoC.Test;

[TestClass]
public class StubTypeTest
{
	[TestMethod]
	public void IsResolvedTypeTest()
	{
		// Arrange

		// Act

		// Assert
		Assert.IsTrue(StubType.IsResolvedType(typeof(int[])));
		Assert.IsTrue(StubType.IsResolvedType(typeof(List<List<int>>)));
		Assert.IsFalse(StubType.IsResolvedType(typeof(List<List<Any>>)));
		Assert.IsFalse(StubType.IsResolvedType(typeof(List<>)));
	}

	[TestMethod]
	public void TryMapTest()
	{
		// Arrange

		// Act

		// Assert
		Assert.IsTrue(StubType.TryMap(typesToMap1: null, typesToMap2: null, typeof(int), arg2: null));
		Assert.IsTrue(StubType.TryMap(typesToMap1: null, typesToMap2: null, arg1: null, typeof(int)));
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, arg1: null, typeof(List<Any>)));
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, typeof(List<Any>), arg2: null));
		Assert.IsTrue(StubType.TryMap(typesToMap1: null, typesToMap2: null, typeof(List<Any[]>), typeof(List<int[]>)));
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, typeof(List<Any[]>), typeof(List<string>)));
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, typeof(List<string>), typeof(List<Any[]>)));
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, typeof(Any[]), arg2: null));
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, arg1: null, typeof(Any[])));
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, new[] { typeof(int) }, new[] { typeof(int), typeof(int) }));
	}

	[TestMethod]
	public void UpdateTypeTest()
	{
		// Arrange
		var args = typeof(GenericClass<>).GetGenericArguments();

		// Act
		StubType.TryMap(args, typesToMap2: null, args[0], arg2: null);

		// Assert
		Assert.AreEqual(typeof(void), args[0]);
	}

	// ReSharper disable All
	public class GenericClass<T> { }

	// ReSharper restore All
}