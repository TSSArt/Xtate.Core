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
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable All

namespace Xtate.IoC.Test;

[TestClass]
public class FactoryProviderTest
{
	private ServiceCollection _services = default!;

	[TestInitialize]
	public void Initialization()
	{
		_services = new ServiceCollection();
	}

	[TestMethod]
	public async Task AsyncNotResolvedTypeTest()
	{
		// Arrange
		_services.AddType<Class<Integer>>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetOptionalService<Class<string>>();

		// Assert
		Assert.IsNull(obj);
	}

	[TestMethod]
	public async Task AsyncResolvedTypeTest()
	{
		// Arrange
		_services.AddType<Class<Integer>>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<Class<int>>();

		// Assert
		Assert.AreEqual(expected: "Int32", obj.ToString());
	}

	[TestMethod]
	public void SyncNotResolvedTypeTest()
	{
		// Arrange
		_services.AddTypeSync<Class<Integer>>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetOptionalSyncFactory<Class<string>>()();

		// Assert
		Assert.IsNull(obj);
	}

	[TestMethod]
	public void SyncResolvedTypeTest()
	{
		// Arrange
		_services.AddTypeSync<Class<Integer>>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredSyncFactory<Class<int>>()();

		// Assert
		Assert.AreEqual(expected: "Int32", obj.ToString());
	}

	[TestMethod]
	public void IncorrectTypeTest()
	{
		// Arrange
		var fct = _services.AddImplementation<Class<Any>>();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => fct.For<IEnumerable>());
	}

	[TestMethod]
	public void IncorrectImplementationTest()
	{
		// Arrange
		var fct = _services.AddImplementation<Implementation<Any>>();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => fct.For<IEnumerable>());
	}

	[TestMethod]
	public void IncorrectImplementationSyncTest()
	{
		// Arrange
		var fct = _services.AddImplementationSync<Implementation<Any>>();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => fct.For<IEnumerable>());
	}

	[TestMethod]
	public async Task NotResolvedServiceTest()
	{
		// Arrange
		_services.AddImplementation<Implementation<Integer>>().For<IService<Integer>>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetOptionalService<IService<string>>();

		// Assert
		Assert.IsNull(obj);
	}

	[TestMethod]
	public void NotResolvedServiceSyncTest()
	{
		// Arrange
		_services.AddImplementationSync<Implementation<Integer>>().For<IService<Integer>>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetOptionalServiceSync<IService<string>>();

		// Assert
		Assert.IsNull(obj);
	}

	[TestMethod]
	public void StubNonGenTypeTest()
	{
		// Arrange
		_services.AddImplementation<ImplementationLong>().For<IService<long>>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredService<IService<long>>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public void StubTypeTest()
	{
		// Arrange
		_services.AddImplementation<Implementation<Integer>>().For<IService<Integer>>();
		var sp = _services.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => sp.GetOptionalService<IService<Integer>>());
	}

	[TestMethod]
	public void StubTypeSyncTest()
	{
		// Arrange
		_services.AddImplementationSync<Implementation<Integer>>().For<IService<Integer>>();
		var sp = _services.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => sp.GetOptionalServiceSync<IService<Integer>>());
	}

	[TestMethod]
	public void IncorrectServiceTest()
	{
		// Arrange
		var fct = _services.AddImplementation<Implementation<Integer>>();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => fct.For<IService<string>>());
	}

	[TestMethod]
	public void IncorrectDecoratorTest()
	{
		// Arrange
		_services.AddImplementation<Decorated<Any>>().For<IService<Any>>();
		var fct = _services.AddDecorator<Decorator<Any>>();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => fct.For<IService<string>>());
	}

	[TestMethod]
	public void IncorrectDecoratorSyncTest()
	{
		// Arrange
		_services.AddImplementationSync<Decorated<Any>>().For<IService<Any>>();
		var fct = _services.AddDecoratorSync<Decorator<Any>>();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => fct.For<IService<string>>());
	}

	[TestMethod]
	public async Task IncorrectFactoryTest()
	{
		// Arrange
		_services.AddFactory<Factory>().For<IService<long>, string>();
		var sp = _services.BuildProvider();

		// Act

		// Assert
		await Assert.ThrowsExceptionAsync<DependencyInjectionException>([ExcludeFromCodeCoverage] async () => await sp.GetRequiredService<IService<long>, string>("D"));
	}

	[TestMethod]
	public void IncorrectFactorySyncTest()
	{
		// Arrange
		_services.AddFactorySync<Factory>().For<IService<long>, string>();
		var sp = _services.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => sp.GetRequiredServiceSync<IService<long>, string>("D"));
	}

	[TestMethod]
	public async Task IncorrectFactoryGenericTest()
	{
		// Arrange
		_services.AddFactory<Factory<Integer>>().For<IService<Integer>, string>();
		var sp = _services.BuildProvider();

		// Act
		var service = await sp.GetOptionalService<IService<string>, string>("D");

		// Assert
		Assert.IsNull(service);
	}

	[TestMethod]
	public void IncorrectFactoryGenericSyncTest()
	{
		// Arrange
		_services.AddFactorySync<Factory<Integer>>().For<IService<Integer>, string>();
		var sp = _services.BuildProvider();

		// Act
		var service = sp.GetOptionalServiceSync<IService<string>, string>("D");

		// Assert
		Assert.IsNull(service);
	}

	[TestMethod]
	public async Task FactoryGeneric2Test()
	{
		// Arrange
		_services.AddFactory<Factory<Integer, int>>().For<IService<Integer>>();
		var sp = _services.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<IService<long>>();

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task ClassFactoryFuncArgTest()
	{
		// Arrange
		_services.AddType<Class, int>();
		_services.AddType<ContainerFuncArg>();
		var sp = _services.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<ContainerFuncArg>();

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task ClassFactoryTest()
	{
		// Arrange
		_services.AddType<Class, int>();
		_services.AddType<ContainerFuncArg>();
		var sp = _services.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<ContainerFuncArg>();

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task ClassMultiFactoryTest()
	{
		// Arrange
		_services.AddType<Class>();
		_services.AddType<Class, int>();
		_services.AddType<ContainerMultiFactoryArg>();
		var sp = _services.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<ContainerMultiFactoryArg>();

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void TwoConstructorTest()
	{
		// Arrange

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>(_services.AddType<TwoConstructor>);
	}

	[TestMethod]
	public async Task ObsoleteConstructorTest()
	{
		// Arrange
		_services.AddType<Class<int>>();
		_services.AddType<ObsoleteConstructor>();
		var sp = _services.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<ObsoleteConstructor>();

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void NoConstructorTest()
	{
		// Arrange

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>(_services.AddType<NoConstructor>);
	}

	[TestMethod]
	public async Task MultipleObsoleteConstructorTest()
	{
		// Arrange
		_services.AddType<Class<int>>();
		_services.AddType<MultipleObsoleteConstructor>();
		var sp = _services.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<MultipleObsoleteConstructor>();

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void IncorrectSyncAsyncTest()
	{
		// Arrange
		_services.AddType<AsyncClass>();
		_services.AddTypeSync<SyncClass>();
		var sp = _services.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => sp.GetRequiredSyncFactory<SyncClass>()());
	}

	[TestMethod]
	public async Task ClassWithErrorTest()
	{
		// Arrange
		_services.AddType<ErrorClass>();
		var sp = _services.BuildProvider();

		// Act

		// Assert
		await Assert.ThrowsExceptionAsync<DependencyInjectionException>([ExcludeFromCodeCoverage] async () => await sp.GetRequiredService<ErrorClass>());
	}

	public class Integer : IStub
	{
	#region Interface IStub

		public bool IsMatch(Type type)
		{
			return Type.GetTypeCode(type) switch
				   {
					   TypeCode.Int16 or
						   TypeCode.UInt16 or
						   TypeCode.Int32 or
						   TypeCode.UInt32 or
						   TypeCode.Int64 or
						   TypeCode.UInt64 => true,
					   _ => false
				   };
		}

	#endregion
	}

	public interface IService<T> { }

	[ExcludeFromCodeCoverage]
	public class ImplementationLong : IService<long>
	{
		public override string ToString() => typeof(long).Name;
	}

	[ExcludeFromCodeCoverage]
	public class Implementation<T> : IService<T>
	{
		public override string ToString() => typeof(T).Name;
	}

	[ExcludeFromCodeCoverage]
	public class Class<T>
	{
		public override string ToString() => typeof(T).Name;
	}

	[ExcludeFromCodeCoverage]
	public class Class { }

	[ExcludeFromCodeCoverage]
	public class ContainerFuncArg
	{
		public ContainerFuncArg(Func<int, Class> _) { }
	}

	[ExcludeFromCodeCoverage]
	public class ContainerMultiFactoryArg
	{
		public ContainerMultiFactoryArg(Func<Class?> _, Func<int, Class> __, Func<int, Class?> ___) { }
	}

	[ExcludeFromCodeCoverage]
	public class Factory
	{
		public IService<long> GetService(string _) => default!;
	}

	[ExcludeFromCodeCoverage]
	public class Factory<T>
	{
		public IService<T> GetService(string _) => default!;
	}

	[ExcludeFromCodeCoverage]
	public class Factory<T1, T2>
	{
		public IService<T1> GetService() => new Decorated<T1>();
	}

	[ExcludeFromCodeCoverage]
	public class Decorated<T> : IService<T>
	{
		public override string ToString() => typeof(T).Name;
	}

	[ExcludeFromCodeCoverage]
	public class Decorator<T> : IService<T>
	{
		public Decorator(IService<T> decorated) => Decorated = decorated;

		public IService<T> Decorated { get; }

		public override string ToString() => $"{typeof(T).Name}:{Decorated}";
	}

	[ExcludeFromCodeCoverage]
	public class TwoConstructor
	{
		public TwoConstructor(int _) { }
		public TwoConstructor(string _) { }
	}

	[ExcludeFromCodeCoverage]
	public class ObsoleteConstructor
	{
		public ObsoleteConstructor(Class<int> _) { }

		[Obsolete]
		public ObsoleteConstructor(Class<string> _) { }
	}

	[ExcludeFromCodeCoverage]
	public class MultipleObsoleteConstructor
	{
		public MultipleObsoleteConstructor(Class<int> _) { }

		[Obsolete]
		public MultipleObsoleteConstructor(Class<string> _) { }

		[Obsolete]
		public MultipleObsoleteConstructor(Class<long> _) { }
	}

	[ExcludeFromCodeCoverage]
	public class NoConstructor
	{
		private NoConstructor() { }
	}

	[ExcludeFromCodeCoverage]
	public class AsyncClass { }

	[ExcludeFromCodeCoverage]
	public class SyncClass
	{
		public SyncClass(AsyncClass _) { }
	}

	[ExcludeFromCodeCoverage]
	public class ErrorClass
	{
		public ErrorClass() => throw new ApplicationException("Hmm");
	}
}