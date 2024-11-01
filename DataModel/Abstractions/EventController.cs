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

namespace Xtate.DataModel;

public class EventController : IEventController
{
	private const int SendEventId = 1;

	private const int CancelEventId = 2;

	private static readonly Uri InternalTarget = new(uriString: "_internal", UriKind.Relative);

	public required IExternalEventCommunication ExternalEventCommunication { private get; [UsedImplicitly] init; }

	public required ILogger<IEventController> Logger { private get; [UsedImplicitly] init; }

	public required StateMachineRuntimeError StateMachineRuntimeError { private get; [UsedImplicitly] init; }

	public required IStateMachineContext StateMachineContext { private get; [UsedImplicitly] init; }

#region Interface IEventController

	public virtual async ValueTask Cancel(SendId sendId)
	{
		await Logger.Write(Level.Trace, CancelEventId, $@"Cancel Event '{sendId}'", sendId).ConfigureAwait(false);

		try
		{
			await ExternalEventCommunication.Cancel(sendId).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw StateMachineRuntimeError.CommunicationError(ex, sendId);
		}
	}

	public virtual async ValueTask Send(IOutgoingEvent outgoingEvent)
	{
		await Logger.Write(Level.Trace, SendEventId, $@"Send event: '{outgoingEvent.Name}'", outgoingEvent).ConfigureAwait(false);

		if (await TrySendEvent(outgoingEvent).ConfigureAwait(false) == SendStatus.ToInternalQueue)
		{
			if (outgoingEvent.DelayMs != 0)
			{
				throw new ExecutionException(Resources.Exception_InternalEventsCantBeDelayed);
			}

			StateMachineContext.InternalQueue.Enqueue(new EventObject(outgoingEvent) { Type = EventType.Internal });
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
			return await ExternalEventCommunication.TrySend(outgoingEvent).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw StateMachineRuntimeError.CommunicationError(ex, outgoingEvent.SendId);
		}
	}

	private static bool IsInternalEvent(IOutgoingEvent outgoingEvent)
	{
		if (outgoingEvent.Target == InternalTarget && outgoingEvent.Type is null)
		{
			return true;
		}

		return false;
	}
}