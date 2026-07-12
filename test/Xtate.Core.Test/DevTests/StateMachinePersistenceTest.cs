// Copyright © 2019-2026 Sergii Artemenko
// 
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

using System.IO;
using Xtate.Persistence;
using Xtate.Persistence.Services;

namespace Xtate.Test;

[TestClass]
public class StateMachinePersistenceTest
{
	[ExcludeFromCodeCoverage]
	public class TestStorage : IStorageProvider
	{
		private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, MemoryStream>> _storage = new();

	#region Interface IStorageProvider

		public async ValueTask<ITransactionalStorage> GetTransactionalStorage(string? partition, string key)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentException(message: @"Value cannot be null or empty.", nameof(key));

			var partitionStorage = _storage.GetOrAdd(partition ?? "", _ => new ConcurrentDictionary<string, MemoryStream>());
			var memStream = partitionStorage.GetOrAdd(key, _ => new MemoryStream());

			//var newMemStream = memStream;
			/*var newMemStream = new MemoryStream();
			var buffer = memStream.ToArray();
			newMemStream.Write(buffer, 0, buffer.Length);
			newMemStream.Position = 0;*/

			var streamStorage = new StreamStorage(memStream, disposeStream: false)
								{
									InMemoryStorageFactory = b => new InMemoryStorage(b),
									InMemoryStorageBaselineFactory = memory => new InMemoryStorage(memory.Span)
								};
			await streamStorage.InitializeAsync();

			return streamStorage;
		}

		public ValueTask RemoveTransactionalStorage(string? partition, string key)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentException(message: @"Value cannot be null or empty.", nameof(key));

			var partitionStorage = _storage.GetOrAdd(partition ?? "", _ => new ConcurrentDictionary<string, MemoryStream>());
			partitionStorage.TryRemove(key, out _);

			return default;
		}

		public ValueTask RemoveAllTransactionalStorage(string? partition)
		{
			_storage.TryRemove(partition ?? "", out _);

			return default;
		}

	#endregion
	}
}