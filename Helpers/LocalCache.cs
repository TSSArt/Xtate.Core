// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate.Core;

public class LocalCache<TKey, TValue> : IDisposable, IAsyncDisposable where TKey : notnull
{
	private readonly Dictionary<TKey, CacheEntry<TValue>> _localDictionary = [];
	public required  GlobalCache<TKey, TValue>            GlobalCache { private get; [UsedImplicitly] init; }

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}

#endregion

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			DisposeAsync().SynchronousWait();
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		foreach (var pair in _localDictionary)
		{
			await DropEntry(pair.Key, pair.Value).ConfigureAwait(false);
		}

		_localDictionary.Clear();
	}

	private async ValueTask DropEntry(TKey key, CacheEntry<TValue> entry)
	{
		var noMoreReferences = await entry.RemoveReference().ConfigureAwait(false);

		if ((entry.Options & ValueOptions.ThreadSafe) != 0 && noMoreReferences)
		{
			GlobalCache.Remove(key, entry);
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
			GlobalCache.Set(key, newEntry);
		}
	}

	public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
	{
		if (_localDictionary.TryGetValue(key, out var localEntry) && localEntry.TryGetValue(out value))
		{
			return true;
		}

		if (GlobalCache.TryGetValue(key, out var globalEntry) && globalEntry.TryGetValue(out value) && globalEntry.AddReference())
		{
			if (localEntry is not null)
			{
				var valueTask = localEntry.RemoveReference();
				Infra.Assert(valueTask.IsCompleted);
				valueTask.GetAwaiter().GetResult();
			}

			_localDictionary[key] = globalEntry;

			return true;
		}

		value = default;

		return false;
	}
}