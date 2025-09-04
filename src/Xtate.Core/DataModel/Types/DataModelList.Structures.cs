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

namespace Xtate;

public partial class DataModelList
{
    [Serializable]
    public readonly struct Entry : IEquatable<Entry>
    {
        internal Entry(int index, in DataModelValue value)
        {
            Index = index;
            Value = value;
            Key = default;
            Access = default;
            Metadata = default;
        }

        internal Entry(int index, string? key, in DataModelValue value)
        {
            Index = index;
            Value = value;
            Key = key;
            Access = default;
            Metadata = default;
        }

        internal Entry(int index,
                       in DataModelValue value,
                       DataModelAccess access,
                       DataModelList? metadata)
        {
            Index = index;
            Value = value;
            Key = default;
            Access = access;
            Metadata = metadata;
        }

        internal Entry(int index,
                       string? key,
                       in DataModelValue value,
                       DataModelAccess access,
                       DataModelList? metadata)
        {
            Index = index;
            Value = value;
            Key = key;
            Access = access;
            Metadata = metadata;
        }

        internal Entry(int index) : this() => Index = index;

        internal Entry(DataModelList? metadata) : this() => Metadata = metadata;

        public int Index { get; }

        public string? Key { get; }

        public DataModelAccess Access { get; }

        public DataModelList? Metadata { get; }

        public DataModelValue Value { get; }

    #region Interface IEquatable<Entry>

        public bool Equals(Entry other) => Index == other.Index && Key == other.Key && Access == other.Access && Equals(Metadata, other.Metadata) && Value.Equals(other.Value);

    #endregion

        public override bool Equals(object? obj) => obj is Entry other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Index, Key, Access, Metadata, Value);

        public static bool operator ==(Entry left, Entry right) => left.Equals(right);

        public static bool operator !=(Entry left, Entry right) => !left.Equals(right);
    }

    [Serializable]
    public readonly struct KeyValue : IEquatable<KeyValue>
    {
        internal KeyValue(string? key, in DataModelValue value)
        {
            Key = key;
            Value = value;
        }

        public string? Key { get; }

        public DataModelValue Value { get; }

    #region Interface IEquatable<KeyValue>

        public bool Equals(KeyValue other) => Key == other.Key && Value.Equals(other.Value);

    #endregion

        public override bool Equals(object? obj) => obj is KeyValue other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Key, Value);

        public static bool operator ==(KeyValue left, KeyValue right) => left.Equals(right);

        public static bool operator !=(KeyValue left, KeyValue right) => !left.Equals(right);

        public void Deconstruct(out string? key, out DataModelValue value)
        {
            key = Key;
            value = Value;
        }
    }

    private struct Args
    {
        public AdapterBase Adapter;

        public HashKey HashKey;

        public int Index;

        public string? Key;

        public KeyMetaValue[] KeyMetaValues;

        public HashKeyValue[] KeyValues;

        public Meta Meta;

        public MetaValue[] MetaValues;

        public int StoredCount;

        public DataModelValue Value;

        public DataModelValue[] Values;
    }

    [Serializable]
    private readonly struct KeyMetaValue(in HashKey hashKey, in Meta meta, in DataModelValue value)
    {
        public readonly HashKey HashKey = hashKey;

        public readonly Meta Meta = meta;

        public readonly DataModelValue Value = value;
    }

    [Serializable]
    private readonly struct MetaValue(in Meta meta, in DataModelValue value)
    {
        public readonly Meta Meta = meta;

        public readonly DataModelValue Value = value;
    }

    [Serializable]
    private readonly struct HashKeyValue(in HashKey hashKey, in DataModelValue value)
    {
        public readonly HashKey HashKey = hashKey;

        public readonly DataModelValue Value = value;
    }

    [Serializable]
    private readonly struct HashKey(int hash, string? key)
    {
        public readonly int Hash = hash;

        public readonly string? Key = key;
    }

    [Serializable]
    private readonly struct Meta(DataModelAccess access, DataModelList? metadata)
    {
        public readonly DataModelAccess Access = access;

        public readonly DataModelList? Metadata = metadata;
    }
}