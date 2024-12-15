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

namespace Xtate.Persistence;

internal sealed class PersistedEventScheduler(IStorageProvider storageProvider, IHostEventDispatcher hostEventDispatcher, IEventSchedulerLogger logger)
	: InProcEventScheduler
{
	private const string HostPartition = "StateMachineHost";

	private const string PersistedEventSchedulerKey = "scheduler";

	private const int ScheduledEventsKey = 0;

	private readonly SemaphoreSlim _lockScheduledEvents = new(initialCount: 1, maxCount: 1);

	private readonly HashSet<PersistedScheduledEvent> _scheduledEvents = [];

	private int _recordId;

	private int _scheduledEventRecordId;

	private ITransactionalStorage _storage = default!;

	protected async ValueTask<ScheduledEvent> CreateScheduledEvent(IRouterEvent routerEvent, CancellationToken token)
	{
		var persistedScheduledEvent = new PersistedScheduledEvent(this, routerEvent);

		await _lockScheduledEvents.WaitAsync(token).ConfigureAwait(false);

		try
		{
			_scheduledEvents.Add(persistedScheduledEvent);

			persistedScheduledEvent.RecordId = _scheduledEventRecordId ++;

			var rootBucket = new Bucket(_storage).Nested(ScheduledEventsKey);
			rootBucket.Add(Bucket.RootKey, _scheduledEventRecordId);
			persistedScheduledEvent.Store(rootBucket.Nested(persistedScheduledEvent.RecordId));

			await _storage.CheckPoint(level: 0).ConfigureAwait(false);
		}
		finally
		{
			_lockScheduledEvents.Release();
		}

		return persistedScheduledEvent;
	}

	private async ValueTask DeleteEvent(PersistedScheduledEvent persistedScheduledEvent, CancellationToken token)
	{
		if (persistedScheduledEvent is null) throw new ArgumentNullException(nameof(persistedScheduledEvent));

		await _lockScheduledEvents.WaitAsync(token).ConfigureAwait(false);

		try
		{
			_scheduledEvents.Remove(persistedScheduledEvent);

			var rootBucket = new Bucket(_storage).Nested(ScheduledEventsKey);
			rootBucket.RemoveSubtree(persistedScheduledEvent.RecordId);

			await _storage.CheckPoint(level: 0).ConfigureAwait(false);

			await ShrinkScheduledEvents().ConfigureAwait(false);
		}
		finally
		{
			_lockScheduledEvents.Release();
		}
	}

	private async ValueTask ShrinkScheduledEvents()
	{
		if (_scheduledEvents.Count * 2 > _recordId)
		{
			return;
		}

		_recordId = 0;
		var rootBucket = new Bucket(_storage).Nested(ScheduledEventsKey);
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

		await _storage.CheckPoint(level: 0).ConfigureAwait(false);
		await _storage.Shrink().ConfigureAwait(false);
	}

	public async ValueTask Initialize(CancellationToken token)
	{
		_storage = await storageProvider.GetTransactionalStorage(HostPartition, PersistedEventSchedulerKey).ConfigureAwait(false);

		await LoadScheduledEvents(_storage, token).ConfigureAwait(false);

		_lockScheduledEvents.Release();
	}

	private async ValueTask LoadScheduledEvents(IStorage storage, CancellationToken token)
	{
		var bucket = new Bucket(storage).Nested(ScheduledEventsKey);

		bucket.TryGet(Bucket.RootKey, out _recordId);

		for (var i = 0; i < _recordId; i ++)
		{
			var eventBucket = bucket.Nested(i);

			if (eventBucket.TryGet(Key.TypeInfo, out TypeInfo typeInfo) && typeInfo == TypeInfo.ScheduledEvent)
			{
				var scheduledEvent = new PersistedScheduledEvent(this, eventBucket) { RecordId = i };

				_scheduledEvents.Add(scheduledEvent);

				await ScheduleEvent(scheduledEvent, token).ConfigureAwait(false);
			}
		}
	}

	private sealed class PersistedScheduledEvent : ScheduledEvent, IAsyncDisposable
	{
		private readonly PersistedEventScheduler _eventScheduler;

		private readonly long _fireOnUtcTicks;

		public PersistedScheduledEvent(PersistedEventScheduler eventScheduler, IRouterEvent routerEvent) : base(routerEvent)
		{
			_eventScheduler = eventScheduler;

			_fireOnUtcTicks = DateTime.UtcNow.Ticks + routerEvent.DelayMs * TimeSpan.TicksPerMillisecond;
		}

		public PersistedScheduledEvent(PersistedEventScheduler eventScheduler, in Bucket bucket) : base(bucket)
		{
			_eventScheduler = eventScheduler;

			bucket.TryGet(Key.FireOn, out long fireOnUtcTicks);

			var delayMs = (int) ((fireOnUtcTicks - DateTime.UtcNow.Ticks) / TimeSpan.TicksPerMillisecond);
			DelayMs = delayMs > 1 ? delayMs : 1;
		}

		public int RecordId { get; set; }

		protected override TypeInfo TypeInfo => TypeInfo.ScheduledEvent;

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync() => _eventScheduler.DeleteEvent(this, token: default);

	#endregion

		public override void Store(Bucket bucket)
		{
			bucket.Add(Key.FireOn, _fireOnUtcTicks);
		}
	}
}