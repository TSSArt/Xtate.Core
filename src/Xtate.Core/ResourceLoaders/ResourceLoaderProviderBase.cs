using System;
using System.Threading.Tasks;

namespace Xtate.Core;

public abstract class ResourceLoaderProviderBase<TResourceLoader> : IResourceLoaderProvider where TResourceLoader : class, IResourceLoader
{
	private readonly Func<ValueTask<TResourceLoader>> _factory;

	protected ResourceLoaderProviderBase(Func<ValueTask<TResourceLoader>> factory) => _factory = factory;

#region Interface IResourceLoaderProvider

	public async ValueTask<IResourceLoader?> TryGetResourceLoader(Uri uri)
	{
		if (uri is null) throw new ArgumentNullException(nameof(uri));

		return CanHandle(uri) ? await _factory().ConfigureAwait(false) : default;
	}

#endregion

	protected abstract bool CanHandle(Uri uri);
}