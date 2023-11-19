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

using System.Collections.Immutable;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel;

public abstract class IfEvaluator : IIf, IExecEvaluator, IAncestorProvider
{
	private readonly IIf _if;

	protected IfEvaluator(IIf @if)
	{
		Infra.Requires(@if);

		_if = @if;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _if;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface IIf

	public virtual ImmutableArray<IExecutableEntity> Action    => _if.Action;
	public virtual IConditionExpression              Condition => _if.Condition!;

#endregion
}

[PublicAPI]
public class DefaultIfEvaluator : IfEvaluator
{
	public DefaultIfEvaluator(IIf @if) : base(@if)
	{
		Infra.NotNull(@if.Condition);

		var currentCondition = @if.Condition.As<IBooleanEvaluator>();
		var currentActions = ImmutableArray.CreateBuilder<IExecEvaluator>();
		var branchesBuilder = ImmutableArray.CreateBuilder<(IBooleanEvaluator? Condition, ImmutableArray<IExecEvaluator> Actions)>();

		foreach (var op in @if.Action)
		{
			switch (op)
			{
				case IElseIf elseIf:
					branchesBuilder.Add((currentCondition, currentActions.ToImmutable()));
					Infra.NotNull(elseIf.Condition);
					currentCondition = elseIf.Condition.As<IBooleanEvaluator>();
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

		branchesBuilder.Add((currentCondition, currentActions.ToImmutable()));

		Branches = branchesBuilder.ToImmutable();
	}

	public ImmutableArray<(IBooleanEvaluator? Condition, ImmutableArray<IExecEvaluator> Actions)> Branches { get; }

	public override async ValueTask Execute()
	{
		foreach (var (condition, actions) in Branches)
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