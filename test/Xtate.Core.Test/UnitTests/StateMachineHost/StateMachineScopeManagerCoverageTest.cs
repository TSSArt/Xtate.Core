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
using Xtate.DataTypes;
using Xtate.IoC;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;
using Xtate.TaskMonitor;

namespace Xtate.Test.UnitTests.StateMachineHost;

[TestClass]
public class StateMachineScopeManagerCoverageTest
{
	[TestMethod]
	public async Task ExecuteRegistersControllerReturnsResultAndCleansUp()
	{
		var sessionId = SessionId.FromString("execute-session");
		var controller = new Mock<IStateMachineController>();
		controller.Setup(static value => value.GetResult()).Returns(new ValueTask<DataModelValue>(new DataModelValue("execute-result")));
		var collection = new Mock<IStateMachineCollection>();
		var manager = await CreateManager(controller.Object, collection.Object, new CapturingTaskMonitor());

		var result = await manager.Execute(CreateStateMachine(sessionId), SecurityContextType.NewTrustedStateMachine);

		Assert.AreEqual("execute-result", result.AsString());
		collection.Verify(value => value.Register(sessionId), Times.Once);
		collection.Verify(value => value.SetController(sessionId, controller.Object), Times.Once);
		collection.Verify(value => value.Unregister(sessionId), Times.Once);
		controller.Verify(static value => value.GetResult(), Times.Exactly(2));
		manager.Dispose();
	}

	[TestMethod]
	public async Task ExecuteFailureStillUnregistersAndDisposesScope()
	{
		var sessionId = SessionId.FromString("failed-execute-session");
		var failure = new InvalidOperationException("execution failed");
		var controller = new Mock<IStateMachineController>();
		controller.Setup(static value => value.GetResult()).Returns(ValueTask.FromException<DataModelValue>(failure));
		var collection = new Mock<IStateMachineCollection>();
		var manager = await CreateManager(controller.Object, collection.Object, new CapturingTaskMonitor());

		var thrown = await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () =>
			await manager.Execute(CreateStateMachine(sessionId), SecurityContextType.NewStateMachine));

		Assert.AreSame(failure, thrown);
		collection.Verify(value => value.Unregister(sessionId), Times.Once);
		controller.Verify(static value => value.GetResult(), Times.Exactly(2));
		await manager.DisposeAsync();
	}

	[TestMethod]
	public async Task StartCompletesResultAndCleanupThroughTaskMonitor()
	{
		var sessionId = SessionId.FromString("start-session");
		var completion = new TaskCompletionSource<DataModelValue>(TaskCreationOptions.RunContinuationsAsynchronously);
		var controller = new Mock<IStateMachineController>();
		controller.Setup(static value => value.GetResult()).Returns(new ValueTask<DataModelValue>(completion.Task));
		var collection = new Mock<IStateMachineCollection>();
		var monitor = new CapturingTaskMonitor();
		var manager = await CreateManager(controller.Object, collection.Object, monitor);

		var stateMachineResult = await manager.Start(CreateStateMachine(sessionId), SecurityContextType.NewTrustedStateMachine);
		Assert.HasCount(expected: 1, monitor.ForgottenTasks);
		Assert.IsFalse(monitor.ForgottenTasks[0].IsCompleted);

		completion.SetResult(new DataModelValue("started-result"));
		Assert.AreEqual("started-result", (await stateMachineResult.GetResult()).AsString());
		await monitor.ForgottenTasks[0].WaitAsync(TimeSpan.FromSeconds(5));
		collection.Verify(value => value.Unregister(sessionId), Times.Once);
		manager.Dispose();
	}

	[TestMethod]
	public async Task StartPropagatesControllerFailureAfterCleanup()
	{
		var sessionId = SessionId.FromString("failed-start-session");
		var failure = new InvalidOperationException("detached execution failed");
		var completion = new TaskCompletionSource<DataModelValue>(TaskCreationOptions.RunContinuationsAsynchronously);
		var controller = new Mock<IStateMachineController>();
		controller.Setup(static value => value.GetResult()).Returns(new ValueTask<DataModelValue>(completion.Task));
		var collection = new Mock<IStateMachineCollection>();
		var monitor = new CapturingTaskMonitor();
		var manager = await CreateManager(controller.Object, collection.Object, monitor);
		var stateMachineResult = await manager.Start(CreateStateMachine(sessionId), SecurityContextType.NewStateMachine);

		completion.SetException(failure);
		var thrown = await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () =>
			await stateMachineResult.GetResult());
		await monitor.ForgottenTasks.Single().WaitAsync(TimeSpan.FromSeconds(5));

		Assert.AreSame(failure, thrown);
		collection.Verify(value => value.Unregister(sessionId), Times.Once);
		await manager.DisposeAsync();
	}

	[TestMethod]
	public async Task DuplicateSessionDestroyDestroyAllAndTerminateCoverActiveAndMissingScopes()
	{
		var sessionId = SessionId.FromString("managed-session");
		var resultCompletion = new TaskCompletionSource<DataModelValue>(TaskCreationOptions.RunContinuationsAsynchronously);
		var destroyCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var controller = new Mock<IStateMachineController>();
		controller.Setup(static value => value.GetResult()).Returns(new ValueTask<DataModelValue>(resultCompletion.Task));
		controller.Setup(static value => value.Destroy()).Returns(new ValueTask(destroyCompletion.Task));
		var collection = new Mock<IStateMachineCollection>();
		var monitor = new CapturingTaskMonitor();
		var manager = await CreateManager(controller.Object, collection.Object, monitor);
		var stateMachine = CreateStateMachine(sessionId);
		var result = await manager.Start(stateMachine, SecurityContextType.NewTrustedStateMachine);

		await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () =>
			await manager.Start(stateMachine, SecurityContextType.NewTrustedStateMachine));
		await manager.Destroy(SessionId.FromString("missing-session"));
		var destroyAll = manager.DestroyAll();
		Assert.IsFalse(destroyAll.IsCompletedSuccessfully);
		destroyCompletion.SetResult();
		await destroyAll;
		await manager.DestroyAll();
		controller.Verify(static value => value.Destroy(), Times.Exactly(2));

		await manager.Terminate(sessionId);
		await manager.Terminate(sessionId);
		await manager.Destroy(sessionId);
		resultCompletion.SetResult(DataModelValue.Null);
		Assert.AreEqual(DataModelValue.Null, await result.GetResult());
		await monitor.ForgottenTasks.Single().WaitAsync(TimeSpan.FromSeconds(5));
		collection.Verify(value => value.Unregister(sessionId), Times.Once);
		manager.Dispose();
	}

	[TestMethod]
	public async Task BothDisposalModesRejectNewScopesAndAllowRepeatedDisposal()
	{
		var controller = Mock.Of<IStateMachineController>();
		var syncManager = await CreateManager(controller, Mock.Of<IStateMachineCollection>(), new CapturingTaskMonitor());
		syncManager.Dispose();
		syncManager.Dispose();
		await Assert.ThrowsExactlyAsync<ObjectDisposedException>([ExcludeFromCodeCoverage] async () =>
			await syncManager.Start(CreateStateMachine(SessionId.FromString("sync-disposed")), SecurityContextType.NewTrustedStateMachine));

		var asyncManager = await CreateManager(controller, Mock.Of<IStateMachineCollection>(), new CapturingTaskMonitor());
		await asyncManager.DisposeAsync();
		await asyncManager.DisposeAsync();
		await Assert.ThrowsExactlyAsync<ObjectDisposedException>([ExcludeFromCodeCoverage] async () =>
			await asyncManager.Execute(CreateStateMachine(SessionId.FromString("async-disposed")), SecurityContextType.NewTrustedStateMachine));
	}

	private static RuntimeStateMachine CreateStateMachine(SessionId sessionId) =>
		new(Mock.Of<IStateMachine>(static value => value.Name == "coverage-machine")) { SessionId = sessionId };

	private static async ValueTask<StateMachineScopeManager> CreateManager(
		IStateMachineController controller,
		IStateMachineCollection collection,
		ITaskMonitor taskMonitor)
	{
		var services = new ServiceCollection();
		services.AddConstant(controller);
		var provider = services.BuildProvider();
		var securityContextFactory = new SecurityContextFactory();

		return new StateMachineScopeManager
			   {
				   ServiceScopeFactory = await provider.GetRequiredService<IServiceScopeFactory>(),
				   StateMachineCollection = collection,
				   SecurityContextRegistrationFactory = securityContextFactory.GetRegistration,
				   TaskMonitor = taskMonitor
			   };
	}

	[ExcludeFromCodeCoverage]
	private sealed class CapturingTaskMonitor : ITaskMonitor
	{
		public List<Task> ForgottenTasks { get; } = [];

		public Task WaitAsync(Task task, CancellationToken token) => task.WaitAsync(token);

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task.WaitAsync(token);

		public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => new(valueTask.AsTask().WaitAsync(token));

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) => new(valueTask.AsTask().WaitAsync(token));

		public void Forget(Task task) => ForgottenTasks.Add(task);

		public void Forget(ValueTask valueTask) => ForgottenTasks.Add(valueTask.AsTask());

		public void Forget<TResult>(ValueTask<TResult> valueTask) => ForgottenTasks.Add(valueTask.AsTask());
	}
}
