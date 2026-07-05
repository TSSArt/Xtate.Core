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

using Xtate.DataModel;
using Xtate.Interpreter.Internal;
using Xtate.Logging;
using Xtate.Scxml;
using Xtate.StateMachine;

namespace Xtate.Interpreter.Services;

[InstantiatedByIoC]
public class EventController : IEventController
{
	private const int SendEventId = 1;

	private const int CancelEventId = 2;

	public required IExternalCommunication ExternalCommunication { private get; [SetByIoC] init; }

	public required ILogger<IEventController> Logger { private get; [SetByIoC] init; }

	public required StateMachineRuntimeError StateMachineRuntimeError { private get; [SetByIoC] init; }

	public required IStateMachineContext StateMachineContext { private get; [SetByIoC] init; }

#region Interface IEventController

	public virtual async ValueTask Send(IOutgoingEvent outgoingEvent)
	{
		var sendId = outgoingEvent.SendId;
		var eventName = outgoingEvent.Name;
		await Logger.Write(Level.Trace, SendEventId, $@"Send Event. SendId: [{sendId}], Name: '{eventName}'", outgoingEvent).ConfigureAwait(false);

		if (await TrySendEvent(outgoingEvent).ConfigureAwait(false) == SendStatus.ToInternalQueue)
		{
			if (outgoingEvent.DelayMs != 0)
			{
				throw new ExecutionException(Resources.Exception_InternalEventsCantBeDelayed);
			}

			StateMachineContext.InternalQueue.Enqueue(new IncomingEvent(outgoingEvent) { Type = EventType.Internal });
		}
	}

	public virtual async ValueTask Cancel(SendId sendId)
	{
		await Logger.Write(Level.Trace, CancelEventId, $@"Cancel Event. SendId: [{sendId}]", sendId).ConfigureAwait(false);

		try
		{
			await ExternalCommunication.Cancel(sendId).ConfigureAwait(false);
		}
		catch (Exception ex) when (!StateMachineRuntimeError.IsPlatformError(ex))
		{
			throw StateMachineRuntimeError.CommunicationError(ex, sendId);
		}
	}

#endregion

	private async ValueTask<SendStatus> TrySendEvent(IOutgoingEvent outgoingEvent)
	{
		if (IsInternalEvent(outgoingEvent))
		{
			return SendStatus.ToInternalQueue;
		}

		try
		{
			return await ExternalCommunication.TrySend(outgoingEvent).ConfigureAwait(false);
		}
		catch (Exception ex) when (!StateMachineRuntimeError.IsPlatformError(ex))
		{
			throw StateMachineRuntimeError.CommunicationError(ex, outgoingEvent.SendId);
		}
	}

	private static bool IsInternalEvent(IOutgoingEvent outgoingEvent)
	{
		if (outgoingEvent.Target == Const.InternalTarget && outgoingEvent.Type is null)
		{
			return true;
		}

		return false;
	}
}