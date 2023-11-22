using System;
using System.Threading.Tasks;

namespace Xtate.Core;

public class FileResourceLoaderProvider : ResourceLoaderProviderBase<FileResourceLoader>
{
	public FileResourceLoaderProvider(Func<ValueTask<FileResourceLoader>> factory) : base(factory) { }

	protected override bool CanHandle(Uri uri)
	{
		if (uri is null) throw new ArgumentNullException(nameof(uri));

		return !uri.IsAbsoluteUri || uri.IsFile || uri.IsUnc;
	}
}