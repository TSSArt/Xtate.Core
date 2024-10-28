using Xtate.Service;

namespace Xtate.Core;

public class StateMachineControllerProxy(StateMachineRuntimeController stateMachineRuntimeController) : IStateMachineController
{
	private readonly IStateMachineController _baseStateMachineController = stateMachineRuntimeController;

#region Interface IEventDispatcher

	public ValueTask Send(IEvent evt, CancellationToken token = default) => _baseStateMachineController.Send(evt, token);

#endregion

#region Interface IService

	public ValueTask Destroy() => _baseStateMachineController.Destroy();

	ValueTask<DataModelValue> IService.GetResult() => _baseStateMachineController.GetResult();

#endregion


	//TODO:
	//public ValueTask DisposeAsync() => _baseStateMachineController.DisposeAsync();

	//public void TriggerDestroySignal() => _baseStateMachineController.TriggerDestroySignal();

	//public ValueTask StartAsync(CancellationToken token) => _baseStateMachineController.StartAsync(token);

	//public SessionId SessionId            => _baseStateMachineController.SessionId;
	//public Uri       StateMachineLocation => _baseStateMachineController.StateMachineLocation;
}