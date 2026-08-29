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

// ReSharper disable AccessToDisposedClosure

using System.Threading;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.IoC;
using Xtate.IoC.Options;
using Xtate.IoC.Tools;
using Xtate.Persistence;
using Xtate.Scxml;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;
using Xtate.StateMachineOptions.Services;

namespace Xtate.Core.Test.UnitTests.StateMachineHost;

[TestClass]
public class HostDispatchAndOptionsCoverageTest
{
	[TestMethod]
	public void StateMachineOptionsProviderForwardsEveryHostedOption()
	{
		var options = new StateMachineOptions.StateMachineOptions
					  {
						  PersistenceLevel = PersistenceLevel.Transition,
						  UnhandledErrorBehaviour = UnhandledErrorBehaviour.IgnoreError,
						  DestroyOnIdleTimeout = TimeSpan.FromMinutes(3),
						  XIncludeAllowed = true,
						  XIncludeMaxNestingLevel = 23
					  };
		var source = Mock.Of<IOptions<StateMachineOptions.StateMachineOptions>>(value => value.Value == options);
		var provider = new StateMachineOptionsProvider(source);

		Assert.AreEqual(PersistenceLevel.Transition, ((IPersistenceOptions)provider).PersistenceLevel);
		Assert.AreEqual(UnhandledErrorBehaviour.IgnoreError, ((IUnhandledErrorBehaviour)provider).Behaviour);
		Assert.AreEqual(TimeSpan.FromMinutes(3), ((IDestroyOnIdleTimeout)provider).IdleTimeout);
		Assert.IsTrue(((IXIncludeOptions)provider).XIncludeAllowed);
		Assert.AreEqual(expected: 23, ((IXIncludeOptions)provider).MaxNestingLevel);
	}

	[TestMethod]
	public async Task ScxmlStringChildStateMachineRegistersInvokedStateMachineContext()
	{
		const string scxml = "<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'/>";
		var parentSessionId = SessionId.FromString("parent");
		var invokeId = InvokeId.FromString(invokeId: "child", uniqueInvokeId: "unique-child");
		var type = new FullUri("scxml");
		var child = new ScxmlStringInvokedStateMachine(scxml) { ParentSessionId = parentSessionId, InvokeId = invokeId, Type = type };

		var services = new ServiceCollection();
		child.AddServices(services);
		var provider = services.BuildProvider();
		Assert.AreSame(child, await provider.GetRequiredService<IInvokedStateMachine>());
		Assert.AreSame(child, await provider.GetRequiredService<IParentStateMachineSessionId>());
		Assert.AreSame(child, await provider.GetRequiredService<IExternalServiceInvokeId>());
		Assert.AreSame(child, await provider.GetRequiredService<IExternalServiceType>());
	}

	[TestMethod]
	public async Task InternalEventDispatcherRoutesSessionsInvokesAndUnknownServices()
	{
		var stateMachines = new Mock<IStateMachineCollection>();
		var externalServices = new Mock<IExternalServiceCollection>();
		var deadLetters = new Mock<IDeadLetterQueue<TestSource>>();
		var dispatcher = new InternalEventDispatcher<TestSource>
						 {
							 StateMachineCollection = stateMachines.Object,
							 ExternalServiceCollection = externalServices.Object,
							 DeadLetterQueue = deadLetters.Object
						 };
		var incomingEvent = Mock.Of<IIncomingEvent>();
		using var cancellation = new CancellationTokenSource();
		var sessionId = SessionId.FromString("session");
		var invokeId = InvokeId.FromString("invoke");
		var unknown = new OtherServiceId("other");

		await dispatcher.Dispatch(sessionId, incomingEvent, cancellation.Token);
		await dispatcher.Dispatch(invokeId, incomingEvent, cancellation.Token);
		await dispatcher.Dispatch(unknown, incomingEvent, cancellation.Token);

		stateMachines.Verify(s => s.Dispatch(sessionId, incomingEvent, cancellation.Token), Times.Once);
		externalServices.Verify(s => s.Dispatch(invokeId, incomingEvent, cancellation.Token), Times.Once);
		deadLetters.Verify(s => s.Enqueue(unknown, incomingEvent), Times.Once);
	}

	[TestMethod]
	public async Task ExternalServiceManagerForwardsStartCancelAndEventsWithDisposeToken()
	{
		var collection = new Mock<IExternalServiceCollection>();
		var scopes = new Mock<IExternalServiceScopeManager>();
		using var cancellation = new CancellationTokenSource();
		var disposeToken = new DisposeToken(cancellation.Token);
		ExternalServiceClass? externalServiceClass = null;
		var manager = new ExternalServiceManager
					  {
						  ExternalServiceCollection = collection.Object,
						  ExternalServiceScopeManager = scopes.Object,
						  ExternalServiceClassFactory = invokeData => new ValueTask<ExternalServiceClass>(externalServiceClass = CreateExternalServiceClass(invokeData, collection.Object)),
						  DisposeToken = disposeToken
					  };
		var invokeId = InvokeId.FromString("invoke");
		var incomingEvent = Mock.Of<IIncomingEvent>();
		var invokeData = new InvokeData(invokeId, new FullUri("service"), Source: null, RawContent: null, DataModelValue.Undefined, DataModelValue.Undefined);

		await manager.Forward(invokeId, incomingEvent);
		await manager.Start(invokeData);
		await manager.Cancel(invokeId);

		collection.Verify(c => c.Dispatch(invokeId, incomingEvent, cancellation.Token), Times.Once);
		scopes.Verify(s => s.Start(externalServiceClass!, cancellation.Token), Times.Once);
		scopes.Verify(s => s.Cancel(invokeId, cancellation.Token), Times.Once);
	}

	private static ExternalServiceClass CreateExternalServiceClass(InvokeData invokeData, IExternalServiceCollection collection) =>
		new(
			invokeData,
			Mock.Of<IEventDispatcher>(),
			Mock.Of<IStateMachineSessionId>(),
			Mock.Of<IStateMachineLocation>(),
			Mock.Of<ICaseSensitivity>());

	[TestMethod]
	public async Task ExternalEventDispatcherRoutesSessionsHandledAndUnhandledInvokesAndUnknownServices()
	{
		var stateMachines = new Mock<IStateMachineCollection>();
		var globalServices = new Mock<IExternalServiceGlobalCollection>();
		var deadLetters = new Mock<IDeadLetterQueue<TestSource>>();
		var dispatcher = new ExternalEventDispatcher<TestSource>
						 {
							 StateMachineCollection = stateMachines.Object,
							 DeadLetterQueue = deadLetters.Object,
							 ExternalServiceGlobalCollection = globalServices.Object
						 };
		var incomingEvent = Mock.Of<IIncomingEvent>();
		using var cancellation = new CancellationTokenSource();
		var sessionId = SessionId.FromString("session");
		var handledInvoke = (UniqueInvokeId)InvokeId.FromString("handled");
		var unhandledInvoke = (UniqueInvokeId)InvokeId.FromString("unhandled");
		var unknown = new OtherServiceId("other");
		globalServices.Setup(g => g.TryDispatch(handledInvoke, incomingEvent, cancellation.Token)).ReturnsAsync(true);
		globalServices.Setup(g => g.TryDispatch(unhandledInvoke, incomingEvent, cancellation.Token)).ReturnsAsync(false);

		await dispatcher.Dispatch(sessionId, incomingEvent, cancellation.Token);
		await dispatcher.Dispatch(handledInvoke, incomingEvent, cancellation.Token);
		await dispatcher.Dispatch(unhandledInvoke, incomingEvent, cancellation.Token);
		await dispatcher.Dispatch(unknown, incomingEvent, cancellation.Token);

		stateMachines.Verify(s => s.Dispatch(sessionId, incomingEvent, cancellation.Token), Times.Once);
		globalServices.Verify(g => g.TryDispatch(handledInvoke, incomingEvent, cancellation.Token), Times.Once);
		globalServices.Verify(g => g.TryDispatch(unhandledInvoke, incomingEvent, cancellation.Token), Times.Once);
		deadLetters.Verify(d => d.Enqueue(handledInvoke, incomingEvent), Times.Never);
		deadLetters.Verify(d => d.Enqueue(unhandledInvoke, incomingEvent), Times.Once);
		deadLetters.Verify(d => d.Enqueue(unknown, incomingEvent), Times.Once);
	}

	[TestMethod]
	public async Task LocationChildStateMachineResolvesBothRelativeConstructorFormsAndRegistersInvokedContext()
	{
		var baseUri = new Uri("https://example.test/machines/");
		var parentSessionId = SessionId.FromString("parent");
		var invokeId = InvokeId.FromString(invokeId: "child", uniqueInvokeId: "unique-child");
		var type = new FullUri("scxml");
		var fromString = new LocationInvokedStateMachine(baseUri, relativeUri: "child.scxml")
						 {
							 ParentSessionId = parentSessionId,
							 InvokeId = invokeId,
							 Type = type
						 };
		var fromUri = new LocationInvokedStateMachine(baseUri, new Uri(uriString: "second.scxml", UriKind.Relative))
					  {
						  ParentSessionId = parentSessionId,
						  InvokeId = invokeId,
						  Type = type
					  };
		Assert.AreEqual(new Uri("https://example.test/machines/child.scxml"), ((IStateMachineLocation)fromString).Location);
		Assert.AreEqual(new Uri("https://example.test/machines/second.scxml"), ((IStateMachineLocation)fromUri).Location);

		var services = new ServiceCollection();
		fromUri.AddServices(services);
		var provider = services.BuildProvider();
		Assert.AreSame(fromUri, await provider.GetRequiredService<IInvokedStateMachine>());
		Assert.AreSame(fromUri, await provider.GetRequiredService<IParentStateMachineSessionId>());
		Assert.AreSame(fromUri, await provider.GetRequiredService<IExternalServiceInvokeId>());
		Assert.AreSame(fromUri, await provider.GetRequiredService<IExternalServiceType>());
	}

	[TestMethod]
	public async Task StateMachineCollectionDispatchesWrapsEventsDestroysAndUsesDeadLettersAfterUnregister()
	{
		var deadLetters = new Mock<IDeadLetterQueue<IStateMachineCollection>>();
		var controller = new Mock<IStateMachineController>();
		var collection = new TestStateMachineCollection { DeadLetterQueue = deadLetters.Object };
		var sessionId = SessionId.FromString("session");
		var sourceEvent = Mock.Of<IIncomingEvent>();
		using var cancellation = new CancellationTokenSource();

		collection.Register(sessionId);
		collection.SetController(sessionId, controller.Object);
		await collection.Dispatch(sessionId, sourceEvent, cancellation.Token);
		controller.Verify(c => c.Dispatch(It.Is<IncomingEvent>(e => !ReferenceEquals(e, sourceEvent)), cancellation.Token), Times.Once);

		var incomingEvent = new IncomingEvent { Name = EventName.FromString("event") };
		await collection.Dispatch(sessionId, incomingEvent, cancellation.Token);
		controller.Verify(c => c.Dispatch(incomingEvent, cancellation.Token), Times.Once);
		await collection.Destroy(sessionId);
		controller.Verify(static c => c.Destroy(), Times.Once);
		controller.Setup(static c => c.GetResult()).ReturnsAsync(new DataModelValue("result"));

		collection.Unregister(sessionId);
		await collection.Dispatch(sessionId, sourceEvent, cancellation.Token);
		await collection.Destroy(sessionId);
		deadLetters.Verify(d => d.Enqueue(sessionId, sourceEvent), Times.Once);
		controller.Verify(static c => c.Destroy(), Times.Once);
	}

	// Moq must be able to proxy this source type at runtime.
	// ReSharper disable once MemberCanBePrivate.Global
	// ReSharper disable once ClassNeverInstantiated.Global
	public sealed class TestSource;

	private sealed class TestStateMachineCollection : StateMachineCollection;

	[ExcludeFromCodeCoverage]
	private sealed class OtherServiceId(string id) : ServiceId(id)
	{
		public override string ServiceType => "other";

		protected override string GenerateId() => "generated";
	}
}
