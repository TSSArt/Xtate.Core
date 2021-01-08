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
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.Core
{
	[PublicAPI]
	public static class FactoryContextExtensions
	{
		public static ValueTask<Resource> GetResource(this IFactoryContext factoryContext, Uri uri, CancellationToken token) => GetResource(factoryContext, uri, headers: default, token);

		public static async ValueTask<Resource> GetResource(this IFactoryContext factoryContext, Uri uri, NameValueCollection? headers, CancellationToken token)
		{
			if (factoryContext is null) throw new ArgumentNullException(nameof(factoryContext));

			var factories = factoryContext.ResourceLoaderFactories;
			if (!factories.IsDefaultOrEmpty)
			{
				foreach (var resourceLoaderFactory in factories)
				{
					if (await resourceLoaderFactory.TryGetActivator(factoryContext, uri, token).ConfigureAwait(false) is { } activator)
					{
						var resourceLoader = await activator.CreateResourceLoader(factoryContext, token).ConfigureAwait(false);

						return await resourceLoader.Request(uri, headers, token).ConfigureAwait(false);
					}
				}
			}

			throw new ProcessorException(Resources.Exception_Cannot_find_ResourceLoader_to_load_external_resource);
		}
	}
}