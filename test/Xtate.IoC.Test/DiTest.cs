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
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedVariable
// ReSharper disable ClassNeverInstantiated.Global

namespace Xtate.IoC.Test.DI;

[TestClass]
public class DiTest
{
	[TestMethod]
	public async Task CreateEmptyClass()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class1>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class1InstanceA = await serviceProvider.GetRequiredService<Class1>();
		var class1InstanceB = await serviceProvider.GetRequiredService<Class1>();

		Assert.IsNotNull(class1InstanceA);
		Assert.IsNotNull(class1InstanceB);
		Assert.AreNotSame(class1InstanceA, class1InstanceB);
	}

	[TestMethod]
	public async Task CreateClassWithDependency()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class1>();
		serviceCollection.AddType<Class2>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class2InstanceA = await serviceProvider.GetRequiredService<Class2>();
		var class2InstanceB = await serviceProvider.GetRequiredService<Class2>();

		Assert.IsNotNull(class2InstanceA);
		Assert.IsNotNull(class2InstanceB);
		Assert.AreNotSame(class2InstanceA, class2InstanceB);
		Assert.AreNotSame(class2InstanceA.Class1, class2InstanceB.Class1);
	}

	[TestMethod]
	public async Task CreateSingleton()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSharedType<SingletonClass>(SharedWithin.Container);
		var serviceProvider = serviceCollection.BuildProvider();
		var classInstanceA = await serviceProvider.GetRequiredService<SingletonClass>();
		var classInstanceB = await serviceProvider.GetRequiredService<SingletonClass>();

		var serviceScopeFactory = await serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var scopedServiceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
		var classInstanceC = await scopedServiceProvider.GetRequiredService<SingletonClass>();
		var classInstanceD = await scopedServiceProvider.GetRequiredService<SingletonClass>();

		Assert.IsNotNull(classInstanceA);
		Assert.IsNotNull(classInstanceB);
		Assert.IsNotNull(classInstanceC);
		Assert.IsNotNull(classInstanceD);
		Assert.AreSame(classInstanceA, classInstanceB);
		Assert.AreSame(classInstanceC, classInstanceD);
		Assert.AreSame(classInstanceA, classInstanceC);
	}

	[TestMethod]
	public async Task CreateScoped()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSharedType<ScopedClass>(SharedWithin.Scope);
		var serviceProvider = serviceCollection.BuildProvider();
		var classInstanceA = await serviceProvider.GetRequiredService<ScopedClass>();
		var classInstanceB = await serviceProvider.GetRequiredService<ScopedClass>();

		var serviceScopeFactory = await serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var scopedServiceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
		var classInstanceC = await scopedServiceProvider.GetRequiredService<ScopedClass>();
		var classInstanceD = await scopedServiceProvider.GetRequiredService<ScopedClass>();

		Assert.IsNotNull(classInstanceA);
		Assert.IsNotNull(classInstanceB);
		Assert.IsNotNull(classInstanceC);
		Assert.IsNotNull(classInstanceD);
		Assert.AreSame(classInstanceA, classInstanceB);
		Assert.AreSame(classInstanceC, classInstanceD);
		Assert.AreNotSame(classInstanceA, classInstanceC);
	}

	[TestMethod]
	public async Task CreateDecorator()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddImplementation<DecoratedClass>().For<IForDecoration>();
		serviceCollection.AddDecorator<DecoratorClass>().For<IForDecoration>();
		var serviceProvider = serviceCollection.BuildProvider();
		var classInstance = await serviceProvider.GetRequiredService<IForDecoration>();

		Assert.IsNotNull(classInstance);
		Assert.AreEqual(classInstance.Value, actual: @"[DecoratedClass]");
	}

	[TestMethod]
	public async Task CreateSyncDecorator()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTransient<IForDecoration>(sp => new DecoratedClass());
		serviceCollection.AddTransientDecorator<IForDecoration>((sp, d) => new DecoratorClass(d));
		var serviceProvider = serviceCollection.BuildProvider();
		var classInstance = await serviceProvider.GetRequiredService<IForDecoration>();

		Assert.IsNotNull(classInstance);
		Assert.AreEqual(classInstance.Value, actual: @"[DecoratedClass]");
	}

	[TestMethod]
	public async Task CreateMultiInterface()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddImplementation<Class3>().For<IInterface1>().For<IInterface2>();
		var serviceProvider = serviceCollection.BuildProvider();
		var interface1 = await serviceProvider.GetRequiredService<IInterface1>();
		var interface2 = await serviceProvider.GetRequiredService<IInterface2>();
		var class3Instance = await serviceProvider.GetOptionalService<Class3>();

		Assert.IsNotNull(interface1);
		Assert.IsNotNull(interface2);
		Assert.IsNull(class3Instance);
	}

	[TestMethod]
	public async Task CreateWithArg()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class4, int>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class4 = await serviceProvider.GetRequiredService<Class4, int>(55);

		Assert.IsNotNull(class4);
		Assert.AreEqual(expected: 55, class4.Val);
	}

	[TestMethod]
	public async Task CreateWithArg2()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class4A, int, string>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class4 = await serviceProvider.GetRequiredService<Class4A, int, string>(arg1: 556, arg2: "aa");

		Assert.IsNotNull(class4);
		Assert.AreEqual(expected: 556, class4.Val);
		Assert.AreEqual(expected: "aa", class4.Sval);
	}

	[TestMethod]
	public async Task CreateWithArg2A()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class4A, (int, string)>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class4 = await serviceProvider.GetRequiredService<Class4A, (int, string)>((556, "aa"));

		Assert.IsNotNull(class4);
		Assert.AreEqual(expected: 556, class4.Val);
		Assert.AreEqual(expected: "aa", class4.Sval);
	}

	[TestMethod]
	public async Task CreateClassDifferentRequiredDependencies()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class1>();
		serviceCollection.AddType<Class2>();
		serviceCollection.AddType<Class4, int>();
		serviceCollection.AddTypeSync<Class4Sync, int>();
		serviceCollection.AddType<Class4, int, int>();
		serviceCollection.AddTypeSync<Class4Sync, int, int>();
		serviceCollection.AddType<Class5, int>();
		serviceCollection.AddImplementation<DecoratedClass>().For<IForDecoration>();
		serviceCollection.AddDecorator<DecoratorClass>().For<IForDecoration>();
		serviceCollection.AddImplementationSync<DecoratedClassSync>().For<IForDecorationSync>();
		serviceCollection.AddDecoratorSync<DecoratorClassSync>().For<IForDecorationSync>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class5 = await serviceProvider.GetRequiredService<Class5, int>(22);

		Assert.IsNotNull(class5);
		Assert.IsNotNull(class5.PropClass1);
		Assert.IsNotNull(class5.PropClass2);
		Assert.IsNotNull(class5.PropClass4);
		Assert.AreEqual(expected: 22, class5.PropClass4.Val);
		Assert.AreEqual(expected: 1, class5.PropClass4S.Length);
		Assert.AreEqual(expected: 22, class5.PropClass4S[0].Val);
		Assert.AreEqual(expected: 1, class5.PropClass4SSync.Length);
		Assert.AreEqual(expected: 22, class5.PropClass4SSync[0].Val);
		Assert.AreEqual(expected: 1, class5.PropClass4STwo.Length);
		Assert.AreEqual(expected: 22, class5.PropClass4STwo[0].Val);
		Assert.AreEqual(expected: 1, class5.PropClass4STwoSync.Length);
		Assert.AreEqual(expected: 22, class5.PropClass4STwoSync[0].Val);
		Assert.AreEqual(expected: 2, class5.PropForDecorations.Length);
		Assert.AreEqual(expected: 2, class5.PropForDecorationsSync.Length);
	}
	
	[TestMethod]
	public async Task CreateOptionalDependencies()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class2Opt>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class2 = await serviceProvider.GetRequiredService<Class2Opt>();

		Assert.IsNotNull(class2);
	}

	[TestMethod]
	public void CreateOptionalSyncDependencies()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTypeSync<Class2Opt>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class2 = serviceProvider.GetRequiredServiceSync<Class2Opt>();

		Assert.IsNotNull(class2);
	}

	[TestMethod]
	public async Task CreateClassDifferentOptionalDependencies()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class6, int>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class6 = await serviceProvider.GetRequiredService<Class6, int>(22);

		Assert.IsNotNull(class6);
		Assert.IsNull(class6.PropClass1);
		Assert.IsNull(class6.PropClass2);
		Assert.IsNull(class6.PropClass4);
		Assert.AreEqual(expected: 0, class6.PropClass4S.Length);
		Assert.AreEqual(expected: 0, class6.PropForDecorations.Length);
	}

	[TestMethod]
	public async Task GenericClassRegistration()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<GenericClass<Any, Any>>();
		var serviceProvider = serviceCollection.BuildProvider();
		var genTypeInt = await serviceProvider.GetRequiredService<GenericClass<int, int>>();
		var genTypeVersion = await serviceProvider.GetRequiredService<GenericClass<Version, Version>>();

		Assert.IsNotNull(genTypeInt);
		Assert.AreSame(typeof(int), genTypeInt.ArgType1);

		Assert.IsNotNull(genTypeVersion);
		Assert.AreSame(typeof(Version), genTypeVersion.ArgType1);
	}

	[TestMethod]
	public async Task GenericClassWithInterfaceRegistration()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddImplementation<GenericClass<Any, Any>>().For<IGenericInterface<byte, Any, Any>>();
		var serviceProvider = serviceCollection.BuildProvider();
		var genTypeInt = await serviceProvider.GetRequiredService<IGenericInterface<byte, int, long>>();
		var genTypeVersion = await serviceProvider.GetRequiredService<IGenericInterface<byte, Version, object>>();

		var genTypeVersion1 = await serviceProvider.GetRequiredService<IGenericInterface<byte, Version, object>>();

		Assert.IsNotNull(genTypeInt);
		Assert.AreSame(typeof(long), genTypeInt.ArgType1);
		Assert.AreSame(typeof(int), genTypeInt.ArgType2);

		Assert.IsNotNull(genTypeVersion);
		Assert.AreSame(typeof(object), genTypeVersion.ArgType1);
		Assert.AreSame(typeof(Version), genTypeVersion.ArgType2);
	}

	[TestMethod]
	public void GenericClassWithInterfaceRegistrationSync()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddImplementationSync<GenericClass<Any, Any>>().For<IGenericInterface<byte, Any, Any>>();
		var serviceProvider = serviceCollection.BuildProvider();
		var genTypeInt = serviceProvider.GetRequiredServiceSync<IGenericInterface<byte, int, long>>();
		var genTypeVersion = serviceProvider.GetRequiredServiceSync<IGenericInterface<byte, Version, object>>();

		var genTypeVersion1 = serviceProvider.GetRequiredServiceSync<IGenericInterface<byte, Version, object>>();

		Assert.IsNotNull(genTypeInt);
		Assert.AreSame(typeof(long), genTypeInt.ArgType1);
		Assert.AreSame(typeof(int), genTypeInt.ArgType2);

		Assert.IsNotNull(genTypeVersion);
		Assert.AreSame(typeof(object), genTypeVersion.ArgType1);
		Assert.AreSame(typeof(Version), genTypeVersion.ArgType2);
	}

	[TestMethod]
	public async Task FactoryRegistration()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddFactory<Class8>().For<Class7>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class7 = await serviceProvider.GetRequiredService<Class7>();
		var class8 = await serviceProvider.GetOptionalService<Class8>();

		Assert.IsNotNull(class7);
		Assert.IsNull(class8);
	}

	[TestMethod]
	public async Task GenericFactoryRegistration()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class9<Any, Any>>();
		serviceCollection.AddFactory<Class10<Any>>().For<IInterface9<Any, Any, Any>>();
		var serviceProvider = serviceCollection.BuildProvider();
		var i9 = await serviceProvider.GetRequiredService<IInterface9<string, List<object>, int>>();
		var i9A = await serviceProvider.GetRequiredService<IInterface9<Version, Uri, char>>();

		Assert.IsNotNull(i9);
		Assert.IsNotNull(i9A);
	}

	[TestMethod]
	public void GenericFactorySyncRegistration()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTypeSync<Class9<Any, Any>>();
		serviceCollection.AddFactorySync<Class10Sync<Any>>().For<IInterface9<Any, Any, Any>>();
		var serviceProvider = serviceCollection.BuildProvider();
		var i9 = serviceProvider.GetRequiredServiceSync<IInterface9<string, List<object>, int>>();
		var i9A = serviceProvider.GetRequiredServiceSync<IInterface9<Version, Uri, char>>();

		Assert.IsNotNull(i9);
		Assert.IsNotNull(i9A);
	}

	/*
	[TestMethod]
	public async Task RecursionTest()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<R1>();
		serviceCollection.AddType<R2>();
		var serviceProvider = serviceCollection.BuildProvider();

		await Assert.ThrowsExceptionAsync<DependencyInjectionException>(async () => await serviceProvider.GetRequiredService<R1>());
	}
	*/

	[TestMethod]
	public void SyncTest()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTypeSync<Class11>();
		serviceCollection.AddTypeSync<Class12>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class12Factory = serviceProvider.GetRequiredSyncFactory<Class12>();
		var class12 = class12Factory();
		var class11 = class12.Class11;

		Assert.IsNotNull(class11);
		Assert.IsNotNull(class12);
	}

	[TestMethod]
	public async Task MultiArgTest()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class14, int, long>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class14 = await serviceProvider.GetRequiredService<Class14, int, long>(arg1: 3, arg2: 4);

		Assert.AreEqual(expected: 7, class14.Result);
	}

	[TestMethod]
	public async Task IndirectAsyncInitTest()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTransient<ClassA1>(sp => new ClassAsyncInit1());
		var serviceProvider = serviceCollection.BuildProvider();

		var class1 = await serviceProvider.GetRequiredService<ClassA1>();

		Assert.IsTrue(class1.InitCalled);
	}

	[TestMethod]
	public async Task DisposableTest()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<DisposableClass>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class1 = await serviceProvider.GetRequiredService<DisposableClass>();

		await ((IAsyncDisposable) serviceProvider).DisposeAsync();

		Assert.IsTrue(class1.Disposed);
	}

	[TestMethod]
	public async Task Disposable2Test()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<DisposableClass>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class1 = await serviceProvider.GetRequiredService<DisposableClass>();

		((IDisposable) serviceProvider).Dispose();

		Assert.IsTrue(class1.Disposed);
	}

	[TestMethod]
	public async Task AsyncDisposableTest()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<AsyncDisposableClass>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class1 = await serviceProvider.GetRequiredService<AsyncDisposableClass>();

		await ((IAsyncDisposable) serviceProvider).DisposeAsync();

		Assert.IsTrue(class1.Disposed);
	}

	[TestMethod]
	public async Task AsyncDisposable2Test()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<AsyncDisposableClass>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class1 = await serviceProvider.GetRequiredService<AsyncDisposableClass>();

		((IDisposable) serviceProvider).Dispose();

		Assert.IsTrue(class1.Disposed);
	}

	[TestMethod]
	public async Task MultiArgFuncTestTest()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTypeSync<Class1, int, long>();
		serviceCollection.AddType<ClassMultiDep>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class1 = await serviceProvider.GetRequiredService<ClassMultiDep>();

		Assert.IsNotNull(class1);
	}

	public class ClassMulti
	{
		[ExcludeFromCodeCoverage]
		public ClassMulti(int _, long __) { }
	}

	public class ClassMultiDep
	{
		public ClassMultiDep(Func<int, long, Class1> _) { }
	}

	public class ClassA1
	{
		public bool InitCalled;
	}

	public class ClassAsyncInit1 : ClassA1, IAsyncInitialization
	{
	#region Interface IAsyncInitialization

		public Task Initialization
		{
			get
			{
				InitCalled = true;
				return Task.CompletedTask;
			}
		}

	#endregion
	}

	public class Class1;

	public class Class2
	{
		public Class2(Class1 class1)
		{
			Infra.Requires(class1);

			Class1 = class1;
		}

		public Class1 Class1 { get; }
	}

	public class Class2Opt
	{
		public Class2Opt(Class1? class1) => Class1 = class1;

		public Class1? Class1 { [ExcludeFromCodeCoverage] get; }
	}

	public interface IInterface1;

	public interface IInterface2;

	public class Class3 : IInterface1, IInterface2;

	public class SingletonClass;

	public class ScopedClass;

	public class Class4
	{
		public Class4(int val) => Val = val;
		public int Val { get; }
	}

	public class Class4Sync
	{
		public Class4Sync(int val) => Val = val;
		public int Val { get; }
	}

	public class Class4A
	{
		public Class4A(string sval, int val)
		{
			Sval = sval;
			Val = val;
		}

		public string Sval { get; }
		public int    Val  { get; }
	}

	public class Class5 : IAsyncInitialization
	{
		private readonly int                                      _arg;
		private readonly Func<ValueTask<Class2>>                  _class2Factory;
		private readonly Func<int, IAsyncEnumerable<Class4>>      _class4Factories;
		private readonly Func<int, IEnumerable<Class4Sync>>       _class4FactoriesSync;
		private readonly Func<int, int, IAsyncEnumerable<Class4>> _class4FactoriesTwo;
		private readonly Func<int, int, IEnumerable<Class4Sync>>  _class4FactoriesTwoSync;
		private readonly Func<int, ValueTask<Class4>>             _class4Factory;
		private readonly IAsyncEnumerable<IForDecoration>         _forDecorations;
		private readonly IEnumerable<IForDecorationSync>          _forDecorationsSync;

		public Class5(Class1 class1,
					  int arg,
					  Func<ValueTask<Class2>> class2Factory,
					  Func<int, ValueTask<Class4>> class4Factory,
					  IAsyncEnumerable<IForDecoration> forDecorations,
					  IEnumerable<IForDecorationSync> forDecorationsSync,
					  Func<int, IAsyncEnumerable<Class4>> class4Factories,
  					  Func<int, IEnumerable<Class4Sync>> class4FactoriesSync,
					  Func<int, int, IAsyncEnumerable<Class4>> class4FactoriesTwo,
  					  Func<int, int, IEnumerable<Class4Sync>> class4FactoriesTwoSync)
		{
			PropClass1 = class1;
			_arg = arg;
			_class2Factory = class2Factory;
			_class4Factory = class4Factory;
			_forDecorations = forDecorations;
			_forDecorationsSync = forDecorationsSync;
			_class4Factories = class4Factories;
			_class4FactoriesSync = class4FactoriesSync;
			_class4FactoriesTwo = class4FactoriesTwo;
			_class4FactoriesTwoSync = class4FactoriesTwoSync;

			Initialization = Initialize();
		}

		public Class1 PropClass1 { get; }

		public ImmutableArray<Class4>     PropClass4S     { get; set; } = ImmutableArray<Class4>.Empty;
		public ImmutableArray<Class4Sync> PropClass4SSync { get; set; } = ImmutableArray<Class4Sync>.Empty;

		public ImmutableArray<Class4>     PropClass4STwo     { get; set; } = ImmutableArray<Class4>.Empty;
		public ImmutableArray<Class4Sync> PropClass4STwoSync { get; set; } = ImmutableArray<Class4Sync>.Empty;

		public ImmutableArray<IForDecoration>     PropForDecorations     { get; set; } = ImmutableArray<IForDecoration>.Empty;
		public ImmutableArray<IForDecorationSync> PropForDecorationsSync { get; set; } = ImmutableArray<IForDecorationSync>.Empty;
		
		public Class4 PropClass4 { get; set; } = default!;

		public Class2 PropClass2 { get; set; } = default!;

	#region Interface IAsyncInitialization

		public Task Initialization { get; }

	#endregion

		private async Task Initialize()
		{
			PropClass2 = await _class2Factory();
			PropClass4 = await _class4Factory(_arg);
			await foreach (var i in _forDecorations)
			{
				PropForDecorations = PropForDecorations.Add(i);
			}

			foreach (var i in _forDecorationsSync)
			{
				PropForDecorationsSync = PropForDecorationsSync.Add(i);
			}

			await foreach (var i in _class4Factories(_arg))
			{
				PropClass4S = PropClass4S.Add(i);
			}

			foreach (var i in _class4FactoriesSync(_arg))
			{
				PropClass4SSync = PropClass4SSync.Add(i);
			}

			await foreach (var i in _class4FactoriesTwo(_arg, _arg))
			{
				PropClass4STwo = PropClass4STwo.Add(i);
			}

			foreach (var i in _class4FactoriesTwoSync(_arg, _arg))
			{
				PropClass4STwoSync = PropClass4STwoSync.Add(i);
			}
		}
	}

	public class Class6 : IAsyncInitialization
	{
		private readonly int                                 _arg;
		private readonly Func<ValueTask<Class2?>>            _class2Factory;
		private readonly Func<int, IAsyncEnumerable<Class4>> _class4Factories;
		private readonly Func<int, ValueTask<Class4?>>       _class4Factory;
		private readonly IAsyncEnumerable<IForDecoration>    _forDecorations;

		public Class6(Class1? class1,
					  int arg,
					  Func<ValueTask<Class2?>> class2Factory,
					  Func<int, ValueTask<Class4?>> class4Factory,
					  Func<object, ValueTask<Class4?>> _,
					  IAsyncEnumerable<IForDecoration> forDecorations,
					  Func<int, IAsyncEnumerable<Class4>> class4Factories)
		{
			PropClass1 = class1;
			_arg = arg;
			_class2Factory = class2Factory;
			_class4Factory = class4Factory;
			_forDecorations = forDecorations;
			_class4Factories = class4Factories;

			Initialization = Initialize();
		}

		public Class1? PropClass1 { get; }

		public ImmutableArray<Class4> PropClass4S { get; [ExcludeFromCodeCoverage] set; } = ImmutableArray<Class4>.Empty;

		public ImmutableArray<IForDecoration> PropForDecorations { get; [ExcludeFromCodeCoverage] set; } = ImmutableArray<IForDecoration>.Empty;

		public Class4? PropClass4 { get; set; }

		public Class2? PropClass2 { get; set; }

	#region Interface IAsyncInitialization

		public Task Initialization { get; }

	#endregion

		[ExcludeFromCodeCoverage]
		private async Task Initialize()
		{
			PropClass2 = await _class2Factory();
			PropClass4 = await _class4Factory(_arg);
			await foreach (var i in _forDecorations)
			{
				PropForDecorations = PropForDecorations.Add(i);
			}

			await foreach (var i in _class4Factories(_arg))
			{
				PropClass4S = PropClass4S.Add(i);
			}
		}
	}

	public interface IForDecoration
	{
		string Value { get; }
	}

	public interface IForDecorationSync
	{
		string Value { get; }
	}

	public class DecoratedClass : IForDecoration
	{
	#region Interface IForDecoration

		public string Value => "DecoratedClass";

	#endregion
	}

	public class DecoratorClass : IForDecoration
	{
		public DecoratorClass(IForDecoration decorated) => Decorated = decorated;
		public IForDecoration Decorated { get; }

	#region Interface IForDecoration

		public string Value => "[" + Decorated.Value + "]";

	#endregion
	}

	public class DecoratedClassSync : IForDecorationSync
	{
	#region Interface IForDecoration

		public string Value => "DecoratedClassSync";

	#endregion
	}

	public class DecoratorClassSync : IForDecorationSync
	{
		public DecoratorClassSync(IForDecorationSync decorated) => Decorated = decorated;
		public IForDecorationSync Decorated { get; }

	#region Interface IForDecoration

		public string Value => "[" + Decorated.Value + "]";

	#endregion
	}

	public interface IGenericInterface<TI0, TI1, TI2>
	{
		Type ArgType1 { get; }
		Type ArgType2 { get; }
	}

	public class GenericClass<TC1, TC2> : IGenericInterface<byte, TC2, TC1>, IGenericInterface<TC1, sbyte, TC2>
	{
	#region Interface IGenericInterface<byte,TC2,TC1>

		public Type ArgType1 => typeof(TC1);
		public Type ArgType2 => typeof(TC2);

	#endregion
	}

	public class Class7;

	public class Class8
	{
		public ValueTask<Class7> Factory() => new(new Class7());

		[Obsolete]
		[ExcludeFromCodeCoverage]
		public ValueTask<Class7> Factory1() => default;
	}

	public interface IInterface9<TI1, TI2, TI3>;

	public class Class9<TC1, TC2> : IInterface9<TC2, List<TC1>, int>, IInterface9<TC2, TC1, long>, IInterface9<TC2, TC1, char>;

	public class Class10<T2>
	{
		private readonly IServiceProvider _serviceProvider;

		public Class10(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

		public async ValueTask<IInterface9<T2, List<T1>, int>> Factory<T1>() => await _serviceProvider.GetRequiredService<Class9<T1, T2>>();

		[ExcludeFromCodeCoverage]
		public async ValueTask<IInterface9<T2, T1, long>> Factory2<T1>() => await _serviceProvider.GetRequiredService<Class9<T1, T2>>();

		public IInterface9<T2, T1, char> Factory3<T1>(CancellationToken token) => _serviceProvider.GetRequiredService<Class9<T1, T2>>().Result;
	}

	public class Class10Sync<T2>
	{
		private readonly IServiceProvider _serviceProvider;

		public Class10Sync(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

		public IInterface9<T2, List<T1>, int> Factory<T1>() => _serviceProvider.GetRequiredServiceSync<Class9<T1, T2>>();

		[ExcludeFromCodeCoverage]
		public IInterface9<T2, T1, long> Factory2<T1>() => _serviceProvider.GetRequiredServiceSync<Class9<T1, T2>>();

		public IInterface9<T2, T1, char> Factory3<T1>() => _serviceProvider.GetRequiredService<Class9<T1, T2>>().Result;
	}

	public class R1
	{
		[ExcludeFromCodeCoverage]
		public R1(R2 _) { }
	}

	public class R2
	{
		[ExcludeFromCodeCoverage]
		public R2(R1 _) { }
	}

	public class Class11;

	public class Class12
	{
		public Class12(Func<Class11> factory, Func<Class11> _) => Class11 = factory();

		public Class11 Class11 { get; }
	}

	public class Class14
	{
		private readonly int  _a;
		private readonly long _b;

		public Class14(int a, long b)
		{
			_a = a;
			_b = b;
		}

		public long Result => _a + _b;
	}

	public class DisposableClass : IDisposable
	{
		public bool Disposed;

	#region Interface IDisposable

		public void Dispose()
		{
			Disposed = true;
		}

	#endregion
	}

	public class AsyncDisposableClass : IAsyncDisposable
	{
		public bool Disposed;

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			Disposed = true;

			return default;
		}

	#endregion
	}
}