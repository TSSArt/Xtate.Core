using Xtate.Interpreter;

namespace Xtate.Core.Test.Exhaustive.Interpreter;

internal sealed class InterpreterStateTrace : INotifyStateChanged
{
	private readonly List<StateMachineInterpreterState> _states = [];

	public IReadOnlyList<StateMachineInterpreterState> States => _states;

	public ValueTask OnChanged(StateMachineInterpreterState state)
	{
		_states.Add(state);
		return ValueTask.CompletedTask;
	}
}
