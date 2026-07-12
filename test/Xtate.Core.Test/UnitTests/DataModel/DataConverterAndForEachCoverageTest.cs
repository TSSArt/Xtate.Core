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

using System.IO;
using System.Text;
using Xtate.Ancestor;
using Xtate.DataModel;
using Xtate.DataModel.Services;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.ResourceLoaders;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class DataConverterAndForEachCoverageTest
{
	[TestMethod]
	public async Task DataConverterPrefersExpressionContentThenObjectBodyThenStringBody()
	{
		var converter = new DataConverter(caseSensitivity: null);
		var body = new ValueExpressionSource
				   {
					   StringValue = "body string",
					   ObjectValue = new DataModelValue("body object")
				   };

		var expressionResult = await converter.GetContent(body, new ValueExpressionSource { ObjectValue = new DataModelValue("expression object") });
		var objectBodyResult = await converter.GetContent(body, contentExpressionEvaluator: null);
		var stringBodyResult = await converter.GetContent(new StringOnlyEvaluator("plain text"), contentExpressionEvaluator: null);
		var undefinedResult = await converter.GetContent(contentBodyEvaluator: null, contentExpressionEvaluator: null);

		Assert.AreEqual("expression object", expressionResult.AsString());
		Assert.AreEqual("body object", objectBodyResult.AsString());
		Assert.AreEqual("plain text", stringBodyResult.AsString());
		Assert.IsTrue(undefinedResult.IsUndefined());
	}

	[TestMethod]
	public async Task DataConverterBuildsParameterObjectFromNameEvaluatorsAndParamAncestors()
	{
		var converter = new DataConverter(new CaseSensitivitySource(caseInsensitive: true));
		var nameEvaluator = new LocationExpressionSource("DirectName", new DataModelValue("direct value"));
		var expression = new ValueExpressionSource { ObjectValue = new DataModelValue("expression value") };
		var location = new LocationExpressionSource("location-source", new DataModelValue("location value"));
		var parameters = DataConverter.AsParamArray(
			ImmutableArray.Create<IParam>(
				new ParamSource("ExpressionParam", expression, location: null),
				new ParamSource("LocationParam", expression: null, location)));

		var value = await converter.GetData(
			contentBodyEvaluator: null,
			contentExpressionEvaluator: null,
			ImmutableArray.Create<ILocationEvaluator>(nameEvaluator),
			parameters);
		var list = value.AsList();

		Assert.IsTrue(list.CaseInsensitive);
		Assert.AreEqual("direct value", list["directname"].AsString());
		Assert.AreEqual("expression value", list["expressionparam"].AsString());
		Assert.AreEqual("location value", list["locationparam"].AsString());
		Assert.IsTrue(DataConverter.AsParamArray(default).IsDefault);
	}

	[TestMethod]
	public async Task DataConverterConvertsResourceEventAndExceptionObjects()
	{
		var converter = new DataConverter(new CaseSensitivitySource(caseInsensitive: true));
		await using var resource = new Resource(new MemoryStream(Encoding.UTF8.GetBytes("resource text")), contentType: null);

		Assert.AreEqual("resource text", (await converter.FromContent(resource)).AsString());

		var eventData = new DataModelList { { "payload", "value" } };
		var eventValue = converter.FromEvent(new IncomingEvent
											 {
												 Name = (EventName) "event.name",
												 Type = EventType.External,
												 SendId = SendId.FromString("send-1"),
												 Origin = new FullUri("https://origin.test/"),
												 OriginType = new FullUri("https://type.test/"),
												 InvokeId = InvokeId.FromString("invoke-1"),
												 Data = eventData
											 });
		var eventList = eventValue.AsList();

		Assert.AreEqual("event.name", eventList["NAME"].AsString());
		Assert.AreEqual("external", eventList["type"].AsString());
		Assert.AreEqual("send-1", eventList["sendid"].AsString());
		Assert.AreEqual("https://origin.test/", eventList["origin"].AsString());
		Assert.AreEqual("https://type.test/", eventList["origintype"].AsString());
		Assert.AreEqual("invoke-1", eventList["invokeid"].AsString());
		Assert.AreEqual("value", eventList["data"].AsList()["payload"].AsString());
		Assert.AreEqual("platform", converter.FromEvent(new IncomingEvent { Name = (EventName) "platform", Type = EventType.Platform }).AsList()["type"].AsString());
		Assert.AreEqual("internal", converter.FromEvent(new IncomingEvent { Name = (EventName) "internal", Type = EventType.Internal }).AsList()["type"].AsString());

		var exceptionList = converter.FromException(new InvalidOperationException("boom")).AsList();

		Assert.AreEqual("boom", exceptionList["MESSAGE"].AsString());
		Assert.AreEqual(nameof(InvalidOperationException), exceptionList["typename"].AsString());
		Assert.IsTrue(exceptionList.ContainsKey("TYPEFULLNAME"));
		Assert.IsTrue(exceptionList.ContainsKey("text"));
	}

	[TestMethod]
	public async Task DefaultForEachEvaluatorSetsItemIndexAndRunsActionsForEachArrayEntry()
	{
		var arrayExpression = new ValueExpressionSource
							  {
								  ArrayValue = Enumerable.Range(0, 257)
														 .Select(static index => (IObject) new DataModelValue($"item-{index}"))
														 .ToArray()
							  };
		var itemLocation = new LocationExpressionSource("item", DataModelValue.Undefined);
		var indexLocation = new LocationExpressionSource("index", DataModelValue.Undefined);
		var action = new ExecutableEntitySource();
		var forEach = new ForEachSource(arrayExpression, itemLocation, indexLocation, ImmutableArray.Create<IExecutableEntity>(action));
		var evaluator = new DefaultForEachEvaluator(forEach);

		Assert.AreSame(forEach, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreSame(arrayExpression, evaluator.Array);
		Assert.AreSame(itemLocation, evaluator.Item);
		Assert.AreSame(indexLocation, evaluator.Index);
		Assert.AreSame(action, evaluator.Action.Single());

		await evaluator.Execute();

		Assert.AreEqual(257, action.ExecuteCount);
		Assert.AreEqual(257, itemLocation.SetValues.Count);
		Assert.AreEqual(257, indexLocation.SetValues.Count);
		Assert.AreEqual("item-0", DataModelValue.FromObject(itemLocation.SetValues[0].ToObject()).AsString());
		Assert.AreEqual("item-256", DataModelValue.FromObject(itemLocation.SetValues[256].ToObject()).AsString());
		Assert.AreEqual(0, indexLocation.SetValues[0].ToObject());
		Assert.AreEqual(255, indexLocation.SetValues[255].ToObject());
		Assert.AreEqual(256, indexLocation.SetValues[256].ToObject());
	}

	[TestMethod]
	public async Task DefaultForEachEvaluatorAllowsMissingIndexAndEmptyActions()
	{
		var arrayExpression = new ValueExpressionSource { ArrayValue = [new DataModelValue("only item")] };
		var itemLocation = new LocationExpressionSource("item", DataModelValue.Undefined);
		var forEach = new ForEachSource(arrayExpression, itemLocation, index: null, action: []);
		var evaluator = new DefaultForEachEvaluator(forEach);

		await evaluator.Execute();

		Assert.IsNull(evaluator.Index);
		Assert.AreEqual(1, itemLocation.SetValues.Count);
		Assert.AreEqual("only item", DataModelValue.FromObject(itemLocation.SetValues.Single().ToObject()).AsString());
	}

	private sealed class CaseSensitivitySource(bool caseInsensitive) : ICaseSensitivity
	{
		public bool CaseInsensitive => caseInsensitive;
	}

	private sealed class ValueExpressionSource : IValueExpression, IValueEvaluator, IObjectEvaluator, IArrayEvaluator, IStringEvaluator
	{
		public string? Expression { get; init; }

		public DataModelValue ObjectValue { get; init; } = DataModelValue.Undefined;

		public IObject[] ArrayValue { get; init; } = [];

		public string StringValue { get; init; } = string.Empty;

		public ValueTask<DataModelValue> Evaluate() => new(ObjectValue);

		public ValueTask<IObject> EvaluateObject() => new(ObjectValue);

		public ValueTask<IObject[]> EvaluateArray() => new(ArrayValue);

		public ValueTask<string> EvaluateString() => new(StringValue);
	}

	private sealed class StringOnlyEvaluator(string value) : IValueEvaluator, IStringEvaluator
	{
		public ValueTask<DataModelValue> Evaluate() => new(value);

		public ValueTask<string> EvaluateString() => new(value);
	}

	private sealed class LocationExpressionSource(string name, IObject value) : ILocationExpression, ILocationEvaluator
	{
		private IObject _value = value;

		public List<IObject> SetValues { get; } = [];

		public string? Expression => name;

		public ValueTask SetValue(IObject value)
		{
			_value = value;
			SetValues.Add(value);

			return ValueTask.CompletedTask;
		}

		public ValueTask<IObject> GetValue() => new(_value);

		public ValueTask<string> GetName() => new(name);
	}

	private sealed class ExecutableEntitySource : IExecutableEntity, IExecEvaluator
	{
		public int ExecuteCount { get; private set; }

		public ValueTask Execute()
		{
			ExecuteCount ++;

			return ValueTask.CompletedTask;
		}
	}

	private sealed class ParamSource(string name, IValueExpression? expression, ILocationExpression? location) : IParam
	{
		public string Name => name;

		public IValueExpression? Expression => expression;

		public ILocationExpression? Location => location;
	}

	private sealed class ForEachSource(IValueExpression array,
									   ILocationExpression item,
									   ILocationExpression? index,
									   ImmutableArray<IExecutableEntity> action) : IForEach
	{
		public IValueExpression? Array => array;

		public ILocationExpression? Item => item;

		public ILocationExpression? Index => index;

		public ImmutableArray<IExecutableEntity> Action => action;
	}
}
