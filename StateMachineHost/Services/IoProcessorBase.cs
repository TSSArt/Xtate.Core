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

using Xtate.Interpreter;
using Xtate.StateMachine;

namespace Xtate.StateMachineHost.Services;

public abstract class IoProcessorBase(FullUri ioProcessorId, FullUri? ioProcessorAlias = null) : IIoProcessor, IEventRouter
{
	public required IExternalServiceInvokeId? InvokeIdBase { private get; [SetByIoC] init; }

	public required IStateMachineSessionId SessionIdBase { private get; [SetByIoC] init; }

	protected virtual FullUri? Target => field ??= CreateTarget();

#region Interface IEventRouter

	bool IEventRouter.IsInternalTarget(FullUri? target) => IsInternalTarget(target);

	ValueTask<IRouterEvent> IEventRouter.GetRouterEvent(IOutgoingEvent outgoingEvent, CancellationToken token) => GetRouterEvent(outgoingEvent, token);

	ValueTask IEventRouter.Dispatch(IRouterEvent routerEvent, CancellationToken token) => Dispatch(routerEvent, token);

	bool IEventRouter.CanHandle(FullUri? type) => CanHandle(type);

#endregion

#region Interface IIoProcessor

	FullUri? IIoProcessor.Target => Target;

	FullUri IIoProcessor.Id => ioProcessorId;

#endregion

	private FullUri? CreateTarget() => InvokeIdBase?.InvokeId is { } invokeId ? CreateExternalServiceTarget(invokeId) : CreateStateMachineTarget(SessionIdBase.SessionId);

	protected virtual FullUri? CreateExternalServiceTarget(InvokeId invokeId) => null;

	protected virtual FullUri? CreateStateMachineTarget(SessionId sessionId) => null;

	protected virtual bool CanHandle(FullUri? type) => type == ioProcessorId || (type is not null && type == ioProcessorAlias);

	protected virtual bool IsInternalTarget(FullUri? target) => false;

	protected virtual ValueTask<IRouterEvent> GetRouterEvent(IOutgoingEvent outgoingEvent, CancellationToken token)
	{
		var serviceId = (ServiceId?) InvokeIdBase?.InvokeId ?? SessionIdBase.SessionId;

		var routerEvent = new RouterEvent(serviceId, ioProcessorId, Target, outgoingEvent);

		return new ValueTask<IRouterEvent>(routerEvent);
	}

	protected abstract ValueTask Dispatch(IRouterEvent routerEvent, CancellationToken token);
}