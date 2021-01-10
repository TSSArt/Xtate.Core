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
using System.Collections.Generic;
using System.Collections.Immutable;
using Xtate.Core;

namespace Xtate.Persistence
{
	internal static class BucketExtensions
	{
		public static void AddEntity<T>(this in Bucket bucket, Key key, T? entity) where T : class
		{
			entity?.As<IStoreSupport>().Store(bucket.Nested(key));
		}

		public static void AddEntityList<T>(this in Bucket bucket, Key key, ImmutableArray<T> array) where T : class
		{
			if (array.IsDefaultOrEmpty)
			{
				return;
			}

			bucket.Add(key, array.Length);

			var listStorage = bucket.Nested(key);

			for (var i = 0; i < array.Length; i ++)
			{
				array[i].As<IStoreSupport>().Store(listStorage.Nested(i));
			}
		}

		public static ImmutableArray<T> RestoreList<T>(this in Bucket bucket, Key key, Func<Bucket, T?> factory) where T : class
		{
			if (!bucket.TryGet(key, out int length))
			{
				return default;
			}

			var itemsBucket = bucket.Nested(key);

			var builder = ImmutableArray.CreateBuilder<T>(length);

			for (var i = 0; i < length; i ++)
			{
				var item = factory(itemsBucket.Nested(i));

				if (item is null)
				{
					throw new PersistenceException(Resources.Exception_ItemCantBeNull);
				}

				builder.Add(item);
			}

			return builder.MoveToImmutable();
		}

		public static void AddId(this in Bucket bucket, Key key, SessionId? sessionId)
		{
			if (sessionId is not null)
			{
				bucket.Add(key, sessionId.Value);
			}
		}

		public static void AddId(this in Bucket bucket, Key key, SendId? sendId)
		{
			if (sendId is not null)
			{
				bucket.Add(key, sendId.Value);
			}
		}

		public static void AddId(this in Bucket bucket, Key key, InvokeId? invokeId)
		{
			if (invokeId is not null)
			{
				bucket.Add(key, invokeId.Value);
				bucket.Nested(key).Add(key: 1, invokeId.InvokeUniqueIdValue);
			}
		}

		public static TEnum Get<TEnum>(this in Bucket bucket, Key key) where TEnum : struct, Enum =>
				bucket.TryGet(key, out TEnum value) ? value : throw new KeyNotFoundException(Res.Format(Resources.Exception_KeyNotFound, key));

		public static int GetInt32(this in Bucket bucket, Key key) => bucket.TryGet(key, out int value) ? value : throw new KeyNotFoundException(Res.Format(Resources.Exception_KeyNotFound, key));

		public static bool GetBoolean(this in Bucket bucket, Key key) =>
				bucket.TryGet(key, out bool value) ? value : throw new KeyNotFoundException(Res.Format(Resources.Exception_KeyNotFound, key));

		public static string? GetString(this in Bucket bucket, Key key) => bucket.TryGet(key, out string? value) ? value : null;

		public static SessionId? GetSessionId(this in Bucket bucket, Key key) => bucket.TryGet(key, out string? value) ? SessionId.FromString(value) : null;

		public static SendId? GetSendId(this in Bucket bucket, Key key) => bucket.TryGet(key, out string? value) ? SendId.FromString(value) : null;

		public static InvokeId? GetInvokeId(this in Bucket bucket, Key key)
		{
			bucket.TryGet(key, out string? invokeId);
			bucket.Nested(key).TryGet(key: 1, out string? invokeUniqueId);

			if (invokeId is null || invokeUniqueId is null)
			{
				return null;
			}

			return InvokeId.FromString(invokeId, invokeUniqueId);
		}

		public static Uri? GetUri(this in Bucket bucket, Key key) => bucket.TryGet(key, out Uri? value) ? value : null;

		public static DataModelValue GetDataModelValue(this in Bucket bucket, DataModelReferenceTracker tracker, DataModelValue baseValue)
		{
			if (tracker is null) throw new ArgumentNullException(nameof(tracker));

			bucket.TryGet(Key.Type, out DataModelValueType type);

			switch (type)
			{
				case DataModelValueType.Undefined: return default;
				case DataModelValueType.Null: return DataModelValue.Null;
				case DataModelValueType.String when bucket.TryGet(Key.Item, out string? value): return value;
				case DataModelValueType.Number when bucket.TryGet(Key.Item, out double value): return value;
				case DataModelValueType.DateTime when bucket.TryGet(Key.Item, out DataModelDateTime value): return value;
				case DataModelValueType.Boolean when bucket.TryGet(Key.Item, out bool value): return value;

				case DataModelValueType.List when bucket.TryGet(Key.RefId, out int refId):
					var list = baseValue.Type == DataModelValueType.List ? baseValue.AsList() : null;
					return DataModelValue.FromObject(tracker.GetValue(refId, type, list));

				default: return Infrastructure.UnexpectedValue<DataModelValue>(type);
			}
		}

		public static void SetDataModelValue(this in Bucket bucket, DataModelReferenceTracker tracker, in DataModelValue item)
		{
			if (tracker is null) throw new ArgumentNullException(nameof(tracker));

			var type = item.Type;
			if (type != DataModelValueType.Undefined)
			{
				bucket.Add(Key.Type, type);
			}

			switch (type)
			{
				case DataModelValueType.Undefined: break;
				case DataModelValueType.Null: break;

				case DataModelValueType.String:
					bucket.Add(Key.Item, item.AsString());
					break;

				case DataModelValueType.Number:
					bucket.Add(Key.Item, item.AsNumber());
					break;

				case DataModelValueType.DateTime:
					bucket.Add(Key.Item, item.AsDateTime());
					break;

				case DataModelValueType.Boolean:
					bucket.Add(Key.Item, item.AsBoolean());
					break;

				case DataModelValueType.List:
					bucket.Add(Key.RefId, tracker.GetRefId(item));
					break;

				default:
					Infrastructure.UnexpectedValue(type);
					break;
			}
		}
	}
}