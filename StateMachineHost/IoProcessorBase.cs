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

namespace Xtate.IoProcessor;

public abstract class IoProcessorBase : IIoProcessor, IEventRouter
{
	private readonly FullUri? _ioProcessorAliasId;

	private readonly FullUri _ioProcessorId;

	protected IoProcessorBase(FullUri ioProcessorId, FullUri? ioProcessorAlias)
	{
		Infra.Requires(ioProcessorId);

		_ioProcessorId = ioProcessorId;
		_ioProcessorAliasId = ioProcessorAlias;
	}

	public required IStateMachineSessionId StateMachineSessionId { private get; [UsedImplicitly] init; }

#region Interface IEventRouter

	bool IEventRouter.IsInternalTarget(FullUri? target) => false;

	ValueTask<IRouterEvent> IEventRouter.GetRouterEvent(IOutgoingEvent outgoingEvent, CancellationToken token) => GetRouterEvent(outgoingEvent, token);

	ValueTask IEventRouter.Dispatch(IRouterEvent routerEvent, CancellationToken token) => OutgoingEvent(routerEvent, token);

	bool IEventRouter.CanHandle(FullUri? type) => type == _ioProcessorId || (type is not null && type == _ioProcessorAliasId);

#endregion

#region Interface IIoProcessor

	FullUri? IIoProcessor.GetTarget(ServiceId serviceId) => GetTarget(serviceId);

	FullUri IIoProcessor.Id => _ioProcessorId;

#endregion

	protected abstract FullUri? GetTarget(ServiceId serviceId);

	protected virtual ValueTask<IRouterEvent> GetRouterEvent(IOutgoingEvent outgoingEvent, CancellationToken token)
	{
		var sessionId = StateMachineSessionId.SessionId;

		var routerEvent = new RouterEvent(sessionId, outgoingEvent.Target is { } target ? UriId.FromUri(target) : null, _ioProcessorId, GetTarget(sessionId), outgoingEvent);

		return new ValueTask<IRouterEvent>(routerEvent);
	}

	protected abstract ValueTask OutgoingEvent(IRouterEvent routerEvent, CancellationToken token);
}