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
	public sealed class WebResourceLoader : IResourceLoader
	{
		public static IResourceLoader Instance { get; } = new WebResourceLoader();

	#region Interface IResourceLoader

		public bool CanHandle(Uri uri)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			return uri.IsAbsoluteUri && (uri.Scheme == @"http" || uri.Scheme == @"https");
		}

		public async ValueTask<Resource> Request(Uri uri, NameValueCollection? headers, CancellationToken token)
		{
			var request = WebRequest.CreateHttp(uri);

			SetHeader(request, headers);

			var response = await GetResponse(request, token).ConfigureAwait(false);

			var contentType = response.Headers[HttpResponseHeader.ContentType] is { Length: > 0 } val ? new ContentType(val) : null;

			return new Resource(response.GetResponseStream()!, contentType);
		}

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

	#endregion

		private static async Task<HttpWebResponse> GetResponse(HttpWebRequest request, CancellationToken token)
		{
#if NET461 || NETSTANDARD2_0
			using var registration = token.Register(request.Abort, useSynchronizationContext: false);
#else
			await using var registration = token.Register(request.Abort, useSynchronizationContext: false);
#endif

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