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


public class MiniDictionary<TKey, TValue>(IEqualityComparer<TKey>? comparer = default) : IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : notnull where TValue : class
{
	private readonly IEqualityComparer<TKey> _comparer = comparer ?? EqualityComparer<TKey>.Default;

	private ConcurrentDictionary<TKey, TValue>? _dictionary;

	private ConcurrentDictionary<TKey, TaskCompletionSource<(bool Found, TValue Value)>?>? _tcsDictionary;

	public bool IsEmpty => _dictionary?.IsEmpty ?? true;

#region Interface IEnumerable

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#endregion

#region Interface IEnumerable<KeyValuePair<TKey,TValue>>

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>?) _dictionary ?? Array.Empty<KeyValuePair<TKey, TValue>>()).GetEnumerator();

#endregion

#region Interface IReadOnlyCollection<KeyValuePair<TKey,TValue>>

	public int Count => _dictionary?.Count ?? 0;

#endregion

	[DoesNotReturn]
	private static KeyNotFoundException GetKeyNotFoundException(TKey key) => throw new KeyNotFoundException(Res.Format(Resources.Exception_KeyNotFound, key?.ToString()));

	private ConcurrentDictionary<TKey, TValue> GetDictionary()
	{
		if (_dictionary is { } dictionary)
		{
			return dictionary;
		}

		dictionary = new ConcurrentDictionary<TKey, TValue>(_comparer);

		return Interlocked.CompareExchange(ref _dictionary, dictionary, comparand: default) ?? dictionary;
	}

	private ConcurrentDictionary<TKey, TaskCompletionSource<(bool Found, TValue Value)>?> GetTcsDictionary()
	{
		if (_tcsDictionary is { } dictionary)
		{
			return dictionary;
		}

		dictionary = new ConcurrentDictionary<TKey, TaskCompletionSource<(bool Found, TValue Value)>?>(_comparer);

		return Interlocked.CompareExchange(ref _tcsDictionary, dictionary, comparand: default) ?? dictionary;
	}

	private void SetTaskCompletionSource(TKey key, TValue value, bool found)
	{
		if (_tcsDictionary is { } tcsDictionary && tcsDictionary.TryRemove(key, out var tcs) && tcs is not null)
		{
			tcs.TrySetResult((found, value));
		}
	}

	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		if (_dictionary is { } dictionary && dictionary.TryGetValue(key, out value))
		{
			return true;
		}

		value = default;

		return false;
	}

	public bool TryAdd(TKey key, TValue value)
	{
		var result = GetDictionary().TryAdd(key, value);

		if (result)
		{
			SetTaskCompletionSource(key, value, true);
		}

		return result;
	}

	public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		value = default;

		var result = _dictionary is { } dictionary && dictionary.TryRemove(key, out value);

		SetTaskCompletionSource(key, default!, false);

		return result;
	}

	public bool TryRemovePair(TKey key, TValue value) => _dictionary?.TryRemove(new KeyValuePair<TKey, TValue>(key, value)) ?? false;

	public bool TryTake([MaybeNullWhen(false)] out TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		if (_dictionary is { } dictionary)
		{
			foreach (var pair in dictionary)
			{
				if (dictionary.TryRemove(pair.Key, out value))
				{
					key = pair.Key;

					return true;
				}
			}
		}

		key = default;
		value = default;

		return false;
	}

	public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue) => _dictionary?.TryUpdate(key, newValue, comparisonValue) ?? false;

	public TValue AddOrUpdate<TArg>(TKey key, Func<TKey, TArg, TValue> addValueFactory, Func<TKey, TValue, TArg, TValue> updateValueFactory, TArg factoryArgument)
	{
		var result = GetDictionary().AddOrUpdate(key, addValueFactory, updateValueFactory, factoryArgument);

		SetTaskCompletionSource(key, result, true);

		return result;
	}

	public ValueTask<(bool Found, TValue Value)> TryGetValueAsync(TKey key)
	{
		while (true)
		{
			if (TryGetValue(key, out var value))
			{
				return new ValueTask<(bool Found, TValue Value)>((true, value));
			}

			if (_tcsDictionary is { } dictionary && dictionary.TryGetValue(key, out var tcs))
			{
				if (tcs is not null)
				{
					return new ValueTask<(bool Found, TValue Value)>(tcs.Task);
				}

				tcs = new TaskCompletionSource<(bool Found, TValue Value)>();

				if (dictionary.TryUpdate(key, tcs, comparisonValue: null))
				{
					return new ValueTask<(bool Found, TValue Value)>(tcs.Task);
				}
			}
			else
			{
				return new ValueTask<(bool Found, TValue Value)>((false, default!));
			}
		}
	}

	public bool TryAddPending(TKey key) => GetTcsDictionary().TryAdd(key, value: default);

	public TValue this[TKey key]
	{
		get => TryGetValue(key, out var value) ? value : throw GetKeyNotFoundException(key);
		set
		{
			GetDictionary()[key] = value;

			SetTaskCompletionSource(key, value, true);
		}
	}

	public void UpdateOrRemove<TArg>(TKey key, Func<TKey, TValue, TArg, bool> isDeletePredicate, Func<TKey, TValue, TArg, TValue> updateValueFactory, TArg factoryArgument)
	{
		while (TryGetValue(key, out var value))
		{
			if (isDeletePredicate(key, value, factoryArgument))
			{
				if (TryRemovePair(key, value))
				{
					return;
				}
			}
			else
			{
				if (TryUpdate(key, updateValueFactory(key, value, factoryArgument), value))
				{
					return;
				}
			}
		}
	}
}