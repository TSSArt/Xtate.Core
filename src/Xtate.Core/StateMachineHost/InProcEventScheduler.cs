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

using System.Collections.Concurrent;
using Xtate.IoProcessor;

namespace Xtate.Core;

public class InProcEventScheduler : IEventScheduler
{
	private readonly ConcurrentDictionary<(ServiceId, SendId), object> _scheduledEvents = new();

	public required Func<Uri?, IIoProcessor> IoProcessorFactory { private get; [UsedImplicitly] init; }

	public required Func<IHostEvent, ValueTask<ScheduledEvent>> ScheduledEventFactory { private get; [UsedImplicitly] init; }

	public required EventSchedulerInfoEnricher EventSchedulerInfoEnricher { private get; [UsedImplicitly] init; }

	public required ILogger<InProcEventScheduler> Logger { private get; [UsedImplicitly] init; }

#region Interface IEventScheduler

	public async ValueTask ScheduleEvent(IHostEvent hostEvent, CancellationToken token)
	{
		if (hostEvent.SenderServiceId is SessionId sessionId)
		{
			EventSchedulerInfoEnricher.SetSessionId(sessionId);
		}

		var scheduledEvent = await ScheduledEventFactory(hostEvent).ConfigureAwait(false);

		AddScheduledEvent(scheduledEvent);

		DelayedFire(scheduledEvent).Forget();
	}

	public ValueTask CancelEvent(ServiceId senderServiceId, SendId sendId, CancellationToken token)
	{
		if (!_scheduledEvents.TryRemove((senderServiceId, sendId), out var value))
		{
			return default;
		}

		if (value is ImmutableHashSet<ScheduledEvent> set)
		{
			foreach (var evt in set)
			{
				evt.Cancel();
			}
		}
		else
		{
			((ScheduledEvent) value).Cancel();
		}

		return default;
	}

#endregion

	private void AddScheduledEvent(ScheduledEvent scheduledEvent)
	{
		if (scheduledEvent.SendId is { } sendId)
		{
			_scheduledEvents.AddOrUpdate((scheduledEvent.SenderServiceId, sendId), static (_, e) => e, Update, scheduledEvent);
		}

		static object Update((ServiceId, SendId) key, object prev, ScheduledEvent arg)
		{
			if (prev is not ImmutableHashSet<ScheduledEvent> set)
			{
				set = ImmutableHashSet<ScheduledEvent>.Empty.Add((ScheduledEvent) prev);
			}

			return set.Add(arg);
		}
	}

	private async ValueTask DispatchEvent(IHostEvent hostEvent)
	{
		if (hostEvent.OriginType is not { } originType)
		{
			throw new PlatformException(Resources.Exception_OriginTypeMustBeProvidedInIoProcessorEvent);
		}

		var ioProcessor = IoProcessorFactory(originType);

		if (ioProcessor.Id != originType)
		{
			throw new ProcessorException(Resources.Exception_InvalidType);
		}

		await ioProcessor.Dispatch(hostEvent, token: default).ConfigureAwait(false);
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
			await scheduledEvent.Dispose(token: default).ConfigureAwait(false);
		}
	}

	private void RemoveScheduledEvent(ScheduledEvent scheduledEvent)
	{
		if (scheduledEvent.SendId is not { } sendId)
		{
			return;
		}

		var serviceId = scheduledEvent.SenderServiceId;
		var exit = false;

		while (!exit && _scheduledEvents.TryGetValue((serviceId, sendId), out var value))
		{
			var newValue = RemoveFromValue(value, scheduledEvent);

			exit = newValue is null
				? _scheduledEvents.TryRemove(new KeyValuePair<(ServiceId, SendId), object>((serviceId, sendId), value))
				: ReferenceEquals(value, newValue) || _scheduledEvents.TryUpdate((serviceId, sendId), value, newValue);
		}

		static object? RemoveFromValue(object value, ScheduledEvent scheduledEvent)
		{
			if (ReferenceEquals(value, scheduledEvent))
			{
				return null;
			}

			if (value is not ImmutableHashSet<ScheduledEvent> set)
			{
				return value;
			}

			var newSet = set.Remove(scheduledEvent);

			return newSet.Count > 0 ? newSet : null;
		}
	}
}