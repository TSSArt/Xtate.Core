using System;
using System.IO;
using Xtate.IoC;
using Xtate.Persistence;

namespace Xtate.Core;

public static class PersistenceExtensions
{
	public static void RegisterPersistence(this IServiceCollection services)
	{
		if (services.IsRegistered<int>())//TODO:replace int
		{
			return;
		}

		services.AddForwarding<InMemoryStorage, bool>((_, writeOnly) => new InMemoryStorage(writeOnly));
		services.AddForwarding<InMemoryStorage, ReadOnlyMemory<byte>>((_, baseline) => new InMemoryStorage(baseline.Span));
		services.AddForwarding<IStorage, bool>((_, writeOnly) => new InMemoryStorage(writeOnly));
		services.AddForwarding<IStorage, ReadOnlyMemory<byte>>((_, baseline) => new InMemoryStorage(baseline.Span));
		services.AddForwarding<ITransactionalStorage, Stream>(
			(sp, stream) => new StreamStorage(stream)
							{
								InMemoryStorageFactory = sp.GetRequiredSyncFactory<InMemoryStorage, bool>(),
								InMemoryStorageBaselineFactory = sp.GetRequiredSyncFactory<InMemoryStorage, ReadOnlyMemory<byte>>()
							});
		services.AddForwarding<ITransactionalStorage, Stream, int>(
			(sp, stream, rollbackLevel) => new StreamStorage(stream, rollbackLevel: rollbackLevel)
										   {
											   InMemoryStorageFactory = sp.GetRequiredSyncFactory<InMemoryStorage, bool>(),
											   InMemoryStorageBaselineFactory = sp.GetRequiredSyncFactory<InMemoryStorage, ReadOnlyMemory<byte>>()
										   });
	}
}