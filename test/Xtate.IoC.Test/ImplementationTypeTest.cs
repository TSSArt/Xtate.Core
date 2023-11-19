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
public class ImplementationTypeTest
{
	[TestMethod]
	public void NotValidImplementationTypeTest()
	{
		// Arrange

		// Act

		// Assert
		Assert.ThrowsException<ArgumentException>([ExcludeFromCodeCoverage]() => ImplementationType.TypeOf<Interface>());
	}

	[TestMethod]
	public void NotValidImplementationType2Test()
	{
		// Arrange
		var sc = new ServiceCollection();

		// Act

		// Assert
		Assert.ThrowsException<ArgumentException>([ExcludeFromCodeCoverage]() => sc.AddImplementation<Interface>());
	}

	[TestMethod]
	public void ImplementationTypeEmptyTest()
	{
		// Arrange
		var empty = new ImplementationType();

		// Act

		// Assert
		Assert.ThrowsException<InvalidOperationException>([ExcludeFromCodeCoverage]() => empty.Type);
	}

	[TestMethod]
	public void ImplementationTypeDefinitionTest()
	{
		// Arrange
		var intType = ImplementationType.TypeOf<int>();

		// Act

		// Assert
		Assert.AreEqual(new ImplementationType(), intType.Definition);
	}

	[TestMethod]
	public void ImplementationTypeGenericDefinitionTest()
	{
		// Arrange
		var listIntType = ImplementationType.TypeOf<List<int>>();
		var listLongType = ImplementationType.TypeOf<List<long>>();

		// Act

		// Assert
		Assert.IsTrue(listIntType.Definition.Equals(listIntType.Definition));
		Assert.IsTrue(listIntType.Definition.Equals(listLongType.Definition));
		Assert.IsTrue(listIntType.Definition.Equals((object) listIntType.Definition));
		Assert.IsTrue(listIntType.Definition.Equals((object) listLongType.Definition));
		Assert.IsFalse(listIntType.Definition.Equals(new object()));
	}

	[TestMethod]
	public void ImplementationTypeEmptyBaseMethodsTest()
	{
		// Arrange
		var empty = new ImplementationType();

		// Act

		// Assert
		Assert.IsTrue(empty.Equals(empty));
		Assert.AreEqual(expected: "", empty.ToString());
		Assert.AreEqual(expected: 0, empty.GetHashCode());
	}

	[TestMethod]
	public void ImplementationTypeBaseMethodsTest()
	{
		// Arrange
		var intType = ImplementationType.TypeOf<int>();

		// Act

		// Assert
		Assert.IsTrue(intType.Equals(intType));
		Assert.IsFalse(string.IsNullOrEmpty(intType.ToString()));
		Assert.AreNotEqual(long.MaxValue, intType.GetHashCode());
	}

	[TestMethod]
	public void TryConstructBaseClassTest()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<Service<Any>>();
		var srvType = ServiceType.TypeOf<BaseBaseService<int>>();

		// Act
		var result = implType.TryConstruct(srvType, out var newImplType);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(typeof(Service<int>), newImplType.Type);
	}

	[TestMethod]
	public void TryConstructInterfaceTest()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<Service<Any>>();
		var srvType = ServiceType.TypeOf<IService<sbyte>>();

		// Act
		var result = implType.TryConstruct(srvType, out var newImplType);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(typeof(Service<sbyte>), newImplType.Type);
	}

	[TestMethod]
	public void GetMethodInfoNotResolvedTypeTest()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<Factory<Any>>();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => implType.GetMethodInfo<IService<int>, int>(false));
	}

	[TestMethod]
	public void GetMethodInfoNotResolvedMethodTest()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<Factory2>();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => implType.GetMethodInfo<IService<Any>, int>(false));
	}

	[TestMethod]
	public void GetMethodInfoTest()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<Factory<object>>();

		// Act
		var method = implType.GetMethodInfo<IService<int>, int>(false);

		// Assert
		Assert.IsNotNull(method);
	}

	[TestMethod]
	public void MultipleObsoleteTest()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<FactoryObsolete>();

		// Act
		var method = implType.GetMethodInfo<IService<int>, int>(false);

		// Assert
		Assert.IsNotNull(method);
	}

	[TestMethod]
	public void MultipleActualTest()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<FactoryMultiActual>();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => implType.GetMethodInfo<IService<int>, int>(false));
	}

	[TestMethod]
	public void ValidParametersTest()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<Factory<int>>();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => implType.GetMethodInfo<IService<string>, int>(false));
	}

	// ReSharper disable All
	public interface Interface { }

	public class BaseBaseService<T> { }

	public class BaseService<T> : BaseBaseService<T> { }

	public class Service<T> : BaseService<T>, IService2<T>, IService<T> { }

	public interface IService<T> { }

	public interface IService2<T> { }

	public class Factory<T>
	{
		[ExcludeFromCodeCoverage]
		public IService<int> M1() => default!;

		[ExcludeFromCodeCoverage]
		public IService<string> M2(ref int _) => default!;
	}

	public class FactoryObsolete
	{
		[ExcludeFromCodeCoverage]
		[Obsolete]
		public IService<int> M1() => default!;

		[ExcludeFromCodeCoverage]
		[Obsolete]
		public IService<int> M2() => default!;

		[ExcludeFromCodeCoverage]
		public IService<int> M3() => default!;
	}

	public class FactoryMultiActual
	{
		[ExcludeFromCodeCoverage]
		[Obsolete]
		public IService<int> M1() => default!;

		[ExcludeFromCodeCoverage]
		public IService<int> M2() => default!;

		[ExcludeFromCodeCoverage]
		public IService<int> M3() => default!;
	}

	public class Factory2
	{
		[ExcludeFromCodeCoverage]
		public IService<TM> M1<TM>() => default!;
	}

	// ReSharper restore All
}