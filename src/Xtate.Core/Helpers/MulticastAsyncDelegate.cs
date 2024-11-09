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

public readonly struct MulticastAsyncDelegate<TArg>()
{
	private readonly ConcurrentDictionary<Func<TArg, ValueTask>, uint> _delegates = new(concurrencyLevel: 1, capacity: 1);

	public event Func<TArg, ValueTask> OnInvoke
	{
		add
		{
			_delegates.AddOrUpdate(value, addValue: 0, (_, count) => count + 1);
		}
		remove
		{
			var count = 0u;

			do
			{
				switch (count)
				{
					case 0 when _delegates.TryRemove(new KeyValuePair<Func<TArg, ValueTask>, uint>(value, count)):
					case not 0 when _delegates.TryUpdate(value, count - 1, count):
						return;
				}
			}
			while (_delegates.TryGetValue(value, out count));
		}
	}

	public async ValueTask Invoke(TArg arg)
	{
		foreach (var pair in _delegates)
		{
			for (var i = 0; i <= pair.Value; i ++)
			{
				await pair.Key(arg).ConfigureAwait(false);
			}
		}
	}
}