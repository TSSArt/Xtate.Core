#region Copyright © 2019-2020 Sergii Artemenko

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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Xtate.Persistence
{
	internal sealed class StateMachinePersistedController : StateMachineController, IStorageProvider
	{
		private const    string                              ControllerStateKey = "cs";
		private const    int                                 ExternalEventsKey  = 0;
		private const    int                                 ScheduledEventsKey = 1;
		private readonly ChannelPersistingController<IEvent> _channelPersistingController;

		private readonly HashSet<ScheduledPersistedEvent> _scheduledEvents = new HashSet<ScheduledPersistedEvent>();
		private readonly CancellationToken                _stopToken;
		private readonly SemaphoreSlim                    _storageLock = new SemaphoreSlim(initialCount: 0, maxCount: 1);
		private readonly IStorageProvider                 _storageProvider;

		private bool                   _disposed;
		private int                    _recordId;
		private ITransactionalStorage? _storage;

		public StateMachinePersistedController(SessionId sessionId, IStateMachineOptions? options, IStateMachine? stateMachine, Uri? stateMachineLocation,
											   IStateMachineHost stateMachineHost, IStorageProvider storageProvider, TimeSpan idlePeriod, in InterpreterOptions defaultOptions)
				: base(sessionId, options, stateMachine, stateMachineLocation, stateMachineHost, idlePeriod, defaultOptions)
		{
			_storageProvider = storageProvider;
			_stopToken = defaultOptions.StopToken;

			_channelPersistingController = new ChannelPersistingController<IEvent>(base.Channel);
		}

		protected override Channel<IEvent> Channel => _channelPersistingController;

	#region Interface IStorageProvider

		ValueTask<ITransactionalStorage> IStorageProvider.GetTransactionalStorage(string? partition, string key, CancellationToken token)
		{
			if (partition is not null) throw new ArgumentException(Resources.Exception_Partition_argument_should_be_null, nameof(partition));

			return _storageProvider.GetTransactionalStorage(SessionId.Value, key, token);
		}

		ValueTask IStorageProvider.RemoveTransactionalStorage(string? partition, string key, CancellationToken token)
		{
			if (partition is not null) throw new ArgumentException(Resources.Exception_Partition_argument_should_be_null, nameof(partition));

			return _storageProvider.RemoveTransactionalStorage(SessionId.Value, key, token);
		}

		ValueTask IStorageProvider.RemoveAllTransactionalStorage(string? partition, CancellationToken token)
		{
			if (partition is not null) throw new ArgumentException(Resources.Exception_Partition_argument_should_be_null, nameof(partition));

			return _storageProvider.RemoveAllTransactionalStorage(SessionId.Value, token);
		}

	#endregion

		public override async ValueTask DisposeAsync()
		{
			if (_disposed)
			{
				return;
			}

			_storageLock.Dispose();

			_channelPersistingController.Dispose();

			if (_storage is { } storage)
			{
				await storage.DisposeAsync().ConfigureAwait(false);
			}

			await base.DisposeAsync().ConfigureAwait(false);

			_disposed = true;
		}

		protected override async ValueTask Initialize()
		{
			await base.Initialize().ConfigureAwait(false);

			_storage = await _storageProvider.GetTransactionalStorage(SessionId.Value, ControllerStateKey, _stopToken).ConfigureAwait(false);

			_channelPersistingController.Initialize(new Bucket(_storage).Nested(ExternalEventsKey), bucket => new EventObject(bucket), _storageLock, token => _storage.CheckPoint(level: 0, token));

			LoadScheduledEvents(_storage);

			_storageLock.Release();
		}

		protected override async ValueTask ScheduleEvent(IOutgoingEvent evt, CancellationToken token)
		{
			var scheduledPersistedEvent = new ScheduledPersistedEvent(evt);

			await _storageLock.WaitAsync(token).ConfigureAwait(false);
			try
			{
				_scheduledEvents.Add(scheduledPersistedEvent);

				if (_storage is { } storage)
				{
					scheduledPersistedEvent.RecordId = _recordId ++;

					var rootBucket = new Bucket(storage).Nested(ScheduledEventsKey);
					rootBucket.Add(Bucket.RootKey, _recordId);
					scheduledPersistedEvent.Store(rootBucket.Nested(scheduledPersistedEvent.RecordId));

					await storage.CheckPoint(level: 0, token).ConfigureAwait(false);
				}
			}
			finally
			{
				_storageLock.Release();
			}

			DelayedFire(scheduledPersistedEvent, evt.DelayMs).Forget();
		}

		protected override async ValueTask CancelEvent(ScheduledEvent scheduledEvent, CancellationToken token)
		{
			if (scheduledEvent is null) throw new ArgumentNullException(nameof(scheduledEvent));

			var scheduledPersistedEvent = (ScheduledPersistedEvent) scheduledEvent;

			scheduledPersistedEvent.Cancel();

			await _storageLock.WaitAsync(token).ConfigureAwait(false);
			try
			{
				_scheduledEvents.Remove(scheduledPersistedEvent);

				if (_storage is { } storage)
				{
					var rootBucket = new Bucket(storage).Nested(ScheduledEventsKey);
					rootBucket.RemoveSubtree(scheduledPersistedEvent.RecordId);

					await storage.CheckPoint(level: 0, token).ConfigureAwait(false);

					await ShrinkScheduledEvents(storage, token).ConfigureAwait(false);
				}
			}
			finally
			{
				_storageLock.Release();
			}
		}

		private async ValueTask ShrinkScheduledEvents(ITransactionalStorage storage, CancellationToken token)
		{
			if (_scheduledEvents.Count * 2 > _recordId)
			{
				return;
			}

			_recordId = 0;
			var rootBucket = new Bucket(storage).Nested(ScheduledEventsKey);
			rootBucket.RemoveSubtree(Bucket.RootKey);

			foreach (var scheduledEvent in _scheduledEvents)
			{
				scheduledEvent.RecordId = _recordId ++;
				scheduledEvent.Store(rootBucket.Nested(scheduledEvent.RecordId));
			}

			if (_recordId > 0)
			{
				rootBucket.Add(Bucket.RootKey, _recordId);
			}

			await storage.CheckPoint(level: 0, token).ConfigureAwait(false);
			await storage.Shrink(token).ConfigureAwait(false);
		}

		private void LoadScheduledEvents(ITransactionalStorage storage)
		{
			var bucket = new Bucket(storage).Nested(ScheduledEventsKey);

			bucket.TryGet(Bucket.RootKey, out _recordId);

			var utcNow = DateTime.UtcNow;

			for (var i = 0; i < _recordId; i ++)
			{
				var eventBucket = bucket.Nested(i);
				if (eventBucket.TryGet(Key.TypeInfo, out TypeInfo typeInfo) && typeInfo == TypeInfo.ScheduledEvent)
				{
					var scheduledEvent = new ScheduledPersistedEvent(eventBucket) { RecordId = i };

					_scheduledEvents.Add(scheduledEvent);

					var delayMs = (int) ((scheduledEvent.FireOnUtc - utcNow).Ticks / TimeSpan.TicksPerMillisecond);

					DelayedFire(scheduledEvent, delayMs > 1 ? delayMs : 1).Forget();
				}
			}
		}

		private sealed class ScheduledPersistedEvent : ScheduledEvent, IStoreSupport
		{
			private readonly long _fireOnUtcTicks;

			public ScheduledPersistedEvent(IOutgoingEvent evt) : base(evt)
			{
				if (evt is null) throw new ArgumentNullException(nameof(evt));

				_fireOnUtcTicks = DateTime.UtcNow.Ticks + evt.DelayMs * TimeSpan.TicksPerMillisecond;
			}

			public ScheduledPersistedEvent(Bucket bucket) : base(RestoreEvent(bucket))
			{
				bucket.TryGet(Key.FireOn, out _fireOnUtcTicks);
			}

			public int RecordId { get; set; }

			public DateTime FireOnUtc => new DateTime(_fireOnUtcTicks, DateTimeKind.Utc);

		#region Interface IStoreSupport

			public void Store(Bucket bucket)
			{
				bucket.Add(Key.TypeInfo, TypeInfo.ScheduledEvent);
				bucket.Add(Key.SendId, Event.SendId);
				bucket.Add(Key.Name, EventName.ToName(Event.NameParts));
				bucket.Add(Key.Target, Event.Target);
				bucket.Add(Key.Type, Event.Type);
				bucket.Add(Key.DelayMs, Event.DelayMs);
				bucket.Add(Key.FireOn, _fireOnUtcTicks);

				if (!Event.Data.IsUndefined())
				{
					var dataBucket = bucket.Nested(Key.Data);
					using var tracker = new DataModelReferenceTracker(dataBucket.Nested(Key.DataReferences));
					dataBucket.SetDataModelValue(tracker, Event.Data);
				}
			}

		#endregion

			private static IOutgoingEvent RestoreEvent(Bucket bucket)
			{
				var dataBucket = bucket.Nested(Key.Data);
				using var tracker = new DataModelReferenceTracker(dataBucket.Nested(Key.DataReferences));
				var name = bucket.GetString(Key.Name);

				return new EventEntity
					   {
							   SendId = bucket.GetSendId(Key.SendId),
							   NameParts = name is not null ? EventName.ToParts(name) : default,
							   Target = bucket.GetUri(Key.Target),
							   Type = bucket.GetUri(Key.Type),
							   DelayMs = bucket.GetInt32(Key.DelayMs),
							   Data = dataBucket.GetDataModelValue(tracker, baseValue: default)
					   };
			}
		}
	}
}