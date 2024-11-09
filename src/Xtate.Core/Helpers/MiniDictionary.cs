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

using System.Collections.Concurrent;

namespace Xtate.Core;

public class MiniDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
	private ConcurrentDictionary<TKey, TValue>? _dictionary;

	private readonly IEqualityComparer<TKey>? _equalityComparer;

	public MiniDictionary() :this(default) { }

	public MiniDictionary(IEqualityComparer<TKey> equalityComparer) => _equalityComparer = equalityComparer;

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)_dictionary ?? Array.Empty<KeyValuePair<TKey, TValue>>()).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public bool TryAdd(TKey key, TValue value)
	{
		if (_dictionary is not { } dictionary)
		{
			dictionary = InitDictionary();
		}

		return dictionary.TryAdd(key, value);
	}

	private ConcurrentDictionary<TKey, TValue> InitDictionary()
	{
		lock (this)
		{
			return _dictionary ??= new ConcurrentDictionary<TKey, TValue>(_equalityComparer!);
		}
	}

	public bool TryRemove(TKey key, [MaybeNullWhen(false)]out TValue value)
	{
		if (_dictionary is not { } dictionary)
		{
			value = default;

			return false;
		}

		return dictionary.TryRemove(key, out value);
	}

	public bool Remove(TKey key, TValue value)
	{
		if (_dictionary is not { } dictionary)
		{
			value = default;

			return false;
		}

		return dictionary.TryRemove(new KeyValuePair<TKey, TValue>(key, value));
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		if (_dictionary is not { } dictionary)
		{
			value = default;

			return false;
		}

		return dictionary.TryGetValue(key, out value);
	}
}