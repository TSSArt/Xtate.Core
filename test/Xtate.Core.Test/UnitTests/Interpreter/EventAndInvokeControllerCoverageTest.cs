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

using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.Interpreter.Internal;
using Xtate.Interpreter.Services;
using Xtate.IoC.Tools;
using Xtate.Logging;
using Xtate.Scxml;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.Interpreter;

[TestClass]
public class EventAndInvokeControllerCoverageTest
{
	[TestMethod]
	public async Task EventControllerQueuesExplicitAndCommunicationSelectedInternalEvents()
	{
		var communication = new Mock<IExternalCommunication>();
		communication.Setup(static c => c.TrySend(It.IsAny<IOutgoingEvent>())).ReturnsAsync(SendStatus.ToInternalQueue);
		var queue = new EntityQueue<IIncomingEvent>();
		var controller = CreateEventController(communication.Object, queue);
		var explicitInternal = CreateOutgoingEvent(target: Const.InternalTarget, type: null, delayMs: 0);
		var selectedInternal = CreateOutgoingEvent(target: new FullUri("target"), type: new FullUri("processor"), delayMs: 0);

		await controller.Send(explicitInternal);
		await controller.Send(selectedInternal);

		communication.Verify(c => c.TrySend(explicitInternal), Times.Never);
		communication.Verify(c => c.TrySend(selectedInternal), Times.Once);
		Assert.AreEqual(expected: 2, queue.Count);
		Assert.IsTrue(queue.TryDequeue(out var first));
		Assert.AreEqual(EventType.Internal, first.Type);
		Assert.IsTrue(queue.TryDequeue(out var second));
		Assert.AreEqual(EventType.Internal, second.Type);
	}

	[TestMethod]
	public async Task EventControllerRejectsDelayedInternalEventsFromBothSelectionPaths()
	{
		var communication = new Mock<IExternalCommunication>();
		communication.Setup(static c => c.TrySend(It.IsAny<IOutgoingEvent>())).ReturnsAsync(SendStatus.ToInternalQueue);
		var controller = CreateEventController(communication.Object, new EntityQueue<IIncomingEvent>());

		await Assert.ThrowsExactlyAsync<ExecutionException>([ExcludeFromCodeCoverage] async () =>
			await controller.Send(CreateOutgoingEvent(Const.InternalTarget, type: null, delayMs: 1)));
		await Assert.ThrowsExactlyAsync<ExecutionException>([ExcludeFromCodeCoverage] async () =>
			await controller.Send(CreateOutgoingEvent(new FullUri("target"), new FullUri("processor"), delayMs: 1)));
	}

	[TestMethod]
	public async Task EventControllerLeavesSentAndScheduledEventsOutsideInternalQueue()
	{
		var communication = new Mock<IExternalCommunication>();
		communication.SetupSequence(static c => c.TrySend(It.IsAny<IOutgoingEvent>()))
					 .ReturnsAsync(SendStatus.Sent)
					 .ReturnsAsync(SendStatus.Scheduled);
		var queue = new EntityQueue<IIncomingEvent>();
		var controller = CreateEventController(communication.Object, queue);
		var outgoingEvent = CreateOutgoingEvent(new FullUri("target"), new FullUri("processor"), delayMs: 0);

		await controller.Send(outgoingEvent);
		await controller.Send(outgoingEvent);

		Assert.AreEqual(expected: 0, queue.Count);
	}

	[TestMethod]
	public async Task EventControllerWrapsCommunicationFailuresAndPreservesOwnedPlatformErrors()
	{
		var runtimeError = new StateMachineRuntimeError(new ScopeObject());
		var failure = new InvalidOperationException("send failed");
		var platform = runtimeError.PlatformError("platform");
		var communication = new Mock<IExternalCommunication>();
		communication.SetupSequence(static c => c.TrySend(It.IsAny<IOutgoingEvent>()))
					 .Returns(new ValueTask<SendStatus>(Task.FromException<SendStatus>(failure)))
					 .Returns(new ValueTask<SendStatus>(Task.FromException<SendStatus>(platform)));
		var controller = CreateEventController(communication.Object, new EntityQueue<IIncomingEvent>(), runtimeError);
		var outgoingEvent = CreateOutgoingEvent(new FullUri("target"), new FullUri("processor"), delayMs: 0);

		var wrapped = await Assert.ThrowsExactlyAsync<CommunicationException>([ExcludeFromCodeCoverage] async () => await controller.Send(outgoingEvent));
		var preserved = await Assert.ThrowsExactlyAsync<PlatformException>([ExcludeFromCodeCoverage] async () => await controller.Send(outgoingEvent));

		Assert.AreSame(failure, wrapped.InnerException);
		Assert.AreEqual(outgoingEvent.SendId, wrapped.SendId);
		Assert.AreSame(platform, preserved);
	}

	[TestMethod]
	public async Task EventControllerCancelsAndWrapsOnlyNonPlatformFailures()
	{
		var runtimeError = new StateMachineRuntimeError(new ScopeObject());
		var failure = new InvalidOperationException("cancel failed");
		var platform = runtimeError.PlatformError("platform");
		var communication = new Mock<IExternalCommunication>();
		communication.SetupSequence(static c => c.Cancel(It.IsAny<SendId>()))
					 .Returns(ValueTask.CompletedTask)
					 .Returns(ValueTask.FromException(failure))
					 .Returns(ValueTask.FromException(platform));
		var controller = CreateEventController(communication.Object, new EntityQueue<IIncomingEvent>(), runtimeError);
		var sendId = SendId.FromString("send-00000001")!;

		await controller.Cancel(sendId);
		var wrapped = await Assert.ThrowsExactlyAsync<CommunicationException>([ExcludeFromCodeCoverage] async () => await controller.Cancel(sendId));
		var preserved = await Assert.ThrowsExactlyAsync<PlatformException>([ExcludeFromCodeCoverage] async () => await controller.Cancel(sendId));

		Assert.AreSame(failure, wrapped.InnerException);
		Assert.AreEqual(sendId, wrapped.SendId);
		Assert.AreSame(platform, preserved);
	}

	[TestMethod]
	public async Task InvokeControllerForwardsStartCancelAndEventsAndWritesTraceEntries()
	{
		var manager = new Mock<IExternalServiceManager>();
		var logger = new Mock<ILogger<IInvokeController>>();
		var controller = CreateInvokeController(manager.Object, logger.Object);
		var invokeId = InvokeId.FromString("invoke");
		var invokeData = CreateInvokeData(invokeId);
		var incomingEvent = new IncomingEvent { Name = (EventName) "event", SendId = SendId.FromString("send-00000001") };

		await controller.Start(invokeData);
		await controller.Cancel(invokeId);
		await controller.Forward(invokeId, incomingEvent);

		manager.Verify(m => m.Start(invokeData), Times.Once);
		manager.Verify(m => m.Cancel(invokeId), Times.Once);
		manager.Verify(m => m.Forward(invokeId, incomingEvent), Times.Once);
		Assert.AreEqual(expected: 3, logger.Invocations.Count(static invocation => invocation.Method.Name == nameof(ILogger<IInvokeController>.Write)));
	}

	[TestMethod]
	public async Task InvokeControllerWrapsOrdinaryFailuresFromEveryOperation()
	{
		var manager = new Mock<IExternalServiceManager>();
		var startFailure = new InvalidOperationException("start");
		var cancelFailure = new InvalidOperationException("cancel");
		var forwardFailure = new InvalidOperationException("forward");
		manager.Setup(static m => m.Start(It.IsAny<InvokeData>())).Returns(ValueTask.FromException(startFailure));
		manager.Setup(static m => m.Cancel(It.IsAny<InvokeId>())).Returns(ValueTask.FromException(cancelFailure));
		manager.Setup(static m => m.Forward(It.IsAny<InvokeId>(), It.IsAny<IIncomingEvent>())).Returns(ValueTask.FromException(forwardFailure));
		var controller = CreateInvokeController(manager.Object);
		var invokeId = InvokeId.FromString("invoke");

		var start = await Assert.ThrowsExactlyAsync<CommunicationException>([ExcludeFromCodeCoverage] async () => await controller.Start(CreateInvokeData(invokeId)));
		var cancel = await Assert.ThrowsExactlyAsync<CommunicationException>([ExcludeFromCodeCoverage] async () => await controller.Cancel(invokeId));
		var forward = await Assert.ThrowsExactlyAsync<CommunicationException>([ExcludeFromCodeCoverage] async () => await controller.Forward(invokeId, Mock.Of<IIncomingEvent>()));

		Assert.AreSame(startFailure, start.InnerException);
		Assert.AreSame(cancelFailure, cancel.InnerException);
		Assert.AreSame(forwardFailure, forward.InnerException);
	}

	[TestMethod]
	public async Task InvokeControllerPreservesOwnedPlatformErrorsFromEveryOperation()
	{
		var runtimeError = new StateMachineRuntimeError(new ScopeObject());
		var platform = runtimeError.PlatformError("platform");
		var manager = new Mock<IExternalServiceManager>();
		manager.Setup(static m => m.Start(It.IsAny<InvokeData>())).Returns(ValueTask.FromException(platform));
		manager.Setup(static m => m.Cancel(It.IsAny<InvokeId>())).Returns(ValueTask.FromException(platform));
		manager.Setup(static m => m.Forward(It.IsAny<InvokeId>(), It.IsAny<IIncomingEvent>())).Returns(ValueTask.FromException(platform));
		var controller = CreateInvokeController(manager.Object, runtimeError: runtimeError);
		var invokeId = InvokeId.FromString("invoke");

		Assert.AreSame(platform, await Assert.ThrowsExactlyAsync<PlatformException>([ExcludeFromCodeCoverage] async () => await controller.Start(CreateInvokeData(invokeId))));
		Assert.AreSame(platform, await Assert.ThrowsExactlyAsync<PlatformException>([ExcludeFromCodeCoverage] async () => await controller.Cancel(invokeId)));
		Assert.AreSame(platform, await Assert.ThrowsExactlyAsync<PlatformException>([ExcludeFromCodeCoverage] async () => await controller.Forward(invokeId, Mock.Of<IIncomingEvent>())));
	}

	private static EventController CreateEventController(
		IExternalCommunication communication,
		EntityQueue<IIncomingEvent> queue,
		StateMachineRuntimeError? runtimeError = null) =>
		new()
		{
			ExternalCommunication = communication,
			Logger = Mock.Of<ILogger<IEventController>>(),
			StateMachineRuntimeError = runtimeError ?? new StateMachineRuntimeError(new ScopeObject()),
			StateMachineContext = Mock.Of<IStateMachineContext>(context => context.InternalQueue == queue)
		};

	private static InvokeController CreateInvokeController(
		IExternalServiceManager manager,
		ILogger<IInvokeController>? logger = null,
		StateMachineRuntimeError? runtimeError = null) =>
		new()
		{
			ExternalServiceManager = manager,
			Logger = logger ?? Mock.Of<ILogger<IInvokeController>>(),
			StateMachineRuntimeError = runtimeError ?? new StateMachineRuntimeError(new ScopeObject())
		};

	private static InvokeData CreateInvokeData(InvokeId invokeId) =>
		new(invokeId, new FullUri("urn:service"), Source: null, RawContent: null, DataModelValue.Undefined, DataModelValue.Undefined);

	private static IOutgoingEvent CreateOutgoingEvent(FullUri? target, FullUri? type, int delayMs) =>
		new OutgoingEvent
		{
			SendId = SendId.FromString("send-00000001"),
			Name = (EventName) "event",
			Target = target,
			Type = type,
			DelayMs = delayMs,
			Data = DataModelValue.Undefined
		};

	private sealed class OutgoingEvent : IOutgoingEvent
	{
		public SendId? SendId { get; init; }

		public EventName Name { get; init; }

		public FullUri? Target { get; init; }

		public FullUri? Type { get; init; }

		public int DelayMs { get; init; }

		public DataModelValue Data { get; init; }
	}
}
