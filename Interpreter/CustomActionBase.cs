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

public abstract class CustomActionBase
{
	public abstract ValueTask Execute();

	public virtual IEnumerable<Value> GetValues() => [];

	public virtual IEnumerable<Location> GetLocations() => [];

	public class ArrayValue(string? expression) : Value(expression)
	{
		private IArrayEvaluator? _arrayEvaluator;

		internal override void SetEvaluator(IValueEvaluator valueEvaluator)
		{
			base.SetEvaluator(valueEvaluator);

			_arrayEvaluator = valueEvaluator as IArrayEvaluator;
		}

		public new async ValueTask<object?[]> GetValue()
		{
			var array = _arrayEvaluator is not null
				? await _arrayEvaluator.EvaluateArray().ConfigureAwait(false)
				: (IObject[]?) await base.GetValue().ConfigureAwait(false);

			return array is not null ? Array.ConvertAll(array, i => i.ToObject()) : [];
		}
	}

	public class StringValue(string? expression, string? defaultValue = default) : Value(expression, defaultValue)
	{
		private IStringEvaluator? _stringEvaluator;

		internal override void SetEvaluator(IValueEvaluator valueEvaluator)
		{
			base.SetEvaluator(valueEvaluator);

			_stringEvaluator = valueEvaluator as IStringEvaluator;
		}

		public new async ValueTask<string?> GetValue() =>
			_stringEvaluator is not null
				? await _stringEvaluator.EvaluateString().ConfigureAwait(false)
				: Convert.ToString(await base.GetValue().ConfigureAwait(false));
	}

	public class IntegerValue(string? expression, int? defaultValue = default) : Value(expression, defaultValue)
	{
		private IIntegerEvaluator? _integerEvaluator;

		internal override void SetEvaluator(IValueEvaluator valueEvaluator)
		{
			base.SetEvaluator(valueEvaluator);

			_integerEvaluator = valueEvaluator as IIntegerEvaluator;
		}

		public new async ValueTask<int> GetValue() =>
			_integerEvaluator is not null
				? await _integerEvaluator.EvaluateInteger().ConfigureAwait(false)
				: Convert.ToInt32(await base.GetValue().ConfigureAwait(false));
	}

	public class BooleanValue(string? expression, bool? defaultValue = default) : Value(expression, defaultValue)
	{
		private IBooleanEvaluator? _booleanEvaluator;

		internal override void SetEvaluator(IValueEvaluator valueEvaluator)
		{
			base.SetEvaluator(valueEvaluator);

			_booleanEvaluator = valueEvaluator as IBooleanEvaluator;
		}

		public new async ValueTask<bool> GetValue() =>
			_booleanEvaluator is not null
				? await _booleanEvaluator.EvaluateBoolean().ConfigureAwait(false)
				: Convert.ToBoolean(await base.GetValue().ConfigureAwait(false));
	}

	public class Value(string? expression, object? defaultValue = default) : IValueExpression
	{
		private readonly IObject _defaultValue = expression is null ? new DefaultObject(defaultValue) : DefaultObject.Null;
		
		private IObjectEvaluator? _objectEvaluator;

		#region Interface IValueExpression

		string? IValueExpression.Expression => expression;

	#endregion

		internal virtual void SetEvaluator(IValueEvaluator valueEvaluator) => _objectEvaluator = valueEvaluator as IObjectEvaluator;

		internal ValueTask<IObject> GetObject() => _objectEvaluator?.EvaluateObject() ?? new ValueTask<IObject>(_defaultValue);

		public async ValueTask<object?> GetValue() => (await GetObject().ConfigureAwait(false)).ToObject();
	}

	public class Location(string? expression) : ILocationExpression
	{
		private ILocationEvaluator? _locationEvaluator;

	#region Interface ILocationExpression

		string? ILocationExpression.Expression => expression;

	#endregion

		internal void SetEvaluator(ILocationEvaluator locationEvaluator) => _locationEvaluator = locationEvaluator;

		public ValueTask SetValue(object? value) => _locationEvaluator?.SetValue(new DefaultObject(value)) ?? default;

		public async ValueTask CopyFrom(Value value)
		{
			if (_locationEvaluator is not null)
			{
				var val = await value.GetObject().ConfigureAwait(false);
				await _locationEvaluator.SetValue(val).ConfigureAwait(false);
			}
		}
	}
}