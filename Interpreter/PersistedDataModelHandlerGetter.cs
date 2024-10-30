using Xtate.DataModel;

namespace Xtate.Core;

public class PersistedDataModelHandlerGetter
{
	public required IPersistedStateMachineRunState RunState { private get; [UsedImplicitly] init; }

	public required IInterpreterModel InterpreterModel { private get; [UsedImplicitly] init; }

	public required IDataModelHandlerService DataModelHandlerService { private get; [UsedImplicitly] init; }

	public required IStateMachine? StateMachine { private get; [UsedImplicitly] init; }

	[UsedImplicitly]
	public virtual ValueTask<IDataModelHandler?> GetDataModelHandler() =>
		DataModelHandlerService.GetDataModelHandler(RunState.IsRestored ? InterpreterModel.Root.DataModelType : StateMachine.DataModelType);
}