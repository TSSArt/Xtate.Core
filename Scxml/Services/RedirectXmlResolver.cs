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

using System.Collections.Specialized;
using System.IO;
using System.Net.Mime;
using System.Xml;
using Xtate.IoC.Tools;
using Xtate.ResourceLoaders;
using Xtate.ResourceLoaders.Extensions;

namespace Xtate.Scxml.Services;

[InstantiatedByIoC]
public class RedirectXmlResolver : XmlResolver, IExternalEntityGetter
{
	public required DisposeToken DisposeToken { private get; [SetByIoC] init; }

	public required Func<ValueTask<IResourceLoader>> ResourceLoaderFactory { private get; [SetByIoC] init; }

	public required Func<Stream, ContentType?, Resource> ResourceFactory { private get; [SetByIoC] init; }

	public required Func<ValueTask<IXIncludeOptions?>> XIncludeOptionsFactory { private get; [SetByIoC] init; }

	#region Interface IExternalEntityGetter

	public override bool SupportsType(Uri absoluteUri, Type type) => type == typeof(Resource) || base.SupportsType(absoluteUri, type);

	public virtual object? GetEntity(Uri absoluteUri, NameValueCollection? headers, Type? ofObjectToReturn) => throw new NotSupportedException(Resources.Exception_LoadingExternalResourcesSynchronouslyDoesNotSupported);

	public virtual async ValueTask<object?> GetEntityAsync(Uri absoluteUri, NameValueCollection? headers, Type? ofObjectToReturn)
	{
		if (ofObjectToReturn is not null && ofObjectToReturn != typeof(Stream) && ofObjectToReturn != typeof(Resource))
		{
			throw new ArgumentException(Res.Format(Resources.Exception_UnsupportedClass, ofObjectToReturn));
		}

		var xIncludeOptions = await XIncludeOptionsFactory().ConfigureAwait(false);

		if (xIncludeOptions?.XIncludeAllowed != true)
		{
			throw new XmlException(Resources.Exception_LoadingExternalResourcesDoesNotSupported);
		}

		var resourceLoader = await ResourceLoaderFactory().ConfigureAwait(false);
		var resource = await resourceLoader.Request(absoluteUri, headers).ConfigureAwait(false);
		var stream = await resource.GetStream(true).ConfigureAwait(false);
		stream = stream.InjectCancellationToken(DisposeToken);

		return ofObjectToReturn == typeof(Resource) ? ResourceFactory(stream, resource.ContentType) : stream;
	}

#endregion

	public sealed override object? GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) => GetEntity(absoluteUri, headers: null, ofObjectToReturn);

	public sealed override Task<object?> GetEntityAsync(Uri absoluteUri, string role, Type ofObjectToReturn) => GetEntityAsync(absoluteUri, headers: null, ofObjectToReturn).AsTask();
}