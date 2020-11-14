#region Copyright © 2019-2020 Sergii Artemenko

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
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public static class StreamExtensions
	{
		public static ConfiguredStreamAwaitable ConfigureAwait(this Stream stream, bool continueOnCapturedContext) => new(stream, continueOnCapturedContext);

		public static Stream InjectCancellationToken(this Stream stream, CancellationToken token) => new InjectedCancellationStream(stream, token);

		private class InjectedCancellationStream : Stream
		{
			private readonly Stream            _stream;
			private readonly CancellationToken _token;

			public InjectedCancellationStream(Stream stream, CancellationToken token)
			{
				_stream = stream;
				_token = token;
			}

			public override bool CanRead => _stream.CanRead;

			public override bool CanSeek => _stream.CanSeek;

			public override bool CanTimeout => _stream.CanTimeout;

			public override bool CanWrite => _stream.CanWrite;

			public override long Length => _stream.Length;

			public override long Position
			{
				get => _stream.Position;
				set => _stream.Position = value;
			}

			public override int ReadTimeout
			{
				get => _stream.ReadTimeout;
				set => _stream.ReadTimeout = value;
			}

			public override int WriteTimeout
			{
				get => _stream.WriteTimeout;
				set => _stream.WriteTimeout = value;
			}

			public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => _stream.BeginRead(buffer, offset, count, callback, state);

			public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => _stream.BeginWrite(buffer, offset, count, callback, state);

			public override void Close() => _stream.Close();

			public override int EndRead(IAsyncResult asyncResult) => _stream.EndRead(asyncResult);

			public override void EndWrite(IAsyncResult asyncResult) => _stream.EndWrite(asyncResult);

			public override void Flush() => _stream.Flush();

			public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

			public override int ReadByte() => _stream.ReadByte();

			public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

			public override void SetLength(long value) => _stream.SetLength(value);

			public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

			public override void WriteByte(byte value) => _stream.WriteByte(value);

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_stream.Dispose();
				}

				base.Dispose(disposing);
			}

			public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token) =>
					IsCombinedTokenRequired(ref token) ? ReadAsyncInternal(buffer, offset, count, token) : _stream.ReadAsync(buffer, offset, count, token);

			private async Task<int> ReadAsyncInternal(byte[] buffer, int offset, int count, CancellationToken token)
			{
				using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _token);

				return await _stream.ReadAsync(buffer, offset, count, cts.Token).ConfigureAwait(false);
			}

			public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken token) =>
					IsCombinedTokenRequired(ref token) ? CopyToAsyncInternal(destination, bufferSize, token) : _stream.CopyToAsync(destination, bufferSize, token);

			private async Task CopyToAsyncInternal(Stream destination, int bufferSize, CancellationToken token)
			{
				using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _token);

				await _stream.CopyToAsync(destination, bufferSize, cts.Token).ConfigureAwait(false);
			}

			public override Task FlushAsync(CancellationToken token) => IsCombinedTokenRequired(ref token) ? FlushAsyncInternal(token) : _stream.FlushAsync(token);

			private async Task FlushAsyncInternal(CancellationToken token)
			{
				using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _token);

				await _stream.FlushAsync(cts.Token).ConfigureAwait(false);
			}

			public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token) =>
					IsCombinedTokenRequired(ref token) ? WriteAsyncInternal(buffer, offset, count, token) : _stream.WriteAsync(buffer, offset, count, token);

			private async Task WriteAsyncInternal(byte[] buffer, int offset, int count, CancellationToken token)
			{
				using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _token);

				await _stream.WriteAsync(buffer, offset, count, cts.Token).ConfigureAwait(false);
			}

			private bool IsCombinedTokenRequired(ref CancellationToken token)
			{
				if (token.CanBeCanceled)
				{
					return _token.CanBeCanceled;
				}

				token = _token;

				return false;
			}

#if !NET461 && !NETSTANDARD2_0
			public override void CopyTo(Stream destination, int bufferSize) => _stream.CopyTo(destination, bufferSize);

			public override int Read(Span<byte> buffer) => _stream.Read(buffer);

			public override void Write(ReadOnlySpan<byte> buffer) => _stream.Write(buffer);

			public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default) =>
					IsCombinedTokenRequired(ref token) ? ReadAsyncInternal(buffer, token) : _stream.ReadAsync(buffer, token);

			private async ValueTask<int> ReadAsyncInternal(Memory<byte> buffer, CancellationToken token)
			{
				using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _token);

				return await _stream.ReadAsync(buffer, cts.Token).ConfigureAwait(false);
			}

			public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default) =>
					IsCombinedTokenRequired(ref token) ? WriteAsyncInternal(buffer, token) : _stream.WriteAsync(buffer, token);

			private async ValueTask WriteAsyncInternal(ReadOnlyMemory<byte> buffer, CancellationToken token)
			{
				using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _token);

				await _stream.WriteAsync(buffer, cts.Token).ConfigureAwait(false);
			}

			public override async ValueTask DisposeAsync()
			{
				await _stream.DisposeAsync().ConfigureAwait(false);
				await base.DisposeAsync().ConfigureAwait(false);
			}

#endif
		}
	}

	[PublicAPI]
	public readonly struct ConfiguredStreamAwaitable
	{
		private readonly Stream _stream;
		private readonly bool   _continueOnCapturedContext;

		public ConfiguredStreamAwaitable(Stream stream, bool continueOnCapturedContext)
		{
			_stream = stream;
			_continueOnCapturedContext = continueOnCapturedContext;
		}

#if NET461 || NETSTANDARD2_0
		public ConfiguredValueTaskAwaitable DisposeAsync()
		{
			_stream.Dispose();

			return new ValueTask().ConfigureAwait(_continueOnCapturedContext);
		}
#else
		public ConfiguredValueTaskAwaitable DisposeAsync() => _stream.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
#endif
	}
}