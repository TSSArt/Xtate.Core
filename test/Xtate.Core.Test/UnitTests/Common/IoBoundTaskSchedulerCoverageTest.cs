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

using System.Reflection;
using System.Threading;
using Xtate.IoBoundTask.Services;

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class IoBoundTaskSchedulerCoverageTest
{
	[TestMethod]
	public async Task SchedulerQueuesBehindConcurrencyLimitExposesQueueAndNeverExecutesInline()
	{
		var scheduler = new IoBoundTaskScheduler(maximumConcurrencyLevel: 1);
		var factory = new TaskFactory(scheduler);
		using var started = new ManualResetEventSlim(initialState: false);
		using var release = new ManualResetEventSlim(initialState: false);
		var executionThreads = new List<int>();
		var first = factory.StartNew(
			() =>
			{
				lock (executionThreads)
				{
					executionThreads.Add(Environment.CurrentManagedThreadId);
				}

				started.Set();
				release.Wait();
			});

		Assert.IsTrue(started.Wait(TimeSpan.FromSeconds(5)), "The first scheduler worker did not start.");
		var second = factory.StartNew(
			() =>
			{
				lock (executionThreads)
				{
					executionThreads.Add(Environment.CurrentManagedThreadId);
				}
			});

		try
		{
			var scheduled = GetScheduledTasks(scheduler);
			Assert.IsTrue(scheduled.Contains(second));
			Assert.IsFalse(TryExecuteInline(scheduler, second));
			Assert.AreEqual(expected: 1, scheduler.MaximumConcurrencyLevel);
			Assert.AreEqual(TimeSpan.FromSeconds(5), IoBoundTaskScheduler.KeepAliveThreadTimeout);
		}
		finally
		{
			release.Set();
		}

		await Task.WhenAll(first, second);
		Assert.HasCount(expected: 2, executionThreads);
		Assert.IsFalse(GetScheduledTasks(scheduler).Any());
	}

	[TestMethod]
	public void SchedulerRejectsInvalidConcurrencyAndKeepAliveRanges()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>([ExcludeFromCodeCoverage]() => new IoBoundTaskScheduler(maximumConcurrencyLevel: 0));
		var property = typeof(IoBoundTaskScheduler).GetProperty(nameof(IoBoundTaskScheduler.KeepAliveThreadTimeout))!;

		var negative = Assert.ThrowsExactly<TargetInvocationException>([ExcludeFromCodeCoverage]() => property.SetValue(obj: null, TimeSpan.FromMilliseconds(-1)));
		var excessive = Assert.ThrowsExactly<TargetInvocationException>([ExcludeFromCodeCoverage]() => property.SetValue(obj: null, TimeSpan.FromSeconds(61)));
		Assert.IsInstanceOfType<ArgumentOutOfRangeException>(negative.InnerException);
		Assert.IsInstanceOfType<ArgumentOutOfRangeException>(excessive.InnerException);
	}

	private static Task[] GetScheduledTasks(IoBoundTaskScheduler scheduler)
	{
		var method = typeof(IoBoundTaskScheduler).GetMethod("GetScheduledTasks", BindingFlags.Instance | BindingFlags.NonPublic)!;
		return ((IEnumerable<Task>) method.Invoke(scheduler, parameters: null)!).ToArray();
	}

	private static bool TryExecuteInline(IoBoundTaskScheduler scheduler, Task task)
	{
		var method = typeof(IoBoundTaskScheduler).GetMethod("TryExecuteTaskInline", BindingFlags.Instance | BindingFlags.NonPublic)!;
		return (bool) method.Invoke(scheduler, [task, true])!;
	}
}
