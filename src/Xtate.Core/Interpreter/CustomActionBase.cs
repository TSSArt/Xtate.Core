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
	public virtual async ValueTask Execute()
	{
		foreach (var value in GetValues())
		{
			await value.Evaluate().ConfigureAwait(false);
		}

		var result = Evaluate();

		foreach (var location in GetLocations())
		{
			await location.SetValue(result).ConfigureAwait(false);
		}

		foreach (var value in GetValues())
		{
			value.Reset();
		}
	}

	public virtual DataModelValue Evaluate() => default;

	public virtual IEnumerable<Value> GetValues() => [];

	public virtual IEnumerable<Location> GetLocations() => [];

	public class ArrayValue(string? expression) : TypedValue<object?[]>(expression)
	{
		private IArrayEvaluator?  _arrayEvaluator;
		private IObjectEvaluator? _objectEvaluator;

		internal override void SetEvaluator(IValueEvaluator valueEvaluator)
		{
			valueEvaluator.Is(out _objectEvaluator);
			valueEvaluator.Is(out _arrayEvaluator);
		}

		public override async ValueTask<object?[]> GetValue()
		{
			if (_arrayEvaluator is not null)
			{
				var array = await _arrayEvaluator.EvaluateArray().ConfigureAwait(false);

				return array is not null ? Array.ConvertAll(array, i => i.ToObject()) : [];
			}

			if (_objectEvaluator is not null)
			{
				var obj = (await _objectEvaluator.EvaluateObject().ConfigureAwait(false)).ToObject();

				return obj switch
					   {
						   IEnumerable<object> e1 => e1.ToArray(),
						   IEnumerable e2         => e2.Cast<object>().ToArray(),
						   not null               => [obj],
						   _                      => []
					   };
			}

			return [];
		}
	}

	public class StringValue(string? expression, string? defaultValue = default) : TypedValue<string>(expression)
	{
		private IObjectEvaluator? _objectEvaluator;
		private IStringEvaluator? _stringEvaluator;

		internal override void SetEvaluator(IValueEvaluator valueEvaluator)
		{
			valueEvaluator.Is(out _objectEvaluator);
			valueEvaluator.Is(out _stringEvaluator);
		}

		public override async ValueTask<string> GetValue()
		{
			if (_stringEvaluator is not null)
			{
				return await _stringEvaluator.EvaluateString().ConfigureAwait(false);
			}

			if (_objectEvaluator is not null)
			{
				var obj = await _objectEvaluator.EvaluateObject().ConfigureAwait(false);

				return Convert.ToString(obj?.ToObject()) ?? string.Empty;
			}

			return defaultValue ?? string.Empty;
		}
	}

	public class IntegerValue(string? expression, int? defaultValue = default) : TypedValue<int>(expression)
	{
		private IIntegerEvaluator? _integerEvaluator;
		private IObjectEvaluator?  _objectEvaluator;

		internal override void SetEvaluator(IValueEvaluator valueEvaluator)
		{
			valueEvaluator.Is(out _objectEvaluator);
			valueEvaluator.Is(out _integerEvaluator);
		}

		public override async ValueTask<int> GetValue()
		{
			if (_integerEvaluator is not null)
			{
				return await _integerEvaluator.EvaluateInteger().ConfigureAwait(false);
			}

			if (_objectEvaluator is not null)
			{
				var obj = await _objectEvaluator.EvaluateObject().ConfigureAwait(false);

				return Convert.ToInt32(obj?.ToObject());
			}

			return defaultValue ?? default;
		}
	}

	public class BooleanValue(string? expression, bool? defaultValue = default) : TypedValue<bool>(expression)
	{
		private IBooleanEvaluator? _booleanEvaluator;
		private IObjectEvaluator?  _objectEvaluator;

		internal override void SetEvaluator(IValueEvaluator valueEvaluator)
		{
			valueEvaluator.Is(out _objectEvaluator);
			valueEvaluator.Is(out _booleanEvaluator);
		}

		public override async ValueTask<bool> GetValue()
		{
			if (_booleanEvaluator is not null)
			{
				return await _booleanEvaluator.EvaluateBoolean().ConfigureAwait(false);
			}

			if (_objectEvaluator is not null)
			{
				var obj = await _objectEvaluator.EvaluateObject().ConfigureAwait(false);

				return Convert.ToBoolean(obj?.ToObject());
			}

			return defaultValue ?? default;
		}
	}

	public class ObjectValue(string? expression, object? defaultValue = default) : TypedValue<DataModelValue>(expression)
	{
		private IObjectEvaluator? _objectEvaluator;

		internal override void SetEvaluator(IValueEvaluator valueEvaluator) => valueEvaluator.Is(out _objectEvaluator);

		public override async ValueTask<DataModelValue> GetValue()
		{
			var obj = _objectEvaluator is not null ? await _objectEvaluator.EvaluateObject().ConfigureAwait(false) : defaultValue;

			return DataModelValue.FromObject(obj);
		}
	}

	public abstract class TypedValue<T>(string? expression) : Value(expression)
	{
		private ValueTuple<T>? _value;

		public T Value => _value.HasValue ? _value.Value.Item1 : throw new InfrastructureException(Resources.Exception_PropertyAvailableInEvaluateMethod);

		internal override void Reset() => _value = default;

		internal override async ValueTask Evaluate() => _value = new ValueTuple<T>(await GetValue().ConfigureAwait(false));

		public abstract ValueTask<T> GetValue();
	}

	public abstract class Value(string? expression) : IValueExpression
	{
	#region Interface IValueExpression

		string? IValueExpression.Expression => expression;

	#endregion

		internal abstract void SetEvaluator(IValueEvaluator valueEvaluator);

		internal abstract void Reset();

		internal abstract ValueTask Evaluate();
	}

	public class Location(string? expression) : ILocationExpression
	{
		private ILocationEvaluator? _locationEvaluator;

	#region Interface ILocationExpression

		string? ILocationExpression.Expression => expression;

	#endregion

		internal void SetEvaluator(ILocationEvaluator locationEvaluator) => _locationEvaluator = locationEvaluator;

		public ValueTask SetValue(DataModelValue value) => _locationEvaluator?.SetValue(value.AsIObject()) ?? default;

		public async ValueTask<DataModelValue> GetValue()
		{
			var obj = _locationEvaluator is not null ? await _locationEvaluator.GetValue().ConfigureAwait(false) : null;

			return DataModelValue.FromObject(obj);
		}
	}
}