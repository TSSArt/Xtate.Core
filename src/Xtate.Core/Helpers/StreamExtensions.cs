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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.Core
{
	[PublicAPI]
	public static class StreamExtensions
	{
		public static ConfiguredAwaitable ConfigureAwait(this Stream stream, bool continueOnCapturedContext) => new(stream, continueOnCapturedContext);

#if NET461 || NETSTANDARD2_0
		public static ValueTask DisposeAsync(this Stream stream)
		{
			if (stream is null) throw new ArgumentNullException(nameof(stream));

			stream.Dispose();

			return default;
		}
#endif

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

		[PublicAPI]
		public readonly struct ConfiguredAwaitable
		{
			private readonly bool _continueOnCapturedContext;

			private readonly Stream _stream;

			public ConfiguredAwaitable(Stream stream, bool continueOnCapturedContext)
			{
				_stream = stream;
				_continueOnCapturedContext = continueOnCapturedContext;
			}

			public ConfiguredValueTaskAwaitable DisposeAsync() => _stream.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
		}
	}
}