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

#if !NET7_0_OR_GREATER
using System.Buffers;
using System.Text;

namespace System.IO;

internal static class StreamReaderPolyfills
{
	extension(StreamReader reader)
	{
		public async Task<string> ReadToEndAsync(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			var stream = reader.BaseStream;
			var sb = new StringBuilder(stream.CanSeek ? (int) (stream.Length - stream.Position) : 0);

			var buffer = ArrayPool<char>.Shared.Rent(1024);

			try
			{
				int count;

				while ((count = await reader.ReadAsync(buffer, index: 0, buffer.Length).ConfigureAwait(false)) > 0)
				{
					token.ThrowIfCancellationRequested();
					sb.Append(buffer, startIndex: 0, count);
				}

				return sb.ToString();
			}
			finally
			{
				ArrayPool<char>.Shared.Return(buffer);
			}
		}
	}
}

#endif