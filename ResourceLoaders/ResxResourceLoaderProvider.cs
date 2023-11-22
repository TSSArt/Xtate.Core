using System;
using System.Threading.Tasks;

namespace Xtate.Core;

public class ResxResourceLoaderProvider : ResourceLoaderProviderBase<ResxResourceLoader>
{
	public ResxResourceLoaderProvider(Func<ValueTask<ResxResourceLoader>> factory) : base(factory) { }

	protected override bool CanHandle(Uri uri)
	{
		if (uri is null) throw new ArgumentNullException(nameof(uri));

		return uri.IsAbsoluteUri && uri.Scheme is @"res" or @"resx";
	}
}