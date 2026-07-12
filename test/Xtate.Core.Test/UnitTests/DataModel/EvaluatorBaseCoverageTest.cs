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
public class EvaluatorBaseCoverageTest
{
	[TestMethod]
	public async Task BaseEvaluatorsForwardAncestorAndStateMachineProperties()
	{
		var valueExpression = new ValueExpressionSource("value");
		var locationExpression = new LocationExpressionSource("location");
		var inlineContent = new InlineContentSource("inline");
		var executable = new ExecutableEntitySource();
		var condition = new ConditionExpressionSource("condition");
		var outgoingEvent = new OutgoingEventSource();
		var content = new ContentSource(valueExpression);
		var param = new ParamSource(name: "param", valueExpression, locationExpression);

		await AssertAssignEvaluator(new AssignSource(locationExpression, valueExpression, inlineContent), locationExpression, valueExpression, inlineContent);
		await AssertCancelEvaluator(new CancelSource(sendId: "send-id", valueExpression), valueExpression);
		await AssertCustomActionEvaluator(new CustomActionSource(locationExpression, valueExpression), locationExpression, valueExpression);
		await AssertIfEvaluator(new IfSource(condition, ImmutableArray.Create<IExecutableEntity>(executable)), condition, executable);
		await AssertLogEvaluator(new LogSource(label: "label", valueExpression), valueExpression);
		await AssertRaiseEvaluator(new RaiseSource(outgoingEvent), outgoingEvent);
		await AssertSendEvaluator(new SendSource(content, valueExpression, locationExpression, param), content, valueExpression, locationExpression, param);
	}

	[TestMethod]
	public async Task DefaultLogEvaluatorLogsExpressionOnlyWhenControllerIsEnabled()
	{
		var expression = new ValueExpressionSource("logged data");
		var enabledController = new CapturingLogController(isEnabled: true);
		var enabledEvaluator = new DefaultLogEvaluator(new LogSource(label: "label", expression))
							   {
								   LogController = () => new ValueTask<ILogController>(enabledController)
							   };

		await enabledEvaluator.Execute();

		Assert.AreEqual(expected: 1, enabledController.LogCount);
		Assert.AreEqual(expected: "label", enabledController.LastMessage);
		Assert.AreEqual(expected: "logged data", enabledController.LastArguments.AsString());
		Assert.AreEqual(expected: 1, expression.EvaluateObjectCount);

		var disabledExpression = new ValueExpressionSource("not logged");
		var disabledController = new CapturingLogController(isEnabled: false);
		var disabledEvaluator = new DefaultLogEvaluator(new LogSource(label: "disabled", disabledExpression))
								{
									LogController = () => new ValueTask<ILogController>(disabledController)
								};

		await disabledEvaluator.Execute();

		Assert.AreEqual(expected: 0, disabledController.LogCount);
		Assert.AreEqual(expected: 0, disabledExpression.EvaluateObjectCount);
	}

	private static async Task AssertAssignEvaluator(AssignSource source,
													ILocationExpression location,
													IValueExpression expression,
													IInlineContent inlineContent)
	{
		var evaluator = new TestAssignEvaluator(source);

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreSame(location, evaluator.Location);
		Assert.AreSame(expression, evaluator.Expression);
		Assert.AreSame(inlineContent, evaluator.InlineContent);
		Assert.AreEqual(expected: "assign-type", evaluator.Type);
		Assert.AreEqual(expected: "assign-attribute", evaluator.Attribute);

		await evaluator.Execute();

		Assert.AreEqual(expected: 1, evaluator.ExecuteCount);
	}

	private static async Task AssertCancelEvaluator(CancelSource source, IValueExpression expression)
	{
		var evaluator = new TestCancelEvaluator(source);

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreEqual(expected: "send-id", evaluator.SendId);
		Assert.AreSame(expression, evaluator.SendIdExpression);

		await evaluator.Execute();

		Assert.AreEqual(expected: 1, evaluator.ExecuteCount);
	}

	private static async Task AssertCustomActionEvaluator(CustomActionSource source,
														  ILocationExpression location,
														  IValueExpression expression)
	{
		var evaluator = new TestCustomActionEvaluator(source);

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreEqual(expected: "urn:test", evaluator.XmlNamespace);
		Assert.AreEqual(expected: "custom", evaluator.XmlName);
		Assert.AreEqual(expected: "<custom />", evaluator.Xml);
		Assert.AreSame(location, evaluator.Locations.Single());
		Assert.AreSame(expression, evaluator.Values.Single());

		await evaluator.Execute();

		Assert.AreEqual(expected: 1, evaluator.ExecuteCount);
	}

	private static async Task AssertIfEvaluator(IfSource source, IConditionExpression condition, IExecutableEntity executable)
	{
		var evaluator = new TestIfEvaluator(source);

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreSame(condition, evaluator.Condition);
		Assert.AreSame(executable, evaluator.Action.Single());

		await evaluator.Execute();

		Assert.AreEqual(expected: 1, evaluator.ExecuteCount);
	}

	private static async Task AssertLogEvaluator(LogSource source, IValueExpression expression)
	{
		var evaluator = new TestLogEvaluator(source);

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreEqual(expected: "label", evaluator.Label);
		Assert.AreSame(expression, evaluator.Expression);

		await evaluator.Execute();

		Assert.AreEqual(expected: 1, evaluator.ExecuteCount);
	}

	private static async Task AssertRaiseEvaluator(RaiseSource source, IOutgoingEvent outgoingEvent)
	{
		var evaluator = new TestRaiseEvaluator(source);

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreSame(outgoingEvent, evaluator.OutgoingEvent);

		await evaluator.Execute();

		Assert.AreEqual(expected: 1, evaluator.ExecuteCount);
	}

	private static async Task AssertSendEvaluator(SendSource source,
												  IContent content,
												  IValueExpression valueExpression,
												  ILocationExpression locationExpression,
												  IParam param)
	{
		var evaluator = new TestSendEvaluator(source);

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreSame(content, evaluator.Content);
		Assert.AreEqual(expected: "event-name", evaluator.EventName);
		Assert.AreSame(valueExpression, evaluator.EventExpression);
		Assert.AreEqual(new FullUri("https://target.test/"), evaluator.Target);
		Assert.AreSame(valueExpression, evaluator.TargetExpression);
		Assert.AreEqual(new FullUri("https://type.test/"), evaluator.Type);
		Assert.AreSame(valueExpression, evaluator.TypeExpression);
		Assert.AreEqual(expected: "id", evaluator.Id);
		Assert.AreSame(locationExpression, evaluator.IdLocation);
		Assert.AreEqual(expected: 42, evaluator.DelayMs);
		Assert.AreSame(valueExpression, evaluator.DelayExpression);
		Assert.AreSame(locationExpression, evaluator.NameList.Single());
		Assert.AreSame(param, evaluator.Parameters.Single());

		await evaluator.Execute();

		Assert.AreEqual(expected: 1, evaluator.ExecuteCount);
	}

	private sealed class TestAssignEvaluator(IAssign assign) : AssignEvaluator(assign)
	{
		public int ExecuteCount { get; private set; }

		public override ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}
	}

	private sealed class TestCancelEvaluator(ICancel cancel) : CancelEvaluator(cancel)
	{
		public int ExecuteCount { get; private set; }

		public override ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}
	}

	private sealed class TestCustomActionEvaluator(ICustomAction customAction) : CustomActionEvaluator(customAction)
	{
		public int ExecuteCount { get; private set; }

		public override ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}
	}

	private sealed class TestIfEvaluator(IIf iif) : IfEvaluator(iif)
	{
		public int ExecuteCount { get; private set; }

		public override ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}
	}

	private sealed class TestLogEvaluator(ILog log) : LogEvaluator(log)
	{
		public int ExecuteCount { get; private set; }

		public override ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}
	}

	private sealed class TestRaiseEvaluator(IRaise raise) : RaiseEvaluator(raise)
	{
		public int ExecuteCount { get; private set; }

		public override ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}
	}

	private sealed class TestSendEvaluator(ISend send) : SendEvaluator(send)
	{
		public int ExecuteCount { get; private set; }

		public override ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}
	}

	private sealed class CapturingLogController(bool isEnabled) : ILogController
	{
		public int LogCount { get; private set; }

		public string? LastMessage { get; private set; }

		public DataModelValue LastArguments { get; private set; }

	#region Interface ILogController

		public ValueTask Log(string? message, DataModelValue arguments)
		{
			LogCount ++;
			LastMessage = message;
			LastArguments = arguments;

			return ValueTask.CompletedTask;
		}

		public bool IsEnabled => isEnabled;

	#endregion
	}

	private sealed class ValueExpressionSource(string value) : IValueExpression, IObjectEvaluator
	{
		public int EvaluateObjectCount { get; private set; }

	#region Interface IObjectEvaluator

		public ValueTask<IObject> EvaluateObject()
		{
			EvaluateObjectCount ++;

			return new ValueTask<IObject>(new DataModelValue(value));
		}

	#endregion

	#region Interface IValueExpression

		public string? Expression => value;

	#endregion

		public ValueTask<DataModelValue> Evaluate() => new(value);
	}

	private sealed class LocationExpressionSource(string expression) : ILocationExpression
	{
	#region Interface ILocationExpression

		public string? Expression => expression;

	#endregion
	}

	private sealed class InlineContentSource(string value) : IInlineContent
	{
	#region Interface IInlineContent

		public string? Value => value;

	#endregion
	}

	private sealed class ExecutableEntitySource : IExecutableEntity;

	private sealed class ConditionExpressionSource(string expression) : IConditionExpression
	{
	#region Interface IConditionExpression

		public string? Expression => expression;

	#endregion
	}

	private sealed class OutgoingEventSource : IOutgoingEvent
	{
	#region Interface IOutgoingEvent

		public SendId? SendId => SendId.FromString("send-id");

		public EventName Name => (EventName) "outgoing";

		public FullUri? Target => new("https://target.test/");

		public FullUri? Type => new("https://type.test/");

		public int DelayMs => 42;

		public DataModelValue Data => "data";

	#endregion
	}

	private sealed class ContentSource(IValueExpression expression) : IContent
	{
	#region Interface IContent

		public IValueExpression? Expression => expression;

		public IContentBody? Body => null;

	#endregion
	}

	private sealed class ParamSource(string name, IValueExpression expression, ILocationExpression location) : IParam
	{
	#region Interface IParam

		public string Name => name;

		public IValueExpression? Expression => expression;

		public ILocationExpression? Location => location;

	#endregion
	}

	private sealed class AssignSource(
		ILocationExpression location,
		IValueExpression expression,
		IInlineContent inlineContent) : IAssign
	{
	#region Interface IAssign

		public ILocationExpression? Location => location;

		public IValueExpression? Expression => expression;

		public IInlineContent? InlineContent => inlineContent;

		public string? Type => "assign-type";

		public string? Attribute => "assign-attribute";

	#endregion
	}

	private sealed class CancelSource(string sendId, IValueExpression sendIdExpression) : ICancel
	{
	#region Interface ICancel

		public string? SendId => sendId;

		public IValueExpression? SendIdExpression => sendIdExpression;

	#endregion
	}

	private sealed class CustomActionSource(ILocationExpression location, IValueExpression value) : ICustomAction
	{
	#region Interface ICustomAction

		public string? XmlNamespace => "urn:test";

		public string? XmlName => "custom";

		public string? Xml => "<custom />";

		public ImmutableArray<ILocationExpression> Locations => ImmutableArray.Create(location);

		public ImmutableArray<IValueExpression> Values => ImmutableArray.Create(value);

	#endregion
	}

	private sealed class IfSource(IConditionExpression condition, ImmutableArray<IExecutableEntity> action) : IIf
	{
	#region Interface IIf

		public IConditionExpression? Condition => condition;

		public ImmutableArray<IExecutableEntity> Action => action;

	#endregion
	}

	private sealed class LogSource(string label, IValueExpression expression) : ILog
	{
	#region Interface ILog

		public string? Label => label;

		public IValueExpression? Expression => expression;

	#endregion
	}

	private sealed class RaiseSource(IOutgoingEvent outgoingEvent) : IRaise
	{
	#region Interface IRaise

		public IOutgoingEvent? OutgoingEvent => outgoingEvent;

	#endregion
	}

	private sealed class SendSource(
		IContent content,
		IValueExpression expression,
		ILocationExpression location,
		IParam param) : ISend
	{
	#region Interface ISend

		public string? EventName => "event-name";

		public IValueExpression? EventExpression => expression;

		public FullUri? Target => new("https://target.test/");

		public IValueExpression? TargetExpression => expression;

		public FullUri? Type => new("https://type.test/");

		public IValueExpression? TypeExpression => expression;

		public string? Id => "id";

		public ILocationExpression? IdLocation => location;

		public int? DelayMs => 42;

		public IValueExpression? DelayExpression => expression;

		public ImmutableArray<ILocationExpression> NameList => ImmutableArray.Create(location);

		public ImmutableArray<IParam> Parameters => ImmutableArray.Create(param);

		public IContent? Content => content;

	#endregion
	}
}