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
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;
using Xtate.XInclude;

namespace Xtate
{
	[PublicAPI]
	public sealed class Resource : IDisposable, IAsyncDisposable, IXIncludeResource
	{
		private readonly Stream  _stream;
		private          byte[]? _bytes;
		private          string? _content;

		public Resource(Stream stream, ContentType? contentType = default)
		{
			_stream = stream ?? throw new ArgumentNullException(nameof(stream));
			ContentType = contentType;
		}

		public Encoding Encoding => !string.IsNullOrEmpty(ContentType?.CharSet) ? Encoding.GetEncoding(ContentType.CharSet) : Encoding.UTF8;

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync() => _stream.DisposeAsync();

	#endregion

	#region Interface IDisposable

		public void Dispose() => _stream.Dispose();

	#endregion

	#region Interface IXIncludeResource

		ValueTask<Stream> IXIncludeResource.GetStream() => GetStream(doNotCache: true, token: default);

		public ContentType? ContentType { get; }

	#endregion

		public async ValueTask<string> GetContent(CancellationToken token)
		{
			if (_content is not null)
			{
				return _content;
			}

			if (_bytes is not null)
			{
				using var reader = new StreamReader(new MemoryStream(_bytes), Encoding, detectEncodingFromByteOrderMarks: true);

				return _content = await reader.ReadToEndAsync().ConfigureAwait(false);
			}

			await using (_stream.ConfigureAwait(false))
			{
				using var reader = new StreamReader(_stream.InjectCancellationToken(token), Encoding, detectEncodingFromByteOrderMarks: true);

				return _content = await reader.ReadToEndAsync().ConfigureAwait(false);
			}
		}

		public async ValueTask<byte[]> GetBytes(CancellationToken token)
		{
			if (_bytes is not null)
			{
				return _bytes;
			}

			if (_content is not null)
			{
				return _bytes = Encoding.GetBytes(_content);
			}

			await using (_stream.ConfigureAwait(false))
			{
				return _bytes = await _stream.ReadToEndAsync(token).ConfigureAwait(false);
			}
		}

		public async ValueTask<Stream> GetStream(bool doNotCache, CancellationToken token)
		{
			if (_bytes is not null)
			{
				return new MemoryStream(_bytes, writable: false);
			}

			if (_content is not null)
			{
				return new MemoryStream(Encoding.GetBytes(_content));
			}

			if (doNotCache)
			{
				return _stream;
			}

			await using (_stream.ConfigureAwait(false))
			{
				_bytes = await _stream.ReadToEndAsync(token).ConfigureAwait(false);

				return new MemoryStream(_bytes, writable: false);
			}
		}
	}
}