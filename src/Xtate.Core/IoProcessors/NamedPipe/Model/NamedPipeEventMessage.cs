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

using Xtate.DataModel;
using Xtate.Interpreter;
using Xtate.Persistence;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.IoProcessors.NamedPipe;

public class NamedPipeEventMessage : IncomingEvent, IStoreSupport
{
	public NamedPipeEventMessage(DateTime timestamp, ServiceId targetServiceId, IIncomingEvent incomingEvent) : base(incomingEvent)
	{
		Timestamp = timestamp;
		TargetServiceId = targetServiceId;
	}

	void IStoreSupport.Store(Bucket bucket)
	{
		bucket.Add(Key.Id, Timestamp);
		bucket.AddServiceId(Key.Target, TargetServiceId);
		bucket.Add(Key.TypeInfo, TypeInfo.Message);
		bucket.AddEventName(Key.Name, Name);
		bucket.Add(Key.Type, Type);
		bucket.AddId(Key.SendId, SendId);
		bucket.Add(Key.Origin, Origin);
		bucket.Add(Key.OriginType, OriginType);
		bucket.AddId(Key.InvokeId, InvokeId);
		bucket.AddDataModelValue(Key.Data, Data);
	}

	public NamedPipeEventMessage(in Bucket bucket) 
	{
		if (!bucket.TryGet(Key.TypeInfo, out TypeInfo storedTypeInfo)
			|| storedTypeInfo != TypeInfo.Message
			|| !bucket.TryGet(Key.Id, out DateTime timestamp)
			|| !bucket.TryGetServiceId(Key.Target, out var serviceId)
			|| serviceId is null)
		{
			throw new ArgumentException(Resources.Exception_InvalidTypeInfoValue);
		}

		Timestamp = timestamp;
		TargetServiceId = serviceId;
		Name = bucket.GetEventName(Key.Name);
		Type = bucket.GetEnum(Key.Type).As<EventType>();
		SendId = bucket.GetSendId(Key.SendId);
		Origin = bucket.GetFullUri(Key.Origin);
		OriginType = bucket.GetFullUri(Key.OriginType);
		InvokeId = bucket.GetInvokeId(Key.InvokeUniqueId);
		Data = bucket.GetDataModelValue(Key.Data);
	}

	public DateTime Timestamp { get; }

	public ServiceId TargetServiceId { get; }
}