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

// ReSharper disable AccessToDisposedClosure

using System.Reflection;
using System.Threading;
using Xtate.IoBoundTask.Services;

namespace Xtate.Core.Test.UnitTests.Common;

[TestClass]
public class IoBoundTaskSchedulerCoverageTest
{
	[TestMethod]
	[DataRow(0)]
	[DataRow(-1)]
	public void NonPositiveConcurrencyIsRejected(int concurrency) =>
		Assert.Throws<ArgumentException>(() => new IoBoundTaskScheduler(concurrency));

	[TestMethod]
	[DoNotParallelize]
	public void KeepAliveTimeoutAcceptsEndpointsAndRejectsValuesOutsideSupportedRange()
	{
		var property = typeof(IoBoundTaskScheduler).GetProperty(nameof(IoBoundTaskScheduler.KeepAliveThreadTimeout))!;
		var original = IoBoundTaskScheduler.KeepAliveThreadTimeout;
		try
		{
			foreach (var value in new[] { TimeSpan.Zero, TimeSpan.FromSeconds(60) })
			{
				property.SetValue(null, value);
				Assert.AreEqual(value, IoBoundTaskScheduler.KeepAliveThreadTimeout);
			}
			foreach (var value in new[] { TimeSpan.FromTicks(-1), TimeSpan.FromSeconds(60) + TimeSpan.FromTicks(1) })
			{
				var error = Assert.ThrowsExactly<TargetInvocationException>(() => property.SetValue(null, value));
				Assert.IsInstanceOfType<ArgumentOutOfRangeException>(error.InnerException);
			}
		}
		finally
		{
			property.SetValue(null, original);
		}
	}

	[TestMethod]
	public async Task SchedulerHonorsConcurrencyLimitExposesQueueAndRecoversWorkerSlots()
	{
		var scheduler = new IoBoundTaskScheduler(maximumConcurrencyLevel: 2);
		var factory = new TaskFactory(scheduler);
		using var release = new ManualResetEventSlim(initialState: false);
		using var firstTwoStarted = new CountdownEvent(initialCount: 2);
		var startedCount = 0;
		var runningCount = 0;
		var peakConcurrency = 0;
		var tasks = Enumerable.Range(start: 0, count: 8)
							  .Select(_ => factory.StartNew(() =>
															{
																var running = Interlocked.Increment(ref runningCount);
																UpdateMaximum(ref peakConcurrency, running);

																if (Interlocked.Increment(ref startedCount) <= 2)
																{
																	firstTwoStarted.Signal();
																}

																try
																{
																	release.Wait();
																}
																finally
																{
																	Interlocked.Decrement(ref runningCount);
																}
															}))
							  .ToArray();

		try
		{
			Assert.IsTrue(firstTwoStarted.Wait(TimeSpan.FromSeconds(5)), message: "The scheduler did not reach its configured concurrency.");
			var scheduled = GetScheduledTasks(scheduler);
			Assert.IsTrue(scheduled.Length >= 6, message: "Tasks beyond the concurrency limit should remain visible to the debugger snapshot.");
			Assert.IsFalse(TryExecuteInline(scheduler, tasks[^1]), message: "A thread outside this scheduler must not inline its tasks.");
			Assert.AreEqual(expected: 2, scheduler.MaximumConcurrencyLevel);
		}
		finally
		{
			release.Set();
		}

		await CompletesWithin(Task.WhenAll(tasks), TimeSpan.FromSeconds(5));
		Assert.AreEqual(expected: 2, peakConcurrency);
		Assert.AreEqual(expected: 8, startedCount);
		Assert.IsFalse(GetScheduledTasks(scheduler).Any());

		var afterDrain = factory.StartNew(() =>
											  (ThreadId: Environment.CurrentManagedThreadId, Thread.CurrentThread.IsThreadPoolThread, Thread.CurrentThread.IsBackground));
		var worker = await CompletesWithin(afterDrain, TimeSpan.FromSeconds(5));
		Assert.AreNotEqual(notExpected: 0, worker.ThreadId);
		Assert.IsFalse(worker.IsThreadPoolThread, message: "Blocking I/O must execute on a dedicated thread rather than a ThreadPool thread.");
		Assert.IsTrue(worker.IsBackground, message: "A scheduler worker must not keep the process alive.");
	}

	[TestMethod]
	public async Task WarmWorkerDoesNotPreventNextBurstFromScalingUp()
	{
		const int concurrency = 4;
		var scheduler = new IoBoundTaskScheduler(concurrency);
		var factory = new TaskFactory(scheduler);
		await CompletesWithin(factory.StartNew(() => { }), TimeSpan.FromSeconds(5));

		await Task.Delay(TimeSpan.FromMilliseconds(250));

		using var release = new ManualResetEventSlim(initialState: false);
		using var allStarted = new CountdownEvent(concurrency);
		var tasks = Enumerable.Range(start: 0, concurrency)
							  .Select(_ => factory.StartNew(() =>
															{
																allStarted.Signal();
																release.Wait();
															}))
							  .ToArray();

		try
		{
			Assert.IsTrue(allStarted.Wait(TimeSpan.FromSeconds(4)), message: "A warm worker serialized a burst instead of allowing scale-up.");
		}
		finally
		{
			release.Set();
		}

		await CompletesWithin(Task.WhenAll(tasks), TimeSpan.FromSeconds(5));
	}

	[TestMethod]
	public async Task SchedulerWorkerInlinesNestedTaskAndAvoidsSingleThreadDeadlock()
	{
		var scheduler = new IoBoundTaskScheduler(maximumConcurrencyLevel: 1);
		var factory = new TaskFactory(scheduler);
		var parent = factory.StartNew(() =>
									  {
										  var parentThread = Environment.CurrentManagedThreadId;
										  var child = factory.StartNew(() => Environment.CurrentManagedThreadId);
										  child.Wait();

										  return (ParentThread: parentThread, ChildThread: child.Result);
									  });

		var result = await CompletesWithin(parent, TimeSpan.FromSeconds(5));
		Assert.AreEqual(result.ParentThread, result.ChildThread, message: "The synchronously awaited child should inline on the scheduler worker.");
	}

	[TestMethod]
	public async Task FaultedTaskIsCapturedAndDoesNotStopQueueDraining()
	{
		var factory = new TaskFactory(new IoBoundTaskScheduler(maximumConcurrencyLevel: 1));
		var faulted = factory.StartNew([ExcludeFromCodeCoverage]() => throw new InvalidOperationException("Expected task failure."));
		var following = factory.StartNew(() => 42);

		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => faulted);
		Assert.AreEqual(expected: 42, await CompletesWithin(following, TimeSpan.FromSeconds(5)));
	}

	private static async Task CompletesWithin(Task task, TimeSpan timeout)
	{
		var completed = await Task.WhenAny(task, Task.Delay(timeout));
		Assert.AreSame(task, completed, message: "The scheduler task did not complete before the timeout.");
		await task;
	}

	private static async Task<T> CompletesWithin<T>(Task<T> task, TimeSpan timeout)
	{
		await CompletesWithin((Task)task, timeout);

		return await task;
	}

	private static void UpdateMaximum(ref int maximum, int candidate)
	{
		var current = Volatile.Read(ref maximum);

		while (candidate > current)
		{
			var observed = Interlocked.CompareExchange(ref maximum, candidate, current);

			if (observed == current)
			{
				return;
			}

			current = observed;
		}
	}

	private static Task[] GetScheduledTasks(IoBoundTaskScheduler scheduler)
	{
		var method = typeof(IoBoundTaskScheduler).GetMethod(name: "GetScheduledTasks", BindingFlags.Instance | BindingFlags.NonPublic)!;

		return [.. (IEnumerable<Task>)method.Invoke(scheduler, parameters: null)!];
	}

	private static bool TryExecuteInline(IoBoundTaskScheduler scheduler, Task task)
	{
		var method = typeof(IoBoundTaskScheduler).GetMethod(name: "TryExecuteTaskInline", BindingFlags.Instance | BindingFlags.NonPublic)!;

		return (bool)method.Invoke(scheduler, [task, true])!;
	}
}
