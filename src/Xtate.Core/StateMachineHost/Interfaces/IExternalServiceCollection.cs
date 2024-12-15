using Xtate.ExternalService;

namespace Xtate.Core;

public interface IExternalServiceCollection
{
	void Register(InvokeId invokeId);

	void SetExternalService(InvokeId invokeId, IExternalService externalService);

	void Unregister(InvokeId invokeId);

	ValueTask<bool> TryDispatch(InvokeId invokeId, IIncomingEvent incomingEvent, CancellationToken token);
}