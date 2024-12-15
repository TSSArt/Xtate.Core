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

using Xtate.DataModel;
using Xtate.IoProcessor;

namespace Xtate.Core;

public class ExternalCommunication : IExternalCommunication
{
	public required ServiceList<IEventRouter> EventRouters { private get; [UsedImplicitly] init; }

	public required IEventScheduler EventScheduler { private get; [UsedImplicitly] init; }

	public required DisposeToken DisposeToken { private get; [UsedImplicitly] init; }

	#region Interface IExternalCommunication

	public ValueTask<SendStatus> TrySend(IOutgoingEvent outgoingEvent)
	{
		var eventRouter = GetEventRouter(outgoingEvent.Type);

		if (eventRouter.IsInternalTarget(outgoingEvent.Target))
		{
			return new ValueTask<SendStatus>(SendStatus.ToInternalQueue);
		}

		return ScheduleOrSend(eventRouter, outgoingEvent);
	}

	public ValueTask Cancel(SendId sendId) => EventScheduler.CancelEvent(sendId, DisposeToken);

#endregion

	private async ValueTask<SendStatus> ScheduleOrSend(IEventRouter eventRouter, IOutgoingEvent outgoingEvent)
	{
		var routerEvent = await eventRouter.GetRouterEvent(outgoingEvent, DisposeToken).ConfigureAwait(false);

		if (outgoingEvent.DelayMs > 0)
		{
			await EventScheduler.ScheduleEvent(routerEvent, DisposeToken).ConfigureAwait(false);

			return SendStatus.Scheduled;
		}

		await eventRouter.Dispatch(routerEvent, DisposeToken).ConfigureAwait(false);

		return SendStatus.Sent;
	}

	private IEventRouter GetEventRouter(FullUri? type)
	{
		foreach (var eventRouter in EventRouters)
		{
			if (eventRouter.CanHandle(type))
			{
				return eventRouter;
			}
		}

		throw new ProcessorException(Res.Format(Resources.Exception_InvalidType, type));
	}
}