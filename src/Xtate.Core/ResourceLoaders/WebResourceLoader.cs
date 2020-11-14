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
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public sealed class WebResourceLoader : IResourceLoader
	{
		private ImmutableDictionary<Uri, WeakReference<Resource>> _cachedWebResources = ImmutableDictionary<Uri, WeakReference<Resource>>.Empty;

		public static IResourceLoader Instance { get; } = new WebResourceLoader();

	#region Interface IResourceLoader

		public bool CanHandle(Uri uri)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			return uri.IsAbsoluteUri && (uri.Scheme == "http" || uri.Scheme == "https");
		}

		public async ValueTask<Resource> Request(Uri uri, CancellationToken token)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			using var client = new HttpClient();
			HttpResponseMessage responseMessage;

			var cachedWebResources = _cachedWebResources;
			if (cachedWebResources.TryGetValue(uri, out var weakReference) && weakReference.TryGetTarget(out var resource))
			{
				client.DefaultRequestHeaders.IfModifiedSince = resource.ModifiedDate;
				responseMessage = await client.GetAsync(uri, token).ConfigureAwait(false);
				if (responseMessage.StatusCode == HttpStatusCode.NotModified)
				{
					return resource;
				}
			}
			else
			{
				responseMessage = await client.GetAsync(uri, token).ConfigureAwait(false);
			}

			responseMessage.EnsureSuccessStatusCode();

			var content = responseMessage.Content;
			var headers = content.Headers;
			var contentType = headers.ContentType is { } ct ? new ContentType(ct.ToString()) : new ContentType();
			var lastModified = headers.LastModified;

#if NET461 || NETSTANDARD2_0
			var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
#else
			var stream = await content.ReadAsStreamAsync(token).ConfigureAwait(false);
#endif

			await using (stream.ConfigureAwait(false))
			{
				if (headers.ContentLength is { } longLen && longLen < int.MaxValue)
				{
					var count = (int) longLen;
					var bytes = new byte[count];
					await stream.ReadAsync(bytes, offset: 0, count, token).ConfigureAwait(false);
					resource = new Resource(uri, contentType, lastModified, bytes: bytes);
				}
				else
				{
					var memStream = new MemoryStream();
					await using (memStream.ConfigureAwait(false))
					{
						await stream.CopyToAsync(memStream, bufferSize: 4096, token).ConfigureAwait(false);
						resource = new Resource(uri, contentType, lastModified, bytes: memStream.ToArray());
					}
				}
			}

			if (weakReference is null)
			{
				weakReference = new WeakReference<Resource>(resource);
			}
			else
			{
				weakReference.SetTarget(resource);
			}

			_cachedWebResources = cachedWebResources.SetItem(uri, weakReference);

			return resource;
		}

		public ValueTask<XmlReader> RequestXmlReader(Uri uri, XmlReaderSettings? readerSettings = default, XmlParserContext? parserContext = default, CancellationToken token = default)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			try
			{
				return new ValueTask<XmlReader>(XmlReader.Create(uri.ToString(), readerSettings, parserContext));
			}
			catch (Exception ex)
			{
				return new ValueTask<XmlReader>(Task.FromException<XmlReader>(ex));
			}
		}

	#endregion
	}
}