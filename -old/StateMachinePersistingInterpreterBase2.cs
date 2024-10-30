namespace Xtate.Core;

public class StateMachinePersistingInterpreterBase2(
	IPersistingInterpreterState persistingInterpreterState,
	IInterpreterModel interpreterModel) : StateMachinePersistingInterpreterBase(
#pragma warning disable CS9107
	persistingInterpreterState,
#pragma warning restore CS9107
	interpreterModel)
{
	protected override async ValueTask<IEvent> ReadExternalEvent()
	{
		await persistingInterpreterState.CheckPoint(16).ConfigureAwait(false);

		return await base.ReadExternalEvent().ConfigureAwait(false);
	}
}