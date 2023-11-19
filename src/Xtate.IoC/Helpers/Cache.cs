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

using System.Collections.Concurrent;

namespace Xtate.IoC;

internal class Cache<TKey, TValue> where TKey : notnull
{
	private const int ConcurrencyLevel = 1;

	private readonly ConcurrentDictionary<TKey, TValue> _dictionary;

	public Cache(int initialCapacity) => _dictionary = new ConcurrentDictionary<TKey, TValue>(ConcurrencyLevel, initialCapacity);

	public Cache(IEnumerable<KeyValuePair<TKey, TValue>> initialCollection) =>
		_dictionary = new ConcurrentDictionary<TKey, TValue>(ConcurrencyLevel, initialCollection, EqualityComparer<TKey>.Default);

	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dictionary.TryGetValue(key, out value);

	public TValue GetOrAdd(TKey key, TValue value) => _dictionary.GetOrAdd(key, value);

	public void TryAdd(TKey key, TValue value) => _dictionary.TryAdd(key, value);
}