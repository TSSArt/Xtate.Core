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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.IoC.Test;

[TestClass]
public class ServiceProviderExtensionsTest
{
	[TestMethod]
	public async Task GetOptionalService2ArgsTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetOptionalService<Service, int, int>(arg1: 1, arg2: 2);

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void GetOptionalServiceSync2ArgsTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTypeSync<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var service = sp.GetOptionalServiceSync<Service, int, int>(arg1: 1, arg2: 2);

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void GetOptionalServiceSyncArgsTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var service = sp.GetOptionalServiceSync<Service>();

		// Assert
		Assert.IsNull(service);
	}

	[TestMethod]
	public async Task GetServices2ArgsTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var list = new List<Service>();
		await foreach (var vc in sp.GetServices<Service, int, int>(arg1: 1, arg2: 2))
		{
			list.Add(vc);
		}

		// Assert
		Assert.AreEqual(expected: 1, list.Count);
		Assert.IsNotNull(list[0]);
	}

	[TestMethod]
	public void GetServicesSyncTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTypeSync<Service>();
		var sp = sc.BuildProvider();

		// Act
		var list = new List<Service>();
		foreach (var vc in sp.GetServicesSync<Service>())
		{
			list.Add(vc);
		}

		// Assert
		Assert.AreEqual(expected: 1, list.Count);
		Assert.IsNotNull(list[0]);
	}

	[TestMethod]
	public void GetServicesSyncMissingTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var result = sp.GetServicesSync<Service>().GetEnumerator().MoveNext();

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void GetSyncServicesTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTypeSync<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var list = new List<Service>();
		foreach (var vc in sp.GetServicesSync<Service, int, int>(arg1: 1, arg2: 2))
		{
			list.Add(vc);
		}

		// Assert
		Assert.AreEqual(expected: 1, list.Count);
		Assert.IsNotNull(list[0]);
	}

	[TestMethod]
	public async Task GetServicesFactory2ArgsTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var list = new List<Service>();
		await foreach (var vc in sp.GetServicesFactory<Service, int, int>()(arg1: 1, arg2: 2))
		{
			list.Add(vc);
		}

		// Assert
		Assert.AreEqual(expected: 1, list.Count);
		Assert.IsNotNull(list[0]);
	}

	[TestMethod]
	public void GetServicesFactorySync2ArgsTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTypeSync<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var list = new List<Service>();
		foreach (var vc in sp.GetServicesSyncFactory<Service, int, int>()(arg1: 1, arg2: 2))
		{
			list.Add(vc);
		}

		// Assert
		Assert.AreEqual(expected: 1, list.Count);
		Assert.IsNotNull(list[0]);
	}

	[TestMethod]
	public async Task GetServicesFactoryEmptyTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var enumerable = sp.GetServicesFactory<Service, int, int>()(arg1: 0, arg2: 1);
		var next = await enumerable.GetAsyncEnumerator().MoveNextAsync();

		// Assert
		Assert.IsFalse(next);
	}

	[TestMethod]
	public void GetServicesSyncFactoryArg2EmptyTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var enumerable = sp.GetServicesSyncFactory<Service, int, int>()(arg1: 0, arg2: 1);
		var next = enumerable.GetEnumerator().MoveNext();

		// Assert
		Assert.IsFalse(next);
	}

	[TestMethod]
	public void GetServicesSyncFactoryArgEmptyTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var enumerable = sp.GetServicesSyncFactory<Service, int>()(2);
		var next = enumerable.GetEnumerator().MoveNext();

		// Assert
		Assert.IsFalse(next);
	}

	[TestMethod]
	public async Task GetRequiredFactory2ArgsTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetRequiredFactory<Service, int, int>()(arg1: 1, arg2: 2);

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task GetOptionalFactory2ArgsTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetOptionalFactory<Service, int, int>()(arg1: 1, arg2: 2);

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task GetOptionalFactory2ArgsEmptyTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetOptionalFactory<Service, int, int>()(arg1: 1, arg2: 2);

		// Assert
		Assert.IsNull(service);
	}

	[TestMethod]
	public void GetRequiredSyncFactoryEmptyTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>(sp.GetRequiredSyncFactory<Service, int, int>);
	}

	[TestMethod]
	public void GetOptionalSyncFactoryArgEmptyTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var service = sp.GetOptionalSyncFactory<Service, int>()(3);

		// Assert
		Assert.IsNull(service);
	}

	[TestMethod]
	public void GetOptionalSyncFactoryTwoArgsEmptyTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var service = sp.GetOptionalSyncFactory<Service, int, int>()(arg1: 3, arg2: 3);

		// Assert
		Assert.IsNull(service);
	}

	// ReSharper disable All
	public class Service { }

	// ReSharper restore All
}