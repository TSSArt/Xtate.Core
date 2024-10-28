namespace Xtate.Core;

public class StateMachineArguments(DataModelValue arguments) : IStateMachineArguments
{
	public DataModelValue Arguments { get; init; } = arguments;
}