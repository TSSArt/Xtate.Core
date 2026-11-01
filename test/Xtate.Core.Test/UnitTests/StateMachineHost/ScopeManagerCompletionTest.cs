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
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;
using Xtate.TaskMonitor;

namespace Xtate.Core.Test.UnitTests.StateMachineHost;

[TestClass]
public class ScopeManagerCompletionTest
{
	public enum Cleanup { Success, UnregisterFailure, DisposalFailure, SuspendedDisposalFailure, DisposalCancellation, DefaultTokenDisposalCancellation, AggregateDisposalFailure }

	public enum Outcome { Success, Fault, Cancellation, DefaultTokenCancellation, AggregateFault }

	private static IEnumerable<object[]> CompletionCases()
	{
		foreach (var external in new[] { false, true })
		{
			foreach (var outcome in new[] { Outcome.Success, Outcome.Fault, Outcome.Cancellation })
			{
				foreach (var cleanup in new[] { Cleanup.Success, Cleanup.UnregisterFailure, Cleanup.DisposalFailure, Cleanup.SuspendedDisposalFailure })
				{
					yield return [external, outcome, cleanup, false];
				}

				yield return [external, outcome, Cleanup.DisposalCancellation, false];
				yield return [external, outcome, Cleanup.DefaultTokenDisposalCancellation, false];
			}

			yield return [external, Outcome.DefaultTokenCancellation, Cleanup.Success, false];
			yield return [external, Outcome.DefaultTokenCancellation, Cleanup.SuspendedDisposalFailure, false];
			yield return [external, Outcome.DefaultTokenCancellation, Cleanup.DisposalCancellation, false];
			yield return [external, Outcome.AggregateFault, Cleanup.Success, false];
			yield return [external, Outcome.AggregateFault, Cleanup.DisposalFailure, false];
			yield return [external, Outcome.Fault, Cleanup.AggregateDisposalFailure, false];
			yield return [external, Outcome.Success, Cleanup.DisposalFailure, true];
		}
	}

	[TestMethod]
	[DynamicData(nameof(CompletionCases))]
	public async Task StartPublishesOperationAndCleanupOutcome(bool external,
															   Outcome outcome,
															   Cleanup cleanup,
															   bool immediateCompletion)
	{
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		using var cleanupCancellation = new CancellationTokenSource();
		cleanupCancellation.Cancel();
		var token = outcome == Outcome.DefaultTokenCancellation ? CancellationToken.None : cancellation.Token;
		var operation = new TaskCompletionSource<DataModelValue>(TaskCreationOptions.RunContinuationsAsynchronously);
		var disposalEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var releaseDisposal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		Exception operationFailure = outcome == Outcome.AggregateFault
			? new AggregateException(new InvalidOperationException("operation"))
			: new InvalidOperationException("operation");
		Exception? cleanupFailure = cleanup switch
									{
										Cleanup.Success                          => null,
										Cleanup.DisposalCancellation             => new OperationCanceledException(cleanupCancellation.Token),
										Cleanup.DefaultTokenDisposalCancellation => new OperationCanceledException(CancellationToken.None),
										Cleanup.AggregateDisposalFailure         => new AggregateException(new InvalidOperationException("cleanup")),
										_                                        => new ApplicationException("cleanup")
									};
		var stateController = new Mock<IStateMachineController>();
		stateController.Setup(static value => value.GetResult()).Returns(new ValueTask<DataModelValue>(operation.Task));
		var externalController = new Mock<IExternalServiceController>();
		externalController.Setup(static value => value.WaitForCompletion()).Returns(new ValueTask(operation.Task));
		var services = new ServiceCollection();
		services.AddConstant(stateController.Object);
		services.AddConstant(externalController.Object);
		var provider = services.BuildProvider();
		var scope = new Mock<IServiceScope>();
		scope.SetupGet(static value => value.ServiceProvider).Returns(provider);
		scope.Setup(static value => value.DisposeAsync()).Returns(DisposeScope);
		var scopeFactory = new Mock<IServiceScopeFactory>();
		scopeFactory.Setup(static value => value.CreateScope(It.IsAny<Action<IServiceCollection>>())).Returns(scope.Object);
		var capturedMonitor = new CapturingTaskMonitor();
		ITaskMonitor monitor = immediateCompletion ? new TaskMonitor.Services.TaskMonitor { Logger = null! } : capturedMonitor;
		var security = new SecurityContextFactory();
		var unregisterCalls = 0;
		IDisposable owner;
		Func<ValueTask<Task>> start;

		if (external)
		{
			var invokeId = InvokeId.FromString("completion");
			var collection = new Mock<IExternalServiceCollection>();
			collection.Setup(value => value.Unregister(invokeId)).Callback(Unregister);
			var manager = new ExternalServiceScopeManager
						  {
							  ServiceScopeFactory = scopeFactory.Object,
							  SecurityContextRegistrationFactory = security.GetRegistration,
							  ExternalServiceCollection = collection.Object,
							  TaskMonitor = monitor
						  };
			var invokeData = new InvokeData(invokeId, new FullUri("urn:service"), Source: null, RawContent: null, DataModelValue.Undefined, DataModelValue.Undefined);
			var serviceClass = new ExternalServiceClass(
				invokeData, Mock.Of<IEventDispatcher>(),
				Mock.Of<IStateMachineSessionId>(value => value.SessionId == SessionId.FromString("parent")),
				Mock.Of<IStateMachineLocation>(), Mock.Of<ICaseSensitivity>());
			owner = manager;
			start = async () => (await manager.Start(serviceClass, CancellationToken.None)).WaitForCompletion().AsTask();
		}
		else
		{
			var sessionId = SessionId.FromString("completion");
			var collection = new Mock<IStateMachineCollection>();
			collection.Setup(value => value.Unregister(sessionId)).Callback(Unregister);
			var manager = new StateMachineScopeManager
						  {
							  ServiceScopeFactory = scopeFactory.Object,
							  SecurityContextRegistrationFactory = security.GetRegistration,
							  StateMachineCollection = collection.Object,
							  TaskMonitor = monitor
						  };
			var stateMachine = new RuntimeStateMachine(Mock.Of<IStateMachine>()) { SessionId = sessionId };
			owner = manager;
			start = async () => (await manager.Start(stateMachine, SecurityContextType.NewTrustedStateMachine)).GetResult().AsTask();
		}

		using (owner)
		using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
		{
			try
			{
				if (immediateCompletion)
				{
					CompleteOperation();
				}

				var result = await start();

				if (!immediateCompletion)
				{
					Assert.IsFalse(result.IsCompleted);
					Assert.HasCount(expected: 1, capturedMonitor.Tasks);
					CompleteOperation();

					if (cleanup == Cleanup.SuspendedDisposalFailure)
					{
						await disposalEntered.Task.WaitAsync(timeout.Token);
						Assert.IsFalse(result.IsCompleted);
						Assert.IsFalse(capturedMonitor.Tasks.Single().IsCompleted);
						releaseDisposal.SetResult();
					}

					// The completion worker has an observer separate from the published result.
					await capturedMonitor.Tasks.Single().WaitAsync(timeout.Token);
				}

				Assert.IsTrue(result.IsCompleted, message: "Cleanup must not strand the published result.");
				var failure = await ObserveFailure(result);

				var operationCanceled = outcome is Outcome.Cancellation or Outcome.DefaultTokenCancellation;
				var cleanupCanceled = cleanupFailure is OperationCanceledException;

				if ((operationCanceled && (cleanupFailure is null || cleanupCanceled)) || (outcome == Outcome.Success && cleanupCanceled))
				{
					Assert.IsTrue(result.IsCanceled);
					Assert.IsNull(result.Exception);
					Assert.IsInstanceOfType<OperationCanceledException>(failure);
					var expectedToken = operationCanceled ? token : ((OperationCanceledException)cleanupFailure!).CancellationToken;
					Assert.AreEqual(expectedToken, ((OperationCanceledException)failure).CancellationToken);
				}
				else if (cleanupFailure is not null)
				{
					Assert.IsTrue(result.IsFaulted);

					if (outcome != Outcome.Success)
					{
						Assert.IsNotNull(result.Exception);
						var errors = result.Exception.InnerExceptions;
						Assert.HasCount(expected: 2, errors);
						Assert.AreSame(errors[0], failure, message: "Await must throw the first stored failure.");
						Assert.AreSame(cleanupFailure, errors[1]);

						if (outcome is Outcome.Cancellation or Outcome.DefaultTokenCancellation)
						{
							Assert.IsInstanceOfType<OperationCanceledException>(errors[0]);
							Assert.AreEqual(token, ((OperationCanceledException)errors[0]).CancellationToken);
						}
						else
						{
							Assert.AreSame(operationFailure, errors[0]);
						}
					}
					else
					{
						Assert.AreSame(cleanupFailure, failure);
						Assert.IsNotNull(result.Exception);
						Assert.HasCount(expected: 1, result.Exception.InnerExceptions);
						Assert.AreSame(cleanupFailure, result.Exception.InnerExceptions[0]);
					}
				}
				else if (outcome is Outcome.Fault or Outcome.AggregateFault)
				{
					Assert.IsTrue(result.IsFaulted);
					Assert.AreSame(operationFailure, failure);
					Assert.IsNotNull(result.Exception);
					Assert.HasCount(expected: 1, result.Exception.InnerExceptions);
					Assert.AreSame(operationFailure, result.Exception.InnerExceptions[0]);
				}
				else
				{
					Assert.IsNull(failure);
					Assert.AreEqual(TaskStatus.RanToCompletion, result.Status);

					if (!external)
					{
						Assert.AreEqual(expected: "result", (await (Task<DataModelValue>)result).AsString());
					}
				}

				Assert.AreEqual(expected: 1, unregisterCalls);
				scope.Verify(static value => value.DisposeAsync(), cleanup == Cleanup.UnregisterFailure ? Times.Never() : Times.Once());
			}
			finally
			{
				operation.TrySetResult(new DataModelValue("result"));
				releaseDisposal.TrySetResult();
			}
		}

		return;

		void Unregister()
		{
			unregisterCalls++;

			if (cleanup == Cleanup.UnregisterFailure)
			{
				throw cleanupFailure!;
			}
		}

		ValueTask DisposeScope()
		{
			if (cleanup == Cleanup.SuspendedDisposalFailure)
			{
				return FailAfterSuspension();
			}

			return cleanupFailure is null ? ValueTask.CompletedTask : ValueTask.FromException(cleanupFailure);
		}

		async ValueTask FailAfterSuspension()
		{
			disposalEntered.SetResult();
			await releaseDisposal.Task;

			throw cleanupFailure!;
		}

		void CompleteOperation()
		{
			switch (outcome)
			{
				case Outcome.Success: operation.SetResult(new DataModelValue("result")); break;
				case Outcome.Cancellation:
				case Outcome.DefaultTokenCancellation: Assert.IsTrue(operation.TrySetCanceled(token)); break;
				default: operation.SetException(operationFailure); break;
			}
		}
	}

	[TestMethod]
	public async Task ExecuteCancelsWhenCleanupReportsCancellation()
	{
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		var cleanupFailure = new OperationCanceledException(cancellation.Token);
		var controller = new Mock<IStateMachineController>();
		controller.Setup(static value => value.GetResult()).Returns(new ValueTask<DataModelValue>(new DataModelValue("result")));
		var collection = new Mock<IStateMachineCollection>();
		collection.Setup(static value => value.Unregister(It.IsAny<SessionId>())).Throws(cleanupFailure);
		var services = new ServiceCollection();
		services.AddConstant(controller.Object);
		var provider = services.BuildProvider();
		var security = new SecurityContextFactory();
		using var manager = new StateMachineScopeManager
							{
								ServiceScopeFactory = await provider.GetRequiredService<IServiceScopeFactory>(),
								SecurityContextRegistrationFactory = security.GetRegistration,
								StateMachineCollection = collection.Object,
								TaskMonitor = new TaskMonitor.Services.TaskMonitor { Logger = null! }
							};
		var stateMachine = new RuntimeStateMachine(Mock.Of<IStateMachine>()) { SessionId = SessionId.FromString("execute-cleanup") };
		var result = manager.Execute(stateMachine, SecurityContextType.NewTrustedStateMachine).AsTask();
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		var failure = await ObserveFailure(result.WaitAsync(timeout.Token));

		Assert.IsTrue(result.IsCanceled);
		Assert.IsNull(result.Exception);
		Assert.IsInstanceOfType<OperationCanceledException>(failure);
		Assert.AreEqual(cancellation.Token, ((OperationCanceledException)failure).CancellationToken);
	}

	private static async Task<Exception?> ObserveFailure(Task task)
	{
		try
		{
			await task;

			return null;
		}
		catch (Exception exception)
		{
			return exception;
		}
	}

	private sealed class CapturingTaskMonitor : ITaskMonitor
	{
		public List<Task> Tasks { get; } = [];

	#region Interface ITaskMonitor

		public Task WaitAsync(Task task, CancellationToken token) => task.WaitAsync(token);

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task.WaitAsync(token);

		public ValueTask WaitAsync(ValueTask task, CancellationToken token) => new(task.AsTask().WaitAsync(token));

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> task, CancellationToken token) => new(task.AsTask().WaitAsync(token));

		public void Forget(Task task) => Tasks.Add(task);

		public void Forget(ValueTask task) => Tasks.Add(task.AsTask());

		public void Forget<TResult>(ValueTask<TResult> task) => Tasks.Add(task.AsTask());

	#endregion
	}
}
