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

using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;

namespace Xtate.IoProcessors.Http.Services;

public class HttpIoProcessor() : IoProcessorBase(ioProcessorId: @"http://www.w3.org/TR/scxml/#BasicHTTPEventProcessor", ioProcessorAlias: @"http")
{
	public required HttpController HttpController { private get; [SetByIoC] init; }

	public required IInternalEventDispatcher<HttpIoProcessor> InternalEventDispatcher { private get; [SetByIoC] init; }

	protected override ValueTask Dispatch(IRouterEvent routerEvent, CancellationToken token)
	{
		Infra.Assert(routerEvent.DelayMs == 0);

		if (routerEvent.Target is not { } target)
		{
			throw new ProcessorException(Resources.Exception_TargetIsNotDefined);
		}

		if (HttpController.TryMatchTarget(target, out var serviceId))
		{
			return InternalEventDispatcher.Dispatch(serviceId, routerEvent, token);
		}

		return HttpController.SendEvent(target, routerEvent, token);
	}

	protected override FullUri CreateExternalServiceTarget(InvokeId invokeId) => HttpController.ToInvokeTarget(invokeId);

	protected override FullUri CreateStateMachineTarget(SessionId sessionId) => HttpController.ToSessionTarget(sessionId);
}