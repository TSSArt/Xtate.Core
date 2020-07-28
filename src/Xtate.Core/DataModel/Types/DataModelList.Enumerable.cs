#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xtate.Annotations;

namespace Xtate
{
	public abstract partial class DataModelList
	{
		[PublicAPI]
		[SuppressMessage(category: "Design", checkId: "CA1034:Nested types should not be visible", Justification = "Wrapper")]
		[SuppressMessage(category: "Performance", checkId: "CA1815:Override equals and operator equals on value types", Justification = "No equality")]
		public readonly struct ValueEnumerable : IEnumerable<DataModelValue>
		{
			private readonly DataModelList _list;

			internal ValueEnumerable(DataModelList list) => _list = list;

		#region Interface IEnumerable

			IEnumerator IEnumerable.GetEnumerator() => new ValueEnumerator(_list);

		#endregion

		#region Interface IEnumerable<DataModelValue>

			IEnumerator<DataModelValue> IEnumerable<DataModelValue>.GetEnumerator() => new ValueEnumerator(_list);

		#endregion

			public ValueEnumerator GetEnumerator() => new ValueEnumerator(_list);
		}

		[PublicAPI]
		public struct ValueEnumerator : IEnumerator<DataModelValue>
		{
			private readonly int  _count;
			private          Args _args;

			internal ValueEnumerator(DataModelList list)
			{
				_count = list._count;
				list.CreateArgs(out _args);
				_args.Index = -1;
				Current = default;
			}

			public DataModelValue Current { get; private set; }

		#region Interface IDisposable

			public void Dispose() { }

		#endregion

		#region Interface IEnumerator

			public bool MoveNext()
			{
				if (_args.Index < _count)
				{
					if (++ _args.Index < _count)
					{
						Current = _args.Index < _args.StoredCount ? _args.Adapter.GetValueByIndex(ref _args) : default;

						return true;
					}
				}

				Current = default;

				return false;
			}

			public void Reset() => _args.Index = -1;

			object IEnumerator.Current => Current;

		#endregion

		#region Interface IEnumerator<DataModelValue>

			DataModelValue IEnumerator<DataModelValue>.Current => Current;

		#endregion
		}

		[PublicAPI]
		[SuppressMessage(category: "Design", checkId: "CA1034:Nested types should not be visible", Justification = "Wrapper")]
		[SuppressMessage(category: "Performance", checkId: "CA1815:Override equals and operator equals on value types", Justification = "No equality")]
		public readonly struct KeyValueEnumerable : IEnumerable<KeyValue>
		{
			private readonly DataModelList _list;

			internal KeyValueEnumerable(DataModelList list) => _list = list;

		#region Interface IEnumerable

			IEnumerator IEnumerable.GetEnumerator() => new KeyValueEnumerator(_list);

		#endregion

		#region Interface IEnumerable<KeyValue>

			IEnumerator<KeyValue> IEnumerable<KeyValue>.GetEnumerator() => new KeyValueEnumerator(_list);

		#endregion

			public KeyValueEnumerator GetEnumerator() => new KeyValueEnumerator(_list);
		}

		[PublicAPI]
		public struct KeyValueEnumerator : IEnumerator<KeyValue>
		{
			private readonly int  _count;
			private          Args _args;

			internal KeyValueEnumerator(DataModelList list)
			{
				_count = list._count;
				list.CreateArgs(out _args);
				_args.Index = -1;
				Current = default;
			}

		#region Interface IDisposable

			public void Dispose() { }

		#endregion

		#region Interface IEnumerator

			public bool MoveNext()
			{
				if (_args.Index < _count)
				{
					if (++ _args.Index < _count)
					{
						if (_args.Index < _args.StoredCount)
						{
							_args.Adapter.GetEntryByIndex(ref _args, out var entry);
							Current = new KeyValue(entry.Key, entry.Value);
						}
						else
						{
							Current = default;
						}

						return true;
					}
				}

				Current = default;

				return false;
			}

			public void Reset() => _args.Index = -1;

			object IEnumerator.Current => Current;

		#endregion

		#region Interface IEnumerator<KeyValue>

			public KeyValue Current { get; private set; }

		#endregion
		}

		[PublicAPI]
		[SuppressMessage(category: "Design", checkId: "CA1034:Nested types should not be visible", Justification = "Wrapper")]
		[SuppressMessage(category: "Performance", checkId: "CA1815:Override equals and operator equals on value types", Justification = "No equality")]
		public readonly struct EntryEnumerable : IEnumerable<Entry>
		{
			private readonly DataModelList _list;

			internal EntryEnumerable(DataModelList list) => _list = list;

		#region Interface IEnumerable

			IEnumerator IEnumerable.GetEnumerator() => new EntryEnumerator(_list);

		#endregion

		#region Interface IEnumerable<Entry>

			IEnumerator<Entry> IEnumerable<Entry>.GetEnumerator() => new EntryEnumerator(_list);

		#endregion

			public EntryEnumerator GetEnumerator() => new EntryEnumerator(_list);
		}

		[PublicAPI]
		public struct EntryEnumerator : IEnumerator<Entry>
		{
			private readonly int   _count;
			private          Args  _args;
			private          Entry _current;

			internal EntryEnumerator(DataModelList list)
			{
				_count = list._count;
				list.CreateArgs(out _args);
				_args.Index = -1;
				_current = default;
			}

		#region Interface IDisposable

			public void Dispose() { }

		#endregion

		#region Interface IEnumerator

			public bool MoveNext()
			{
				if (_args.Index < _count)
				{
					if (++ _args.Index < _count)
					{
						if (_args.Index < _args.StoredCount)
						{
							_args.Adapter.GetEntryByIndex(ref _args, out _current);
						}
						else
						{
							_current = default;
						}

						return true;
					}
				}

				_current = default;

				return false;
			}

			public void Reset() => _args.Index = -1;

			object IEnumerator.Current => _current;

		#endregion

		#region Interface IEnumerator<Entry>

			public Entry Current => _current;

		#endregion
		}

		[PublicAPI]
		[SuppressMessage(category: "Design", checkId: "CA1034:Nested types should not be visible", Justification = "Wrapper")]
		[SuppressMessage(category: "Performance", checkId: "CA1815:Override equals and operator equals on value types", Justification = "No equality")]
		public readonly struct ValueByKeyEnumerable : IEnumerable<DataModelValue>
		{
			private readonly bool          _caseInsensitive;
			private readonly string        _key;
			private readonly DataModelList _list;

			internal ValueByKeyEnumerable(DataModelList list, string key, bool caseInsensitive)
			{
				_list = list;
				_key = key;
				_caseInsensitive = caseInsensitive;
			}

		#region Interface IEnumerable

			IEnumerator IEnumerable.GetEnumerator() => new ValueByKeyEnumerator(_list, _key, _caseInsensitive);

		#endregion

		#region Interface IEnumerable<DataModelValue>

			IEnumerator<DataModelValue> IEnumerable<DataModelValue>.GetEnumerator() => new ValueByKeyEnumerator(_list, _key, _caseInsensitive);

		#endregion

			public ValueByKeyEnumerator GetEnumerator() => new ValueByKeyEnumerator(_list, _key, _caseInsensitive);
		}

		[PublicAPI]
		public struct ValueByKeyEnumerator : IEnumerator<DataModelValue>
		{
			private readonly bool          _caseInsensitive;
			private readonly int           _hash;
			private readonly DataModelList _list;
			private          Args          _args;

			internal ValueByKeyEnumerator(DataModelList list, string key, bool caseInsensitive)
			{
				_list = list;
				list.CreateArgs(out _args);
				_args.Index = -1;
				_args.Key = key;
				_caseInsensitive = caseInsensitive;
				_hash = list.UseHash(caseInsensitive) ? list.GetHashCodeForKey(key) : 0;
				Current = default;
			}

			public DataModelValue Current { get; private set; }

		#region Interface IDisposable

			public void Dispose() { }

		#endregion

		#region Interface IEnumerator

			public bool MoveNext()
			{
				if (_args.Index < _list._count)
				{
					++ _args.Index;

					if (_args.Key != null)
					{
						var result = _list.MoveNextKey(ref _args, _caseInsensitive, _hash);
						Current = result ? _args.Value : default;

						return result;
					}

					if (_args.Index < _list._count)
					{
						Current = _args.Index < _args.StoredCount ? _args.Adapter.GetValueByIndex(ref _args) : default;

						return true;
					}
				}

				Current = default;

				return false;
			}

			public void Reset() => _args.Index = -1;

			object IEnumerator.Current => Current;

		#endregion

		#region Interface IEnumerator<DataModelValue>

			DataModelValue IEnumerator<DataModelValue>.Current => Current;

		#endregion
		}

		[PublicAPI]
		[SuppressMessage(category: "Design", checkId: "CA1034:Nested types should not be visible", Justification = "Wrapper")]
		[SuppressMessage(category: "Performance", checkId: "CA1815:Override equals and operator equals on value types", Justification = "No equality")]
		public readonly struct KeyValueByKeyEnumerable : IEnumerable<KeyValue>
		{
			private readonly bool          _caseInsensitive;
			private readonly string        _key;
			private readonly DataModelList _list;

			internal KeyValueByKeyEnumerable(DataModelList list, string key, bool caseInsensitive)
			{
				_list = list;
				_key = key;
				_caseInsensitive = caseInsensitive;
			}

		#region Interface IEnumerable

			IEnumerator IEnumerable.GetEnumerator() => new KeyValueByKeyEnumerator(_list, _key, _caseInsensitive);

		#endregion

		#region Interface IEnumerable<KeyValue>

			IEnumerator<KeyValue> IEnumerable<KeyValue>.GetEnumerator() => new KeyValueByKeyEnumerator(_list, _key, _caseInsensitive);

		#endregion

			public KeyValueByKeyEnumerator GetEnumerator() => new KeyValueByKeyEnumerator(_list, _key, _caseInsensitive);
		}

		[PublicAPI]
		public struct KeyValueByKeyEnumerator : IEnumerator<KeyValue>
		{
			private readonly bool          _caseInsensitive;
			private readonly int           _hash;
			private readonly DataModelList _list;
			private          Args          _args;

			internal KeyValueByKeyEnumerator(DataModelList list, string key, bool caseInsensitive)
			{
				_list = list;
				list.CreateArgs(out _args);
				_args.Index = -1;
				_args.Key = key;
				_caseInsensitive = caseInsensitive;
				_hash = list.UseHash(caseInsensitive) ? list.GetHashCodeForKey(key) : 0;

				Current = default;
			}

		#region Interface IDisposable

			public void Dispose() { }

		#endregion

		#region Interface IEnumerator

			public bool MoveNext()
			{
				if (_args.Index < _list._count)
				{
					++ _args.Index;

					if (_args.Key != null)
					{
						var result = _list.MoveNextKey(ref _args, _caseInsensitive, _hash);
						Current = result ? new KeyValue(_args.HashKey.Key, _args.Value) : default;

						return result;
					}

					if (_args.Index < _list._count)
					{
						if (_args.Index < _args.StoredCount)
						{
							_args.Adapter.GetEntryByIndex(ref _args, out var entry);
							Current = new KeyValue(entry.Key, entry.Value);
						}
						else
						{
							Current = default;
						}

						return true;
					}
				}

				Current = default;

				return false;
			}

			public void Reset() => _args.Index = -1;

			object IEnumerator.Current => Current;

		#endregion

		#region Interface IEnumerator<KeyValue>

			public KeyValue Current { get; private set; }

		#endregion
		}

		[PublicAPI]
		[SuppressMessage(category: "Design", checkId: "CA1034:Nested types should not be visible", Justification = "Wrapper")]
		[SuppressMessage(category: "Performance", checkId: "CA1815:Override equals and operator equals on value types", Justification = "No equality")]
		public readonly struct EntryByKeyEnumerable : IEnumerable<Entry>
		{
			private readonly bool          _caseInsensitive;
			private readonly string        _key;
			private readonly DataModelList _list;

			internal EntryByKeyEnumerable(DataModelList list, string key, bool caseInsensitive)
			{
				_list = list;
				_key = key;
				_caseInsensitive = caseInsensitive;
			}

		#region Interface IEnumerable

			IEnumerator IEnumerable.GetEnumerator() => new EntryByKeyEnumerator(_list, _key, _caseInsensitive);

		#endregion

		#region Interface IEnumerable<Entry>

			IEnumerator<Entry> IEnumerable<Entry>.GetEnumerator() => new EntryByKeyEnumerator(_list, _key, _caseInsensitive);

		#endregion

			public EntryByKeyEnumerator GetEnumerator() => new EntryByKeyEnumerator(_list, _key, _caseInsensitive);
		}

		[PublicAPI]
		public struct EntryByKeyEnumerator : IEnumerator<Entry>
		{
			private readonly bool          _caseInsensitive;
			private readonly int           _hash;
			private readonly DataModelList _list;
			private          Args          _args;
			private          Entry         _current;

			internal EntryByKeyEnumerator(DataModelList list, string key, bool caseInsensitive)
			{
				_list = list;
				list.CreateArgs(out _args);
				_args.Index = -1;
				_args.Key = key;
				_caseInsensitive = caseInsensitive;
				_hash = list.UseHash(caseInsensitive) ? list.GetHashCodeForKey(key) : 0;

				_current = default;
			}

		#region Interface IDisposable

			public void Dispose() { }

		#endregion

		#region Interface IEnumerator

			public bool MoveNext()
			{
				if (_args.Index < _list._count)
				{
					++ _args.Index;

					if (_args.Key != null)
					{
						var result = _list.MoveNextKey(ref _args, _caseInsensitive, _hash);
						_current = result ? new Entry(_args.Index, _args.Key, _args.Value, _args.Meta.Access, _args.Meta.Metadata) : default;

						return result;
					}

					if (_args.Index < _list._count)
					{
						if (_args.Index < _args.StoredCount)
						{
							_args.Adapter.GetEntryByIndex(ref _args, out _current);
						}
						else
						{
							_current = default;
						}

						return true;
					}
				}

				_current = default;

				return false;
			}

			public void Reset() => _args.Index = -1;

			object IEnumerator.Current => _current;

		#endregion

		#region Interface IEnumerator<Entry>

			public Entry Current => _current;

		#endregion
		}
	}
}