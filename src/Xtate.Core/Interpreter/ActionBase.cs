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

public abstract class ActionBase
{
	protected static async ValueTask<object?[]> GetArray(IValueEvaluator valueEvaluator)
	{
		if (valueEvaluator.Is<IArrayEvaluator>(out var arrayEvaluator))
		{
			var array = await arrayEvaluator.EvaluateArray().ConfigureAwait(false);

			return array is not null ? Array.ConvertAll(array, i => i.ToObject()) : [];
		}

		if (valueEvaluator.Is<IObjectEvaluator>(out var objectEvaluator))
		{
			var obj = (await objectEvaluator.EvaluateObject().ConfigureAwait(false)).ToObject();

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

	protected static async ValueTask<string> GetString(IValueEvaluator valueEvaluator, string? defaultValue)
	{
		if (valueEvaluator.Is<IStringEvaluator>(out var stringEvaluator))
		{
			return await stringEvaluator.EvaluateString().ConfigureAwait(false);
		}

		if (valueEvaluator.Is<IObjectEvaluator>(out var objectEvaluator))
		{
			var obj = await objectEvaluator.EvaluateObject().ConfigureAwait(false);

			return Convert.ToString(obj?.ToObject()) ?? string.Empty;
		}

		return defaultValue ?? string.Empty;
	}

	protected static async ValueTask<int> GetInteger(IValueEvaluator valueEvaluator, int? defaultValue)
	{
		if (valueEvaluator.Is<IIntegerEvaluator>(out var integerEvaluator))
		{
			return await integerEvaluator.EvaluateInteger().ConfigureAwait(false);
		}

		if (valueEvaluator.Is<IObjectEvaluator>(out var objectEvaluator))
		{
			var obj = await objectEvaluator.EvaluateObject().ConfigureAwait(false);

			return Convert.ToInt32(obj?.ToObject());
		}

		return defaultValue ?? default;
	}

	protected static async ValueTask<bool> GetBoolean(IValueEvaluator valueEvaluator, bool? defaultValue = default)
	{
		if (valueEvaluator.Is<IBooleanEvaluator>(out var booleanEvaluator))
		{
			return await booleanEvaluator.EvaluateBoolean().ConfigureAwait(false);
		}

		if (valueEvaluator.Is<IObjectEvaluator>(out var objectEvaluator))
		{
			var obj = await objectEvaluator.EvaluateObject().ConfigureAwait(false);

			return Convert.ToBoolean(obj?.ToObject());
		}

		return defaultValue ?? default;
	}

	protected static async ValueTask<DataModelValue> GetObject(IValueEvaluator valueEvaluator, object? defaultValue)
	{
		var obj = valueEvaluator.Is<IObjectEvaluator>(out var objectEvaluator)
			? await objectEvaluator.EvaluateObject().ConfigureAwait(false)
			: defaultValue;

		return DataModelValue.FromObject(obj);
	}
}