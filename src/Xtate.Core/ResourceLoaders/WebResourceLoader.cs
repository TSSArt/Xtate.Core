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

using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Mime;

namespace Xtate.Core;

public class WebResourceLoaderProvider : ResourceLoaderProviderBase<WebResourceLoader>
{
	protected override bool CanHandle(Uri uri) => uri is { IsAbsoluteUri: true, Scheme: @"http" or @"https" };
}

public class WebResourceLoader : IResourceLoader, IDisposable
{
	private readonly DisposingToken _disposingToken = new();

	public required Func<Stream, ContentType?, Resource> ResourceFactory { private get; [UsedImplicitly] init; }

	public required Func<HttpClient> HttpClientFactory { private get; [UsedImplicitly] init; }

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IResourceLoader

	public virtual async ValueTask<Resource> Request(Uri uri, NameValueCollection? headers)
	{
		Infra.Requires(uri);

		using var request = CreateRequestMessage(uri, headers);
		using var httpClient = HttpClientFactory();

		var response = await httpClient.SendAsync(request, _disposingToken.Token).ConfigureAwait(false);
		var contentType = response.Content.Headers.ContentType?.MediaType is { Length: > 0 } value ? new ContentType(value) : null;

#if NET5_0_OR_GREATER
		var stream = await response.Content.ReadAsStreamAsync(_disposingToken.Token).ConfigureAwait(false);
#else
		var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif

		return ResourceFactory(stream, contentType);
	}

#endregion

	protected virtual HttpRequestMessage CreateRequestMessage(Uri uri, NameValueCollection? headers)
	{
		var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

		if (headers is not null)
		{
			for (var i = 0; i < headers.Count; i ++)
			{
				if (headers.GetKey(i) is { Length: > 0 } key)
				{
					httpRequestMessage.Headers.Add(key, headers.Get(i));
				}
			}
		}

		return httpRequestMessage;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposingToken.Dispose();
		}
	}
}