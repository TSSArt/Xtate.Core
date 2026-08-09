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

	private static async ValueTask<PersistedStateMachineScopeManager> CreateManager(ITransactionalStorage storage,
																					ITaskMonitor taskMonitor,
																					SnapshotCollection snapshots)
	{
		var services = new ServiceCollection();
		services.AddConstant(snapshots);
		services.AddImplementation<CapturingController>().For<IStateMachineController>();
		var serviceProvider = services.BuildProvider();
		var securityContextFactory = new SecurityContextFactory();

		return new TestPersistedStateMachineScopeManager
			   {
				   Storage = storage,
				   StorageManager = null!,
				   PersistedTaskMonitor = taskMonitor,
				   ServiceScopeFactory = await serviceProvider.GetRequiredService<IServiceScopeFactory>(),
				   StateMachineCollection = Mock.Of<IStateMachineCollection>(),
				   SecurityContextRegistrationFactory = securityContextFactory.GetRegistration,
				   TaskMonitor = taskMonitor
			   };
	}

	private sealed class TestPersistedStateMachineScopeManager : PersistedStateMachineScopeManager;

	private sealed class TestStateMachineClass : StateMachineClass;

	private sealed class SnapshotCollection
	{
		public List<SessionId> Items { get; } = [];
	}

	[InstantiatedByIoC]
	private sealed class CapturingController : IStateMachineController
	{
		private readonly SessionId _sessionId;

		public CapturingController(SnapshotCollection snapshots, IStateMachineSessionId sessionId)
		{
			_sessionId = sessionId.SessionId;
			snapshots.Items.Add(_sessionId);
		}

	#region Interface IEventDispatcher

		public ValueTask Dispatch(IIncomingEvent incomingEvent, CancellationToken token) => ValueTask.CompletedTask;

	#endregion

	#region Interface IExternalService

		public ValueTask<DataModelValue> GetResult() => ValueTask.FromException<DataModelValue>(new StateMachineSuspendedException { Owner = _sessionId });

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
