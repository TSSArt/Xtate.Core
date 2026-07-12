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
using Xtate.IoC.Tools;
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
		var param = new ParamSource("param", valueExpression, locationExpression);

		await AssertAssignEvaluator(new AssignSource(locationExpression, valueExpression, inlineContent), locationExpression, valueExpression, inlineContent);
		await AssertCancelEvaluator(new CancelSource("send-id", valueExpression), valueExpression);
		await AssertCustomActionEvaluator(new CustomActionSource(locationExpression, valueExpression), locationExpression, valueExpression);
		await AssertIfEvaluator(new IfSource(condition, ImmutableArray.Create<IExecutableEntity>(executable)), condition, executable);
		await AssertLogEvaluator(new LogSource("label", valueExpression), valueExpression);
		await AssertRaiseEvaluator(new RaiseSource(outgoingEvent), outgoingEvent);
		await AssertSendEvaluator(new SendSource(content, valueExpression, locationExpression, param), content, valueExpression, locationExpression, param);
	}

	[TestMethod]
	public async Task DefaultLogEvaluatorLogsExpressionOnlyWhenControllerIsEnabled()
	{
		var expression = new ValueExpressionSource("logged data");
		var enabledController = new CapturingLogController(isEnabled: true);
		var enabledEvaluator = new DefaultLogEvaluator(new LogSource("label", expression))
							   {
								   LogController = () => new ValueTask<ILogController>(enabledController)
							   };

		await enabledEvaluator.Execute();

		Assert.AreEqual(1, enabledController.LogCount);
		Assert.AreEqual("label", enabledController.LastMessage);
		Assert.AreEqual("logged data", enabledController.LastArguments.AsString());
		Assert.AreEqual(1, expression.EvaluateObjectCount);

		var disabledExpression = new ValueExpressionSource("not logged");
		var disabledController = new CapturingLogController(isEnabled: false);
		var disabledEvaluator = new DefaultLogEvaluator(new LogSource("disabled", disabledExpression))
								 {
									 LogController = () => new ValueTask<ILogController>(disabledController)
								 };

		await disabledEvaluator.Execute();

		Assert.AreEqual(0, disabledController.LogCount);
		Assert.AreEqual(0, disabledExpression.EvaluateObjectCount);
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
		Assert.AreEqual("assign-type", evaluator.Type);
		Assert.AreEqual("assign-attribute", evaluator.Attribute);

		await evaluator.Execute();

		Assert.AreEqual(1, evaluator.ExecuteCount);
	}

	private static async Task AssertCancelEvaluator(CancelSource source, IValueExpression expression)
	{
		var evaluator = new TestCancelEvaluator(source);

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreEqual("send-id", evaluator.SendId);
		Assert.AreSame(expression, evaluator.SendIdExpression);

		await evaluator.Execute();

		Assert.AreEqual(1, evaluator.ExecuteCount);
	}

	private static async Task AssertCustomActionEvaluator(CustomActionSource source,
														 ILocationExpression location,
														 IValueExpression expression)
	{
		var evaluator = new TestCustomActionEvaluator(source);

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreEqual("urn:test", evaluator.XmlNamespace);
		Assert.AreEqual("custom", evaluator.XmlName);
		Assert.AreEqual("<custom />", evaluator.Xml);
		Assert.AreSame(location, evaluator.Locations.Single());
		Assert.AreSame(expression, evaluator.Values.Single());

		await evaluator.Execute();

		Assert.AreEqual(1, evaluator.ExecuteCount);
	}

	private static async Task AssertIfEvaluator(IfSource source, IConditionExpression condition, IExecutableEntity executable)
	{
		var evaluator = new TestIfEvaluator(source);

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreSame(condition, evaluator.Condition);
		Assert.AreSame(executable, evaluator.Action.Single());

		await evaluator.Execute();

		Assert.AreEqual(1, evaluator.ExecuteCount);
	}

	private static async Task AssertLogEvaluator(LogSource source, IValueExpression expression)
	{
		var evaluator = new TestLogEvaluator(source);

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreEqual("label", evaluator.Label);
		Assert.AreSame(expression, evaluator.Expression);

		await evaluator.Execute();

		Assert.AreEqual(1, evaluator.ExecuteCount);
	}

	private static async Task AssertRaiseEvaluator(RaiseSource source, IOutgoingEvent outgoingEvent)
	{
		var evaluator = new TestRaiseEvaluator(source);

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreSame(outgoingEvent, evaluator.OutgoingEvent);

		await evaluator.Execute();

		Assert.AreEqual(1, evaluator.ExecuteCount);
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
		Assert.AreEqual("event-name", evaluator.EventName);
		Assert.AreSame(valueExpression, evaluator.EventExpression);
		Assert.AreEqual(new FullUri("https://target.test/"), evaluator.Target);
		Assert.AreSame(valueExpression, evaluator.TargetExpression);
		Assert.AreEqual(new FullUri("https://type.test/"), evaluator.Type);
		Assert.AreSame(valueExpression, evaluator.TypeExpression);
		Assert.AreEqual("id", evaluator.Id);
		Assert.AreSame(locationExpression, evaluator.IdLocation);
		Assert.AreEqual(42, evaluator.DelayMs);
		Assert.AreSame(valueExpression, evaluator.DelayExpression);
		Assert.AreSame(locationExpression, evaluator.NameList.Single());
		Assert.AreSame(param, evaluator.Parameters.Single());

		await evaluator.Execute();

		Assert.AreEqual(1, evaluator.ExecuteCount);
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

		public bool IsEnabled => isEnabled;

		public ValueTask Log(string? message, DataModelValue arguments)
		{
			LogCount ++;
			LastMessage = message;
			LastArguments = arguments;

			return ValueTask.CompletedTask;
		}
	}

	private sealed class ValueExpressionSource(string value) : IValueExpression, IObjectEvaluator
	{
		public int EvaluateObjectCount { get; private set; }

		public string? Expression => value;

		public ValueTask<DataModelValue> Evaluate() => new(value);

		public ValueTask<IObject> EvaluateObject()
		{
			EvaluateObjectCount ++;

			return new(new DataModelValue(value));
		}
	}

	private sealed class LocationExpressionSource(string expression) : ILocationExpression
	{
		public string? Expression => expression;
	}

	private sealed class InlineContentSource(string value) : IInlineContent
	{
		public string? Value => value;
	}

	private sealed class ExecutableEntitySource : IExecutableEntity;

	private sealed class ConditionExpressionSource(string expression) : IConditionExpression
	{
		public string? Expression => expression;
	}

	private sealed class OutgoingEventSource : IOutgoingEvent
	{
		public SendId? SendId => SendId.FromString("send-id");

		public EventName Name => (EventName) "outgoing";

		public FullUri? Target => new("https://target.test/");

		public FullUri? Type => new("https://type.test/");

		public int DelayMs => 42;

		public DataModelValue Data => "data";
	}

	private sealed class ContentSource(IValueExpression expression) : IContent
	{
		public IValueExpression? Expression => expression;

		public IContentBody? Body => null;
	}

	private sealed class ParamSource(string name, IValueExpression expression, ILocationExpression location) : IParam
	{
		public string Name => name;

		public IValueExpression? Expression => expression;

		public ILocationExpression? Location => location;
	}

	private sealed class AssignSource(ILocationExpression location,
									  IValueExpression expression,
									  IInlineContent inlineContent) : IAssign
	{
		public ILocationExpression? Location => location;

		public IValueExpression? Expression => expression;

		public IInlineContent? InlineContent => inlineContent;

		public string? Type => "assign-type";

		public string? Attribute => "assign-attribute";
	}

	private sealed class CancelSource(string sendId, IValueExpression sendIdExpression) : ICancel
	{
		public string? SendId => sendId;

		public IValueExpression? SendIdExpression => sendIdExpression;
	}

	private sealed class CustomActionSource(ILocationExpression location, IValueExpression value) : ICustomAction
	{
		public string? XmlNamespace => "urn:test";

		public string? XmlName => "custom";

		public string? Xml => "<custom />";

		public ImmutableArray<ILocationExpression> Locations => ImmutableArray.Create(location);

		public ImmutableArray<IValueExpression> Values => ImmutableArray.Create(value);
	}

	private sealed class IfSource(IConditionExpression condition, ImmutableArray<IExecutableEntity> action) : IIf
	{
		public IConditionExpression? Condition => condition;

		public ImmutableArray<IExecutableEntity> Action => action;
	}

	private sealed class LogSource(string label, IValueExpression expression) : ILog
	{
		public string? Label => label;

		public IValueExpression? Expression => expression;
	}

	private sealed class RaiseSource(IOutgoingEvent outgoingEvent) : IRaise
	{
		public IOutgoingEvent? OutgoingEvent => outgoingEvent;
	}

	private sealed class SendSource(IContent content,
									IValueExpression expression,
									ILocationExpression location,
									IParam param) : ISend
	{
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
	}
}
