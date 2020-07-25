using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultIfEvaluator : IIf, IExecEvaluator, IAncestorProvider
	{
		private readonly IfEntity _if;

		public DefaultIfEvaluator(in IfEntity @if)
		{
			_if = @if;

			Infrastructure.Assert(@if.Condition != null);

			var currentCondition = @if.Condition.As<IBooleanEvaluator>();
			var currentActions = ImmutableArray.CreateBuilder<IExecEvaluator>();
			var branchesBuilder = ImmutableArray.CreateBuilder<(IBooleanEvaluator? Condition, ImmutableArray<IExecEvaluator> Actions)>();

			foreach (var op in @if.Action)
			{
				switch (op)
				{
					case IElseIf elseIf:
						branchesBuilder.Add((currentCondition, currentActions.ToImmutable()));
						Infrastructure.Assert(elseIf.Condition != null);
						currentCondition = elseIf.Condition.As<IBooleanEvaluator>();
						currentActions.Clear();
						break;

					case IElse _:
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
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			foreach (var (condition, actions) in Branches)
			{
				if (condition == null || await condition.EvaluateBoolean(executionContext, token).ConfigureAwait(false))
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