#region Copyright © 2019-2020 Sergii Artemenko

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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	[DebuggerTypeProxy(typeof(DebugView))]
	[DebuggerDisplay(value: "Count = {" + nameof(Count) + "}")]
	[Serializable]
	public sealed class DataModelArray : DataModelList, IDynamicMetaObjectProvider, IList<DataModelValue>, IFormattable
	{
		public static readonly DataModelArray Empty = new DataModelArray(DataModelAccess.Constant);

		public DataModelArray() : this(DataModelAccess.Writable) { }

		internal DataModelArray(bool isReadOnly) : this(isReadOnly ? DataModelAccess.ReadOnly : DataModelAccess.Writable) { }

		private DataModelArray(DataModelAccess access) : base(access) { }

	#region Interface ICollection<DataModelValue>

		public void CopyTo(DataModelValue[] array, int index)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (index < 0 || index >= array.Length) throw new ArgumentOutOfRangeException(nameof(index), Resources.Exception_Index_should_be_non_negative_and_less_then_aarray_size);
			if (Count - index < array.Length) throw new ArgumentException(Resources.Exception_Destination_array_is_not_long_enough, nameof(array));

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

			return index >= 0 && base.Remove(index);
		}

		bool ICollection<DataModelValue>.IsReadOnly => Access != DataModelAccess.Writable;

	#endregion

	#region Interface IDynamicMetaObjectProvider

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new MetaObject(parameter, this, Dynamic.CreateMetaObject);

	#endregion

	#region Interface IEnumerable

		IEnumerator IEnumerable.GetEnumerator() => new ValueEnumerator(this);

	#endregion

	#region Interface IEnumerable<DataModelValue>

		IEnumerator<DataModelValue> IEnumerable<DataModelValue>.GetEnumerator() => new ValueEnumerator(this);

	#endregion

	#region Interface IFormattable

		public string ToString(string? format, IFormatProvider? formatProvider)
		{
			if (Count == 0)
			{
				return "[]";
			}

			var sb = new StringBuilder();
			var addDelimiter = false;

			sb.Append('[');
			foreach (var value in Values)
			{
				if (addDelimiter)
				{
					sb.Append(',');
				}
				else
				{
					addDelimiter = true;
				}

				sb.Append(value.ToString(format: null, formatProvider));
			}

			sb.Append(']');

			return sb.ToString();
		}

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

		public void RemoveAt(int index) => base.Remove(index);

	#endregion

		private protected override DataModelList CreateNewInstance(DataModelAccess access) => new DataModelArray(access);

		private protected override DataModelList GetEmptyInstance() => Empty;

		public ValueEnumerator GetEnumerator() => new ValueEnumerator(this);

		public override string ToString() => ToString(format: null, formatProvider: null);

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

		public DataModelArray CloneAsWritable() => (DataModelArray) DeepClone(DataModelAccess.Writable);

		public DataModelArray CloneAsReadOnly() => (DataModelArray) DeepClone(DataModelAccess.ReadOnly);

		public DataModelArray AsConstant() => (DataModelArray) DeepClone(DataModelAccess.Constant);

		internal class Dynamic : DynamicObject
		{
			private static readonly IDynamicMetaObjectProvider Instance = new Dynamic(default!);

			private static readonly ConstructorInfo ConstructorInfo = typeof(Dynamic).GetConstructor(new[] { typeof(DataModelArray) })!;

			private readonly DataModelArray _array;

			public Dynamic(DataModelArray array) => _array = array;

			public static DynamicMetaObject CreateMetaObject(Expression expression)
			{
				var newExpression = Expression.New(ConstructorInfo, Expression.Convert(expression, typeof(DataModelArray)));
				return Instance.GetMetaObject(newExpression);
			}

			public override bool TryGetMember(GetMemberBinder binder, out object? result)
			{
				if (string.Equals(binder.Name, b: @"length", binder.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
				{
					result = _array.Count;

					return true;
				}

				result = null;

				return false;
			}

			public override bool TrySetMember(SetMemberBinder binder, object value)
			{
				if (string.Equals(binder.Name, b: @"length", binder.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) && value is IConvertible convertible)
				{
					_array.SetLength(convertible.ToInt32(NumberFormatInfo.InvariantInfo));

					return true;
				}

				return false;
			}

			public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
			{
				var arg = indexes.Length == 1 ? indexes[0] : null;

				if (arg is IConvertible convertible)
				{
					result = _array[convertible.ToInt32(NumberFormatInfo.InvariantInfo)].ToObject();

					return true;
				}

				result = null;

				return false;
			}

			public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
			{
				var arg = indexes.Length == 1 ? indexes[0] : null;

				if (arg is IConvertible convertible)
				{
					_array[convertible.ToInt32(NumberFormatInfo.InvariantInfo)] = DataModelValue.FromObject(value);

					return true;
				}

				return false;
			}

			public override bool TryConvert(ConvertBinder binder, out object? result)
			{
				if (binder.Type == typeof(DataModelList) || binder.Type == typeof(DataModelArray))
				{
					result = _array;

					return true;
				}

				if (binder.Type == typeof(DataModelValue))
				{
					result = new DataModelValue(_array);

					return true;
				}

				result = null;

				return false;
			}
		}

		[ExcludeFromCodeCoverage]
		[DebuggerDisplay(value: "{" + nameof(_value) + "}", Name = "[{" + nameof(_index) + "}]")]
		private readonly struct DebugIndexValue
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly int _index;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			private readonly DataModelValue _value;

			public DebugIndexValue(in Entry entry)
			{
				_index = entry.Index;
				_value = entry.Value;
			}
		}

		[ExcludeFromCodeCoverage]
		[PublicAPI]
		private class DebugView
		{
			private readonly DataModelArray _array;

			public DebugView(DataModelArray array) => _array = array;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public DebugIndexValue[] Items => _array.Entries.Select(entry => new DebugIndexValue(entry)).ToArray();
		}
	}
}