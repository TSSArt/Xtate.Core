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

using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace Xtate.ResourceLoaders.Internal;

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1
internal abstract class DelegatedStream(Stream innerStream) : Stream
#else
internal abstract class DelegatedStream(Stream innerStream) : Stream, IAsyncDisposable
#endif
{
	public override bool CanRead => innerStream.CanRead;

	public override bool CanSeek => innerStream.CanSeek;

	public override bool CanTimeout => innerStream.CanTimeout;

	public override bool CanWrite => innerStream.CanWrite;

	public override long Length => innerStream.Length;

	public override long Position
	{
		get => innerStream.Position;
		set => innerStream.Position = value;
	}

	public override int ReadTimeout
	{
		get => innerStream.ReadTimeout;
		set => innerStream.ReadTimeout = value;
	}

	public override int WriteTimeout
	{
		get => innerStream.WriteTimeout;
		set => innerStream.WriteTimeout = value;
	}

	public override IAsyncResult BeginRead(byte[] buffer,
										   int offset,
										   int count,
										   AsyncCallback? callback,
										   object? state) =>
		innerStream.BeginRead(buffer, offset, count, callback, state);

	public override IAsyncResult BeginWrite(byte[] buffer,
											int offset,
											int count,
											AsyncCallback? callback,
											object? state) =>
		innerStream.BeginWrite(buffer, offset, count, callback, state);

	public override void Close() => innerStream.Close();

	public override int EndRead(IAsyncResult asyncResult) => innerStream.EndRead(asyncResult);

	public override void EndWrite(IAsyncResult asyncResult) => innerStream.EndWrite(asyncResult);

	public override void Flush() => innerStream.Flush();

	public override int Read(byte[] buffer, int offset, int count) => innerStream.Read(buffer, offset, count);

	public override int ReadByte() => innerStream.ReadByte();

	public override long Seek(long offset, SeekOrigin origin) => innerStream.Seek(offset, origin);

	public override void SetLength(long value) => innerStream.SetLength(value);

	public override void Write(byte[] buffer, int offset, int count) => innerStream.Write(buffer, offset, count);

	public override void WriteByte(byte value) => innerStream.WriteByte(value);

	public override Task<int> ReadAsync(byte[] buffer,
										int offset,
										int count,
										CancellationToken token) =>
		innerStream.ReadAsync(buffer, offset, count, token);

	public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken token) => innerStream.CopyToAsync(destination, bufferSize, token);

	public override Task FlushAsync(CancellationToken token) => innerStream.FlushAsync(token);

	public override Task WriteAsync(byte[] buffer,
									int offset,
									int count,
									CancellationToken token) =>
		innerStream.WriteAsync(buffer, offset, count, token);

	protected override void Dispose(bool disposing)
	{
		innerStream.Dispose();

		base.Dispose(disposing);
	}

#if NET5_0_OR_GREATER
	public override void CopyTo(Stream destination, int bufferSize) => innerStream.CopyTo(destination, bufferSize);
#else
	public new virtual void CopyTo(Stream destination, int bufferSize) => innerStream.CopyTo(destination, bufferSize);
#endif

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1
	public override int Read(Span<byte> buffer) => innerStream.Read(buffer);

	public override void Write(ReadOnlySpan<byte> buffer) => innerStream.Write(buffer);

	public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default) => innerStream.ReadAsync(buffer, token);

	public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default) => innerStream.WriteAsync(buffer, token);

	public override async ValueTask DisposeAsync()
	{
		await innerStream.DisposeAsync().ConfigureAwait(false);

		await base.DisposeAsync().ConfigureAwait(false);
	}
#else
	public virtual int Read(Span<byte> buffer)
	{
		var buf = ArrayPool<byte>.Shared.Rent(buffer.Length);

		try
		{
			var count = innerStream.Read(buf, offset: 0, buffer.Length);
			buf.AsSpan(start: 0, count).CopyTo(buffer);

			return count;
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buf);
		}
	}

	public virtual void Write(ReadOnlySpan<byte> buffer)
	{
		var buf = ArrayPool<byte>.Shared.Rent(buffer.Length);

		try
		{
			buffer.CopyTo(buf);
			innerStream.Write(buf, offset: 0, buffer.Length);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buf);
		}
	}

	public virtual ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
	{
		if (MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>) buffer, out var array))
		{
			return new ValueTask<int>(innerStream.ReadAsync(array.Array!, array.Offset, array.Count, token));
		}

		var buf = ArrayPool<byte>.Shared.Rent(buffer.Length);

		return FinishReadAsync(innerStream.ReadAsync(buf, offset: 0, buffer.Length, token), buf, buffer);

		static async ValueTask<int> FinishReadAsync(Task<int> readTask, byte[] localBuffer, Memory<byte> localDestination)
		{
			try
			{
				var result = await readTask.ConfigureAwait(false);
				localBuffer.AsSpan(start: 0, result).CopyTo(localDestination.Span);

				return result;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(localBuffer);
			}
		}
	}

	public virtual ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default)
	{
		if (MemoryMarshal.TryGetArray(buffer, out var array))
		{
			return new ValueTask(innerStream.WriteAsync(array.Array!, array.Offset, array.Count, token));
		}

		var buf = ArrayPool<byte>.Shared.Rent(buffer.Length);
		buffer.Span.CopyTo(buf);

		return FinishWriteAsync(innerStream.WriteAsync(buf, offset: 0, buffer.Length, token), buf);

		static async ValueTask FinishWriteAsync(Task writeTask, byte[] localBuffer)
		{
			try
			{
				await writeTask.ConfigureAwait(false);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(localBuffer);
			}
		}
	}

	public virtual ValueTask DisposeAsync() => innerStream.DisposeAsync();
#endif
}