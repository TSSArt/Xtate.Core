// Copyright © 2019-2025 Sergii Artemenko
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

namespace Xtate.Core;

internal abstract class DelegatedStream : Stream
{
    protected abstract Stream InnerStream { get; }

    public override bool CanRead => InnerStream.CanRead;

    public override bool CanSeek => InnerStream.CanSeek;

    public override bool CanTimeout => InnerStream.CanTimeout;

    public override bool CanWrite => InnerStream.CanWrite;

    public override long Length => InnerStream.Length;

    public override long Position
    {
        get => InnerStream.Position;
        set => InnerStream.Position = value;
    }

    public override int ReadTimeout
    {
        get => InnerStream.ReadTimeout;
        set => InnerStream.ReadTimeout = value;
    }

    public override int WriteTimeout
    {
        get => InnerStream.WriteTimeout;
        set => InnerStream.WriteTimeout = value;
    }

    public override IAsyncResult BeginRead(byte[] buffer,
                                           int offset,
                                           int count,
                                           AsyncCallback? callback,
                                           object? state) =>
        InnerStream.BeginRead(buffer, offset, count, callback, state);

    public override IAsyncResult BeginWrite(byte[] buffer,
                                            int offset,
                                            int count,
                                            AsyncCallback? callback,
                                            object? state) =>
        InnerStream.BeginWrite(buffer, offset, count, callback, state);

    public override void Close() => InnerStream.Close();

    public override int EndRead(IAsyncResult asyncResult) => InnerStream.EndRead(asyncResult);

    public override void EndWrite(IAsyncResult asyncResult) => InnerStream.EndWrite(asyncResult);

    public override void Flush() => InnerStream.Flush();

    public override int Read(byte[] buffer, int offset, int count) => InnerStream.Read(buffer, offset, count);

    public override int ReadByte() => InnerStream.ReadByte();

    public override long Seek(long offset, SeekOrigin origin) => InnerStream.Seek(offset, origin);

    public override void SetLength(long value) => InnerStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => InnerStream.Write(buffer, offset, count);

    public override void WriteByte(byte value) => InnerStream.WriteByte(value);

    public override Task<int> ReadAsync(byte[] buffer,
                                        int offset,
                                        int count,
                                        CancellationToken token) =>
        InnerStream.ReadAsync(buffer, offset, count, token);

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken token) => InnerStream.CopyToAsync(destination, bufferSize, token);

    public override Task FlushAsync(CancellationToken token) => InnerStream.FlushAsync(token);

    public override Task WriteAsync(byte[] buffer,
                                    int offset,
                                    int count,
                                    CancellationToken token) =>
        InnerStream.WriteAsync(buffer, offset, count, token);

#if NET5_0_OR_GREATER
    public override void CopyTo(Stream destination, int bufferSize) => InnerStream.CopyTo(destination, bufferSize);

#endif

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1
    public override int Read(Span<byte> buffer) => InnerStream.Read(buffer);

    public override void Write(ReadOnlySpan<byte> buffer) => InnerStream.Write(buffer);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default) => InnerStream.ReadAsync(buffer, token);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default) => InnerStream.WriteAsync(buffer, token);

#endif
}