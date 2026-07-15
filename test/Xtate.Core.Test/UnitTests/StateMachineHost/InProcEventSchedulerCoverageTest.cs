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
using Xtate.Interpreter;
using Xtate.Logging;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;
using Xtate.TaskMonitor;

namespace Xtate.Test.UnitTests.StateMachineHost;

[TestClass]
public class InProcEventSchedulerCoverageTest
{
	[TestMethod]
	public async Task ScheduledEventDispatchesThroughFirstMatchingRouterAndRemovesItself()
	{
		var type = new FullUri("urn:processor");
		var miss = new Mock<IEventRouter>();
		var match = new Mock<IEventRouter>();
		match.Setup(r => r.CanHandle(type)).Returns(true);
		var monitor = new CapturingTaskMonitor();
		await using var scheduler = CreateScheduler([miss.Object, match.Object], monitor);
		var routerEvent = CreateRouterEvent(delayMs: 0, type, SendId.FromString("send-00000001"));

		await scheduler.ScheduleEvent(routerEvent, CancellationToken.None);
		await monitor.Tasks.Single().WaitAsync(TimeSpan.FromSeconds(5));

		miss.Verify(r => r.CanHandle(type), Times.Once);
		match.Verify(r => r.CanHandle(type), Times.Once);
		match.Verify(r => r.Dispatch(It.Is<ScheduledEvent>(e => e.SendId == routerEvent.SendId), It.IsAny<CancellationToken>()), Times.Once);
		await scheduler.CancelEvent(routerEvent.SendId!, CancellationToken.None);
	}

	[TestMethod]
	public async Task SchedulingFailureIsLoggedForMissingOriginAndUnknownRouter()
	{
		var logger = new Mock<ILogger<IEventScheduler>>();
		logger.Setup(static l => l.IsEnabled(Level.Error)).Returns(true);
		var monitor = new CapturingTaskMonitor();
		await using var scheduler = CreateScheduler([], monitor, logger.Object);

		await scheduler.ScheduleEvent(CreateRouterEvent(delayMs: 0, type: null, SendId.FromString("send-00000001")), CancellationToken.None);
		await scheduler.ScheduleEvent(CreateRouterEvent(delayMs: 0, new FullUri("urn:unknown"), SendId.FromString("send-00000002")), CancellationToken.None);
		await Task.WhenAll(monitor.Tasks).WaitAsync(TimeSpan.FromSeconds(5));

		var writes = logger.Invocations.Where(static invocation => invocation.Method.Name == nameof(ILogger<IEventScheduler>.Write)).ToArray();
		Assert.HasCount(expected: 2, writes);
		Assert.IsInstanceOfType<PlatformException>(writes[0].Arguments[^1]);
		Assert.IsInstanceOfType<ProcessorException>(writes[1].Arguments[^1]);
	}

	[TestMethod]
	public async Task DisabledErrorLoggingSuppressesWritesAfterDispatchFailure()
	{
		var type = new FullUri("urn:processor");
		var router = new Mock<IEventRouter>();
		router.Setup(r => r.CanHandle(type)).Returns(true);
		router.Setup(static r => r.Dispatch(It.IsAny<IRouterEvent>(), It.IsAny<CancellationToken>()))
			  .Returns(ValueTask.FromException(new InvalidOperationException("dispatch failed")));
		var logger = new Mock<ILogger<IEventScheduler>>();
		logger.Setup(static l => l.IsEnabled(Level.Error)).Returns(false);
		var monitor = new CapturingTaskMonitor();
		await using var scheduler = CreateScheduler([router.Object], monitor, logger.Object);

		await scheduler.ScheduleEvent(CreateRouterEvent(delayMs: 0, type, SendId.FromString("send-00000001")), CancellationToken.None);
		await monitor.Tasks.Single().WaitAsync(TimeSpan.FromSeconds(5));

		logger.Verify(static l => l.Write(Level.Error, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
	}

	[TestMethod]
	public async Task CancelEventCancelsEveryEventInSendIdGroupAndValidatesSendId()
	{
		var type = new FullUri("urn:processor");
		var router = new Mock<IEventRouter>();
		router.Setup(r => r.CanHandle(type)).Returns(true);
		var monitor = new CapturingTaskMonitor();
		await using var scheduler = CreateScheduler([router.Object], monitor);
		var sendId = SendId.FromString("send-00000001")!;

		await scheduler.ScheduleEvent(CreateRouterEvent(delayMs: 60_000, type, sendId), CancellationToken.None);
		await scheduler.ScheduleEvent(CreateRouterEvent(delayMs: 60_000, type, sendId), CancellationToken.None);
		await scheduler.CancelEvent(sendId, CancellationToken.None);
		await Assert.ThrowsExactlyAsync<TaskCanceledException>([ExcludeFromCodeCoverage] async () =>
			await Task.WhenAll(monitor.Tasks).WaitAsync(TimeSpan.FromSeconds(5)));
		await scheduler.CancelEvent(SendId.FromString("send-00000002")!, CancellationToken.None);

		router.Verify(static r => r.Dispatch(It.IsAny<IRouterEvent>(), It.IsAny<CancellationToken>()), Times.Never);
		await Assert.ThrowsExactlyAsync<ProcessorException>([ExcludeFromCodeCoverage] async () =>
			await scheduler.CancelEvent(SendId.FromString(string.Empty)!, CancellationToken.None));
	}

	[TestMethod]
	public async Task SynchronousAndAsynchronousDisposeCancelPendingEvents()
	{
		var type = new FullUri("urn:processor");
		var router = new Mock<IEventRouter>();
		var syncMonitor = new CapturingTaskMonitor();
		var syncScheduler = CreateScheduler([router.Object], syncMonitor);
		await syncScheduler.ScheduleEvent(CreateRouterEvent(delayMs: 60_000, type, sendId: null), CancellationToken.None);
		syncScheduler.Dispose();
		await Assert.ThrowsExactlyAsync<TaskCanceledException>([ExcludeFromCodeCoverage] async () =>
			await syncMonitor.Tasks.Single().WaitAsync(TimeSpan.FromSeconds(5)));

		var asyncMonitor = new CapturingTaskMonitor();
		var asyncScheduler = CreateScheduler([router.Object], asyncMonitor);
		await asyncScheduler.ScheduleEvent(CreateRouterEvent(delayMs: 60_000, type, SendId.FromString("send-00000001")), CancellationToken.None);
		await asyncScheduler.DisposeAsync();
		await Assert.ThrowsExactlyAsync<TaskCanceledException>([ExcludeFromCodeCoverage] async () =>
			await asyncMonitor.Tasks.Single().WaitAsync(TimeSpan.FromSeconds(5)));

		router.Verify(static r => r.Dispatch(It.IsAny<IRouterEvent>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	private static InProcEventScheduler CreateScheduler(
		IReadOnlyCollection<IEventRouter> routers,
		CapturingTaskMonitor monitor,
		ILogger<IEventScheduler>? logger = null) =>
		new()
		{
			EventRouters = routers,
			Logger = logger ?? Mock.Of<ILogger<IEventScheduler>>(),
			TaskMonitor = monitor
		};

	private static IRouterEvent CreateRouterEvent(int delayMs, FullUri? type, SendId? sendId)
	{
		var routerEvent = new Mock<IRouterEvent>();
		routerEvent.SetupGet(static e => e.DelayMs).Returns(delayMs);
		routerEvent.SetupGet(static e => e.OriginType).Returns(type);
		routerEvent.SetupGet(static e => e.SendId).Returns(sendId);
		routerEvent.SetupGet(static e => e.SenderServiceId).Returns(SessionId.FromString("sender"));

		return routerEvent.Object;
	}

	[ExcludeFromCodeCoverage]
	private sealed class CapturingTaskMonitor : ITaskMonitor
	{
		public List<Task> Tasks { get; } = [];

		public Task WaitAsync(Task task, CancellationToken token) => task;

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task;

		public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => valueTask;

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) => valueTask;

		public void Forget(Task task) => Tasks.Add(task);

		public void Forget(ValueTask valueTask) => Tasks.Add(valueTask.AsTask());

		public void Forget<TResult>(ValueTask<TResult> valueTask) => Tasks.Add(valueTask.AsTask());
	}
}
