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
using Xtate.DataModel.Services;
using Xtate.DataTypes;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class DefaultEvaluatorCoverageTest
{
	[TestMethod]
	public async Task DefaultIfEvaluatorSelectsFirstTrueElseIfBranchAndStops()
	{
		var initialCondition = new BooleanCondition(false);
		var elseIfCondition = new BooleanCondition(true);
		var initialAction = new ExecutableAction();
		var selectedAction = new ExecutableAction();
		var elseAction = new ExecutableAction();
		var source = new IfSource(
			initialCondition,
			ImmutableArray.Create<IExecutableEntity>(
				initialAction,
				new ElseIfSource(elseIfCondition),
				selectedAction,
				new ElseSource(),
				elseAction));
		var evaluator = new DefaultIfEvaluator(source);

		await evaluator.Execute();

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreSame(initialCondition, evaluator.Condition);
		Assert.HasCount(expected: 5, evaluator.Action);
		Assert.AreEqual(expected: 1, initialCondition.EvaluateCount);
		Assert.AreEqual(expected: 1, elseIfCondition.EvaluateCount);
		Assert.AreEqual(expected: 0, initialAction.ExecuteCount);
		Assert.AreEqual(expected: 1, selectedAction.ExecuteCount);
		Assert.AreEqual(expected: 0, elseAction.ExecuteCount);
	}

	[TestMethod]
	public async Task DefaultIfEvaluatorExecutesElseAndHandlesEmptyActionList()
	{
		var elseAction = new ExecutableAction();
		var evaluator = new DefaultIfEvaluator(
			new IfSource(
				new BooleanCondition(false),
				ImmutableArray.Create<IExecutableEntity>(new ElseSource(), elseAction)));
		var emptyEvaluator = new DefaultIfEvaluator(new IfSource(new BooleanCondition(true), []));

		await evaluator.Execute();
		await emptyEvaluator.Execute();

		Assert.AreEqual(expected: 1, elseAction.ExecuteCount);
	}

	[TestMethod]
	public async Task DefaultAssignEvaluatorPrefersExpressionAndFallsBackToInlineContent()
	{
		var expressionLocation = new LocationExpression();
		var expression = new ObjectValueExpression("expression");
		var ignoredInline = new InlineObjectContent("ignored");
		var expressionSource = new AssignSource(expressionLocation, expression, ignoredInline);
		var expressionEvaluator = new DefaultAssignEvaluator(expressionSource);

		await expressionEvaluator.Execute();

		Assert.AreSame(expressionSource, ((IAncestorProvider) expressionEvaluator).Ancestor);
		Assert.AreSame(expressionLocation, expressionEvaluator.Location);
		Assert.AreSame(expression, expressionEvaluator.Expression);
		Assert.AreSame(ignoredInline, expressionEvaluator.InlineContent);
		Assert.AreEqual(expected: "type", expressionEvaluator.Type);
		Assert.AreEqual(expected: "attribute", expressionEvaluator.Attribute);
		Assert.AreEqual(expected: "expression", DataModelValue.FromObject(expressionLocation.Value!.ToObject()).AsString());

		var inlineLocation = new LocationExpression();
		var inlineSource = new AssignSource(inlineLocation, expression: null, new InlineObjectContent("inline"));
		var inlineEvaluator = new DefaultAssignEvaluator(inlineSource);

		await inlineEvaluator.Execute();

		Assert.AreEqual(expected: "inline", DataModelValue.FromObject(inlineLocation.Value!.ToObject()).AsString());
	}

	[TestMethod]
	public async Task DefaultCustomActionEvaluatorForwardsEntityAndCompletesWithoutContainer()
	{
		var source = new CustomActionSource();
		var evaluator = new DefaultCustomActionEvaluator(source);

		await evaluator.Execute();

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreEqual(expected: "urn:test", evaluator.XmlNamespace);
		Assert.AreEqual(expected: "custom", evaluator.XmlName);
		Assert.AreEqual(expected: "<custom />", evaluator.Xml);
		Assert.IsEmpty(evaluator.Values);
		Assert.IsEmpty(evaluator.Locations);
	}

	[TestMethod]
	public async Task DefaultSendEvaluatorUsesDynamicExpressionsContentAndIdLocation()
	{
		var idLocation = new LocationExpression();
		var controller = new EventController();
		var source = new SendSource
					 {
						 EventName = "static.event",
						 EventExpression = new StringValueExpression("dynamic.event"),
						 Target = new FullUri("urn:static-target"),
						 TargetExpression = new StringValueExpression("urn:dynamic-target"),
						 Type = new FullUri("urn:static-type"),
						 TypeExpression = new StringValueExpression("urn:dynamic-type"),
						 Id = "send-id",
						 IdLocation = idLocation,
						 DelayMs = 10,
						 DelayExpression = new IntegerValueExpression(25),
						 Content = new ContentSource(expression: null, new StringContentBody("raw-content"))
					 };
		var evaluator = CreateSendEvaluator(source, controller);

		await evaluator.Execute();

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.IsNotNull(controller.Sent);
		Assert.AreEqual(expected: "dynamic.event", controller.Sent.Name.ToString());
		Assert.AreEqual(new FullUri("urn:dynamic-target"), controller.Sent.Target);
		Assert.AreEqual(new FullUri("urn:dynamic-type"), controller.Sent.Type);
		Assert.AreEqual(expected: 25, controller.Sent.DelayMs);
		Assert.AreEqual(expected: "raw-content", controller.Sent.Data.AsString());
		Assert.AreEqual(expected: "raw-content", controller.RawData);
		Assert.AreEqual(expected: "send-id", controller.Sent.SendId!.ToString());
		Assert.AreSame(controller.Sent.SendId, idLocation.Value);
	}

	[TestMethod]
	public async Task DefaultSendEvaluatorUsesStaticValuesDefaultsAndGeneratedId()
	{
		var controller = new EventController();
		var source = new SendSource
					 {
						 EventName = "static.event",
						 Target = new FullUri("urn:static-target"),
						 Type = new FullUri("urn:static-type"),
						 DelayMs = null
					 };
		var evaluator = CreateSendEvaluator(source, controller);

		await evaluator.Execute();

		Assert.IsNotNull(controller.Sent);
		Assert.AreEqual(expected: "static.event", controller.Sent.Name.ToString());
		Assert.AreEqual(new FullUri("urn:static-target"), controller.Sent.Target);
		Assert.AreEqual(new FullUri("urn:static-type"), controller.Sent.Type);
		Assert.AreEqual(expected: 0, controller.Sent.DelayMs);
		Assert.IsTrue(controller.Sent.Data.IsUndefined());
		Assert.IsNotNull(controller.Sent.SendId);
		Assert.IsNull(controller.RawData);
	}

	private static DefaultSendEvaluator CreateSendEvaluator(ISend source, IEventController controller) =>
		new(source)
		{
			DataConverter = static () => new ValueTask<DataConverter>(new DataConverter(caseSensitivity: null)),
			EventController = () => new ValueTask<IEventController>(controller)
		};

	private sealed class BooleanCondition(bool result) : IConditionExpression, IBooleanEvaluator
	{
		public int EvaluateCount { get; private set; }

	#region Interface IBooleanEvaluator

		public ValueTask<bool> EvaluateBoolean()
		{
			EvaluateCount ++;

			return new ValueTask<bool>(result);
		}

	#endregion

	#region Interface IConditionExpression

		public string? Expression => result.ToString();

	#endregion
	}

	private sealed class ExecutableAction : IExecutableEntity, IExecEvaluator
	{
		public int ExecuteCount { get; private set; }

	#region Interface IExecEvaluator

		public ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}

	#endregion
	}

	private sealed class IfSource(IConditionExpression condition, ImmutableArray<IExecutableEntity> action) : IIf
	{
	#region Interface IIf

		public IConditionExpression? Condition => condition;

		public ImmutableArray<IExecutableEntity> Action => action;

	#endregion
	}

	private sealed class ElseIfSource(IConditionExpression condition) : IElseIf
	{
	#region Interface IElseIf

		public IConditionExpression? Condition => condition;

	#endregion
	}

	private sealed class ElseSource : IElse;

	private sealed class ObjectValueExpression(string value) : IValueExpression, IObjectEvaluator
	{
	#region Interface IObjectEvaluator

		public ValueTask<IObject> EvaluateObject() => new(new DataModelValue(value));

	#endregion

	#region Interface IValueExpression

		public string? Expression => value;

	#endregion
	}

	private sealed class StringValueExpression(string value) : IValueExpression, IStringEvaluator
	{
	#region Interface IStringEvaluator

		public ValueTask<string> EvaluateString() => new(value);

	#endregion

	#region Interface IValueExpression

		public string? Expression => value;

	#endregion
	}

	private sealed class IntegerValueExpression(int value) : IValueExpression, IIntegerEvaluator
	{
	#region Interface IIntegerEvaluator

		public ValueTask<int> EvaluateInteger() => new(value);

	#endregion

	#region Interface IValueExpression

		public string? Expression => value.ToString();

	#endregion
	}

	private sealed class InlineObjectContent(string value) : IInlineContent, IObjectEvaluator
	{
	#region Interface IInlineContent

		public string? Value => value;

	#endregion

	#region Interface IObjectEvaluator

		public ValueTask<IObject> EvaluateObject() => new(new DataModelValue(value));

	#endregion
	}

	private sealed class LocationExpression : ILocationExpression, ILocationEvaluator
	{
		public IObject? Value { get; private set; }

	#region Interface ILocationEvaluator

		public ValueTask SetValue(IObject value)
		{
			Value = value;

			return ValueTask.CompletedTask;
		}

		public ValueTask<IObject> GetValue() => new(Value ?? DataModelValue.Undefined);

		public ValueTask<string> GetName() => new("location");

	#endregion

	#region Interface ILocationExpression

		public string? Expression => "location";

	#endregion
	}

	private sealed class AssignSource(ILocationExpression location, IValueExpression? expression, IInlineContent inlineContent) : IAssign
	{
	#region Interface IAssign

		public ILocationExpression? Location => location;

		public IValueExpression? Expression => expression;

		public IInlineContent? InlineContent => inlineContent;

		public string? Type => "type";

		public string? Attribute => "attribute";

	#endregion
	}

	private sealed class StringContentBody(string value) : IContentBody, IStringEvaluator
	{
	#region Interface IContentBody

		public string? Value => value;

	#endregion

	#region Interface IStringEvaluator

		public ValueTask<string> EvaluateString() => new(value);

	#endregion
	}

	private sealed class ContentSource(IValueExpression? expression, IContentBody? body) : IContent
	{
	#region Interface IContent

		public IValueExpression? Expression => expression;

		public IContentBody? Body => body;

	#endregion
	}

	private sealed class SendSource : ISend
	{
	#region Interface ISend

		public string? EventName { get; init; }

		public IValueExpression? EventExpression { get; init; }

		public FullUri? Target { get; init; }

		public IValueExpression? TargetExpression { get; init; }

		public FullUri? Type { get; init; }

		public IValueExpression? TypeExpression { get; init; }

		public string? Id { get; init; }

		public ILocationExpression? IdLocation { get; init; }

		public int? DelayMs { get; init; }

		public IValueExpression? DelayExpression { get; init; }

		public ImmutableArray<ILocationExpression> NameList { get; } = [];

		public ImmutableArray<IParam> Parameters { get; } = [];

		public IContent? Content { get; init; }

	#endregion
	}

	private sealed class EventController : IEventController
	{
		public IOutgoingEvent? Sent { get; private set; }

		public string? RawData { get; private set; }

	#region Interface IEventController

		public ValueTask Send(IOutgoingEvent outgoingEvent)
		{
			Sent = outgoingEvent;
			RawData = outgoingEvent is EventEntity eventEntity ? eventEntity.RawData : null;

			return ValueTask.CompletedTask;
		}

		public ValueTask Cancel(SendId sendId) => ValueTask.CompletedTask;

	#endregion
	}

	private sealed class CustomActionSource : ICustomAction
	{
	#region Interface ICustomAction

		public string? XmlNamespace => "urn:test";

		public string? XmlName => "custom";

		public string? Xml => "<custom />";

		public ImmutableArray<ILocationExpression> Locations => [];

		public ImmutableArray<IValueExpression> Values => [];

	#endregion
	}
}