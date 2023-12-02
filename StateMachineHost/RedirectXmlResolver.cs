#region Copyright © 2019-2023 Sergii Artemenko

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

using System.Collections.Specialized;
using System.IO;
using Xtate.XInclude;

namespace Xtate.Core;

public class RedirectXmlResolver : ScxmlXmlResolver, IDisposable
{
	private readonly DisposingToken                   _disposingToken = new();
	public required  Func<ValueTask<IResourceLoader>> ResourceLoaderFactory { private get; [UsedImplicitly] init; }

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposingToken.Dispose();
		}
	}

	protected override async ValueTask<object> GetEntityAsync(Uri uri,
															  string? accept,
															  string? acceptLanguage,
															  Type? ofObjectToReturn)
	{
		if (ofObjectToReturn is not null && ofObjectToReturn != typeof(Stream) && ofObjectToReturn != typeof(IXIncludeResource))
		{
			throw new ArgumentException(Res.Format(Resources.Exception_UnsupportedClass, ofObjectToReturn));
		}

		var resourceLoader = await ResourceLoaderFactory().ConfigureAwait(false);
		var resource = await resourceLoader.Request(uri, GetHeaders(accept, acceptLanguage)).ConfigureAwait(false);
		var stream = await resource.GetStream(true).ConfigureAwait(false);
		stream = stream.InjectCancellationToken(_disposingToken.Token);

		return ofObjectToReturn == typeof(IXIncludeResource) ? new Resource(stream, resource.ContentType) : stream;
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