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
using Xtate.ResourceLoaders.Internal;

namespace Xtate.ResourceLoaders.Extensions;

internal static class StreamExtensions
{
	extension(Stream stream)
	{
		public Stream InjectCancellationToken(CancellationToken token) => new InjectedCancellationStream(stream, token);

		public async ValueTask<byte[]> ReadToEndAsync(CancellationToken token)
		{
			var longLength = stream.CanSeek ? stream.Length - stream.Position : 0;
			var capacity = longLength is >= 0 and <= int.MaxValue ? (int) longLength : 0;

			var memoryStream = new MemoryStream(capacity);
			var buffer = ArrayPool<byte>.Shared.Rent(65536);

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

		public byte[] ReadToEnd(CancellationToken token)
		{
			var longLength = stream.CanSeek ? stream.Length - stream.Position : 0;
			var capacity = longLength is >= 0 and <= int.MaxValue ? (int) longLength : 0;

			var memoryStream = new MemoryStream(capacity);
			var buffer = ArrayPool<byte>.Shared.Rent(65536);

			try
			{
				while (true)
				{
					token.ThrowIfCancellationRequested();

					var bytesRead = stream.Read(buffer, offset: 0, buffer.Length);

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
	}
}