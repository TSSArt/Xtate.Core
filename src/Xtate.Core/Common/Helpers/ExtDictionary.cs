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

namespace Xtate;

/// <summary>
/// A thread-safe dictionary that supports asynchronous value retrieval via pending entries.
/// Combines a <see cref="ConcurrentDictionary{TKey,TValue}"/> with task-based awaiting,
/// allowing consumers to wait for a key to be added or removed.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary. Must be non-nullable.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
/// <param name="comparer">
/// An optional equality comparer for keys. If <see langword="null"/>,
/// <see cref="EqualityComparer{T}.Default"/> is used.
/// </param>
public class ExtDictionary<TKey, TValue>(IEqualityComparer<TKey>? comparer = null) : IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : notnull
{
	private readonly IEqualityComparer<TKey> _comparer = comparer ?? EqualityComparer<TKey>.Default;

	private ConcurrentDictionary<TKey, TValue>? _dictionary;

	private ConcurrentDictionary<TKey, TaskCompletionSource<(bool Found, TValue Value)>?>? _tcsDictionary;

	/// <summary>Gets a value indicating whether the dictionary contains no entries.</summary>
	public bool IsEmpty => _dictionary?.IsEmpty ?? true;

	/// <summary>Gets a collection containing the keys in the dictionary.</summary>
	public ICollection<TKey> Keys => _dictionary?.Keys ?? [];

	/// <summary>Gets a collection containing the values in the dictionary.</summary>
	public ICollection<TValue> Values => _dictionary?.Values ?? [];

	/// <summary>
	/// Gets or sets the value associated with the specified key.
	/// Setting a value also completes any pending <see cref="TryGetValueAsync"/> waiters for that key.
	/// </summary>
	/// <param name="key">The key of the value to get or set.</param>
	/// <exception cref="KeyNotFoundException">The key does not exist when getting.</exception>
	public TValue this[TKey key]
	{
		get => _dictionary is { } dictionary ? dictionary[key] : throw GetKeyNotFoundException(key);
		set
		{
			GetDictionary()[key] = value;

			SetTaskCompletionSource(key, value, found: true);
		}
	}

#region Interface IEnumerable

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#endregion

#region Interface IEnumerable<KeyValuePair<TKey,TValue>>

	/// <inheritdoc/>
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>?) _dictionary ?? []).GetEnumerator();

#endregion

#region Interface IReadOnlyCollection<KeyValuePair<TKey,TValue>>

	/// <inheritdoc/>
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

		return Interlocked.CompareExchange(ref _dictionary, dictionary, comparand: null) ?? dictionary;
	}

	private ConcurrentDictionary<TKey, TaskCompletionSource<(bool Found, TValue Value)>?> GetTcsDictionary()
	{
		if (_tcsDictionary is { } dictionary)
		{
			return dictionary;
		}

		dictionary = new ConcurrentDictionary<TKey, TaskCompletionSource<(bool Found, TValue Value)>?>(_comparer);

		return Interlocked.CompareExchange(ref _tcsDictionary, dictionary, comparand: null) ?? dictionary;
	}

	private void SetTaskCompletionSource(TKey key, TValue value, bool found)
	{
		if (_tcsDictionary is { } dictionary && dictionary.TryRemove(key, out var tcs))
		{
			tcs?.TrySetResult((found, value));
		}
	}

	/// <summary>
	/// Attempts to get the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key to locate.</param>
	/// <param name="value">
	/// When this method returns, contains the value associated with <paramref name="key"/>,
	/// or <see langword="default"/> if the key was not found.
	/// </param>
	/// <returns><see langword="true"/> if the key was found; otherwise, <see langword="false"/>.</returns>
	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		if (_dictionary is { } dictionary && dictionary.TryGetValue(key, out value))
		{
			return true;
		}

		value = default;

		return false;
	}

	/// <summary>
	/// Attempts to add the specified key and value to the dictionary.
	/// If the key is added, any pending <see cref="TryGetValueAsync"/> waiters for that key are completed.
	/// </summary>
	/// <param name="key">The key to add.</param>
	/// <param name="value">The value to add.</param>
	/// <returns><see langword="true"/> if the key/value pair was added; <see langword="false"/> if the key already exists.</returns>
	public bool TryAdd(TKey key, TValue value)
	{
		var tryAdd = GetDictionary().TryAdd(key, value);

		if (tryAdd)
		{
			SetTaskCompletionSource(key, value, found: true);
		}

		return tryAdd;
	}

	/// <summary>
	/// Attempts to remove the value with the specified key from the dictionary.
	/// Completes any pending <see cref="TryGetValueAsync"/> waiters for that key with <c>Found = false</c>.
	/// </summary>
	/// <param name="key">The key to remove.</param>
	/// <param name="value">
	/// When this method returns, contains the removed value, or <see langword="default"/> if not found.
	/// </param>
	/// <returns><see langword="true"/> if the key was found and removed; otherwise, <see langword="false"/>.</returns>
	public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		value = default;

		var result = _dictionary is { } dictionary && dictionary.TryRemove(key, out value);

		SetTaskCompletionSource(key, default!, found: false);

		return result;
	}

	/// <summary>
	/// Attempts to remove a specific key/value pair from the dictionary.
	/// Completes any pending <see cref="TryGetValueAsync"/> waiters for that key with <c>Found = false</c>.
	/// </summary>
	/// <param name="key">The key to remove.</param>
	/// <param name="value">The value that must match the current value for the key.</param>
	/// <returns><see langword="true"/> if the exact key/value pair was removed; otherwise, <see langword="false"/>.</returns>
	public bool TryRemovePair(TKey key, TValue value)
	{
		var tryRemove = _dictionary?.TryRemove(new KeyValuePair<TKey, TValue>(key, value)) ?? false;

		if (tryRemove)
		{
			SetTaskCompletionSource(key, default!, found: false);
		}

		return tryRemove;
	}

	/// <summary>
	/// Attempts to remove and return an arbitrary key/value pair from the dictionary.
	/// Completes any pending <see cref="TryGetValueAsync"/> waiters for the removed key with <c>Found = false</c>.
	/// </summary>
	/// <param name="key">When this method returns, contains the removed key, or <see langword="default"/> if the dictionary was empty.</param>
	/// <param name="value">When this method returns, contains the removed value, or <see langword="default"/> if the dictionary was empty.</param>
	/// <returns><see langword="true"/> if a pair was removed; otherwise, <see langword="false"/>.</returns>
	public bool TryTake([MaybeNullWhen(false)] out TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		if (_dictionary is { } dictionary)
		{
			foreach (var (key1, _) in dictionary)
			{
				if (dictionary.TryRemove(key1, out value))
				{
					SetTaskCompletionSource(key1, default!, found: false);

					key = key1;

					return true;
				}
			}
		}

		key = default;
		value = default;

		return false;
	}

	/// <summary>
	/// Updates the value of an existing key if the current value equals <paramref name="comparisonValue"/>.
	/// Completes any pending <see cref="TryGetValueAsync"/> waiters for that key with the new value.
	/// </summary>
	/// <param name="key">The key whose value to update.</param>
	/// <param name="newValue">The replacement value.</param>
	/// <param name="comparisonValue">The value that must match the current value for the update to succeed.</param>
	/// <returns><see langword="true"/> if the value was updated; otherwise, <see langword="false"/>.</returns>
	public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
	{
		var tryUpdate = _dictionary?.TryUpdate(key, newValue, comparisonValue) ?? false;

		if (tryUpdate)
		{
			SetTaskCompletionSource(key, newValue, found: true);
		}

		return tryUpdate;
	}

	/// <summary>
	/// Asynchronously attempts to get the value associated with the specified key.
	/// If a pending entry was registered via <see cref="TryAddPending"/>, this method
	/// awaits until the key is added or removed.
	/// Returns immediately if the key exists or no pending entry is registered.
	/// </summary>
	/// <param name="key">The key to locate.</param>
	/// <returns>
	/// A <see cref="ValueTask{T}"/> that resolves to a tuple where
	/// <c>Found</c> indicates whether the key was found, and <c>Value</c> is the associated value.
	/// </returns>
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

	/// <summary>
	/// Registers a pending entry for the specified key, enabling <see cref="TryGetValueAsync"/>
	/// to await until a value is added or removed for that key.
	/// </summary>
	/// <param name="key">The key to register as pending.</param>
	/// <returns>
	/// <see langword="true"/> if the pending entry was added;
	/// <see langword="false"/> if a pending entry for the key already exists.
	/// </returns>
	public bool TryAddPending(TKey key) => GetTcsDictionary().TryAdd(key, value: null);

	/// <summary>
	/// Gets the value for the specified key, or adds a new value produced by <paramref name="valueFactory"/>
	/// if the key does not exist.
	/// </summary>
	/// <typeparam name="TArg">The type of the argument passed to <paramref name="valueFactory"/>.</typeparam>
	/// <param name="key">The key to locate or add.</param>
	/// <param name="valueFactory">A factory invoked with the key and <paramref name="factoryArgument"/> to produce the value to add.</param>
	/// <param name="factoryArgument">An argument passed to <paramref name="valueFactory"/>.</param>
	/// <returns>The existing or newly added value.</returns>
	public TValue GetOrAdd<TArg>(TKey key,
								 Func<TKey, TArg, TValue> valueFactory,
								 TArg factoryArgument)
	{
		while (true)
		{
			if (TryGetValue(key, out var value))
			{
				return value;
			}

			value = valueFactory(key, factoryArgument);

			if (TryAdd(key, value))
			{
				return value;
			}
		}
	}

	/// <summary>
	/// Adds a new entry or updates an existing one using the provided factory delegates.
	/// Completes any pending <see cref="TryGetValueAsync"/> waiters for that key.
	/// </summary>
	/// <typeparam name="TArg">The type of the argument passed to the factory delegates.</typeparam>
	/// <param name="key">The key to add or update.</param>
	/// <param name="addValueFactory">A factory invoked to produce the value when the key does not exist.</param>
	/// <param name="updateValueFactory">A factory invoked with the existing value to produce the updated value.</param>
	/// <param name="factoryArgument">An argument passed to both factory delegates.</param>
	/// <returns>The new or updated value.</returns>
	public TValue AddOrUpdate<TArg>(TKey key,
									Func<TKey, TArg, TValue> addValueFactory,
									Func<TKey, TValue, TArg, TValue> updateValueFactory,
									TArg factoryArgument)
	{
		var addOrUpdate = GetDictionary().AddOrUpdate(key, addValueFactory, updateValueFactory, factoryArgument);

		SetTaskCompletionSource(key, addOrUpdate, found: true);

		return addOrUpdate;
	}

	/// <summary>
	/// Updates an existing entry or removes it based on a predicate.
	/// If <paramref name="isDeletePredicate"/> returns <see langword="true"/>, the entry is removed;
	/// otherwise, the value is replaced with the result of <paramref name="updateValueFactory"/>.
	/// </summary>
	/// <typeparam name="TArg">The type of the argument passed to the delegates.</typeparam>
	/// <param name="key">The key to update or remove.</param>
	/// <param name="isDeletePredicate">
	/// A predicate that determines whether the entry should be removed.
	/// Receives the key, current value, and <paramref name="factoryArgument"/>.
	/// </param>
	/// <param name="updateValueFactory">
	/// A factory that produces the updated value when the entry is not removed.
	/// Receives the key, current value, and <paramref name="factoryArgument"/>.
	/// </param>
	/// <param name="factoryArgument">An argument passed to both delegates.</param>
	/// <returns>The updated value, or <see langword="default"/> if the entry was removed or not found.</returns>
	public TValue UpdateOrRemove<TArg>(TKey key,
									   Func<TKey, TValue, TArg, bool> isDeletePredicate,
									   Func<TKey, TValue, TArg, TValue> updateValueFactory,
									   TArg factoryArgument)
	{
		while (TryGetValue(key, out var value))
		{
			if (isDeletePredicate(key, value, factoryArgument))
			{
				if (TryRemovePair(key, value))
				{
					return default!;
				}
			}
			else
			{
				var newValue = updateValueFactory(key, value, factoryArgument);

				if (TryUpdate(key, newValue, value))
				{
					return newValue;
				}
			}
		}

		return default!;
	}
}