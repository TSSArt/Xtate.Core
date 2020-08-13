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
	public sealed class DataModelObject : DataModelList, IDynamicMetaObjectProvider, IFormattable, IEnumerable<KeyValuePair<string, DataModelValue>>
	{
		public static readonly DataModelObject Empty = new DataModelObject(DataModelAccess.Constant, caseInsensitive: false);

		public DataModelObject() : this(DataModelAccess.Writable, caseInsensitive: false) { }

		public DataModelObject(bool caseInsensitive) : this(DataModelAccess.Writable, caseInsensitive) { }

		internal DataModelObject(bool isReadOnly, bool caseInsensitive) : this(isReadOnly ? DataModelAccess.ReadOnly : DataModelAccess.Writable, caseInsensitive) { }

		private DataModelObject(DataModelAccess access, bool caseInsensitive) : base(access, caseInsensitive) { }

		public DataModelValue this[string key]
		{
			get
			{
				TryGet(key, CaseInsensitive, out var entry);

				return entry.Value;
			}

			set => Set(key, CaseInsensitive, value, metadata: default);
		}

		public DataModelValue this[string key, bool caseInsensitive]
		{
			get
			{
				TryGet(key, caseInsensitive, out var entry);

				return entry.Value;
			}

			set => Set(key, caseInsensitive, value, metadata: default);
		}

	#region Interface IDynamicMetaObjectProvider

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new MetaObject(parameter, this, Dynamic.CreateMetaObject);

	#endregion

	#region Interface IEnumerable

		IEnumerator IEnumerable.GetEnumerator() => new KeyValueEnumerator(this);

	#endregion

	#region Interface IEnumerable<KeyValuePair<string,DataModelValue>>

		IEnumerator<KeyValuePair<string, DataModelValue>> IEnumerable<KeyValuePair<string, DataModelValue>>.GetEnumerator() => new KeyValuePairEnumerator(this);

	#endregion

	#region Interface IFormattable

		public string ToString(string? format, IFormatProvider? formatProvider)
		{
			if (Count == 0)
			{
				return "()";
			}

			var sb = new StringBuilder();
			var addDelimiter = false;

			sb.Append('(');
			foreach (var keyValue in KeyValues)
			{
				if (addDelimiter)
				{
					sb.Append(',');
				}
				else
				{
					addDelimiter = true;
				}

				sb.Append(keyValue.Key).Append('=').Append(keyValue.Value.ToString(format: null, formatProvider));
			}

			sb.Append(')');

			return sb.ToString();
		}

	#endregion

		public void Add(string key, in DataModelValue value)
		{
			if (key is null) throw new ArgumentNullException(nameof(key));

			Add(key, value, metadata: default);
		}

		public bool ContainsKey(string key) => TryGet(key, CaseInsensitive, out _);

		public bool ContainsKey(string key, bool caseInsensitive) => TryGet(key, caseInsensitive, out _);

		public bool RemoveFirst(string key) => RemoveFirst(key, CaseInsensitive);

		public bool RemoveFirst(string key, bool caseInsensitive)
		{
			if (TryGet(key, caseInsensitive, out var entry))
			{
				if (CanRemove(entry.Index))
				{
					Remove(entry.Index);
				}
				else
				{
					Set(entry.Index, key: default, value: default, metadata: default);
				}

				return true;
			}

			return false;
		}

		public bool RemoveAll(string key) => RemoveAll(key, CaseInsensitive);

		public bool RemoveAll(string key, bool caseInsensitive)
		{
			var enumerator = ListEntries(key, caseInsensitive).GetEnumerator();

			try
			{
				return RemoveNext(ref enumerator);
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		private bool RemoveNext(ref EntryByKeyEnumerator enumerator)
		{
			if (!enumerator.MoveNext())
			{
				return false;
			}

			var index = enumerator.Current.Index;

			RemoveNext(ref enumerator);

			if (CanRemove(index))
			{
				Remove(index);
			}
			else
			{
				Set(index, key: default, value: default, metadata: default);
			}

			return true;
		}

		private protected override DataModelList CreateNewInstance(DataModelAccess access) => new DataModelObject(access, CaseInsensitive);

		private protected override DataModelList GetEmptyInstance() => Empty;

		public KeyValuePairEnumerator GetEnumerator() => new KeyValuePairEnumerator(this);

		public override string ToString() => ToString(format: null, formatProvider: null);

		public DataModelObject CloneAsWritable() => (DataModelObject) DeepClone(DataModelAccess.Writable);

		public DataModelObject CloneAsReadOnly() => (DataModelObject) DeepClone(DataModelAccess.ReadOnly);

		public DataModelObject AsConstant() => (DataModelObject) DeepClone(DataModelAccess.Constant);

		internal class Dynamic : DynamicObject
		{
			private static readonly IDynamicMetaObjectProvider Instance = new Dynamic(default!);

			private static readonly ConstructorInfo ConstructorInfo = typeof(Dynamic).GetConstructor(new[] { typeof(DataModelObject) })!;

			private readonly DataModelObject _obj;

			public Dynamic(DataModelObject obj) => _obj = obj;

			public static DynamicMetaObject CreateMetaObject(Expression expression)
			{
				var newExpression = Expression.New(ConstructorInfo, Expression.Convert(expression, typeof(DataModelObject)));
				return Instance.GetMetaObject(newExpression);
			}

			public override bool TryGetMember(GetMemberBinder binder, out object? result)
			{
				result = _obj[binder.Name, binder.IgnoreCase].ToObject();

				return true;
			}

			public override bool TrySetMember(SetMemberBinder binder, object value)
			{
				_obj[binder.Name, binder.IgnoreCase] = DataModelValue.FromObject(value);

				return true;
			}

			public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
			{
				var arg = indexes.Length == 1 ? indexes[0] : null;

				if (arg is string key)
				{
					result = _obj[key].ToObject();

					return true;
				}

				result = null;

				return false;
			}

			public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
			{
				var arg = indexes.Length == 1 ? indexes[0] : null;

				if (arg is string key)
				{
					_obj[key] = DataModelValue.FromObject(value);

					return true;
				}

				return false;
			}

			public override bool TryConvert(ConvertBinder binder, out object? result)
			{
				if (binder.Type == typeof(DataModelList) || binder.Type == typeof(DataModelObject))
				{
					result = _obj;

					return true;
				}

				if (binder.Type == typeof(DataModelValue))
				{
					result = new DataModelValue(_obj);

					return true;
				}

				result = null;

				return false;
			}
		}

		[ExcludeFromCodeCoverage]
		[DebuggerDisplay(value: "{" + nameof(_value) + "}", Name = "{" + nameof(_key) + ",nq}")]
		private readonly struct DebugKeyValue
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly string? _key;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			private readonly DataModelValue _value;

			public DebugKeyValue(KeyValue keyValue)
			{
				_key = keyValue.Key;
				_value = keyValue.Value;
			}
		}

		[ExcludeFromCodeCoverage]
		[PublicAPI]
		private class DebugView
		{
			private readonly DataModelList _dataModelList;

			public DebugView(DataModelList dataModelList) => _dataModelList = dataModelList;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public DebugKeyValue[] Items => _dataModelList.KeyValues.Select(keyValue => new DebugKeyValue(keyValue)).ToArray();
		}

		[PublicAPI]
		public struct KeyValuePairEnumerator : IEnumerator<KeyValuePair<string, DataModelValue>>
		{
			private KeyValueEnumerator _enumerator;

			internal KeyValuePairEnumerator(DataModelList list)
			{
				_enumerator = list.KeyValues.GetEnumerator();
				Current = default;
			}

		#region Interface IDisposable

			public void Dispose() => _enumerator.Dispose();

		#endregion

		#region Interface IEnumerator

			public bool MoveNext()
			{
				while (_enumerator.MoveNext())
				{
					var current = _enumerator.Current;
					if (current.Key != null)
					{
						Current = new KeyValuePair<string, DataModelValue>(current.Key, current.Value);

						return true;
					}
				}

				Current = default;

				return false;
			}

			public void Reset() => _enumerator.Reset();

			object IEnumerator.Current => _enumerator.Current;

		#endregion

		#region Interface IEnumerator<KeyValuePair<string,DataModelValue>>

			public KeyValuePair<string, DataModelValue> Current { get; private set; }

		#endregion
		}
	}
}