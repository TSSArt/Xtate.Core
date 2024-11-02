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

using Xtate.IoProcessor;
using Xtate.Persistence;

namespace Xtate.Core;

public class HostEvent : IncomingEvent, IHostEvent
{
	private readonly IIoProcessor? _ioProcessor;

	public HostEvent(IIoProcessor ioProcessor,
					 ServiceId senderServiceId,
					 ServiceId? targetServiceId,
					 IOutgoingEvent outgoingEvent) : base(outgoingEvent)
	{
		if (outgoingEvent is null) throw new ArgumentNullException(nameof(outgoingEvent));

		_ioProcessor = ioProcessor ?? throw new ArgumentNullException(nameof(ioProcessor));
		SenderServiceId = senderServiceId ?? throw new ArgumentNullException(nameof(senderServiceId));
		TargetServiceId = targetServiceId;
		Type = EventType.External;
		DelayMs = outgoingEvent.DelayMs;
		InvokeId = senderServiceId as InvokeId;
		OriginType = ioProcessor.Id;
	}

	protected HostEvent(IHostEvent hostEvent) : base(hostEvent)
	{
		SenderServiceId = hostEvent.SenderServiceId;
		TargetServiceId = hostEvent.TargetServiceId;
		IoProcessorData = hostEvent.IoProcessorData;
		DelayMs = hostEvent.DelayMs;
	}

	protected HostEvent(in Bucket bucket) : base(bucket)
	{
		if (bucket.TryGetServiceId(Key.Sender, out var senderServiceId))
		{
			SenderServiceId = senderServiceId;
		}
		else
		{
			Infra.Fail();
		}

		if (bucket.TryGetServiceId(Key.Target, out var targetServiceId))
		{
			TargetServiceId = targetServiceId;
		}

		if (bucket.GetDataModelValue(Key.HostEventData) is { Type: DataModelValueType.List } ioProcessorData)
		{
			IoProcessorData = ioProcessorData.AsList();
		}

		if (bucket.TryGet(Key.DelayMs, out int delayMs))
		{
			DelayMs = delayMs;
		}
	}

	protected override TypeInfo TypeInfo => TypeInfo.HostEvent;

#region Interface IHostEvent

	public int DelayMs { get; protected init; }

	public ServiceId SenderServiceId { get; }

	public ServiceId? TargetServiceId { get; }

	public DataModelList? IoProcessorData { get; }

#endregion

	public override void Store(Bucket bucket)
	{
		base.Store(bucket);

		bucket.AddServiceId(Key.Sender, SenderServiceId);

		if (TargetServiceId is not null)
		{
			bucket.AddServiceId(Key.Target, TargetServiceId);
		}

		if (IoProcessorData is not null)
		{
			bucket.AddDataModelValue(Key.HostEventData, IoProcessorData);
		}

		if (DelayMs > 0)
		{
			bucket.Add(Key.DelayMs, DelayMs);
		}
	}

	protected override FullUri? CreateOrigin() => _ioProcessor?.GetTarget(SenderServiceId);
}