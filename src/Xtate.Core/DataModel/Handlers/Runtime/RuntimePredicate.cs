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

<<<<<<< Updated upstream
using System;
using System.Threading.Tasks;

namespace Xtate.DataModel.Runtime;

public class RuntimePredicateEvaluator : IConditionExpression, IBooleanEvaluator
{
	public required RuntimePredicate Predicate { private get; init; }

	public required Func<ValueTask<RuntimeExecutionContext>> RuntimeExecutionContextFactory { private get; init; }
=======
namespace Xtate.DataModel.Runtime;

public class RuntimePredicateEvaluator : IConditionExpression, IBooleanEvaluator
{
	public required RuntimePredicate Predicate { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<RuntimeExecutionContext>> RuntimeExecutionContextFactory { private get; [UsedImplicitly] init; }
>>>>>>> Stashed changes

#region Interface IBooleanEvaluator

	public async ValueTask<bool> EvaluateBoolean()
	{
		var executionContext = await RuntimeExecutionContextFactory().ConfigureAwait(false);

		Xtate.Runtime.SetCurrentExecutionContext(executionContext);

		return await Predicate.Evaluate().ConfigureAwait(false);
	}

#endregion

#region Interface IConditionExpression

	public string? Expression => Predicate.Expression;

#endregion
}

public abstract class RuntimePredicate : IConditionExpression
{
#region Interface IConditionExpression

	public string? Expression => null;

#endregion

	public static IConditionExpression GetPredicate(Func<bool> predicate)
	{
		Infra.Requires(predicate);

		return new EvaluatorSync(predicate);
	}

	public static IConditionExpression GetPredicate(Func<ValueTask<bool>> predicate)
	{
		Infra.Requires(predicate);

		return new EvaluatorAsync(predicate);
	}

	public abstract ValueTask<bool> Evaluate();

<<<<<<< Updated upstream
	private sealed class EvaluatorSync : RuntimePredicate
	{
		private readonly Func<bool> _predicate;

		public EvaluatorSync(Func<bool> predicate) => _predicate = predicate;

		public override ValueTask<bool> Evaluate() => new(_predicate());
	}

	private sealed class EvaluatorAsync : RuntimePredicate
	{
		private readonly Func<ValueTask<bool>> _predicate;

		public EvaluatorAsync(Func<ValueTask<bool>> predicate) => _predicate = predicate;

		public override ValueTask<bool> Evaluate() => _predicate();
=======
	private sealed class EvaluatorSync(Func<bool> predicate) : RuntimePredicate
	{
		public override ValueTask<bool> Evaluate() => new(predicate());
	}

	private sealed class EvaluatorAsync(Func<ValueTask<bool>> predicate) : RuntimePredicate
	{
		public override ValueTask<bool> Evaluate() => predicate();
>>>>>>> Stashed changes
	}
}