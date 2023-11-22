#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using System;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel.Runtime;

public class RuntimeValueEvaluator : IValueExpression, IObjectEvaluator
{
	public required RuntimeValue Value { private get; init; }

	public required Func<ValueTask<RuntimeExecutionContext>> RuntimeExecutionContextFactory { private get; init; }

	public async ValueTask<IObject> EvaluateObject()
	{
		var executionContext = await RuntimeExecutionContextFactory().ConfigureAwait(false);

		Xtate.Runtime.SetCurrentExecutionContext(executionContext);

		return await Value.Evaluate().ConfigureAwait(false);
	}

	public string? Expression => Value.Expression;
}

public abstract class RuntimeValue : IValueExpression
{
	public static RuntimeValue GetValue(DataModelValue value) => new ConstantValue(value);

	public static RuntimeValue GetValue(Func<DataModelValue> evaluator)
	{
		Infra.Requires(evaluator);

		return new EvaluatorSync(evaluator);
	}

	public static RuntimeValue GetValue(Func<ValueTask<DataModelValue>> evaluator)
	{
		Infra.Requires(evaluator);

		return new EvaluatorAsync(evaluator);
	}

	public abstract ValueTask<DataModelValue> Evaluate();

#region Interface IValueExpression

	public string? Expression => null;

#endregion

	private sealed class ConstantValue : RuntimeValue
	{
		private readonly DataModelValue _value;

		public ConstantValue(DataModelValue value) => _value = value;

		public override ValueTask<DataModelValue> Evaluate() => new(_value);
	}

	private sealed class EvaluatorSync : RuntimeValue
	{
		private readonly Func<DataModelValue> _evaluator;

		public EvaluatorSync(Func<DataModelValue> evaluator) => _evaluator = evaluator;

		public override ValueTask<DataModelValue> Evaluate() => new(_evaluator());
	}

	private sealed class EvaluatorAsync : RuntimeValue
	{
		private readonly Func<ValueTask<DataModelValue>> _evaluator;

		public EvaluatorAsync(Func<ValueTask<DataModelValue>> evaluator) => _evaluator = evaluator;

		public override ValueTask<DataModelValue> Evaluate() => _evaluator();
	}
}