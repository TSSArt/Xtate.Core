namespace Xtate.DataModel.Runtime;

public class RuntimePredicateEvaluator : IConditionExpression, IBooleanEvaluator
{
	public required RuntimePredicate Predicate { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<RuntimeExecutionContext>> RuntimeExecutionContextFactory { private get; [UsedImplicitly] init; }

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