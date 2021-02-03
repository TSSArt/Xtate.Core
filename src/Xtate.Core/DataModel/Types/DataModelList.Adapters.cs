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
using Xtate.Core;

namespace Xtate
{
	public partial class DataModelList
	{
		private abstract class AdapterBase
		{
			public abstract ref readonly DataModelValue  GetValueByIndex(ref Args args);
			public abstract              DataModelAccess GetAccessByIndex(ref Args args);
			public abstract              void            GetEntryByIndex(ref Args args, out Entry entry);
			public abstract              void            ReadToArgsByIndex(ref Args args);
			public abstract              Array           CreateArray(ref Args args, int size);
			public abstract              bool            IsAccessAvailable();
			public abstract              bool            IsKeyAvailable();
			public abstract              void            AssignItemByIndex(ref Args args);
			public abstract              Array           EnsureArray(ref Args args, int size);

			protected static HashKeyValue[] ValueToKeyValue(ref Args args, int size)
			{
				args.KeyValues = new HashKeyValue[size];

				for (var i = 0; i < args.StoredCount; i ++)
				{
					args.KeyValues[i] = new HashKeyValue(hashKey: default, args.Values[i]);
				}

				args.Values = null!;
				args.Adapter = KeyValueAdapterInstance;

				return args.KeyValues;
			}

			protected static MetaValue[] ValueToMetaValue(ref Args args, int size)
			{
				args.MetaValues = new MetaValue[size];

				for (var i = 0; i < args.StoredCount; i ++)
				{
					args.MetaValues[i] = new MetaValue(meta: default, args.Values[i]);
				}

				args.Values = null!;
				args.Adapter = MetaValueAdapterInstance;

				return args.MetaValues;
			}

			protected static KeyMetaValue[] ValueToKeyMetaValue(ref Args args, int size)
			{
				args.KeyMetaValues = new KeyMetaValue[size];

				for (var i = 0; i < args.StoredCount; i ++)
				{
					args.KeyMetaValues[i] = new KeyMetaValue(hashKey: default, meta: default, args.Values[i]);
				}

				args.Values = null!;
				args.Adapter = KeyMetaValueAdapterInstance;

				return args.KeyMetaValues;
			}

			protected static KeyMetaValue[] KeyValueToKeyMetaValue(ref Args args, int size)
			{
				args.KeyMetaValues = new KeyMetaValue[size];

				for (var i = 0; i < args.StoredCount; i ++)
				{
					args.KeyMetaValues[i] = new KeyMetaValue(args.KeyValues[i].HashKey, meta: default, args.KeyValues[i].Value);
				}

				args.KeyValues = null!;
				args.Adapter = KeyMetaValueAdapterInstance;

				return args.KeyMetaValues;
			}

			protected static KeyMetaValue[] MetaValueToKeyMetaValue(ref Args args, int size)
			{
				args.KeyMetaValues = new KeyMetaValue[size];

				for (var i = 0; i < args.StoredCount; i ++)
				{
					args.KeyMetaValues[i] = new KeyMetaValue(hashKey: default, args.MetaValues[i].Meta, args.MetaValues[i].Value);
				}

				args.MetaValues = null!;
				args.Adapter = KeyMetaValueAdapterInstance;

				return args.KeyMetaValues;
			}
		}

		private sealed class ValueAdapter : AdapterBase
		{
			public override Array CreateArray(ref Args args, int size) => args.Values = size > 0 ? new DataModelValue[size] : Array.Empty<DataModelValue>();

			public override void AssignItemByIndex(ref Args args) => args.Values[args.Index] = args.Value;

			public override ref readonly DataModelValue GetValueByIndex(ref Args args) => ref args.Values[args.Index];

			public override void GetEntryByIndex(ref Args args, out Entry entry) => entry = new Entry(args.Index, args.Values[args.Index]);

			public override void ReadToArgsByIndex(ref Args args) => args.Value = args.Values[args.Index];

			public override DataModelAccess GetAccessByIndex(ref Args args) => Infrastructure.Fail<DataModelAccess>();

			public override bool IsAccessAvailable() => false;

			public override bool IsKeyAvailable() => false;

			public override Array EnsureArray(ref Args args, int size)
			{
				if (args.Meta.Access != DataModelAccess.Writable || args.Meta.Metadata is not null)
				{
					return args.HashKey.Key is not null ? ValueToKeyMetaValue(ref args, size) : ValueToMetaValue(ref args, size);
				}

				if (args.HashKey.Key is not null)
				{
					return ValueToKeyValue(ref args, size);
				}

				if (size > args.Values.Length)
				{
					var array = new DataModelValue[size];
					Array.Copy(args.Values, array, args.StoredCount);
					args.Values = array;
				}

				return args.Values;
			}
		}

		private sealed class KeyValueAdapter : AdapterBase
		{
			public override Array CreateArray(ref Args args, int size) => args.KeyValues = size > 0 ? new HashKeyValue[size] : Array.Empty<HashKeyValue>();

			public override void AssignItemByIndex(ref Args args) => args.KeyValues[args.Index] = new HashKeyValue(args.HashKey, args.Value);

			public override ref readonly DataModelValue GetValueByIndex(ref Args args) => ref args.KeyValues[args.Index].Value;

			public override void GetEntryByIndex(ref Args args, out Entry entry) => entry = new Entry(args.Index, args.KeyValues[args.Index].HashKey.Key, args.KeyValues[args.Index].Value);

			public override void ReadToArgsByIndex(ref Args args)
			{
				args.Value = args.KeyValues[args.Index].Value;
				args.HashKey = args.KeyValues[args.Index].HashKey;
			}

			public override DataModelAccess GetAccessByIndex(ref Args args) => Infrastructure.Fail<DataModelAccess>();

			public override bool IsAccessAvailable() => false;

			public override bool IsKeyAvailable() => true;

			public override Array EnsureArray(ref Args args, int size)
			{
				if (args.Meta.Access != DataModelAccess.Writable || args.Meta.Metadata is not null)
				{
					return KeyValueToKeyMetaValue(ref args, size);
				}

				if (size > args.KeyValues.Length)
				{
					var array = new HashKeyValue[size];
					Array.Copy(args.KeyValues, array, args.StoredCount);
					args.KeyValues = array;
				}

				return args.KeyValues;
			}
		}

		private sealed class MetaValueAdapter : AdapterBase
		{
			public override Array CreateArray(ref Args args, int size) => args.MetaValues = size > 0 ? new MetaValue[size] : Array.Empty<MetaValue>();

			public override void AssignItemByIndex(ref Args args) => args.MetaValues[args.Index] = new MetaValue(args.Meta, args.Value);

			public override ref readonly DataModelValue GetValueByIndex(ref Args args) => ref args.MetaValues[args.Index].Value;

			public override void GetEntryByIndex(ref Args args, out Entry entry) =>
					entry = new Entry(args.Index, args.MetaValues[args.Index].Value, args.MetaValues[args.Index].Meta.Access, args.MetaValues[args.Index].Meta.Metadata);

			public override void ReadToArgsByIndex(ref Args args)
			{
				args.Value = args.MetaValues[args.Index].Value;
				args.Meta = args.MetaValues[args.Index].Meta;
			}

			public override DataModelAccess GetAccessByIndex(ref Args args) => args.MetaValues[args.Index].Meta.Access;

			public override bool IsAccessAvailable() => true;

			public override bool IsKeyAvailable() => false;

			public override Array EnsureArray(ref Args args, int size)
			{
				if (args.HashKey.Key is not null)
				{
					return MetaValueToKeyMetaValue(ref args, size);
				}

				if (size > args.MetaValues.Length)
				{
					var array = new MetaValue[size];
					Array.Copy(args.MetaValues, array, args.StoredCount);
					args.MetaValues = array;
				}

				return args.MetaValues;
			}
		}

		private sealed class KeyMetaValueAdapter : AdapterBase
		{
			public override Array CreateArray(ref Args args, int size) => args.KeyMetaValues = size > 0 ? new KeyMetaValue[size] : Array.Empty<KeyMetaValue>();

			public override void AssignItemByIndex(ref Args args) => args.KeyMetaValues[args.Index] = new KeyMetaValue(args.HashKey, args.Meta, args.Value);

			public override ref readonly DataModelValue GetValueByIndex(ref Args args) => ref args.KeyMetaValues[args.Index].Value;

			public override void GetEntryByIndex(ref Args args, out Entry entry) =>
					entry = new Entry(args.Index, args.KeyMetaValues[args.Index].HashKey.Key, args.KeyMetaValues[args.Index].Value,
									  args.KeyMetaValues[args.Index].Meta.Access, args.KeyMetaValues[args.Index].Meta.Metadata);

			public override void ReadToArgsByIndex(ref Args args)
			{
				args.Value = args.KeyMetaValues[args.Index].Value;
				args.HashKey = args.KeyMetaValues[args.Index].HashKey;
				args.Meta = args.KeyMetaValues[args.Index].Meta;
			}

			public override DataModelAccess GetAccessByIndex(ref Args args) => args.KeyMetaValues[args.Index].Meta.Access;

			public override bool IsAccessAvailable() => true;

			public override bool IsKeyAvailable() => true;

			public override Array EnsureArray(ref Args args, int size)
			{
				if (size > args.KeyMetaValues.Length)
				{
					var array = new KeyMetaValue[size];
					Array.Copy(args.KeyMetaValues, array, args.StoredCount);
					args.KeyMetaValues = array;
				}

				return args.KeyMetaValues;
			}
		}
	}
}