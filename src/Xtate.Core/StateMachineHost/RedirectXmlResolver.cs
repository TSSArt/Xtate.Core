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
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xtate.XInclude;

namespace Xtate
{
	public class RedirectXmlResolver : ScxmlXmlResolver
	{
		private readonly ImmutableArray<IResourceLoader> _resourceLoaders;
		private readonly CancellationToken               _token;

		public RedirectXmlResolver(ImmutableArray<IResourceLoader> resourceLoaders, CancellationToken token)
		{
			_resourceLoaders = resourceLoaders;
			_token = token;
		}

		protected override async ValueTask<object> GetEntityAsync(Uri uri, string? accept, string? acceptLanguage, Type? ofObjectToReturn)
		{
			if (ofObjectToReturn is not null && ofObjectToReturn != typeof(Stream) && ofObjectToReturn != typeof(IXIncludeResource))
			{
				throw new ArgumentException(Res.Format(Resources.Exception_UnsupportedClass, ofObjectToReturn));
			}

			if (!_resourceLoaders.IsDefaultOrEmpty)
			{
				foreach (var resourceLoader in _resourceLoaders)
				{
					if (resourceLoader.CanHandle(uri))
					{
						var resource = await resourceLoader.Request(uri, GetHeaders(accept, acceptLanguage), _token).ConfigureAwait(false);
						var stream = await resource.GetStream(doNotCache: true, _token).ConfigureAwait(false);
						stream = stream.InjectCancellationToken(_token);

						return ofObjectToReturn == typeof(IXIncludeResource) ? new Resource(stream, resource.ContentType) : stream;
					}
				}
			}

			throw new ProcessorException(Resources.Exception_Cannot_find_ResourceLoader_to_load_external_resource);
		}

		private static NameValueCollection? GetHeaders(string? accept, string? acceptLanguage)
		{
			if (string.IsNullOrEmpty(accept) && string.IsNullOrEmpty(acceptLanguage))
			{
				return default;
			}

			var headers = new NameValueCollection(2);

			if (!string.IsNullOrEmpty(accept))
			{
				headers.Add(name: @"Accept", accept);
			}

			if (!string.IsNullOrEmpty(accept))
			{
				headers.Add(name: @"Accept-Language", acceptLanguage);
			}

			return headers;
		}
	}
}