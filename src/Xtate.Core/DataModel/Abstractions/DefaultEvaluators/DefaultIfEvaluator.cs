#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultIfEvaluator : IIf, IExecEvaluator, IAncestorProvider
	{
		private readonly IfEntity _if;

		public DefaultIfEvaluator(in IfEntity @if)
		{
			_if = @if;

			Infrastructure.NotNull(@if.Condition);

			var currentCondition = @if.Condition.As<IBooleanEvaluator>();
			var currentActions = ImmutableArray.CreateBuilder<IExecEvaluator>();
			var branchesBuilder = ImmutableArray.CreateBuilder<(IBooleanEvaluator? Condition, ImmutableArray<IExecEvaluator> Actions)>();

			foreach (var op in @if.Action)
			{
				switch (op)
				{
					case IElseIf elseIf:
						branchesBuilder.Add((currentCondition, currentActions.ToImmutable()));
						Infrastructure.NotNull(elseIf.Condition);
						currentCondition = elseIf.Condition.As<IBooleanEvaluator>();
						currentActions.Clear();
						break;

					case IElse:
						branchesBuilder.Add((currentCondition, currentActions.ToImmutable()));
						currentCondition = null!;
						currentActions.Clear();
						break;

					default:
						currentActions.Add(op.As<IExecEvaluator>());
						break;
				}
			}

			branchesBuilder.Add((currentCondition, currentActions.ToImmutable()));

			Branches = branchesBuilder.ToImmutable();
		}

		public ImmutableArray<(IBooleanEvaluator? Condition, ImmutableArray<IExecEvaluator> Actions)> Branches { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _if.Ancestor;

	#endregion

	#region Interface IExecEvaluator

		public virtual async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			foreach (var (condition, actions) in Branches)
			{
				if (condition is null || await condition.EvaluateBoolean(executionContext, token).ConfigureAwait(false))
				{
					foreach (var action in actions)
					{
						await action.Execute(executionContext, token).ConfigureAwait(false);
					}

					return;
				}
			}
		}

	#endregion

	#region Interface IIf

		public ImmutableArray<IExecutableEntity> Action    => _if.Action;
		public IConditionExpression              Condition => _if.Condition!;

	#endregion
	}
}