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

namespace Xtate.Core;

public class ResourceLoaderService : IResourceLoader
{
	public required IAsyncEnumerable<IResourceLoaderProvider> ResourceLoaderProviders { private get; [UsedImplicitly] init; }

#region Interface IResourceLoader

	public virtual async ValueTask<Resource> Request(Uri uri, NameValueCollection? headers = default)
	{
		await foreach (var resourceLoaderProvider in ResourceLoaderProviders.ConfigureAwait(false))
		{
			if (await resourceLoaderProvider.TryGetResourceLoader(uri).ConfigureAwait(false) is { } resourceLoader)
			{
				return await resourceLoader.Request(uri, headers).ConfigureAwait(false);
			}
		}

		throw new ProcessorException(Res.Format(Resources.Exception_CannotFindResourceLoaderToLoadExternalResource, uri));
	}

#endregion
}