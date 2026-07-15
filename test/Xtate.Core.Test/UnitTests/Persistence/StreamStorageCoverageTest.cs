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
using Xtate.Persistence;
using Xtate.Persistence.Services;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class StreamStorageCoverageTest
{
	[TestMethod]
	public void ConstructorRequiresNonNullReadableWritableSeekableStream()
	{
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage] () => CreateStorage(stream: null!));
		using var readOnly = new MemoryStream(new byte[8], writable: false);
		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage] () => CreateStorage(readOnly));
		using var nonSeekable = new CapabilityStream(canRead: true, canWrite: true, canSeek: false);
		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage] () => CreateStorage(nonSeekable));
	}

	[TestMethod]
	public async Task EmptyInitializationCrudCheckpointRollbackAndShrinkRoundTrip()
	{
		using var stream = new MemoryStream();
		await using var storage = CreateStorage(stream, disposeStream: false);
		await storage.InitializeAsync();
		await storage.InitializeAsync();
		await storage.CheckPoint(level: 0);
		Assert.AreEqual(expected: 0L, stream.Length);
		await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>([ExcludeFromCodeCoverage] async () => await storage.CheckPoint(level: -1));

		storage.Set([1], [10, 11]);
		CollectionAssert.AreEqual(new byte[] { 10, 11 }, storage.Get([1]).ToArray());
		storage.Remove([1]);
		Assert.IsTrue(storage.Get([1]).IsEmpty);
		storage.Set([1, 2], [12]);
		storage.Set([1, 3], [13]);
		storage.RemoveAll([1]);
		Assert.IsTrue(storage.Get([1, 2]).IsEmpty);
		Assert.IsTrue(storage.Get([1, 3]).IsEmpty);
		storage.Set([2], [20]);
		await storage.CheckPoint(level: 0);
		storage.Set([2], [21]);
		await storage.CheckPoint(level: 1);
		await storage.Shrink();
		await storage.Shrink();

		using var rollbackStream = new MemoryStream(stream.ToArray());
		await using var rollbackStorage = CreateStorage(rollbackStream, disposeStream: false, rollbackLevel: 0);
		await rollbackStorage.InitializeAsync();
		CollectionAssert.AreEqual(new byte[] { 20 }, rollbackStorage.Get([2]).ToArray());
	}

	[TestMethod]
	public async Task DisposalHonorsStreamOwnershipInSyncAndAsyncModes()
	{
		var retainedSyncStream = new TrackingMemoryStream();
		var retainedSync = CreateStorage(retainedSyncStream, disposeStream: false);
		retainedSync.Dispose();
		retainedSync.Dispose();
		Assert.AreEqual(expected: 0, retainedSyncStream.DisposeCount);

		var retainedAsyncStream = new TrackingMemoryStream();
		var retainedAsync = CreateStorage(retainedAsyncStream, disposeStream: false);
		await retainedAsync.DisposeAsync();
		await retainedAsync.DisposeAsync();
		Assert.AreEqual(expected: 0, retainedAsyncStream.DisposeCount);
		Assert.AreEqual(expected: 0, retainedAsyncStream.DisposeAsyncCount);

		var ownedSyncStream = new TrackingMemoryStream();
		var ownedSync = CreateStorage(ownedSyncStream);
		ownedSync.Dispose();
		ownedSync.Dispose();
		Assert.AreEqual(expected: 1, ownedSyncStream.DisposeCount);

		var ownedAsyncStream = new TrackingMemoryStream();
		var ownedAsync = CreateStorage(ownedAsyncStream);
		await ownedAsync.DisposeAsync();
		await ownedAsync.DisposeAsync();
		Assert.AreEqual(expected: 1, ownedAsyncStream.DisposeAsyncCount);
	}

	[TestMethod]
	public async Task InitializationRejectsTruncatedLevelSizeAndPayloadRecords()
	{
		await AssertMalformed([0xC2]);
		await AssertMalformed([0x01]);
		await AssertMalformed([0x01, 0xC2]);
		await AssertMalformed([0x01, 0x02, 0xAA]);
	}

	[TestMethod]
	public async Task InitializationConsumesSkipFinalAndSkipBlockControlRecords()
	{
		await AssertControlRecordIsEmpty([0x00, 0x00]);
		await AssertControlRecordIsEmpty([0x04, 0x01, 0x02]);
		await AssertControlRecordIsEmpty([0x02, 0x02, 0xAA, 0xBB]);
	}

	private static StreamStorage CreateStorage(Stream stream, bool disposeStream = true, int? rollbackLevel = null) =>
		new(stream, disposeStream, rollbackLevel)
		{
			InMemoryStorageFactory = static writeOnly => new InMemoryStorage(writeOnly),
			InMemoryStorageBaselineFactory = static memory => new InMemoryStorage(memory.Span)
		};

	private static async Task AssertMalformed(byte[] bytes)
	{
		using var stream = new MemoryStream(bytes);
		await using var storage = CreateStorage(stream, disposeStream: false);

		await Assert.ThrowsExactlyAsync<PersistenceException>([ExcludeFromCodeCoverage] async () => await storage.InitializeAsync());
	}

	private static async Task AssertControlRecordIsEmpty(byte[] bytes)
	{
		using var stream = new MemoryStream(bytes);
		await using var storage = CreateStorage(stream, disposeStream: false);

		await storage.InitializeAsync();

		Assert.IsTrue(storage.Get([1]).IsEmpty);
		Assert.AreEqual(expected: 0L, stream.Length);
	}

	private sealed class TrackingMemoryStream : MemoryStream
	{
		public int DisposeCount { get; private set; }

		public int DisposeAsyncCount { get; private set; }

		public override ValueTask DisposeAsync()
		{
			DisposeAsyncCount ++;

			return base.DisposeAsync();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposeCount ++;
			}

			base.Dispose(disposing);
		}
	}

	private sealed class CapabilityStream(bool canRead, bool canWrite, bool canSeek) : Stream
	{
		public override bool CanRead { get; } = canRead;

		public override bool CanSeek { get; } = canSeek;

		public override bool CanWrite { get; } = canWrite;

		public override long Length => 0;

		public override long Position { get; set; }

		public override void Flush() { }

		public override int Read(byte[] buffer, int offset, int count) => 0;

		public override long Seek(long offset, SeekOrigin origin) => 0;

		public override void SetLength(long value) { }

		public override void Write(byte[] buffer, int offset, int count) { }
	}
}
