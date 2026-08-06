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

// ReSharper disable MethodHasAsyncOverload

using System.Threading;
using System.Globalization;
using Xtate.Logging;
using Xtate.Logging.Internal;
using MonitoredTask = Xtate.TaskMonitor.Services.TaskMonitor;

namespace Xtate.Core.Test.UnitTests.Common;

[TestClass]
public class TaskMonitorCoverageTest
{
	[TestMethod]
	public async Task WaitAsyncReturnsImmediateTasksAndHonorsAlreadyCancelledTokenForEveryShape()
	{
		var monitor = CreateMonitor();
		var completed = Task.CompletedTask;
		var completedResult = Task.FromResult(17);
		var pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var cancelled = new CancellationTokenSource();
		cancelled.Cancel();

		Assert.AreSame(completed, monitor.WaitAsync(completed, CancellationToken.None));
		Assert.AreSame(completedResult, monitor.WaitAsync(completedResult, CancellationToken.None));
		Assert.AreSame(pending.Task, monitor.WaitAsync(pending.Task, CancellationToken.None));
		Assert.AreEqual(expected: 23, await monitor.WaitAsync(new ValueTask<int>(23), CancellationToken.None));
		await monitor.WaitAsync(ValueTask.CompletedTask, CancellationToken.None);

		var cancelledTask = monitor.WaitAsync(pending.Task, cancelled.Token);
		var cancelledGenericTask = monitor.WaitAsync(GetPendingResultTask(), cancelled.Token);
		var cancelledValueTask = monitor.WaitAsync(new ValueTask(pending.Task), cancelled.Token);
		var cancelledGenericValueTask = monitor.WaitAsync(new ValueTask<int>(GetPendingResultTask()), cancelled.Token);

		await Assert.ThrowsExactlyAsync<TaskCanceledException>([ExcludeFromCodeCoverage] async () => await cancelledTask);
		await Assert.ThrowsExactlyAsync<TaskCanceledException>([ExcludeFromCodeCoverage] async () => await cancelledGenericTask);
		await Assert.ThrowsExactlyAsync<TaskCanceledException>([ExcludeFromCodeCoverage] async () => await cancelledValueTask);
		await Assert.ThrowsExactlyAsync<TaskCanceledException>([ExcludeFromCodeCoverage] async () => await cancelledGenericValueTask);
		pending.SetResult(true);

		return;

		static async Task<int> GetPendingResultTask()
		{
			await Task.Yield();
			await Task.Delay(Timeout.InfiniteTimeSpan, CancellationToken.None);

			return 0;
		}
	}

	[TestMethod]
	public async Task WaitAsyncContinuesMonitoringUnderlyingTaskAfterCallerCancellation()
	{
		var failedMonitor = CreateMonitor();
		var failedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var failedCancellation = new CancellationTokenSource();
		var failedWait = failedMonitor.WaitAsync(failedSource.Task, failedCancellation.Token);
		failedCancellation.Cancel();
		await Assert.ThrowsExactlyAsync<TaskCanceledException>([ExcludeFromCodeCoverage] async () => await failedWait);
		var expectedFailure = new InvalidOperationException("detached failure");
		failedSource.SetException(expectedFailure);

		Assert.AreSame(expectedFailure, await CompleteWithin(failedMonitor.Failed.Task));

		var cancelledMonitor = CreateMonitor();
		var cancelledSource = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var callerCancellation = new CancellationTokenSource();
		using var underlyingCancellation = new CancellationTokenSource();
		var cancelledWait = cancelledMonitor.WaitAsync(cancelledSource.Task, callerCancellation.Token);
		callerCancellation.Cancel();
		await Assert.ThrowsExactlyAsync<TaskCanceledException>([ExcludeFromCodeCoverage] async () => await cancelledWait);
		underlyingCancellation.Cancel();
		cancelledSource.TrySetCanceled(underlyingCancellation.Token);

		var cancellation = await CompleteWithin(cancelledMonitor.Cancelled.Task);
		Assert.AreEqual(underlyingCancellation.Token, cancellation.CancellationToken);
	}

	[TestMethod]
	public async Task ValueTaskWaitAndForgetMonitorCompletionFailureAndCancellationPaths()
	{
		var successMonitor = CreateMonitor();
		var successSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var waitCancellation = new CancellationTokenSource();
		var valueTaskWait = successMonitor.WaitAsync(new ValueTask(successSource.Task), waitCancellation.Token);
		successSource.SetResult(true);
		await valueTaskWait;
		successMonitor.Forget(Task.CompletedTask);
		successMonitor.Forget(ValueTask.CompletedTask);
		successMonitor.Forget(new ValueTask<int>(31));

		var failedMonitor = CreateMonitor();
		var failedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		failedMonitor.Forget(new ValueTask(failedSource.Task));
		var expectedFailure = new FormatException("value-task failure");
		failedSource.SetException(expectedFailure);
		Assert.AreSame(expectedFailure, await CompleteWithin(failedMonitor.Failed.Task));

		var cancelledMonitor = CreateMonitor();
		var cancelledSource = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
		cancelledMonitor.Forget(new ValueTask<int>(cancelledSource.Task));
		using var cancellationSource = new CancellationTokenSource();
		cancellationSource.Cancel();
		cancelledSource.TrySetCanceled(cancellationSource.Token);
		Assert.AreEqual(cancellationSource.Token, (await CompleteWithin(cancelledMonitor.Cancelled.Task)).CancellationToken);

		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => successMonitor.Forget(Task.FromException(new InvalidOperationException("task"))));
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => successMonitor.Forget(new ValueTask(Task.FromException(new InvalidOperationException("value task")))));
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() =>
												successMonitor.Forget(new ValueTask<int>(Task.FromException<int>(new InvalidOperationException("generic value task")))));
	}

	[TestMethod]
	public async Task BaseMonitorLogsDetachedTaskFailureAndCancellation()
	{
		var logger = new CapturingLogger();
		var monitor = new MonitoredTask { Logger = logger };
		var failed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		monitor.Forget(failed.Task);
		var exception = new InvalidOperationException("detached");
		failed.SetException(exception);
		var failureLog = await CompleteWithin(logger.Next.Task);
		Assert.AreEqual(Level.Error, failureLog.Level);
		Assert.AreSame(exception, failureLog.Entity);

		logger.Next = new TaskCompletionSource<LogEntry>(TaskCreationOptions.RunContinuationsAsynchronously);
		var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		monitor.Forget(cancelled.Task);
		using var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Cancel();
		Assert.IsTrue(cancelled.TrySetCanceled(cancellationTokenSource.Token));
		var cancellationLog = await CompleteWithin(logger.Next.Task);
		Assert.AreEqual(Level.Warning, cancellationLog.Level);
		Assert.IsInstanceOfType<OperationCanceledException>(cancellationLog.Entity);
	}

	private static TestTaskMonitor CreateMonitor() => new() { Logger = null! };

	private static async Task<T> CompleteWithin<T>(Task<T> task)
	{
		var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(10)));
		Assert.AreSame(task, completed, message: "The detached task-monitor callback did not complete within 10 seconds.");

		return await task;
	}

	private sealed class TestTaskMonitor : MonitoredTask
	{
		public TaskCompletionSource<OperationCanceledException> Cancelled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public TaskCompletionSource<Exception> Failed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		protected override ValueTask TaskCancelled(OperationCanceledException ex)
		{
			Cancelled.TrySetResult(ex);

			return ValueTask.CompletedTask;
		}

		protected override ValueTask TaskFailed(Exception ex)
		{
			Failed.TrySetResult(ex);

			return ValueTask.CompletedTask;
		}
	}

	private sealed class CapturingLogger : ILogger<MonitoredTask>
	{
		public TaskCompletionSource<LogEntry> Next { get; set; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

		public bool IsEnabled(Level level) => true;

		public ValueTask Write(Level level, int eventId, string? message)
		{
			Next.TrySetResult(new LogEntry(level, Entity: null));

			return ValueTask.CompletedTask;
		}

		public ValueTask Write<TEntity>(Level level, int eventId, string? message, TEntity entity)
		{
			Next.TrySetResult(new LogEntry(level, entity));

			return ValueTask.CompletedTask;
		}

		public ValueTask Write(Level level, int eventId, LoggingInterpolatedStringHandler formattedMessage) =>
			Write(level, eventId, formattedMessage.ToString(out _));

		public ValueTask Write<TEntity>(Level level, int eventId, LoggingInterpolatedStringHandler formattedMessage, TEntity entity) =>
			Write(level, eventId, formattedMessage.ToString(out _), entity);
	}

	private sealed record LogEntry(Level Level, object? Entity);
}
