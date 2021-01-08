#region Copyright © 2019-2020 Sergii Artemenko

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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.Core
{
	[PublicAPI]
	public sealed class LocalCache<TKey, TValue> : IDisposable, IAsyncDisposable where TKey : notnull
	{
		private readonly ConcurrentDictionary<TKey, CacheEntry<TValue>> _globalDictionary;
		private readonly Dictionary<TKey, CacheEntry<TValue>>           _localDictionary;

		internal LocalCache(ConcurrentDictionary<TKey, CacheEntry<TValue>> globalDictionary, IEqualityComparer<TKey> comparer)
		{
			_globalDictionary = globalDictionary;
			_localDictionary = new Dictionary<TKey, CacheEntry<TValue>>(comparer);
		}

	#region Interface IAsyncDisposable

		public async ValueTask DisposeAsync()
		{
			foreach (var pair in _localDictionary)
			{
				await DropEntry(pair.Key, pair.Value).ConfigureAwait(false);
			}

			_localDictionary.Clear();
		}

	#endregion

	#region Interface IDisposable

		public void Dispose() => DisposeAsync().SynchronousWait();

	#endregion

		private async ValueTask DropEntry(TKey key, CacheEntry<TValue> entry)
		{
			var noMoreReferences = await entry.RemoveReference().ConfigureAwait(false);

			if ((entry.Options & ValueOptions.ThreadSafe) != 0 && noMoreReferences)
			{
				var collection = (ICollection<KeyValuePair<TKey, CacheEntry<TValue>>>) _globalDictionary;
				collection.Remove(new KeyValuePair<TKey, CacheEntry<TValue>>(key, entry));
			}
		}

		public async ValueTask SetValue(TKey key, [DisallowNull] TValue value, ValueOptions options)
		{
			if (_localDictionary.TryGetValue(key, out var entry))
			{
				await DropEntry(key, entry).ConfigureAwait(false);
			}

			var newEntry = new CacheEntry<TValue>(value, options);

			_localDictionary[key] = newEntry;

			if ((options & ValueOptions.ThreadSafe) != 0)
			{
				_globalDictionary[key] = newEntry;
			}
		}

		public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
		{
			if (_localDictionary.TryGetValue(key, out var localEntry) && localEntry.TryGetValue(out value))
			{
				return true;
			}

			if (_globalDictionary.TryGetValue(key, out var globalEntry) && globalEntry.TryGetValue(out value) && globalEntry.AddReference())
			{
				if (localEntry is not null)
				{
					var valueTask = localEntry.RemoveReference();
					Infrastructure.Assert(valueTask.IsCompleted);
					valueTask.GetAwaiter().GetResult();
				}

				_localDictionary[key] = globalEntry;

				return true;
			}

			value = default;

			return false;
		}
	}
}