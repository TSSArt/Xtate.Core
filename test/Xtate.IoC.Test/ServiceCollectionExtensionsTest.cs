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

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable ClassNeverInstantiated.Local

namespace Xtate.IoC.Test;

[TestClass]
public class ServiceCollectionExtensionsTest
{
	private ServiceCollection _services = default!;

	[TestInitialize]
	public void Initialization()
	{
		_services = new ServiceCollection();
	}

	[TestMethod]
	public void ServiceCollectionEnumTest()
	{
		// Arrange
		_services.AddType<ClassArg>();

		// Act
		var next = ((IEnumerable) _services).GetEnumerator().MoveNext();

		// Assert
		Assert.IsTrue(next);
	}

	[TestMethod]
	public void IsRegisteredEmptyTest()
	{
		// Arrange

		// Act
		var registered = _services.IsRegistered<ClassArg>();

		// Assert
		Assert.IsFalse(registered);
	}

	[TestMethod]
	public void IsRegisteredNoArgTest()
	{
		// Arrange	
		_services.AddType<ClassArg>();

		// Act
		var registered = _services.IsRegistered<ClassArg>();

		// Assert
		Assert.IsTrue(registered);
	}

	[TestMethod]
	public void IsNotRegisteredArgTest()
	{
		// Arrange	
		_services.AddType<ClassArg, Arg1>();

		// Act
		var registered = _services.IsRegistered<ClassArg, Arg2>();

		// Assert
		Assert.IsFalse(registered);
	}

	[TestMethod]
	public void IsRegisteredArgTest()
	{
		// Arrange	
		_services.AddType<ClassArg, Arg1>();

		// Act
		var registered = _services.IsRegistered<ClassArg, Arg1>();

		// Assert
		Assert.IsTrue(registered);
	}

	[TestMethod]
	public void IsRegisteredMultiArgTest()
	{
		// Arrange
		_services.AddType<ClassArg, Arg1, Arg2>();

		// Act
		var registered1 = _services.IsRegistered<ClassArg, Arg1, Arg2>();
		var registered2 = _services.IsRegistered<ClassArg, (Arg1, Arg2)>();

		// Assert
		Assert.IsTrue(registered1);
		Assert.IsTrue(registered2);
	}

	[TestMethod]
	public async Task AddTypeNoArgTest()
	{
		// Arrange
		_services.AddType<ClassNoArg>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<ClassNoArg>();

		// Assert
		Assert.AreEqual(expected: "c0", obj.ToString());
	}

	[TestMethod]
	public async Task AddTypeArgTest()
	{
		// Arrange
		_services.AddType<ClassArg, Arg1>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<ClassArg, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "c1:a1", obj.ToString());
	}

	[TestMethod]
	public async Task AddTypeMultiArgTest()
	{
		// Arrange
		_services.AddType<ClassMultiArg, Arg1, Arg2>();
		var sp = _services.BuildProvider();

		// Act
		var obj1 = await sp.GetRequiredService<ClassMultiArg, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = await sp.GetRequiredService<ClassMultiArg, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "c2:a1:a2", obj1.ToString());
		Assert.AreEqual(expected: "c2:a1:a2", obj2.ToString());
	}

	[TestMethod]
	public async Task AddTypeMultiArg3Test()
	{
		// Arrange
		_services.AddType<ClassMultiArg3, (Arg1, Arg2, Arg3)>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<ClassMultiArg3, (Arg1, Arg2, Arg3)>((Arg1.Val, Arg2.Val, Arg3.Val));

		// Assert
		Assert.AreEqual(expected: "c3:a1:a2:a3", obj.ToString());
	}

	[TestMethod]
	public async Task AddTypeMultiArg8Test()
	{
		// Arrange
		_services.AddType<ClassMultiArg8, (Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8)>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<ClassMultiArg8, (Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8)>((Arg1.Val, Arg2.Val, Arg3.Val, Arg4.Val, Arg5.Val, Arg6.Val, Arg7.Val, Arg8.Val));

		// Assert
		Assert.AreEqual(expected: "c8:a1:a2:a3:a4:a5:a6:a7:a8", obj.ToString());
	}

	[TestMethod]
	public void AddTypeSyncNoArgTest()
	{
		// Arrange
		_services.AddTypeSync<ClassNoArg>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredSyncFactory<ClassNoArg>()();

		// Assert
		Assert.AreEqual(expected: "c0", obj.ToString());
	}

	[TestMethod]
	public void AddTypeSyncArgTest()
	{
		// Arrange
		_services.AddTypeSync<ClassArg, Arg1>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredSyncFactory<ClassArg, Arg1>()(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "c1:a1", obj.ToString());
	}

	[TestMethod]
	public void AddTypeSyncMultiArgTest()
	{
		// Arrange
		_services.AddTypeSync<ClassMultiArg, Arg1, Arg2>();
		var sp = _services.BuildProvider();

		// Act
		var obj1 = sp.GetRequiredSyncFactory<ClassMultiArg, Arg1, Arg2>()(Arg1.Val, Arg2.Val);
		var obj2 = sp.GetRequiredSyncFactory<ClassMultiArg, (Arg1, Arg2)>()((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "c2:a1:a2", obj1.ToString());
		Assert.AreEqual(expected: "c2:a1:a2", obj2.ToString());
	}

	[TestMethod]
	public async Task AddDecoratorNoArgTest()
	{
		// Arrange
		_services.AddImplementation<ClassDecoratedNoArg>().For<IDecor>();
		_services.AddDecorator<ClassDecoratorNoArg>().For<IDecor>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<IDecor>();

		// Assert
		Assert.AreEqual(expected: "cdr0[cdd0]", obj.ToString());
	}

	[TestMethod]
	public async Task AddDecoratorArgTest()
	{
		// Arrange
		_services.AddImplementation<ClassDecoratedArg, Arg1>().For<IDecor>();
		_services.AddDecorator<ClassDecoratorArg, Arg1>().For<IDecor>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<IDecor, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "cdr1:a1[cdd1:a1]", obj.ToString());
	}

	[TestMethod]
	public async Task AddDecoratorMultiArgTest()
	{
		// Arrange
		_services.AddImplementation<ClassDecoratedMultiArg, Arg1, Arg2>().For<IDecor>();
		_services.AddDecorator<ClassDecoratorMultiArg, Arg1, Arg2>().For<IDecor>();
		var sp = _services.BuildProvider();

		// Act
		var obj1 = await sp.GetRequiredService<IDecor, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = await sp.GetRequiredService<IDecor, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "cdr2:a1:a2[cdd2:a1:a2]", obj1.ToString());
		Assert.AreEqual(expected: "cdr2:a1:a2[cdd2:a1:a2]", obj2.ToString());
	}

	[TestMethod]
	public void AddDecoratorSyncNoArgTest()
	{
		// Arrange
		_services.AddImplementationSync<ClassDecoratedNoArg>().For<IDecor>();
		_services.AddDecoratorSync<ClassDecoratorNoArg>().For<IDecor>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredServiceSync<IDecor>();

		// Assert
		Assert.AreEqual(expected: "cdr0[cdd0]", obj.ToString());
	}

	[TestMethod]
	public void AddDecoratorSyncArgTest()
	{
		// Arrange
		_services.AddImplementationSync<ClassDecoratedArg, Arg1>().For<IDecor>();
		_services.AddDecoratorSync<ClassDecoratorArg, Arg1>().For<IDecor>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredServiceSync<IDecor, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "cdr1:a1[cdd1:a1]", obj.ToString());
	}

	[TestMethod]
	public void AddDecoratorSyncMultiArgTest()
	{
		// Arrange
		_services.AddImplementationSync<ClassDecoratedMultiArg, Arg1, Arg2>().For<IDecor>();
		_services.AddDecoratorSync<ClassDecoratorMultiArg, Arg1, Arg2>().For<IDecor>();
		var sp = _services.BuildProvider();

		// Act
		var obj1 = sp.GetRequiredServiceSync<IDecor, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = sp.GetRequiredServiceSync<IDecor, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "cdr2:a1:a2[cdd2:a1:a2]", obj1.ToString());
		Assert.AreEqual(expected: "cdr2:a1:a2[cdd2:a1:a2]", obj2.ToString());
	}

	[TestMethod]
	public async Task AddImplementationNoArgTest()
	{
		// Arrange
		_services.AddImplementation<ClassNoArg>().For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<IService>();

		// Assert
		Assert.AreEqual(expected: "c0", obj.ToString());
	}

	[TestMethod]
	public async Task AddImplementationArgTest()
	{
		// Arrange
		_services.AddImplementation<ClassArg, Arg1>().For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<IService, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "c1:a1", obj.ToString());
	}

	[TestMethod]
	public async Task AddImplementationMultiArgTest()
	{
		// Arrange
		_services.AddImplementation<ClassMultiArg, Arg1, Arg2>().For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj1 = await sp.GetRequiredService<IService, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = await sp.GetRequiredService<IService, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "c2:a1:a2", obj1.ToString());
		Assert.AreEqual(expected: "c2:a1:a2", obj2.ToString());
	}

	[TestMethod]
	public async Task AddFactoryNoArgTest()
	{
		// Arrange
		_services.AddFactory<FactoryNoArg>().For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<IService>();

		// Assert
		Assert.AreEqual(expected: "c0", obj.ToString());
	}

	[TestMethod]
	public async Task AddFactoryArgTest()
	{
		// Arrange
		_services.AddFactory<FactoryArg>().For<IService, Arg1>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<IService, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "c1:a1", obj.ToString());
	}

	[TestMethod]
	public async Task AddFactoryMultiArg2Test()
	{
		// Arrange
		_services.AddFactory<FactoryMultiArg>().For<IService, Arg1, Arg2>();
		var sp = _services.BuildProvider();

		// Act
		var obj1 = await sp.GetRequiredService<IService, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = await sp.GetRequiredService<IService, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "c2:a1:a2", obj1.ToString());
		Assert.AreEqual(expected: "c2:a1:a2", obj2.ToString());
	}

	[TestMethod]
	public void AddFactorySyncNoArgTest()
	{
		// Arrange
		_services.AddFactorySync<FactoryNoArg>().For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredServiceSync<IService>();

		// Assert
		Assert.AreEqual(expected: "c0", obj.ToString());
	}

	[TestMethod]
	public void AddFactorySyncArgTest()
	{
		// Arrange
		_services.AddFactorySync<FactoryArg>().For<IService, Arg1>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredServiceSync<IService, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "c1:a1", obj.ToString());
	}

	[TestMethod]
	public void AddFactorySyncMultiArg2Test()
	{
		// Arrange
		_services.AddFactorySync<FactoryMultiArg>().For<IService, Arg1, Arg2>();
		var sp = _services.BuildProvider();

		// Act
		var obj1 = sp.GetRequiredServiceSync<IService, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = sp.GetRequiredServiceSync<IService, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "c2:a1:a2", obj1.ToString());
		Assert.AreEqual(expected: "c2:a1:a2", obj2.ToString());
	}

	[TestMethod]
	public async Task AddSharedTypeNoArgTest()
	{
		// Arrange
		_services.AddSharedType<ClassNoArg>(SharedWithin.Container);
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<ClassNoArg>();

		// Assert
		Assert.AreEqual(expected: "c0", obj.ToString());
	}

	[TestMethod]
	public async Task AddSharedTypeArgTest()
	{
		// Arrange
		_services.AddSharedType<ClassArg, Arg1>(SharedWithin.Container);
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<ClassArg, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "c1:a1", obj.ToString());
	}

	[TestMethod]
	public async Task AddSharedTypeMultiArgTest()
	{
		// Arrange
		_services.AddSharedType<ClassMultiArg, Arg1, Arg2>(SharedWithin.Container);
		var sp = _services.BuildProvider();

		// Act
		var obj1 = await sp.GetRequiredService<ClassMultiArg, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = await sp.GetRequiredService<ClassMultiArg, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "c2:a1:a2", obj1.ToString());
		Assert.AreEqual(expected: "c2:a1:a2", obj2.ToString());
	}

	[TestMethod]
	public void AddSharedTypeSyncNoArgTest()
	{
		// Arrange
		_services.AddSharedTypeSync<ClassNoArg>(SharedWithin.Container);
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredServiceSync<ClassNoArg>();

		// Assert
		Assert.AreEqual(expected: "c0", obj.ToString());
	}

	[TestMethod]
	public void AddSharedTypeSyncArgTest()
	{
		// Arrange
		_services.AddSharedTypeSync<ClassArg, Arg1>(SharedWithin.Container);
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredServiceSync<ClassArg, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "c1:a1", obj.ToString());
	}

	[TestMethod]
	public void AddSharedTypeSyncMultiArgTest()
	{
		// Arrange
		_services.AddSharedTypeSync<ClassMultiArg, Arg1, Arg2>(SharedWithin.Container);
		var sp = _services.BuildProvider();

		// Act
		var obj1 = sp.GetRequiredServiceSync<ClassMultiArg, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = sp.GetRequiredServiceSync<ClassMultiArg, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "c2:a1:a2", obj1.ToString());
		Assert.AreEqual(expected: "c2:a1:a2", obj2.ToString());
	}

	[TestMethod]
	public async Task AddSharedDecoratorNoArgTest()
	{
		// Arrange
		_services.AddImplementation<ClassDecoratedNoArg>().For<IDecor>();
		_services.AddSharedDecorator<ClassDecoratorNoArg>(SharedWithin.Container).For<IDecor>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<IDecor>();

		// Assert
		Assert.AreEqual(expected: "cdr0[cdd0]", obj.ToString());
	}

	[TestMethod]
	public async Task AddSharedDecoratorArgTest()
	{
		// Arrange
		_services.AddImplementation<ClassDecoratedArg, Arg1>().For<IDecor>();
		_services.AddSharedDecorator<ClassDecoratorArg, Arg1>(SharedWithin.Container).For<IDecor>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<IDecor, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "cdr1:a1[cdd1:a1]", obj.ToString());
	}

	[TestMethod]
	public async Task AddSharedDecoratorMultiArgTest()
	{
		// Arrange
		_services.AddImplementation<ClassDecoratedMultiArg, Arg1, Arg2>().For<IDecor>();
		_services.AddSharedDecorator<ClassDecoratorMultiArg, Arg1, Arg2>(SharedWithin.Container).For<IDecor>();
		var sp = _services.BuildProvider();

		// Act
		var obj1 = await sp.GetRequiredService<IDecor, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = await sp.GetRequiredService<IDecor, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "cdr2:a1:a2[cdd2:a1:a2]", obj1.ToString());
		Assert.AreEqual(expected: "cdr2:a1:a2[cdd2:a1:a2]", obj2.ToString());
	}

	[TestMethod]
	public void AddSharedDecoratorSyncNoArgTest()
	{
		// Arrange
		_services.AddImplementationSync<ClassDecoratedNoArg>().For<IDecor>();
		_services.AddSharedDecoratorSync<ClassDecoratorNoArg>(SharedWithin.Container).For<IDecor>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredServiceSync<IDecor>();

		// Assert
		Assert.AreEqual(expected: "cdr0[cdd0]", obj.ToString());
	}

	[TestMethod]
	public void AddSharedDecoratorSyncArgTest()
	{
		// Arrange
		_services.AddImplementationSync<ClassDecoratedArg, Arg1>().For<IDecor>();
		_services.AddSharedDecoratorSync<ClassDecoratorArg, Arg1>(SharedWithin.Container).For<IDecor>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredServiceSync<IDecor, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "cdr1:a1[cdd1:a1]", obj.ToString());
	}

	[TestMethod]
	public void AddSharedDecoratorSyncMultiArgTest()
	{
		// Arrange
		_services.AddImplementationSync<ClassDecoratedMultiArg, Arg1, Arg2>().For<IDecor>();
		_services.AddSharedDecoratorSync<ClassDecoratorMultiArg, Arg1, Arg2>(SharedWithin.Container).For<IDecor>();
		var sp = _services.BuildProvider();

		// Act
		var obj1 = sp.GetRequiredServiceSync<IDecor, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = sp.GetRequiredServiceSync<IDecor, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "cdr2:a1:a2[cdd2:a1:a2]", obj1.ToString());
		Assert.AreEqual(expected: "cdr2:a1:a2[cdd2:a1:a2]", obj2.ToString());
	}

	[TestMethod]
	public async Task AddSharedImplementationNoArgTest()
	{
		// Arrange
		_services.AddSharedImplementation<ClassNoArg>(SharedWithin.Container).For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<IService>();

		// Assert
		Assert.AreEqual(expected: "c0", obj.ToString());
	}

	[TestMethod]
	public async Task AddSharedImplementationArgTest()
	{
		// Arrange
		_services.AddSharedImplementation<ClassArg, Arg1>(SharedWithin.Container).For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<IService, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "c1:a1", obj.ToString());
	}

	[TestMethod]
	public async Task AddSharedImplementationMultiArgTest()
	{
		// Arrange
		_services.AddSharedImplementation<ClassMultiArg, Arg1, Arg2>(SharedWithin.Container).For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj1 = await sp.GetRequiredService<IService, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = await sp.GetRequiredService<IService, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "c2:a1:a2", obj1.ToString());
		Assert.AreEqual(expected: "c2:a1:a2", obj2.ToString());
	}

	[TestMethod]
	public void AddSharedImplementationSyncNoArgTest()
	{
		// Arrange
		_services.AddSharedImplementationSync<ClassNoArg>(SharedWithin.Container).For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredServiceSync<IService>();

		// Assert
		Assert.AreEqual(expected: "c0", obj.ToString());
	}

	[TestMethod]
	public void AddSharedImplementationSyncArgTest()
	{
		// Arrange
		_services.AddSharedImplementationSync<ClassArg, Arg1>(SharedWithin.Container).For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredServiceSync<IService, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "c1:a1", obj.ToString());
	}

	[TestMethod]
	public void AddSharedImplementationSyncMultiArgTest()
	{
		// Arrange
		_services.AddSharedImplementationSync<ClassMultiArg, Arg1, Arg2>(SharedWithin.Container).For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj1 = sp.GetRequiredServiceSync<IService, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = sp.GetRequiredServiceSync<IService, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "c2:a1:a2", obj1.ToString());
		Assert.AreEqual(expected: "c2:a1:a2", obj2.ToString());
	}

	[TestMethod]
	public async Task AddSharedFactoryNoArgTest()
	{
		// Arrange
		_services.AddSharedFactory<FactoryNoArg>(SharedWithin.Container).For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<IService>();

		// Assert
		Assert.AreEqual(expected: "c0", obj.ToString());
	}

	[TestMethod]
	public async Task AddSharedFactoryArgTest()
	{
		// Arrange
		_services.AddSharedFactory<FactoryArg>(SharedWithin.Container).For<IService, Arg1>();
		var sp = _services.BuildProvider();

		// Act
		var obj = await sp.GetRequiredService<IService, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "c1:a1", obj.ToString());
	}

	[TestMethod]
	public async Task AddSharedFactoryMultiArgTest()
	{
		// Arrange
		_services.AddSharedFactory<FactoryMultiArg>(SharedWithin.Container).For<IService, Arg1, Arg2>();
		var sp = _services.BuildProvider();

		// Act
		var obj1 = await sp.GetRequiredService<IService, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = await sp.GetRequiredService<IService, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "c2:a1:a2", obj1.ToString());
		Assert.AreEqual(expected: "c2:a1:a2", obj2.ToString());
	}

	[TestMethod]
	public void AddSharedFactorySyncNoArgTest()
	{
		// Arrange
		_services.AddSharedFactorySync<FactoryNoArg>(SharedWithin.Container).For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredServiceSync<IService>();

		// Assert
		Assert.AreEqual(expected: "c0", obj.ToString());
	}

	[TestMethod]
	public void AddSharedFactorySyncArgTest()
	{
		// Arrange
		_services.AddSharedFactorySync<FactoryArg>(SharedWithin.Container).For<IService, Arg1>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredServiceSync<IService, Arg1>(Arg1.Val);

		// Assert
		Assert.AreEqual(expected: "c1:a1", obj.ToString());
	}

	[TestMethod]
	public void AddSharedFactorySyncMultiArgTest()
	{
		// Arrange
		_services.AddSharedFactorySync<FactoryMultiArg>(SharedWithin.Container).For<IService, Arg1, Arg2>();
		var sp = _services.BuildProvider();

		// Act
		var obj1 = sp.GetRequiredServiceSync<IService, Arg1, Arg2>(Arg1.Val, Arg2.Val);
		var obj2 = sp.GetRequiredServiceSync<IService, (Arg1, Arg2)>((Arg1.Val, Arg2.Val));

		// Assert
		Assert.AreEqual(expected: "c2:a1:a2", obj1.ToString());
		Assert.AreEqual(expected: "c2:a1:a2", obj2.ToString());
	}

	[TestMethod]
	public async Task AddOtherTest()
	{
		// Arrange

		// Act
		#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

		_services.AddShared(SharedWithin.Container, [ExcludeFromCodeCoverage](sp) => new ClassNoArg());
		_services.AddShared(SharedWithin.Container, [ExcludeFromCodeCoverage] async (sp) => new ClassNoArg());
		_services.AddShared(SharedWithin.Container, [ExcludeFromCodeCoverage](IServiceProvider sp, Arg1 arg) => new ClassArg(arg));
		_services.AddShared(SharedWithin.Container, [ExcludeFromCodeCoverage] async (IServiceProvider sp, Arg1 arg) => new ClassArg(arg));
		_services.AddShared(SharedWithin.Container, [ExcludeFromCodeCoverage](IServiceProvider sp, Arg1 arg1, Arg2 arg2) => new ClassMultiArg(arg1, arg2));
		_services.AddShared(SharedWithin.Container, [ExcludeFromCodeCoverage] async (IServiceProvider sp, Arg1 arg1, Arg2 arg2) => new ClassMultiArg(arg1, arg2));

		_services.AddSharedDecorator(SharedWithin.Container, [ExcludeFromCodeCoverage](IServiceProvider sp, ClassNoArg _) => new ClassNoArg());
		_services.AddSharedDecorator(SharedWithin.Container, [ExcludeFromCodeCoverage] async (IServiceProvider sp, ClassNoArg _) => new ClassNoArg());
		_services.AddSharedDecorator(SharedWithin.Container, [ExcludeFromCodeCoverage](IServiceProvider sp, ClassArg _, Arg1 arg) => new ClassArg(arg));
		_services.AddSharedDecorator(SharedWithin.Container, [ExcludeFromCodeCoverage] async (IServiceProvider sp, ClassArg _, Arg1 arg) => new ClassArg(arg));
		_services.AddSharedDecorator(
			SharedWithin.Container, [ExcludeFromCodeCoverage](IServiceProvider sp,
															  ClassMultiArg _,
															  Arg1 arg1,
															  Arg2 arg2) => new ClassMultiArg(arg1, arg2));
		_services.AddSharedDecorator(
			SharedWithin.Container, [ExcludeFromCodeCoverage] async (IServiceProvider sp,
																	 ClassMultiArg _,
																	 Arg1 arg1,
																	 Arg2 arg2) => new ClassMultiArg(arg1, arg2));

		_services.AddTransient([ExcludeFromCodeCoverage](sp) => new ClassNoArg());
		_services.AddTransient([ExcludeFromCodeCoverage] async (sp) => new ClassNoArg());
		_services.AddTransient([ExcludeFromCodeCoverage](IServiceProvider sp, Arg1 arg) => new ClassArg(arg));
		_services.AddTransient([ExcludeFromCodeCoverage] async (IServiceProvider sp, Arg1 arg) => new ClassArg(arg));
		_services.AddTransient([ExcludeFromCodeCoverage](IServiceProvider sp, Arg1 arg1, Arg2 arg2) => new ClassMultiArg(arg1, arg2));
		_services.AddTransient([ExcludeFromCodeCoverage] async (IServiceProvider sp, Arg1 arg1, Arg2 arg2) => new ClassMultiArg(arg1, arg2));

		_services.AddTransientDecorator([ExcludeFromCodeCoverage](IServiceProvider sp, ClassNoArg _) => new ClassNoArg());
		_services.AddTransientDecorator([ExcludeFromCodeCoverage] async (IServiceProvider sp, ClassNoArg _) => new ClassNoArg());
		_services.AddTransientDecorator([ExcludeFromCodeCoverage](IServiceProvider sp, ClassArg _, Arg1 arg) => new ClassArg(arg));
		_services.AddTransientDecorator([ExcludeFromCodeCoverage] async (IServiceProvider sp, ClassArg _, Arg1 arg) => new ClassArg(arg));
		_services.AddTransientDecorator(
			[ExcludeFromCodeCoverage](IServiceProvider sp,
									  ClassMultiArg _,
									  Arg1 arg1,
									  Arg2 arg2) => new ClassMultiArg(arg1, arg2));
		_services.AddTransientDecorator(
			[ExcludeFromCodeCoverage] async (IServiceProvider sp,
											 ClassMultiArg _,
											 Arg1 arg1,
											 Arg2 arg2) => new ClassMultiArg(arg1, arg2));

		_services.AddForwarding([ExcludeFromCodeCoverage](sp) => new ClassNoArg());
		_services.AddForwarding([ExcludeFromCodeCoverage] async (sp) => new ClassNoArg());
		_services.AddForwarding([ExcludeFromCodeCoverage](IServiceProvider sp, Arg1 arg) => new ClassArg(arg));
		_services.AddForwarding([ExcludeFromCodeCoverage] async (IServiceProvider sp, Arg1 arg) => new ClassArg(arg));
		_services.AddForwarding([ExcludeFromCodeCoverage](IServiceProvider sp, Arg1 arg1, Arg2 arg2) => new ClassMultiArg(arg1, arg2));
		_services.AddForwarding([ExcludeFromCodeCoverage] async (IServiceProvider sp, Arg1 arg1, Arg2 arg2) => new ClassMultiArg(arg1, arg2));

		var sp = _services.BuildProvider();

		#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

		var noArgCount = 0;
		await foreach (var _ in sp.GetServices<ClassNoArg>())
		{
			noArgCount ++;
		}

		var argCount = 0;
		await foreach (var _ in sp.GetServices<ClassArg, Arg1>(Arg1.Val))
		{
			argCount ++;
		}

		var multiArgCount = 0;
		await foreach (var _ in sp.GetServices<ClassMultiArg, Arg1, Arg2>(Arg1.Val, Arg2.Val))
		{
			multiArgCount ++;
		}

		// Assert
		Assert.AreEqual(expected: 10, noArgCount);
		Assert.AreEqual(expected: 10, argCount);
		Assert.AreEqual(expected: 10, multiArgCount);
	}

	[TestMethod]
	public void AddImplementationSyncNoArgTest()
	{
		// Arrange
		_services.AddImplementationSync<ServiceNoArg>().For<IService>();
		var sp = _services.BuildProvider();

		// Act
		var obj = sp.GetRequiredSyncFactory<IService>()();

		// Assert
		Assert.AreEqual(expected: "cn0", obj.ToString());
	}

	private class ServiceNoArg : IService
	{
		public override string ToString() => "cn0";
	}

	// ReSharper disable All
	private interface IService { }

	private interface IDecor { }

	private class ClassDecoratedNoArg : IDecor
	{
		public override string ToString() => "cdd0";
	}

	private class ClassDecoratorNoArg : IDecor
	{
		private readonly string _val;

		public ClassDecoratorNoArg(IDecor decor) => _val = $"cdr0[{decor}]";

		public override string ToString() => _val;
	}

	private class ClassDecoratedArg : IDecor
	{
		private readonly string _val;

		public ClassDecoratedArg(Arg1 arg1) => _val = $"cdd1:{arg1}";

		public override string ToString() => _val;
	}

	private class ClassDecoratorArg : IDecor
	{
		private readonly string _val;

		public ClassDecoratorArg(IDecor decor, Arg1 arg1) => _val = $"cdr1:{arg1}[{decor}]";

		public override string ToString() => _val;
	}

	private class ClassDecoratedMultiArg : IDecor
	{
		private readonly string _val;

		public ClassDecoratedMultiArg(Arg1 arg1, Arg2 arg2) => _val = $"cdd2:{arg1}:{arg2}";

		public override string ToString() => _val;
	}

	private class ClassDecoratorMultiArg : IDecor
	{
		private readonly string _val;

		public ClassDecoratorMultiArg(IDecor decor, Arg1 arg1, Arg2 arg2) => _val = $"cdr2:{arg1}:{arg2}[{decor}]";

		public override string ToString() => _val;
	}

	private class Arg1
	{
		public static readonly Arg1 Val = new();

		public override string ToString() => "a1";
	}

	private class Arg2
	{
		public static readonly Arg2 Val = new();

		public override string ToString() => "a2";
	}

	private class Arg3
	{
		public static readonly Arg3 Val = new();

		public override string ToString() => "a3";
	}

	private class Arg4
	{
		public static readonly Arg4 Val = new();

		public override string ToString() => "a4";
	}

	private class Arg5
	{
		public static readonly Arg5 Val = new();

		public override string ToString() => "a5";
	}

	private class Arg6
	{
		public static readonly Arg6 Val = new();

		public override string ToString() => "a6";
	}

	private class Arg7
	{
		public static readonly Arg7 Val = new();

		public override string ToString() => "a7";
	}

	private class Arg8
	{
		public static readonly Arg8 Val = new();

		public override string ToString() => "a8";
	}

	private class FactoryNoArg : IService
	{
		public IService CreateService() => new ClassNoArg();
	}

	private class FactoryArg : IService
	{
		public IService CreateService(Arg1 arg1) => new ClassArg(arg1);
	}

	private class FactoryMultiArg : IService
	{
		public IService CreateService(Arg1 arg1, Arg2 arg2) => new ClassMultiArg(arg1, arg2);
	}

	private class ClassNoArg : IService
	{
		public override string ToString() => "c0";
	}

	private class ClassArg : IService
	{
		private readonly string _val;

		public ClassArg(Arg1 arg1) => _val = $"c1:{arg1}";

		public override string ToString() => _val;
	}

	private class ClassMultiArg : IService
	{
		private readonly string _val;

		public ClassMultiArg(Arg1 arg1, Arg2 arg2) => _val = $"c2:{arg1}:{arg2}";

		public override string ToString() => _val;
	}

	private class ClassMultiArg3 : IService
	{
		private readonly string _val;

		public ClassMultiArg3(Arg1 arg1, Arg2 arg2, Arg3 arg3) => _val = $"c3:{arg1}:{arg2}:{arg3}";

		public override string ToString() => _val;
	}

	private class ClassMultiArg8 : IService
	{
		private readonly string _val;

		public ClassMultiArg8(Arg1 arg1,
							  Arg2 arg2,
							  Arg3 arg3,
							  Arg4 arg4,
							  Arg5 arg5,
							  Arg6 arg6,
							  Arg7 arg7,
							  Arg8 arg8) =>
			_val = $"c8:{arg1}:{arg2}:{arg3}:{arg4}:{arg5}:{arg6}:{arg7}:{arg8}";

		public override string ToString() => _val;
	}

	// ReSharper restore All
}