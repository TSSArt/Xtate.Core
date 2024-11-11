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

public class RouterEvent : IncomingEvent, IRouterEvent
{
	public RouterEvent(ServiceId senderServiceId,
					 ServiceId? targetServiceId,
					 FullUri? originType,
					 FullUri? origin,
					 IOutgoingEvent outgoingEvent) : base(outgoingEvent)
	{
		SenderServiceId = senderServiceId;
		TargetServiceId = targetServiceId;
		OriginType = originType;
		Origin = origin;
		Type = EventType.External;
		DelayMs = outgoingEvent.DelayMs;
		TargetType = outgoingEvent.Type;
		Target = outgoingEvent.Target;
		InvokeId = senderServiceId as InvokeId;
	}

	protected RouterEvent(IRouterEvent routerEvent) : base(routerEvent)
	{
		SenderServiceId = routerEvent.SenderServiceId;
		TargetServiceId = routerEvent.TargetServiceId;
		IoProcessorData = routerEvent.IoProcessorData;
		TargetType = routerEvent.TargetType;
		Target = routerEvent.Target;
		DelayMs = routerEvent.DelayMs;
	}

	protected RouterEvent(in Bucket bucket) : base(bucket)
	{
		if (bucket.TryGetServiceId(Key.SenderServiceId, out var senderServiceId))
		{
			SenderServiceId = senderServiceId;
		}
		else
		{
			Infra.Fail();
		}

		if (bucket.TryGetServiceId(Key.TargetServiceId, out var targetServiceId))
		{
			TargetServiceId = targetServiceId;
		}

		if (bucket.GetDataModelValue(Key.RouterEventData) is { Type: DataModelValueType.List } ioProcessorData)
		{
			IoProcessorData = ioProcessorData.AsList();
		}

		if (bucket.TryGet(Key.DelayMs, out int delayMs))
		{
			DelayMs = delayMs;
		}

		if (bucket.TryGet(Key.TargetType, out FullUri? targetType))
		{
			TargetType = targetType;
		}

		if (bucket.TryGet(Key.Target, out FullUri? target))
		{
			Target = target;
		}
	}

	protected override TypeInfo TypeInfo => TypeInfo.RouterEvent;

#region Interface IRouterEvent

	public int DelayMs { get; protected init; }

	public ServiceId SenderServiceId { get; }

	public ServiceId? TargetServiceId { get; }

	public DataModelList? IoProcessorData { get; }

	public FullUri? TargetType { get; }

	public FullUri? Target { get; }

#endregion

	public override void Store(Bucket bucket)
	{
		base.Store(bucket);

		bucket.AddServiceId(Key.SenderServiceId, SenderServiceId);

		if (TargetServiceId is not null)
		{
			bucket.AddServiceId(Key.TargetServiceId, TargetServiceId);
		}

		if (IoProcessorData is not null)
		{
			bucket.AddDataModelValue(Key.RouterEventData, IoProcessorData);
		}

		if (DelayMs > 0)
		{
			bucket.Add(Key.DelayMs, DelayMs);
		}

		if (TargetType is not null)
		{
			bucket.Add(Key.TargetType, TargetType);
		}

		if (Target is not null)
		{
			bucket.Add(Key.TargetType, Target);
		}
	}
}