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

using Xtate.Actions;
using Xtate.DataModel;
using Xtate.DataTypes;

namespace Xtate.Test.UnitTests.Actions;

[TestClass]
public class ActionValueCoverageTest
{
	[TestMethod]
	public async Task AsyncActionExposesValuesLocationsAndUsesTypedEvaluators()
	{
		var action = new TestAsyncAction();
		var values = ((IAction) action).GetValues().ToArray();
		var locations = ((IAction) action).GetLocations().ToArray();
		var locationEvaluator = new TestLocationEvaluator(new DataModelValue("initial"));

		values[0].SetEvaluator(new TestArrayEvaluator([new DataModelValue("left"), new DataModelValue(7)]));
		values[1].SetEvaluator(new TestStringEvaluator("text"));
		values[2].SetEvaluator(new TestIntegerEvaluator(42));
		values[3].SetEvaluator(new TestBooleanEvaluator(true));
		values[4].SetEvaluator(new TestObjectEvaluator(new DataModelValue("object")));
		locations[0].SetEvaluator(locationEvaluator);
		locations[0].SetEvaluator(new TestLocationEvaluator(new DataModelValue("ignored")));

		var result = await action.ReadAll();
		await ((IAction) action).Execute();

		CollectionAssert.AreEqual(new object?[] { "left", 7 }, result.Array);
		Assert.AreEqual(expected: "text", result.String);
		Assert.AreEqual(expected: 42, result.Integer);
		Assert.IsTrue(result.Boolean);
		Assert.AreEqual(expected: "object", result.Object.AsString());
		Assert.AreEqual(expected: "initial", result.LocationBefore.AsString());
		Assert.AreEqual(expected: "updated", result.LocationAfter.AsString());
		Assert.IsTrue(action.Executed);
		CollectionAssert.AreEqual(values, ((IAction) action).GetValues().ToArray());
		CollectionAssert.AreEqual(locations, ((IAction) action).GetLocations().ToArray());
		Assert.AreEqual(expected: "array", values[0].Expression);
		Assert.AreEqual(expected: "target", locations[0].Expression);
	}

	[TestMethod]
	public async Task AsyncActionValuesUseObjectAndDefaultFallbacks()
	{
		var action = new TestAsyncAction();
		var values = ((IAction) action).GetValues().ToArray();

		values[0].SetEvaluator(new TestObjectEvaluator(DataModelValue.FromObject(new[] { "a", "b" })));
		values[1].SetEvaluator(new TestObjectEvaluator(new DataModelValue("from object")));
		values[2].SetEvaluator(new TestObjectEvaluator(new DataModelValue(123)));
		values[3].SetEvaluator(new TestObjectEvaluator(new DataModelValue(true)));

		var result = await action.ReadAll();
		var defaults = await action.ReadDefaults();

		CollectionAssert.AreEqual(new[] { "a", "b" }, result.Array.Select(static item => Convert.ToString(item)).ToArray());
		Assert.AreEqual(expected: "from object", result.String);
		Assert.AreEqual(expected: 123, result.Integer);
		Assert.IsTrue(result.Boolean);
		Assert.AreEqual(expected: "default string", defaults.String);
		Assert.AreEqual(expected: 17, defaults.Integer);
		Assert.IsFalse(defaults.Boolean);
		Assert.AreEqual(expected: "default object", defaults.Object.AsString());
	}

	[TestMethod]
	public async Task SyncActionEvaluatesValuesSetsFirstLocationAndResetsTypedValues()
	{
		var action = new TestSyncAction();
		var values = ((IAction) action).GetValues().ToArray();
		var locations = ((IAction) action).GetLocations().ToArray();
		var target = new TestLocationEvaluator(DataModelValue.Null);

		values[0].SetEvaluator(new TestArrayEvaluator([new DataModelValue("left"), new DataModelValue("right")]));
		values[1].SetEvaluator(new TestBooleanEvaluator(true));
		values[2].SetEvaluator(new TestIntegerEvaluator(5));
		values[3].SetEvaluator(new TestStringEvaluator("hello"));
		locations[0].SetEvaluator(target);

		await ((IAction) action).Execute();

		Assert.AreEqual(expected: "hello:5:True:2", target.Value.AsString());
		Assert.AreEqual(expected: "hello:5:True:2", action.EvaluatedValue.AsString());
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => action.ReadStringAfterReset());
		CollectionAssert.AreEqual(values, ((IAction) action).GetValues().ToArray());
		CollectionAssert.AreEqual(locations, ((IAction) action).GetLocations().ToArray());
		Assert.AreEqual(expected: "array", values[0].Expression);
		Assert.AreEqual(expected: "target", locations[0].Expression);
	}

	private sealed class TestAsyncAction : AsyncAction
	{
		private readonly ArrayValue _array = new("array");

		private readonly BooleanValue _boolean = new(expression: "boolean", defaultValue: false);

		private readonly IntegerValue _integer = new(expression: "integer", defaultValue: 17);

		private readonly Location _location = new("target");

		private readonly ObjectValue _object = new(expression: "object", defaultValue: "default object");

		private readonly StringValue _string = new(expression: "string", defaultValue: "default string");

		public bool Executed { get; private set; }

		protected override IEnumerable<Value> GetValues() => [_array, _string, _integer, _boolean, _object];

		protected override IEnumerable<Location> GetLocations() => [_location];

		protected override ValueTask Execute()
		{
			Executed = true;

			return ValueTask.CompletedTask;
		}

		public async ValueTask<ActionValues> ReadAll()
		{
			var locationBefore = await _location.GetValue();
			await _location.SetValue(new DataModelValue("updated"));

			return new ActionValues(
				await _array.GetValue(),
				await _string.GetValue(),
				await _integer.GetValue(),
				await _boolean.GetValue(),
				await _object.GetValue(),
				locationBefore,
				await _location.GetValue());
		}

		public async ValueTask<ActionValues> ReadDefaults() =>
			new(
				await new ArrayValue(expression: null).GetValue(),
				await new StringValue(expression: null, defaultValue: "default string").GetValue(),
				await new IntegerValue(expression: null, defaultValue: 17).GetValue(),
				await new BooleanValue(expression: null, defaultValue: false).GetValue(),
				await new ObjectValue(expression: null, defaultValue: "default object").GetValue(),
				DataModelValue.Null,
				DataModelValue.Null);
	}

	private sealed class TestSyncAction : SyncAction
	{
		private readonly ArrayValue _array = new("array");

		private readonly BooleanValue _boolean = new("boolean");

		private readonly IntegerValue _integer = new("integer");

		private readonly Location _location = new("target");

		private readonly StringValue _string = new("string");

		public DataModelValue EvaluatedValue { get; private set; }

		protected override IEnumerable<Value> GetValues() => [_array, _boolean, _integer, _string];

		protected override IEnumerable<Location> GetLocations() => [_location];

		protected override DataModelValue Evaluate()
		{
			EvaluatedValue = new DataModelValue($"{_string.Value}:{_integer.Value}:{_boolean.Value}:{_array.Value.Length}");

			return EvaluatedValue;
		}

		public string ReadStringAfterReset() => _string.Value;
	}

	private sealed record ActionValues(
		object?[] Array,
		string String,
		int Integer,
		bool Boolean,
		DataModelValue Object,
		DataModelValue LocationBefore,
		DataModelValue LocationAfter);

	private sealed class TestArrayEvaluator(IObject[] value) : IArrayEvaluator
	{
	#region Interface IArrayEvaluator

		public ValueTask<IObject[]> EvaluateArray() => new(value);

	#endregion
	}

	private sealed class TestStringEvaluator(string value) : IStringEvaluator
	{
	#region Interface IStringEvaluator

		public ValueTask<string> EvaluateString() => new(value);

	#endregion
	}

	private sealed class TestIntegerEvaluator(int value) : IIntegerEvaluator
	{
	#region Interface IIntegerEvaluator

		public ValueTask<int> EvaluateInteger() => new(value);

	#endregion
	}

	private sealed class TestBooleanEvaluator(bool value) : IBooleanEvaluator
	{
	#region Interface IBooleanEvaluator

		public ValueTask<bool> EvaluateBoolean() => new(value);

	#endregion
	}

	private sealed class TestObjectEvaluator(IObject value) : IObjectEvaluator
	{
	#region Interface IObjectEvaluator

		public ValueTask<IObject> EvaluateObject() => new(value);

	#endregion
	}

	private sealed class TestLocationEvaluator(IObject value) : ILocationEvaluator
	{
		public DataModelValue Value { get; private set; } = DataModelValue.FromObject(value);

	#region Interface ILocationEvaluator

		public ValueTask SetValue(IObject value)
		{
			Value = DataModelValue.FromObject(value);

			return ValueTask.CompletedTask;
		}

		public ValueTask<IObject> GetValue() => new(Value);

		public ValueTask<string> GetName() => new("target");

	#endregion
	}
}