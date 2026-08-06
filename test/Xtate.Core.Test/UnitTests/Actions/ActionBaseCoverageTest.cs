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

using System.Collections;
using Xtate.Actions;
using Xtate.DataModel;
using Xtate.DataTypes;

namespace Xtate.Core.Test.UnitTests.Actions;

[TestClass]
public class ActionBaseCoverageTest
{
	[TestMethod]
	public async Task GetArraySupportsArrayObjectEnumerableScalarNullAndMissingEvaluators()
	{
		CollectionAssert.AreEqual(new object?[] { 1, "two" }, await Probe.ReadArray(new ArrayEvaluator([new DataModelValue(1), new DataModelValue("two")])));
		Assert.IsEmpty(await Probe.ReadArray(new ArrayEvaluator(null!)));
		CollectionAssert.AreEqual(new object?[] { 1, "two" }, await Probe.ReadArray(new ObjectEvaluator(new object[] { 1, "two" })));
		CollectionAssert.AreEqual(new object?[] { 1, "two" }, await Probe.ReadArray(new ObjectEvaluator(new ArrayList { 1, "two" })));
		CollectionAssert.AreEqual(new object?[] { 42 }, await Probe.ReadArray(new ObjectEvaluator(42)));
		Assert.IsEmpty(await Probe.ReadArray(new ObjectEvaluator(value: null)));
		Assert.IsEmpty(await Probe.ReadArray(new PlainEvaluator()));
	}

	[TestMethod]
	public async Task ScalarHelpersPreferTypedThenObjectThenDefaultValues()
	{
		Assert.AreEqual(expected: "typed", await Probe.ReadString(new StringEvaluator("typed"), defaultValue: "default"));
		Assert.AreEqual(expected: "object", await Probe.ReadString(new ObjectEvaluator("object"), defaultValue: "default"));
		Assert.AreEqual(expected: "", await Probe.ReadString(new ObjectEvaluator(value: null), defaultValue: "default"));
		Assert.AreEqual(expected: "default", await Probe.ReadString(new PlainEvaluator(), defaultValue: "default"));
		Assert.AreEqual(expected: "", await Probe.ReadString(new PlainEvaluator(), defaultValue: null));

		Assert.AreEqual(expected: 7, await Probe.ReadInteger(new IntegerEvaluator(7), defaultValue: 9));
		Assert.AreEqual(expected: 8, await Probe.ReadInteger(new ObjectEvaluator(8), defaultValue: 9));
		Assert.AreEqual(expected: 9, await Probe.ReadInteger(new PlainEvaluator(), defaultValue: 9));
		Assert.AreEqual(expected: 0, await Probe.ReadInteger(new PlainEvaluator(), defaultValue: null));

		Assert.IsTrue(await Probe.ReadBoolean(new BooleanEvaluator(value: true), defaultValue: false));
		Assert.IsTrue(await Probe.ReadBoolean(new ObjectEvaluator(value: true), defaultValue: false));
		Assert.IsTrue(await Probe.ReadBoolean(new PlainEvaluator(), defaultValue: true));
		Assert.IsFalse(await Probe.ReadBoolean(new PlainEvaluator(), defaultValue: null));
	}

	// ReSharper disable once ClassNeverInstantiated.Local
	private sealed class Probe : ActionBase
	{
		public static ValueTask<object?[]> ReadArray(IValueEvaluator evaluator) => GetArray(evaluator);

		public static ValueTask<string> ReadString(IValueEvaluator evaluator, string? defaultValue) => GetString(evaluator, defaultValue);

		public static ValueTask<int> ReadInteger(IValueEvaluator evaluator, int? defaultValue) => GetInteger(evaluator, defaultValue);

		public static ValueTask<bool> ReadBoolean(IValueEvaluator evaluator, bool? defaultValue) => GetBoolean(evaluator, defaultValue);
	}

	private sealed class PlainEvaluator : IValueEvaluator;

	private sealed class ArrayEvaluator(IObject[] values) : IArrayEvaluator
	{
	#region Interface IArrayEvaluator

		public ValueTask<IObject[]> EvaluateArray() => new(values);

	#endregion
	}

	private sealed class ObjectEvaluator(object? value) : IObjectEvaluator
	{
	#region Interface IObjectEvaluator

		public ValueTask<IObject> EvaluateObject() => new(new ObjectValue(value));

	#endregion
	}

	private sealed class StringEvaluator(string value) : IStringEvaluator
	{
	#region Interface IStringEvaluator

		public ValueTask<string> EvaluateString() => new(value);

	#endregion
	}

	private sealed class IntegerEvaluator(int value) : IIntegerEvaluator
	{
	#region Interface IIntegerEvaluator

		public ValueTask<int> EvaluateInteger() => new(value);

	#endregion
	}

	private sealed class BooleanEvaluator(bool value) : IBooleanEvaluator
	{
	#region Interface IBooleanEvaluator

		public ValueTask<bool> EvaluateBoolean() => new(value);

	#endregion
	}

	private sealed class ObjectValue(object? value) : IObject
	{
	#region Interface IObject

		public object? ToObject() => value;

	#endregion
	}
}