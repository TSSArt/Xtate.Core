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
public class DelegatedStreamCoverageTest
{
	[TestMethod]
	public async Task DelegatedStreamForwardsPropertiesAndSynchronousOperations()
	{
		var innerStream = new ProbeStream([1, 2, 3, 4]);
		using var stream = new TestDelegatedStream(innerStream);

		Assert.IsTrue(stream.CanRead);
		Assert.IsTrue(stream.CanSeek);
		Assert.IsTrue(stream.CanWrite);
		Assert.IsTrue(stream.CanTimeout);
		Assert.AreEqual(expected: 4, stream.Length);

		stream.Position = 1;
		Assert.AreEqual(expected: 1, innerStream.Position);
		Assert.AreEqual(expected: 2, stream.ReadByte());

		stream.ReadTimeout = 123;
		stream.WriteTimeout = 456;
		Assert.AreEqual(expected: 123, stream.ReadTimeout);
		Assert.AreEqual(expected: 456, stream.WriteTimeout);
		Assert.AreEqual(expected: 123, innerStream.ReadTimeout);
		Assert.AreEqual(expected: 456, innerStream.WriteTimeout);

		var buffer = new byte[2];
		Assert.AreEqual(expected: 2, stream.Read(buffer, offset: 0, buffer.Length));
		CollectionAssert.AreEqual(new byte[] { 3, 4 }, buffer);

		Assert.AreEqual(expected: 0, stream.Seek(offset: 0, SeekOrigin.Begin));
		stream.WriteByte(9);
		stream.Write([8], offset: 0, count: 1);
		stream.SetLength(3);
		stream.Flush();

		CollectionAssert.AreEqual(new byte[] { 9, 8, 3 }, innerStream.ToArray());

		using var copied = new MemoryStream();
		stream.Position = 0;
		stream.CopyTo(copied, bufferSize: 16);
		CollectionAssert.AreEqual(new byte[] { 9, 8, 3 }, copied.ToArray());

		stream.Position = 0;
		var spanBuffer = new byte[2];
		Assert.AreEqual(expected: 2, stream.Read(spanBuffer.AsSpan()));
		CollectionAssert.AreEqual(new byte[] { 9, 8 }, spanBuffer);

		stream.Position = 0;
		stream.Write(new byte[] { 7, 6 }.AsSpan());
		CollectionAssert.AreEqual(new byte[] { 7, 6, 3 }, innerStream.ToArray());

		await stream.DisposeAsync();
		Assert.IsTrue(innerStream.Disposed);
	}

	[TestMethod]
	public void DelegatedStreamSynchronousDisposeClosesInnerStream()
	{
		var innerStream = new ProbeStream([1]);
		var stream = new TestDelegatedStream(innerStream);

		stream.DisposeCore(disposing: true);

		Assert.IsTrue(innerStream.Disposed);
	}

	[TestMethod]
	public async Task DelegatedStreamForwardsAsyncAndApmOperations()
	{
		var innerStream = new ProbeStream([1, 2, 3, 4]);
		using var stream = new TestDelegatedStream(innerStream);

		var beginReadBuffer = new byte[1];
		var readResult = stream.BeginRead(beginReadBuffer, offset: 0, count: 1, callback: null, state: null);
		Assert.AreEqual(expected: 1, stream.EndRead(readResult));
		CollectionAssert.AreEqual(new byte[] { 1 }, beginReadBuffer);

		var writeResult = stream.BeginWrite([9], offset: 0, count: 1, callback: null, state: null);
		stream.EndWrite(writeResult);

		stream.Position = 0;
		var asyncBuffer = new byte[2];
		Assert.AreEqual(expected: 2, await stream.ReadAsync(asyncBuffer, offset: 0, asyncBuffer.Length, CancellationToken.None));
		CollectionAssert.AreEqual(new byte[] { 1, 9 }, asyncBuffer);

		await stream.WriteAsync([5], offset: 0, count: 1, CancellationToken.None);
		await stream.FlushAsync(CancellationToken.None);

		stream.Position = 0;
		var memoryBuffer = new byte[3];
		Assert.AreEqual(expected: 3, await stream.ReadAsync(memoryBuffer.AsMemory(), CancellationToken.None));
		CollectionAssert.AreEqual(new byte[] { 1, 9, 5 }, memoryBuffer);

		stream.Position = 0;
		await stream.WriteAsync(new byte[] { 4, 3 }.AsMemory(), CancellationToken.None);

		using var copied = new MemoryStream();
		stream.Position = 0;
		await stream.CopyToAsync(copied, bufferSize: 16, CancellationToken.None);
		CollectionAssert.AreEqual(new byte[] { 4, 3, 5, 4 }, copied.ToArray());
	}

	private sealed class TestDelegatedStream(Stream innerStream) : DelegatedStream(innerStream)
	{
		public void DisposeCore(bool disposing) => base.Dispose(disposing);
	}

	private sealed class ProbeStream : MemoryStream
	{
		public ProbeStream(byte[] bytes)
		{
			Write(bytes, offset: 0, bytes.Length);
			Position = 0;
		}

		public override bool CanTimeout => true;

		public bool Disposed { get; private set; }

		public override int ReadTimeout { get; set; }

		public override int WriteTimeout { get; set; }

		protected override void Dispose(bool disposing)
		{
			Disposed = true;

			base.Dispose(disposing);
		}

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1
		public override async ValueTask DisposeAsync()
		{
			Disposed = true;

			await base.DisposeAsync();
		}
#endif
	}
}
