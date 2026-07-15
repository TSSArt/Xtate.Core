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

using System.Reflection;
using Xtate.Http.Services;

namespace Xtate.Test.UnitTests.ExternalServices;

[TestClass]
public class HttpClientFactoryCoverageTest
{
	[TestMethod]
	public void FactoryCreatesDistinctClientsOverSharedActiveHandlerAndDisposesIdempotently()
	{
		var lifetime = TimeSpan.FromMinutes(3);
		var gracePeriod = TimeSpan.FromSeconds(45);
		var factory = new HttpClientFactory { HandlerLifetime = lifetime, HandlerGracePeriod = gracePeriod };

		using var first = factory.GetClient();
		var firstEntry = GetActiveEntry(factory);
		using var second = factory.GetClient();
		var secondEntry = GetActiveEntry(factory);

		Assert.AreEqual(lifetime, factory.HandlerLifetime);
		Assert.AreEqual(gracePeriod, factory.HandlerGracePeriod);
		Assert.AreNotSame(first, second);
		Assert.AreSame(firstEntry, secondEntry);
		Assert.IsNotNull(firstEntry);
		var entryType = firstEntry.GetType();
		Assert.IsFalse((bool) entryType.GetProperty("IsExpired")!.GetValue(firstEntry)!);
		Assert.IsFalse((bool) entryType.GetProperty("CanDispose")!.GetValue(firstEntry)!);
		Assert.IsNotNull(entryType.GetProperty("Handler")!.GetValue(firstEntry));

		factory.Dispose();
		factory.Dispose();

		Assert.IsNull(GetActiveEntry(factory));
		Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage]() => factory.GetClient());
	}

	[TestMethod]
	public void FactoryDefaultsMatchExpectedHandlerTiming()
	{
		using var factory = new HttpClientFactory();

		Assert.AreEqual(TimeSpan.FromMinutes(2), factory.HandlerLifetime);
		Assert.AreEqual(TimeSpan.FromMinutes(1), factory.HandlerGracePeriod);
	}

	private static object? GetActiveEntry(HttpClientFactory factory) =>
		typeof(HttpClientFactory).GetField("_activeEntry", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(factory);
}
