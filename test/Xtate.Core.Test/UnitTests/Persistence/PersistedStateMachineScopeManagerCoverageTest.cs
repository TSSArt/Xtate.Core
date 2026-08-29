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
using Xtate.Interpreter;
using Xtate.IoC;
using Xtate.Persistence;
using Xtate.Persistence.DependencyInjection;
using Xtate.Persistence.Services;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;
using Xtate.TaskMonitor;

namespace Xtate.Core.Test.UnitTests.Persistence;

[TestClass]
public class PersistedStateMachineScopeManagerCoverageTest
{
	[TestMethod]
	public async Task CompletedStateMachineIsNotRestored()
	{
		await using var storage = new TestTransactionalStorage();
		var sessionId = SessionId.FromString("completed-scope-session");
		var snapshots = new SnapshotCollection();
		var behavior = ControllerBehavior.Completed;

		var firstManager = await CreateManager(storage, new CapturingTaskMonitor(), snapshots, behavior);
		await firstManager.InitializeAsync();

		var result = await firstManager.Start(new TestStateMachineClass { SessionId = sessionId }, SecurityContextType.NewStateMachine);
		await result.GetResult();

		var secondManager = await CreateManager(storage, new CapturingTaskMonitor(), snapshots, behavior);
		await secondManager.InitializeAsync();

		Assert.AreEqual(expected: 1, snapshots.Items.Count);
		Assert.AreEqual(sessionId, snapshots.Items.Single());
	}

	[TestMethod]
	public async Task SuspendedStateMachineIsResumedWithItsSessionId()
	{
		await using var storage = new TestTransactionalStorage();
		var sessionId = SessionId.FromString("persisted-scope-session");
		var original = new TestStateMachineClass { SessionId = sessionId };
		var snapshots = new SnapshotCollection();

		var firstMonitor = new CapturingTaskMonitor();
		var firstManager = await CreateManager(storage, firstMonitor, snapshots);
		await firstManager.InitializeAsync();

		var suspendedResult = await firstManager.Start(original, SecurityContextType.NewStateMachine);
		await Assert.ThrowsExactlyAsync<StateMachineSuspendedException>(async () => await suspendedResult.GetResult());

		var secondMonitor = new CapturingTaskMonitor();
		var secondManager = await CreateManager(storage, secondMonitor, snapshots);
		await secondManager.InitializeAsync();

		Assert.HasCount(expected: 2, snapshots.Items);
		Assert.AreEqual(sessionId, snapshots.Items[1]);
		Assert.HasCount(expected: 1, secondMonitor.ForgottenResultTasks);
		await Assert.ThrowsExactlyAsync<StateMachineSuspendedException>(async () => await secondMonitor.ForgottenResultTasks[0]);
	}

	[TestMethod]
	public async Task CompletedInvokedStateMachineIsNotRestored()
	{
		await using var storage = new TestTransactionalStorage();
		var original = CreateInvokedStateMachine("completed-invoked");
		var snapshots = new InvocationSnapshotCollection();

		var firstManager = await CreateInvokedManager(storage, new CapturingTaskMonitor(), snapshots, ControllerBehavior.Completed);
		await firstManager.InitializeAsync();

		var result = await firstManager.Start(original, SecurityContextType.InvokedService);
		await result.GetResult();

		var secondManager = await CreateInvokedManager(storage, new CapturingTaskMonitor(), snapshots, ControllerBehavior.Completed);
		await secondManager.InitializeAsync();

		Assert.HasCount(expected: 1, snapshots.Items);
		AssertInvocationSnapshot(original, snapshots.Items.Single());
	}

	[TestMethod]
	public async Task SuspendedInvokedStateMachineIsNotRestoredByHostScopeManager()
	{
		await using var storage = new TestTransactionalStorage();
		var original = CreateInvokedStateMachine("suspended-invoked");
		var snapshots = new InvocationSnapshotCollection();

		var firstManager = await CreateInvokedManager(storage, new CapturingTaskMonitor(), snapshots, ControllerBehavior.Suspended);
		await firstManager.InitializeAsync();

		var result = await firstManager.Start(original, SecurityContextType.InvokedService);
		await Assert.ThrowsExactlyAsync<StateMachineSuspendedException>(async () => await result.GetResult());

		var secondMonitor = new CapturingTaskMonitor();
		var secondManager = await CreateInvokedManager(storage, secondMonitor, snapshots, ControllerBehavior.Suspended);
		await secondManager.InitializeAsync();

		Assert.HasCount(expected: 1, snapshots.Items);
		AssertInvocationSnapshot(original, snapshots.Items.Single());
		Assert.IsEmpty(secondMonitor.ForgottenResultTasks);
	}

	[TestMethod]
	public async Task SuspendedParentAndInvokedChildResumeOnlyParent()
	{
		await using var storage = new TestTransactionalStorage();
		var parentSessionId = SessionId.FromString("mixed-parent");
		var child = CreateInvokedStateMachine("mixed-child");
		var snapshots = new SnapshotCollection();
		var behavior = ControllerBehavior.Suspended;
		var firstManager = await CreateManager(storage, new CapturingTaskMonitor(), snapshots, behavior);
		await firstManager.InitializeAsync();

		var parentResult = await firstManager.Start(new TestStateMachineClass { SessionId = parentSessionId }, SecurityContextType.NewStateMachine);
		var childResult = await firstManager.Start(child, SecurityContextType.InvokedService);
		await Assert.ThrowsExactlyAsync<StateMachineSuspendedException>(async () => await parentResult.GetResult());
		await Assert.ThrowsExactlyAsync<StateMachineSuspendedException>(async () => await childResult.GetResult());

		var secondMonitor = new CapturingTaskMonitor();
		var secondManager = await CreateManager(storage, secondMonitor, snapshots, behavior);
		await secondManager.InitializeAsync();

		CollectionAssert.AreEqual(
			new[] { parentSessionId, child.SessionId, parentSessionId },
			snapshots.Items);
		Assert.HasCount(expected: 1, secondMonitor.ForgottenResultTasks);
		await Assert.ThrowsExactlyAsync<StateMachineSuspendedException>(async () => await secondMonitor.ForgottenResultTasks.Single());
	}

	[TestMethod]
	public async Task MixedCompletedAndSuspendedParentsRestoreOnlySuspendedEntriesOnce()
	{
		await using var storage = new TestTransactionalStorage();
		var completedSessionId = SessionId.FromString("mixed-completed-parent");
		var firstSuspendedSessionId = SessionId.FromString("mixed-suspended-parent-one");
		var secondSuspendedSessionId = SessionId.FromString("mixed-suspended-parent-two");
		var snapshots = new SnapshotCollection();
		var behavior = new ControllerBehavior(sessionId =>
												  sessionId == completedSessionId
													  ? new ValueTask<DataModelValue>(DataModelValue.Undefined)
													  : ValueTask.FromException<DataModelValue>(new StateMachineSuspendedException { Owner = sessionId }));
		var firstManager = await CreateManager(storage, new CapturingTaskMonitor(), snapshots, behavior);
		await firstManager.InitializeAsync();

		var completedResult = await firstManager.Start(new TestStateMachineClass { SessionId = completedSessionId }, SecurityContextType.NewStateMachine);
		var firstSuspendedResult = await firstManager.Start(new TestStateMachineClass { SessionId = firstSuspendedSessionId }, SecurityContextType.NewStateMachine);
		var secondSuspendedResult = await firstManager.Start(new TestStateMachineClass { SessionId = secondSuspendedSessionId }, SecurityContextType.NewStateMachine);
		await completedResult.GetResult();
		await Assert.ThrowsExactlyAsync<StateMachineSuspendedException>(async () => await firstSuspendedResult.GetResult());
		await Assert.ThrowsExactlyAsync<StateMachineSuspendedException>(async () => await secondSuspendedResult.GetResult());

		var secondMonitor = new CapturingTaskMonitor();
		var secondManager = await CreateManager(storage, secondMonitor, snapshots, behavior);
		await secondManager.InitializeAsync();

		Assert.AreEqual(expected: 1, snapshots.Items.Count(id => id == completedSessionId));
		Assert.AreEqual(expected: 2, snapshots.Items.Count(id => id == firstSuspendedSessionId));
		Assert.AreEqual(expected: 2, snapshots.Items.Count(id => id == secondSuspendedSessionId));
		Assert.HasCount(expected: 2, secondMonitor.ForgottenResultTasks);

		foreach (var task in secondMonitor.ForgottenResultTasks)
		{
			await Assert.ThrowsExactlyAsync<StateMachineSuspendedException>(async () => await task);
		}
	}

	private static async ValueTask<PersistedStateMachineScopeManager> CreateManager(ITransactionalStorage storage,
																					ITaskMonitor taskMonitor,
																					SnapshotCollection snapshots,
																					ControllerBehavior? behavior = null)
	{
		var services = new ServiceCollection();
		services.AddModule<PersistenceModule>();
		services.AddConstant(snapshots);
		services.AddConstant(behavior ?? ControllerBehavior.Suspended);
		services.AddImplementation<CapturingController>().For<IStateMachineController>();

		return await CreateManager(storage, taskMonitor, services);
	}

	private static async ValueTask<PersistedStateMachineScopeManager> CreateInvokedManager(ITransactionalStorage storage,
																						   ITaskMonitor taskMonitor,
																						   InvocationSnapshotCollection snapshots,
																						   ControllerBehavior behavior)
	{
		var services = new ServiceCollection();
		services.AddModule<PersistenceModule>();
		services.AddConstant(snapshots);
		services.AddConstant(behavior);
		services.AddImplementation<CapturingInvokedController>().For<IStateMachineController>();

		return await CreateManager(storage, taskMonitor, services);
	}

	private static async ValueTask<PersistedStateMachineScopeManager> CreateManager(ITransactionalStorage storage,
																					ITaskMonitor taskMonitor,
																					ServiceCollection services)
	{
		var serviceProvider = services.BuildProvider();
		var securityContextFactory = new SecurityContextFactory();

		return new TestPersistedStateMachineScopeManager
			   {
				   Storage = storage,
				   StorageManager = new StorageManager
									{
										StorageProvider = Mock.Of<IStorageProvider>(),
										StateMachineSessionId = Mock.Of<IStateMachineSessionId>()
									},
				   PersistedTaskMonitor = taskMonitor,
				   ServiceScopeFactory = await serviceProvider.GetRequiredService<IServiceScopeFactory>(),
				   StateMachineCollection = Mock.Of<IStateMachineCollection>(),
				   SecurityContextRegistrationFactory = securityContextFactory.GetRegistration,
				   TaskMonitor = taskMonitor
			   };
	}

	private static TestInvokedStateMachineClass CreateInvokedStateMachine(string id) =>
		new()
		{
			SessionId = SessionId.FromString(id + "-session"),
			ParentSessionId = SessionId.FromString(id + "-parent"),
			InvokeId = InvokeId.FromString(id + "-invoke", id + "-unique"),
			Type = new FullUri("urn:" + id)
		};

	private static void AssertInvocationSnapshot(TestInvokedStateMachineClass expected, InvocationSnapshot actual)
	{
		Assert.AreEqual(expected.SessionId, actual.SessionId);
		Assert.AreEqual(expected.ParentSessionId, actual.ParentSessionId);
		Assert.AreEqual(expected.InvokeId, actual.InvokeId);
		Assert.AreEqual(expected.Type, actual.Type);
	}

	private sealed class TestPersistedStateMachineScopeManager : PersistedStateMachineScopeManager;

	private sealed class TestStateMachineClass : StateMachineClass;

	private sealed class TestInvokedStateMachineClass : StateMachineClass, IInvokedStateMachine
	{
	#region Interface IExternalServiceInvokeId

		public required InvokeId InvokeId { get; init; }

	#endregion

	#region Interface IExternalServiceType

		public required FullUri Type { get; init; }

	#endregion

	#region Interface IParentStateMachineSessionId

		public required SessionId ParentSessionId { get; init; }

	#endregion

		public override void AddServices(IServiceCollection services)
		{
			base.AddServices(services);

			services.AddForwarding<IInvokedStateMachine>(_ => this);
			services.AddForwarding<IParentStateMachineSessionId>(_ => this);
			services.AddForwarding<IExternalServiceInvokeId>(_ => this);
			services.AddForwarding<IExternalServiceType>(_ => this);
		}
	}

	private sealed class SnapshotCollection
	{
		public List<SessionId> Items { get; } = [];
	}

	private sealed class InvocationSnapshotCollection
	{
		public List<InvocationSnapshot> Items { get; } = [];
	}

	private sealed record InvocationSnapshot(
		SessionId SessionId,
		SessionId ParentSessionId,
		InvokeId InvokeId,
		FullUri Type);

	private sealed class ControllerBehavior(Func<SessionId, ValueTask<DataModelValue>> getResult)
	{
		public static ControllerBehavior Completed { get; } = new(_ => new ValueTask<DataModelValue>(DataModelValue.Undefined));

		public static ControllerBehavior Suspended { get; } =
			new(sessionId => ValueTask.FromException<DataModelValue>(new StateMachineSuspendedException { Owner = sessionId }));

		public ValueTask<DataModelValue> GetResult(SessionId sessionId) => getResult(sessionId);
	}

	[InstantiatedByIoC]
	private sealed class CapturingController : IStateMachineController
	{
		private readonly ControllerBehavior _behavior;

		private readonly SessionId _sessionId;

		public CapturingController(SnapshotCollection snapshots, IStateMachineSessionId sessionId, ControllerBehavior behavior)
		{
			_sessionId = sessionId.SessionId;
			_behavior = behavior;
			snapshots.Items.Add(_sessionId);
		}

	#region Interface IEventDispatcher

		public ValueTask Dispatch(IIncomingEvent incomingEvent, CancellationToken token) => ValueTask.CompletedTask;

	#endregion

	#region Interface IExternalService

		public ValueTask<DataModelValue> GetResult() => _behavior.GetResult(_sessionId);

	#endregion

	#region Interface IStateMachineController

		public ValueTask Destroy() => ValueTask.CompletedTask;

	#endregion
	}

	[InstantiatedByIoC]
	private sealed class CapturingInvokedController : IStateMachineController
	{
		private readonly ControllerBehavior _behavior;

		private readonly SessionId _sessionId;

		public CapturingInvokedController(InvocationSnapshotCollection snapshots,
										  IStateMachineSessionId sessionId,
										  IParentStateMachineSessionId parentSessionId,
										  IExternalServiceInvokeId invokeId,
										  IExternalServiceType type,
										  ControllerBehavior behavior)
		{
			_sessionId = sessionId.SessionId;
			_behavior = behavior;
			snapshots.Items.Add(new InvocationSnapshot(_sessionId, parentSessionId.ParentSessionId, invokeId.InvokeId, type.Type));
		}

	#region Interface IEventDispatcher

		public ValueTask Dispatch(IIncomingEvent incomingEvent, CancellationToken token) => ValueTask.CompletedTask;

	#endregion

	#region Interface IExternalService

		public ValueTask<DataModelValue> GetResult() => _behavior.GetResult(_sessionId);

	#endregion

	#region Interface IStateMachineController

		public ValueTask Destroy() => ValueTask.CompletedTask;

	#endregion
	}

	private sealed class CapturingTaskMonitor : ITaskMonitor
	{
		// ReSharper disable once CollectionNeverQueried.Local
		private List<Task> ForgottenTasks { get; } = [];

		public List<Task<DataModelValue>> ForgottenResultTasks { get; } = [];

	#region Interface ITaskMonitor

		public Task WaitAsync(Task task, CancellationToken token) => task.WaitAsync(token);

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task.WaitAsync(token);

		public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => new(valueTask.AsTask().WaitAsync(token));

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) => new(valueTask.AsTask().WaitAsync(token));

		public void Forget(Task task)
		{
			if (task is Task<DataModelValue> resultTask)
			{
				ForgottenResultTasks.Add(resultTask);

				return;
			}

			ForgottenTasks.Add(task);
		}

		public void Forget(ValueTask valueTask) => ForgottenTasks.Add(valueTask.AsTask());

		public void Forget<TResult>(ValueTask<TResult> valueTask) => ForgottenTasks.Add(valueTask.AsTask());

	#endregion
	}

	private sealed class TestTransactionalStorage : ITransactionalStorage
	{
		private readonly InMemoryStorage _storage = new(writeOnly: false);

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

		public ValueTask CheckPoint(int level) => ValueTask.CompletedTask;

		public ValueTask Shrink() => ValueTask.CompletedTask;

	#endregion
	}
}
