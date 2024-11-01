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

namespace Xtate.DataModel;

public class DefaultIfEvaluator : IfEvaluator
{
	private readonly ImmutableArray<(IBooleanEvaluator? Condition, ImmutableArray<IExecEvaluator> Actions)> _branches;

	public DefaultIfEvaluator(IIf iif) : base(iif)
	{
		var currentCondition = base.Condition?.As<IBooleanEvaluator>();
		Infra.NotNull(currentCondition);

		var currentActions = ImmutableArray.CreateBuilder<IExecEvaluator>();
		var branchesBuilder = ImmutableArray.CreateBuilder<(IBooleanEvaluator? Condition, ImmutableArray<IExecEvaluator> Actions)>();

		var operations = base.Action;

		if (!operations.IsDefaultOrEmpty)
		{
			foreach (var op in operations)
			{
				switch (op)
				{
					case IElseIf elseIf:
						branchesBuilder.Add((currentCondition, currentActions.ToImmutable()));
						currentCondition = elseIf.Condition?.As<IBooleanEvaluator>();
						Infra.NotNull(currentCondition);
						currentActions.Clear();

						break;

					case IElse:
						branchesBuilder.Add((currentCondition, currentActions.ToImmutable()));
						currentCondition = default!;
						currentActions.Clear();

						break;

					default:
						currentActions.Add(op.As<IExecEvaluator>());

						break;
				}
			}
		}

		branchesBuilder.Add((currentCondition, currentActions.ToImmutable()));

		_branches = branchesBuilder.ToImmutable();
	}

	public override async ValueTask Execute()
	{
		foreach (var (condition, actions) in _branches)
		{
			if (condition is null || await condition.EvaluateBoolean().ConfigureAwait(false))
			{
				foreach (var action in actions)
				{
					await action.Execute().ConfigureAwait(false);
				}

				return;
			}
		}
	}
}