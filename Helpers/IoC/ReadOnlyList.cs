// Copyright © 2019-2025 Sergii Artemenko
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

using System.Runtime.InteropServices;

namespace Xtate.Core;

public abstract class ReadOnlyList<T> : IList<T>, IList, IReadOnlyList<T>
{
	private T[] _array = [];

	protected ReadOnlyList() { }

	protected ReadOnlyList(ImmutableArray<T> list) => Items = list;

	protected ImmutableArray<T> Items
	{
		get => ImmutableCollectionsMarshal.AsImmutableArray(_array);
		set => _array = ImmutableCollectionsMarshal.AsArray(value) ?? [];
	}

#region Interface ICollection

	void ICollection.CopyTo(Array array, int index) => Array.Copy(_array, sourceIndex: 0, array, index, _array.Length);

	bool ICollection.IsSynchronized => true;

	object ICollection.SyncRoot => throw new NotSupportedException();

#endregion

#region Interface ICollection<T>

	public bool Contains(T value) => Array.IndexOf(_array, value) >= 0;

	public void CopyTo(T[] array, int index) => _array.CopyTo(array, index);

	void ICollection<T>.Add(T value) => throw ReadOnlyCollectionException();

	void ICollection<T>.Clear() => throw ReadOnlyCollectionException();

	bool ICollection<T>.Remove(T value) => throw ReadOnlyCollectionException();

	public int Count => _array.Length;

	bool ICollection<T>.IsReadOnly => true;

#endregion

#region Interface IEnumerable

	IEnumerator IEnumerable.GetEnumerator() => _array.GetEnumerator();

#endregion

#region Interface IEnumerable<T>

	IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();

	#endregion

	#region Interface IList

	object? IList.this[int index]
	{
		get => _array[index];
		set => throw ReadOnlyCollectionException();
	}

	int IList.Add(object? value) => throw ReadOnlyCollectionException();

	void IList.Clear() => throw ReadOnlyCollectionException();

	bool IList.Contains(object? value) => IndexOf(value) >= 0;

	int IList.IndexOf(object? value) => IndexOf(value);

	void IList.Insert(int index, object? value) => throw ReadOnlyCollectionException();

	void IList.Remove(object? value) => throw ReadOnlyCollectionException();

	void IList.RemoveAt(int index) => throw ReadOnlyCollectionException();

	bool IList.IsFixedSize => true;

	bool IList.IsReadOnly => true;

#endregion

#region Interface IList<T>

	T IList<T>.this[int index]
	{
		get => _array[index];
		set => throw ReadOnlyCollectionException();
	}

	public int IndexOf(T value) => Array.IndexOf(_array, value);

	void IList<T>.Insert(int index, T value) => throw ReadOnlyCollectionException();

	void IList<T>.RemoveAt(int index) => throw ReadOnlyCollectionException();

#endregion

#region Interface IReadOnlyList<T>

	public T this[int index] => _array[index];

#endregion

	public ImmutableArray<T>.Enumerator GetEnumerator() => Items.GetEnumerator();

	private static NotSupportedException ReadOnlyCollectionException() => new("This collection is read-only and cannot be modified");

	private int IndexOf(object? value) =>
		value is T val
			? Array.IndexOf(_array, val)
			: value == null && default(T) == null
				? Array.IndexOf(_array, default)
				: -1;

	public ReadOnlySpan<T> AsSpan() => _array.AsSpan();

	public ReadOnlyMemory<T> AsMemory() => _array.AsMemory();
}