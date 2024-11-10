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

using Xtate.IoProcessor;

namespace Xtate.Core;

public class InProcEventScheduler : IEventScheduler, IDisposable
{
	private readonly MiniDictionary<ScheduledEvent, SendId?> _scheduledEvents = new();

	public required ServiceList<IEventRouter> EventRouters { private get; [UsedImplicitly] init; }

	public required EventSchedulerInfoEnricher EventSchedulerInfoEnricher { private get; [UsedImplicitly] init; }

	public required ILogger<InProcEventScheduler> Logger { private get; [UsedImplicitly] init; }

	public required IStateMachineSessionId StateMachineSessionId { private get; [UsedImplicitly] init; }
	
	public required TaskCollector TaskCollector { private get; [UsedImplicitly] init; }

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IEventScheduler

	public ValueTask ScheduleEvent(IRouterEvent routerEvent)
	{
		var scheduledEvent = new ScheduledEvent(routerEvent);

		AddScheduledEvent(scheduledEvent);

		var delayedFireTask = DelayedFire(scheduledEvent);

		TaskCollector.Collect(delayedFireTask);

		return default;
	}

	public ValueTask CancelEvent(SendId sendId)
	{
		foreach (var pair in _scheduledEvents)
		{
			if (pair.Value == sendId && _scheduledEvents.TryRemove(pair.Key, out _))
			{
				pair.Key.Cancel();
			}
		}

		return default;
	}

#endregion

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			foreach (var pair in _scheduledEvents)
			{
				if (_scheduledEvents.TryRemove(pair.Key, out _))
				{
					pair.Key.Cancel();
				}
			}
		}
	}

	private void AddScheduledEvent(ScheduledEvent scheduledEvent)
	{
		var tryAdd = _scheduledEvents.TryAdd(scheduledEvent, scheduledEvent.SendId);

		Infra.Assert(tryAdd);
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

	private async ValueTask DispatchEvent(IRouterEvent routerEvent)
	{
		if (routerEvent.OriginType is not { } originType)
		{
			throw new PlatformException(Resources.Exception_OriginTypeMustBeProvidedInRouterEvent);
		}

		var eventRouter = GetEventRouter(originType);

		await eventRouter.Dispatch(routerEvent).ConfigureAwait(false);
	}

	private async ValueTask DelayedFire(ScheduledEvent scheduledEvent)
	{
		EventSchedulerInfoEnricher.SetSessionId(StateMachineSessionId.SessionId);

		try
		{
			await Task.Delay(scheduledEvent.DelayMs, scheduledEvent.CancellationToken).ConfigureAwait(false);

			try
			{
				await DispatchEvent(scheduledEvent).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				if (Logger.IsEnabled(Level.Error))
				{
					var sendId = scheduledEvent.SendId;
					await Logger.Write(Level.Error, eventId: 1, $@"Error on dispatching event. SendId: [{sendId}].", ex).ConfigureAwait(false);
				}
			}
		}
		finally
		{
			_scheduledEvents.TryRemove(scheduledEvent, out _);

			await scheduledEvent.Dispose().ConfigureAwait(false);
		}
	}
}