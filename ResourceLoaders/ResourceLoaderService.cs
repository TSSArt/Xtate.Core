using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Xtate.Core
{
	public class ResourceLoaderService : IResourceLoader
	{
		private readonly IAsyncEnumerable<IResourceLoaderProvider> _resourceLoaderProviders;

		public ResourceLoaderService(IAsyncEnumerable<IResourceLoaderProvider> resourceLoaderProviders) => _resourceLoaderProviders = resourceLoaderProviders;

	#region Interface IResourceLoaderService

		public virtual async ValueTask<Resource> Request(Uri uri, NameValueCollection? headers = default)
		{
			await foreach (var resourceLoaderProvider in _resourceLoaderProviders.ConfigureAwait(false))
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
}
