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

using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Xtate.Core;

public class ResxResourceLoader : IResourceLoader
{
	public required IIoBoundTask IoBoundTask { private get; init; }

	public required Func<Stream, Resource> ResourceFactory { private get; init; }

#region Interface IResourceLoader

	public async ValueTask<Resource> Request(Uri uri, NameValueCollection? headers)
	{
		Infra.Requires(uri);

		return ResourceFactory(await GetResourceStreamAsync(uri).ConfigureAwait(false));
	}

#endregion

	private Task<Stream> GetResourceStreamAsync(Uri uri) => IoBoundTask.Factory.StartNew(() => GetResourceStream(uri));

	protected virtual Stream GetResourceStream(Uri uri)
	{
		Infra.Requires(uri);

		var assemblyName = uri.Host;

		var name = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped).Replace(oldChar: '/', newChar: '.');

		if (Assembly.Load(assemblyName).GetManifestResourceStream(name) is { } stream)
		{
			return stream;	
		}

		throw new ResourceNotFoundException(Res.Format(Resources.Exception_ResourceNotFound, uri));
	}
}