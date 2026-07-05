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
using Xtate.Interpreter.Internal;
using Xtate.Scxml;
using Xtate.StateMachine;

namespace Xtate.StateMachineHost.Services;

public class ExternalServiceEventRouter : IEventRouter
{
	public required IReadOnlyCollection<IExternalServiceProvider> ExternalServiceProviders { private get; [SetByIoC] init; }

	public required IStateMachineSessionId StateMachineSessionId { private get; [SetByIoC] init; }

	public required IExternalServiceCollection ExternalServiceCollection { private get; [SetByIoC] init; }

	public required StateMachineRuntimeError StateMachineRuntimeError { private get; [SetByIoC] init; }

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

	public bool IsInternalTarget(FullUri? target) => false;

	public ValueTask<IRouterEvent> GetRouterEvent(IOutgoingEvent outgoingEvent, CancellationToken token) =>
		new(new RouterEvent(StateMachineSessionId.SessionId, Const.ScxmlIoProcessorId, Const.ParentTarget, outgoingEvent));

	public ValueTask Dispatch(IRouterEvent routerEvent, CancellationToken token) => ExternalServiceCollection.Dispatch(GetInvokeId(routerEvent.Target), routerEvent, token);

#endregion

	private InvokeId GetInvokeId(FullUri? target)
	{
		if (target?.ToString() is not { } str)
		{
			throw StateMachineRuntimeError.PlatformError(Resources.Exception_TargetValueIsMissed);
		}

		if (str.StartsWith(Const.ScxmlIoProcessorInvokeIdPrefix) && str.Length > Const.ScxmlIoProcessorInvokeIdPrefix.Length)
		{
			return InvokeId.FromString(str[Const.ScxmlIoProcessorInvokeIdPrefix.Length..]);
		}

		throw StateMachineRuntimeError.PlatformError(Resources.Exception_InvalidTargetFormat);
	}
}