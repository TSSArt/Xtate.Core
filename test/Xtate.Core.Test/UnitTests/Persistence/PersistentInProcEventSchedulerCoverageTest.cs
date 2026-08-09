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
using Xtate.IoC.Tools;
using Xtate.Logging;
using Xtate.Persistence;
using Xtate.Persistence.Services;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.TaskMonitor;

namespace Xtate.Core.Test.UnitTests.Persistence;

[TestClass]
public class PersistentEventSchedulerCoverageTest
{
	[TestMethod]
	public async Task CompactionKeepsSurvivingEventsAndNextRecord()
	{
		await using var storage = new TestTransactionalStorage();
		var type = new FullUri("urn:persistent-scheduler");
		var survivingId = SendId.FromString("surviving-event");
		var canceledId = SendId.FromString("canceled-event");
		var addedAfterCompactionId = SendId.FromString("added-after-compaction");
		var firstScheduler = CreateScheduler(storage, CreateRouter(type).Object, new CapturingTaskMonitor(), TimeSpan.Zero);
		await firstScheduler.InitializeAsync();

		await firstScheduler.ScheduleEvent(CreateRouterEvent(delayMs: 60_000, type, survivingId), CancellationToken.None);
		await firstScheduler.ScheduleEvent(CreateRouterEvent(delayMs: 60_000, type, canceledId), CancellationToken.None);
		await firstScheduler.CancelEvent(canceledId, CancellationToken.None);
		await firstScheduler.DisposeAsync();

		var resumedMonitor = new CapturingTaskMonitor();
		var resumedScheduler = CreateScheduler(storage, CreateRouter(type).Object, resumedMonitor, TimeSpan.Zero);
		await resumedScheduler.InitializeAsync();

		Assert.HasCount(expected: 1, resumedMonitor.Tasks);

		await resumedScheduler.ScheduleEvent(CreateRouterEvent(delayMs: 60_000, type, addedAfterCompactionId), CancellationToken.None);
		await resumedScheduler.DisposeAsync();

		var finalMonitor = new CapturingTaskMonitor();
		await using var finalScheduler = CreateScheduler(storage, CreateRouter(type).Object, finalMonitor, TimeSpan.Zero);
		await finalScheduler.InitializeAsync();

		Assert.HasCount(expected: 2, finalMonitor.Tasks);
	}

	[TestMethod]
	public async Task DispatchStartRemovesPersistedEventBeforeSchedulerDisposal()
	{
		await using var storage = new TestTransactionalStorage();
		var type = new FullUri("urn:persistent-scheduler");
		var sendId = SendId.FromString("interrupted-dispatch");
		var dispatchStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var firstRouter = CreateRouter(type);
		firstRouter
			.Setup(router => router.Dispatch(It.IsAny<IRouterEvent>(), It.IsAny<CancellationToken>()))
			.Returns((IRouterEvent _, CancellationToken cancellationToken) => WaitUntilCancelled(dispatchStarted, cancellationToken));
		var firstScheduler = CreateScheduler(storage, firstRouter.Object, new CapturingTaskMonitor(), TimeSpan.Zero);
		await firstScheduler.InitializeAsync();

		await firstScheduler.ScheduleEvent(CreateRouterEvent(delayMs: 0, type, sendId), CancellationToken.None);
		var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
		await dispatchStarted.Task.WaitAsync(cts.Token);
		await firstScheduler.DisposeAsync();

		Assert.AreEqual(expected: 2, storage.CheckPointCount);

		var resumedRouter = CreateRouter(type);
		await using var resumedScheduler = CreateScheduler(storage, resumedRouter.Object, new CapturingTaskMonitor(), TimeSpan.Zero);
		await resumedScheduler.InitializeAsync();

		resumedRouter.Verify(
			router => router.Dispatch(It.Is<IRouterEvent>(routerEvent => routerEvent.SendId == sendId), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[TestMethod]
	public async Task CancelEventCheckpointsAllEventsWithSameSendIdAsSingleBatch()
	{
		await using var storage = new TestTransactionalStorage();
		var type = new FullUri("urn:persistent-scheduler");
		var sendId = SendId.FromString("shared-send-id");
		await using var scheduler = CreateScheduler(storage, CreateRouter(type).Object, new CapturingTaskMonitor(), TimeSpan.Zero);
		await scheduler.InitializeAsync();

		await scheduler.ScheduleEvent(CreateRouterEvent(delayMs: 60_000, type, sendId), CancellationToken.None);
		await scheduler.ScheduleEvent(CreateRouterEvent(delayMs: 60_000, type, sendId), CancellationToken.None);
		await scheduler.CancelEvent(sendId, CancellationToken.None);

		Assert.AreEqual(expected: 3, storage.CheckPointCount);
	}

	[TestMethod]
	public async Task RestartDispatchesMissedEventsAndReschedulesFutureEvents()
	{
		await using var storage = new TestTransactionalStorage();
		var type = new FullUri("urn:persistent-scheduler");
		var missedId = SendId.FromString("missed-event");
		var futureId = SendId.FromString("future-event");
		var firstRouter = CreateRouter(type);
		var firstScheduler = CreateScheduler(storage, firstRouter.Object, new CapturingTaskMonitor(), TimeSpan.Zero);
		await firstScheduler.InitializeAsync();

		await firstScheduler.ScheduleEvent(CreateRouterEvent(delayMs: 60_000, type, missedId), CancellationToken.None);
		await firstScheduler.ScheduleEvent(CreateRouterEvent(delayMs: 180_000, type, futureId), CancellationToken.None);
		await firstScheduler.DisposeAsync();

		Assert.AreEqual(expected: 2, storage.CheckPointCount);

		var resumedRouter = CreateRouter(type);
		var resumedMonitor = new CapturingTaskMonitor();
		await using var resumedScheduler = CreateScheduler(storage, resumedRouter.Object, resumedMonitor, TimeSpan.FromMinutes(2));
		await resumedScheduler.InitializeAsync();

		Assert.HasCount(expected: 2, resumedMonitor.Tasks);
		resumedRouter.Verify(
			router => router.Dispatch(It.Is<IRouterEvent>(scheduledEvent => scheduledEvent.SendId == missedId), It.IsAny<CancellationToken>()),
			Times.Once);
		resumedRouter.Verify(
			router => router.Dispatch(It.Is<IRouterEvent>(scheduledEvent => scheduledEvent.SendId == futureId), It.IsAny<CancellationToken>()),
			Times.Never);

		await resumedScheduler.CancelEvent(futureId, CancellationToken.None);

		var finalMonitor = new CapturingTaskMonitor();
		await using var finalScheduler = CreateScheduler(storage, CreateRouter(type).Object, finalMonitor, TimeSpan.FromHours(1));
		await finalScheduler.InitializeAsync();

		Assert.IsEmpty(finalMonitor.Tasks);
	}

	private static Mock<IEventRouter> CreateRouter(FullUri type)
	{
		var router = new Mock<IEventRouter>();
		router.Setup(candidate => candidate.CanHandle(type)).Returns(true);

		return router;
	}

	private static async ValueTask WaitUntilCancelled(TaskCompletionSource dispatchStarted, CancellationToken cancellationToken)
	{
		dispatchStarted.SetResult();

		await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
	}

	private static IRouterEvent CreateRouterEvent(int delayMs, FullUri type, SendId sendId)
	{
		var routerEvent = new Mock<IRouterEvent>();
		routerEvent.SetupGet(static scheduledEvent => scheduledEvent.DelayMs).Returns(delayMs);
		routerEvent.SetupGet(static scheduledEvent => scheduledEvent.OriginType).Returns(type);
		routerEvent.SetupGet(static scheduledEvent => scheduledEvent.SendId).Returns(sendId);
		routerEvent.SetupGet(static scheduledEvent => scheduledEvent.SenderServiceId).Returns(SessionId.FromString("sender"));
		routerEvent.SetupGet(static scheduledEvent => scheduledEvent.Target).Returns(new FullUri("urn:target"));

		return routerEvent.Object;
	}

	private static TestPersistentEventScheduler CreateScheduler(ITransactionalStorage storage,
																IEventRouter router,
																CapturingTaskMonitor monitor,
																TimeSpan utcNowOffset) =>
		new(utcNowOffset)
		{
			Storage = storage,
			EventRouters = [router],
			Logger = Mock.Of<ILogger<IEventScheduler>>(),
			TaskMonitor = monitor,
			DisposeToken = new DisposeToken()
		};

	private sealed class TestPersistentEventScheduler(TimeSpan utcNowOffset) : PersistentEventScheduler
	{
		protected override TimeSpan GetDelay(PersistedScheduledEvent scheduledEvent) => scheduledEvent.FireOn - DateTime.UtcNow - utcNowOffset;
	}

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

	private sealed class TestTransactionalStorage : ITransactionalStorage
	{
		private readonly InMemoryStorage _storage = new(writeOnly: false);

		private int _checkPointCount;

		public int CheckPointCount => _checkPointCount;

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync() => ValueTask.CompletedTask;

	#endregion

	#region Interface IDisposable

		public void Dispose() { }

	#endregion

	#region Interface IStorage

		public ReadOnlyMemory<byte> Get(ReadOnlySpan<byte> key) => _storage.Get(key);

		public void Set(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value) => _storage.Set(key, value);

		public void Remove(ReadOnlySpan<byte> key) => _storage.Remove(key);

		public void RemoveAll(ReadOnlySpan<byte> prefix) => _storage.RemoveAll(prefix);

	#endregion

	#region Interface ITransactionalStorage

		public ValueTask CheckPoint(int level)
		{
			Interlocked.Increment(ref _checkPointCount);

			return ValueTask.CompletedTask;
		}

		public ValueTask Shrink() => ValueTask.CompletedTask;

	#endregion
	}
}