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
using Xtate.Interpreter;
using Xtate.Interpreter.Model;
using Xtate.StateMachine;
using Xtate.StateMachine.Internal;

namespace Xtate.Test.UnitTests.Interpreter;

[TestClass]
public class InterpreterNodeCoverageTest
{
	[TestMethod]
	public async Task ExecutableNodesForwardSourcePropertiesAndDelegateExecution()
	{
		var documentIds = new LinkedList<int>();
		var expression = new ValueExpressionSource("expression");
		var location = new LocationExpressionSource("location");
		var inlineContent = new InlineContentSource("inline");
		var condition = new ConditionExpressionSource("condition");
		var outgoingEvent = new OutgoingEventSource();
		var executable = new ExecutableEntitySource();
		var content = new ContentSource(expression);
		var param = new ParamSource("param", expression, location);

		await AssertAssignNode(new AssignSource(location, expression, inlineContent), documentIds, location, expression, inlineContent);
		await AssertCancelNode(new CancelSource("send-id", expression), documentIds, expression);
		await AssertForEachNode(new ForEachSource(expression, location, location, executable), documentIds, expression, location, executable);
		await AssertIfNode(new IfSource(condition, executable), documentIds, condition, executable);
		await AssertLogNode(new LogSource("label", expression), documentIds, expression);
		await AssertRaiseNode(new RaiseSource(outgoingEvent), documentIds, outgoingEvent);
		await AssertSendNode(new SendSource(content, expression, location, param), documentIds, content, expression, location, param);
		await AssertElseNode(new ElseSource(), documentIds);
		AssertElseIfNode(new ElseIfSource(condition), documentIds, condition);
	}

	[TestMethod]
	public async Task DataParamDoneDataAndEntryExitNodesExposeCompiledEvaluators()
	{
		var documentIds = new LinkedList<int>();
		var expression = new ValueExpressionSource("expression value");
		var source = new ExternalDataExpressionSource("source value");
		var inlineContent = new InlineContentSource("inline value");
		var dataSource = new DataSource("data-id", source, expression, inlineContent);
		var dataNode = new DataNode(new DocumentIdNode(documentIds), dataSource);

		Assert.AreSame(dataSource, ((IAncestorProvider) dataNode).Ancestor);
		Assert.AreEqual("data-id", dataNode.Id);
		Assert.AreSame(source, dataNode.Source);
		Assert.AreSame(expression, dataNode.Expression);
		Assert.AreSame(inlineContent, dataNode.InlineContent);
		Assert.AreSame(source, dataNode.SourceEvaluator);
		Assert.AreSame(expression, dataNode.ExpressionEvaluator);
		Assert.AreSame(inlineContent, dataNode.InlineContentEvaluator);
		AssertDebugIdContainsDocumentId(dataNode);

		var dataModelSource = new DataModelSource(ImmutableArray.Create<IData>(dataNode));
		var dataModelNode = new DataModelNode(new DocumentIdNode(documentIds), dataModelSource);

		Assert.AreSame(dataModelSource, ((IAncestorProvider) dataModelNode).Ancestor);
		Assert.AreSame(dataNode, dataModelNode.Data.Single());
		Assert.AreSame(dataNode, ((IDataModel) dataModelNode).Data.Single());
		Assert.AreEqual(-1, dataModelNode.DocumentId);

		var param = new ParamSource("param", expression, new LocationExpressionSource("location"));
		var paramNode = new ParamNode(new DocumentIdNode(documentIds), param);

		Assert.AreSame(param, ((IAncestorProvider) paramNode).Ancestor);
		Assert.AreEqual("param", paramNode.Name);
		Assert.AreSame(expression, paramNode.Expression);
		Assert.AreSame(param.Location, paramNode.Location);
		AssertDebugIdContainsDocumentId(paramNode);

		var doneDataSource = new DoneDataSource(new ContentSource(expression), ImmutableArray.Create<IParam>(param));
		var doneDataNode = new DoneDataNode(new DocumentIdNode(documentIds), doneDataSource)
						   {
							   DataConverterFactory = () => new ValueTask<DataConverter>(new DataConverter(caseSensitivity: null))
						   };

		Assert.AreSame(doneDataSource, ((IAncestorProvider) doneDataNode).Ancestor);
		Assert.AreSame(doneDataSource.Content, doneDataNode.Content);
		Assert.AreSame(param, doneDataNode.Parameters.Single());
		Assert.AreEqual("expression value", (await doneDataNode.Evaluate()).AsList()["param"].AsString());
		AssertDebugIdContainsDocumentId(doneDataNode);

		var action = new ExecutableEntitySource();
		var onEntrySource = new OnEntrySource(action);
		var onExitSource = new OnExitSource(action);
		var onEntryNode = new OnEntryNode(new DocumentIdNode(documentIds), onEntrySource);
		var onExitNode = new OnExitNode(new DocumentIdNode(documentIds), onExitSource);

		Assert.AreSame(onEntrySource, ((IAncestorProvider) onEntryNode).Ancestor);
		Assert.AreSame(action, onEntryNode.Action.Single());
		Assert.AreSame(action, onEntryNode.ActionEvaluators.Single());
		AssertDebugIdContainsDocumentId(onEntryNode);

		Assert.AreSame(onExitSource, ((IAncestorProvider) onExitNode).Ancestor);
		Assert.AreSame(action, onExitNode.Action.Single());
		Assert.AreSame(action, onExitNode.ActionEvaluators.Single());
		AssertDebugIdContainsDocumentId(onExitNode);
	}

	private static async Task AssertAssignNode(AssignSource source,
											  LinkedList<int> documentIds,
											  ILocationExpression location,
											  IValueExpression expression,
											  IInlineContent inlineContent)
	{
		var node = new AssignNode(new DocumentIdNode(documentIds), source);

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreSame(location, node.Location);
		Assert.AreSame(expression, node.Expression);
		Assert.AreSame(inlineContent, node.InlineContent);
		Assert.AreEqual("assign-type", node.Type);
		Assert.AreEqual("assign-attribute", node.Attribute);
		AssertDebugIdContainsDocumentId(node);

		await node.Execute();

		Assert.AreEqual(1, source.ExecuteCount);
	}

	private static async Task AssertCancelNode(CancelSource source, LinkedList<int> documentIds, IValueExpression expression)
	{
		var node = new CancelNode(new DocumentIdNode(documentIds), source);

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreEqual("send-id", node.SendId);
		Assert.AreSame(expression, node.SendIdExpression);
		AssertDebugIdContainsDocumentId(node);

		await node.Execute();

		Assert.AreEqual(1, source.ExecuteCount);
	}

	private static async Task AssertForEachNode(ForEachSource source,
											   LinkedList<int> documentIds,
											   IValueExpression expression,
											   ILocationExpression location,
											   IExecutableEntity executable)
	{
		var node = new ForEachNode(new DocumentIdNode(documentIds), source);

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreSame(expression, node.Array);
		Assert.AreSame(location, node.Item);
		Assert.AreSame(location, node.Index);
		Assert.AreSame(executable, node.Action.Single());
		AssertDebugIdContainsDocumentId(node);

		await node.Execute();

		Assert.AreEqual(1, source.ExecuteCount);
	}

	private static async Task AssertIfNode(IfSource source,
										  LinkedList<int> documentIds,
										  IConditionExpression condition,
										  IExecutableEntity executable)
	{
		var node = new IfNode(new DocumentIdNode(documentIds), source);

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreSame(condition, node.Condition);
		Assert.AreSame(executable, node.Action.Single());
		AssertDebugIdContainsDocumentId(node);

		await node.Execute();

		Assert.AreEqual(1, source.ExecuteCount);
	}

	private static async Task AssertLogNode(LogSource source, LinkedList<int> documentIds, IValueExpression expression)
	{
		var node = new LogNode(new DocumentIdNode(documentIds), source);

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreEqual("label", node.Label);
		Assert.AreSame(expression, node.Expression);
		AssertDebugIdContainsDocumentId(node);

		await node.Execute();

		Assert.AreEqual(1, source.ExecuteCount);
	}

	private static async Task AssertRaiseNode(RaiseSource source, LinkedList<int> documentIds, IOutgoingEvent outgoingEvent)
	{
		var node = new RaiseNode(new DocumentIdNode(documentIds), source);

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreSame(outgoingEvent, node.OutgoingEvent);
		AssertDebugIdContainsDocumentId(node);

		await node.Execute();

		Assert.AreEqual(1, source.ExecuteCount);
	}

	private static async Task AssertSendNode(SendSource source,
											LinkedList<int> documentIds,
											IContent content,
											IValueExpression expression,
											ILocationExpression location,
											IParam param)
	{
		var node = new SendNode(new DocumentIdNode(documentIds), source);

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreEqual("event-name", node.EventName);
		Assert.AreSame(expression, node.EventExpression);
		Assert.AreEqual(new FullUri("https://target.test/"), node.Target);
		Assert.AreSame(expression, node.TargetExpression);
		Assert.AreEqual(new FullUri("https://type.test/"), node.Type);
		Assert.AreSame(expression, node.TypeExpression);
		Assert.AreEqual("send-node-id", node.Id);
		Assert.AreSame(location, node.IdLocation);
		Assert.AreEqual(10, node.DelayMs);
		Assert.AreSame(expression, node.DelayExpression);
		Assert.AreSame(location, node.NameList.Single());
		Assert.AreSame(param, node.Parameters.Single());
		Assert.AreSame(content, node.Content);
		AssertDebugIdContainsDocumentId(node);

		await node.Execute();

		Assert.AreEqual(1, source.ExecuteCount);
	}

	private static async Task AssertElseNode(ElseSource source, LinkedList<int> documentIds)
	{
		var node = new ElseNode(new DocumentIdNode(documentIds), source);

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		AssertDebugIdContainsDocumentId(node);

		await source.Execute();

		Assert.AreEqual(1, source.ExecuteCount);
	}

	private static void AssertElseIfNode(ElseIfSource source, LinkedList<int> documentIds, IConditionExpression condition)
	{
		var node = new ElseIfNode(new DocumentIdNode(documentIds), source);

		Assert.AreSame(source, ((IAncestorProvider) node).Ancestor);
		Assert.AreSame(condition, node.Condition);
		AssertDebugIdContainsDocumentId(node);
	}

	private static void AssertDebugIdContainsDocumentId(IDebugEntityId node)
	{
		var text = FormattableString.Invariant(node.EntityId);

		Assert.IsTrue(text.Contains("#", StringComparison.Ordinal), text);
		Assert.IsTrue(text.Contains(")", StringComparison.Ordinal), text);
	}

	private abstract class ExecutableSourceBase : IExecEvaluator
	{
		public int ExecuteCount { get; private set; }

		public ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}
	}

	private sealed class ValueExpressionSource(string value) : IValueExpression, IObjectEvaluator
	{
		public string? Expression => value;

		public ValueTask<DataModelValue> Evaluate() => new(value);

		public ValueTask<IObject> EvaluateObject() => new(new DataModelValue(value));
	}

	private sealed class LocationExpressionSource(string value) : ILocationExpression, ILocationEvaluator
	{
		public string? Expression => value;

		public ValueTask SetValue(IObject value) => ValueTask.CompletedTask;

		public ValueTask<IObject> GetValue() => new(new DataModelValue(value));

		public ValueTask<string> GetName() => new(value);
	}

	private sealed class InlineContentSource(string value) : IInlineContent, IObjectEvaluator
	{
		public string? Value => value;

		public ValueTask<DataModelValue> Evaluate() => new(value);

		public ValueTask<IObject> EvaluateObject() => new(new DataModelValue(value));
	}

	private sealed class ExternalDataExpressionSource(string value) : IExternalDataExpression, IObjectEvaluator
	{
		public Uri? Uri => new("https://data.test/");

		public ValueTask<DataModelValue> Evaluate() => new(value);

		public ValueTask<IObject> EvaluateObject() => new(new DataModelValue(value));
	}

	private sealed class ConditionExpressionSource(string value) : IConditionExpression
	{
		public string? Expression => value;
	}

	private sealed class ExecutableEntitySource : ExecutableSourceBase, IExecutableEntity;

	private sealed class OutgoingEventSource : IOutgoingEvent
	{
		public SendId? SendId => SendId.FromString("send-id");

		public EventName Name => (EventName) "outgoing";

		public FullUri? Target => new("https://target.test/");

		public FullUri? Type => new("https://type.test/");

		public int DelayMs => 10;

		public DataModelValue Data => "event-data";
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
									  IInlineContent inlineContent) : ExecutableSourceBase, IAssign
	{
		public ILocationExpression? Location => location;

		public IValueExpression? Expression => expression;

		public IInlineContent? InlineContent => inlineContent;

		public string? Type => "assign-type";

		public string? Attribute => "assign-attribute";
	}

	private sealed class CancelSource(string sendId, IValueExpression expression) : ExecutableSourceBase, ICancel
	{
		public string? SendId => sendId;

		public IValueExpression? SendIdExpression => expression;
	}

	private sealed class ForEachSource(IValueExpression array,
									   ILocationExpression item,
									   ILocationExpression index,
									   IExecutableEntity executable) : ExecutableSourceBase, IForEach
	{
		public IValueExpression? Array => array;

		public ILocationExpression? Item => item;

		public ILocationExpression? Index => index;

		public ImmutableArray<IExecutableEntity> Action => ImmutableArray.Create(executable);
	}

	private sealed class IfSource(IConditionExpression condition, IExecutableEntity executable) : ExecutableSourceBase, IIf
	{
		public IConditionExpression? Condition => condition;

		public ImmutableArray<IExecutableEntity> Action => ImmutableArray.Create(executable);
	}

	private sealed class LogSource(string label, IValueExpression expression) : ExecutableSourceBase, ILog
	{
		public string? Label => label;

		public IValueExpression? Expression => expression;
	}

	private sealed class RaiseSource(IOutgoingEvent outgoingEvent) : ExecutableSourceBase, IRaise
	{
		public IOutgoingEvent? OutgoingEvent => outgoingEvent;
	}

	private sealed class SendSource(IContent content,
									IValueExpression expression,
									ILocationExpression location,
									IParam param) : ExecutableSourceBase, ISend
	{
		public string? EventName => "event-name";

		public IValueExpression? EventExpression => expression;

		public FullUri? Target => new("https://target.test/");

		public IValueExpression? TargetExpression => expression;

		public FullUri? Type => new("https://type.test/");

		public IValueExpression? TypeExpression => expression;

		public string? Id => "send-node-id";

		public ILocationExpression? IdLocation => location;

		public int? DelayMs => 10;

		public IValueExpression? DelayExpression => expression;

		public ImmutableArray<ILocationExpression> NameList => ImmutableArray.Create(location);

		public ImmutableArray<IParam> Parameters => ImmutableArray.Create(param);

		public IContent? Content => content;
	}

	private sealed class ElseSource : ExecutableSourceBase, IElse;

	private sealed class ElseIfSource(IConditionExpression condition) : ExecutableSourceBase, IElseIf
	{
		public IConditionExpression? Condition => condition;
	}

	private sealed class DataSource(string id,
									IExternalDataExpression source,
									IValueExpression expression,
									IInlineContent inlineContent) : IData
	{
		public string? Id => id;

		public IExternalDataExpression? Source => source;

		public IValueExpression? Expression => expression;

		public IInlineContent? InlineContent => inlineContent;
	}

	private sealed class DataModelSource(ImmutableArray<IData> data) : IDataModel
	{
		public ImmutableArray<IData> Data => data;
	}

	private sealed class DoneDataSource(IContent content, ImmutableArray<IParam> parameters) : IDoneData
	{
		public IContent? Content => content;

		public ImmutableArray<IParam> Parameters => parameters;
	}

	private sealed class OnEntrySource(IExecutableEntity executable) : IOnEntry
	{
		public ImmutableArray<IExecutableEntity> Action => ImmutableArray.Create(executable);
	}

	private sealed class OnExitSource(IExecutableEntity executable) : IOnExit
	{
		public ImmutableArray<IExecutableEntity> Action => ImmutableArray.Create(executable);
	}
}
