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
using Xtate.IoC.Tools;
using Xtate.Logging;
using Xtate.Persistence;
using Xtate.Persistence.DependencyInjection;
using Xtate.Persistence.Services;
using Xtate.Scxml;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.DependencyInjection;
using Xtate.StateMachineHost.Services;
using Xtate.TaskMonitor;

namespace Xtate.Core.Test.UnitTests.Persistence;

[TestClass]
public class PersistedExternalServiceScopeManagerCoverageTest
{
	[TestMethod]
	public async Task PersistenceModuleReplacesExternalServiceLifecycleServicesAndRegistersResumeClass()
	{
		var source = Mock.Of<IExternalServiceSource>();
		var parameters = Mock.Of<IExternalServiceParameters>();
		var invokeId = Mock.Of<IExternalServiceInvokeId>(value => value.InvokeId == InvokeId.FromString("invoke", "unique-invoke"));
		var type = Mock.Of<IExternalServiceType>(value => value.Type == Const.ScxmlServiceTypeId);
		var location = Mock.Of<IStateMachineLocation>();
		var sessionId = Mock.Of<IStateMachineSessionId>(value => value.SessionId == SessionId.FromString("parent"));
		var stateMachine = Mock.Of<IStateMachine>();
		await using var container = Container.Create<StateMachineProcessorModule, PersistenceModule>(services =>
																									 {
																										 services.AddConstant(source);
																										 services.AddConstant(parameters);
																										 services.AddConstant(invokeId);
																										 services.AddConstant(type);
																										 services.AddConstant(location);
																										 services.AddConstant(sessionId);
																										 services.AddConstant(stateMachine);
																									 });

		var collection = await container.GetRequiredService<IExternalServiceCollection>();
		var scopeManager = await container.GetRequiredService<IExternalServiceScopeManager>();
		var controller = await container.GetRequiredService<IExternalServiceController>();
		_ = await container.GetRequiredService<StateMachineExternalService>();
		var resumeClass = await container.GetRequiredService<ResumeExternalServiceClass, InvokeData>(CreateInvokeData());

		Assert.IsInstanceOfType<ExternalServiceCollection>(collection);
		Assert.IsInstanceOfType<PersistedExternalServiceScopeManager>(scopeManager);
		Assert.IsInstanceOfType<PersistedExternalServiceController>(controller);
		Assert.IsNotNull(resumeClass);
	}

	[TestMethod]
	public async Task ResumedStateMachineServiceExecutesRestoredChildAndWaitsForItsCompletion()
	{
		var childSessionId = SessionId.FromString("unique-child");
		var childResult = new TaskCompletionSource<DataModelValue>(TaskCreationOptions.RunContinuationsAsynchronously);
		StateMachineClass? resumedChild = null;
		var scopeManager = new Mock<IStateMachineScopeManager>();
		scopeManager.Setup(manager => manager.Execute(It.IsAny<StateMachineClass>(), SecurityContextType.InvokedService))
					.Callback<StateMachineClass, SecurityContextType>((stateMachineClass, _) => resumedChild = stateMachineClass)
					.Returns(new ValueTask<DataModelValue>(childResult.Task));
		var stateMachineCollection = new Mock<IStateMachineCollection>();
		var taskMonitor = new CapturingTaskMonitor();
		var invokeId = InvokeId.FromString(invokeId: "child", uniqueInvokeId: "unique-child");
		var service = new PersistedStateMachineExternalService
					  {
						  StateMachineScopeManager = () => new ValueTask<IStateMachineScopeManager>(scopeManager.Object),
						  StateMachineLocation = Mock.Of<IStateMachineLocation>(),
						  StateMachineCollection = stateMachineCollection.Object,
						  ParentStateMachineSessionId = Mock.Of<IStateMachineSessionId>(value => value.SessionId == SessionId.FromString("parent")),
						  ExternalServiceInvokeId = Mock.Of<IExternalServiceInvokeId>(value => value.InvokeId == invokeId),
						  ExternalServiceType = Mock.Of<IExternalServiceType>(value => value.Type == Const.ScxmlServiceTypeId),
						  TaskMonitor = taskMonitor,
						  ExternalServiceSourceBase = Mock.Of<IExternalServiceSource>(value => value.Content == DataModelValue.Undefined),
						  ExternalServiceParametersBase = Mock.Of<IExternalServiceParameters>(value => value.Parameters == DataModelValue.Undefined),
						  TaskMonitorBase = taskMonitor,
						  DisposeTokenBase = new DisposeToken(CancellationToken.None)
					  };

		await ((IResumableExternalService)service).RestoreExecutionState();
		var completion = ((IExternalService)service).GetResult();
		Assert.IsFalse(completion.IsCompleted);
		var invokedStateMachine = Assert.IsInstanceOfType<ResumedInvokedStateMachine>(resumedChild);
		Assert.AreEqual(childSessionId.Value, invokedStateMachine.SessionId.Value);
		Assert.AreEqual(SessionId.FromString("parent"), invokedStateMachine.ParentSessionId);
		Assert.AreEqual(invokeId, invokedStateMachine.InvokeId);
		Assert.AreEqual(Const.ScxmlServiceTypeId, invokedStateMachine.Type);

		childResult.SetResult(new DataModelValue("child-result"));
		Assert.AreEqual(expected: "child-result", (await completion).AsString());

		await service.DisposeAsync();
		stateMachineCollection.Verify(collection => collection.Destroy(childSessionId), Times.Once);
	}

	[TestMethod]
	public async Task ResumedStateMachineServiceWaitsForChildFailureAndPropagatesIt()
	{
		var childFailure = new InvalidOperationException("resumed child failed");
		var childResult = new TaskCompletionSource<DataModelValue>(TaskCreationOptions.RunContinuationsAsynchronously);
		var scopeManager = new Mock<IStateMachineScopeManager>();
		scopeManager.Setup(manager => manager.Execute(It.IsAny<StateMachineClass>(), SecurityContextType.InvokedService))
					.Returns(new ValueTask<DataModelValue>(childResult.Task));
		var stateMachineCollection = new Mock<IStateMachineCollection>();
		var taskMonitor = new CapturingTaskMonitor();
		var invokeId = InvokeId.FromString(invokeId: "failing-child", uniqueInvokeId: "unique-failing-child");
		var service = new PersistedStateMachineExternalService
					  {
						  StateMachineScopeManager = () => new ValueTask<IStateMachineScopeManager>(scopeManager.Object),
						  StateMachineLocation = Mock.Of<IStateMachineLocation>(),
						  StateMachineCollection = stateMachineCollection.Object,
						  ParentStateMachineSessionId = Mock.Of<IStateMachineSessionId>(value => value.SessionId == SessionId.FromString("parent")),
						  ExternalServiceInvokeId = Mock.Of<IExternalServiceInvokeId>(value => value.InvokeId == invokeId),
						  ExternalServiceType = Mock.Of<IExternalServiceType>(value => value.Type == Const.ScxmlServiceTypeId),
						  TaskMonitor = taskMonitor,
						  ExternalServiceSourceBase = Mock.Of<IExternalServiceSource>(value => value.Content == DataModelValue.Undefined),
						  ExternalServiceParametersBase = Mock.Of<IExternalServiceParameters>(value => value.Parameters == DataModelValue.Undefined),
						  TaskMonitorBase = taskMonitor,
						  DisposeTokenBase = new DisposeToken(CancellationToken.None)
					  };

		await ((IResumableExternalService)service).RestoreExecutionState();
		var completion = ((IExternalService)service).GetResult();
		Assert.IsFalse(completion.IsCompleted);

		childResult.SetException(childFailure);
		var actualFailure = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await completion);
		Assert.AreSame(childFailure, actualFailure);

		await service.DisposeAsync();
		stateMachineCollection.Verify(collection => collection.Destroy(It.Is<SessionId>(sessionId => sessionId.Value == invokeId.UniqueId.Value)), Times.Once);
	}

	[TestMethod]
	public async Task PersistedControllerReportsTerminatedNonResumableService()
	{
		var service = new Mock<IExternalService>(MockBehavior.Strict);
		var sentEvents = new List<IOutgoingEvent>();
		var externalCommunication = new Mock<IExternalCommunication>();
		externalCommunication.Setup(communication => communication.TrySend(It.IsAny<IOutgoingEvent>()))
							 .Returns((IOutgoingEvent outgoingEvent) =>
									  {
										  sentEvents.Add(outgoingEvent);

										  return new ValueTask<SendStatus>(SendStatus.Sent);
									  });
		var controller = CreateController(service.Object, InvokeId.FromString("invoke"), externalCommunication.Object, resume: true);

		await controller.WaitForCompletion();

		Assert.AreEqual(EventName.ErrorExecution, sentEvents.Single().Name);
		service.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task ActiveInvokeDataIsRestoredAndRemovedInvokesAreNotResumed()
	{
		await using var storage = new TestTransactionalStorage();
		var invokeData = CreateInvokeData();
		await using var firstManager = await PersistSuspendedInvoke(storage, invokeData);
		var restoredInvokes = new List<InvokeData>();
		var result = new TaskCompletionSource<DataModelValue>(TaskCreationOptions.RunContinuationsAsynchronously);
		var secondService = new Mock<IExternalService>();
		secondService.As<IResumableExternalService>();
		secondService.Setup(static service => service.GetResult()).Returns(new ValueTask<DataModelValue>(result.Task));
		var secondMonitor = new CapturingTaskMonitor();
		await using var secondManager = await CreateScopeManager(storage, CreateCollection(), secondService.Object, secondMonitor, restoredInvokes.Add);

		await secondManager.InitializeAsync();

		var restored = restoredInvokes.Single();
		Assert.AreEqual(invokeData.InvokeId, restored.InvokeId);
		Assert.AreEqual(invokeData.Type, restored.Type);
		Assert.AreEqual(invokeData.Source, restored.Source);
		Assert.AreEqual(invokeData.RawContent, restored.RawContent);
		AssertDataModelListEqual(invokeData.Content, restored.Content);
		AssertDataModelListEqual(invokeData.Parameters, restored.Parameters);

		result.SetResult(DataModelValue.Undefined);
		await Task.WhenAll(secondMonitor.ForgottenTasks);

		var thirdInvokes = new List<InvokeData>();
		await using var thirdManager = await CreateScopeManager(storage, CreateCollection(), Mock.Of<IExternalService>(), new CapturingTaskMonitor(), thirdInvokes.Add);
		await thirdManager.InitializeAsync();

		Assert.IsEmpty(thirdInvokes);
		Assert.AreEqual(expected: 3, storage.CheckPointCount);
		Assert.AreEqual(expected: 1, storage.ShrinkCount);
	}

	[TestMethod]
	public async Task CompletedExternalServiceIsNotRestored()
	{
		await using var storage = new TestTransactionalStorage();
		var invokeData = CreateInvokeData();
		var firstService = new Mock<IExternalService>();
		firstService.Setup(static service => service.GetResult()).Returns(new ValueTask<DataModelValue>(DataModelValue.Undefined));
		var firstMonitor = new CapturingTaskMonitor();
		await using var firstManager = await CreateScopeManager(storage, CreateCollection(), firstService.Object, firstMonitor);

		var result = await firstManager.Start(CreateExternalServiceClass(invokeData, CreateCollection()), CancellationToken.None);
		await result.WaitForCompletion();
		await Task.WhenAll(firstMonitor.ForgottenTasks);

		var restoredInvokes = new List<InvokeData>();
		await using var secondManager = await CreateScopeManager(storage, CreateCollection(), Mock.Of<IExternalService>(), new CapturingTaskMonitor(), restoredInvokes.Add);
		await secondManager.InitializeAsync();

		Assert.IsEmpty(restoredInvokes);
	}

	[TestMethod]
	public void PersistedInvokeDataRoundTripsAllFields()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var expected = CreateInvokeData();

		new PersistedInvokeData(expected.InvokeId, expected.Type, expected.Source, expected.RawContent, expected.Content, expected.Parameters).Store(new Bucket(storage));
		var actual = new PersistedInvokeData(new Bucket(storage));

		Assert.AreEqual(expected.InvokeId, actual.InvokeId);
		Assert.AreEqual(expected.Type, actual.Type);
		Assert.AreEqual(expected.Source, actual.Source);
		Assert.AreEqual(expected.RawContent, actual.RawContent);
		AssertDataModelListEqual(expected.Content, actual.Content);
		AssertDataModelListEqual(expected.Parameters, actual.Parameters);
	}

	[TestMethod]
	public async Task ResumeStartsOnlyServicesThatImplementPersistenceResumeContract()
	{
		await using var storage = new TestTransactionalStorage();
		var invokeData = CreateInvokeData();
		await using var firstManager = await PersistSuspendedInvoke(storage, invokeData);
		var result = new TaskCompletionSource<DataModelValue>(TaskCreationOptions.RunContinuationsAsynchronously);
		var externalService = new Mock<IExternalService>();
		externalService.Setup(static service => service.GetResult()).Returns(new ValueTask<DataModelValue>(result.Task));
		var resumableExternalService = externalService.As<IResumableExternalService>();
		resumableExternalService.Setup(static service => service.RestoreExecutionState()).Returns(ValueTask.CompletedTask);
		var taskMonitor = new CapturingTaskMonitor();
		await using var scopeManager = await CreateScopeManager(storage, CreateCollection(), externalService.Object, taskMonitor);

		await scopeManager.InitializeAsync();

		resumableExternalService.Verify(static service => service.RestoreExecutionState(), Times.Once);
		Assert.HasCount(expected: 1, taskMonitor.ForgottenTasks);

		result.SetResult(DataModelValue.Undefined);
		await Task.WhenAll(taskMonitor.ForgottenTasks);
	}

	[TestMethod]
	public async Task ResumeRemovesPersistedServiceThatDoesNotImplementResumeContract()
	{
		await using var storage = new TestTransactionalStorage();
		var invokeData = CreateInvokeData();
		await using var firstManager = await PersistSuspendedInvoke(storage, invokeData);
		var taskMonitor = new CapturingTaskMonitor();
		await using var scopeManager = await CreateScopeManager(storage, CreateCollection(), Mock.Of<IExternalService>(), taskMonitor);

		await scopeManager.InitializeAsync();
		await Task.WhenAll(taskMonitor.ForgottenTasks);

		var restoredInvokes = new List<InvokeData>();
		await using var restoredManager = await CreateScopeManager(storage, CreateCollection(), Mock.Of<IExternalService>(), new CapturingTaskMonitor(), restoredInvokes.Add);
		await restoredManager.InitializeAsync();

		Assert.IsEmpty(restoredInvokes);
	}

	[TestMethod]
	public async Task ResumeFailureForNonStateMachineSendsParentErrorAndDestroysService()
	{
		await using var storage = new TestTransactionalStorage();
		var invokeData = CreateInvokeData();
		await using var firstManager = await PersistSuspendedInvoke(storage, invokeData);
		var externalService = new Mock<IExternalService>(MockBehavior.Strict);
		var sentEvents = new List<IOutgoingEvent>();
		var externalCommunication = new Mock<IExternalCommunication>();
		externalCommunication.Setup(communication => communication.TrySend(It.IsAny<IOutgoingEvent>()))
							 .Returns((IOutgoingEvent outgoingEvent) =>
									  {
										  sentEvents.Add(outgoingEvent);

										  return new ValueTask<SendStatus>(SendStatus.Sent);
									  });
		var collection = new Mock<IExternalServiceCollection>();
		var taskMonitor = new CapturingTaskMonitor();
		await using var scopeManager = await CreateScopeManager(
			storage,
			collection.Object,
			externalService.Object,
			taskMonitor,
			externalCommunication: externalCommunication.Object);

		await scopeManager.InitializeAsync();
		await Task.WhenAll(taskMonitor.ForgottenTasks);

		Assert.AreEqual(EventName.ErrorExecution, sentEvents.Single().Name);
		collection.Verify(value => value.Unregister(invokeData.InvokeId), Times.Once);
		externalService.Verify(static service => service.GetResult(), Times.Never);
	}

	[TestMethod]
	public async Task MultipleNonResumableServicesEachSendParentErrorAndAreNotRetried()
	{
		await using var storage = new TestTransactionalStorage();
		var firstInvoke = CreateInvokeData("non-resumable-one");
		var secondInvoke = CreateInvokeData("non-resumable-two");
		var suspendedService = new Mock<IExternalService>();
		suspendedService.Setup(static service => service.GetResult())
						.Returns(ValueTask.FromException<DataModelValue>(new ExternalServiceSuspendedException { Owner = new object() }));
		var firstMonitor = new CapturingTaskMonitor();
		var firstCollection = CreateCollection();
		await using var firstManager = await CreateScopeManager(storage, firstCollection, suspendedService.Object, firstMonitor);
		await firstManager.Start(CreateExternalServiceClass(firstInvoke, firstCollection), CancellationToken.None);
		await firstManager.Start(CreateExternalServiceClass(secondInvoke, firstCollection), CancellationToken.None);
		await Task.WhenAll(firstMonitor.ForgottenTasks);

		var nonResumableService = new Mock<IExternalService>(MockBehavior.Strict);
		var sentEvents = new List<IOutgoingEvent>();
		var communication = new Mock<IExternalCommunication>();
		communication.Setup(value => value.TrySend(It.IsAny<IOutgoingEvent>()))
					 .Returns((IOutgoingEvent outgoingEvent) =>
							  {
								  sentEvents.Add(outgoingEvent);

								  return new ValueTask<SendStatus>(SendStatus.Sent);
							  });
		var resumedCollection = new Mock<IExternalServiceCollection>();
		var resumedMonitor = new CapturingTaskMonitor();
		await using var resumedManager = await CreateScopeManager(
			storage,
			resumedCollection.Object,
			nonResumableService.Object,
			resumedMonitor,
			externalCommunication: communication.Object);

		await resumedManager.InitializeAsync();
		await Task.WhenAll(resumedMonitor.ForgottenTasks);

		Assert.HasCount(expected: 2, sentEvents);
		Assert.IsTrue(sentEvents.All(outgoingEvent => outgoingEvent.Name == EventName.ErrorExecution));
		resumedCollection.Verify(value => value.Unregister(firstInvoke.InvokeId), Times.Once);
		resumedCollection.Verify(value => value.Unregister(secondInvoke.InvokeId), Times.Once);
		nonResumableService.Verify(static service => service.GetResult(), Times.Never);

		var retriedInvokes = new List<InvokeData>();
		await using var thirdManager = await CreateScopeManager(storage, CreateCollection(), Mock.Of<IExternalService>(), new CapturingTaskMonitor(), retriedInvokes.Add);
		await thirdManager.InitializeAsync();
		Assert.IsEmpty(retriedInvokes);
	}

	[TestMethod]
	public async Task RestoreExceptionSendsParentErrorWithoutExecutingServiceAndRemovesPersistedEntry()
	{
		await using var storage = new TestTransactionalStorage();
		var invokeData = CreateInvokeData("restore-failure");
		await using var firstManager = await PersistSuspendedInvoke(storage, invokeData);
		var externalService = new Mock<IExternalService>(MockBehavior.Strict);
		var resumableService = externalService.As<IResumableExternalService>();
		resumableService.Setup(static service => service.RestoreExecutionState())
						.Returns(ValueTask.FromException(new InvalidOperationException("restore failed")));
		var sentEvents = new List<IOutgoingEvent>();
		var communication = new Mock<IExternalCommunication>();
		communication.Setup(value => value.TrySend(It.IsAny<IOutgoingEvent>()))
					 .Returns((IOutgoingEvent outgoingEvent) =>
							  {
								  sentEvents.Add(outgoingEvent);

								  return new ValueTask<SendStatus>(SendStatus.Sent);
							  });
		var collection = new Mock<IExternalServiceCollection>();
		var taskMonitor = new CapturingTaskMonitor();
		await using var resumedManager = await CreateScopeManager(
			storage,
			collection.Object,
			externalService.Object,
			taskMonitor,
			externalCommunication: communication.Object);

		await resumedManager.InitializeAsync();
		await Task.WhenAll(taskMonitor.ForgottenTasks);

		Assert.AreEqual(EventName.ErrorExecution, sentEvents.Single().Name);
		resumableService.Verify(static service => service.RestoreExecutionState(), Times.Once);
		externalService.Verify(static service => service.GetResult(), Times.Never);
		collection.Verify(value => value.Unregister(invokeData.InvokeId), Times.Once);

		var retriedInvokes = new List<InvokeData>();
		await using var thirdManager = await CreateScopeManager(storage, CreateCollection(), Mock.Of<IExternalService>(), new CapturingTaskMonitor(), retriedInvokes.Add);
		await thirdManager.InitializeAsync();
		Assert.IsEmpty(retriedInvokes);
	}

	[TestMethod]
	public async Task ScopeManagerCapturesExternalServiceSuspendedExceptionAndPreservesServiceForResume()
	{
		await using var storage = new TestTransactionalStorage();
		var invokeData = CreateInvokeData();
		await using var firstManager = await PersistSuspendedInvoke(storage, invokeData);
		var restoredInvokes = new List<InvokeData>();
		await using var secondManager = await CreateScopeManager(storage, CreateCollection(), Mock.Of<IExternalService>(), new CapturingTaskMonitor(), restoredInvokes.Add);

		await secondManager.InitializeAsync();

		Assert.AreEqual(invokeData.InvokeId, restoredInvokes.Single().InvokeId);
	}

	[TestMethod]
	public async Task SuspendCancellationPreservesServiceForResumeWithoutSendingParentError()
	{
		var cancellation = new CancellationToken(canceled: true);
		var externalService = new Mock<IExternalService>();
		externalService.Setup(static service => service.GetResult()).Returns(ValueTask.FromCanceled<DataModelValue>(cancellation));
		var suspendEventDispatcher = new SuspendEventDispatcher();
		suspendEventDispatcher.Suspend(setSuspendRequestedFlag: true);
		var externalCommunication = new Mock<IExternalCommunication>();
		var controller = new PersistedExternalServiceController
						 {
							 ExternalService = externalService.Object,
							 DataConverter = new DataConverter(caseSensitivity: null),
							 ExternalCommunication = externalCommunication.Object,
							 Logger = Mock.Of<ILogger<ExternalServiceController>>(),
							 ExternalServiceInvokeId = Mock.Of<IExternalServiceInvokeId>(),
							 SuspendEventDispatcher = suspendEventDispatcher,
							 ResumeExternalService = null,
							 ResumableExternalService = externalService.Object
						 };

		await Assert.ThrowsExactlyAsync<ExternalServiceSuspendedException>(async () => await controller.WaitForCompletion());
		externalCommunication.Verify(static communication => communication.TrySend(It.IsAny<IOutgoingEvent>()), Times.Never);
	}

	private static InvokeData CreateInvokeData(string id = "invoke") =>
		new(
			InvokeId.FromString(id, "unique-" + id),
			new FullUri("urn:service"),
			new Uri(uriString: "relative/source", UriKind.Relative),
			RawContent: "<content />",
			DataModelValue.FromObject(new DataModelList { ["content"] = 42 }),
			DataModelValue.FromObject(new DataModelList { ["parameter"] = "value" }));

	private static void AssertDataModelListEqual(DataModelValue expected, DataModelValue actual)
	{
		Assert.AreEqual(DataModelValueType.List, actual.Type);
		var expectedList = expected.AsList();
		var actualList = actual.AsList();
		Assert.AreEqual(expectedList.Count, actualList.Count);
		CollectionAssert.AreEqual(expectedList.Keys.ToArray(), actualList.Keys.ToArray());
		CollectionAssert.AreEqual(expectedList.Values.ToArray(), actualList.Values.ToArray());
	}

	private static PersistedExternalServiceController CreateController(IExternalService externalService,
																	   InvokeId invokeId,
																	   IExternalCommunication externalCommunication,
																	   bool resume = false) =>
		new()
		{
			ExternalService = externalService,
			DataConverter = new DataConverter(caseSensitivity: null),
			ExternalCommunication = externalCommunication,
			Logger = Mock.Of<ILogger<ExternalServiceController>>(),
			ExternalServiceInvokeId = Mock.Of<IExternalServiceInvokeId>(value => value.InvokeId == invokeId),
			SuspendEventDispatcher = Mock.Of<ISuspendEventDispatcher>(),
			ResumeExternalService = resume ? Mock.Of<IResumeExternalService>() : null,
			ResumableExternalService = externalService
		};

	private static ExternalServiceCollection CreateCollection() =>
		new()
		{
			DeadLetterQueue = Mock.Of<IDeadLetterQueue<IExternalServiceCollection>>(),
			ExternalServiceGlobalCollection = Mock.Of<IExternalServiceGlobalCollection>()
		};

	private static async ValueTask<PersistedExternalServiceScopeManager> PersistSuspendedInvoke(ITransactionalStorage storage, InvokeData invokeData)
	{
		var externalService = new Mock<IExternalService>();
		externalService.Setup(static service => service.GetResult())
					   .Returns(ValueTask.FromException<DataModelValue>(new ExternalServiceSuspendedException { Owner = new object() }));
		var taskMonitor = new CapturingTaskMonitor();
		var collection = CreateCollection();
		var scopeManager = await CreateScopeManager(storage, collection, externalService.Object, taskMonitor);

		await scopeManager.Start(CreateExternalServiceClass(invokeData, collection), CancellationToken.None);
		await Task.WhenAll(taskMonitor.ForgottenTasks);

		return scopeManager;
	}

	private static async ValueTask<PersistedExternalServiceScopeManager> CreateScopeManager(ITransactionalStorage storage,
																							IExternalServiceCollection collection,
																							IExternalService externalService,
																							ITaskMonitor taskMonitor,
																							Action<InvokeData>? onStart = null,
																							ISuspendEventDispatcher? suspendEventDispatcher = null,
																							IExternalCommunication? externalCommunication = null)
	{
		var communication = externalCommunication ?? Mock.Of<IExternalCommunication>(value => value.TrySend(It.IsAny<IOutgoingEvent>()) == new ValueTask<SendStatus>(SendStatus.Sent));
		var services = new ServiceCollection();
		services.AddConstant(externalService);
		services.AddConstant(new DataConverter(caseSensitivity: null));
		services.AddConstant(communication);
		services.AddConstant(suspendEventDispatcher ?? Mock.Of<ISuspendEventDispatcher>());
		services.AddConstant(Mock.Of<ILogger<ExternalServiceController>>());
		services.AddSharedImplementation<PersistedExternalServiceController>(SharedWithin.Scope).For<IExternalServiceController>();
		var provider = services.BuildProvider();
		var securityContextFactory = new SecurityContextFactory();

		var scopeManager = new PersistedExternalServiceScopeManager
						   {
							   Storage = storage,
							   ResumeExternalServiceClassFactory = invokeData =>
																   {
																	   onStart?.Invoke(invokeData);

																	   return new ValueTask<ResumeExternalServiceClass>(
																		   CreateResumeExternalServiceClass(
																			   invokeData,
																			   collection));
																   },
							   ServiceScopeFactory = await provider.GetRequiredService<IServiceScopeFactory>(),
							   SecurityContextRegistrationFactory = securityContextFactory.GetRegistration,
							   ExternalServiceCollection = collection,
							   TaskMonitor = taskMonitor,
							   DisposeToken = new DisposeToken()
						   };

		return scopeManager;
	}

	private static ExternalServiceClass CreateExternalServiceClass(InvokeData invokeData, IExternalServiceCollection _) =>
		new(
			invokeData,
			Mock.Of<IEventDispatcher>(),
			Mock.Of<IStateMachineSessionId>(service => service.SessionId == SessionId.FromString("parent")),
			Mock.Of<IStateMachineLocation>(),
			Mock.Of<ICaseSensitivity>());

	private static ResumeExternalServiceClass CreateResumeExternalServiceClass(InvokeData invokeData, IExternalServiceCollection _) =>
		new(
			invokeData,
			Mock.Of<IEventDispatcher>(),
			Mock.Of<IStateMachineSessionId>(service => service.SessionId == SessionId.FromString("parent")),
			Mock.Of<IStateMachineLocation>(),
			Mock.Of<ICaseSensitivity>());

	[ExcludeFromCodeCoverage]
	private sealed class CapturingTaskMonitor : ITaskMonitor
	{
		public List<Task> ForgottenTasks { get; } = [];

	#region Interface ITaskMonitor

		public Task WaitAsync(Task task, CancellationToken token) => task.WaitAsync(token);

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task.WaitAsync(token);

		public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => new(valueTask.AsTask().WaitAsync(token));

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) => new(valueTask.AsTask().WaitAsync(token));

		public void Forget(Task task) => ForgottenTasks.Add(task);

		public void Forget(ValueTask valueTask) => ForgottenTasks.Add(valueTask.AsTask());

		public void Forget<TResult>(ValueTask<TResult> valueTask) => ForgottenTasks.Add(valueTask.AsTask());

	#endregion
	}

	private sealed class TestTransactionalStorage : ITransactionalStorage
	{
		private readonly InMemoryStorage _storage = new(writeOnly: false);

		public int CheckPointCount { get; private set; }

		public int ShrinkCount { get; private set; }

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			Dispose();

			return ValueTask.CompletedTask;
		}

	#endregion

	#region Interface IDisposable

		public void Dispose() => _storage.Dispose();

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
			CheckPointCount++;

			return ValueTask.CompletedTask;
		}

		public ValueTask Shrink()
		{
			ShrinkCount++;

			return ValueTask.CompletedTask;
		}

	#endregion
	}
}
