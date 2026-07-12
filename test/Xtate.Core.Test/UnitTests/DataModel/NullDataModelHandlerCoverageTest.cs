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

using Xtate.Ancestor;
using Xtate.DataModel;
using Xtate.DataModel.Null.Services;
using Xtate.DataModel.Services;
using Xtate.StateMachine;
using Xtate.StateMachine.Validator;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class NullDataModelHandlerCoverageTest
{
	[TestMethod]
	public async Task NullConditionExpressionCreatesEvaluatorForInStateExpression()
	{
		var inStateController = new Mock<IInStateController>();
		var handler = CreateHandler(Mock.Of<IErrorProcessorService<NullDataModelHandler>>(), inStateController.Object);
		var source = new ConditionExpressionSource("In(active)");
		IConditionExpression conditionExpression = source;

		inStateController.Setup(static controller => controller.InState(It.Is<IIdentifier>(id => id.ToString() == "active"))).Returns(true);

		((IDataModelHandler) handler).Process(ref conditionExpression);

		var evaluator = (NullConditionExpressionEvaluator) conditionExpression;

		Assert.AreEqual("In(active)", evaluator.Expression);
		Assert.IsInstanceOfType<IBooleanEvaluator>(evaluator);
		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.IsTrue(await ((IBooleanEvaluator) evaluator).EvaluateBoolean());
		inStateController.Verify(static controller => controller.InState(It.Is<IIdentifier>(id => id.ToString() == "active")), Times.Once);
	}

	[TestMethod]
	public async Task NullConditionExpressionEvaluatorReturnsFalseWhenControllerIsUnavailable()
	{
		var source = new ConditionExpressionSource("In(active)");
		var evaluator = new NullConditionExpressionEvaluator(source, Identifier.FromString("active"))
						{
							InStateControllerFactory = static () => new ValueTask<IInStateController?>((IInStateController?) null)
						};

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.IsFalse(await ((IBooleanEvaluator) evaluator).EvaluateBoolean());
	}

	[TestMethod]
	public void NullDataModelHandlerReportsUnsupportedAndInvalidEntities()
	{
		var errorProcessor = new Mock<IErrorProcessorService<NullDataModelHandler>>();
		var handler = CreateHandler(errorProcessor.Object, Mock.Of<IInStateController>());
		IConditionExpression invalidCondition = new ConditionExpressionSource("state == active");
		IConditionExpression invalidInState = new ConditionExpressionSource("In()");
		IValueExpression valueExpression = new ValueExpressionSource("value");
		ILocationExpression locationExpression = new LocationExpressionSource("location");
		IExecutableEntity forEachExecutable = new ForEachSource();
		IScript script = new ScriptSource();
		IDataModel dataModel = new DataModelSource();
		IDoneData doneData = new DoneDataSource();

		((IDataModelHandler) handler).Process(ref invalidCondition);
		((IDataModelHandler) handler).Process(ref invalidInState);
		((IDataModelHandler) handler).Process(ref valueExpression);
		((IDataModelHandler) handler).Process(ref locationExpression);
		((IDataModelHandler) handler).Process(ref forEachExecutable);
		handler.Process(ref script);
		handler.Process(ref dataModel);
		handler.Process(ref doneData);

		errorProcessor.Verify(processor => processor.AddError(invalidCondition, It.IsAny<string>(), null), Times.Once);
		errorProcessor.Verify(processor => processor.AddError(invalidInState, It.IsAny<string>(), null), Times.Once);
		errorProcessor.Verify(processor => processor.AddError(valueExpression, It.IsAny<string>(), null), Times.Once);
		errorProcessor.Verify(processor => processor.AddError(locationExpression, It.IsAny<string>(), null), Times.Once);
		errorProcessor.Verify(processor => processor.AddError(forEachExecutable, It.IsAny<string>(), null), Times.Once);
		errorProcessor.Verify(processor => processor.AddError(script, It.IsAny<string>(), null), Times.Once);
		errorProcessor.Verify(processor => processor.AddError(dataModel, It.IsAny<string>(), null), Times.Once);
		errorProcessor.Verify(processor => processor.AddError(doneData, It.IsAny<string>(), null), Times.Once);
	}

	private static TestNullDataModelHandler CreateHandler(IErrorProcessorService<NullDataModelHandler> errorProcessor, IInStateController inStateController)
	{
		return new TestNullDataModelHandler
			   {
				   NullErrorProcessorService = errorProcessor,
				   NullConditionExpressionEvaluatorFactory = (expression, state) => new NullConditionExpressionEvaluator(expression, state)
																				  {
																					  InStateControllerFactory = () => new ValueTask<IInStateController?>(inStateController)
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

	private sealed class TestNullDataModelHandler : NullDataModelHandler
	{
		public void Process(ref IScript script) => Visit(ref script);

		public void Process(ref IDataModel dataModel) => Visit(ref dataModel);

		public void Process(ref IDoneData doneData) => Visit(ref doneData);
	}

	private sealed class ConditionExpressionSource(string expression) : IConditionExpression
	{
		public string? Expression => expression;
	}

	private sealed class ValueExpressionSource(string expression) : IValueExpression
	{
		public string? Expression => expression;
	}

	private sealed class LocationExpressionSource(string expression) : ILocationExpression
	{
		public string? Expression => expression;
	}

	private sealed class ForEachSource : IForEach
	{
		public IValueExpression? Array => null;

		public ILocationExpression? Item => null;

		public ILocationExpression? Index => null;

		public ImmutableArray<IExecutableEntity> Action => [];
	}

	private sealed class ScriptSource : IScript
	{
		public IScriptExpression? Content => null;

		public IExternalScriptExpression? Source => null;
	}

	private sealed class DataModelSource : IDataModel
	{
		public ImmutableArray<IData> Data => [];
	}

	private sealed class DoneDataSource : IDoneData
	{
		public IContent? Content => null;

		public ImmutableArray<IParam> Parameters => [];
	}
}
