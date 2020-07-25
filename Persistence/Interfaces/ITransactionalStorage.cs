using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Persistence
{
	public interface ITransactionalStorage : IStorage, IAsyncDisposable
	{
		ValueTask CheckPoint(int level, CancellationToken token);
		ValueTask Shrink(CancellationToken token);
	}
}