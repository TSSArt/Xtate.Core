namespace Xtate.Core;

public interface IExternalServiceCollection
{
	void Subscribe(InvokeId invokeId, Func<IIncomingEvent, CancellationToken, ValueTask> handler);

	void Unsubscribe(InvokeId invokeId);

	ValueTask Dispatch(InvokeId invokeId, IIncomingEvent incomingEvent, CancellationToken token);
}