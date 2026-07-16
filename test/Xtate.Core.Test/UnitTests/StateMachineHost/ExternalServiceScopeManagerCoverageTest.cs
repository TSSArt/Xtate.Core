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
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;
using Xtate.TaskMonitor;

namespace Xtate.Test.UnitTests.StateMachineHost;

[TestClass]
public class ExternalServiceScopeManagerCoverageTest
{
	[TestMethod]
	public async Task StartRegistersServiceWaitsForRunnerAndCleansUpAfterCompletion()
	{
		var invokeId = InvokeId.FromString("invoke");
		var runnerCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var runner = new Mock<IExternalServiceRunner>();
		runner.Setup(static r => r.WaitForCompletion()).Returns(new ValueTask(runnerCompletion.Task));
		var externalService = Mock.Of<IExternalService>();
		var collection = new Mock<IExternalServiceCollection>();
		var monitor = new CapturingTaskMonitor();
		var manager = await CreateManager(runner.Object, externalService, collection.Object, monitor);

		await manager.Start(CreateInvokeData(invokeId), CancellationToken.None);

		collection.Verify(c => c.Register(invokeId), Times.Once);
		collection.Verify(c => c.SetExternalService(invokeId, externalService), Times.Once);
		Assert.HasCount(expected: 1, monitor.ForgottenTasks);
		Assert.IsFalse(monitor.ForgottenTasks[0].IsCompleted);

		runnerCompletion.SetResult();
		var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromSeconds(5));
		await monitor.ForgottenTasks[0].WaitAsync(cts.Token);
		collection.Verify(c => c.Unregister(invokeId), Times.Once);
		await manager.Cancel(invokeId, CancellationToken.None);
		await manager.DisposeAsync();
	}

	[TestMethod]
	public async Task CancelDisposesActiveScopeAndCompletionStillUnregistersService()
	{
		var invokeId = InvokeId.FromString("invoke");
		var runnerCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var runner = new Mock<IExternalServiceRunner>();
		runner.Setup(static r => r.WaitForCompletion()).Returns(new ValueTask(runnerCompletion.Task));
		var collection = new Mock<IExternalServiceCollection>();
		var monitor = new CapturingTaskMonitor();
		var manager = await CreateManager(runner.Object, Mock.Of<IExternalService>(), collection.Object, monitor);

		await manager.Start(CreateInvokeData(invokeId), CancellationToken.None);
		await manager.Cancel(invokeId, CancellationToken.None);
		await manager.Cancel(invokeId, CancellationToken.None);
		runnerCompletion.SetResult();
		var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromSeconds(5));
		await monitor.ForgottenTasks.Single().WaitAsync(cts.Token);

		collection.Verify(c => c.Unregister(invokeId), Times.Once);
		manager.Dispose();
	}

	[TestMethod]
	public async Task FailedClassFactoryRunsCleanupAndDoesNotLeaveScope()
	{
		var invokeId = InvokeId.FromString("invoke");
		var failure = new InvalidOperationException("class creation failed");
		var collection = new Mock<IExternalServiceCollection>();
		var securityContextFactory = new SecurityContextFactory();
		var services = new ServiceCollection();
		var provider = services.BuildProvider();
		var manager = new ExternalServiceScopeManager
					  {
						  ExternalServiceClassFactory = _ => ValueTask.FromException<ExternalServiceClass>(failure),
						  ServiceScopeFactory = await provider.GetRequiredService<IServiceScopeFactory>(),
						  SecurityContextRegistrationFactory = securityContextFactory.GetRegistration,
						  ExternalServiceCollection = collection.Object,
						  TaskMonitor = new CapturingTaskMonitor()
					  };

		var thrown = await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () =>
																					await manager.Start(CreateInvokeData(invokeId), CancellationToken.None));

		Assert.AreSame(failure, thrown);
		collection.Verify(c => c.Unregister(invokeId), Times.Once);
		manager.Dispose();
	}

	[TestMethod]
	public async Task DisposedManagerRejectsNewScopesInBothDisposalModes()
	{
		var runner = Mock.Of<IExternalServiceRunner>();
		var collection = new Mock<IExternalServiceCollection>();
		var syncManager = await CreateManager(runner, Mock.Of<IExternalService>(), collection.Object, new CapturingTaskMonitor());
		syncManager.Dispose();
		syncManager.Dispose();

		await Assert.ThrowsExactlyAsync<ObjectDisposedException>([ExcludeFromCodeCoverage] async () =>
																	 await syncManager.Start(CreateInvokeData(InvokeId.FromString("sync")), CancellationToken.None));

		var asyncManager = await CreateManager(runner, Mock.Of<IExternalService>(), collection.Object, new CapturingTaskMonitor());
		await asyncManager.DisposeAsync();
		await asyncManager.DisposeAsync();
		await Assert.ThrowsExactlyAsync<ObjectDisposedException>([ExcludeFromCodeCoverage] async () =>
																	 await asyncManager.Start(CreateInvokeData(InvokeId.FromString("async")), CancellationToken.None));
	}

	private static async ValueTask<ExternalServiceScopeManager> CreateManager(IExternalServiceRunner runner,
																			  IExternalService externalService,
																			  IExternalServiceCollection collection,
																			  ITaskMonitor taskMonitor)
	{
		var services = new ServiceCollection();
		services.AddConstant(runner);
		services.AddConstant(externalService);
		var provider = services.BuildProvider();
		var securityContextFactory = new SecurityContextFactory();

		return new ExternalServiceScopeManager
			   {
				   ExternalServiceClassFactory = invokeData => new ValueTask<ExternalServiceClass>(CreateExternalServiceClass(invokeData)),
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