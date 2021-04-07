#region Copyright © 2019-2021 Sergii Artemenko

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

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Persistence;

namespace Xtate.Core
{
	internal class InProcEventScheduler : IEventScheduler
	{
		private readonly IHostEventDispatcher _hostEventDispatcher;
		private readonly ILogger?             _logger;

		private readonly ConcurrentDictionary<(ServiceId, SendId), object> _scheduledEvents = new();

		public InProcEventScheduler(IHostEventDispatcher hostEventDispatcher, ILogger? logger)
		{
			_hostEventDispatcher = hostEventDispatcher;
			_logger = logger;
		}

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
				_scheduledEvents.AddOrUpdate((scheduledEvent.SenderServiceId, sendId), static(_, e) => e, Update, scheduledEvent);
			}

			static object Update((ServiceId, SendId) key, object prev, ScheduledEvent arg)
			{
				if (prev is not ImmutableHashSet<ScheduledEvent> set)
				{
					set = ImmutableHashSet.Create((ScheduledEvent) prev);
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
					await _hostEventDispatcher.DispatchEvent(scheduledEvent, token: default).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					if (_logger is not null)
					{
						var loggerContext = new LoggerContext(scheduledEvent);
						var message = Res.Format(Resources.Exception_ErrorOnDispatchingEvent, scheduledEvent.SendId?.Value);
						await _logger.ExecuteLog(loggerContext, LogLevel.Error, message, arguments: default, ex, token: default).ConfigureAwait(false);
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

		private class LoggerContext : IEventSchedulerLoggerContext
		{
			private readonly ScheduledEvent _scheduledEvent;

			public LoggerContext(ScheduledEvent scheduledEvent) => _scheduledEvent = scheduledEvent;

		#region Interface IEventSchedulerLoggerContext

			public SessionId? SessionId => _scheduledEvent.SenderServiceId as SessionId;

		#endregion

		#region Interface ILoggerContext

			public DataModelList GetProperties()
			{
				if (_scheduledEvent.SenderServiceId is SessionId sessionId)
				{
					var properties = new DataModelList { { @"SessionId", sessionId } };
					properties.MakeDeepConstant();

					return properties;
				}

				return DataModelList.Empty;
			}

			public string LoggerContextType => nameof(IEventSchedulerLoggerContext);

		#endregion
		}

		[PublicAPI]
		protected class ScheduledEvent : HostEvent
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
}