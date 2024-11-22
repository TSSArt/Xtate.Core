namespace Xtate.Core;

public interface IExternalServiceCollection
{
	void Subscribe(InvokeId invokeId, IEventDispatcher eventDispatcher);

	void Unsubscribe(InvokeId invokeId);

	ValueTask Dispatch(InvokeId invokeId, IIncomingEvent incomingEvent, CancellationToken token);
}