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
using Xtate.Class;
using Xtate.DataModel;
using Xtate.DataModel.Services;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.IoC;
using Xtate.Logging;
using Xtate.Scxml;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;

namespace Xtate.Test.UnitTests.StateMachineHost;

[TestClass]
public class ExternalServiceExecutionCoverageTest
{
	[TestMethod]
	public async Task ExternalServiceClassForwardsInvokeContextAndRegistersEveryForwardingService()
	{
		var invokeId = InvokeId.FromString("invoke", "unique-invoke");
		var invokeData = new InvokeData(
			invokeId,
			new FullUri("urn:service"),
			new Uri("child.scxml", UriKind.Relative),
			"raw",
			new DataModelValue("content"),
			new DataModelValue("parameters"));
		var targetDispatcher = new Mock<IEventDispatcher>();
		var sessionId = SessionId.FromString("session");
		var location = new Uri("https://example.test/parent.scxml");
		var serviceClass = new ExternalServiceClass(
			invokeData,
			targetDispatcher.Object,
			Mock.Of<IStateMachineSessionId>(s => s.SessionId == sessionId),
			Mock.Of<IStateMachineLocation>(l => l.Location == location),
			Mock.Of<ICaseSensitivity>(c => c.CaseInsensitive));

		Assert.AreSame(invokeId, ((IExternalServiceInvokeId) serviceClass).InvokeId);
		Assert.AreEqual(invokeData.Type, ((IExternalServiceType) serviceClass).Type);
		Assert.AreEqual(invokeData.Source, ((IExternalServiceSource) serviceClass).Source);
		Assert.AreEqual(invokeData.RawContent, ((IExternalServiceSource) serviceClass).RawContent);
		Assert.AreEqual(expected: "content", ((IExternalServiceSource) serviceClass).Content.AsString());
		Assert.AreEqual(expected: "parameters", ((IExternalServiceParameters) serviceClass).Parameters.AsString());
		Assert.AreSame(sessionId, ((IStateMachineSessionId) serviceClass).SessionId);
		Assert.AreEqual(location, ((IStateMachineLocation) serviceClass).Location);
		Assert.IsTrue(((ICaseSensitivity) serviceClass).CaseInsensitive);

		var sourceEvent = Mock.Of<IIncomingEvent>();
		await ((IParentEventDispatcher) serviceClass).Dispatch(sourceEvent, CancellationToken.None);
		await ((IParentEventDispatcher) serviceClass).Dispatch(sourceEvent, CancellationToken.None);
		targetDispatcher.Verify(d => d.Dispatch(
			It.Is<IncomingEvent>(e => e.Type == EventType.External &&
									 e.OriginType == invokeData.Type &&
									 e.Origin == new FullUri("#_invoke") &&
									 e.InvokeId == invokeId),
			CancellationToken.None), Times.Exactly(2));

		var services = new ServiceCollection();
		serviceClass.AddServices(services);
		var provider = services.BuildProvider();
		Assert.AreSame(serviceClass, await provider.GetRequiredService<IStateMachineSessionId>());
		Assert.AreSame(serviceClass, await provider.GetRequiredService<IStateMachineLocation>());
		Assert.AreSame(serviceClass, await provider.GetRequiredService<ICaseSensitivity>());
		Assert.AreSame(serviceClass, await provider.GetRequiredService<IExternalServiceInvokeId>());
		Assert.AreSame(serviceClass, await provider.GetRequiredService<IExternalServiceType>());
		Assert.AreSame(serviceClass, await provider.GetRequiredService<IExternalServiceSource>());
		Assert.AreSame(serviceClass, await provider.GetRequiredService<IExternalServiceParameters>());
		Assert.AreSame(serviceClass, await provider.GetRequiredService<IParentEventDispatcher>());
	}

	[TestMethod]
	public async Task RunnerSendsDoneEventOnceAndPreservesResult()
	{
		var invokeId = InvokeId.FromString("invoke");
		var service = new Mock<IExternalService>();
		service.Setup(static s => s.GetResult()).ReturnsAsync(new DataModelValue("result"));
		var sentEvents = new List<IOutgoingEvent>();
		var communication = CreateCommunication(sentEvents, SendStatus.Sent);
		var runner = CreateRunner(invokeId, service.Object, communication.Object, Mock.Of<ILogger<ExternalServiceRunner>>());

		await runner.WaitForCompletion();
		await runner.WaitForCompletion();

		service.Verify(static s => s.GetResult(), Times.Once);
		Assert.HasCount(expected: 1, sentEvents);
		Assert.AreEqual(EventName.GetDoneInvokeName(invokeId).ToString(), sentEvents[0].Name.ToString());
		Assert.AreEqual(expected: "result", sentEvents[0].Data.AsString());
		Assert.AreEqual(Const.ScxmlIoProcessorId, sentEvents[0].Type);
		Assert.AreEqual(Const.ParentTarget, sentEvents[0].Target);
	}

	[TestMethod]
	public async Task RunnerConvertsServiceExceptionToErrorExecutionEvent()
	{
		var failure = new InvalidOperationException("service failed");
		var service = new Mock<IExternalService>();
		service.Setup(static s => s.GetResult()).Returns(new ValueTask<DataModelValue>(Task.FromException<DataModelValue>(failure)));
		var sentEvents = new List<IOutgoingEvent>();
		var communication = CreateCommunication(sentEvents, SendStatus.Sent);
		var runner = CreateRunner(InvokeId.FromString("invoke"), service.Object, communication.Object, Mock.Of<ILogger<ExternalServiceRunner>>());

		await runner.WaitForCompletion();

		Assert.HasCount(expected: 1, sentEvents);
		Assert.AreEqual(EventName.ErrorExecution.ToString(), sentEvents[0].Name.ToString());
		Assert.AreNotEqual(DataModelValue.Undefined, sentEvents[0].Data);
	}

	[TestMethod]
	public async Task RunnerLogsOriginalAndSecondaryFailuresWhenErrorEventCannotBeSent()
	{
		var service = new Mock<IExternalService>();
		service.Setup(static s => s.GetResult()).ReturnsAsync(new DataModelValue("result"));
		var communication = new Mock<IExternalCommunication>();
		communication.Setup(static c => c.TrySend(It.IsAny<IOutgoingEvent>())).ReturnsAsync(SendStatus.Scheduled);
		var logger = new Mock<ILogger<ExternalServiceRunner>>();
		var runner = CreateRunner(InvokeId.FromString("invoke"), service.Object, communication.Object, logger.Object);

		await runner.WaitForCompletion();

		communication.Verify(static c => c.TrySend(It.IsAny<IOutgoingEvent>()), Times.Exactly(2));
		logger.Verify(static l => l.Write(Level.Error, eventId: 1, "Service Execution error.", It.IsAny<Exception>()), Times.Once);
		logger.Verify(static l => l.Write(Level.Error, eventId: 2, "Error on sending error to Parent.", It.IsAny<Exception>()), Times.Once);
	}

	[TestMethod]
	public async Task StateMachineHostStartsInEnumerationOrderAndStopsInReverseOrder()
	{
		var calls = new List<string>();
		var first = new Mock<IIoProcessorHost>();
		var second = new Mock<IIoProcessorHost>();
		first.Setup(static h => h.Start()).Returns(() => Record(calls, "start-first"));
		first.Setup(static h => h.Stop()).Returns(() => Record(calls, "stop-first"));
		second.Setup(static h => h.Start()).Returns(() => Record(calls, "start-second"));
		second.Setup(static h => h.Stop()).Returns(() => Record(calls, "stop-second"));
		IStateMachineHostNew host = new StateMachineHostNew { IoProcessorHosts = ToAsyncEnumerable(first.Object, second.Object) };

		await host.Start();
		await host.Stop();
		await host.Stop();

		CollectionAssert.AreEqual(new[] { "start-first", "start-second", "stop-second", "stop-first" }, calls);
	}

	private static ExternalServiceRunner CreateRunner(
		InvokeId invokeId,
		IExternalService service,
		IExternalCommunication communication,
		ILogger<ExternalServiceRunner> logger) =>
		new(Mock.Of<IExternalServiceInvokeId>(i => i.InvokeId == invokeId))
		{
			ExternalService = service,
			DataConverter = new DataConverter(caseSensitivity: null),
			ExternalCommunication = communication,
			Logger = logger
		};

	private static Mock<IExternalCommunication> CreateCommunication(List<IOutgoingEvent> events, SendStatus status)
	{
		var communication = new Mock<IExternalCommunication>();
		communication.Setup(static c => c.TrySend(It.IsAny<IOutgoingEvent>()))
					 .Returns((IOutgoingEvent outgoingEvent) =>
							  {
								  events.Add(outgoingEvent);

								  return new ValueTask<SendStatus>(status);
							  });

		return communication;
	}

	private static ValueTask Record(List<string> calls, string value)
	{
		calls.Add(value);

		return ValueTask.CompletedTask;
	}

	private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(params T[] items)
	{
		foreach (var item in items)
		{
			yield return item;
			await Task.Yield();
		}
	}
}
