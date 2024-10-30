namespace Xtate.DataModel.Runtime;

public class RuntimeActionExecutor : IExecutableEntity, IExecEvaluator
{
	public required RuntimeAction Action { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<RuntimeExecutionContext>> RuntimeExecutionContextFactory { private get; [UsedImplicitly] init; }

#region Interface IExecEvaluator

	public async ValueTask Execute()
	{
		var executionContext = await RuntimeExecutionContextFactory().ConfigureAwait(false);

		Xtate.Runtime.SetCurrentExecutionContext(executionContext);

		await Action.DoAction().ConfigureAwait(false);
	}

#endregion
}