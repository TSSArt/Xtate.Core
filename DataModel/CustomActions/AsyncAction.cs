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

public abstract class AsyncAction : ActionBase, IAction
{
#region Interface IAction

	ValueTask IAction.Execute() => Execute();

	IEnumerable<IActionValue> IAction.GetValues() => GetValues();

	IEnumerable<IActionLocation> IAction.GetLocations() => GetLocations();

#endregion

	protected virtual IEnumerable<Value> GetValues() => [];

	protected virtual IEnumerable<Location> GetLocations() => [];

	protected abstract ValueTask Execute();

	protected abstract class Value(string? expression) : IActionValue
	{
		protected IValueEvaluator ValueEvaluator { get; private set; } = default!;

	#region Interface IActionValue

		void IActionValue.SetEvaluator(IValueEvaluator valueEvaluator) => ValueEvaluator ??= valueEvaluator;

	#endregion

	#region Interface IValueExpression

		string? IValueExpression.Expression => expression;

	#endregion
	}

	protected class Location(string? expression) : IActionLocation
	{
		private ILocationEvaluator? _locationEvaluator;

	#region Interface IActionLocation

		void IActionLocation.SetEvaluator(ILocationEvaluator locationEvaluator) => _locationEvaluator ??= locationEvaluator;

	#endregion

	#region Interface ILocationExpression

		string? ILocationExpression.Expression => expression;

	#endregion

		public virtual ValueTask SetValue(DataModelValue value) => _locationEvaluator?.SetValue(value.AsIObject()) ?? default;

		public virtual async ValueTask<DataModelValue> GetValue()
		{
			var obj = _locationEvaluator is not null ? await _locationEvaluator.GetValue().ConfigureAwait(false) : null;

			return DataModelValue.FromObject(obj);
		}
	}

	protected class ArrayValue(string? expression) : Value(expression)
	{
		public ValueTask<object?[]> GetValue() => GetArray(ValueEvaluator);
	}

	protected class StringValue(string? expression, string? defaultValue = default) : Value(expression)
	{
		public ValueTask<string> GetValue() => GetString(ValueEvaluator, defaultValue);
	}

	protected class IntegerValue(string? expression, int? defaultValue = default) : Value(expression)
	{
		public ValueTask<int> GetValue() => GetInteger(ValueEvaluator, defaultValue);
	}

	protected class BooleanValue(string? expression, bool? defaultValue = default) : Value(expression)
	{
		public ValueTask<bool> GetValue() => GetBoolean(ValueEvaluator, defaultValue);
	}

	protected class ObjectValue(string? expression, object? defaultValue = default) : Value(expression)
	{
		public ValueTask<DataModelValue> GetValue() => GetObject(ValueEvaluator, defaultValue);
	}
}