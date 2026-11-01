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


using System.Reflection;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.Interpreter.Internal;
using Xtate.Interpreter.Model;
using Xtate.Interpreter.Services;
using Xtate.IoC.Tools;
using Xtate.Logging;
using Xtate.Persistence;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Core.Test.UnitTests.Persistence;

[TestClass]
public class PersistingInterpreterReplayCoverageTest
{
	[TestMethod]
	public async Task StartingWithPendingSuspensionClosesTheExternalReaderBeforeReplay()
	{
		using var storage = new InMemoryStorage(false);
		var bucket = new Bucket(storage);
		bucket.Nested(0).Nested(0).Add(Bucket.RootKey, 1);
		bucket.Nested(0).Nested(0).Add(0, 4); // Interpret already completed in the saved execution.
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(bucket);
		context.SetupGet(value => value.InternalQueue).Returns(new EntityQueue<IIncomingEvent>());
		context.SetupGet(value => value.DoneData).Returns(new DataModelValue("saved result"));
		var reader = new Mock<IEventReader>();
		var interpreter = Create(context.Object, suspend: Mock.Of<ISuspendEventDispatcher>(value => value.IsSuspendRequested), reader: reader.Object);
		Assert.AreEqual("saved result", (await interpreter.Run()).AsString());
		reader.Verify(value => value.Complete(), Times.Once);
	}
	[TestMethod]
	[DataRow("missing-frame")]
	[DataRow("missing-key")]
	[DataRow("wrong-key")]
	[DataRow("missing-state")]
	[DataRow("completed-state")]
	public async Task ChangedExecutionRecordIsRejectedWhenAnActionCompletes(string corruption)
	{
		using var storage = new InMemoryStorage(false);
		var bucket = new Bucket(storage);
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(bucket);
		var action = new Mock<IExecEvaluator>();
		action.Setup(value => value.Execute()).Callback(() =>
		{
			var record = bucket.Nested(0).Nested(0);
			switch (corruption)
			{
				case "missing-frame": bucket.Nested(1).Remove(Bucket.RootKey); break;
				case "missing-key": record.Remove(0); break;
				case "wrong-key": record.Add(0, 99); break;
				case "missing-state": record.Remove(Bucket.RootKey); break;
				case "completed-state": record.Add(Bucket.RootKey, 1); break;
			}
		}).Returns(default(ValueTask));
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await Invoke(Create(context.Object), "RunExecutableEntity", ImmutableArray.Create(action.Object)));
		action.Verify(value => value.Execute(), Times.Once);
	}

	[TestMethod]
	public async Task MainLoopRejectsAnIterationIncorrectlyMarkedAsCompleted()
	{
		using var storage = new InMemoryStorage(false);
		var bucket = new Bucket(storage);
		var iteration = bucket.Nested(1).Nested(0);
		iteration.Add(Bucket.RootKey, 1);
		iteration.Add(0, 7); // MainEventLoopIteration is an iteration marker, never a completed operation.
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(bucket);
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await Invoke(Create(context.Object), "MainEventLoop"));
	}

	[TestMethod]
	public async Task InternalEventSelectionRestoresItsResultWithoutDequeuingTheNextEvent()
	{
		using var storage = new InMemoryStorage(false);
		var queue = new EntityQueue<IIncomingEvent>();
		queue.Enqueue(new IncomingEvent { Name = (EventName)"first", Type = EventType.Internal });
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(new Bucket(storage));
		context.SetupGet(value => value.InternalQueue).Returns(queue);
		context.SetupGet(value => value.DataModel).Returns(new DataModelList());
		context.SetupGet(value => value.Configuration).Returns(new OrderedSet<StateEntityNode>());
		var model = Mock.Of<IInterpreterModel>(value => value.EntityMap == Mock.Of<IEntityMap>());
		var method = typeof(StateMachinePersistingInterpreter).GetMethod("SelectInternalEventTransitions", BindingFlags.Instance | BindingFlags.NonPublic)!;
		Assert.IsEmpty(await (ValueTask<List<TransitionNode>>)method.Invoke(Create(context.Object, model), null)!);
		queue.Enqueue(new IncomingEvent { Name = (EventName)"second", Type = EventType.Internal });
		Assert.IsEmpty(await (ValueTask<List<TransitionNode>>)method.Invoke(Create(context.Object, model), null)!);
		Assert.AreEqual("second", queue.Dequeue().Name.ToString());
	}

	[TestMethod]
	public async Task NormalQueueCompletionDoesNotRequestSuspension()
	{
		using var storage = new InMemoryStorage(false);
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(new Bucket(storage));
		await Invoke(Create(context.Object), "ExternalQueueCompleted");
	}
	[TestMethod]
	public async Task CompletedActionIsNotExecutedAgainAfterInterpreterRecreation()
	{
		using var storage = new InMemoryStorage(false);
		var bucket = new Bucket(storage);
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(bucket);
		var action = new Mock<IExecEvaluator>();
		action.Setup(value => value.Execute()).Returns(default(ValueTask));
		var first = Create(context.Object);
		await Invoke(first, "RunExecutableEntity", ImmutableArray.Create(action.Object));
		action.Verify(value => value.Execute(), Times.Once);
		var restored = Create(context.Object);
		await Invoke(restored, "RunExecutableEntity", ImmutableArray.Create(action.Object));
		action.Verify(value => value.Execute(), Times.Once);
	}

	[TestMethod]
	[DataRow("ExecuteGlobalScript", 0)]
	[DataRow("InitialEnterStates", 1)]
	[DataRow("ExitStates", 2)]
	[DataRow("EnterStates", 3)]
	[DataRow("Interpret", 4)]
	[DataRow("ExitSteps", 5)]
	[DataRow("InitializeDataModels", 6)]
	[DataRow("MainEventLoop", 16)]
	[DataRow("ExitInterpreter", 17)]
	[DataRow("ExecuteTransitionContent", 18)]
	[DataRow("RunExecutableEntity", 19)]
	[DataRow("StartInvoke", 20)]
	[DataRow("CancelInvoke", 21)]
	[DataRow("InitializeDataModel", 22)]
	[DataRow("InitializeData", 23)]
	public async Task CompletedPersistedOperationDoesNotRepeatSideEffects(string method, int key)
	{
		using var storage = new InMemoryStorage(false);
		var bucket = new Bucket(storage);
		var record = bucket.Nested(0).Nested(0);
		record.Add(Bucket.RootKey, 1); // MethodState.Completed in the persisted format.
		record.Add(0, key);
		var context = new Mock<IStateMachinePersistenceContext>(MockBehavior.Strict);
		context.Setup(value => value.GetStateBucket()).Returns(bucket);
		var interpreter = Create(context.Object);
		var info = typeof(StateMachinePersistingInterpreter).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!;
		var arguments = info.GetParameters().Select(parameter => parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null).ToArray();
		await (ValueTask)info.Invoke(interpreter, arguments)!;
		context.Verify(value => value.GetStateBucket(), Times.Once);
		context.VerifyNoOtherCalls();
	}

	[TestMethod]
	[DataRow("StartInvokeLoop", 8, true)]
	[DataRow("Microstep", 9, false)]
	[DataRow("InternalQueueProcess", 10, true)]
	[DataRow("IsInternalQueueEmpty", 11, false)]
	[DataRow("Macrostep", 13, true)]
	[DataRow("ExternalQueueProcess", 14, false)]
	public async Task CompletedBooleanOperationReturnsItsPersistedResult(string method, int key, bool expected)
	{
		using var storage = new InMemoryStorage(false);
		var bucket = new Bucket(storage);
		var record = bucket.Nested(0).Nested(0);
		record.Add(Bucket.RootKey, 1);
		record.Add(0, key);
		if (expected) record.Add(1, true);
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(bucket);
		var interpreter = Create(context.Object);
		var info = typeof(StateMachinePersistingInterpreter).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!;
		var arguments = info.GetParameters().Select(_ => (object?)null).ToArray();
		Assert.AreEqual(expected, await (ValueTask<bool>)info.Invoke(interpreter, arguments)!);
	}

	[TestMethod]
	[DataRow("SelectTransitions", 15)]
	[DataRow("SelectInternalEventTransitions", 10)]
	public async Task CompletedTransitionSelectionRestoresNodesFromEntityMap(string method, int key)
	{
		using var storage = new InMemoryStorage(false);
		var bucket = new Bucket(storage);
		var record = bucket.Nested(0).Nested(0);
		record.Add(Bucket.RootKey, 1);
		record.Add(0, key);
		record.Nested(1).Add(Bucket.RootKey, 1);
		record.Nested(1).Add(0, 42);
		var transition = new TransitionNode(new DocumentIdNode(null), new TransitionEntity());
		IEntity? entity = transition;
		var map = new Mock<IEntityMap>();
		map.Setup(value => value.TryGetEntityByDocumentId(42, out entity)).Returns(true);
		var model = Mock.Of<IInterpreterModel>(value => value.EntityMap == map.Object);
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(bucket);
		var interpreter = Create(context.Object, model);
		var info = typeof(StateMachinePersistingInterpreter).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!;
		var args = info.GetParameters().Select(_ => (object?)null).ToArray();
		var result = await (ValueTask<List<TransitionNode>>)info.Invoke(interpreter, args)!;
		CollectionAssert.AreEqual(new[] { transition }, result);
	}

	[TestMethod]
	public async Task MissingPersistedTransitionIsRejectedInsteadOfSilentlySkipped()
	{
		using var storage = new InMemoryStorage(false);
		var bucket = new Bucket(storage);
		var record = bucket.Nested(0).Nested(0);
		record.Add(Bucket.RootKey, 1);
		record.Add(0, 15);
		record.Nested(1).Add(Bucket.RootKey, 1);
		record.Nested(1).Add(0, 42);
		var model = Mock.Of<IInterpreterModel>(value => value.EntityMap == Mock.Of<IEntityMap>());
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(bucket);
		var interpreter = Create(context.Object, model);
		var info = typeof(StateMachinePersistingInterpreter).GetMethod("SelectTransitions", BindingFlags.Instance | BindingFlags.NonPublic)!;
		await Assert.ThrowsExactlyAsync<KeyNotFoundException>(async () => await (ValueTask<List<TransitionNode>>)info.Invoke(interpreter, [null])!);
	}
	[TestMethod]
	[DataRow(PersistenceLevel.StableState, 1, 1)]
	[DataRow(PersistenceLevel.Event, 0, 0)]
	public async Task CheckpointRespectsConfiguredLevelAndShrinksOnlyAtStableState(PersistenceLevel level, int checkpointCount, int shrinkCount)
	{
		using var storage = new InMemoryStorage(false);
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(new Bucket(storage));
		await Invoke(Create(context.Object), "CheckPoint", level);
		context.Verify(value => value.CheckPoint((int)level), Times.Exactly(checkpointCount));
		context.Verify(value => value.Shrink(), Times.Exactly(shrinkCount));
	}

	[TestMethod]
	public async Task CompletedEmptyTransitionSelectionCanBeReplayed()
	{
		using var storage = new InMemoryStorage(false);
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(new Bucket(storage));
		context.SetupGet(value => value.Configuration).Returns(new OrderedSet<StateEntityNode>());
		var model = Mock.Of<IInterpreterModel>(value => value.EntityMap == Mock.Of<IEntityMap>());
		var method = typeof(StateMachinePersistingInterpreter).GetMethod("SelectTransitions", BindingFlags.Instance | BindingFlags.NonPublic)!;
		var initial = await (ValueTask<List<TransitionNode>>)method.Invoke(Create(context.Object, model), [null])!;
		var replay = await (ValueTask<List<TransitionNode>>)method.Invoke(Create(context.Object, model), [null])!;
		Assert.IsEmpty(initial);
		Assert.IsEmpty(replay);
		context.VerifyGet(value => value.Configuration, Times.Once);
	}

	[TestMethod]
	[DataRow(99, 4)]
	[DataRow(1, 99)]
	public async Task CorruptMethodStateOrMismatchedOperationIsRejected(int state, int key)
	{
		using var storage = new InMemoryStorage(false);
		var bucket = new Bucket(storage);
		bucket.Nested(0).Nested(0).Add(Bucket.RootKey, state);
		bucket.Nested(0).Nested(0).Add(0, key);
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(bucket);
		await Assert.ThrowsAsync<Exception>(async () => await Invoke(Create(context.Object), "Interpret"));
	}

	private static async Task Invoke(StateMachinePersistingInterpreter interpreter, string method, params object[] arguments) =>
		await (ValueTask)typeof(StateMachinePersistingInterpreter).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(interpreter, arguments)!;

	[TestMethod]
	[DataRow(PersistenceLevel.None)]
	[DataRow((PersistenceLevel)999)]
	public void DisabledOrInvalidPersistenceLevelIsRejected(PersistenceLevel level)
	{
		using var storage = new InMemoryStorage(false);
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(new Bucket(storage));
		Assert.Throws<ArgumentException>(() => Create(context.Object, level: level));
	}

	[TestMethod]
	public async Task AlreadyRequestedSuspensionTurnsQueueCompletionIntoSuspension()
	{
		using var storage = new InMemoryStorage(false);
		var context = new Mock<IStateMachinePersistenceContext>();
		context.Setup(value => value.GetStateBucket()).Returns(new Bucket(storage));
		var suspend = Mock.Of<ISuspendEventDispatcher>(value => value.IsSuspendRequested);
		var interpreter = Create(context.Object, suspend: suspend);
		var error = await Assert.ThrowsExactlyAsync<StateMachineSuspendedException>(async () => await Invoke(interpreter, "ExternalQueueCompleted"));
		await Invoke(interpreter, "HandleMainLoopException", error);
	}

	private static StateMachinePersistingInterpreter Create(IStateMachinePersistenceContext context, IInterpreterModel? model = null, PersistenceLevel level = PersistenceLevel.StableState, ISuspendEventDispatcher? suspend = null, IEventReader? reader = null) => new()
	{
		StateMachinePersistenceContext = context,
		PersistenceOptions = Mock.Of<IPersistenceOptions>(value => value.PersistenceLevel == level),
		SuspendEventDispatcher = suspend ?? Mock.Of<ISuspendEventDispatcher>(),
		InterpreterModel = model!, StateMachineRuntimeError = new StateMachineRuntimeError(new ScopeObject()),
		StateMachineArguments = null!, DataConverter = new Xtate.DataModel.Services.DataConverter(Mock.Of<ICaseSensitivity>()), CaseSensitivity = Mock.Of<ICaseSensitivity>(), EventReader = reader!, Logger = Mock.Of<ILogger<StateMachineInterpreter>>(),
		Model = null!, NotifyStateChanged = [], UnhandledErrorBehaviour = null, StateMachineContext = context,
		InvokeController = null!, DisposeToken = new DisposeToken()
	};
}
