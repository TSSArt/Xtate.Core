using Xtate.Persistence;

namespace Xtate.Core;

public class DefaultTransactionalStorage
{
	public required IStorageProvider StorageProvider { private get; [SetByIoC] init; }

	[CalledByIoC]
	public ValueTask<ITransactionalStorage> Factory(string name) => StorageProvider.GetTransactionalStorage(null, name);
}