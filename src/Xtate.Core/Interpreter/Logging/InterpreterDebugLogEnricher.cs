namespace Xtate.Core;

public class InterpreterDebugLogEnricher<TSource> : ILogEnricher<TSource>
{
	public required IStateMachine StateMachine { private get; [UsedImplicitly] init; }

	public required IStateMachineContext StateMachineContext { private get; [UsedImplicitly] init; }

#region Interface ILogEnricher<TSource>

	public IEnumerable<LoggingParameter> EnumerateProperties()
	{
		if (!string.IsNullOrEmpty(StateMachine.Name))
		{
			yield return new LoggingParameter(name: @"StateMachineName", StateMachine.Name);
		}

		if (StateMachineContext.Configuration.Count > 0)
		{
			var activeStates = new DataModelList();

			foreach (var node in StateMachineContext.Configuration)
			{
				activeStates.Add(node.Id.Value);
			}

			activeStates.MakeDeepConstant();

			yield return new LoggingParameter(name: @"ActiveStates", activeStates);
		}
	}

	public string Namespace => @"ctx";

	public Level Level => Level.Debug;

#endregion
}