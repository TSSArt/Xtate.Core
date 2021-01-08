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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.Core
{
	[PublicAPI]
	public sealed class IoBoundTaskScheduler : TaskScheduler
	{
		private const int NoWaitTimeout     = 0;
		private const int IndefiniteTimeout = -1;

		private static readonly ConcurrentQueue<IoBoundTaskScheduler> SchedulerQueue   = new();
		private static readonly Semaphore                             WaitingSemaphore = new(initialCount: 0, int.MaxValue);

		private static RegisteredWaitHandle? _startNewWorkerRegisteredWaitHandle;
		private static int                   _waitingThreadCount;

		private ConcurrentQueue<Task>? _taskQueue;
		private Semaphore?             _taskSemaphore;

		static IoBoundTaskScheduler() => RegisterStartNewWorker();

		public IoBoundTaskScheduler(int maximumConcurrencyLevel)
		{
			if (maximumConcurrencyLevel <= 0) throw new ArgumentOutOfRangeException(nameof(maximumConcurrencyLevel));

			MaximumConcurrencyLevel = maximumConcurrencyLevel;
		}

		public static int KeepAliveThreadTimeout { get; private set; } = 30000;

		public override int MaximumConcurrencyLevel { get; }

		public static void SetKeepAliveThreadTimeout(int value)
		{
			if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));

			KeepAliveThreadTimeout = value;
		}

		private static void RegisterStartNewWorker()
		{
			var newHandle = ThreadPool.RegisterWaitForSingleObject(WaitingSemaphore, StartNewWorker, state: default, IndefiniteTimeout, executeOnlyOnce: false);

			if (Interlocked.Exchange(ref _startNewWorkerRegisteredWaitHandle, newHandle) is { } oldHandle)
			{
				oldHandle.Unregister(WaitingSemaphore);
			}
		}

		private static void StartNewWorker(object? o, bool b)
		{
			var thread = new Thread(WorkerThread) { IsBackground = true };

			thread.Start();
		}

		private static void UnregisterStartNewWorker()
		{
			if (Interlocked.Exchange(ref _startNewWorkerRegisteredWaitHandle, value: default) is { } oldHandle)
			{
				oldHandle.Unregister(WaitingSemaphore);
			}
		}

		private static void WorkerThread()
		{
			var signaled = true;

			try
			{
				while (signaled)
				{
					if (SchedulerQueue.TryDequeue(out var scheduler))
					{
						scheduler.WorkerRunQueuedTasks();
					}

					signaled = WaitingSemaphore.WaitOne(NoWaitTimeout);

					if (signaled || KeepAliveThreadTimeout == 0)
					{
						continue;
					}

					UnregisterStartNewWorker();

					Interlocked.Increment(ref _waitingThreadCount);

					try
					{
						signaled = WaitingSemaphore.WaitOne(KeepAliveThreadTimeout);
					}
					finally
					{
						Interlocked.Decrement(ref _waitingThreadCount);
					}
				}
			}
			finally
			{
				if (Interlocked.CompareExchange(ref _waitingThreadCount, value: 0, comparand: 0) == 0)
				{
					RegisterStartNewWorker();
				}
			}
		}

		private static void PublishSchedulerException(Exception _) { }

		private void WorkerRunQueuedTasks()
		{
			try
			{
				ProcessTaskQueue();

				Infrastructure.NotNull(_taskQueue);

				while (!_taskQueue.IsEmpty)
				{
					Infrastructure.NotNull(_taskSemaphore);

					if (_taskSemaphore.WaitOne(NoWaitTimeout))
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
			Infrastructure.NotNull(_taskSemaphore);

			try
			{
				Infrastructure.NotNull(_taskQueue);

				while (_taskQueue.TryDequeue(out var task))
				{
					TryExecuteTask(task);
				}
			}
			finally
			{
				_taskSemaphore.Release();
			}
		}

		protected override void QueueTask(Task task)
		{
			if (_taskQueue is not { } taskQueue)
			{
				taskQueue = new ConcurrentQueue<Task>();
				Interlocked.CompareExchange(ref _taskQueue, taskQueue, comparand: null);
			}

			if (_taskSemaphore is not { } taskSemaphore)
			{
				taskSemaphore = new Semaphore(MaximumConcurrencyLevel, MaximumConcurrencyLevel);

				if (Interlocked.CompareExchange(ref _taskSemaphore, taskSemaphore, comparand: null) is { } currentTaskSemaphore)
				{
					taskSemaphore.Dispose();
					taskSemaphore = currentTaskSemaphore;
				}
			}

			taskQueue.Enqueue(task);

			if (taskSemaphore.WaitOne(NoWaitTimeout))
			{
				SchedulerQueue.Enqueue(this);

				WaitingSemaphore.Release();
			}
		}

		protected override IEnumerable<Task> GetScheduledTasks() => _taskQueue is { } queue ? queue : Array.Empty<Task>();

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;
	}
}