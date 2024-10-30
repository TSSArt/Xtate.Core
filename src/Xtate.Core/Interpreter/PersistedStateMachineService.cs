namespace Xtate.Core;

public class PersistedStateMachineService : IStateMachineService
{
	public required IPersistedStateMachineRunState RunState { private get; [UsedImplicitly] init; }

	public required IStateMachineService StateMachineService { private get; [UsedImplicitly] init; }

#region Interface IStateMachineService

	public virtual ValueTask<IStateMachine?> GetStateMachine() => RunState.IsRestored ? default : StateMachineService.GetStateMachine();

#endregion
}