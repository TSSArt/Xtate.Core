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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.IoC.Test;

public static class AsyncEnumExt
{
	public static int Count<T>(this IAsyncEnumerable<T> en)
	{
		return InternalCount().Result;

		async Task<int> InternalCount()
		{
			var count = 0;

			await foreach (var _ in en)
			{
				count ++;
			}

			return count;
		}
	}
}

[TestClass]
public class ServiceProviderTest
{
	[TestMethod]
	public async Task NewScopeTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Class>();
		var sp = sc.BuildProvider();
		var ssf = await sp.GetRequiredService<IServiceScopeFactory>();
		var sp2 = ssf.CreateScope(
						 sc2 =>
						 {
							 sc2.AddType<Class>();
							 sc2.AddType<Class2>();
							 sc2.AddType<Class2>();
						 })
					 .ServiceProvider;

		// Act
		var s1 = await sp2.GetRequiredService<Class>();
		var s2 = await sp2.GetRequiredService<Class2>();

		// Assert
		Assert.IsNotNull(s1);
		Assert.IsNotNull(s2);
	}

	[TestMethod]
	public async Task SelfRegisterTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTransient(sp => (object) sp);
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<object>();
		((IDisposable) sp).Dispose();

		// Assert
		Assert.AreSame(service, sp);
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task SelfRegisterSingletonTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddShared(SharedWithin.Container, sp => (object) sp);
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<object>();
		((IDisposable) sp).Dispose();

		// Assert
		Assert.AreSame(service, sp);
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task SelfRegisterScopeTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddShared(SharedWithin.Scope, sp => (object) sp);
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<object>();
		((IDisposable) sp).Dispose();

		// Assert
		Assert.AreSame(service, sp);
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void IncorrectTypeTest()
	{
		// Arrange
		var sc = new ServiceCollection();

		// Act

		// Assert
		Assert.ThrowsException<InfrastructureException>([ExcludeFromCodeCoverage]() => sc.AddShared((SharedWithin) (-99), [ExcludeFromCodeCoverage](sp) => (object) sp));
	}

	[TestMethod]
	public async Task MultipleGenericsTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<GenericClass<int>>();
		sc.AddType<GenericClass<byte>>();
		sc.AddType<GenericClass<long>>();
		var sp = sc.BuildProvider();

		// Act
		var service1 = await sp.GetRequiredService<GenericClass<int>>();
		var service2 = await sp.GetRequiredService<GenericClass<byte>>();
		var service3 = await sp.GetRequiredService<GenericClass<long>>();

		// Assert
		Assert.IsNotNull(service1);
		Assert.IsNotNull(service2);
		Assert.IsNotNull(service3);
	}

	[TestMethod]
	public async Task EmptyInitAsyncTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddForwarding<IInitializationHandler>(_ => null!);
		sc.AddType<Class>();
		var sp = sc.BuildProvider();

		// Act
		var service1 = await sp.GetRequiredService<Class>();

		// Assert
		Assert.IsNotNull(service1);
	}

	[TestMethod]
	public async Task ScopePropagationFroGenericsTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<GenericClass<int>>();
		sc.AddType<GenericClass<long>>();
		var sp = sc.BuildProvider();
		var ssf = await sp.GetRequiredService<IServiceScopeFactory>();
		var sp2 = ssf.CreateScope(
						 sc2 =>
						 {
							 sc2.AddType<GenericClass<int>>();
							 sc2.AddType<GenericClass<long>>();
						 })
					 .ServiceProvider;

		// Act
		var s1 = sp2.GetServices<GenericClass<int>>().Count();
		var s2 = sp2.GetServices<GenericClass<long>>().Count();

		// Assert
		Assert.AreEqual(expected: 2, s1);
		Assert.AreEqual(expected: 2, s2);
	}

	[TestMethod]
	public void WrongInstanceScopeTest()
	{
		// Arrange
		var sc = new ServiceCollection();

		[ExcludeFromCodeCoverage]
		static int Factory() => 33;

		sc.Add(new ServiceEntry(TypeKey.ServiceKey<int, int>(), (InstanceScope) 456456456, Factory));

		// Act

		// Assert
		Assert.ThrowsException<InfrastructureException>(sc.BuildProvider);
	}

	[TestMethod]
	public async Task SingletonSyncDisposeTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddSharedType<DisposableClass>(SharedWithin.Container);
		var sp = sc.BuildProvider();
		var service = await sp.GetRequiredService<DisposableClass>();

		// Act
		((IDisposable) sp).Dispose();

		// Assert
		Assert.IsTrue(service.Disposed);
	}

	[TestMethod]
	public async Task SingletonAsyncDisposeTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddSharedType<DisposableClass>(SharedWithin.Container);
		var sp = sc.BuildProvider();
		var service = await sp.GetRequiredService<DisposableClass>();

		// Act
		await ((IAsyncDisposable) sp).DisposeAsync();

		// Assert
		Assert.IsTrue(service.Disposed);
	}

	// ReSharper disable All
	private class Class { }

	private class Class2 { }

	public class GenericClass<T> { }

	public class DisposableClass : IDisposable
	{
		public bool Disposed;

	#region Interface IDisposable

		public void Dispose()
		{
			Disposed = true;
			GC.SuppressFinalize(true);
		}

	#endregion
	}

	// ReSharper restore All
}