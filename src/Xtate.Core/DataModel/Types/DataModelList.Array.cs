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

using System;
using System.Collections;
using System.Collections.Generic;
using Xtate.Core;

namespace Xtate
{
	public partial class DataModelList : IList<DataModelValue>
	{
	#region Interface ICollection<DataModelValue>

		public void CopyTo(DataModelValue[] array, int index)
		{
			if (array is null) throw new ArgumentNullException(nameof(array));
			if (index < 0 || index >= array.Length) throw new ArgumentOutOfRangeException(nameof(index), Resources.Exception_IndexShouldBeNonNegativeAndLessThenAarraySize);
			if (_count - index < array.Length) throw new ArgumentException(Resources.Exception_DestinationArrayIsNotLongEnough, nameof(array));

			foreach (var value in Values)
			{
				array[index ++] = value;
			}
		}

		public void Add(DataModelValue value) => Add(key: default, value, metadata: default);

		public bool Contains(DataModelValue value) => GetIndex(value) >= 0;

		public bool Remove(DataModelValue item)
		{
			var index = GetIndex(item);

			return index >= 0 && Remove(index);
		}

		bool ICollection<DataModelValue>.IsReadOnly => Access != DataModelAccess.Writable;

	#endregion

	#region Interface IEnumerable

		IEnumerator IEnumerable.GetEnumerator() => new ValueEnumerator(this);

	#endregion

	#region Interface IEnumerable<DataModelValue>

		IEnumerator<DataModelValue> IEnumerable<DataModelValue>.GetEnumerator() => new ValueEnumerator(this);

	#endregion

	#region Interface IList<DataModelValue>

		public DataModelValue this[int index]
		{
			get
			{
				TryGet(index, out var entry);

				return entry.Value;
			}

			set => Set(index, key: default, value, metadata: default);
		}

		public int IndexOf(DataModelValue item) => GetIndex(item);

		public void Insert(int index, DataModelValue item) => Insert(index, key: default, item, metadata: default);

		public void RemoveAt(int index) => Remove(index);

	#endregion

		public ValueEnumerator GetEnumerator() => new(this);

		public DataModelValue[] Slice(int start, int length)
		{
			if (start < 0 || start > _count) throw new ArgumentOutOfRangeException(nameof(start));
			if (length < 0 || length > _count - start) throw new ArgumentOutOfRangeException(nameof(length));

			if (length == 0)
			{
				return Array.Empty<DataModelValue>();
			}

			var array = new DataModelValue[length];
			var index = 0;

			foreach (var value in Values)
			{
				if (start > 0)
				{
					start --;

					continue;
				}

				array[index ++] = value;

				if (index == length)
				{
					break;
				}
			}

			return array;
		}

		private int GetIndex(in DataModelValue value)
		{
			foreach (var entry in Entries)
			{
				if (entry.Value == value)
				{
					return entry.Index;
				}
			}

			return -1;
		}
	}
}