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

#if NET6_0_OR_GREATER
#pragma warning disable CA1835
#endif

using System.Buffers;
using System.IO;

namespace Xtate.Core;

public static class StreamExtensions
{
	public static Stream InjectCancellationToken(this Stream stream, CancellationToken token) => new InjectedCancellationStream(stream, token);

	[SuppressMessage(category: "ReSharper", checkId: "MethodHasAsyncOverloadWithCancellation")]
	public static async ValueTask<byte[]> ReadToEndAsync(this Stream stream, CancellationToken token)
	{
		if (stream is null) throw new ArgumentNullException(nameof(stream));

		var longLength = stream.Length - stream.Position;
		var capacity = longLength is >= 0 and <= int.MaxValue ? (int) longLength : 0;

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

#if !NET6_0_OR_GREATER
	public static ConfiguredAwaitable ConfigureAwait(this Stream stream, bool continueOnCapturedContext) => new(stream, continueOnCapturedContext);

	public static ValueTask DisposeAsync(this Stream stream)
	{
		if (stream is null) throw new ArgumentNullException(nameof(stream));

		stream.Dispose();

		return default;
	}

	public readonly struct ConfiguredAwaitable(Stream stream, bool continueOnCapturedContext)
	{
		public ConfiguredValueTaskAwaitable DisposeAsync() => stream.DisposeAsync().ConfigureAwait(continueOnCapturedContext);
	}

#endif
}