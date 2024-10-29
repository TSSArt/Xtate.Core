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
using System.Net.Mime;
using System.Text;
using Xtate.XInclude;

namespace Xtate.Core;

public class Resource(Stream stream, ContentType? contentType) : IDisposable, IAsyncDisposable, IXIncludeResource
{
	private readonly DisposingToken _disposingToken = new();

	private readonly Stream _stream = stream ?? throw new ArgumentNullException(nameof(stream));

	private byte[]? _bytes;

	private string? _content;

	public Encoding Encoding => !string.IsNullOrEmpty(ContentType?.CharSet) ? Encoding.GetEncoding(ContentType.CharSet) : Encoding.UTF8;

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		Dispose(false);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IXIncludeResource

	ValueTask<Stream> IXIncludeResource.GetStream() => GetStream(doNotCache: true);

	public ContentType? ContentType { get; } = contentType;

#endregion

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposingToken.Dispose();
		}
	}

	protected virtual ValueTask DisposeAsyncCore()
	{
		_disposingToken.Dispose();

		return default;
	}

	public async ValueTask<string> GetContent()
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
			using var reader = new StreamReader(_stream.InjectCancellationToken(_disposingToken.Token), Encoding, detectEncodingFromByteOrderMarks: true);

			return _content = await reader.ReadToEndAsync().ConfigureAwait(false);
		}
	}

	public async ValueTask<byte[]> GetBytes()
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
			return _bytes = await _stream.ReadToEndAsync(_disposingToken.Token).ConfigureAwait(false);
		}
	}

	public async ValueTask<Stream> GetStream(bool doNotCache)
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
			_bytes = await _stream.ReadToEndAsync(_disposingToken.Token).ConfigureAwait(false);

			return new MemoryStream(_bytes, writable: false);
		}
	}
}