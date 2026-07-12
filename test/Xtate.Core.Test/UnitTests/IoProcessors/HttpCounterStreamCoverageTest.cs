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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xtate.IoProcessors.Http;
using Xtate.IoProcessors.Http.Internal;

namespace Xtate.Test.UnitTests.IoProcessors;

[TestClass]
public class HttpCounterStreamCoverageTest
{
	[TestMethod]
	public void CounterStreamInvokesHooksForSynchronousReadAndWritePaths()
	{
		using var readStream = new ObservedCounterStream(new MemoryStream([1, 2, 3]));
		var buffer = new byte[4];

		Assert.AreEqual(2, readStream.Read(buffer, 0, 2));
		Assert.AreEqual(3, readStream.ReadByte());
		Assert.AreEqual(-1, readStream.ReadByte());
		CollectionAssert.AreEqual(new[] { "pre-read:2", "post-read:2", "pre-read:1", "post-read:1", "pre-read:1", "post-read:0" }, readStream.Events);

		using var writeStream = new ObservedCounterStream(new MemoryStream());

		writeStream.Write([4, 5], 0, 2);
		writeStream.WriteByte(6);

		CollectionAssert.AreEqual(new[] { "pre-write:2", "post-write:2", "pre-write:1", "post-write:1" }, writeStream.Events);
	}

	[TestMethod]
	public async Task CounterStreamInvokesHooksForAsynchronousReadAndWritePaths()
	{
		using var readStream = new ObservedCounterStream(new MemoryStream([1, 2, 3, 4]));
		var buffer = new byte[3];

		Assert.AreEqual(3, await readStream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None));
		CollectionAssert.AreEqual(new[] { "pre-read:3", "post-read:3" }, readStream.Events);

		using var writeStream = new ObservedCounterStream(new MemoryStream());

		await writeStream.WriteAsync([5, 6, 7], 0, 3, CancellationToken.None);

		CollectionAssert.AreEqual(new[] { "pre-write:3", "post-write:3" }, writeStream.Events);
	}

	[TestMethod]
	public void CounterStreamRejectsInvalidPreReadCountMutations()
	{
		using var zeroMutation = new MutatingPreReadCounterStream(new MemoryStream([1]), 1);

		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage] () => zeroMutation.Read([], 0, 0));

		using var oversizeMutation = new MutatingPreReadCounterStream(new MemoryStream([1]), 3);
		var buffer = new byte[2];

		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage] () => oversizeMutation.Read(buffer, 0, 2));
	}

	[TestMethod]
	public void ReadLimitStreamTruncatesLastAllowedReadAndThrowsAfterLimit()
	{
		using var stream = new ReadLimitStream(new MemoryStream([1, 2, 3, 4, 5]), maxReadBytes: 3);
		var buffer = new byte[10];

		Assert.AreEqual(3, stream.Read(buffer, 0, buffer.Length));
		CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, buffer.Take(3).ToArray());

		var exception = Assert.ThrowsExactly<HttpRequestProcessException>([ExcludeFromCodeCoverage] () => stream.Read(buffer, 0, 1));

		Assert.AreEqual(HttpStatusCode.RequestEntityTooLarge, exception.StatusCode);
		StringAssert.Contains(exception.Message, "Read limit exceeded");
	}

	[TestMethod]
	public void ReadLimitStreamAllowsZeroLengthReadAtLimit()
	{
		using var stream = new ReadLimitStream(new MemoryStream([1]), maxReadBytes: 1);
		var buffer = new byte[1];

		Assert.AreEqual(1, stream.Read(buffer, 0, 1));
		Assert.AreEqual(0, stream.Read(buffer, 0, 0));
	}

	[TestMethod]
	public async Task ReadLimitStreamAppliesLimitToAsyncRead()
	{
		using var stream = new ReadLimitStream(new MemoryStream([1, 2, 3, 4]), maxReadBytes: 2);
		var buffer = new byte[4];

		Assert.AreEqual(2, await stream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None));

		await Assert.ThrowsExactlyAsync<HttpRequestProcessException>(async () => await stream.ReadAsync(buffer, 0, 1, CancellationToken.None));
	}

	[TestMethod]
	public async Task WriteLimitStreamAllowsWritesUntilLimitAndThrowsWhenExceeded()
	{
		using var stream = new WriteLimitStream(maxWriteBytes: 3);

		stream.Write([1, 2], 0, 2);
		await stream.WriteAsync([3], 0, 1, CancellationToken.None);

		var exception = Assert.ThrowsExactly<IOException>([ExcludeFromCodeCoverage] () => stream.WriteByte(4));

		StringAssert.Contains(exception.Message, "Write limit exceeded");
	}

	private sealed class ObservedCounterStream(Stream stream) : CounterStream(stream)
	{
		public string[] Events => _events.ToArray();

		private readonly List<string> _events = [];

		protected override void PreRead(ref int count) => _events.Add($"pre-read:{count}");

		protected override void PostRead(int count) => _events.Add($"post-read:{count}");

		protected override void PreWrite(int count) => _events.Add($"pre-write:{count}");

		protected override void PostWrite(int count) => _events.Add($"post-write:{count}");
	}

	private sealed class MutatingPreReadCounterStream(Stream stream, int mutatedCount) : CounterStream(stream)
	{
		protected override void PreRead(ref int count) => count = mutatedCount;
	}
}
