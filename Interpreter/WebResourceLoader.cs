#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System;
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
		public static readonly IResourceLoader Instance = new WebResourceLoader();

	#region Interface IResourceLoader

		public bool CanHandle(Uri uri)
		{
			if (uri == null) throw new ArgumentNullException(nameof(uri));

			return uri.Scheme == "http" || uri.Scheme == "https";
		}

		public async ValueTask<Resource> Request(Uri uri, CancellationToken token)
		{
			if (uri == null) throw new ArgumentNullException(nameof(uri));

			using var client = new HttpClient();
			using var responseMessage = await client.GetAsync(uri, token).ConfigureAwait(false);

			responseMessage.EnsureSuccessStatusCode();

			var contentType = new ContentType(responseMessage.Content.Headers.ContentType.ToString());
			var content = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false); //TODO: ReadAsStringAsync replace to support CancellationToken

			return new Resource(uri, contentType, content);
		}

		public ValueTask<XmlReader> RequestXmlReader(Uri uri, XmlReaderSettings? readerSettings = default, XmlParserContext? parserContext = default, CancellationToken token = default)
		{
			if (uri == null) throw new ArgumentNullException(nameof(uri));

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