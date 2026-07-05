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

#if !NET5_0_OR_GREATER

// ReSharper disable once CheckNamespace
namespace System.Collections.Concurrent;

internal static class ConcurrentDictionaryPolyfills
{
	extension<TKey, TValue>(ConcurrentDictionary<TKey, TValue> concurrentDictionary) where TKey : notnull
	{
		public bool TryRemove(KeyValuePair<TKey, TValue> pair) => ((ICollection<KeyValuePair<TKey, TValue>>) concurrentDictionary).Remove(pair);

#if !NETCOREAPP2_0 && !NETCOREAPP2_1_OR_GREATER && !NETSTANDARD2_1 && !NET472 && !NET48

		public TValue AddOrUpdate<TArg>(TKey key,
										Func<TKey, TArg, TValue> addValueFactory,
										Func<TKey, TValue, TArg, TValue> updateValueFactory,
										TArg factoryArgument)
		{
			while (true)
			{
				if (concurrentDictionary.TryGetValue(key, out var value))
				{
					var newValue = updateValueFactory(key, value, factoryArgument);

					if (concurrentDictionary.TryUpdate(key, newValue, value)) return newValue;
				}
				else
				{
					var newValue = addValueFactory(key, factoryArgument);

					if (concurrentDictionary.TryAdd(key, newValue)) return newValue;
				}
			}
		}

		public TValue GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
		{
			if (concurrentDictionary.TryGetValue(key, out var value)) return value;

			var newValue = valueFactory(key, factoryArgument);

			return concurrentDictionary.GetOrAdd(key, newValue);
		}

#endif
	}
}

#endif