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

using System.Reflection;
using System.Threading;
using Xtate.DataModel;
using Xtate.DataModel.Services;
using Xtate.DataTypes;
using Xtate.Logging;
using Xtate.Logging.Internal;
using Xtate.Interpreter;
using Xtate.Interpreter.Internal;
using Xtate.Interpreter.Model;
using Xtate.Interpreter.Services;
using Xtate.IoC.Tools;
using Xtate.StateMachine;

namespace Xtate.Core.Test.UnitTests.Interpreter;

[TestClass]
public class StateMachineInterpreterCoverageTest
{
	[TestMethod]
	public async Task InterpreterExitRunsOnExitBeforeCancellingInvocationsAndClearsConfiguration()
	{
		var order = new List<string>();
		var action = new Mock<IExecutableEntity>();
		action.As<IExecEvaluator>().Setup(value => value.Execute()).Callback(() => order.Add("exit")).Returns(default(ValueTask));
		var onExit = new OnExitNode(new DocumentIdNode(null), new OnExitEntity { Action = [action.Object] });
		var invokeId = InvokeId.FromString("active");
		var invoke = new InvokeNode(new DocumentIdNode(null), new InvokeSource()) { DataConverter = null!, CurrentInvokeId = invokeId };
		var state = new StateNode(new DocumentIdNode(null), new StateEntity { Id = Identifier.FromString("state"), OnExit = [onExit], Invoke = [invoke] });
		var configuration = new OrderedSet<StateEntityNode> { state };
		var active = new OrderedSet<InvokeNode> { invoke };
		var context = Mock.Of<IStateMachineContext>(value => value.Configuration == configuration && value.ActiveInvokes == active);
		var controller = new Mock<IInvokeController>();
		controller.Setup(value => value.Cancel(invokeId)).Callback(() => order.Add("cancel")).Returns(default(ValueTask));
		var interpreter = CreateInterpreter(Mock.Of<IEventReader>(), CancellationToken.None, context, invokeController: controller.Object);
		var method = typeof(StateMachineInterpreter).GetMethod("ExitInterpreter", BindingFlags.Instance | BindingFlags.NonPublic)!;
		await (ValueTask)method.Invoke(interpreter, null)!;
		CollectionAssert.AreEqual(new[] { "exit", "cancel" }, order);
		Assert.HasCount(0, configuration);
		Assert.HasCount(0, active);
		Assert.IsNull(invoke.CurrentInvokeId);
	}
	[TestMethod]
	public async Task DestroyTerminationCancellationAndEmptyFinalizePathsAreCovered()
	{
		var eventReader = new Mock<IEventReader>();
		var interpreter = CreateInterpreter(eventReader.Object, CancellationToken.None);

		interpreter.TriggerDestroySignal();
		eventReader.Verify(static reader => reader.Complete(), Times.Once);
		var failedDestroyReader = new Mock<IEventReader>();
		CreateInterpreter(failedDestroyReader.Object, CancellationToken.None).TriggerDestroySignal(new InvalidOperationException("destroy"));
		failedDestroyReader.Verify(static reader => reader.Complete(), Times.Once);

		await interpreter.HandleMainLoopFailure(new InvalidOperationException("failure"));
		Assert.AreSame(StateMachineInterpreterState.Terminated, interpreter.NotifiedState);
		Assert.IsTrue(interpreter.IsInterpreterError(new InvalidOperationException("failure")));
		Assert.IsTrue(interpreter.IsInterpreterError(new OperationCanceledException()));

		using var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Cancel();
		var cancelledInterpreter = CreateInterpreter(Mock.Of<IEventReader>(), cancellationTokenSource.Token);
		Assert.IsFalse(cancelledInterpreter.IsInterpreterError(new OperationCanceledException()));

		var invoke = new InvokeNode(new DocumentIdNode(list: null), new InvokeSource()) { DataConverter = null! };
		var applyFinalize = typeof(StateMachineInterpreter).GetMethod(name: "ApplyFinalize", BindingFlags.Instance | BindingFlags.NonPublic)!;
		var result = (ValueTask)applyFinalize.Invoke(interpreter, [invoke])!;
		await result;
	}

	[TestMethod]
	[DataRow("execution", false)]
	[DataRow("execution", true)]
	[DataRow("platform", true)]
	[DataRow("communication", true)]
	public async Task RuntimeErrorsBecomePlatformEventsAndAreLogged(string kind, bool logging)
	{
		var queue = new EntityQueue<IIncomingEvent>();
		var context = Mock.Of<IStateMachineContext>(value => value.InternalQueue == queue);
		var logger = new Mock<ILogger<StateMachineInterpreter>>();
		logger.Setup(value => value.IsEnabled(Level.Error)).Returns(logging);
		var runtime = new StateMachineRuntimeError(new ScopeObject());
		var failure = new InvalidOperationException("failure");
		Exception exception = kind switch
		{
			"platform" => runtime.PlatformError(failure),
			"communication" => runtime.CommunicationError(failure, SendId.FromString("send")),
			_ => failure
		};
		var interpreter = CreateInterpreter(Mock.Of<IEventReader>(), CancellationToken.None, context, logger.Object, runtime);
		await ReportError(interpreter, exception);
		Assert.HasCount(1, queue);
		var message = queue.Dequeue();
		Assert.AreEqual(EventType.Platform, message.Type);
		Assert.AreEqual("error." + kind, message.Name.ToString());
		Assert.IsFalse(message.Data.IsUndefined());
		Assert.AreEqual(kind == "communication" ? "send" : null, message.SendId?.ToString());
		Assert.AreEqual(logging ? 1 : 0, logger.Invocations.Count(invocation => invocation.Method.Name == "Write"));
	}

	[TestMethod]
	public async Task LoggerFailureQueuesOneAdditionalPlatformErrorWithoutRecursingForever()
	{
		var queue = new EntityQueue<IIncomingEvent>();
		var context = Mock.Of<IStateMachineContext>(value => value.InternalQueue == queue);
		var logger = new Mock<ILogger<StateMachineInterpreter>>();
		logger.Setup(value => value.IsEnabled(Level.Error)).Returns(true);
		logger.Setup(value => value.Write(Level.Error, It.IsAny<int>(), It.IsAny<LoggingInterpolatedStringHandler>(), It.IsAny<Exception>()))
			.Throws(new InvalidOperationException("logger failed"));
		var interpreter = CreateInterpreter(Mock.Of<IEventReader>(), CancellationToken.None, context, logger.Object);
		await ReportError(interpreter, new InvalidOperationException("execution failed"));
		Assert.HasCount(2, queue);
		Assert.AreEqual(EventName.ErrorExecution, queue.Dequeue().Name);
		Assert.AreEqual(EventName.ErrorPlatform, queue.Dequeue().Name);
		Assert.AreEqual(2, logger.Invocations.Count(invocation => invocation.Method.Name == "Write"));
	}

	private static async Task ReportError(StateMachineInterpreter interpreter, Exception exception)
	{
		var method = typeof(StateMachineInterpreter).GetMethod("Error", BindingFlags.Instance | BindingFlags.NonPublic)!;
		await (ValueTask)method.Invoke(interpreter, [new object(), exception, true])!;
	}
	[TestMethod]
	public async Task EventsFromInactiveInvocationsAreSkippedWhileActiveInvocationEventsAreReturned()
	{
		var invoke = new InvokeNode(new DocumentIdNode(null), new InvokeSource())
		{
			DataConverter = null!, CurrentInvokeId = InvokeId.FromString("active")
		};
		var active = new OrderedSet<InvokeNode> { invoke };
		var context = Mock.Of<IStateMachineContext>(value => value.ActiveInvokes == active);
		var expected = new IncomingEvent { Name = EventName.FromString("accepted"), InvokeId = invoke.CurrentInvokeId };
		var reader = new QueuedEventReader(new IncomingEvent { InvokeId = InvokeId.FromString("inactive") }, expected);
		var interpreter = CreateInterpreter(reader, CancellationToken.None, context);
		var method = typeof(StateMachineInterpreter).GetMethod("ReadExternalEventFiltered", BindingFlags.Instance | BindingFlags.NonPublic)!;
		Assert.AreSame(expected, await (ValueTask<IIncomingEvent>)method.Invoke(interpreter, null)!);
		Assert.AreEqual(2, reader.ReadCount);
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public async Task ForwardingDispatchesToInvokeOrQueuesExecutionErrorWhenDispatcherFails(bool fail)
	{
		var invoke = new InvokeNode(new DocumentIdNode(null), new InvokeSource())
		{
			DataConverter = null!, CurrentInvokeId = InvokeId.FromString("active")
		};
		var queue = new EntityQueue<IIncomingEvent>();
		var context = Mock.Of<IStateMachineContext>(value => value.InternalQueue == queue);
		var dispatcher = new Mock<IInvokeController>();
		var message = new IncomingEvent { Name = EventName.FromString("forward") };
		if (fail)
		{
			dispatcher.Setup(value => value.Forward(invoke.CurrentInvokeId, message)).Throws(new InvalidOperationException("forward failed"));
		}
		var interpreter = CreateInterpreter(Mock.Of<IEventReader>(), CancellationToken.None, context, invokeController: dispatcher.Object);
		var method = typeof(StateMachineInterpreter).GetMethod("ForwardEvent", BindingFlags.Instance | BindingFlags.NonPublic)!;
		await (ValueTask)method.Invoke(interpreter, [invoke, message])!;
		dispatcher.Verify(value => value.Forward(invoke.CurrentInvokeId, message), Times.Once);
		Assert.AreEqual(fail ? 1 : 0, queue.Count);
		if (fail) Assert.AreEqual(EventName.ErrorExecution, queue.Dequeue().Name);
	}

	[TestMethod]
	public async Task DataEvaluationFailureQueuesErrorAndLeavesExistingVariableUnchanged()
	{
		var queue = new EntityQueue<IIncomingEvent>();
		var dataModel = new DataModelList { ["value"] = "original" };
		var context = Mock.Of<IStateMachineContext>(value => value.InternalQueue == queue && value.DataModel == dataModel);
		var expression = new Mock<IValueExpression>();
		expression.As<IObjectEvaluator>().Setup(value => value.EvaluateObject()).Throws(new InvalidOperationException("evaluation failed"));
		var data = new DataNode(new DocumentIdNode(null), new DataEntity { Id = "value", Expression = expression.Object });
		var interpreter = CreateInterpreter(Mock.Of<IEventReader>(), CancellationToken.None, context);
		var method = typeof(StateMachineInterpreter).GetMethod("InitializeData", BindingFlags.Instance | BindingFlags.NonPublic)!;
		await (ValueTask)method.Invoke(interpreter, [data, null])!;
		Assert.AreEqual("original", dataModel["value"].AsString());
		Assert.AreEqual(EventName.ErrorExecution, queue.Dequeue().Name);
	}

	[TestMethod]
	public async Task DoneDataFailureQueuesErrorAndReturnsUndefined()
	{
		var queue = new EntityQueue<IIncomingEvent>();
		var context = Mock.Of<IStateMachineContext>(value => value.InternalQueue == queue);
		var doneData = new DoneDataNode(new DocumentIdNode(null), new DoneDataEntity())
		{
			DataConverterFactory = () => throw new InvalidOperationException("done data failed")
		};
		var interpreter = CreateInterpreter(Mock.Of<IEventReader>(), CancellationToken.None, context);
		var method = typeof(StateMachineInterpreter).GetMethod("EvaluateDoneData", BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(DoneDataNode)], null)!;
		var result = await (ValueTask<DataModelValue>)method.Invoke(interpreter, [doneData])!;
		Assert.IsTrue(result.IsUndefined());
		Assert.AreEqual(EventName.ErrorExecution, queue.Dequeue().Name);
	}

	[TestMethod]
	public async Task GlobalScriptFailureQueuesErrorInsteadOfEscaping()
	{
		var queue = new EntityQueue<IIncomingEvent>();
		var context = Mock.Of<IStateMachineContext>(value => value.InternalQueue == queue);
		var script = new Mock<IScript>();
		script.As<IExecEvaluator>().Setup(value => value.Execute()).Throws(new InvalidOperationException("script failed"));
		var scriptNode = new ScriptNode(new DocumentIdNode(null), script.Object);
		var initial = new InitialNode(new DocumentIdNode(null), new InitialEntity { Transition = new TransitionNode(new DocumentIdNode(null), new TransitionEntity()) });
		var root = new StateMachineNode(new DocumentIdNode(null), new StateMachineEntity { Initial = initial, Script = scriptNode, States = [] });
		var model = Mock.Of<IInterpreterModel>(value => value.Root == root);
		var interpreter = CreateInterpreter(Mock.Of<IEventReader>(), CancellationToken.None, context, model: model);
		var method = typeof(StateMachineInterpreter).GetMethod("ExecuteGlobalScript", BindingFlags.Instance | BindingFlags.NonPublic)!;
		await (ValueTask)method.Invoke(interpreter, null)!;
		Assert.AreEqual(EventName.ErrorExecution, queue.Dequeue().Name);
		script.As<IExecEvaluator>().Verify(value => value.Execute(), Times.Once);
	}

	[TestMethod]
	public async Task MatchingInvokeEventRunsFinalizeAndAutoForwardsBeforeTransitionSelection()
	{
		var action = new Mock<IExecutableEntity>();
		action.As<IExecEvaluator>().Setup(value => value.Execute()).Returns(default(ValueTask));
		var finalize = new FinalizeNode(new FinalizeEntity { Action = [action.Object] });
		var invoke = new InvokeNode(new DocumentIdNode(null), new InvokeSource(finalize: finalize, autoForward: true))
		{
			DataConverter = null!, CurrentInvokeId = InvokeId.FromString("active")
		};
		var state = new InvokingState(invoke);
		var initial = new InitialNode(new DocumentIdNode(null), new InitialEntity { Transition = new TransitionNode(new DocumentIdNode(null), new TransitionEntity()) });
		var root = new StateMachineNode(new DocumentIdNode(null), new StateMachineEntity { Initial = initial, States = [state] });
		var configuration = new OrderedSet<StateEntityNode> { state };
		var active = new OrderedSet<InvokeNode> { invoke };
		var data = new DataModelList();
		var context = Mock.Of<IStateMachineContext>(value => value.Configuration == configuration && value.ActiveInvokes == active && value.DataModel == data);
		var message = new IncomingEvent { Name = EventName.FromString("received"), Type = EventType.External, InvokeId = invoke.CurrentInvokeId };
		var dispatcher = new Mock<IInvokeController>();
		var interpreter = CreateInterpreter(new QueuedEventReader(message), CancellationToken.None, context, invokeController: dispatcher.Object, model: Mock.Of<IInterpreterModel>(value => value.Root == root));
		var method = typeof(StateMachineInterpreter).GetMethod("ExternalEventTransitions", BindingFlags.Instance | BindingFlags.NonPublic)!;
		var transitions = await (ValueTask<List<TransitionNode>>)method.Invoke(interpreter, null)!;
		Assert.IsEmpty(transitions);
		action.As<IExecEvaluator>().Verify(value => value.Execute(), Times.Once);
		dispatcher.Verify(value => value.Forward(invoke.CurrentInvokeId, message), Times.Once);
		Assert.IsFalse(data["_event"].IsUndefined());
	}

	[TestMethod]
	public void TerminateOnUnhandledErrorPreservesTheOriginalException()
	{
		var interpreter = CreateInterpreter(Mock.Of<IEventReader>(), CancellationToken.None, unhandled: UnhandledErrorBehaviour.TerminateStateMachine);
		var original = new InvalidOperationException("unhandled failure");
		var message = new IncomingEvent { Name = EventName.ErrorExecution, Ancestor = original };
		var method = typeof(StateMachineInterpreter).GetMethod("ProcessUnhandledError", BindingFlags.Instance | BindingFlags.NonPublic)!;
		var wrapped = Assert.ThrowsExactly<TargetInvocationException>(() => method.Invoke(interpreter, [message]));
		var error = Assert.IsInstanceOfType<StateMachineUnhandledErrorException>(wrapped.InnerException);
		Assert.AreSame(original, error.InnerException);
	}

	[TestMethod]
	public async Task InvokeStartFailureIsConvertedToAnExecutionError()
	{
		var queue = new EntityQueue<IIncomingEvent>();
		var active = new OrderedSet<InvokeNode>();
		var context = Mock.Of<IStateMachineContext>(value => value.InternalQueue == queue && value.ActiveInvokes == active);
		var invoke = new InvokeNode(new DocumentIdNode(null), new InvokeSource(type: new FullUri("urn:service")))
		{
			DataConverter = () => new ValueTask<DataConverter>(new DataConverter(null))
		};
		var dispatcher = new Mock<IInvokeController>();
		dispatcher.Setup(value => value.Start(It.IsAny<InvokeData>())).Throws(new InvalidOperationException("start failed"));
		var interpreter = CreateInterpreter(Mock.Of<IEventReader>(), CancellationToken.None, context, invokeController: dispatcher.Object);
		var method = typeof(StateMachineInterpreter).GetMethod("StartInvoke", BindingFlags.Instance | BindingFlags.NonPublic)!;
		await (ValueTask)method.Invoke(interpreter, [InvokeId.FromString("started"), invoke])!;
		dispatcher.Verify(value => value.Start(It.IsAny<InvokeData>()), Times.Once);
		Assert.AreEqual(EventName.ErrorExecution, queue.Dequeue().Name);
	}

	[TestMethod]
	public async Task LateBoundStateDataIsAvailableToItsOnEntryActions()
	{
		var data = new DataModelList();
		var expression = new Mock<IValueExpression>();
		expression.As<IObjectEvaluator>().Setup(value => value.EvaluateObject()).Returns(new ValueTask<IObject>(new DataModelValue("initialized")));
		var dataNode = new DataNode(new DocumentIdNode(null), new DataEntity { Id = "local", Expression = expression.Object });
		var dataModel = new DataModelNode(new DocumentIdNode(null), new DataModelEntity { Data = [dataNode] });
		var action = new Mock<IExecutableEntity>();
		action.As<IExecEvaluator>().Setup(value => value.Execute()).Callback(() => Assert.AreEqual("initialized", data["local"].AsString())).Returns(default(ValueTask));
		var onEntry = new OnEntryNode(new DocumentIdNode(null), new OnEntryEntity { Action = [action.Object] });
		var state = new StateNode(new DocumentIdNode(null), new StateEntity { Id = Identifier.FromString("state"), DataModel = dataModel, OnEntry = [onEntry] });
		var transition = new TransitionNode(new DocumentIdNode(null), new TransitionEntity { Target = [state.Id] });
		var initial = new InitialNode(new DocumentIdNode(null), new InitialEntity { Transition = transition });
		var root = new StateMachineNode(new DocumentIdNode(null), new StateMachineEntity { Initial = initial, Binding = BindingType.Late, States = [state] });
		Assert.IsTrue(transition.TryMapTarget(new Dictionary<IIdentifier, StateEntityNode> { [state.Id] = state }));
		var configuration = new OrderedSet<StateEntityNode>();
		var pending = new OrderedSet<StateEntityNode>();
		var context = Mock.Of<IStateMachineContext>(value => value.DataModel == data && value.Configuration == configuration && value.StatesToInvoke == pending);
		var interpreter = CreateInterpreter(Mock.Of<IEventReader>(), CancellationToken.None, context, model: Mock.Of<IInterpreterModel>(value => value.Root == root));
		var method = typeof(StateMachineInterpreter).GetMethod("EnterStates", BindingFlags.Instance | BindingFlags.NonPublic)!;
		await (ValueTask)method.Invoke(interpreter, [new List<TransitionNode> { transition }])!;
		action.As<IExecEvaluator>().Verify(value => value.Execute(), Times.Once);
		Assert.IsTrue(configuration.IsMember(state));
	}

	private sealed class InvokingState(InvokeNode invoke) : StateEntityNode(new DocumentIdNode(null))
	{
		public override bool IsAtomicState => true;
		public override ImmutableArray<TransitionNode> Transitions => [];
		public override ImmutableArray<InvokeNode> Invoke => [invoke];
	}

	private sealed class QueuedEventReader(params IIncomingEvent[] events) : IEventReader
	{
		private readonly Queue<IIncomingEvent> _events = new(events);
		public int ReadCount { get; private set; }
		public bool TryReadEvent([MaybeNullWhen(false)] out IIncomingEvent incomingEvent)
		{
			ReadCount++;
			incomingEvent = _events.Count > 0 ? _events.Dequeue() : null!;
			return incomingEvent is not null;
		}
		public ValueTask<bool> WaitToEvent() => new(_events.Count > 0);
		public void Complete() { }
	}

	private static TestInterpreter CreateInterpreter(IEventReader eventReader, CancellationToken cancellationToken, IStateMachineContext? context = null, ILogger<StateMachineInterpreter>? logger = null, StateMachineRuntimeError? runtimeError = null, IInvokeController? invokeController = null, IInterpreterModel? model = null, UnhandledErrorBehaviour? unhandled = null) =>
		new()
		{
			StateMachineRuntimeError = runtimeError ?? new StateMachineRuntimeError(new ScopeObject()),
			StateMachineArguments = null!,
			DataConverter = new DataConverter(Mock.Of<ICaseSensitivity>()),
			CaseSensitivity = Mock.Of<ICaseSensitivity>(),
			EventReader = eventReader,
			Logger = logger ?? Mock.Of<ILogger<StateMachineInterpreter>>(),
			Model = model!,
			NotifyStateChanged = [],
			UnhandledErrorBehaviour = unhandled is { } behaviour ? Mock.Of<IUnhandledErrorBehaviour>(value => value.Behaviour == behaviour) : null,
			StateMachineContext = context!,
			InvokeController = invokeController!,
			DisposeToken = new DisposeToken(cancellationToken)
		};

	[SuppressMessage(category: "ReSharper", checkId: "RedundantOverriddenMember")]
	private sealed class TestInterpreter : StateMachineInterpreter
	{
		public StateMachineInterpreterState? NotifiedState { get; private set; }

		public ValueTask HandleMainLoopFailure(Exception exception) => HandleMainLoopException(exception);

		public bool IsInterpreterError(Exception exception) => IsError(exception);

		public override void TriggerDestroySignal(Exception? innerException) => base.TriggerDestroySignal(innerException);

		protected override ValueTask EnterSteps() => base.EnterSteps();

		protected override ValueTask<List<TransitionNode>> ExternalEventTransitions() => base.ExternalEventTransitions();

		protected override ValueTask<IIncomingEvent> ReadExternalEvent() => base.ReadExternalEvent();

		protected override bool IsError(Exception ex) => base.IsError(ex);

		protected override ValueTask NotifyInterpreterState(StateMachineInterpreterState state)
		{
			NotifiedState = state;

			return default;
		}
	}

	private sealed class InvokeSource(IFinalize? finalize = null, bool autoForward = false, FullUri? type = null) : IInvoke
	{
	#region Interface IInvoke

		public FullUri? Type => type;

		public IValueExpression? TypeExpression => null;

		public Uri? Source => null;

		public IValueExpression? SourceExpression => null;

		public string? Id => null;

		public ILocationExpression? IdLocation => null;

		public ImmutableArray<ILocationExpression> NameList => [];

		public bool AutoForward => autoForward;

		public ImmutableArray<IParam> Parameters => [];

		public IFinalize? Finalize => finalize;

		public IContent? Content => null;

	#endregion
	}
}
