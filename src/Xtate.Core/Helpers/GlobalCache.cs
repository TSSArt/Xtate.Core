﻿#region Copyright © 2019-2023 Sergii Artemenko

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

namespace Xtate.Core;


public sealed class GlobalCache<TKey, TValue> where TKey : notnull
{
	private readonly IEqualityComparer<TKey> _comparer;

	private readonly ConcurrentDictionary<TKey, CacheEntry<TValue>> _global;

	public GlobalCache() : this(EqualityComparer<TKey>.Default) { }

	public GlobalCache(IEqualityComparer<TKey> comparer)
	{
		_comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
		_global = new ConcurrentDictionary<TKey, CacheEntry<TValue>>(comparer);
	}

	public LocalCache<TKey, TValue> CreateLocalCache() => new(_global, _comparer);
}