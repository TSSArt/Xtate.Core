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

using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.IoC.Options;
using Xtate.IoC.Tools;
using Xtate.IoProcessors;
using Xtate.IoProcessors.Http;
using Xtate.IoProcessors.Http.Internal;
using Xtate.IoProcessors.Http.Services;
using Xtate.IoProcessors.NamedPipe;
using Xtate.IoProcessors.NamedPipe.Services;
using Xtate.Logging;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.TaskMonitor;

namespace Xtate.Test.UnitTests.IoProcessors;

[TestClass]
public class IoProcessorAndHostCoverageTest
{
	[TestMethod]
	public async Task IoProcessorHostStartsOnceStopsAndCanRestartWithFreshToken()
	{
		using var disposeToken = new DisposingToken();
		var monitor = new CapturingTaskMonitor();
		var host = new TestIoProcessorHost
				   {
					   DisposeTokenBase = new DisposeToken(disposeToken.Token),
					   TaskMonitorBase = monitor
				   };
		IIoProcessorHost ioProcessorHost = host;

		Assert.IsTrue(host.CurrentToken.IsCancellationRequested);
		await ioProcessorHost.Stop();
		await ioProcessorHost.Start();
		await monitor.LastTask!;
		var firstToken = host.ObservedTokens.Single();
		Assert.IsFalse(firstToken.IsCancellationRequested);
		await ioProcessorHost.Start();
		Assert.AreEqual(expected: 1, host.BackgroundCount);

		await ioProcessorHost.Stop();
		Assert.IsTrue(firstToken.IsCancellationRequested);
		Assert.IsTrue(host.CurrentToken.IsCancellationRequested);
		await ioProcessorHost.Start();
		await monitor.LastTask!;
		Assert.AreEqual(expected: 2, host.BackgroundCount);
		Assert.AreNotEqual(firstToken, host.ObservedTokens[1]);
		await ioProcessorHost.Stop();
	}

	[TestMethod]
	public async Task ResilientHostLogsFailureRetriesAndReturnsAfterSuccess()
	{
		using var disposeToken = new DisposingToken();
		var monitor = new CapturingTaskMonitor();
		var logger = new Mock<ILogger<ResilientIoProcessorHostBase>>();
		var host = new TestResilientHost(failuresBeforeSuccess: 1)
				   {
					   DisposeTokenBase = new DisposeToken(disposeToken.Token),
					   TaskMonitorBase = monitor,
					   LoggerBase = logger.Object
				   };

		await ((IIoProcessorHost) host).Start();
		await monitor.LastTask!.WaitAsync(TimeSpan.FromSeconds(5));

		Assert.AreEqual(expected: 2, host.AttemptCount);
		logger.Verify(static l => l.Write(Level.Error, eventId: 1, It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
		await ((IIoProcessorHost) host).Stop();
	}

	[TestMethod]
	public async Task ResilientHostTreatsOwnedCancellationAsNormalShutdown()
	{
		using var disposeToken = new DisposingToken();
		var monitor = new CapturingTaskMonitor();
		var logger = new Mock<ILogger<ResilientIoProcessorHostBase>>();
		var host = new TestResilientHost(failuresBeforeSuccess: -1)
				   {
					   DisposeTokenBase = new DisposeToken(disposeToken.Token),
					   TaskMonitorBase = monitor,
					   LoggerBase = logger.Object
				   };

		await ((IIoProcessorHost) host).Start();
		await host.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
		await ((IIoProcessorHost) host).Stop();
		await monitor.LastTask!.WaitAsync(TimeSpan.FromSeconds(5));

		logger.Verify(static l => l.Write(Level.Error, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
	}

	[TestMethod]
	public async Task HttpIoProcessorBuildsTargetsAndRoutesLocalAndRemoteEvents()
	{
		var handler = new CapturingHttpMessageHandler();
		var controller = CreateHttpController(handler);
		var internalDispatcher = new Mock<IInternalEventDispatcher<HttpIoProcessor>>();
		var sessionId = SessionId.FromString("session");
		var processor = CreateHttpIoProcessor(controller, internalDispatcher.Object, invokeId: null, sessionId);
		var ioProcessor = (IIoProcessor) processor;
		var router = (IEventRouter) processor;

		Assert.AreEqual(new FullUri("https://localhost:8443/root/session/session"), ioProcessor.Target);
		Assert.IsTrue(router.CanHandle(new FullUri("http")));
		Assert.IsTrue(router.CanHandle(new FullUri("http://www.w3.org/TR/scxml/#BasicHTTPEventProcessor")));

		var localEvent = CreateRouterEvent(controller.ToSessionTarget(SessionId.FromString("target")), sessionId);
		await router.Dispatch(localEvent, CancellationToken.None);
		internalDispatcher.Verify(d => d.Dispatch(It.Is<SessionId>(id => id.Value == "target"), localEvent, CancellationToken.None), Times.Once);

		var remoteEvent = CreateRouterEvent(new FullUri("https://remote.test/events"), sessionId, name: "remote.event", data: new DataModelValue("payload"));
		await router.Dispatch(remoteEvent, CancellationToken.None);
		Assert.AreEqual(new Uri("https://remote.test/events?_scxmleventname=remote.event"), handler.RequestUri);
		Assert.AreEqual(expected: "payload", handler.Content);
		Assert.AreEqual(expected: "https://localhost:8443/root/session/session", handler.Origin);

		var invokeProcessor = CreateHttpIoProcessor(controller, internalDispatcher.Object, InvokeId.FromString("invoke"), sessionId);
		Assert.AreEqual(new FullUri("https://localhost:8443/root/invoke/invoke"), ((IIoProcessor) invokeProcessor).Target);
	}

	[TestMethod]
	public async Task HttpIoProcessorBuildsTargetsAndRoutesLocalAndUnnamedRemoteEvents()
	{
		var handler = new CapturingHttpMessageHandler();
		var controller = CreateHttpController(handler);
		var internalDispatcher = new Mock<IInternalEventDispatcher<HttpIoProcessor>>();
		var sessionId = SessionId.FromString("session");
		var processor = CreateHttpIoProcessor(controller, internalDispatcher.Object, invokeId: null, sessionId);
		var ioProcessor = (IIoProcessor) processor;
		var router = (IEventRouter) processor;

		Assert.AreEqual(new FullUri("https://localhost:8443/root/session/session"), ioProcessor.Target);
		Assert.IsTrue(router.CanHandle(new FullUri("http")));
		Assert.IsTrue(router.CanHandle(new FullUri("http://www.w3.org/TR/scxml/#BasicHTTPEventProcessor")));

		var localEvent = CreateRouterEvent(controller.ToSessionTarget(SessionId.FromString("target")), sessionId);
		await router.Dispatch(localEvent, CancellationToken.None);
		internalDispatcher.Verify(d => d.Dispatch(It.Is<SessionId>(id => id.Value == "target"), localEvent, CancellationToken.None), Times.Once);

		var remoteEvent = CreateRouterEvent(new FullUri("https://remote.test/events"), sessionId, data: new DataModelValue("payload"));
		await router.Dispatch(remoteEvent, CancellationToken.None);
		Assert.AreEqual(new Uri("https://remote.test/events"), handler.RequestUri);
		Assert.AreEqual(expected: "payload", handler.Content);
		Assert.AreEqual(expected: "https://localhost:8443/root/session/session", handler.Origin);

		var invokeProcessor = CreateHttpIoProcessor(controller, internalDispatcher.Object, InvokeId.FromString("invoke"), sessionId);
		Assert.AreEqual(new FullUri("https://localhost:8443/root/invoke/invoke"), ((IIoProcessor) invokeProcessor).Target);
	}

	[TestMethod]
	public async Task HttpIoProcessorRejectsDelayedAndMissingTargetEvents()
	{
		var processor = CreateHttpIoProcessor(CreateHttpController(new CapturingHttpMessageHandler()), Mock.Of<IInternalEventDispatcher<HttpIoProcessor>>(), null, SessionId.FromString("session"));
		var router = (IEventRouter) processor;

		await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () =>
			await router.Dispatch(CreateRouterEvent(new FullUri("https://remote.test/"), SessionId.FromString("session"), delayMs: 1), CancellationToken.None));
		await Assert.ThrowsExactlyAsync<ProcessorException>([ExcludeFromCodeCoverage] async () =>
			await router.Dispatch(CreateRouterEvent(target: null, SessionId.FromString("session")), CancellationToken.None));
	}

	[TestMethod]
	public async Task HttpControllerMatchesTargetsSerializesContentAndAddsRoutingHeaders()
	{
		var handler = new CapturingHttpMessageHandler();
		var controller = CreateHttpController(handler);
		var sessionId = SessionId.FromString("sender");

		Assert.IsTrue(controller.TryMatchTarget(controller.ToSessionTarget(SessionId.FromString("target")), out var targetSession));
		Assert.IsInstanceOfType<SessionId>(targetSession);
		Assert.AreEqual(expected: "target", targetSession.Value);
		Assert.IsTrue(controller.TryMatchTarget(controller.ToInvokeTarget(InvokeId.FromString("invoke")), out var targetInvoke));
		Assert.IsInstanceOfType<InvokeId>(targetInvoke);
		Assert.IsFalse(controller.TryMatchTarget(new Uri("http://localhost:8443/root/session/target"), out _));
		Assert.IsFalse(controller.TryMatchTarget(new Uri("https://other.test:8443/root/session/target"), out _));
		Assert.IsFalse(controller.TryMatchTarget(new Uri("https://localhost:9443/root/session/target"), out _));
		Assert.IsFalse(controller.TryMatchTarget(new Uri("https://localhost:8443/other/session/target"), out _));
		Assert.IsFalse(controller.TryMatchTarget(new Uri("https://localhost:8443/root/unknown/target"), out _));
		Assert.IsFalse(controller.TryMatchTarget(new Uri("https://localhost:8443/root/session"), out _));

		var data = new DataModelList();
		data.Add("key", "value");
		var formEvent = CreateRouterEvent(
			new FullUri("https://remote.test/form"),
			sessionId,
			name: "form.event",
			data: data,
			sendId: SendId.FromString("send-00000001"),
			invokeId: InvokeId.FromString("source"));
		await controller.SendEvent(formEvent.Target!, formEvent, CancellationToken.None);

		Assert.AreEqual(expected: "_scxmleventname=form.event&key=value", handler.Content);
		Assert.AreEqual(expected: "application/x-www-form-urlencoded", handler.ContentType);
		Assert.AreEqual(expected: "send-00000001", handler.SendId);
		Assert.AreEqual(expected: "source", handler.InvokeId);
	}

	[TestMethod]
	public async Task HttpControllerCoversEmptyScalarXmlOversizeAndFailedResponsePaths()
	{
		var handler = new CapturingHttpMessageHandler();
		var controller = CreateHttpController(handler);
		var sender = InvokeId.FromString("sender");
		await controller.SendEvent(new FullUri("https://remote.test/empty"), CreateRouterEvent(new FullUri("https://remote.test/empty"), sender), CancellationToken.None);
		Assert.IsNull(handler.Content);

		await controller.SendEvent(new FullUri("https://remote.test/number"), CreateRouterEvent(new FullUri("https://remote.test/number"), sender, data: new DataModelValue(42)), CancellationToken.None);
		Assert.AreEqual(expected: "text/xml", handler.ContentType);
		Assert.IsNotNull(handler.Content);

		var list = new DataModelList();
		list.Add(new DataModelValue("item"));
		await controller.SendEvent(new FullUri("https://remote.test/list"), CreateRouterEvent(new FullUri("https://remote.test/list"), sender, data: list), CancellationToken.None);
		Assert.AreEqual(expected: "text/xml", handler.ContentType);

		var limited = CreateHttpController(new CapturingHttpMessageHandler(), maxMessageSize: 2);
		await Assert.ThrowsExactlyAsync<HttpRequestException>([ExcludeFromCodeCoverage] async () =>
			await limited.SendEvent(new FullUri("https://remote.test/large"), CreateRouterEvent(new FullUri("https://remote.test/large"), sender, data: new DataModelValue("payload")), CancellationToken.None));

		var failed = CreateHttpController(new CapturingHttpMessageHandler(HttpStatusCode.BadRequest));
		await Assert.ThrowsExactlyAsync<HttpRequestException>([ExcludeFromCodeCoverage] async () =>
			await failed.SendEvent(new FullUri("https://remote.test/failure"), CreateRouterEvent(new FullUri("https://remote.test/failure"), sender), CancellationToken.None));
	}

	[TestMethod]
	public async Task HttpControllerCoversEmptyScalarXmlCurrentOversizeAndFailedResponsePaths()
	{
		var handler = new CapturingHttpMessageHandler();
		var controller = CreateHttpController(handler);
		var sender = InvokeId.FromString("sender");
		await controller.SendEvent(new FullUri("https://remote.test/empty"), CreateRouterEvent(new FullUri("https://remote.test/empty"), sender), CancellationToken.None);
		Assert.IsNull(handler.Content);

		await controller.SendEvent(new FullUri("https://remote.test/number"), CreateRouterEvent(new FullUri("https://remote.test/number"), sender, data: new DataModelValue(42)), CancellationToken.None);
		Assert.AreEqual(expected: "text/xml", handler.ContentType);
		Assert.IsNotNull(handler.Content);

		var list = new DataModelList();
		list.Add(new DataModelValue("item"));
		await controller.SendEvent(new FullUri("https://remote.test/list"), CreateRouterEvent(new FullUri("https://remote.test/list"), sender, data: list), CancellationToken.None);
		Assert.AreEqual(expected: "text/xml", handler.ContentType);

		var limited = CreateHttpController(new CapturingHttpMessageHandler(), maxMessageSize: 2);
		await Assert.ThrowsExactlyAsync<HttpRequestException>([ExcludeFromCodeCoverage] async () =>
			await limited.SendEvent(new FullUri("https://remote.test/large"), CreateRouterEvent(new FullUri("https://remote.test/large"), sender, data: new DataModelValue("payload")), CancellationToken.None));

		var failed = CreateHttpController(new CapturingHttpMessageHandler(HttpStatusCode.BadRequest));
		await Assert.ThrowsExactlyAsync<HttpRequestException>([ExcludeFromCodeCoverage] async () =>
			await failed.SendEvent(new FullUri("https://remote.test/failure"), CreateRouterEvent(new FullUri("https://remote.test/failure"), sender), CancellationToken.None));
	}

	[TestMethod]
	public void NamedPipeControllerCreatesAndParsesLocalRemoteAndInvalidTargets()
	{
		var controller = CreateNamedPipeController(name: "machine", host: "localhost");
		Assert.IsTrue(controller.IsNamedPipeIoProcessorEnabled);
		Assert.IsTrue(controller.TryParseTarget(controller.ToSessionTarget(SessionId.FromString("session")), out var localHost, out var localName, out var session));
		Assert.IsNull(localHost);
		Assert.IsNull(localName);
		Assert.IsInstanceOfType<SessionId>(session);
		Assert.IsTrue(controller.TryParseTarget(controller.ToInvokeTarget(InvokeId.FromString("invoke")), out _, out _, out var invoke));
		Assert.IsInstanceOfType<InvokeId>(invoke);

		Assert.IsTrue(controller.TryParseTarget(new FullUri("net.pipe://remote.example/other/#_session_remote"), out var remoteHost, out var remoteName, out var remoteSession));
		Assert.AreEqual(expected: "remote.example", remoteHost);
		Assert.AreEqual(expected: "other", remoteName);
		Assert.AreEqual(expected: "remote", remoteSession.Value);

		Assert.IsFalse(controller.TryParseTarget(new FullUri("https://localhost/machine/#_session_value"), out _, out _, out _));
		Assert.IsFalse(controller.TryParseTarget(new FullUri("net.pipe://localhost/#_session_value"), out _, out _, out _));
		Assert.IsFalse(controller.TryParseTarget(new FullUri("net.pipe://localhost/machine/#_unknown_value"), out _, out _, out _));
		Assert.IsFalse(CreateNamedPipeController(name: null).IsNamedPipeIoProcessorEnabled);
	}

	[TestMethod]
	public async Task NamedPipeControllerRoundTripsEventsAndReturnsConsumerFailures()
	{
		var controller = CreateNamedPipeController(name: "machine" + Guid.NewGuid().ToString("N"));
		var targetId = SessionId.FromString("target");
		var incomingEvent = new IncomingEvent { Name = (EventName) "event", Data = new DataModelValue("payload") };
		NamedPipeEventMessage? received = null;
		var server = controller.ReceiveAndProcessEvent(message =>
											 {
												 received = message;

												 return ValueTask.CompletedTask;
											 }, CancellationToken.None).AsTask();
		await controller.SendEvent(host: null, name: null, targetId, incomingEvent, CancellationToken.None);
		await server;
		Assert.IsNotNull(received);
		Assert.AreEqual(targetId, received.TargetServiceId);
		Assert.AreEqual(expected: "event", received.Name.ToString());
		Assert.AreEqual(expected: "payload", received.Data.AsString());

		var failedServer = controller.ReceiveAndProcessEvent(
			static _ => ValueTask.FromException(new InvalidOperationException("consumer failure")), CancellationToken.None).AsTask();
		await Assert.ThrowsExactlyAsync<ProcessorException>([ExcludeFromCodeCoverage] async () =>
			await controller.SendEvent(host: null, name: null, targetId, incomingEvent, CancellationToken.None));
		await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () => await failedServer);
	}

	[TestMethod]
	public async Task NamedPipeIoProcessorRoutesLocalTargetsAndRejectsInvalidEvents()
	{
		var controller = CreateNamedPipeController(name: "machine", host: "localhost");
		var internalDispatcher = new Mock<IInternalEventDispatcher<NamedPipeIoProcessor>>();
		var sessionId = SessionId.FromString("session");
		var processor = new NamedPipeIoProcessor
						{
							NamedPipeController = controller,
							InternalEventDispatcher = internalDispatcher.Object,
							InvokeIdBase = null,
							SessionIdBase = Mock.Of<IStateMachineSessionId>(s => s.SessionId == sessionId)
						};
		var router = (IEventRouter) processor;
		Assert.AreEqual(controller.ToSessionTarget(sessionId), ((IIoProcessor) processor).Target);
		Assert.IsTrue(router.CanHandle(new FullUri("net.pipe")));

		var localEvent = CreateRouterEvent(controller.ToInvokeTarget(InvokeId.FromString("target")), sessionId);
		await router.Dispatch(localEvent, CancellationToken.None);
		internalDispatcher.Verify(d => d.Dispatch(It.Is<InvokeId>(id => id.Value == "target"), localEvent, CancellationToken.None), Times.Once);

		await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () =>
			await router.Dispatch(CreateRouterEvent(controller.ToSessionTarget(sessionId), sessionId, delayMs: 1), CancellationToken.None));
		await Assert.ThrowsExactlyAsync<ProcessorException>([ExcludeFromCodeCoverage] async () =>
			await router.Dispatch(CreateRouterEvent(target: null, sessionId), CancellationToken.None));
		await Assert.ThrowsExactlyAsync<ProcessorException>([ExcludeFromCodeCoverage] async () =>
			await router.Dispatch(CreateRouterEvent(new FullUri("https://invalid.test/"), sessionId), CancellationToken.None));
	}

	[TestMethod]
	public async Task HttpIoProcessorHostConfiguresPrefixRunsCancelledLoopAndDisposesListener()
	{
		var options = new HttpIoProcessorOptions { ListenUrl = "http://localhost:54321/", PublicBaseUrl = "http://localhost:54321/" };
		var host = new TestHttpIoProcessorHost(Mock.Of<IOptions<HttpIoProcessorOptions>>(o => o.Value == options))
				   {
					   HttpController = CreateHttpController(new CapturingHttpMessageHandler()),
					   ExternalEventDispatcher = Mock.Of<IExternalEventDispatcher<HttpIoProcessorHost>>(),
					   DisposeTokenBase = new DisposeToken(CancellationToken.None),
					   TaskMonitorBase = new CapturingTaskMonitor(),
					   LoggerBase = Mock.Of<ILogger<ResilientIoProcessorHostBase>>()
				   };

		Assert.AreEqual(expected: 1, host.PrefixCount);
		await host.RunProtected();
		Assert.AreEqual(expected: 1, host.StartListenerCount);
		Assert.AreEqual(expected: 1, host.StopListenerCount);
		host.Dispose();
		Assert.AreEqual(expected: 2, host.StopListenerCount);
	}

	[TestMethod]
	public async Task NamedPipeIoProcessorHostReturnsForDisabledAndAlreadyCancelledEnabledControllers()
	{
		var disabled = CreateNamedPipeHost(CreateNamedPipeController(name: null));
		var enabled = CreateNamedPipeHost(CreateNamedPipeController(name: "machine"));

		await disabled.RunProtected();
		await enabled.RunProtected();

		Assert.AreEqual(expected: 0, disabled.ExternalDispatchCount);
		Assert.AreEqual(expected: 0, enabled.ExternalDispatchCount);
	}

	private static HttpController CreateHttpController(CapturingHttpMessageHandler handler, long maxMessageSize = 0)
	{
		var options = new HttpIoProcessorOptions
					  {
						  PublicBaseUrl = "https://localhost:8443/root/",
						  MaxMessageSize = maxMessageSize,
						  Timeout = TimeSpan.FromSeconds(5)
					  };

		return new HttpController(Mock.Of<IOptions<HttpIoProcessorOptions>>(o => o.Value == options))
			   {
				   ExternalEventDispatcher = Mock.Of<IExternalEventDispatcher<HttpIoProcessor>>(),
				   HttpClientFactory = () => new HttpClient(handler, disposeHandler: false)
			   };
	}

	private static HttpIoProcessor CreateHttpIoProcessor(
		HttpController controller,
		IInternalEventDispatcher<HttpIoProcessor> dispatcher,
		InvokeId? invokeId,
		SessionId sessionId) =>
		new()
		{
			HttpController = controller,
			InternalEventDispatcher = dispatcher,
			InvokeIdBase = invokeId is null ? null : Mock.Of<IExternalServiceInvokeId>(i => i.InvokeId == invokeId),
			SessionIdBase = Mock.Of<IStateMachineSessionId>(s => s.SessionId == sessionId)
		};

	private static NamedPipeController CreateNamedPipeController(string? name, string host = ".")
	{
		var options = new NamedPipeIoProcessorOptions { Name = name, Timeout = TimeSpan.FromSeconds(5) };
		if (host != ".")
		{
			options.Host = host;
		}

		return new NamedPipeController(Mock.Of<IOptions<NamedPipeIoProcessorOptions>>(o => o.Value == options));
	}

	private static TestNamedPipeIoProcessorHost CreateNamedPipeHost(NamedPipeController controller)
	{
		var externalDispatcher = new Mock<IExternalEventDispatcher<NamedPipeIoProcessorHost>>();

		return new TestNamedPipeIoProcessorHost(externalDispatcher)
			   {
				   NamedPipeController = controller,
				   ExternalEventDispatcher = externalDispatcher.Object,
				   DisposeTokenBase = new DisposeToken(CancellationToken.None),
				   TaskMonitorBase = new CapturingTaskMonitor(),
				   LoggerBase = Mock.Of<ILogger<ResilientIoProcessorHostBase>>()
			   };
	}

	private static IRouterEvent CreateRouterEvent(
		FullUri? target,
		ServiceId sender,
		int delayMs = 0,
		string? name = null,
		DataModelValue data = default,
		SendId? sendId = null,
		InvokeId? invokeId = null)
	{
		var routerEvent = new Mock<IRouterEvent>();
		routerEvent.SetupGet(static e => e.Target).Returns(target);
		routerEvent.SetupGet(static e => e.DelayMs).Returns(delayMs);
		routerEvent.SetupGet(static e => e.SenderServiceId).Returns(sender);
		routerEvent.SetupGet(static e => e.Name).Returns(name is null ? default : (EventName) name);
		routerEvent.SetupGet(static e => e.Data).Returns(data);
		routerEvent.SetupGet(static e => e.SendId).Returns(sendId);
		routerEvent.SetupGet(static e => e.InvokeId).Returns(invokeId);

		return routerEvent.Object;
	}

	private sealed class TestIoProcessorHost : IoProcessorHostBase
	{
		public int BackgroundCount { get; private set; }

		public List<CancellationToken> ObservedTokens { get; } = [];

		public CancellationToken CurrentToken => Token;

		protected override Task StartNewBackgroundProcess() => BackgroundProcess();

		protected override Task BackgroundProcess()
		{
			BackgroundCount ++;
			ObservedTokens.Add(Token);

			return Task.CompletedTask;
		}
	}

	private sealed class TestResilientHost(int failuresBeforeSuccess) : ResilientIoProcessorHostBase
	{
		public int AttemptCount { get; private set; }

		public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		protected override async Task ProtectedBackgroundProcess()
		{
			AttemptCount ++;
			Entered.TrySetResult();

			if (failuresBeforeSuccess < 0)
			{
				await Task.Delay(Timeout.InfiniteTimeSpan, Token);
			}

			if (AttemptCount <= failuresBeforeSuccess)
			{
				throw new InvalidOperationException("retry");
			}
		}
	}

	private sealed class TestHttpIoProcessorHost(IOptions<HttpIoProcessorOptions> options) : HttpIoProcessorHost(options)
	{
		public int PrefixCount => Listener.Prefixes.Count;

		public int StartListenerCount { get; private set; }

		public int StopListenerCount { get; private set; }

		public Task RunProtected() => ProtectedBackgroundProcess();

		protected override void StartListener() => StartListenerCount ++;

		protected override void StopListener() => StopListenerCount ++;
	}

	private sealed class TestNamedPipeIoProcessorHost(Mock<IExternalEventDispatcher<NamedPipeIoProcessorHost>> externalDispatcher) : NamedPipeIoProcessorHost
	{
		public int ExternalDispatchCount => externalDispatcher.Invocations.Count;

		public Task RunProtected() => ProtectedBackgroundProcess();
	}

	private sealed class CapturingTaskMonitor : ITaskMonitor
	{
		public Task? LastTask { get; private set; }

		public Task WaitAsync(Task task, CancellationToken token) => task;

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task;

		public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => valueTask;

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) => valueTask;

		public void Forget(Task task) => LastTask = task;

		public void Forget(ValueTask valueTask) => LastTask = valueTask.AsTask();

		public void Forget<TResult>(ValueTask<TResult> valueTask) => LastTask = valueTask.AsTask();
	}

	private sealed class CapturingHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
	{
		public Uri? RequestUri { get; private set; }

		public string? Content { get; private set; }

		public string? ContentType { get; private set; }

		public string? Origin { get; private set; }

		public string? SendId { get; private set; }

		public string? InvokeId { get; private set; }

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			RequestUri = request.RequestUri;
			Content = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
			ContentType = request.Content?.Headers.ContentType?.MediaType;
			Origin = request.Headers.TryGetValues("Origin", out var origins) ? origins.Single() : null;
			SendId = request.Headers.TryGetValues("SCXML-SendId", out var sendIds) ? sendIds.Single() : null;
			InvokeId = request.Headers.TryGetValues("SCXML-InvokeId", out var invokeIds) ? invokeIds.Single() : null;

			return new HttpResponseMessage(statusCode);
		}
	}
}
