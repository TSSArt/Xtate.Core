// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.Ancestor.Extensions;
using Xtate.DataTypes;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Persistence.Extensions;

public static class BucketExtensions
{
	extension(in Bucket bucket)
	{
		public void AddEntity<TKey, TValue>(TKey key, TValue? entity) where TKey : notnull where TValue : class
		{
			entity?.UseAncestor.As<IStoreSupport>().Store(bucket.Nested(key));
		}

		public void AddEntityList<TKey, TValue>(TKey key, ImmutableArray<TValue> array) where TKey : notnull where TValue : class
		{
			if (array.IsDefaultOrEmpty)
			{
				return;
			}

			bucket.Add(key, array.Length);

			var listStorage = bucket.Nested(key);

			for (var i = 0; i < array.Length; i ++)
			{
				array[i].UseAncestor.As<IStoreSupport>().Store(listStorage.Nested(i));
			}
		}

		public ImmutableArray<TValue> RestoreList<TKey, TValue>(TKey key, Func<Bucket, TValue?> factory) where TKey : notnull where TValue : class
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

		public void AddId<TKey>(TKey key, SessionId? sessionId) where TKey : notnull
		{
			if (sessionId is not null)
			{
				bucket.Add(key, sessionId.Value);
			}
		}

		public void AddId<TKey>(TKey key, SendId? sendId) where TKey : notnull
		{
			if (sendId is not null)
			{
				bucket.Add(key, sendId.Value);
			}
		}

		public void AddId<TKey>(TKey key, InvokeId? invokeId) where TKey : notnull
		{
			switch (invokeId)
			{
				case null:
					break;

				case UniqueInvokeId uniqueInvokeId:
					bucket.Add(key, uniqueInvokeId.Value);

					break;

				default:
					bucket.Add(key, invokeId.Value);
					bucket.Nested(key).Add(key: 1, invokeId.UniqueId.Value);

					break;
			}
		}

		public void AddEventName<TKey>(TKey key, EventName eventName) where TKey : notnull
		{
			if (!eventName.IsDefault)
			{
				bucket.Add(key, eventName.ToString());
			}
		}

		public EnumGetter<TKey> GetEnum<TKey>(TKey key) where TKey : notnull => new(bucket, key);

		public int GetInt32<TKey>(TKey key) where TKey : notnull => bucket.TryGet(key, out int value) ? value : throw new KeyNotFoundException(Res.Format(Resources.Exception_KeyNotFound, key));

		public bool GetBoolean<TKey>(TKey key) where TKey : notnull => bucket.TryGet(key, out bool value) ? value : throw new KeyNotFoundException(Res.Format(Resources.Exception_KeyNotFound, key));

		public string? GetString<TKey>(TKey key) where TKey : notnull => bucket.TryGet(key, out string? value) ? value : null;

		public SessionId? GetSessionId<TKey>(TKey key) where TKey : notnull => bucket.TryGet(key, out string? value) ? SessionId.FromString(value) : null;

		public SendId? GetSendId<TKey>(TKey key) where TKey : notnull => bucket.TryGet(key, out string? value) ? SendId.FromString(value) : null;

		public InvokeId? GetInvokeId<TKey>(TKey key) where TKey : notnull
		{
			bucket.TryGet(key, out string? invokeId);
			bucket.Nested(key).TryGet(key: 1, out string? uniqueId);

			return invokeId is not null ? uniqueId is not null ? InvokeId.FromString(invokeId, uniqueId) : InvokeId.FromString(invokeId) : null;
		}

		public Uri? GetUri<TKey>(TKey key) where TKey : notnull => bucket.TryGet(key, out Uri? value) ? value : null;

		public FullUri? GetFullUri<TKey>(TKey key) where TKey : notnull => bucket.TryGet(key, out FullUri? value) ? value : null;

		public EventName GetEventName<TKey>(TKey key) where TKey : notnull => bucket.TryGet(key, out string? value) ? EventName.FromString(value) : default;

		public void AddServiceId<TKey>(TKey key, ServiceId? serviceId) where TKey : notnull
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
			}
		}

		public ServiceId? GetServiceId<TKey>(TKey key) where TKey : notnull => bucket.TryGetServiceId(key, out var serviceId) ? serviceId : null;

		public bool TryGetServiceId<TKey>(TKey key, [NotNullWhen(true)] out ServiceId? serviceId) where TKey : notnull
		{
			var serviceBucket = bucket.Nested(key);

			serviceId = serviceBucket.GetSessionId(Key.SessionId)
						?? serviceBucket.GetInvokeId(Key.InvokeId)
						?? (ServiceId?) null;

			return serviceId is not null;
		}

		public DataModelValue GetDataModelValue<TKey>(TKey key) where TKey : notnull
		{
			var valRefBucket = bucket.Nested(key);

			using var tracker = new DataModelReferenceTracker(valRefBucket.Nested(Key.DataReferences));

			return valRefBucket.GetDataModelValue(tracker, baseValue: default);
		}

		public DataModelValue GetDataModelValue(DataModelReferenceTracker tracker, in DataModelValue baseValue)
		{
			bucket.TryGet(Key.Type, out DataModelValueType type);

			switch (type)
			{
				case DataModelValueType.Undefined:                                                          return default;
				case DataModelValueType.Null:                                                               return DataModelValue.Null;
				case DataModelValueType.String when bucket.TryGet(Key.Item, out string? value):             return value;
				case DataModelValueType.Number when bucket.TryGet(Key.Item, out DataModelNumber value):     return value;
				case DataModelValueType.DateTime when bucket.TryGet(Key.Item, out DataModelDateTime value): return value;
				case DataModelValueType.Boolean when bucket.TryGet(Key.Item, out bool value):               return value;

				case DataModelValueType.List when bucket.TryGet(Key.RefId, out int refId):
					var list = baseValue.Type == DataModelValueType.List ? baseValue.AsList() : null;

					return DataModelValue.FromObject(tracker.GetValue(refId, type, list));

				default: throw Infra.Unmatched(type);
			}
		}

		public void AddDataModelValue<TKey>(TKey key, in DataModelValue item) where TKey : notnull
		{
			if (!item.IsUndefined())
			{
				var valRefBucket = bucket.Nested(key);
				using var tracker = new DataModelReferenceTracker(valRefBucket.Nested(Key.DataReferences));
				valRefBucket.SetDataModelValue(tracker, item);
			}
		}

		public void SetDataModelValue(DataModelReferenceTracker tracker, in DataModelValue item)
		{
			var type = item.Type;

			if (type != DataModelValueType.Undefined)
			{
				bucket.Add(Key.Type, type);
			}

			switch (type)
			{
				case DataModelValueType.Undefined:
				case DataModelValueType.Null:
					break;

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
					throw Infra.Unmatched(type);
			}
		}
	}

	public readonly struct EnumGetter<TKey>(Bucket bucket, TKey key) where TKey : notnull
	{
		public TEnum As<TEnum>() where TEnum : struct, Enum => bucket.TryGet(key, out TEnum value) ? value : throw new KeyNotFoundException(Res.Format(Resources.Exception_KeyNotFound, key));
	}
}