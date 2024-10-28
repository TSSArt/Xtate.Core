using Xtate.Service;

namespace Xtate.Core;

public interface IStateMachineHostContext
{
	void AddStateMachineController(SessionId sessionId, IStateMachineController controller);
	void RemoveStateMachineController(SessionId sessionId);

	ValueTask AddService(SessionId sessionId,
						 InvokeId invokeId,
						 IService service,
						 CancellationToken token);

	ValueTask<IService?> TryCompleteService(SessionId sessionId, InvokeId invokeId);
	ValueTask<IService?> TryRemoveService(SessionId sessionId, InvokeId invokeId);
}