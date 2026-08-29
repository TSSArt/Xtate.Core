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

// ReSharper disable MethodHasAsyncOverload

using System.Threading;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.IoC;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;
using Xtate.TaskMonitor;

namespace Xtate.Core.Test.UnitTests.StateMachineHost;

[TestClass]
public class ExternalServiceScopeManagerCoverageTest
{
	[TestMethod]
	public async Task StartRegistersServiceWaitsForControllerAndCleansUpAfterCompletion()
	{
		var invokeId = InvokeId.FromString("invoke");
		var controllerCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var controller = new Mock<IExternalServiceController>();
		controller.Setup(static c => c.WaitForCompletion()).Returns(new ValueTask(controllerCompletion.Task));
		var collection = new Mock<IExternalServiceCollection>();
		var monitor = new CapturingTaskMonitor();
		var manager = await CreateManager(controller.Object, collection.Object, monitor);

		await manager.Start(CreateExternalServiceClass(CreateInvokeData(invokeId)), CancellationToken.None);

		collection.Verify(c => c.Register(invokeId), Times.Once);
		collection.Verify(c => c.SetController(invokeId, controller.Object), Times.Once);
		Assert.HasCount(expected: 1, monitor.ForgottenTasks);
		Assert.IsFalse(monitor.ForgottenTasks[0].IsCompleted);

		controllerCompletion.SetResult();
		var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromSeconds(10));
		await monitor.ForgottenTasks[0].WaitAsync(cts.Token);
		collection.Verify(c => c.Unregister(invokeId), Times.Once);
		await manager.Cancel(invokeId, CancellationToken.None);
		await manager.DisposeAsync();
	}

	[TestMethod]
	public async Task CancelDisposesActiveScopeAndCompletionStillUnregistersService()
	{
		var invokeId = InvokeId.FromString("invoke");
		var controllerCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var controller = new Mock<IExternalServiceController>();
		controller.Setup(static c => c.WaitForCompletion()).Returns(new ValueTask(controllerCompletion.Task));
		var collection = new Mock<IExternalServiceCollection>();
		var monitor = new CapturingTaskMonitor();
		var manager = await CreateManager(controller.Object, collection.Object, monitor);

		await manager.Start(CreateExternalServiceClass(CreateInvokeData(invokeId)), CancellationToken.None);
		await manager.Cancel(invokeId, CancellationToken.None);
		await manager.Cancel(invokeId, CancellationToken.None);
		controllerCompletion.SetResult();
		var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromSeconds(10));
		await monitor.ForgottenTasks.Single().WaitAsync(cts.Token);

		collection.Verify(c => c.Unregister(invokeId), Times.Once);
		manager.Dispose();
	}

	[TestMethod]
	public async Task FailedControllerResolutionRunsCleanupAndDoesNotLeaveScope()
	{
		var invokeId = InvokeId.FromString("invoke");
		var collection = new Mock<IExternalServiceCollection>();
		var securityContextFactory = new SecurityContextFactory();
		var services = new ServiceCollection();
		var provider = services.BuildProvider();
		var manager = new ExternalServiceScopeManager
					  {
						  ServiceScopeFactory = await provider.GetRequiredService<IServiceScopeFactory>(),
						  SecurityContextRegistrationFactory = securityContextFactory.GetRegistration,
						  ExternalServiceCollection = collection.Object,
						  TaskMonitor = new CapturingTaskMonitor()
					  };

		await Assert.ThrowsExactlyAsync<MissedServiceException>([ExcludeFromCodeCoverage] async () =>
																	await manager.Start(CreateExternalServiceClass(CreateInvokeData(invokeId)), CancellationToken.None));

		collection.Verify(c => c.Unregister(invokeId), Times.Once);
		manager.Dispose();
	}

	[TestMethod]
	public async Task DisposedManagerRejectsNewScopesInBothDisposalModes()
	{
		var controller = Mock.Of<IExternalServiceController>();
		var collection = new Mock<IExternalServiceCollection>();
		var syncManager = await CreateManager(controller, collection.Object, new CapturingTaskMonitor());
		syncManager.Dispose();
		syncManager.Dispose();

		await Assert.ThrowsExactlyAsync<ObjectDisposedException>([ExcludeFromCodeCoverage] async () =>
																	 await syncManager.Start(CreateExternalServiceClass(CreateInvokeData(InvokeId.FromString("sync"))), CancellationToken.None));

		var asyncManager = await CreateManager(controller, collection.Object, new CapturingTaskMonitor());
		await asyncManager.DisposeAsync();
		await asyncManager.DisposeAsync();
		await Assert.ThrowsExactlyAsync<ObjectDisposedException>([ExcludeFromCodeCoverage] async () =>
																	 await asyncManager.Start(CreateExternalServiceClass(CreateInvokeData(InvokeId.FromString("async"))), CancellationToken.None));
	}

	private static async ValueTask<ExternalServiceScopeManager> CreateManager(IExternalServiceController controller,
																			  IExternalServiceCollection collection,
																			  ITaskMonitor taskMonitor)
	{
		var services = new ServiceCollection();
		services.AddConstant(controller);
		var provider = services.BuildProvider();
		var securityContextFactory = new SecurityContextFactory();

		return new ExternalServiceScopeManager
			   {
				   ServiceScopeFactory = await provider.GetRequiredService<IServiceScopeFactory>(),
				   SecurityContextRegistrationFactory = securityContextFactory.GetRegistration,
				   ExternalServiceCollection = collection,
				   TaskMonitor = taskMonitor
			   };
	}

	private static ExternalServiceClass CreateExternalServiceClass(InvokeData invokeData) =>
		new(
			invokeData,
			Mock.Of<IEventDispatcher>(),
			Mock.Of<IStateMachineSessionId>(s => s.SessionId == SessionId.FromString("parent")),
			Mock.Of<IStateMachineLocation>(),
			Mock.Of<ICaseSensitivity>());

	private static InvokeData CreateInvokeData(InvokeId invokeId) => new(invokeId, new FullUri("urn:service"), Source: null, RawContent: null, DataModelValue.Undefined, DataModelValue.Undefined);

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
}
