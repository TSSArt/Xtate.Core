#region Copyright © 2019-2023 Sergii Artemenko

// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.IO;
using Xtate.IoC;
using Xtate.Persistence;

namespace Xtate.Core;

public static class PersistenceExtensions
{
	public static void RegisterPersistence(this IServiceCollection services)
	{
		if (services.IsRegistered<int>()) //TODO:replace int
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