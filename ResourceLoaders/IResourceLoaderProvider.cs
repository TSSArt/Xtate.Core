using System;
using System.Threading.Tasks;

namespace Xtate.Core;

public interface IResourceLoaderProvider
{
	ValueTask<IResourceLoader?> TryGetResourceLoader(Uri uri);
}