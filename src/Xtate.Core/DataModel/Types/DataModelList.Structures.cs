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

using System;
using System.Diagnostics.CodeAnalysis;

namespace Xtate
{
	public abstract partial class DataModelList
	{
		[Serializable]
		[SuppressMessage(category: "Design", checkId: "CA1034:Nested types should not be visible", Justification = "Internal DTO")]
		public readonly struct Entry : IEquatable<Entry>
		{
			internal Entry(int index, in DataModelValue value)
			{
				Index = index;
				Value = value;
				Key = null;
				Access = default;
				Metadata = null;
			}

			internal Entry(int index, string? key, in DataModelValue value)
			{
				Index = index;
				Value = value;
				Key = key;
				Access = default;
				Metadata = null;
			}

			internal Entry(int index, in DataModelValue value, DataModelAccess access, DataModelList? metadata)
			{
				Index = index;
				Value = value;
				Key = null;
				Access = access;
				Metadata = metadata;
			}

			internal Entry(int index, string? key, in DataModelValue value, DataModelAccess access, DataModelList? metadata)
			{
				Index = index;
				Value = value;
				Key = key;
				Access = access;
				Metadata = metadata;
			}

			internal Entry(int index) : this() => Index = index;

			internal Entry(DataModelList? metadata) : this() => Metadata = metadata;

			public int             Index    { get; }
			public string?         Key      { get; }
			public DataModelAccess Access   { get; }
			public DataModelList?  Metadata { get; }
			public DataModelValue  Value    { get; }

		#region Interface IEquatable<Entry>

			public bool Equals(Entry other) => Index == other.Index && Key == other.Key && Access == other.Access && Equals(Metadata, other.Metadata) && Value.Equals(other.Value);

		#endregion

			public override bool Equals(object? obj) => obj is Entry other && Equals(other);

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = (Index * 397) ^ (Key != null ? Key.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (int) Access;
					hashCode = (hashCode * 397) ^ (Metadata != null ? Metadata.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ Value.GetHashCode();

					return hashCode;
				}
			}

			public static bool operator ==(Entry left, Entry right) => left.Equals(right);

			public static bool operator !=(Entry left, Entry right) => !left.Equals(right);
		}

		[Serializable]
		[SuppressMessage(category: "Design", checkId: "CA1034:Nested types should not be visible", Justification = "Internal DTO")]
		public readonly struct KeyValue : IEquatable<KeyValue>
		{
			internal KeyValue(string? key, in DataModelValue value)
			{
				Key = key;
				Value = value;
			}

			public string?        Key   { get; }
			public DataModelValue Value { get; }

		#region Interface IEquatable<KeyValue>

			public bool Equals(KeyValue other) => Key == other.Key && Value.Equals(other.Value);

		#endregion

			public override bool Equals(object? obj) => obj is KeyValue other && Equals(other);

			public override int GetHashCode() => unchecked(((Key != null ? Key.GetHashCode() : 0) * 397) ^ Value.GetHashCode());

			public static bool operator ==(KeyValue left, KeyValue right) => left.Equals(right);

			public static bool operator !=(KeyValue left, KeyValue right) => !left.Equals(right);
		}

		private struct Args
		{
			public AdapterBase      Adapter;
			public HashKey          HashKey;
			public int              Index;
			public string?          Key;
			public KeyMetaValue[]   KeyMetaValues;
			public HashKeyValue[]   KeyValues;
			public Meta             Meta;
			public MetaValue[]      MetaValues;
			public int              StoredCount;
			public DataModelValue   Value;
			public DataModelValue[] Values;
		}

		[Serializable]
		private readonly struct KeyMetaValue
		{
			public readonly HashKey        HashKey;
			public readonly Meta           Meta;
			public readonly DataModelValue Value;

			public KeyMetaValue(in HashKey hashKey, in Meta meta, in DataModelValue value)
			{
				HashKey = hashKey;
				Meta = meta;
				Value = value;
			}
		}

		[Serializable]
		private readonly struct MetaValue
		{
			public readonly Meta           Meta;
			public readonly DataModelValue Value;

			public MetaValue(in Meta meta, in DataModelValue value)
			{
				Meta = meta;
				Value = value;
			}
		}

		[Serializable]
		private readonly struct HashKeyValue
		{
			public readonly HashKey        HashKey;
			public readonly DataModelValue Value;

			public HashKeyValue(in HashKey hashKey, in DataModelValue value)
			{
				HashKey = hashKey;
				Value = value;
			}
		}

		[Serializable]
		private readonly struct HashKey
		{
			public readonly int     Hash;
			public readonly string? Key;

			public HashKey(int hash, string? key)
			{
				Hash = hash;
				Key = key;
			}
		}

		[Serializable]
		private readonly struct Meta
		{
			public readonly DataModelAccess Access;
			public readonly DataModelList?  Metadata;

			public Meta(DataModelAccess access, DataModelList? metadata)
			{
				Access = access;
				Metadata = metadata;
			}
		}
	}
}