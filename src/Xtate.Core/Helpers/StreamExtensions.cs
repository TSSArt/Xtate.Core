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
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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

		[SuppressMessage(category: "ReSharper", checkId: "MethodHasAsyncOverloadWithCancellation")]
		public static async ValueTask<byte[]> ReadToEndAsync(this Stream stream, CancellationToken token)
		{
			if (stream is null) throw new ArgumentNullException(nameof(stream));

			var longLength = stream.Length - stream.Position;
			var capacity = 0 <= longLength && longLength <= int.MaxValue ? (int) longLength : 0;

			var memoryStream = new MemoryStream(capacity);
			var buffer = ArrayPool<byte>.Shared.Rent(4096);
			try
			{
				while (true)
				{
					var bytesRead = await stream.ReadAsync(buffer, offset: 0, buffer.Length, token).ConfigureAwait(false);
					if (bytesRead == 0)
					{
						return memoryStream.Length == memoryStream.Capacity ? memoryStream.GetBuffer() : memoryStream.ToArray();
					}

					memoryStream.Write(buffer, offset: 0, bytesRead);
				}
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}

		private sealed class InjectedCancellationStream : DelegatedStream
		{
			private readonly CancellationToken _token;

			public InjectedCancellationStream(Stream stream, CancellationToken token)
			{
				InnerStream = stream;
				_token = token;
			}

			protected override Stream InnerStream { get; }

			public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token) =>
					IsCombinedTokenRequired(ref token) ? ReadAsyncInternal(buffer, offset, count, token) : InnerStream.ReadAsync(buffer, offset, count, token);

			private async Task<int> ReadAsyncInternal(byte[] buffer, int offset, int count, CancellationToken token)
			{
				using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _token);

				return await InnerStream.ReadAsync(buffer, offset, count, cts.Token).ConfigureAwait(false);
			}

			public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken token) =>
					IsCombinedTokenRequired(ref token) ? CopyToAsyncInternal(destination, bufferSize, token) : InnerStream.CopyToAsync(destination, bufferSize, token);

			private async Task CopyToAsyncInternal(Stream destination, int bufferSize, CancellationToken token)
			{
				using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _token);

				await InnerStream.CopyToAsync(destination, bufferSize, cts.Token).ConfigureAwait(false);
			}

			public override Task FlushAsync(CancellationToken token) => IsCombinedTokenRequired(ref token) ? FlushAsyncInternal(token) : InnerStream.FlushAsync(token);

			private async Task FlushAsyncInternal(CancellationToken token)
			{
				using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _token);

				await InnerStream.FlushAsync(cts.Token).ConfigureAwait(false);
			}

			public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token) =>
					IsCombinedTokenRequired(ref token) ? WriteAsyncInternal(buffer, offset, count, token) : InnerStream.WriteAsync(buffer, offset, count, token);

			private async Task WriteAsyncInternal(byte[] buffer, int offset, int count, CancellationToken token)
			{
				using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _token);

				await InnerStream.WriteAsync(buffer, offset, count, cts.Token).ConfigureAwait(false);
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
			public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default) =>
					IsCombinedTokenRequired(ref token) ? ReadAsyncInternal(buffer, token) : InnerStream.ReadAsync(buffer, token);

			private async ValueTask<int> ReadAsyncInternal(Memory<byte> buffer, CancellationToken token)
			{
				using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _token);

				return await InnerStream.ReadAsync(buffer, cts.Token).ConfigureAwait(false);
			}

			public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default) =>
					IsCombinedTokenRequired(ref token) ? WriteAsyncInternal(buffer, token) : InnerStream.WriteAsync(buffer, token);

			private async ValueTask WriteAsyncInternal(ReadOnlyMemory<byte> buffer, CancellationToken token)
			{
				using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _token);

				await InnerStream.WriteAsync(buffer, cts.Token).ConfigureAwait(false);
			}
#endif
		}
	}
}