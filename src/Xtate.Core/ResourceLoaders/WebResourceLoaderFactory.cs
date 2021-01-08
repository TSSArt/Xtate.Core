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
using System.Collections.Specialized;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public sealed class WebResourceLoaderFactory : IResourceLoaderFactory
	{
		private readonly Activator _activator = new();

		public static IResourceLoaderFactory Instance { get; } = new WebResourceLoaderFactory();

	#region Interface IResourceLoaderFactory

		public ValueTask<IResourceLoaderFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, Uri uri, CancellationToken token)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			return CanHandle(uri) ? new ValueTask<IResourceLoaderFactoryActivator?>(_activator) : default;
		}

	#endregion

		private static bool CanHandle(Uri uri) => uri.IsAbsoluteUri && (uri.Scheme == @"http" || uri.Scheme == @"https");

		private sealed class Activator : IResourceLoaderFactoryActivator
		{
			private readonly ResourceLoader _resourceLoader = new();

		#region Interface IResourceLoaderFactoryActivator

			public ValueTask<IResourceLoader> CreateResourceLoader(IFactoryContext factoryContext, CancellationToken token) => new(_resourceLoader);

		#endregion
		}

		private sealed class ResourceLoader : IResourceLoader
		{
		#region Interface IResourceLoader

			public async ValueTask<Resource> Request(Uri uri, NameValueCollection? headers, CancellationToken token)
			{
				if (uri is null) throw new ArgumentNullException(nameof(uri));

				Infrastructure.Assert(CanHandle(uri));

				var request = WebRequest.CreateHttp(uri);
				SetHeader(request, headers);

				var response = await GetResponse(request, token).ConfigureAwait(false);
				var contentType = response.Headers[HttpResponseHeader.ContentType] is { Length: > 0 } val ? new ContentType(val) : null;

				return new Resource(response.GetResponseStream()!, contentType);
			}

		#endregion

			private static void SetHeader(HttpWebRequest request, NameValueCollection? headers)
			{
				if (headers is null)
				{
					return;
				}

				for (var i = 0; i < headers.Count; i ++)
				{
					if (headers.GetKey(i) is { Length: > 0 } key)
					{
						request.Headers[key] = headers.Get(i);
					}
				}
			}

			private static async Task<HttpWebResponse> GetResponse(HttpWebRequest request, CancellationToken token)
			{
				var registration = token.Register(request.Abort, useSynchronizationContext: false);

				await using (registration.ConfigureAwait(false))
				{
					try
					{
						return (HttpWebResponse) await request.GetResponseAsync().ConfigureAwait(false);
					}
					catch (WebException ex)
					{
						if (token.IsCancellationRequested)
						{
							throw new OperationCanceledException(ex.Message, ex, token);
						}

						throw;
					}
				}
			}
		}
	}
}