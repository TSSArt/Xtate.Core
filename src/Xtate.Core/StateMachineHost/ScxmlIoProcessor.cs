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

using Xtate.IoProcessor;

namespace Xtate.Core;

public class ScxmlIoProcessor(IExternalServiceInvokeId? externalServiceInvokeId, IStateMachineSessionId stateMachineSessionId) : IIoProcessor, IEventRouter
{
	private readonly ServiceId _senderServiceId = (ServiceId?) externalServiceInvokeId?.InvokeId ?? stateMachineSessionId.SessionId;

	public required IParentStateMachineSessionId? ParentStateMachineSessionId { private get; [UsedImplicitly] init; }

	public required IEventDispatcher? EventDispatcher { private get; [UsedImplicitly] init; }

	public required IParentEventDispatcher? ParentEventDispatcher { private get; [UsedImplicitly] init; }

	public required DisposeToken DisposeToken { private get; [UsedImplicitly] init; }

#region Interface IEventRouter

	public bool CanHandle(FullUri? type) => type is null || type == Const.ScxmlIoProcessorId || type == Const.ScxmlIoProcessorAliasId;

	public bool IsInternalTarget(FullUri? target) => target == Const.InternalTarget || target == Const.ScxmlIoProcessorInternalTarget;

	public ValueTask<IRouterEvent> GetRouterEvent(IOutgoingEvent outgoingEvent)
	{
		var routerEvent = new RouterEvent(_senderServiceId, GetTargetServiceId(outgoingEvent.Target), Const.ScxmlIoProcessorId, GetTarget(_senderServiceId), outgoingEvent);

		return new ValueTask<IRouterEvent>(routerEvent);
	}

	public ValueTask Dispatch(IRouterEvent routerEvent)
	{
		if (DisposeToken.IsCancellationRequested)
		{
			return default;
		}

		Infra.NotNull(routerEvent.TargetServiceId);

		if (routerEvent.TargetServiceId == _senderServiceId)
		{
			return EventDispatcher?.Dispatch(routerEvent) ?? default;
		}

		if (routerEvent.TargetServiceId == ParentStateMachineSessionId?.SessionId)
		{
			return ParentEventDispatcher?.Dispatch(routerEvent) ?? default;
		}

		//var service = await GetService(routerEvent.TargetServiceId, token: default).ConfigureAwait(false);
		//await service.Dispatch(routerEvent).ConfigureAwait(false);
		throw new NotImplementedException(); //TODO:
	}

#endregion

#region Interface IIoProcessor

	public FullUri? GetTarget(ServiceId serviceId) =>
		serviceId switch
		{
			SessionId sessionId => new FullUri(Const.ScxmlIoProcessorBaseUri, Const.ScxmlIoProcessorSessionIdPrefix + sessionId.Value),
			InvokeId invokeId   => new FullUri(Const.ScxmlIoProcessorBaseUri, Const.ScxmlIoProcessorInvokeIdPrefix + invokeId.Value),
			_                   => default
		};

	public FullUri Id => Const.ScxmlIoProcessorId;

#endregion

	private ServiceId GetTargetServiceId(FullUri? target)
	{
		if (target is null)
		{
			return _senderServiceId;
		}

		if (IsTargetParent(target) && ParentStateMachineSessionId is not null)
		{
			return ParentStateMachineSessionId.SessionId;
		}

		if (IsTargetSessionId(target, out var targetSessionId))
		{
			return targetSessionId;
		}

		if (IsTargetInvokeId(target, out var targetInvokeId))
		{
			return targetInvokeId;
		}

		throw new ProcessorException(Resources.Exception_CannotFindTarget);
	}

	private static bool IsTargetParent(FullUri target) => target == Const.ParentTarget || target == Const.ScxmlIoProcessorParentTarget;

	private static bool IsTargetSessionId(FullUri target, [NotNullWhen(true)] out SessionId? sessionId)
	{
		var value = target.ToString();

		if (value.StartsWith(Const.ScxmlIoProcessorSessionIdPrefix, StringComparison.Ordinal))
		{
			sessionId = SessionId.FromString(value[Const.ScxmlIoProcessorSessionIdPrefix.Length..]);

			return true;
		}

		sessionId = default;

		return false;
	}

	private static bool IsTargetInvokeId(FullUri target, [NotNullWhen(true)] out InvokeId? invokeId)
	{
		var value = target.ToString();

		if (value.StartsWith(Const.ScxmlIoProcessorInvokeIdPrefix, StringComparison.Ordinal))
		{
			invokeId = InvokeId.FromString(value[Const.ScxmlIoProcessorInvokeIdPrefix.Length..]);

			return true;
		}

		invokeId = default;

		return false;
	}
}