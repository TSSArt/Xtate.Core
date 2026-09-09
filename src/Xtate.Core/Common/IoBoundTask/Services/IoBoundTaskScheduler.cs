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

using System.Diagnostics;

namespace Xtate.IoBoundTask.Services;

public class IoBoundTaskScheduler : TaskScheduler
{
	private const int NoWaitTimeout = 0;

	private const int IndefiniteTimeout = -1;

	private static readonly ConcurrentQueue<IoBoundTaskScheduler> SchedulerQueue = new();

	private static readonly Semaphore WaitingSemaphore = new(initialCount: 0, int.MaxValue);

	[ThreadStatic]
	private static IoBoundTaskScheduler? _executingScheduler;

	private readonly ConcurrentQueue<Task> _taskQueue = new();

	private int _availableWorkerSlots;

	static IoBoundTaskScheduler() => ThreadPool.RegisterWaitForSingleObject(WaitingSemaphore, StartNewWorker, state: null, IndefiniteTimeout, executeOnlyOnce: false);

	public IoBoundTaskScheduler(int maximumConcurrencyLevel = 256)
	{
		Infra.RequiresPositive(maximumConcurrencyLevel);

		MaximumConcurrencyLevel = _availableWorkerSlots = maximumConcurrencyLevel;
	}

	public static TimeSpan KeepAliveThreadTimeout
	{
		get;
		private set
		{
			if (value < TimeSpan.Zero || value > TimeSpan.FromSeconds(60))
			{
				throw new ArgumentOutOfRangeException(nameof(KeepAliveThreadTimeout), Resources.Exception_ValueMustBeBetween0and60Seconds);
			}

			field = value;
		}
	} = TimeSpan.FromSeconds(5);

	public override int MaximumConcurrencyLevel { get; }

	private static void StartNewWorker(object? _0, bool _1)
	{
		try
		{
			new Thread(WorkerThread) { IsBackground = true }.Start();
		}
		catch
		{
			WaitingSemaphore.Release();
		}
	}

	private static void WorkerThread()
	{
		var signaled = true;

		while (signaled)
		{
			if (SchedulerQueue.TryDequeue(out var scheduler))
			{
				scheduler.WorkerRunQueuedTasks();
			}

			signaled = WaitingSemaphore.WaitOne(NoWaitTimeout);

			if (!signaled && KeepAliveThreadTimeout > TimeSpan.Zero)
			{
				signaled = WaitingSemaphore.WaitOne(KeepAliveThreadTimeout);
			}
		}
	}

	private static void PublishSchedulerException(Exception ex) => Trace.TraceError(Resources.Exception_IoBoundTaskSchedulerInfrastructureFailure, ex);

	private void WorkerRunQueuedTasks()
	{
		try
		{
			ProcessTaskQueue();

			while (!_taskQueue.IsEmpty)
			{
				if (TryAcquireWorkerSlot())
				{
					ProcessTaskQueue();
				}
				else
				{
					break;
				}
			}
		}
		catch (Exception ex)
		{
			PublishSchedulerException(ex);

			throw;
		}
	}

	private void ProcessTaskQueue()
	{
		var previousScheduler = _executingScheduler;
		_executingScheduler = this;

		try
		{
			while (_taskQueue.TryDequeue(out var task))
			{
				TryExecuteTask(task);
			}
		}
		finally
		{
			_executingScheduler = previousScheduler;
			Interlocked.Increment(ref _availableWorkerSlots);
		}
	}

	protected override void QueueTask(Task task)
	{
		_taskQueue.Enqueue(task);

		if (TryAcquireWorkerSlot())
		{
			SchedulerQueue.Enqueue(this);

			WaitingSemaphore.Release();
		}
	}

	private bool TryAcquireWorkerSlot()
	{
		var initVal = _availableWorkerSlots;

		while (initVal > 0)
		{
			if (Interlocked.CompareExchange(ref _availableWorkerSlots, initVal - 1, initVal) is var oldVal && oldVal == initVal)
			{
				return true;
			}

			initVal = oldVal;
		}

		return false;
	}

	protected override IEnumerable<Task> GetScheduledTasks() => _taskQueue;

	protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => ReferenceEquals(_executingScheduler, this) && TryExecuteTask(task);
}
