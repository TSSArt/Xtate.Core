﻿#region Copyright © 2019-2021 Sergii Artemenko

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

#if NET461 || NETSTANDARD2_0
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
			if (concurrentDictionary is null) throw new ArgumentNullException(nameof(concurrentDictionary));

			return ((ICollection<KeyValuePair<TKey, TValue>>) concurrentDictionary).Remove(pair);
		}

		public static TValue AddOrUpdate<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> concurrentDictionary,
															 TKey key,
															 Func<TKey, TArg, TValue> addValueFactory,
															 Func<TKey, TValue, TArg, TValue> updateValueFactory,
															 TArg factoryArgument)
		{
			if (concurrentDictionary is null) throw new ArgumentNullException(nameof(concurrentDictionary));

			return concurrentDictionary.AddOrUpdate(key, keyArg => addValueFactory(keyArg, factoryArgument), (keyArg, valueArg) => updateValueFactory(keyArg, valueArg, factoryArgument));
		}
	}
}
#endif