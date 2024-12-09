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

public class InProcEventScheduler : IEventScheduler, IDisposable, IAsyncDisposable
{
	private static readonly SendId EmptySendId = SendId.FromString(string.Empty);

	private readonly DisposingToken _disposingToken = new ();

	private readonly MiniDictionary<SendId, ImmutableList<ScheduledEvent>> _scheduledEvents = new();

	public required ServiceList<IEventRouter> EventRouters { private get; [UsedImplicitly] init; }

	public required ILogger<IEventScheduler> Logger { private get; [UsedImplicitly] init; }

	public required TaskMonitor TaskMonitor { private get; [UsedImplicitly] init; }

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		Dispose(false);

		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IEventScheduler

	public ValueTask ScheduleEvent(IRouterEvent routerEvent, CancellationToken token)
	{
		var scheduledEvent = new ScheduledEvent(routerEvent);

		AddScheduledEvent(scheduledEvent);

		DelayedFire(scheduledEvent).Forget(TaskMonitor);

		return default;
	}

	public async ValueTask CancelEvent(SendId sendId, CancellationToken token)
	{
		if (sendId == EmptySendId)
		{
			throw new ProcessorException(Resources.Exception_SendIdDoesNotSpecify);
		}

		if (_scheduledEvents.TryRemove(sendId, out var scheduledEvents))
		{
			foreach (var scheduledEvent in scheduledEvents)
			{
				await scheduledEvent.CancelAsync().ConfigureAwait(false);
			}
		}
	}

#endregion

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposingToken.Dispose();

			while (_scheduledEvents.TryTake(out _, out var scheduledEvents))
			{
				foreach (var scheduledEvent in scheduledEvents)
				{
					scheduledEvent.Cancel();
				}
			}
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		await _disposingToken.DisposeAsync().ConfigureAwait(false);

		while (_scheduledEvents.TryTake(out _, out var scheduledEvents))
		{
			foreach (var scheduledEvent in scheduledEvents)
			{
				await scheduledEvent.CancelAsync().ConfigureAwait(false);
			}
		}
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

		await eventRouter.Dispatch(routerEvent, _disposingToken.Token).ConfigureAwait(false);
	}

	private async ValueTask DelayedFire(ScheduledEvent scheduledEvent)
	{
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
			RemoveScheduledEvent(scheduledEvent);

			await scheduledEvent.Dispose().ConfigureAwait(false);
		}
	}

	private void AddScheduledEvent(ScheduledEvent scheduledEvent) =>
		_scheduledEvents.AddOrUpdate(scheduledEvent.SendId ?? EmptySendId, (_, evt) => [evt], (_, list, evt) => list.Add(evt), scheduledEvent);

	private void RemoveScheduledEvent(ScheduledEvent scheduledEvent) =>
		_scheduledEvents.UpdateOrRemove(scheduledEvent.SendId ?? EmptySendId, (_, list, evt) => list is { Count: 1 } && list.Contains(evt), (_, list, evt) => list.Remove(evt), scheduledEvent);
}