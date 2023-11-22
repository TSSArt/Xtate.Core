#region Copyright © 2019-2021 Sergii Artemenko

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

#if !NET6_0_OR_GREATER
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Xtate.Core
{
	[PublicAPI]
	public static class ConcurrentDictionaryExtensions
	{
		public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> concurrentDictionary, KeyValuePair<TKey, TValue> pair)
		{
			Infra.Requires(concurrentDictionary);

			return ((ICollection<KeyValuePair<TKey, TValue>>) concurrentDictionary).Remove(pair);
		}

		public static TValue AddOrUpdate<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> concurrentDictionary,
															 TKey key,
															 Func<TKey, TArg, TValue> addValueFactory,
															 Func<TKey, TValue, TArg, TValue> updateValueFactory,
															 TArg factoryArgument)
		{
			Infra.Requires(concurrentDictionary);
			Infra.Requires(updateValueFactory);
			Infra.Requires(addValueFactory);

			while (true)
			{
				if(concurrentDictionary.TryGetValue(key, out var value))
				{
					var newValue = updateValueFactory(key, value, factoryArgument);

					if (concurrentDictionary.TryUpdate(key, newValue, value))
					{
						return newValue;
					}
				}
				else
				{
					var newValue = addValueFactory(key, factoryArgument);

					if (concurrentDictionary.TryAdd(key, newValue))
					{
						return newValue;
					}
				}
			}
		}

		public static TValue GetOrAdd<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> concurrentDictionary,
														  TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
		{
			Infra.Requires(concurrentDictionary);
			Infra.Requires(valueFactory);

			if (concurrentDictionary.TryGetValue(key, out var value))
			{
				return value;
			}

			var newValue = valueFactory(key, factoryArgument);

			return concurrentDictionary.GetOrAdd(key, newValue);
		}
	}
}
#endif