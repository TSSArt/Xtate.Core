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
using System.Threading.Tasks;
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
		Assert.AreEqual(4, stream.Length);

		stream.Position = 1;
		Assert.AreEqual(1, innerStream.Position);
		Assert.AreEqual(2, stream.ReadByte());

		stream.ReadTimeout = 123;
		stream.WriteTimeout = 456;
		Assert.AreEqual(123, innerStream.ReadTimeout);
		Assert.AreEqual(456, innerStream.WriteTimeout);

		var buffer = new byte[2];
		Assert.AreEqual(2, stream.Read(buffer, 0, buffer.Length));
		CollectionAssert.AreEqual(new byte[] { 3, 4 }, buffer);

		Assert.AreEqual(0, stream.Seek(0, SeekOrigin.Begin));
		stream.WriteByte(9);
		stream.Write([8], 0, 1);
		stream.SetLength(3);
		stream.Flush();

		CollectionAssert.AreEqual(new byte[] { 9, 8, 3 }, innerStream.ToArray());

		using var copied = new MemoryStream();
		stream.Position = 0;
		stream.CopyTo(copied, 16);
		CollectionAssert.AreEqual(new byte[] { 9, 8, 3 }, copied.ToArray());

		stream.Position = 0;
		var spanBuffer = new byte[2];
		Assert.AreEqual(2, stream.Read(spanBuffer.AsSpan()));
		CollectionAssert.AreEqual(new byte[] { 9, 8 }, spanBuffer);

		stream.Position = 0;
		stream.Write(new byte[] { 7, 6 }.AsSpan());
		CollectionAssert.AreEqual(new byte[] { 7, 6, 3 }, innerStream.ToArray());

		await stream.DisposeAsync();
		Assert.IsTrue(innerStream.Disposed);
	}

	[TestMethod]
	public async Task DelegatedStreamForwardsAsyncAndApmOperations()
	{
		var innerStream = new ProbeStream([1, 2, 3, 4]);
		using var stream = new TestDelegatedStream(innerStream);

		var beginReadBuffer = new byte[1];
		var readResult = stream.BeginRead(beginReadBuffer, 0, 1, callback: null, state: null);
		Assert.AreEqual(1, stream.EndRead(readResult));
		CollectionAssert.AreEqual(new byte[] { 1 }, beginReadBuffer);

		var writeResult = stream.BeginWrite([9], 0, 1, callback: null, state: null);
		stream.EndWrite(writeResult);

		stream.Position = 0;
		var asyncBuffer = new byte[2];
		Assert.AreEqual(2, await stream.ReadAsync(asyncBuffer, 0, asyncBuffer.Length, CancellationToken.None));
		CollectionAssert.AreEqual(new byte[] { 1, 9 }, asyncBuffer);

		await stream.WriteAsync([5], 0, 1, CancellationToken.None);
		await stream.FlushAsync(CancellationToken.None);

		stream.Position = 0;
		var memoryBuffer = new byte[3];
		Assert.AreEqual(3, await stream.ReadAsync(memoryBuffer.AsMemory(), CancellationToken.None));
		CollectionAssert.AreEqual(new byte[] { 1, 9, 5 }, memoryBuffer);

		stream.Position = 0;
		await stream.WriteAsync(new byte[] { 4, 3 }.AsMemory(), CancellationToken.None);

		using var copied = new MemoryStream();
		stream.Position = 0;
		await stream.CopyToAsync(copied, 16, CancellationToken.None);
		CollectionAssert.AreEqual(new byte[] { 4, 3, 5, 4 }, copied.ToArray());
	}

	private sealed class TestDelegatedStream(Stream innerStream) : DelegatedStream(innerStream);

	private sealed class ProbeStream : MemoryStream
	{
		private int _readTimeout;

		private int _writeTimeout;

		public ProbeStream(byte[] bytes)
		{
			Write(bytes, 0, bytes.Length);
			Position = 0;
		}

		public override bool CanTimeout => true;

		public bool Disposed { get; private set; }

		public override int ReadTimeout
		{
			get => _readTimeout;
			set => _readTimeout = value;
		}

		public override int WriteTimeout
		{
			get => _writeTimeout;
			set => _writeTimeout = value;
		}

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
