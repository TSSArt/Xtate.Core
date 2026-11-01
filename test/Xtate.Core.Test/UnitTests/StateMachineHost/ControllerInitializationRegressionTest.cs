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
using Xtate.Core.Test.UnitTests.Common;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.Logging;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;
using MonitoredTask = Xtate.TaskMonitor.Services.TaskMonitor;

namespace Xtate.Core.Test.UnitTests.StateMachineHost;

[TestClass]
public class ControllerInitializationRegressionTest
{
	[TestMethod]
	[DataRow(false, false)]
	[DataRow(true, false)]
	[DataRow(false, true)]
	[DataRow(true, true)]
	public async Task FailedStartupPreservesOutcomeAndReportsPendingExecution(bool delayed, bool cancelled)
	{
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Exception failure = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("startup failed");
		var source = new TrackingValueTaskSource<DataModelValue>();
		if (!delayed)
		{
			source.SetException(failure);
		}

		var status = new StateMachineStatus();
		var logger = new Mock<ILogger<MonitoredTask>>();
		var reported = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
		logger.Setup(item => item.Write(It.IsAny<Level>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Exception>()))
			  .Callback<Level, int, string?, Exception>((_, _, _, exception) => reported.TrySetResult(exception))
			  .Returns(ValueTask.CompletedTask);
		var controller = CreateController(source.GenericValueTask, status, logger.Object);
		var startup = controller.InitializeAsync().AsTask();
		if (delayed)
		{
			Assert.IsFalse(startup.IsCompleted);
			source.SetException(failure);
		}

		if (cancelled)
		{
			var actual = await Assert.ThrowsAsync<OperationCanceledException>(async () => await startup);
			Assert.AreEqual(cancellation.Token, actual.CancellationToken);
			Assert.IsTrue(startup.IsCanceled);
		}
		else
		{
			var actual = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await startup);
			Assert.AreSame(failure, actual);
			Assert.IsTrue(status.WhenAccepted().IsFaulted);
		}

		if (delayed)
		{
			using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
			var reportedException = await reported.Task.WaitAsync(timeout.Token);
			if (cancelled)
			{
				Assert.AreEqual(cancellation.Token, ((OperationCanceledException)reportedException).CancellationToken);
			}
			else
			{
				Assert.AreSame(failure, reportedException);
			}
		}
		Assert.AreEqual(1, source.Consumptions);
		// Completed execution propagates directly; pending execution is reported by the monitor.
		logger.Verify(item => item.Write(It.IsAny<Level>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Exception>()), delayed ? Times.Once() : Times.Never());
	}

	[TestMethod]
	public async Task AcceptanceExportsHandleWhileExecutionRemainsPending()
	{
		var source = new TrackingValueTaskSource<DataModelValue>();
		var status = new StateMachineStatus();
		var controller = CreateController(source.GenericValueTask, status, Mock.Of<ILogger<MonitoredTask>>());
		var startup = controller.InitializeAsync().AsTask();
		Assert.IsFalse(startup.IsCompleted);
		await status.OnChanged(StateMachineInterpreterState.Accepted);
		await startup;
		var result = controller.GetResult().AsTask();
		Assert.IsFalse(result.IsCompleted);
		source.SetResult(new DataModelValue("completed"));
		Assert.AreEqual("completed", (await result).AsString());
		Assert.AreEqual(1, source.Consumptions);
	}

	[TestMethod]
	public async Task SuccessfulExecutionCanCompleteBeforeExplicitAcceptance()
	{
		var controller = CreateController(new ValueTask<DataModelValue>(new DataModelValue("completed")), new StateMachineStatus(), Mock.Of<ILogger<MonitoredTask>>());
		Assert.ThrowsExactly<InvalidOperationException>(() => controller.GetResult());
		await controller.InitializeAsync();
		Assert.AreEqual("completed", (await controller.GetResult()).AsString());
	}

	private static TestController CreateController(ValueTask<DataModelValue> execution, StateMachineStatus status, ILogger<MonitoredTask> logger)
	{
		var interpreter = new Mock<IStateMachineInterpreter>();
		interpreter.Setup(static item => item.Run()).Returns(execution);

		return new TestController
		{
			StateMachineStatus = status,
			StateMachineInterpreter = interpreter.Object,
			EventDispatcher = Mock.Of<IEventDispatcher>(),
			TaskMonitor = new MonitoredTask { Logger = logger }
		};
	}

	private sealed class TestController : StateMachineControllerBase;
}
