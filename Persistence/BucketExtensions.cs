#region Copyright © 2019-2023 Sergii Artemenko

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

namespace Xtate.Persistence;


internal static class BucketExtensions
{
	public static void AddEntity<TKey, TValue>(this in Bucket bucket, TKey key, TValue? entity) where TKey : notnull where TValue : class
	{
		entity?.As<IStoreSupport>().Store(bucket.Nested(key));
	}

	public static void AddEntityList<TKey, TValue>(this in Bucket bucket, TKey key, ImmutableArray<TValue> array) where TKey : notnull where TValue : class
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

	public static ImmutableArray<TValue> RestoreList<TKey, TValue>(this in Bucket bucket, TKey key, Func<Bucket, TValue?> factory) where TKey : notnull where TValue : class
	{
		if (!bucket.TryGet(key, out int length))
		{
			return default;
		}

		var itemsBucket = bucket.Nested(key);

		var builder = ImmutableArray.CreateBuilder<TValue>(length);

		for (var i = 0; i < length; i ++)
		{
			var item = factory(itemsBucket.Nested(i)) ?? throw new PersistenceException(Resources.Exception_ItemCantBeNull);

			builder.Add(item);
		}

		return builder.MoveToImmutable();
	}

	public static void AddId<TKey>(this in Bucket bucket, TKey key, SessionId? sessionId) where TKey : notnull
	{
		if (sessionId is not null)
		{
			bucket.Add(key, sessionId.Value);
		}
	}

	public static void AddId<TKey>(this in Bucket bucket, TKey key, SendId? sendId) where TKey : notnull
	{
		if (sendId is not null)
		{
			bucket.Add(key, sendId.Value);
		}
	}

	public static void AddId<TKey>(this in Bucket bucket, TKey key, InvokeId? invokeId) where TKey : notnull
	{
		if (invokeId is not null)
		{
			bucket.Add(key, invokeId.Value);
			bucket.Nested(key).Add(key: 1, invokeId.InvokeUniqueIdValue);
		}
	}

	public static void AddId<TKey>(this in Bucket bucket, TKey key, UriId? uriId) where TKey : notnull
	{
		if (uriId is not null)
		{
			bucket.Add(key, uriId.Uri);
		}
	}

	public static EnumGetter<TKey> GetEnum<TKey>(this in Bucket bucket, TKey key) where TKey : notnull => new(bucket, key);

	public static int GetInt32<TKey>(this in Bucket bucket, TKey key) where TKey : notnull =>
		bucket.TryGet(key, out int value) ? value : throw new KeyNotFoundException(Res.Format(Resources.Exception_KeyNotFound, key));

	public static bool GetBoolean<TKey>(this in Bucket bucket, TKey key) where TKey : notnull =>
		bucket.TryGet(key, out bool value) ? value : throw new KeyNotFoundException(Res.Format(Resources.Exception_KeyNotFound, key));

	public static string? GetString<TKey>(this in Bucket bucket, TKey key) where TKey : notnull => bucket.TryGet(key, out string? value) ? value : null;

	public static SessionId? GetSessionId<TKey>(this in Bucket bucket, TKey key) where TKey : notnull => bucket.TryGet(key, out string? value) ? SessionId.FromString(value) : null;

	public static SendId? GetSendId<TKey>(this in Bucket bucket, TKey key) where TKey : notnull => bucket.TryGet(key, out string? value) ? SendId.FromString(value) : null;

	public static UriId? GetUriId<TKey>(this in Bucket bucket, TKey key) where TKey : notnull => bucket.TryGet(key, out Uri? value) ? UriId.FromUri(value) : null;

	public static InvokeId? GetInvokeId<TKey>(this in Bucket bucket, TKey key) where TKey : notnull
	{
		bucket.TryGet(key, out string? invokeId);
		bucket.Nested(key).TryGet(key: 1, out string? invokeUniqueId);

		if (invokeId is null || invokeUniqueId is null)
		{
			return null;
		}

		return InvokeId.FromString(invokeId, invokeUniqueId);
	}

	public static Uri? GetUri<TKey>(this in Bucket bucket, TKey key) where TKey : notnull => bucket.TryGet(key, out Uri? value) ? value : null;

	public static void AddServiceId<TKey>(this in Bucket bucket, TKey key, ServiceId? serviceId) where TKey : notnull
	{
		var serviceBucket = bucket.Nested(key);

		switch (serviceId)
		{
			case SessionId sessionId:
				serviceBucket.AddId(Key.SessionId, sessionId);
				break;

			case InvokeId invokeId:
				serviceBucket.AddId(Key.InvokeId, invokeId);
				break;

			case UriId uriId:
				serviceBucket.AddId(Key.UriId, uriId);
				break;
		}
	}

	public static bool TryGetServiceId<TKey>(this in Bucket bucket, TKey key, [NotNullWhen(true)] out ServiceId? serviceId) where TKey : notnull
	{
		var serviceBucket = bucket.Nested(key);

		serviceId = serviceBucket.GetSessionId(Key.SessionId)
					?? serviceBucket.GetInvokeId(Key.InvokeId)
					?? serviceBucket.GetUriId(Key.UriId)
					?? (ServiceId?) default;

		return serviceId is not null;
	}

	public static DataModelValue GetDataModelValue<TKey>(this in Bucket bucket, TKey key) where TKey : notnull
	{
		var valRefBucket = bucket.Nested(key);

		using var tracker = new DataModelReferenceTracker(valRefBucket.Nested(Key.DataReferences));

		return valRefBucket.GetDataModelValue(tracker, baseValue: default);
	}

	public static DataModelValue GetDataModelValue(this in Bucket bucket, DataModelReferenceTracker tracker, in DataModelValue baseValue)
	{
		if (tracker is null) throw new ArgumentNullException(nameof(tracker));

		bucket.TryGet(Key.Type, out DataModelValueType type);

		switch (type)
		{
			case DataModelValueType.Undefined:                                                          return default;
			case DataModelValueType.Null:                                                               return DataModelValue.Null;
			case DataModelValueType.String when bucket.TryGet(Key.Item, out string? value):             return value;
			case DataModelValueType.Number when bucket.TryGet(Key.Item, out double value):              return value;
			case DataModelValueType.DateTime when bucket.TryGet(Key.Item, out DataModelDateTime value): return value;
			case DataModelValueType.Boolean when bucket.TryGet(Key.Item, out bool value):               return value;

			case DataModelValueType.List when bucket.TryGet(Key.RefId, out int refId):
				var list = baseValue.Type == DataModelValueType.List ? baseValue.AsList() : null;
				return DataModelValue.FromObject(tracker.GetValue(refId, type, list));

			default: return Infra.Unexpected<DataModelValue>(type);
		}
	}

	public static void AddDataModelValue<TKey>(this in Bucket bucket, TKey key, in DataModelValue item) where TKey : notnull
	{
		if (!item.IsUndefined())
		{
			var valRefBucket = bucket.Nested(key);
			using var tracker = new DataModelReferenceTracker(valRefBucket.Nested(Key.DataReferences));
			valRefBucket.SetDataModelValue(tracker, item);
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
			case DataModelValueType.Null:      break;

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
				Infra.Unexpected(type);
				break;
		}
	}

	public readonly struct EnumGetter<TKey>(Bucket bucket, TKey key) where TKey : notnull
	{
		public TEnum As<TEnum>() where TEnum : struct, Enum => bucket.TryGet(key, out TEnum value) ? value : throw new KeyNotFoundException(Res.Format(Resources.Exception_KeyNotFound, key));
	}
}