// Copyright © 2019-2024 Sergii Artemenko
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

internal sealed class InjectedCancellationStream(Stream stream, CancellationToken extToken) : DelegatedStream
{
	protected override Stream InnerStream { get; } = stream;

	public override Task<int> ReadAsync(byte[] buffer,
										int offset,
										int count,
										CancellationToken token) =>
		IsCombinedTokenRequired(ref token) ? ReadAsyncInternal(buffer, offset, count, token) : InnerStream.ReadAsync(buffer, offset, count, token);

	private async Task<int> ReadAsyncInternal(byte[] buffer,
											  int offset,
											  int count,
											  CancellationToken token)
	{
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, extToken);

		return await InnerStream.ReadAsync(buffer, offset, count, cts.Token).ConfigureAwait(false);
	}

	public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken token) =>
		IsCombinedTokenRequired(ref token) ? CopyToAsyncInternal(destination, bufferSize, token) : InnerStream.CopyToAsync(destination, bufferSize, token);

	private async Task CopyToAsyncInternal(Stream destination, int bufferSize, CancellationToken token)
	{
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, extToken);

		await InnerStream.CopyToAsync(destination, bufferSize, cts.Token).ConfigureAwait(false);
	}

	public override Task FlushAsync(CancellationToken token) => IsCombinedTokenRequired(ref token) ? FlushAsyncInternal(token) : InnerStream.FlushAsync(token);

	private async Task FlushAsyncInternal(CancellationToken token)
	{
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, extToken);

		await InnerStream.FlushAsync(cts.Token).ConfigureAwait(false);
	}

	public override Task WriteAsync(byte[] buffer,
									int offset,
									int count,
									CancellationToken token) =>
		IsCombinedTokenRequired(ref token) ? WriteAsyncInternal(buffer, offset, count, token) : InnerStream.WriteAsync(buffer, offset, count, token);

	private async Task WriteAsyncInternal(byte[] buffer,
										  int offset,
										  int count,
										  CancellationToken token)
	{
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, extToken);

		await InnerStream.WriteAsync(buffer, offset, count, cts.Token).ConfigureAwait(false);
	}

	private bool IsCombinedTokenRequired(ref CancellationToken token)
	{
		if (token.CanBeCanceled)
		{
			return extToken.CanBeCanceled;
		}

		token = extToken;

		return false;
	}

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1
	public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default) => IsCombinedTokenRequired(ref token) ? ReadAsyncInternal(buffer, token) : InnerStream.ReadAsync(buffer, token);

	private async ValueTask<int> ReadAsyncInternal(Memory<byte> buffer, CancellationToken token)
	{
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, extToken);

		return await InnerStream.ReadAsync(buffer, cts.Token).ConfigureAwait(false);
	}

	public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default) => IsCombinedTokenRequired(ref token) ? WriteAsyncInternal(buffer, token) : InnerStream.WriteAsync(buffer, token);

	private async ValueTask WriteAsyncInternal(ReadOnlyMemory<byte> buffer, CancellationToken token)
	{
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, extToken);

		await InnerStream.WriteAsync(buffer, cts.Token).ConfigureAwait(false);
	}

#endif
}