﻿// Copyright © 2019-2024 Sergii Artemenko
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
using Xtate.Persistence;

namespace Xtate.Core;

public interface IEventSchedulerLogger
{
	bool IsEnabled { get; }

	ValueTask LogError(string message, Exception exception, IHostEvent scheduledEvent);
}

internal class InProcEventScheduler(IHostEventDispatcher hostEventDispatcher, IEventSchedulerLogger logger) : IEventScheduler
{
	private readonly ConcurrentDictionary<(ServiceId, SendId), object> _scheduledEvents = new();

#region Interface IEventScheduler

	public async ValueTask ScheduleEvent(IHostEvent hostEvent, CancellationToken token)
	{
		var scheduledEvent = await CreateScheduledEvent(hostEvent, token).ConfigureAwait(false);

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

	protected virtual ValueTask<ScheduledEvent> CreateScheduledEvent(IHostEvent hostEvent, CancellationToken token) => new(new ScheduledEvent(hostEvent));

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

	private async ValueTask DelayedFire(ScheduledEvent scheduledEvent)
	{
		if (scheduledEvent is null) throw new ArgumentNullException(nameof(scheduledEvent));

		try
		{
			await Task.Delay(scheduledEvent.DelayMs, scheduledEvent.CancellationToken).ConfigureAwait(false);

			try
			{
				await hostEventDispatcher.DispatchEvent(scheduledEvent, token: default).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				if (logger.IsEnabled)
				{
					var message = Res.Format(Resources.Exception_ErrorOnDispatchingEvent, scheduledEvent.SendId?.Value);
					await logger.LogError(message, ex, scheduledEvent).ConfigureAwait(false);
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

	private class LoggerContext(ScheduledEvent scheduledEvent) : IEventSchedulerLoggerContext
	{
		public string LoggerContextType => nameof(IEventSchedulerLoggerContext);

	#region Interface IEventSchedulerLoggerContext

		public SessionId? SessionId => scheduledEvent.SenderServiceId as SessionId;

	#endregion

		public DataModelList GetProperties()
		{
			if (scheduledEvent.SenderServiceId is SessionId sessionId)
			{
				var properties = new DataModelList { { @"SessionId", sessionId } };
				properties.MakeDeepConstant();

				return properties;
			}

			return DataModelList.Empty;
		}
	}

	internal class ScheduledEvent : HostEvent
	{
		private readonly CancellationTokenSource _cancellationTokenSource = new();

		public ScheduledEvent(IHostEvent hostEvent) : base(hostEvent) { }

		protected ScheduledEvent(in Bucket bucket) : base(in bucket) { }

		public CancellationToken CancellationToken => _cancellationTokenSource.Token;

		public void Cancel() => _cancellationTokenSource.Cancel();

		public virtual ValueTask Dispose(CancellationToken token)
		{
			_cancellationTokenSource.Dispose();

			return default;
		}
	}
}