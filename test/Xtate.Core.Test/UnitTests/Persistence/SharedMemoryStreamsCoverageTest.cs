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
using Xtate.Persistence;
using Xtate.Persistence.Services;
using System.Reflection;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class SharedMemoryStreamsCoverageTest
{
	[TestMethod]
	public async Task ReadWriteStreamForwardsSynchronousAndAsynchronousOperations()
	{
		var streams = new SharedMemoryStreams<string>();
		using var stream = streams.OpenWrite("key");

		Assert.IsTrue(stream.CanRead);
		Assert.IsTrue(stream.CanSeek);
		Assert.IsTrue(stream.CanWrite);
		stream.WriteByte(1);
		stream.Write(new byte[] { 2, 3 }, offset: 0, count: 2);
		stream.Write(new byte[] { 4, 5 }.AsSpan());
		await stream.WriteAsync(new byte[] { 6 }, offset: 0, count: 1, CancellationToken.None);
		await stream.WriteAsync(new byte[] { 7 }.AsMemory(), CancellationToken.None);
		stream.Flush();
		await stream.FlushAsync(CancellationToken.None);

		Assert.AreEqual(7, stream.Length);
		Assert.AreEqual(7, stream.Position);
		Assert.AreEqual(0, stream.Seek(0, SeekOrigin.Begin));
		Assert.AreEqual(1, stream.ReadByte());

		var buffer = new byte[2];
		Assert.AreEqual(2, stream.Read(buffer, offset: 0, count: buffer.Length));
		CollectionAssert.AreEqual(new byte[] { 2, 3 }, buffer);
		Assert.AreEqual(2, stream.Read(buffer.AsSpan()));
		CollectionAssert.AreEqual(new byte[] { 4, 5 }, buffer);
		Assert.AreEqual(1, await stream.ReadAsync(buffer, offset: 0, count: 1, CancellationToken.None));
		Assert.AreEqual(6, buffer[0]);
		Assert.AreEqual(1, await stream.ReadAsync(buffer.AsMemory(0, 1), CancellationToken.None));
		Assert.AreEqual(7, buffer[0]);

		stream.Position = 0;
		using var syncCopy = new MemoryStream();
		stream.CopyTo(syncCopy, bufferSize: 2);
		CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, syncCopy.ToArray());

		stream.Position = 0;
		using var asyncCopy = new MemoryStream();
		await stream.CopyToAsync(asyncCopy, bufferSize: 2, CancellationToken.None);
		CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, asyncCopy.ToArray());

		stream.SetLength(3);
		Assert.AreEqual(3, stream.Length);
		stream.GetType().GetMethod("Dispose", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(stream, [false]);
		Assert.IsTrue(stream.CanRead);
	}

	[TestMethod]
	public void SharedMemoryStreamsEnforcesReaderWriterExclusionAndDeletion()
	{
		var streams = new SharedMemoryStreams<string>();
		var missingReader = streams.OpenRead("missing");
		Assert.AreEqual(expected: 0, missingReader.Length);
		missingReader.GetType().GetMethod("Dispose", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(missingReader, [false]);
		Assert.IsTrue(missingReader.CanRead);
		missingReader.Dispose();
		Assert.IsTrue(streams.Delete("missing"));
		var writer = streams.OpenWrite("key");
		writer.Write([1, 2, 3], offset: 0, count: 3);

		Assert.ThrowsExactly<IOException>([ExcludeFromCodeCoverage] () => streams.OpenRead("key"));
		Assert.ThrowsExactly<IOException>([ExcludeFromCodeCoverage] () => streams.OpenWrite("key"));
		Assert.ThrowsExactly<IOException>([ExcludeFromCodeCoverage] () => streams.Delete("key"));
		writer.Dispose();

		using var reader1 = streams.OpenRead("key");
		using var reader2 = streams.OpenRead("key");
		Assert.IsFalse(reader1.CanWrite);
		Assert.AreEqual(3, reader1.Length);
		Assert.AreEqual(1, reader1.ReadByte());
		Assert.AreEqual(1, reader2.ReadByte());
		Assert.ThrowsExactly<IOException>([ExcludeFromCodeCoverage] () => streams.OpenWrite("key"));
		Assert.ThrowsExactly<IOException>([ExcludeFromCodeCoverage] () => streams.Delete("key"));

		reader1.Dispose();
		Assert.ThrowsExactly<IOException>([ExcludeFromCodeCoverage] () => streams.Delete("key"));
		reader2.Dispose();
		reader2.Dispose();

		Assert.IsTrue(streams.Delete("key"));
		Assert.IsFalse(streams.Delete("key"));
	}

	[TestMethod]
	public void DisposedReadWriteStreamRejectsOperations()
	{
		var streams = new SharedMemoryStreams<string>();
		var stream = streams.OpenWrite("key");
		stream.Dispose();
		stream.Dispose();

		Assert.IsFalse(stream.CanRead);
		Assert.IsFalse(stream.CanSeek);
		Assert.IsFalse(stream.CanWrite);
		Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage] () => _ = stream.Length);
		Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage] () => _ = stream.Position);
		Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage] () => stream.Position = 0);
		Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage] () => stream.Flush());
		Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage] () => stream.ReadByte());
		Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage] () => stream.WriteByte(1));
		Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage] () => stream.Seek(0, SeekOrigin.Begin));
		Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage] () => stream.SetLength(0));
		Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage] () => stream.Read(new byte[1], 0, 1));
		Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage] () => stream.Write(new byte[1], 0, 1));
	}

	[TestMethod]
	public void DeleteAllRemovesOnlyMatchingFreeStreams()
	{
		var streams = new SharedMemoryStreams<(string Partition, string Key)>();
		streams.OpenWrite(("first", "one")).Dispose();
		streams.OpenWrite(("first", "two")).Dispose();
		streams.OpenWrite(("second", "three")).Dispose();

		Assert.AreEqual(2, streams.DeleteAll(key => key.Partition == "first"));
		Assert.IsFalse(streams.Delete(("first", "one")));
		Assert.IsFalse(streams.Delete(("first", "two")));
		Assert.IsTrue(streams.Delete(("second", "three")));
	}

	[TestMethod]
	public async Task InMemoryStorageProviderCreatesAndRemovesPartitionedStorage()
	{
		var streams = new SharedMemoryStreams<(string? Partition, string Key)>();
		var transactionalStorage = Mock.Of<ITransactionalStorage>();
		Stream? capturedStream = null;
		var provider = new InMemoryStorageProvider
					   {
						   SharedMemoryStreams = streams,
						   TransactionalStorageFactory = stream =>
						   {
							   capturedStream = stream;
							   return new ValueTask<ITransactionalStorage>(transactionalStorage);
						   }
					   };

		Assert.AreSame(transactionalStorage, await provider.GetTransactionalStorage("partition", "key"));
		Assert.IsNotNull(capturedStream);
		capturedStream.Dispose();
		await provider.RemoveTransactionalStorage("partition", "key");
		Assert.IsFalse(streams.Delete(("partition", "key")));

		streams.OpenWrite(("partition", "one")).Dispose();
		streams.OpenWrite(("partition", "two")).Dispose();
		streams.OpenWrite(("other", "three")).Dispose();
		await provider.RemoveAllTransactionalStorage("partition");
		Assert.IsFalse(streams.Delete(("partition", "one")));
		Assert.IsFalse(streams.Delete(("partition", "two")));
		Assert.IsTrue(streams.Delete(("other", "three")));
	}
}
