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

using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.StateMachine;

namespace Xtate.StateMachineHost;

public class RouterEvent : IncomingEvent, IRouterEvent
{
	protected RouterEvent() { }

	public RouterEvent(IOutgoingEvent outgoingEvent) : base(outgoingEvent)
	{
		Type = EventType.External;
		DelayMs = outgoingEvent.DelayMs;
		TargetType = outgoingEvent.Type;
		Target = outgoingEvent.Target;
	}

	protected RouterEvent(IRouterEvent routerEvent) : base(routerEvent)
	{
		SenderServiceId = routerEvent.SenderServiceId;
		InvokeId = routerEvent.InvokeId;
		TargetServiceId = routerEvent.TargetServiceId;
		IoProcessorData = routerEvent.IoProcessorData;
		TargetType = routerEvent.TargetType;
		Target = routerEvent.Target;
		DelayMs = routerEvent.DelayMs;
	}

#region Interface IRouterEvent

	public int DelayMs { get; init; }

	public ServiceId SenderServiceId
	{
		get;
		init
		{
			field = value;
			InvokeId = value as InvokeId;
		}
	} = null!;

	public ServiceId? TargetServiceId { get; init; }

	public DataModelList? IoProcessorData { get; init; }

	public FullUri? TargetType { get; init; }

	public FullUri? Target { get; init; }

#endregion
}
