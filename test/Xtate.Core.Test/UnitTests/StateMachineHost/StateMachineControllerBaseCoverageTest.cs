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

using System.Threading;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.IoC;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;
using Xtate.TaskMonitor;

namespace Xtate.Test.UnitTests.StateMachineHost;

[TestClass]
public class StateMachineControllerBaseCoverageTest
{
	[TestMethod]
	public async Task InitializeRunsInterpreterOnceCompletesResultAndDispatchesEvents()
	{
		var result = new DataModelValue("result");
		var interpreter = new Mock<IStateMachineInterpreter>();
		interpreter.Setup(static i => i.Run()).ReturnsAsync(result);
		var status = CreateAcceptedStatus();
		var dispatcher = new Mock<IEventDispatcher>();
		var monitor = new CapturingTaskMonitor();
		var controller = CreateController(interpreter.Object, status.Object, dispatcher.Object, monitor);

		await ((IAsyncInitialization) controller).InitializeAsync();
		await ((IAsyncInitialization) controller).InitializeAsync();
		await monitor.Tasks.Single();
		Assert.AreEqual(expected: "result", (await controller.GetResult()).AsString());
		interpreter.Verify(static i => i.Run(), Times.Once);
		status.Verify(static s => s.ForceCompleted(), Times.Once);

		var incomingEvent = Mock.Of<IIncomingEvent>();
		await controller.Dispatch(incomingEvent, CancellationToken.None);
		dispatcher.Verify(d => d.Dispatch(incomingEvent, CancellationToken.None), Times.Once);
	}

	[TestMethod]
	public async Task GetResultRequiresInitializationAndPropagatesInterpreterFailure()
	{
		var failure = new InvalidOperationException("interpreter failed");
		var interpreter = new Mock<IStateMachineInterpreter>();
		interpreter.Setup(static i => i.Run()).Returns(new ValueTask<DataModelValue>(Task.FromException<DataModelValue>(failure)));
		var status = CreateAcceptedStatus();
		var monitor = new CapturingTaskMonitor();
		var controller = CreateController(interpreter.Object, status.Object, Mock.Of<IEventDispatcher>(), monitor);

		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => controller.GetResult());
		await ((IAsyncInitialization) controller).InitializeAsync();
		var backgroundFailure = await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () => await monitor.Tasks.Single());
		var resultFailure = await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () => await controller.GetResult());

		Assert.AreSame(failure, backgroundFailure);
		Assert.AreSame(failure, resultFailure);
		status.Verify(s => s.ForceFailed(failure), Times.Once);
	}

	[TestMethod]
	public async Task CancellationMarksStatusAndCancelsResult()
	{
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		var interpreter = new Mock<IStateMachineInterpreter>();
		interpreter.Setup(static i => i.Run()).Returns(new ValueTask<DataModelValue>(Task.FromCanceled<DataModelValue>(cancellation.Token)));
		var status = CreateAcceptedStatus();
		var monitor = new CapturingTaskMonitor();
		var controller = CreateController(interpreter.Object, status.Object, Mock.Of<IEventDispatcher>(), monitor);

		await ((IAsyncInitialization) controller).InitializeAsync();
		await Assert.ThrowsExactlyAsync<TaskCanceledException>([ExcludeFromCodeCoverage] async () => await monitor.Tasks.Single());
		var cancelledResult = await Assert.ThrowsExactlyAsync<TaskCanceledException>([ExcludeFromCodeCoverage] async () => await controller.GetResult());

		Assert.AreEqual(cancellation.Token, cancelledResult.CancellationToken);
		status.Verify(s => s.ForceCancelled(cancellation.Token), Times.Once);
	}

	[TestMethod]
	public async Task DestroySignalsInterpreterAndAbsorbsDestroyedCompletion()
	{
		var destroyed = new StateMachineDestroyedException("destroyed") { Owner = new object(), Reason = DestroyReason.DestroySignal };
		var run = new TaskCompletionSource<DataModelValue>(TaskCreationOptions.RunContinuationsAsynchronously);
		var interpreter = new Mock<IStateMachineInterpreter>();
		interpreter.Setup(static i => i.Run()).Returns(new ValueTask<DataModelValue>(run.Task));
		interpreter.Setup(static i => i.TriggerDestroySignal()).Callback(() => run.TrySetException(destroyed));
		var monitor = new CapturingTaskMonitor();
		var controller = CreateController(interpreter.Object, CreateAcceptedStatus().Object, Mock.Of<IEventDispatcher>(), monitor);

		await ((IAsyncInitialization) controller).InitializeAsync();
		await controller.Destroy();
		await Assert.ThrowsExactlyAsync<StateMachineDestroyedException>([ExcludeFromCodeCoverage] async () => await monitor.Tasks.Single());

		interpreter.Verify(static i => i.TriggerDestroySignal(), Times.Once);
	}

	private static Mock<IStateMachineStatus> CreateAcceptedStatus()
	{
		var status = new Mock<IStateMachineStatus>();
		status.Setup(static s => s.WhenAccepted()).Returns(Task.CompletedTask);

		return status;
	}

	private static TestStateMachineController CreateController(IStateMachineInterpreter interpreter,
															   IStateMachineStatus status,
															   IEventDispatcher dispatcher,
															   ITaskMonitor monitor) =>
		new()
		{
			StateMachineInterpreter = interpreter,
			StateMachineStatus = status,
			EventDispatcher = dispatcher,
			TaskMonitor = monitor
		};

	private sealed class TestStateMachineController : StateMachineControllerBase;

	[ExcludeFromCodeCoverage]
	private sealed class CapturingTaskMonitor : ITaskMonitor
	{
		public List<Task> Tasks { get; } = [];

	#region Interface ITaskMonitor

		public Task WaitAsync(Task task, CancellationToken token) => task;

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task;

		public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => valueTask;

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) => valueTask;

		public void Forget(Task task) => Tasks.Add(task);

		public void Forget(ValueTask valueTask) => Tasks.Add(valueTask.AsTask());

		public void Forget<TResult>(ValueTask<TResult> valueTask) => Tasks.Add(valueTask.AsTask());

	#endregion
	}
}