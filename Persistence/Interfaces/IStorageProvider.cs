using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Persistence
{
	public interface IStorageProvider
	{
		ValueTask<ITransactionalStorage> GetTransactionalStorage(string? partition, string key, CancellationToken token);
		ValueTask                        RemoveTransactionalStorage(string? partition, string key, CancellationToken token);
		ValueTask                        RemoveAllTransactionalStorage(string? partition, CancellationToken token);
	}
}