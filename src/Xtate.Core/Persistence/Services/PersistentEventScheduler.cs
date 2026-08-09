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

using Xtate.IoC;
using Xtate.Persistence.Extensions;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;

namespace Xtate.Persistence.Services;

[InstantiatedByIoC]
public class PersistentEventScheduler : InProcEventScheduler, IAsyncInitialization
{
	private const int Operation = 0;

	private const int Add = 1;

	private const int Remove = 2;

	private const int Event = 3;

	private const int RefId = 4;

	private readonly Bucket _bucket;

	private readonly AsyncInit<PersistentEventScheduler> _init = new(scheduler => scheduler.Init());

	private readonly SemaphoreSlim _lock = new(initialCount: 1, maxCount: 1);

	private int _record;

	public required ITransactionalStorage Storage
	{
		private get;
		[SetByIoC]
		init
		{
			_bucket = new Bucket(value);
			field = value;
		}
	}

#region Interface IAsyncInitialization

	public ValueTask InitializeAsync() => AsyncInit.For(this).Run(_init);

#endregion

	public override async ValueTask ScheduleEvent(IRouterEvent routerEvent, CancellationToken token)
	{
		if (routerEvent is not PersistedScheduledEvent scheduledEvent)
		{
			scheduledEvent = new PersistedScheduledEvent(routerEvent);
		}

		await AddEvent(scheduledEvent, token).ConfigureAwait(false);

		await base.ScheduleEvent(scheduledEvent, token).ConfigureAwait(false);
	}

	protected override async ValueTask CancelScheduledEvents(IReadOnlyList<ScheduledEvent> scheduledEvents, CancellationToken token)
	{
		try
		{
			await RemoveEvents(scheduledEvents.Cast<PersistedScheduledEvent>(), token).ConfigureAwait(false);
		}
		finally
		{
			await base.CancelScheduledEvents(scheduledEvents, token).ConfigureAwait(false);
		}
	}

	private async ValueTask Init()
	{
		var refIds = new HashSet<int>();
		var shrink = false;

		while (true)
		{
			var recordBucket = _bucket.Nested(_record);

			if (!recordBucket.TryGet(Operation, out int operation))
			{
				break;
			}

			switch (operation)
			{
				case Add:
					refIds.Add(_record);

					break;

				case Remove:
					shrink = true;
					refIds.Remove(recordBucket.GetInt32(RefId));

					break;

				default:
					throw new PersistenceException(Resources.Exception_IncorrectDataFormat);
			}

			_record++;
		}

		var events = new List<PersistedScheduledEvent>(refIds.Count);

		foreach (var refId in refIds)
		{
			events.Add(new PersistedScheduledEvent(_bucket.Nested(refId).Nested(Event)) { RefId = refId });
		}

		if (shrink)
		{
			_bucket.RemoveSubtree(Bucket.RootKey);
			_record = 0;

			foreach (var scheduledEvent in events)
			{
				scheduledEvent.RefId = _record++;
				var recordBucket = _bucket.Nested(scheduledEvent.RefId);
				recordBucket.Add(Operation, Add);
				scheduledEvent.Store(recordBucket.Nested(Event));
			}

			await Storage.CheckPoint(level: 0).ConfigureAwait(false);
			await Storage.Shrink().ConfigureAwait(false);
		}

		foreach (var scheduledEvent in events)
		{
			await base.ScheduleEvent(scheduledEvent, CancellationToken.None).ConfigureAwait(false);
		}
	}

	protected virtual TimeSpan GetDelay(PersistedScheduledEvent scheduledEvent) => scheduledEvent.FireOn - DateTime.UtcNow;

	protected override async Task WaitForDispatch(ScheduledEvent scheduledEvent)
	{
		var delay = GetDelay((PersistedScheduledEvent)scheduledEvent);

		if (delay > TimeSpan.Zero)
		{
			await Task.Delay(delay, scheduledEvent.CancellationToken).ConfigureAwait(false);
		}

		await RemoveEvents([(PersistedScheduledEvent)scheduledEvent], CancellationToken.None).ConfigureAwait(false);
	}

	private async ValueTask AddEvent(PersistedScheduledEvent scheduledEvent, CancellationToken token)
	{
		await _lock.WaitAsync(token).ConfigureAwait(false);

		try
		{
			scheduledEvent.RefId = _record++;

			var recordBucket = _bucket.Nested(scheduledEvent.RefId);
			recordBucket.Add(Operation, Add);
			scheduledEvent.Store(recordBucket.Nested(Event));

			try
			{
				await Storage.CheckPoint(level: 0).ConfigureAwait(false);
			}
			catch
			{
				_bucket.RemoveSubtree(--_record);

				throw;
			}
		}
		finally
		{
			_lock.Release();
		}
	}

	private async ValueTask RemoveEvents(IEnumerable<PersistedScheduledEvent> scheduledEventList, CancellationToken token)
	{
		await _lock.WaitAsync(token).ConfigureAwait(false);

		var count = 0;

		try
		{
			foreach (var scheduledEvent in scheduledEventList)
			{
				count++;
				var recordBucket = _bucket.Nested(_record++);
				recordBucket.Add(Operation, Remove);
				recordBucket.Add(RefId, scheduledEvent.RefId);
			}

			try
			{
				await Storage.CheckPoint(level: 0).ConfigureAwait(false);
			}
			catch
			{
				for (var i = 0; i < count; i++)
				{
					_bucket.RemoveSubtree(--_record);
				}

				throw;
			}
		}
		finally
		{
			_lock.Release();
		}
	}
}
