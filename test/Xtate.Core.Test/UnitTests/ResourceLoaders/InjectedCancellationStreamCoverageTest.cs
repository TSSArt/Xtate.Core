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

using System.IO;
using System.Threading;
using Xtate.ResourceLoaders.Internal;

namespace Xtate.Test.UnitTests.ResourceLoaders;

[TestClass]
public class InjectedCancellationStreamCoverageTest
{
	[TestMethod]
	public async Task ByteArrayAsyncOperationsSupportDirectAndLinkedTokens()
	{
		using var external = new CancellationTokenSource();
		using var perCall = new CancellationTokenSource();
		await using var direct = new InjectedCancellationStream(new MemoryStream([1, 2, 3]), external.Token);
		await using var linked = new InjectedCancellationStream(new MemoryStream([4, 5, 6]), external.Token);
		var buffer = new byte[3];

		Assert.AreEqual(expected: 3, await direct.ReadAsync(buffer, offset: 0, count: buffer.Length, CancellationToken.None));
		Assert.AreEqual(expected: 3, await linked.ReadAsync(buffer, offset: 0, count: buffer.Length, perCall.Token));
		direct.Position = 0;
		linked.Position = 0;

		await direct.WriteAsync([7], offset: 0, count: 1, CancellationToken.None);
		await linked.WriteAsync([8], offset: 0, count: 1, perCall.Token);
		await direct.FlushAsync(CancellationToken.None);
		await linked.FlushAsync(perCall.Token);
	}

	[TestMethod]
	public async Task MemoryAsyncOperationsSupportDirectAndLinkedTokens()
	{
		using var external = new CancellationTokenSource();
		using var perCall = new CancellationTokenSource();
		await using var direct = new InjectedCancellationStream(new MemoryStream([1, 2]), external.Token);
		await using var linked = new InjectedCancellationStream(new MemoryStream([3, 4]), external.Token);
		var buffer = new byte[2];

		Assert.AreEqual(expected: 2, await direct.ReadAsync(buffer.AsMemory(), CancellationToken.None));
		Assert.AreEqual(expected: 2, await linked.ReadAsync(buffer.AsMemory(), perCall.Token));
		direct.Position = 0;
		linked.Position = 0;

		await direct.WriteAsync(new ReadOnlyMemory<byte>([5]), CancellationToken.None);
		await linked.WriteAsync(new ReadOnlyMemory<byte>([6]), perCall.Token);
	}

	[TestMethod]
	public async Task CopyToAsyncSupportsDirectAndLinkedTokens()
	{
		using var external = new CancellationTokenSource();
		using var perCall = new CancellationTokenSource();
		await using var direct = new InjectedCancellationStream(new MemoryStream([1, 2]), external.Token);
		await using var linked = new InjectedCancellationStream(new MemoryStream([3, 4]), external.Token);
		await using var directDestination = new MemoryStream();
		await using var linkedDestination = new MemoryStream();

		await direct.CopyToAsync(directDestination, bufferSize: 1, CancellationToken.None);
		await linked.CopyToAsync(linkedDestination, bufferSize: 1, perCall.Token);

		CollectionAssert.AreEqual(new byte[] { 1, 2 }, directDestination.ToArray());
		CollectionAssert.AreEqual(new byte[] { 3, 4 }, linkedDestination.ToArray());
	}
}
