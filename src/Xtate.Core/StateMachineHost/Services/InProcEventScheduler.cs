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
using Xtate.IoC.Tools;
using Xtate.Logging;
using Xtate.StateMachine;
using Xtate.TaskMonitor;

namespace Xtate.StateMachineHost.Services;

public class InProcEventScheduler : IEventScheduler, IDisposable, IAsyncDisposable
{
	private static readonly SendId EmptySendId = SendId.FromString(string.Empty);

	private readonly ExtCollection<SendId, ScheduledEvent> _scheduledEvents = [];

	public required IReadOnlyCollection<IEventRouter> EventRouters { private get; [SetByIoC] init; }

	public required ILogger<IEventScheduler> Logger { private get; [SetByIoC] init; }

	public required ITaskMonitor TaskMonitor { private get; [SetByIoC] init; }

	public required DisposeToken DisposeToken { private get; [SetByIoC] init; }

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

	public virtual ValueTask ScheduleEvent(IRouterEvent routerEvent, CancellationToken token)
	{
		if (routerEvent is not ScheduledEvent scheduledEvent)
		{
			scheduledEvent = new ScheduledEvent(routerEvent);
		}

		_scheduledEvents.Add(scheduledEvent.SendId ?? EmptySendId, scheduledEvent);

		DelayedFire(scheduledEvent).Forget(TaskMonitor);

		return ValueTask.CompletedTask;
	}

	public ValueTask CancelEvent(SendId sendId, CancellationToken token)
	{
		if (sendId == EmptySendId)
		{
			throw new ProcessorException(Resources.Exception_SendIdDoesNotSpecify);
		}

		return _scheduledEvents.TryRemoveGroup(sendId, out var scheduledEventList) ? CancelScheduledEvents(scheduledEventList, token) : ValueTask.CompletedTask;
	}

#endregion

	protected virtual ValueTask CancelScheduledEvents(IReadOnlyList<ScheduledEvent> scheduledEvents, CancellationToken token)
	{
		var task = scheduledEvents.Count == 1 ? scheduledEvents[0].CancelAsync() : Task.WhenAll(scheduledEvents.Select(static e => e.CancelAsync()));

		return new ValueTask(task);
	}

	protected virtual void Dispose(bool disposing)
	{
		int t = 0;

		t ++;
		t++;

		if (disposing)
		{
			List<Exception>? exceptions = null;

			while (_scheduledEvents.TryTake(out _, out var scheduledEvent))
			{
				try
				{
					scheduledEvent.Cancel();
				}
				catch (Exception ex)
				{
					exceptions ??= [];
					exceptions.Add(ex);
				}
			}

			if (exceptions != null)
			{
				throw new AggregateException(exceptions);
			}
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		await Task.WhenAll(CancelAll()).ConfigureAwait(false);

		return;

		IEnumerable<Task> CancelAll()
		{
			while (_scheduledEvents.TryTake(out _, out var scheduledEvent))
			{
				yield return scheduledEvent.CancelAsync();
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

	private async ValueTask DispatchEvent(ScheduledEvent scheduledEvent)
	{
		if (scheduledEvent.OriginType is not { } originType)
		{
			throw new PlatformException(Resources.Exception_OriginTypeMustBeProvidedInRouterEvent) { Owner = null! };
		}

		var eventRouter = GetEventRouter(originType);
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(DisposeToken, scheduledEvent.CancellationToken);

		await eventRouter.Dispatch(scheduledEvent, cts.Token).ConfigureAwait(false);
	}

	protected virtual Task WaitForDispatch(ScheduledEvent scheduledEvent) => Task.Delay(scheduledEvent.DelayMs, scheduledEvent.CancellationToken);

	private async Task DelayedFire(ScheduledEvent scheduledEvent)
	{
		try
		{
			await WaitForDispatch(scheduledEvent).ConfigureAwait(false);

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
			_scheduledEvents.Remove(scheduledEvent.SendId ?? EmptySendId, scheduledEvent);

			scheduledEvent.Dispose();
		}
	}
}