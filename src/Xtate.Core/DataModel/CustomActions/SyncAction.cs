// Copyright © 2019-2024 Sergii Artemenko
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

using Xtate.DataModel;

namespace Xtate.CustomAction;

public abstract class SyncAction : ActionBase, IAction
{
	private Location[]? _locations;
	private Value[]?    _values;

	private Value[] Values => _values ??= GetValues().ToArray();

	private Location[] Locations => _locations ??= GetLocations().ToArray();

#region Interface IAction

	ValueTask IAction.Execute() => Execute();

	IEnumerable<IActionValue> IAction.GetValues() => Values;

	IEnumerable<IActionLocation> IAction.GetLocations() => Locations;

#endregion

	protected virtual IEnumerable<Value> GetValues() => [];

	protected virtual IEnumerable<Location> GetLocations() => [];

	protected virtual async ValueTask Execute()
	{
		foreach (var value in Values)
		{
			await value.Evaluate().ConfigureAwait(false);
		}

		var result = Evaluate();

		if (Locations.Length > 0)
		{
			await Locations[0].SetValue(result).ConfigureAwait(false);
		}

		foreach (var value in Values)
		{
			value.Reset();
		}
	}

	protected abstract DataModelValue Evaluate();

	protected abstract class Value(string? expression) : IActionValue
	{
		protected IValueEvaluator ValueEvaluator { get; private set; } = default!;

	#region Interface IActionValue

		void IActionValue.SetEvaluator(IValueEvaluator valueEvaluator) => ValueEvaluator ??= valueEvaluator;

	#endregion

	#region Interface IValueExpression

		string? IValueExpression.Expression => expression;

	#endregion

		internal abstract ValueTask Evaluate();

		internal abstract void Reset();
	}

	protected class Location(string? expression) : IActionLocation
	{
		protected ILocationEvaluator? LocationEvaluator;

	#region Interface IActionLocation

		void IActionLocation.SetEvaluator(ILocationEvaluator locationEvaluator) => LocationEvaluator ??= locationEvaluator;

	#endregion

	#region Interface ILocationExpression

		string? ILocationExpression.Expression => expression;

	#endregion

		internal ValueTask SetValue(DataModelValue value) => LocationEvaluator?.SetValue(value.AsIObject()) ?? default;

		internal async ValueTask<DataModelValue> GetValue()
		{
			var obj = LocationEvaluator is not null ? await LocationEvaluator.GetValue().ConfigureAwait(false) : null;

			return DataModelValue.FromObject(obj);
		}
	}

	protected class ArrayValue(string? expression) : TypedValue<object?[]>(expression)
	{
		protected override ValueTask<object?[]> GetValue() => GetArray(ValueEvaluator);
	}

	protected class StringValue(string? expression, string? defaultValue = default) : TypedValue<string>(expression)
	{
		protected override ValueTask<string> GetValue() => GetString(ValueEvaluator, defaultValue);
	}

	protected class IntegerValue(string? expression, int? defaultValue = default) : TypedValue<int>(expression)
	{
		protected override ValueTask<int> GetValue() => GetInteger(ValueEvaluator, defaultValue);
	}

	protected class BooleanValue(string? expression, bool? defaultValue = default) : TypedValue<bool>(expression)
	{
		protected override ValueTask<bool> GetValue() => GetBoolean(ValueEvaluator, defaultValue);
	}

	protected class ObjectValue(string? expression, object? defaultValue = default) : TypedValue<DataModelValue>(expression)
	{
		protected override ValueTask<DataModelValue> GetValue() => GetObject(ValueEvaluator, defaultValue);
	}

	protected abstract class TypedValue<T>(string? expression) : Value(expression)
	{
		private ValueTuple<T>? _value;

		// ReSharper disable once MemberHidesStaticFromOuterClass
		public T Value => _value.HasValue ? _value.Value.Item1 : throw new InvalidOperationException(Resources.Exception_PropertyAvailableInEvaluateMethod);

		internal override void Reset() => _value = default;

		internal override async ValueTask Evaluate() => _value = new ValueTuple<T>(await GetValue().ConfigureAwait(false));

		protected abstract ValueTask<T> GetValue();
	}
}