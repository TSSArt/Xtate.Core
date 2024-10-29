using Xtate.Service;

namespace Xtate.Core;

public interface IStateMachineHostContext
{
	void AddStateMachineController(SessionId sessionId, IStateMachineController controller);
	void RemoveStateMachineController(SessionId sessionId);

	ValueTask AddService(SessionId sessionId,
						 InvokeId invokeId,
						 IExternalService externalService,
						 CancellationToken token);

	ValueTask<IExternalService?> TryCompleteService(SessionId sessionId, InvokeId invokeId);
	ValueTask<IExternalService?> TryRemoveService(SessionId sessionId, InvokeId invokeId);
}