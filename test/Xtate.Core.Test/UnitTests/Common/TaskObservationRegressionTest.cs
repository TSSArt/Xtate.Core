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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Threading;
using MonitoredTask = Xtate.TaskMonitor.Services.TaskMonitor;

namespace Xtate.Core.Test.UnitTests.Common;

[TestClass]
public class TaskObservationRegressionTest
{
	[TestMethod]
	[DataRow(0, false)]
	[DataRow(1, false)]
	[DataRow(2, false)]
	[DataRow(3, false)]
	[DataRow(0, true)]
	[DataRow(1, true)]
	[DataRow(2, true)]
	[DataRow(3, true)]
	public async Task CancelledWaitRetainsUnderlyingFailure(int shape, bool cancelAfterWaitStarts)
	{
		using var cancellation = new CancellationTokenSource();
		if (!cancelAfterWaitStarts)
		{
			cancellation.Cancel();
		}

		var source = new TrackingValueTaskSource<int>();
		var taskSource = new TaskCompletionSource<int>();
		var monitor = new RecordingMonitor { Logger = null! };
		var wait = shape switch
		{
			0 => monitor.WaitAsync((Task)taskSource.Task, cancellation.Token),
			1 => monitor.WaitAsync(taskSource.Task, cancellation.Token),
			2 => monitor.WaitAsync(source.ValueTask, cancellation.Token).AsTask(),
			_ => monitor.WaitAsync(source.GenericValueTask, cancellation.Token).AsTask()
		};
		cancellation.Cancel();
		var cancelled = await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await wait);
		Assert.AreEqual(cancellation.Token, cancelled.CancellationToken);

		var failure = new InvalidOperationException("late operation failure");
		if (shape < 2)
		{
			taskSource.SetException(failure);
		}
		else
		{
			source.SetException(failure);
			Assert.AreEqual(1, source.Consumptions);
			Assert.AreEqual(1, source.Registrations);
		}

		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		Assert.AreSame(failure, await monitor.Reported.Task.WaitAsync(timeout.Token));
		Assert.AreEqual(1, monitor.ReportCount);
	}

	[TestMethod]
	[DataRow(0)]
	[DataRow(1)]
	[DataRow(2)]
	[DataRow(3)]
	public async Task CancellationWinsButOriginalFaultsBeforeWaitContinuationResumes(int shape)
	{
		using var cancellation = new CancellationTokenSource();
		var original = new TaskCompletionSource<int>();
		var failure = new InvalidOperationException("completion raced cancelled wait");
		var transitionCount = 0;
		var monitor = new RecordingMonitor { Logger = null! };
		// ExecutionContext restoration runs this hook before the cancelled await resumes.
		// This makes the original complete between cancellation winning and observer selection.
		var gate = new AsyncLocal<Action?>(args =>
										   {
											   if (args.ThreadContextChanged)
											   {
												   args.CurrentValue?.Invoke();
											   }
										   })
				   {
					   Value = () =>
							   {
								   if (Interlocked.Exchange(ref transitionCount, 1) == 0)
								   {
									   original.TrySetException(failure);
								   }
							   }
				   };
		Task wait;
		try
		{
			wait = shape switch
			{
				0 => monitor.WaitAsync((Task)original.Task, cancellation.Token),
				1 => monitor.WaitAsync(original.Task, cancellation.Token),
				2 => monitor.WaitAsync(new ValueTask(original.Task), cancellation.Token).AsTask(),
				_ => monitor.WaitAsync(new ValueTask<int>(original.Task), cancellation.Token).AsTask()
			};
		}
		finally
		{
			gate.Value = null;
		}

		cancellation.Cancel();
		var actual = await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await wait);
		Assert.AreEqual(cancellation.Token, actual.CancellationToken);
		Assert.AreEqual(1, transitionCount);
		Assert.IsTrue(original.Task.IsFaulted);
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		Assert.AreSame(failure, await monitor.Reported.Task.WaitAsync(timeout.Token));
		Assert.AreEqual(1, monitor.ReportCount);
	}

	[TestMethod]
	[DataRow(0, false, false)]
	[DataRow(1, false, false)]
	[DataRow(2, false, false)]
	[DataRow(3, false, false)]
	[DataRow(0, true, false)]
	[DataRow(1, true, false)]
	[DataRow(2, true, false)]
	[DataRow(3, true, false)]
	[DataRow(0, false, true)]
	[DataRow(1, false, true)]
	[DataRow(2, false, true)]
	[DataRow(3, false, true)]
	[DataRow(0, true, true)]
	[DataRow(1, true, true)]
	[DataRow(2, true, true)]
	[DataRow(3, true, true)]
	public async Task CancelledWaitConsumesLateSuccessOrCancellation(int shape, bool underlyingCancellation, bool cancelAfterWaitStarts)
	{
		using var callerCancellation = new CancellationTokenSource();
		if (!cancelAfterWaitStarts)
		{
			callerCancellation.Cancel();
		}
		using var operationCancellation = new CancellationTokenSource();
		operationCancellation.Cancel();
		var taskSource = new TaskCompletionSource<int>();
		var valueSource = new TrackingValueTaskSource<int>();
		var monitor = new RecordingMonitor { Logger = null! };
		var wait = shape switch
		{
			0 => monitor.WaitAsync((Task)taskSource.Task, callerCancellation.Token),
			1 => monitor.WaitAsync(taskSource.Task, callerCancellation.Token),
			2 => monitor.WaitAsync(valueSource.ValueTask, callerCancellation.Token).AsTask(),
			_ => monitor.WaitAsync(valueSource.GenericValueTask, callerCancellation.Token).AsTask()
		};
		callerCancellation.Cancel();
		var cancelled = await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await wait);
		Assert.AreEqual(callerCancellation.Token, cancelled.CancellationToken);
		if (shape < 2)
		{
			if (underlyingCancellation)
			{
				taskSource.TrySetCanceled(operationCancellation.Token);
			}
			else
			{
				taskSource.SetResult(42);
			}
		}
		else
		{
			if (underlyingCancellation)
			{
				valueSource.SetException(new OperationCanceledException(operationCancellation.Token));
			}
			else
			{
				valueSource.SetResult(42);
			}

			Assert.AreEqual(1, valueSource.Consumptions);
		}

		if (underlyingCancellation)
		{
			using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
			var reported = await monitor.Reported.Task.WaitAsync(timeout.Token);
			Assert.AreEqual(operationCancellation.Token, ((OperationCanceledException)reported).CancellationToken);
			Assert.AreEqual(1, monitor.ReportCount);
		}
		else
		{
			Assert.AreEqual(0, monitor.ReportCount);
		}
	}

	[TestMethod]
	public async Task CancelledWaitDoesNotAwaitPendingDiagnostics()
	{
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		var operation = new TaskCompletionSource<int>();
		var report = new TrackingValueTaskSource<int>();
		var monitor = new RecordingMonitor { Logger = null!, Report = _ => report.ValueTask };
		var wait = monitor.WaitAsync(operation.Task, cancellation.Token);
		operation.SetException(new InvalidOperationException("late failure"));
		Assert.AreEqual(1, report.Registrations);
		Assert.AreEqual(0, report.Consumptions);
		await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await wait);
		report.SetResult(0);
		Assert.AreEqual(1, report.Consumptions);
	}

	[TestMethod]
	[DataRow(false, false, false)]
	[DataRow(false, true, false)]
	[DataRow(true, false, false)]
	[DataRow(true, true, false)]
	[DataRow(false, false, true)]
	[DataRow(false, true, true)]
	[DataRow(true, false, true)]
	[DataRow(true, true, true)]
	public void ForgetConsumesFaultedAndCancelledSourcesExactlyOnce(bool generic, bool pending, bool cancelled)
	{
		var source = new TrackingValueTaskSource<int>();
		Exception failure = cancelled ? new OperationCanceledException() : new InvalidOperationException("source fault");
		if (!pending)
		{
			source.SetException(failure);
		}

		var monitor = new RecordingMonitor { Logger = null! };
		void Forget()
		{
			if (generic)
			{
				monitor.Forget(source.GenericValueTask);
			}
			else
			{
				monitor.Forget(source.ValueTask);
			}
		}

		if (pending)
		{
			Forget();
			source.SetException(failure);
			Assert.AreSame(failure, monitor.Reported.Task.GetAwaiter().GetResult());
			Assert.AreEqual(1, monitor.ReportCount);
		}
		else
		{
			var actual = Assert.Throws<Exception>(Forget);
			Assert.AreSame(failure, actual);
			Assert.AreEqual(0, monitor.ReportCount);
		}

		Assert.AreEqual(1, source.Consumptions);
	}

	[TestMethod]
	[DataRow(false, false)]
	[DataRow(true, false)]
	[DataRow(false, true)]
	[DataRow(true, true)]
	public void ForgetConsumesSuccessfulSourceExactlyOnce(bool generic, bool pending)
	{
		var source = new TrackingValueTaskSource<int>();
		if (!pending)
		{
			source.SetResult(42);
		}

		var monitor = new RecordingMonitor { Logger = null! };
		if (generic)
		{
			monitor.Forget(source.GenericValueTask);
		}
		else
		{
			monitor.Forget(source.ValueTask);
		}

		if (pending)
		{
			source.SetResult(42);
		}

		Assert.AreEqual(1, source.Consumptions);
		Assert.AreEqual(0, monitor.ReportCount);
	}

	[TestMethod]
	[DataRow(false, false, 0)]
	[DataRow(false, true, 0)]
	[DataRow(true, false, 0)]
	[DataRow(true, true, 0)]
	[DataRow(false, false, 1)]
	[DataRow(false, true, 1)]
	[DataRow(true, false, 1)]
	[DataRow(true, true, 1)]
	[DataRow(false, false, 2)]
	[DataRow(false, true, 2)]
	[DataRow(true, false, 2)]
	[DataRow(true, true, 2)]
	public void ParameterlessForgetConsumesValueTaskSourceExactlyOnce(bool generic, bool pending, int outcome)
	{
		var source = new TrackingValueTaskSource<int>();
		void Complete()
		{
			switch (outcome)
			{
				case 0:
					source.SetResult(42);
					break;
				case 1:
					source.SetException(new InvalidOperationException("forgotten operation"));
					break;
				default:
					source.SetException(new OperationCanceledException());
					break;
			}
		}

		if (!pending)
		{
			Complete();
		}

		if (generic)
		{
			source.GenericValueTask.Forget();
		}
		else
		{
			source.ValueTask.Forget();
		}

		if (pending)
		{
			Assert.AreEqual(0, source.Consumptions);
			Complete();
		}

		Assert.AreEqual(1, source.Consumptions);
		Assert.AreEqual(pending ? 1 : 0, source.Registrations);
	}

	[TestMethod]
	[DataRow(false, false, false)]
	[DataRow(false, true, false)]
	[DataRow(true, false, false)]
	[DataRow(true, true, false)]
	[DataRow(false, false, true)]
	[DataRow(false, true, true)]
	[DataRow(true, false, true)]
	[DataRow(true, true, true)]
	public async Task LazyCancellationConsumesLateFactoryOutcome(bool withMonitor, bool initiallyCancelled, bool success)
	{
		using var cancellation = new CancellationTokenSource();
		if (initiallyCancelled)
		{
			cancellation.Cancel();
		}

		var monitor = new RecordingMonitor { Logger = null! };
		var source = new TrackingValueTaskSource<int>();
		var factoryCalls = 0;
		var lazy = new LazyTask<int>(() =>
		{
			factoryCalls++;

			return source.GenericValueTask;
		}, withMonitor ? monitor : null, cancellation.Token);
		var result = lazy.Task;
		Assert.AreSame(result, lazy.Task);
		Assert.AreEqual(1, factoryCalls);
		cancellation.Cancel();
		var cancelled = await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await result);
		Assert.AreEqual(cancellation.Token, cancelled.CancellationToken);
		var failure = new InvalidOperationException("late factory failure");
		if (success)
		{
			source.SetResult(42);
		}
		else
		{
			source.SetException(failure);
		}

		Assert.AreEqual(1, source.Consumptions);
		Assert.IsTrue(result.IsCanceled);
		if (withMonitor && !success)
		{
			using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
			Assert.AreSame(failure, await monitor.Reported.Task.WaitAsync(timeout.Token));
			Assert.AreEqual(1, monitor.ReportCount);
		}
		else
		{
			Assert.AreEqual(0, monitor.ReportCount);
		}
	}

	private sealed class RecordingMonitor : MonitoredTask
	{
		public Func<Exception, ValueTask>? Report { get; init; }

		public int ReportCount { get; private set; }

		public TaskCompletionSource<Exception> Reported { get; } = new();

		protected override ValueTask TaskFailed(Exception ex)
		{
			ReportCount++;
			Reported.TrySetResult(ex);

			return Report?.Invoke(ex) ?? ValueTask.CompletedTask;
		}

		protected override ValueTask TaskCancelled(OperationCanceledException ex) => TaskFailed(ex);
	}
}
