using System;
using System.Threading.Tasks;

namespace Xtate.Core;

public class WebResourceLoaderProvider : ResourceLoaderProviderBase<WebResourceLoader>
{
	public WebResourceLoaderProvider(Func<ValueTask<WebResourceLoader>> factory) : base(factory) { }

	protected override bool CanHandle(Uri uri)
	{
		if (uri is null) throw new ArgumentNullException(nameof(uri));

		return uri.IsAbsoluteUri && uri.Scheme is @"http" or @"https";
	}
}