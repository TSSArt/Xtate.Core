// Copyright © 2019-2024 Sergii Artemenko
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

using Xtate.Persistence;

namespace Xtate.Core;

public class IncomingEvent : IIncomingEvent, IStoreSupport, IAncestorProvider
{
    public IncomingEvent() { }

	public IncomingEvent(IIncomingEvent incomingEvent)
	{
		SendId = incomingEvent.SendId;
		Name = incomingEvent.Name;
		Type = incomingEvent.Type;
		Origin = incomingEvent.Origin;
		OriginType = incomingEvent.OriginType;
		InvokeId = incomingEvent.InvokeId;
		Data = incomingEvent.Data;
	}

	public IncomingEvent(IOutgoingEvent outgoingEvent)
	{
		SendId = outgoingEvent.SendId;
		Name = outgoingEvent.Name;
		Data = outgoingEvent.Data;
	}

	public IncomingEvent(in Bucket bucket)
	{
		ValidateTypeInfo(bucket);

		Name = bucket.GetEventName(Key.Name);
		Type = bucket.GetEnum(Key.Type).As<EventType>();
		SendId = bucket.GetSendId(Key.SendId);
		Origin = bucket.GetFullUri(Key.Origin);
		OriginType = bucket.GetFullUri(Key.OriginType);
		InvokeId = bucket.GetInvokeId(Key.InvokeUniqueId);
		Data = bucket.GetDataModelValue(Key.Data);
	}

	protected virtual TypeInfo TypeInfo => TypeInfo.EventObject;

#region Interface IAncestorProvider

	public object? Ancestor { get; init; }

#endregion

#region Interface IIncomingEvent

	public FullUri? OriginType { get; init; }

	public InvokeId? InvokeId { get; init; }

	public EventName Name { get; init; }

	public SendId? SendId { get; init; }

	public EventType Type { get; init; }

    public DataModelValue Data
    {
        get;
        init => field = value.AsConstant();
    }

    public FullUri? Origin { get; init; }

#endregion

#region Interface IStoreSupport

	public virtual void Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo);
		bucket.AddEventName(Key.Name, Name);
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
}