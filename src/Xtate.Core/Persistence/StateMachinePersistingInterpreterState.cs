namespace Xtate.Core;

public class StateMachinePersistingInterpreterState : StateMachineInterpreterState
{
	public static StateMachinePersistingInterpreterState Suspended { get; } = new(nameof(Suspended));
	
	protected StateMachinePersistingInterpreterState(string displayName) : base(displayName) { }
}