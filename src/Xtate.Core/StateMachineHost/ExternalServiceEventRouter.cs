﻿// Copyright © 2019-2024 Sergii Artemenko
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

using Xtate.ExternalService;
using Xtate.IoProcessor;

namespace Xtate.Core;

public class ExternalServiceEventRouter : IEventRouter
{
	private readonly MiniDictionary<InvokeId, Func<IIncomingEvent, ValueTask>> _handlers = new(InvokeId.InvokeUniqueIdComparer);

	public required ServiceList<IExternalServiceProvider> ExternalServiceProviders { private get; [UsedImplicitly] init; }

	public required IStateMachineSessionId StateMachineSessionId { private get; [UsedImplicitly] init; }

#region Interface IEventRouter

	public bool CanHandle(FullUri? type)
	{
		if (type is null)
		{
			return false;
		}

		foreach (var externalServiceProvider in ExternalServiceProviders)
		{
			if (externalServiceProvider.TryGetActivator(type) is not null)
			{
				return true;
			}
		}

		return false;
	}

	public bool IsInternalTarget(FullUri? target) => default;

	public ValueTask<IRouterEvent> GetRouterEvent(IOutgoingEvent outgoingEvent) =>
		new(new RouterEvent(StateMachineSessionId.SessionId, GetInvokeId(outgoingEvent.Target), Const.ScxmlIoProcessorId, Const.ParentTarget, outgoingEvent));

	public ValueTask Dispatch(IRouterEvent routerEvent) => Dispatch((InvokeId) routerEvent.TargetServiceId!, routerEvent);

#endregion

	public ValueTask Dispatch(InvokeId invokeId, IIncomingEvent incomingEvent)
	{
		if (!_handlers.TryGetValue(invokeId, out var handler))
		{
			return default;
		}

		if(incomingEvent is not IncomingEvent)
		{
			incomingEvent = new IncomingEvent(incomingEvent);
		}

		return handler(incomingEvent);
	}

	internal void Subscribe(InvokeId invokeId, Func<IIncomingEvent, ValueTask> handler)
	{
		var added = _handlers.TryAdd(invokeId, handler);

		Infra.Assert(added);
	}

	internal void Unsubscribe(InvokeId invokeId)
	{
		var removed = _handlers.TryRemove(invokeId, out _);

		Infra.Assert(removed);
	}

	private static InvokeId? GetInvokeId(FullUri? target)
	{
		var str = target?.ToString();

		return str?.StartsWith(Const.ScxmlIoProcessorInvokeIdPrefix) == true ? InvokeId.FromString(str[Const.ScxmlIoProcessorInvokeIdPrefix.Length..]) : default;
	}
}