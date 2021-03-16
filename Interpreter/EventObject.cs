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
using System.Collections.Immutable;
using Xtate.Persistence;

namespace Xtate.Core
{
	internal class EventObject : IEvent, IStoreSupport, IAncestorProvider
	{
		private readonly DataModelValue _data;
		private          Uri?           _origin;

		public EventObject() { }

		protected EventObject(IEvent evt)
		{
			if (evt is null) throw new ArgumentNullException(nameof(evt));

			SendId = evt.SendId;
			NameParts = evt.NameParts;
			Type = evt.Type;
			Origin = evt.Origin;
			OriginType = evt.OriginType;
			InvokeId = evt.InvokeId;
			Data = evt.Data;
		}

		public EventObject(IOutgoingEvent outgoingEvent)
		{
			if (outgoingEvent is null) throw new ArgumentNullException(nameof(outgoingEvent));

			SendId = outgoingEvent.SendId;
			NameParts = outgoingEvent.NameParts;
			Data = outgoingEvent.Data;
		}

		public EventObject(in Bucket bucket)
		{
			ValidateTypeInfo(bucket);

			NameParts = bucket.GetString(Key.Name) is { Length: > 0 } name ? EventName.ToParts(name) : default;
			Type = bucket.GetEnum(Key.Type).As<EventType>();
			SendId = bucket.GetSendId(Key.SendId);
			Origin = bucket.GetUri(Key.Origin);
			OriginType = bucket.GetUri(Key.OriginType);
			InvokeId = bucket.GetInvokeId(Key.InvokeUniqueId);
			Data = bucket.GetDataModelValue(Key.Data);
		}

		protected virtual TypeInfo TypeInfo => TypeInfo.EventObject;

	#region Interface IAncestorProvider

		public object? Ancestor { get; init; }

	#endregion

	#region Interface IEvent

		public Uri? OriginType { get; init; }

		public InvokeId? InvokeId { get; init; }

		public ImmutableArray<IIdentifier> NameParts { get; init; }

		public SendId? SendId { get; init; }

		public EventType Type { get; init; }

		public DataModelValue Data
		{
			get => _data;
			init => _data = value.AsConstant();
		}

		public Uri? Origin
		{
			get => _origin ??= CreateOrigin();
			init => _origin = value;
		}

	#endregion

	#region Interface IStoreSupport

		public virtual void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo);
			bucket.Add(Key.Name, EventName.ToName(NameParts));
			bucket.Add(Key.Type, Type);
			bucket.AddId(Key.SendId, SendId);
			bucket.Add(Key.Origin, Origin);
			bucket.Add(Key.OriginType, OriginType);
			bucket.AddId(Key.InvokeId, InvokeId);
			bucket.AddDataModelValue(Key.Data, Data);
		}

	#endregion

		private void ValidateTypeInfo(in Bucket bucket)
		{
			if (!bucket.TryGet(Key.TypeInfo, out TypeInfo storedTypeInfo) || storedTypeInfo != TypeInfo)
			{
				throw new ArgumentException(Resources.Exception_InvalidTypeInfoValue);
			}
		}

		protected virtual Uri? CreateOrigin() => default;
	}
}