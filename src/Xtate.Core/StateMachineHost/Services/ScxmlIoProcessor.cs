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
using Xtate.Scxml;
using Xtate.StateMachine;

namespace Xtate.StateMachineHost.Services;

public class ScxmlIoProcessor() : IoProcessorBase(Const.ScxmlIoProcessorId, Const.ScxmlIoProcessorAliasId)
{
	public required IStateMachineSessionId StateMachineSessionId { private get; [SetByIoC] init; }

	public required IParentStateMachineSessionId? ParentStateMachineSessionId { private get; [SetByIoC] init; }

	public required IInternalEventDispatcher<ScxmlIoProcessor> InternalEventDispatcher { private get; [SetByIoC] init; }

	protected override bool CanHandle(FullUri? type) => type is null || base.CanHandle(type);

	protected override bool IsInternalTarget(FullUri? target) => target == Const.InternalTarget || target == Const.ScxmlIoProcessorInternalTarget || base.IsInternalTarget(target);

	protected override ValueTask Dispatch(IRouterEvent routerEvent, CancellationToken token)
	{
		if (routerEvent.TargetServiceId is not { } targetServiceId)
		{
			throw new ProcessorException(Resources.Exception_InvalidTargetFormat);
		}

		return InternalEventDispatcher.Dispatch(targetServiceId, routerEvent, token);
	}

	protected override FullUri CreateExternalServiceTarget(InvokeId invokeId) => new(Const.ScxmlIoProcessorBaseUri, Const.ScxmlIoProcessorInvokeIdPrefix + invokeId.Value);

	protected override FullUri CreateStateMachineTarget(SessionId sessionId) => new(Const.ScxmlIoProcessorBaseUri, Const.ScxmlIoProcessorSessionIdPrefix + sessionId.Value);

	protected override ServiceId GetTargetServiceId(IOutgoingEvent outgoingEvent)
	{
		var target = outgoingEvent.Target;

		if (target is null)
		{
			return StateMachineSessionId.SessionId;
		}

		if (IsTargetParent(target) && ParentStateMachineSessionId?.ParentSessionId is { } parentSessionId)
		{
			return parentSessionId;
		}

		if (IsTargetSessionId(target, out var sessionId))
		{
			return sessionId;
		}

		if (IsTargetInvokeId(target, out var invokeId))
		{
			return invokeId;
		}

		throw new ProcessorException(Resources.Exception_InvalidTargetFormat);
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

		sessionId = null;

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

		invokeId = null;

		return false;
	}
}
