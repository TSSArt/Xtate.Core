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

using System.Xml;
using Xtate.Ancestor;
using Xtate.DataModel;
using Xtate.DataModel.Runtime;
using Xtate.DataModel.Runtime.Services;
using Xtate.DataModel.Services;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.NameTable;
using Xtate.StateMachine;
using Xtate.StateMachine.Validator;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class RuntimeAndActionProviderCoverageTest
{
	[TestMethod]
	public async Task RuntimeActionWrapsSyncAndAsyncDelegates()
	{
		var syncCalls = 0;
		var asyncCalls = 0;

		await RuntimeAction.GetAction(() => syncCalls ++).DoAction();
		await RuntimeAction.GetAction(() =>
									  {
										  asyncCalls ++;

										  return ValueTask.CompletedTask;
									  })
						   .DoAction();

		Assert.AreEqual(expected: 1, syncCalls);
		Assert.AreEqual(expected: 1, asyncCalls);
		Assert.IsInstanceOfType(RuntimeAction.GetAction(() => { }), typeof(IExecutableEntity));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => RuntimeAction.GetAction((Action) null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => RuntimeAction.GetAction(null!));
	}

	[TestMethod]
	public async Task RuntimePredicateWrapsSyncAndAsyncDelegates()
	{
		var sync = (RuntimePredicate) RuntimePredicate.GetPredicate(() => true);
		var async = (RuntimePredicate) RuntimePredicate.GetPredicate(() => new ValueTask<bool>(false));

		Assert.IsNull(sync.Expression);
		Assert.IsTrue(await sync.Evaluate());
		Assert.IsFalse(await async.Evaluate());
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => RuntimePredicate.GetPredicate((Func<bool>) null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => RuntimePredicate.GetPredicate((Func<ValueTask<bool>>) null!));
	}

	[TestMethod]
	public async Task RuntimeValueWrapsConstantSyncAndAsyncDelegates()
	{
		var constant = RuntimeValue.GetValue(new DataModelValue("constant"));
		var sync = RuntimeValue.GetValue(() => new DataModelValue("sync"));
		var async = RuntimeValue.GetValue(() => new ValueTask<DataModelValue>(new DataModelValue("async")));

		Assert.IsNull(constant.Expression);
		Assert.AreEqual(expected: "constant", (await constant.Evaluate()).AsString());
		Assert.AreEqual(expected: "sync", (await sync.Evaluate()).AsString());
		Assert.AreEqual(expected: "async", (await async.Evaluate()).AsString());
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => RuntimeValue.GetValue((Func<DataModelValue>) null!));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => RuntimeValue.GetValue((Func<ValueTask<DataModelValue>>) null!));
	}

	[TestMethod]
	public async Task RuntimeDataModelHandlerWrapsRuntimeEntitiesAndReportsUnsupportedEntities()
	{
		var errorProcessor = new Mock<IErrorProcessorService<RuntimeDataModelHandler>>();
		var handler = CreateRuntimeHandler(errorProcessor.Object);
		var conditionExpression = RuntimePredicate.GetPredicate(() => true);
		IValueExpression valueExpression = RuntimeValue.GetValue(new DataModelValue("runtime value"));
		var actionCalls = 0;
		IExecutableEntity executableEntity = RuntimeAction.GetAction(() => actionCalls ++);

		((IDataModelHandler) handler).Process(ref conditionExpression);
		((IDataModelHandler) handler).Process(ref valueExpression);
		((IDataModelHandler) handler).Process(ref executableEntity);

		var predicateEvaluator = (RuntimePredicateEvaluator) conditionExpression;
		var valueEvaluator = (RuntimeValueEvaluator) valueExpression;
		var actionExecutor = (RuntimeActionExecutor) executableEntity;

		Assert.IsNull(predicateEvaluator.Expression);
		Assert.IsNull(valueEvaluator.Expression);
		Assert.IsTrue(await predicateEvaluator.EvaluateBoolean());
		Assert.AreEqual(expected: "runtime value", DataModelValue.FromObject(await valueEvaluator.EvaluateObject()).AsString());

		await actionExecutor.Execute();

		Assert.AreEqual(expected: 1, actionCalls);

		IConditionExpression invalidCondition = new ConditionExpressionSource("x > 1");
		IValueExpression invalidValue = new ValueExpressionSource("value");
		IExecutableEntity invalidExecutable = new ExecutableEntitySource();
		IScript script = new ScriptSource();
		IDataModel dataModel = new DataModelSource();

		((IDataModelHandler) handler).Process(ref invalidCondition);
		((IDataModelHandler) handler).Process(ref invalidValue);
		((IDataModelHandler) handler).Process(ref invalidExecutable);
		handler.Process(ref script);
		handler.Process(ref dataModel);

		errorProcessor.Verify(processor => processor.AddError(invalidCondition, It.IsAny<string>(), null), Times.Once);
		errorProcessor.Verify(processor => processor.AddError(invalidValue, It.IsAny<string>(), null), Times.Once);
		errorProcessor.Verify(processor => processor.AddError(invalidExecutable, It.IsAny<string>(), null), Times.Once);
		errorProcessor.Verify(processor => processor.AddError(script, It.IsAny<string>(), null), Times.Once);
		errorProcessor.Verify(processor => processor.AddError(dataModel, It.IsAny<string>(), null), Times.Once);
	}

	[TestMethod]
	public async Task RuntimeFacadeDelegatesToCurrentExecutionContextControllers()
	{
		var dataModel = new DataModelList
						{
							["_x"] = new DataModelList { ["args"] = new DataModelValue("runtime arguments") },
							["value"] = new DataModelValue("runtime data")
						};
		var stateMachineArguments = new Mock<IStateMachineArguments>();
		var dataModelController = new Mock<IDataModelController>();
		var inStateController = new Mock<IInStateController>();
		var logController = new Mock<ILogController>();
		var eventController = new Mock<IEventController>();
		var invokeController = new Mock<IInvokeController>();
		var outgoingEvent = Mock.Of<IOutgoingEvent>();
		var sendId = SendId.FromString("send-id")!;
		var invokeId = InvokeId.FromString("invoke-id");
		var invokeData = new InvokeData(invokeId, Type: "urn:invoke:type", Source: null, RawContent: null, DataModelValue.Undefined, DataModelValue.Undefined);

		dataModelController.SetupGet(static controller => controller.DataModel).Returns(dataModel);
		inStateController.Setup(static controller => controller.InState(It.Is<IIdentifier>(id => id.ToString() == "active"))).Returns(true);
		logController.Setup(static controller => controller.Log("message", It.Is<DataModelValue>(value => value.AsString() == "argument"))).Returns(ValueTask.CompletedTask);
		eventController.Setup(controller => controller.Send(outgoingEvent)).Returns(ValueTask.CompletedTask);
		eventController.Setup(controller => controller.Cancel(sendId)).Returns(ValueTask.CompletedTask);
		invokeController.Setup(controller => controller.Start(invokeData)).Returns(ValueTask.CompletedTask);
		invokeController.Setup(controller => controller.Cancel(invokeId)).Returns(ValueTask.CompletedTask);
		stateMachineArguments.Setup(x => x.Arguments).Returns("runtime arguments");

		var executor = new RuntimeActionExecutor
					   {
						   Action = RuntimeAction.GetAction(async () =>
															{
																Assert.AreSame(dataModel, Runtime.DataModel);
																Assert.AreEqual(expected: "runtime arguments", Runtime.Arguments.AsString());
																Assert.IsTrue(Runtime.InState("active"));

																await Runtime.Log(message: "message", new DataModelValue("argument"));
																await Runtime.SendEvent(outgoingEvent);
																await Runtime.CancelEvent(sendId);
																await Runtime.StartInvoke(invokeData);
																await Runtime.CancelInvoke(invokeId);
															}),
						   RuntimeExecutionContextFactory = () => new ValueTask<RuntimeExecutionContext>(
																new RuntimeExecutionContext
																{
																	InStateController = inStateController.Object,
																	LogController = logController.Object,
																	EventController = eventController.Object,
																	InvokeController = invokeController.Object,
																	DataModelController = dataModelController.Object
																})
					   };

		await executor.Execute();

		inStateController.Verify(static controller => controller.InState(It.Is<IIdentifier>(id => id.ToString() == "active")), Times.Once);
		logController.Verify(static controller => controller.Log("message", It.Is<DataModelValue>(value => value.AsString() == "argument")), Times.Once);
		eventController.Verify(controller => controller.Send(outgoingEvent), Times.Once);
		eventController.Verify(controller => controller.Cancel(sendId), Times.Once);
		invokeController.Verify(controller => controller.Start(invokeData), Times.Once);
		invokeController.Verify(controller => controller.Cancel(invokeId), Times.Once);
	}

	[TestMethod]
	public void ActionProviderReturnsActivatorOnlyForConfiguredNameAndNamespace()
	{
		var provider = CreateActionProvider();

		Assert.AreSame(provider, provider.TryGetActivator(ns1: "urn:test", name1: "custom"));
		Assert.IsNull(provider.TryGetActivator(ns1: "urn:test", name1: "other"));
		Assert.IsNull(provider.TryGetActivator(ns1: "urn:other", name1: "custom"));
	}

	[TestMethod]
	public void ActionProviderActivatesActionFromXmlReaderPositionedOnElement()
	{
		var provider = CreateActionProvider();

		var action = (TestAction) provider.Activate("<custom xmlns=\"urn:test\" attr=\"value\" />");

		Assert.AreEqual(expected: "urn:test", action.NamespaceUri);
		Assert.AreEqual(expected: "custom", action.LocalName);
		Assert.AreEqual(expected: "value", action.AttributeValue);
	}

	[TestMethod]
	public void CaseSensitivityCopiesHandlerSettingAndDefaultsToCaseSensitive()
	{
		Assert.IsFalse(new CaseSensitivity(dataModelHandler: null).CaseInsensitive);

		var handler = new Mock<IDataModelHandler>();
		handler.SetupGet(static h => h.CaseInsensitive).Returns(true);

		Assert.IsTrue(new CaseSensitivity(handler.Object).CaseInsensitive);
	}

	[TestMethod]
	public async Task ContentBodyEvaluatorExposesAncestorValueStringAndObjectEvaluation()
	{
		var body = new ContentBodySource { Value = "body text" };
		var evaluator = new TestContentBodyEvaluator(body, new DataModelValue("object text"));

		Assert.AreSame(body, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreEqual(expected: "body text", evaluator.Value);
		Assert.AreEqual(expected: "body text", await evaluator.EvaluateString());
		Assert.AreEqual(expected: "object text", DataModelValue.FromObject(await evaluator.EvaluateObject()).AsString());
	}

	private static TestActionProvider CreateActionProvider()
	{
		var nameTableProvider = new Mock<INameTableProvider>();
		nameTableProvider.Setup(static provider => provider.GetNameTable()).Returns(new System.Xml.NameTable());

		return new TestActionProvider
			   {
				   NameTableProvider = nameTableProvider.Object,
				   CustomActionFactory = reader => new TestAction(reader)
			   };
	}

	private static TestRuntimeDataModelHandler CreateRuntimeHandler(IErrorProcessorService<RuntimeDataModelHandler> errorProcessor)
	{
		return new TestRuntimeDataModelHandler
			   {
				   RuntimeErrorProcessorService = errorProcessor,
				   RuntimePredicateEvaluatorFactory = predicate => new RuntimePredicateEvaluator
																   {
																	   Predicate = predicate,
																	   RuntimeExecutionContextFactory = CreateRuntimeExecutionContext
																   },
				   RuntimeValueEvaluatorFactory = value => new RuntimeValueEvaluator
														   {
															   Value = value,
															   RuntimeExecutionContextFactory = CreateRuntimeExecutionContext
														   },
				   RuntimeActionExecutorFactory = action => new RuntimeActionExecutor
															{
																Action = action,
																RuntimeExecutionContextFactory = CreateRuntimeExecutionContext
															},
				   DefaultLogEvaluatorFactory = _ => null!,
				   DefaultSendEvaluatorFactory = _ => null!,
				   DefaultCancelEvaluatorFactory = _ => null!,
				   DefaultIfEvaluatorFactory = _ => null!,
				   DefaultRaiseEvaluatorFactory = _ => null!,
				   DefaultForEachEvaluatorFactory = _ => null!,
				   DefaultAssignEvaluatorFactory = _ => null!,
				   DefaultScriptEvaluatorFactory = _ => null!,
				   DefaultCustomActionEvaluatorFactory = _ => null!,
				   DefaultContentBodyEvaluatorFactory = _ => null!,
				   DefaultInlineContentEvaluatorFactory = _ => null!,
				   DefaultExternalDataExpressionEvaluatorFactory = _ => null!,
				   CustomActionContainerFactory = _ => null!
			   };
	}

	private static ValueTask<RuntimeExecutionContext> CreateRuntimeExecutionContext() =>
		new(
			new RuntimeExecutionContext
			{
				InStateController = Mock.Of<IInStateController>(),
				LogController = Mock.Of<ILogController>(),
				EventController = Mock.Of<IEventController>(),
				InvokeController = Mock.Of<IInvokeController>(),
				DataModelController = Mock.Of<IDataModelController>(static controller => controller.DataModel == new DataModelList())
			});

	private sealed class TestActionProvider() : ActionProvider<TestAction>(ns: "urn:test", name: "custom");

	private sealed class TestRuntimeDataModelHandler : RuntimeDataModelHandler
	{
		public void Process(ref IScript script) => Visit(ref script);

		public void Process(ref IDataModel dataModel) => Visit(ref dataModel);
	}

	private sealed class TestAction : IAction
	{
		public TestAction(XmlReader reader)
		{
			NamespaceUri = reader.NamespaceURI;
			LocalName = reader.LocalName;
			AttributeValue = reader.GetAttribute("attr");
		}

		public string NamespaceUri { get; }

		public string LocalName { get; }

		public string? AttributeValue { get; }

	#region Interface IAction

		public IEnumerable<IActionValue> GetValues() => [];

		public IEnumerable<IActionLocation> GetLocations() => [];

		public ValueTask Execute() => ValueTask.CompletedTask;

	#endregion
	}

	private sealed class ContentBodySource : IContentBody
	{
	#region Interface IContentBody

		public string? Value { get; init; }

	#endregion
	}

	private sealed class TestContentBodyEvaluator(IContentBody contentBody, DataModelValue value) : ContentBodyEvaluator(contentBody)
	{
		public override ValueTask<IObject> EvaluateObject() => new(value);
	}

	private sealed class ConditionExpressionSource(string expression) : IConditionExpression
	{
	#region Interface IConditionExpression

		public string? Expression => expression;

	#endregion
	}

	private sealed class ValueExpressionSource(string expression) : IValueExpression
	{
	#region Interface IValueExpression

		public string? Expression => expression;

	#endregion
	}

	private sealed class ExecutableEntitySource : IExecutableEntity;

	private sealed class ScriptSource : IScript
	{
	#region Interface IScript

		public IScriptExpression? Content => null;

		public IExternalScriptExpression? Source => null;

	#endregion
	}

	private sealed class DataModelSource : IDataModel
	{
	#region Interface IDataModel

		public ImmutableArray<IData> Data => [];

	#endregion
	}
}