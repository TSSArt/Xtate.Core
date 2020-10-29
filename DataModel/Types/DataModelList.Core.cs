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
using System.Collections.Generic;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	[Serializable]
	public sealed partial class DataModelList
	{
		public delegate void ChangeHandler(ChangeAction action, in Entry entry);

		public enum ChangeAction
		{
			Append,
			SetAt,
			InsertAt,
			RemoveAt,
			SetCsKey,
			SetCiKey,
			SetCount,
			SetMetadata,
			Reset
		}

		private const          int           CaseInsensitiveBit = 0x10;
		private const          int           AccessMask         = 0xF;
		private const          int           AccessConstant     = (int) DataModelAccess.Constant;
		private const          int           AccessReadOnly     = (int) DataModelAccess.ReadOnly;
		public static readonly DataModelList Empty              = new DataModelList(DataModelAccess.Constant);

		private static readonly ValueAdapter        ValueAdapterInstance        = new ValueAdapter();
		private static readonly KeyValueAdapter     KeyValueAdapterInstance     = new KeyValueAdapter();
		private static readonly MetaValueAdapter    MetaValueAdapterInstance    = new MetaValueAdapter();
		private static readonly KeyMetaValueAdapter KeyMetaValueAdapterInstance = new KeyMetaValueAdapter();

		private Array          _array;
		private int            _flags;
		private DataModelList? _metadata;

		public DataModelList() : this(DataModelAccess.Writable) { }

		public DataModelList(bool caseInsensitive) : this(DataModelAccess.Writable, caseInsensitive) { }

		internal DataModelList(DataModelAccess access, bool caseInsensitive = false)
		{
			_flags = caseInsensitive ? (int) access | CaseInsensitiveBit : (int) access;
			_array = Array.Empty<DataModelValue>();
		}

		public bool CaseInsensitive => (_flags & CaseInsensitiveBit) != 0;

		public bool HasItemAccess
		{
			get
			{
				CreateArgs(out var args);

				return args.Adapter.IsAccessAvailable();
			}
		}

		public bool HasKeys
		{
			get
			{
				if (Count == 0 || _array.Length == 0)
				{
					return false;
				}

				CreateArgs(out var args);

				if (args.Adapter.IsKeyAvailable())
				{
					for (args.Index = 0; args.Index < args.StoredCount; args.Index ++)
					{
						args.Adapter.ReadToArgsByIndex(ref args);

						if (args.HashKey.Key is not null)
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		public DataModelAccess Access
		{
			get => (DataModelAccess) (_flags & AccessMask);

			internal set
			{
				var access = Access;

				if (value == access)
				{
					return;
				}

				if (value == DataModelAccess.ReadOnly && access == DataModelAccess.Writable)
				{
					_flags = CaseInsensitive ? AccessReadOnly | CaseInsensitiveBit : AccessReadOnly;

					return;
				}

				if (value == DataModelAccess.Constant)
				{
					_flags = CaseInsensitive ? AccessConstant | CaseInsensitiveBit : AccessConstant;

					if (Count > 0 && _array.Length > 0)
					{
						CreateArgs(out var args);
						for (args.Index = 0; args.Index < args.StoredCount; args.Index ++)
						{
							args.Adapter.ReadToArgsByIndex(ref args);
							args.Value.MakeDeepConstant();
							args.Meta.Metadata?.MakeDeepConstant();
						}
					}

					_metadata?.MakeDeepConstant();

					return;
				}

				throw new InfrastructureException(Resources.Exception_Access_can_t_be_changed);
			}
		}

		public ValueEnumerable Values => new ValueEnumerable(this);

		public KeyValueEnumerable KeyValues => new KeyValueEnumerable(this);

		public KeyValuePairEnumerable KeyValuePairs => new KeyValuePairEnumerable(this);

		public EntryEnumerable Entries => new EntryEnumerable(this);

	#region Interface ICollection<DataModelValue>

		public void Clear() => ClearItems(DataModelAccess.Writable, throwOnDeny: true);

		public int Count { get; private set; }

	#endregion

		public DataModelList CloneAsWritable() => DeepClone(DataModelAccess.Writable);

		public DataModelList CloneAsReadOnly() => DeepClone(DataModelAccess.ReadOnly);

		public DataModelList AsConstant() => DeepClone(DataModelAccess.Constant);

		public ValueByKeyEnumerable ListValues(string key, bool caseInsensitive) => new ValueByKeyEnumerable(this, key, caseInsensitive);

		public KeyValueByKeyEnumerable ListKeyValues(string key, bool caseInsensitive) => new KeyValueByKeyEnumerable(this, key, caseInsensitive);

		public EntryByKeyEnumerable ListEntries(string key, bool caseInsensitive) => new EntryByKeyEnumerable(this, key, caseInsensitive);

		public DataModelList? GetMetadata() => _metadata;

		public bool TryGet(int index, out Entry entry)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), Resources.Exception_Index_value_must_be_non_negative_integer);

			if (index >= Count)
			{
				entry = default;

				return false;
			}

			if (index >= _array.Length)
			{
				entry = default;

				return true;
			}

			CreateArgs(out var args);
			args.Index = index;

			args.Adapter.GetEntryByIndex(ref args, out entry);

			return true;
		}

		public bool TryGet(string key, bool caseInsensitive, out Entry entry)
		{
			if (key is null) throw new ArgumentNullException(nameof(key));

			if (Count > 0 && _array.Length > 0)
			{
				CreateArgs(out var args);
				args.Key = key;

				if (UseHash(caseInsensitive))
				{
					FindNextKey(ref args, caseInsensitive, GetHashCodeForKey(args.Key));
				}
				else
				{
					FindNextKey(ref args, caseInsensitive);
				}

				if (args.Index < args.StoredCount)
				{
					entry = new Entry(args.Index, args.Key, args.Value, args.Meta.Access, args.Meta.Metadata);

					return true;
				}
			}

			entry = default;

			return false;
		}

		public void Set(string key, bool caseInsensitive, in DataModelValue value, DataModelList? metadata)
		{
			if (key is null) throw new ArgumentNullException(nameof(key));

			CreateArgs(out var args);
			args.Key = key;
			args.Value = value;
			args.Meta = new Meta(DataModelAccess.Writable, metadata);

			SetKey(ref args, caseInsensitive, DataModelAccess.Writable, throwOnDeny: true);
		}

		public void Set(int index, string? key, in DataModelValue value, DataModelList? metadata)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), Resources.Exception_Index_value_must_be_non_negative_integer);

			CreateArgs(out var args);
			args.Index = index;
			args.Value = value;
			args.Meta = new Meta(DataModelAccess.Writable, metadata);

			if (key is not null)
			{
				args.HashKey = CreateHashKey(key);
			}

			SetItem(ref args, DataModelAccess.Writable, throwOnDeny: true);
		}

		public void Insert(int index, string? key, in DataModelValue value, DataModelList? metadata)
		{
			CreateArgs(out var args);
			args.Index = index;
			args.Value = value;
			args.Meta = new Meta(DataModelAccess.Writable, metadata);

			if (key is not null)
			{
				args.HashKey = CreateHashKey(key);
			}

			InsertItem(ref args, DataModelAccess.Writable, throwOnDeny: true);
		}

		public void Add(string? key, in DataModelValue value, DataModelList? metadata)
		{
			CreateArgs(out var args);

			args.Value = value;
			args.Meta = new Meta(DataModelAccess.Writable, metadata);

			if (key is not null)
			{
				args.HashKey = CreateHashKey(key);
			}

			AddItem(ref args, DataModelAccess.Writable, throwOnDeny: true);
		}

		public bool Remove(int index)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), Resources.Exception_Index_value_must_be_non_negative_integer);

			CreateArgs(out var args);
			args.Index = index;

			return RemoveAtItem(ref args, DataModelAccess.Writable, throwOnDeny: true);
		}

		public void SetMetadata(DataModelList? metadata) => SetMetadata(metadata, DataModelAccess.Writable, throwOnDeny: true);

		public void SetLength(int length)
		{
			if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), Resources.Exception_Value_must_be_non_negative_integer);

			SetLengthItems(length, DataModelAccess.Writable, throwOnDeny: true);
		}

		private bool UseHash(bool caseInsensitive) => CaseInsensitive || !caseInsensitive;

		private bool MoveNextKey(ref Args args, bool caseInsensitive, int hash)
		{
			if (UseHash(caseInsensitive))
			{
				FindNextKey(ref args, caseInsensitive, hash);
			}
			else
			{
				FindNextKey(ref args, caseInsensitive);
			}

			if (args.Index < args.StoredCount)
			{
				return true;
			}

			args.Index = Count;

			return false;
		}

		private static void FindNextKey(ref Args args, bool caseInsensitive)
		{
			if (!args.Adapter.IsKeyAvailable())
			{
				args.Index = args.StoredCount;

				return;
			}

			var comparer = caseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

			for (; args.Index < args.StoredCount; args.Index ++)
			{
				args.Adapter.ReadToArgsByIndex(ref args);

				if (comparer.Equals(args.HashKey.Key, args.Key))
				{
					return;
				}
			}
		}

		private static void FindNextKey(ref Args args, bool caseInsensitive, int hash)
		{
			if (!args.Adapter.IsKeyAvailable())
			{
				args.Index = args.StoredCount;

				return;
			}

			var comparer = caseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

			for (; args.Index < args.StoredCount; args.Index ++)
			{
				args.Adapter.ReadToArgsByIndex(ref args);

				if (args.HashKey.Hash != hash)
				{
					continue;
				}

				if (comparer.Equals(args.HashKey.Key, args.Key))
				{
					return;
				}
			}
		}

		internal bool NextEntry(ref int cursor, out Entry entry)
		{
			if (cursor < Count)
			{
				cursor ++;

				if (0 <= cursor && cursor < Count)
				{
					CreateArgs(out var args);
					args.Index = cursor;
					args.Adapter.GetEntryByIndex(ref args, out entry);

					return true;
				}
			}

			entry = default;

			return false;
		}

		internal bool PreviousEntry(ref int cursor, out Entry entry)
		{
			if (cursor >= 0)
			{
				cursor --;

				if (0 <= cursor && cursor < Count)
				{
					CreateArgs(out var args);
					args.Index = cursor;
					args.Adapter.GetEntryByIndex(ref args, out entry);

					return true;
				}
			}

			entry = default;

			return false;
		}

		public event ChangeHandler? Change;

		public void MakeReadOnly() => Access = DataModelAccess.ReadOnly;

		public void MakeDeepConstant() => Access = DataModelAccess.Constant;

		public bool CanAdd()
		{
			CreateArgs(out var args);

			return AddItem(ref args, DataModelAccess.Constant, throwOnDeny: false);
		}

		public bool CanClear() => ClearItems(DataModelAccess.Constant, throwOnDeny: false);

		public bool CanInsert(int index)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), Resources.Exception_Index_value_must_be_non_negative_integer);

			CreateArgs(out var args);
			args.Index = index;

			return InsertItem(ref args, DataModelAccess.Constant, throwOnDeny: false);
		}

		public bool CanRemove(int index)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), Resources.Exception_Index_value_must_be_non_negative_integer);

			CreateArgs(out var args);
			args.Index = index;

			return RemoveAtItem(ref args, DataModelAccess.Constant, throwOnDeny: false);
		}

		public bool CanSet(int index)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), Resources.Exception_Index_value_must_be_non_negative_integer);

			CreateArgs(out var args);
			args.Index = index;

			return SetItem(ref args, DataModelAccess.Constant, throwOnDeny: false);
		}

		public bool CanSet(string key, bool caseInsensitive)
		{
			if (key is null) throw new ArgumentNullException(nameof(key));

			CreateArgs(out var args);
			args.Key = key;

			return SetKey(ref args, caseInsensitive, DataModelAccess.Constant, throwOnDeny: false);
		}

		public bool CanSetLength(int length)
		{
			if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), Resources.Exception_Value_must_be_non_negative_integer);

			return SetLengthItems(length, DataModelAccess.Constant, throwOnDeny: false);
		}

		public bool CanSetMetadata() => SetMetadata(metadata: default, DataModelAccess.Constant, throwOnDeny: false);

		internal bool AddInternal(string? key, in DataModelValue value, DataModelAccess access, DataModelList? metadata = default, bool throwOnDeny = true)
		{
			CreateArgs(out var args);
			args.Value = value;
			args.Meta = new Meta(access, metadata);

			if (key is not null)
			{
				args.HashKey = CreateHashKey(key);
			}

			return AddItem(ref args, DataModelAccess.ReadOnly, throwOnDeny);
		}

		internal bool ClearInternal(bool throwOnDeny = true) => ClearItems(DataModelAccess.ReadOnly, throwOnDeny);

		internal bool InsertInternal(int index, string? key, in DataModelValue value, DataModelAccess access, DataModelList? metadata = default, bool throwOnDeny = true)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), Resources.Exception_Index_value_must_be_non_negative_integer);

			CreateArgs(out var args);
			args.Index = index;
			args.Value = value;
			args.Meta = new Meta(access, metadata);

			if (key is not null)
			{
				args.HashKey = CreateHashKey(key);
			}

			return InsertItem(ref args, DataModelAccess.ReadOnly, throwOnDeny);
		}

		internal bool RemoveInternal(int index, bool throwOnDeny = true)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), Resources.Exception_Index_value_must_be_non_negative_integer);

			CreateArgs(out var args);
			args.Index = index;

			return RemoveAtItem(ref args, DataModelAccess.ReadOnly, throwOnDeny);
		}

		internal bool SetInternal(string key, bool caseInsensitive, in DataModelValue value, DataModelAccess access, DataModelList? metadata = default, bool throwOnDeny = true)
		{
			if (key is null) throw new ArgumentNullException(nameof(key));

			CreateArgs(out var args);
			args.Key = key;
			args.Value = value;
			args.Meta = new Meta(access, metadata);

			return SetKey(ref args, caseInsensitive, DataModelAccess.ReadOnly, throwOnDeny);
		}

		internal bool SetInternal(int index, string? key, in DataModelValue value, DataModelAccess access, DataModelList? metadata = default, bool throwOnDeny = true)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), Resources.Exception_Index_value_must_be_non_negative_integer);

			CreateArgs(out var args);
			args.Index = index;
			args.Value = value;
			args.Meta = new Meta(access, metadata);

			if (key is not null)
			{
				args.HashKey = CreateHashKey(key);
			}

			return SetItem(ref args, DataModelAccess.ReadOnly, throwOnDeny);
		}

		internal bool SetLengthInternal(int length, bool throwOnDeny = true)
		{
			if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), Resources.Exception_Value_must_be_non_negative_integer);

			return SetLengthItems(length, DataModelAccess.ReadOnly, throwOnDeny);
		}

		internal bool SetMetadataInternal(DataModelList? metadata, bool throwOnDeny = true) => SetMetadata(metadata, DataModelAccess.ReadOnly, throwOnDeny);

		private HashKey CreateHashKey(string key) => new HashKey(GetHashCodeForKey(key), key);

		private int GetHashCodeForKey(string key) => (CaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal).GetHashCode(key);

		private bool CheckAccess(ref Args args, int start, int end, DataModelAccess requestedAccess, bool throwOnDeny, out bool result)
		{
			if (NoAccess(Access))
			{
				result = false;

				return true;
			}

			if (start >= args.StoredCount || end <= 0)
			{
				return result = requestedAccess == DataModelAccess.Constant;
			}

			if (end > args.StoredCount)
			{
				end = args.StoredCount;
			}

			if (start < end && args.Adapter.IsAccessAvailable())
			{
				var savedIndex = args.Index;

				for (args.Index = start; args.Index < end; args.Index ++)
				{
					var access = args.Adapter.GetAccessByIndex(ref args);

					if (NoAccess(access))
					{
						args.Index = savedIndex;
						result = false;

						return true;
					}
				}

				args.Index = savedIndex;
			}

			return result = requestedAccess == DataModelAccess.Constant;

			bool NoAccess(DataModelAccess access)
			{
				if (access == DataModelAccess.Writable)
				{
					return false;
				}

				if (access != DataModelAccess.Constant && requestedAccess == DataModelAccess.ReadOnly)
				{
					return false;
				}

				if (throwOnDeny)
				{
					throw new InvalidOperationException(Resources.Exception_Object_can_not_be_modified);
				}

				return true;
			}
		}

		private void OnChange(ChangeAction action, ref Args args)
		{
			if (Change is null)
			{
				return;
			}

			Entry entry;
			switch (action)
			{
				case ChangeAction.SetMetadata:
					entry = new Entry(args.Meta.Metadata);
					break;

				case ChangeAction.SetCount:
					entry = new Entry(args.Index);
					break;

				default:
					if (0 <= args.Index && args.Index < args.StoredCount)
					{
						args.Adapter.GetEntryByIndex(ref args, out entry);
					}
					else
					{
						entry = new Entry(args.Index);
					}

					break;
			}

			Change(action, entry);
		}

		private void EnsureTypeAndCapacity(ref Args args, int minCapacity)
		{
			var size = _array.Length;
			if (size < minCapacity)
			{
				size = size == 0 ? 4 : size * 2;
				size = size < minCapacity ? minCapacity : size;
			}

			_array = args.Adapter.EnsureArray(ref args, size);
		}

		private bool AddItem(ref Args args, DataModelAccess requestedAccess, bool throwOnDeny)
		{
			if (CheckAccess(ref args, start: 0, end: 0, requestedAccess, throwOnDeny, out var result))
			{
				return result;
			}

			EnsureTypeAndCapacity(ref args, Count + 1);

			args.Index = Count ++;
			args.StoredCount = Count;

			args.Adapter.AssignItemByIndex(ref args);

			OnChange(ChangeAction.Append, ref args);

			return true;
		}

		private bool ClearItems(DataModelAccess requestedAccess, bool throwOnDeny)
		{
			CreateArgs(out var args);

			if (CheckAccess(ref args, start: 0, args.StoredCount, requestedAccess, throwOnDeny, out var result))
			{
				return result;
			}

			args.Index = 0;
			OnChange(ChangeAction.SetCount, ref args);

			Array.Clear(_array, index: 0, args.StoredCount);

			Count = 0;

			return true;
		}

		private bool RemoveAtItem(ref Args args, DataModelAccess requestedAccess, bool throwOnDeny)
		{
			if (CheckAccess(ref args, args.Index, args.StoredCount, requestedAccess, throwOnDeny, out var result))
			{
				return result;
			}

			OnChange(ChangeAction.RemoveAt, ref args);

			if (args.Index >= Count)
			{
				return false;
			}

			Count --;
			args.StoredCount --;

			if (args.Index <= args.StoredCount)
			{
				Array.Copy(_array, args.Index + 1, _array, args.Index, args.StoredCount - args.Index);

				args.Index = args.StoredCount;
				args.Value = default;
				args.HashKey = default;
				args.Meta = default;
				args.Adapter.AssignItemByIndex(ref args);
			}

			return true;
		}

		private bool InsertItem(ref Args args, DataModelAccess requestedAccess, bool throwOnDeny)
		{
			if (CheckAccess(ref args, args.Index, args.StoredCount, requestedAccess, throwOnDeny, out var result))
			{
				return result;
			}

			if (args.Index >= Count)
			{
				EnsureTypeAndCapacity(ref args, args.Index + 1);

				Count = args.Index + 1;
			}
			else
			{
				EnsureTypeAndCapacity(ref args, Count + 1);

				Array.Copy(_array, args.Index, _array, args.Index + 1, Count - args.Index);

				Count ++;
			}

			args.StoredCount = Count;
			args.Adapter.AssignItemByIndex(ref args);

			OnChange(ChangeAction.InsertAt, ref args);

			return true;
		}

		private bool SetKey(ref Args args, bool caseInsensitive, DataModelAccess requestedAccess, bool throwOnDeny)
		{
			Infrastructure.NotNull(args.Key);

			var findArgs = args;
			var hash = GetHashCodeForKey(args.Key);

			if (UseHash(caseInsensitive))
			{
				FindNextKey(ref findArgs, caseInsensitive, hash);
			}
			else
			{
				FindNextKey(ref findArgs, caseInsensitive);
			}

			if (CheckAccess(ref args, findArgs.Index, findArgs.Index + 1, requestedAccess, throwOnDeny, out var result))
			{
				return result;
			}

			OnChange(ChangeAction.Reset, ref findArgs);

			if (findArgs.Index < findArgs.StoredCount)
			{
				args.Index = findArgs.Index;
				args.HashKey = findArgs.HashKey;
			}
			else
			{
				args.HashKey = new HashKey(hash, args.Key);
				EnsureTypeAndCapacity(ref args, Count + 1);
				args.Index = Count ++;
				args.StoredCount = Count;
			}

			args.Adapter.AssignItemByIndex(ref args);

			OnChange(caseInsensitive ? ChangeAction.SetCiKey : ChangeAction.SetCsKey, ref args);

			return true;
		}

		private bool SetItem(ref Args args, DataModelAccess requestedAccess, bool throwOnDeny)
		{
			if (CheckAccess(ref args, args.Index, args.Index + 1, requestedAccess, throwOnDeny, out var result))
			{
				return result;
			}

			if (args.Index >= Count)
			{
				EnsureTypeAndCapacity(ref args, args.Index + 1);

				Count = args.Index + 1;
				args.StoredCount = Count;
			}
			else
			{
				EnsureTypeAndCapacity(ref args, Count);
			}

			OnChange(ChangeAction.Reset, ref args);

			args.Adapter.AssignItemByIndex(ref args);

			OnChange(ChangeAction.SetAt, ref args);

			return true;
		}

		private bool SetLengthItems(int length, DataModelAccess requestedAccess, bool throwOnDeny)
		{
			CreateArgs(out var args);
			args.Index = length;

			if (CheckAccess(ref args, length, args.StoredCount, requestedAccess, throwOnDeny, out var result))
			{
				return result;
			}

			OnChange(ChangeAction.SetCount, ref args);

			if (length < args.StoredCount)
			{
				Array.Clear(_array, length, args.StoredCount - length);
			}

			Count = length;

			return true;
		}

		private bool SetMetadata(DataModelList? metadata, DataModelAccess requestedAccess, bool throwOnDeny)
		{
			CreateArgs(out var args);
			args.Meta = new Meta(access: default, metadata);

			if (CheckAccess(ref args, start: 0, end: 0, requestedAccess, throwOnDeny, out var result))
			{
				return result;
			}

			OnChange(ChangeAction.SetMetadata, ref args);

			_metadata = metadata;

			return true;
		}

		private void CreateArgs(out Args args)
		{
			args = default;
			args.StoredCount = Math.Min(Count, _array.Length);

			switch (_array)
			{
				case HashKeyValue[] array:
					args.Adapter = KeyValueAdapterInstance;
					args.KeyValues = array;
					break;

				case DataModelValue[] array:
					args.Adapter = ValueAdapterInstance;
					args.Values = array;
					break;

				case KeyMetaValue[] array:
					args.Adapter = KeyMetaValueAdapterInstance;
					args.KeyMetaValues = array;
					break;

				case MetaValue[] array:
					args.Adapter = MetaValueAdapterInstance;
					args.MetaValues = array;
					break;

				default:
					Infrastructure.UnexpectedValue(_array);
					break;
			}
		}

		internal DataModelList DeepClone(DataModelAccess targetAccess)
		{
			Dictionary<object, DataModelList>? map = null;

			return DeepCloneWithMap(targetAccess, ref map);
		}

		internal DataModelList DeepCloneWithMap(DataModelAccess targetAccess, ref Dictionary<object, DataModelList>? map)
		{
			if (targetAccess == DataModelAccess.Constant)
			{
				if (Count == 0)
				{
					return Empty;
				}

				if (Access == DataModelAccess.Constant)
				{
					return this;
				}
			}

			map ??= new Dictionary<object, DataModelList>();

			if (map.TryGetValue(this, out var val))
			{
				return val;
			}

			var clone = new DataModelList(targetAccess);

			map[this] = clone;

			if (Count == 0)
			{
				return clone;
			}

			CreateArgs(out var args);

			Args cloneArgs = default;
			clone._array = args.Adapter.CreateArray(ref cloneArgs, Count);
			clone.Count = Count;
			clone._metadata = _metadata?.DeepCloneWithMap(targetAccess, ref map);

			for (args.Index = 0; args.Index < args.StoredCount; args.Index ++)
			{
				args.Adapter.ReadToArgsByIndex(ref args);

				cloneArgs.Index = args.Index;
				cloneArgs.HashKey = args.HashKey;
				cloneArgs.Value = args.Value.DeepCloneWithMap(targetAccess, ref map);
				cloneArgs.Meta = new Meta(args.Meta.Access, args.Meta.Metadata?.DeepCloneWithMap(targetAccess, ref map));

				args.Adapter.AssignItemByIndex(ref cloneArgs);
			}

			return clone;
		}
	}
}