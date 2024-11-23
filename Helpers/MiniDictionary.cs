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

public class MiniDictionary<TKey, TValue>(IEqualityComparer<TKey>? equalityComparer = default) : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
{
	private const int ConcurrencyLevel = 1;

	private const int InitialCapacity = 1;

	private ConcurrentDictionary<TKey, TValue>? _dictionary;

	public int Count => _dictionary?.Count ?? 0;

#region Interface IEnumerable

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#endregion

#region Interface IEnumerable<KeyValuePair<TKey,TValue>>

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>?) _dictionary ?? Array.Empty<KeyValuePair<TKey, TValue>>()).GetEnumerator();

#endregion

	public bool TryAdd(TKey key, TValue value) => (_dictionary ?? InitDictionary()).TryAdd(key, value);

	public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		value = default;

		return _dictionary?.TryRemove(key, out value) ?? false;
	}

	public bool Remove(TKey key, TValue value) => _dictionary?.TryRemove(new KeyValuePair<TKey, TValue>(key, value)) ?? false;

	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		value = default;

		return _dictionary?.TryGetValue(key, out value) ?? false;
	}

	private ConcurrentDictionary<TKey, TValue> InitDictionary()
	{
		lock (this)
		{
			return _dictionary ??= new ConcurrentDictionary<TKey, TValue>(ConcurrencyLevel, InitialCapacity, equalityComparer ?? EqualityComparer<TKey>.Default);
		}
	}

	public bool TryTake([MaybeNullWhen(false)] out TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		key = default;
		value = default;

		return _dictionary?.TryTake(out key, out value) ?? false;
	}
}