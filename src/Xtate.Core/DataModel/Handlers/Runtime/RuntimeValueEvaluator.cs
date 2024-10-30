namespace Xtate.DataModel.Runtime;

public class RuntimeValueEvaluator : IValueExpression, IObjectEvaluator
{
	public required RuntimeValue Value { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<RuntimeExecutionContext>> RuntimeExecutionContextFactory { private get; [UsedImplicitly] init; }

#region Interface IObjectEvaluator

	public async ValueTask<IObject> EvaluateObject()
	{
		var executionContext = await RuntimeExecutionContextFactory().ConfigureAwait(false);

		Xtate.Runtime.SetCurrentExecutionContext(executionContext);

		return await Value.Evaluate().ConfigureAwait(false);
	}

#endregion

#region Interface IValueExpression

	public string? Expression => Value.Expression;

#endregion
}