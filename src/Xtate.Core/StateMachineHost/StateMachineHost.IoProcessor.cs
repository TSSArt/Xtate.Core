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

using Xtate.ExternalService;
using Xtate.IoProcessor;

namespace Xtate;

public sealed partial class StateMachineHost : IIoProcessor, IEventConsumer, IEventRouter
{
#region Interface IEventConsumer

	public async ValueTask<IEventDispatcher?> TryGetEventDispatcher(ServiceId serviceId, CancellationToken token) =>
		serviceId switch
		{
			SessionId sessionId                                                                 => (IEventDispatcher)await GetCurrentContext().FindStateMachineController(sessionId, token).ConfigureAwait(false),
			InvokeId invokeId when GetCurrentContext().TryGetService(invokeId, out var service) => (IEventDispatcher)service,
			_                                                                                   => default
		};

#endregion

#region Interface IEventRouter

	public bool IsInternalTarget(FullUri? target) => target == Const.ScxmlIoProcessorInternalTarget;

	ValueTask<IRouterEvent> IEventRouter.GetRouterEvent(IOutgoingEvent outgoingEvent)
	{
		ServiceId senderServiceId = default!;

		if (senderServiceId is null) throw new ArgumentNullException(nameof(senderServiceId));

		var target = outgoingEvent.Target ?? throw new ProcessorException(Resources.Exception_EventTargetDidNotSpecify); //TODO:can be null

		if (senderServiceId is SessionId sessionId && IsTargetParent(target))
		{
			if (GetCurrentContext().TryGetParentSessionId(sessionId, out var parentSessionId))
			{
				return new ValueTask<IRouterEvent>(new RouterEvent(senderServiceId, parentSessionId, Const.ScxmlIoProcessorId, GetTarget(senderServiceId), outgoingEvent));
			}
		}
		else if (IsTargetSessionId(target, out var targetSessionId))
		{
			return new ValueTask<IRouterEvent>(new RouterEvent(senderServiceId, targetSessionId, Const.ScxmlIoProcessorId, GetTarget(senderServiceId), outgoingEvent));
		}
		else if (IsTargetInvokeId(target, out var targetInvokeId))
		{
			return new ValueTask<IRouterEvent>(new RouterEvent(senderServiceId, targetInvokeId, Const.ScxmlIoProcessorId, GetTarget(senderServiceId), outgoingEvent));
		}

		throw new ProcessorException(Resources.Exception_CannotFindTarget);
	}

	async ValueTask IEventRouter.Dispatch(IRouterEvent routerEvent)
	{
		Infra.NotNull(routerEvent.TargetServiceId);

		var service = await GetService(routerEvent.TargetServiceId, token: default).ConfigureAwait(false);
		//await service.Dispatch(routerEvent).ConfigureAwait(false);
	}

	bool IEventRouter.CanHandle(FullUri? type) => CanHandleType(type);

#endregion

#region Interface IIoProcessor

	FullUri? IIoProcessor.GetTarget(ServiceId serviceId) => GetTarget(serviceId);

	FullUri IIoProcessor.Id => Const.ScxmlIoProcessorId;

#endregion

	private async ValueTask<IExternalService> GetService(ServiceId serviceId, CancellationToken token) =>
		serviceId switch
		{
			SessionId sessionId
				when await GetCurrentContext().FindStateMachineController(sessionId, token).ConfigureAwait(false) is { } controller => controller,
			InvokeId invokeId
				when GetCurrentContext().TryGetService(invokeId, out var service) && service is not null => service,
			_ => throw new ProcessorException(Resources.Exception_CannotFindTarget)
		};

	private static bool CanHandleType(FullUri? type) => type is null || type == Const.ScxmlIoProcessorId || type == Const.ScxmlIoProcessorAliasId;

	private static FullUri? GetTarget(ServiceId serviceId) =>
		serviceId switch
		{
			SessionId sessionId => new FullUri(Const.ScxmlIoProcessorBaseUri, Const.ScxmlIoProcessorSessionIdPrefix + sessionId.Value),
			InvokeId invokeId   => new FullUri(Const.ScxmlIoProcessorBaseUri, Const.ScxmlIoProcessorInvokeIdPrefix + invokeId.Value),
			_                   => default
		};

	private static string GetTargetString(FullUri target) => target.IsAbsoluteUri ? target.Fragment : target.OriginalString;

	private static bool IsTargetParent(FullUri target) => target == Const.ScxmlIoProcessorParentTarget;

	private static bool IsTargetSessionId(FullUri target, [NotNullWhen(true)] out SessionId? sessionId)
	{
		var value = GetTargetString(target);

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
		var value = GetTargetString(target);

		if (value.StartsWith(Const.ScxmlIoProcessorInvokeIdPrefix, StringComparison.Ordinal))
		{
			invokeId = InvokeId.FromString(value[Const.ScxmlIoProcessorInvokeIdPrefix.Length..]);

			return true;
		}

		invokeId = default;

		return false;
	}
}