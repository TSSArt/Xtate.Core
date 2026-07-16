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
using Xtate.Interpreter;
using Xtate.Interpreter.Internal;
using Xtate.IoC.Tools;
using Xtate.Scxml;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;

namespace Xtate.Test.UnitTests.StateMachineHost;

[TestClass]
public class ExternalServiceInfrastructureCoverageTest
{
	[TestMethod]
	public async Task GlobalCollectionWaitsForRegisteredServiceWrapsEventsAndUnregisters()
	{
		var collection = new ExternalServiceGlobalCollection();
		var invokeId = InvokeId.FromString(invokeId: "invoke", uniqueInvokeId: "unique-invoke");
		var uniqueInvokeId = invokeId.UniqueId;
		var sourceEvent = Mock.Of<IIncomingEvent>();
		var dispatcher = new Mock<IEventDispatcher>();
		var service = dispatcher.As<IExternalService>().Object;

		Assert.IsFalse(await collection.TryDispatch(uniqueInvokeId, sourceEvent, CancellationToken.None));

		collection.Register(uniqueInvokeId);
		var pendingDispatch = collection.TryDispatch(uniqueInvokeId, sourceEvent, CancellationToken.None).AsTask();
		Assert.IsFalse(pendingDispatch.IsCompleted);
		collection.SetExternalService(uniqueInvokeId, service);

		Assert.IsTrue(await pendingDispatch);
		dispatcher.Verify(d => d.Dispatch(It.Is<IncomingEvent>(e => !ReferenceEquals(e, sourceEvent)), CancellationToken.None), Times.Once);

		var incomingEvent = new IncomingEvent { Name = (EventName) "event" };
		Assert.IsTrue(await collection.TryDispatch(uniqueInvokeId, incomingEvent, CancellationToken.None));
		dispatcher.Verify(d => d.Dispatch(incomingEvent, CancellationToken.None), Times.Once);

		collection.Unregister(uniqueInvokeId);
		Assert.IsFalse(await collection.TryDispatch(uniqueInvokeId, sourceEvent, CancellationToken.None));
	}

	[TestMethod]
	public async Task GlobalCollectionTreatsNonDispatcherServiceAsHandledAndReleasesRemovedPendingDispatch()
	{
		var collection = new ExternalServiceGlobalCollection();
		var directId = InvokeId.FromString("direct").UniqueId;
		var pendingId = InvokeId.FromString("pending").UniqueId;
		var incomingEvent = Mock.Of<IIncomingEvent>();

		collection.SetExternalService(directId, Mock.Of<IExternalService>());
		Assert.IsTrue(await collection.TryDispatch(directId, incomingEvent, CancellationToken.None));

		collection.Register(pendingId);
		var pendingDispatch = collection.TryDispatch(pendingId, incomingEvent, CancellationToken.None).AsTask();
		collection.Unregister(pendingId);
		Assert.IsFalse(await pendingDispatch);
	}

	[TestMethod]
	public async Task LocalCollectionRegistersDispatchersAndMirrorsLifecycleToGlobalCollection()
	{
		var global = new Mock<IExternalServiceGlobalCollection>();
		var deadLetters = new Mock<IDeadLetterQueue<IExternalServiceCollection>>();
		var collection = new ExternalServiceCollection
						 {
							 ExternalServiceGlobalCollection = global.Object,
							 DeadLetterQueue = deadLetters.Object
						 };
		var invokeId = InvokeId.FromString(invokeId: "invoke", uniqueInvokeId: "unique-invoke");
		var sourceEvent = Mock.Of<IIncomingEvent>();
		var dispatcher = new Mock<IEventDispatcher>();
		var service = dispatcher.As<IExternalService>().Object;

		collection.Register(invokeId);
		var pendingDispatch = collection.Dispatch(invokeId, sourceEvent, CancellationToken.None).AsTask();
		Assert.IsFalse(pendingDispatch.IsCompleted);
		collection.SetExternalService(invokeId, service);
		await pendingDispatch;

		global.Verify(g => g.Register(invokeId.UniqueId), Times.Once);
		global.Verify(g => g.SetExternalService(invokeId.UniqueId, service), Times.Once);
		dispatcher.Verify(d => d.Dispatch(It.Is<IncomingEvent>(e => !ReferenceEquals(e, sourceEvent)), CancellationToken.None), Times.Once);

		var incomingEvent = new IncomingEvent { Name = (EventName) "event" };
		await collection.Dispatch(invokeId, incomingEvent, CancellationToken.None);
		dispatcher.Verify(d => d.Dispatch(incomingEvent, CancellationToken.None), Times.Once);

		collection.Unregister(invokeId);
		global.Verify(g => g.Unregister(invokeId.UniqueId), Times.Once);
	}

	[TestMethod]
	public async Task LocalCollectionUsesGlobalFallbackDeadLettersAndHandlesNonDispatcherServices()
	{
		var global = new Mock<IExternalServiceGlobalCollection>();
		var deadLetters = new Mock<IDeadLetterQueue<IExternalServiceCollection>>();
		var collection = new ExternalServiceCollection
						 {
							 ExternalServiceGlobalCollection = global.Object,
							 DeadLetterQueue = deadLetters.Object
						 };
		var directId = InvokeId.FromString("direct");
		var globalId = InvokeId.FromString("global");
		var missingId = InvokeId.FromString("missing");
		var incomingEvent = Mock.Of<IIncomingEvent>();

		collection.SetExternalService(directId, Mock.Of<IExternalService>());
		global.Setup(g => g.TryDispatch(globalId.UniqueId, incomingEvent, CancellationToken.None)).ReturnsAsync(true);
		global.Setup(g => g.TryDispatch(missingId.UniqueId, incomingEvent, CancellationToken.None)).ReturnsAsync(false);

		await collection.Dispatch(directId, incomingEvent, CancellationToken.None);
		await collection.Dispatch(globalId, incomingEvent, CancellationToken.None);
		await collection.Dispatch(missingId, incomingEvent, CancellationToken.None);

		global.Verify(g => g.TryDispatch(directId.UniqueId, incomingEvent, CancellationToken.None), Times.Never);
		deadLetters.Verify(d => d.Enqueue(globalId, incomingEvent), Times.Never);
		deadLetters.Verify(d => d.Enqueue(missingId, incomingEvent), Times.Once);
	}

	[TestMethod]
	public async Task EventRouterDetectsProvidersCreatesParentRouterEventAndDispatchesInvokeTarget()
	{
		var matchingActivator = Mock.Of<IExternalServiceActivator>();
		var miss = new Mock<IExternalServiceProvider>();
		var match = new Mock<IExternalServiceProvider>();
		match.Setup(p => p.TryGetActivator(new FullUri("urn:service"))).Returns(matchingActivator);
		var sessionId = SessionId.FromString("session");
		var stateMachineSessionId = Mock.Of<IStateMachineSessionId>(s => s.SessionId == sessionId);
		var collection = new Mock<IExternalServiceCollection>();
		var router = new ExternalServiceEventRouter
					 {
						 ExternalServiceProviders = [miss.Object, match.Object],
						 StateMachineSessionId = stateMachineSessionId,
						 ExternalServiceCollection = collection.Object,
						 StateMachineRuntimeError = new StateMachineRuntimeError(new ScopeObject())
					 };

		Assert.IsFalse(router.CanHandle(type: null));
		Assert.IsFalse(router.CanHandle(new FullUri("urn:missing")));
		Assert.IsTrue(router.CanHandle(new FullUri("urn:service")));
		Assert.IsFalse(router.IsInternalTarget(new FullUri("#_invoke")));

		var outgoingEvent = Mock.Of<IOutgoingEvent>(e => e.Name == (EventName) "event" && e.Target == new FullUri("target"));
		var routerEvent = await router.GetRouterEvent(outgoingEvent, CancellationToken.None);
		Assert.AreSame(sessionId, routerEvent.SenderServiceId);
		Assert.AreEqual(Const.ScxmlIoProcessorId, routerEvent.OriginType);
		Assert.AreEqual(Const.ParentTarget, routerEvent.Origin);

		var dispatchEvent = Mock.Of<IRouterEvent>(e => e.Target == new FullUri("#_invoke"));
		await router.Dispatch(dispatchEvent, CancellationToken.None);
		collection.Verify(c => c.Dispatch(It.Is<InvokeId>(id => id.Value == "invoke"), dispatchEvent, CancellationToken.None), Times.Once);
	}

	[TestMethod]
	public async Task EventRouterRejectsMissingEmptyAndMalformedInvokeTargets()
	{
		var router = new ExternalServiceEventRouter
					 {
						 ExternalServiceProviders = [],
						 StateMachineSessionId = Mock.Of<IStateMachineSessionId>(),
						 ExternalServiceCollection = Mock.Of<IExternalServiceCollection>(),
						 StateMachineRuntimeError = new StateMachineRuntimeError(new ScopeObject())
					 };

		await Assert.ThrowsExactlyAsync<PlatformException>([ExcludeFromCodeCoverage] async () =>
															   await router.Dispatch(Mock.Of<IRouterEvent>(), CancellationToken.None));
		await Assert.ThrowsExactlyAsync<PlatformException>([ExcludeFromCodeCoverage] async () =>
															   await router.Dispatch(Mock.Of<IRouterEvent>(e => e.Target == new FullUri("#_")), CancellationToken.None));
		await Assert.ThrowsExactlyAsync<PlatformException>([ExcludeFromCodeCoverage] async () =>
															   await router.Dispatch(Mock.Of<IRouterEvent>(e => e.Target == new FullUri("invoke")), CancellationToken.None));
	}

	[TestMethod]
	public async Task FactorySkipsNonMatchingProvidersAndCreatesFromOnlyMatchingActivator()
	{
		var expectedService = Mock.Of<IExternalService>();
		var activator = new Mock<IExternalServiceActivator>();
		activator.Setup(static a => a.Create()).ReturnsAsync(expectedService);
		var miss = new Mock<IExternalServiceProvider>();
		var match = new Mock<IExternalServiceProvider>();
		match.Setup(p => p.TryGetActivator(new FullUri("urn:service"))).Returns(activator.Object);
		var factory = new ExternalServiceFactory
					  {
						  ServiceFactories = ToAsyncEnumerable(miss.Object, match.Object),
						  ExternalServiceType = Mock.Of<IExternalServiceType>(t => t.Type == new FullUri("urn:service"))
					  };

		Assert.AreSame(expectedService, await factory.CreateService());
		miss.Verify(p => p.TryGetActivator(new FullUri("urn:service")), Times.Once);
		match.Verify(p => p.TryGetActivator(new FullUri("urn:service")), Times.Once);
		activator.Verify(static a => a.Create(), Times.Once);
	}

	[TestMethod]
	public async Task FactoryRejectsMissingAndDuplicateMatchingProviders()
	{
		var type = Mock.Of<IExternalServiceType>(t => t.Type == new FullUri("urn:service"));
		var activator = Mock.Of<IExternalServiceActivator>();
		var first = new Mock<IExternalServiceProvider>();
		var second = new Mock<IExternalServiceProvider>();
		first.Setup(p => p.TryGetActivator(new FullUri("urn:service"))).Returns(activator);
		second.Setup(p => p.TryGetActivator(new FullUri("urn:service"))).Returns(activator);
		var missingFactory = new ExternalServiceFactory { ServiceFactories = ToAsyncEnumerable(first: Mock.Of<IExternalServiceProvider>()), ExternalServiceType = type };
		var duplicateFactory = new ExternalServiceFactory { ServiceFactories = ToAsyncEnumerable(first.Object, second.Object), ExternalServiceType = type };

		await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () => await missingFactory.CreateService());
		await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () => await duplicateFactory.CreateService());
	}

	[TestMethod]
	public async Task ScxmlIoProcessorHandlesIdsAliasesInternalTargetsAndStateMachineOrigin()
	{
		var sessionId = SessionId.FromString("session");
		var processor = CreateScxmlIoProcessor(invokeId: null, sessionId);
		var ioProcessor = (IIoProcessor) processor;
		var router = (IEventRouter) processor;

		Assert.AreEqual(Const.ScxmlIoProcessorId, ioProcessor.Id);
		Assert.AreEqual(new FullUri(Const.ScxmlIoProcessorBaseUri, relativeUri: "#_scxml_session"), ioProcessor.Target);
		Assert.IsTrue(router.CanHandle(type: null));
		Assert.IsTrue(router.CanHandle(Const.ScxmlIoProcessorId));
		Assert.IsTrue(router.CanHandle(Const.ScxmlIoProcessorAliasId));
		Assert.IsFalse(router.CanHandle(new FullUri("urn:other")));
		Assert.IsTrue(router.IsInternalTarget(Const.InternalTarget));
		Assert.IsTrue(router.IsInternalTarget(Const.ScxmlIoProcessorInternalTarget));
		Assert.IsFalse(router.IsInternalTarget(new FullUri("other")));

		var outgoingEvent = Mock.Of<IOutgoingEvent>(e => e.Name == (EventName) "event");
		var routerEvent = await router.GetRouterEvent(outgoingEvent, CancellationToken.None);
		Assert.AreSame(sessionId, routerEvent.SenderServiceId);
		Assert.AreEqual(Const.ScxmlIoProcessorId, routerEvent.OriginType);
		Assert.AreEqual(ioProcessor.Target, routerEvent.Origin);
	}

	[TestMethod]
	public async Task ScxmlIoProcessorUsesInvokeOriginAndDispatchesSelfParentSessionAndInvokeTargets()
	{
		var invokeId = InvokeId.FromString("source");
		var self = new Mock<IEventDispatcher>();
		var parent = new Mock<IParentEventDispatcher>();
		var internalDispatcher = new Mock<IInternalEventDispatcher<ScxmlIoProcessor>>();
		var processor = CreateScxmlIoProcessor(invokeId, SessionId.FromString("session"), self.Object, parent.Object, internalDispatcher.Object);
		var ioProcessor = (IIoProcessor) processor;
		var router = (IEventRouter) processor;

		Assert.AreEqual(new FullUri(Const.ScxmlIoProcessorBaseUri, relativeUri: "#_source"), ioProcessor.Target);
		var outgoingEvent = Mock.Of<IOutgoingEvent>();
		var outgoingRouterEvent = await router.GetRouterEvent(outgoingEvent, CancellationToken.None);
		Assert.AreSame(invokeId, outgoingRouterEvent.SenderServiceId);

		var selfEvent = Mock.Of<IRouterEvent>();
		var parentEvent = Mock.Of<IRouterEvent>(e => e.Target == Const.ParentTarget);
		var absoluteParentEvent = Mock.Of<IRouterEvent>(e => e.Target == Const.ScxmlIoProcessorParentTarget);
		var sessionEvent = Mock.Of<IRouterEvent>(e => e.Target == new FullUri("#_scxml_target"));
		var invokeEvent = Mock.Of<IRouterEvent>(e => e.Target == new FullUri("#_target"));
		await router.Dispatch(selfEvent, CancellationToken.None);
		await router.Dispatch(parentEvent, CancellationToken.None);
		await router.Dispatch(absoluteParentEvent, CancellationToken.None);
		await router.Dispatch(sessionEvent, CancellationToken.None);
		await router.Dispatch(invokeEvent, CancellationToken.None);

		self.Verify(d => d.Dispatch(selfEvent, CancellationToken.None), Times.Once);
		parent.Verify(d => d.Dispatch(parentEvent, CancellationToken.None), Times.Once);
		parent.Verify(d => d.Dispatch(absoluteParentEvent, CancellationToken.None), Times.Once);
		internalDispatcher.Verify(d => d.Dispatch(It.Is<SessionId>(id => id.Value == "target"), sessionEvent, CancellationToken.None), Times.Once);
		internalDispatcher.Verify(d => d.Dispatch(It.Is<InvokeId>(id => id.Value == "target"), invokeEvent, CancellationToken.None), Times.Once);
	}

	[TestMethod]
	public async Task ScxmlIoProcessorRejectsInvalidTargetsIncludingParentWithoutDispatcher()
	{
		var router = (IEventRouter) CreateScxmlIoProcessor(invokeId: null, SessionId.FromString("session"));

		await Assert.ThrowsExactlyAsync<ProcessorException>([ExcludeFromCodeCoverage] async () =>
																await router.Dispatch(Mock.Of<IRouterEvent>(e => e.Target == new FullUri("invalid")), CancellationToken.None));
		await Assert.ThrowsExactlyAsync<ProcessorException>([ExcludeFromCodeCoverage] async () =>
																await router.Dispatch(Mock.Of<IRouterEvent>(e => e.Target == Const.ParentTarget), CancellationToken.None));
	}

	private static ScxmlIoProcessor CreateScxmlIoProcessor(InvokeId? invokeId,
														   SessionId sessionId,
														   IEventDispatcher? self = null,
														   IParentEventDispatcher? parent = null,
														   IInternalEventDispatcher<ScxmlIoProcessor>? internalDispatcher = null) =>
		new()
		{
			InvokeIdBase = invokeId is not null ? Mock.Of<IExternalServiceInvokeId>(i => i.InvokeId == invokeId) : null,
			SessionIdBase = Mock.Of<IStateMachineSessionId>(s => s.SessionId == sessionId),
			SelfEventDispatcher = self ?? Mock.Of<IEventDispatcher>(),
			ParentEventDispatcher = parent,
			InternalEventDispatcher = internalDispatcher ?? Mock.Of<IInternalEventDispatcher<ScxmlIoProcessor>>()
		};

	private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(params T[] first)
	{
		foreach (var item in first)
		{
			yield return item;

			await Task.Yield();
		}
	}
}