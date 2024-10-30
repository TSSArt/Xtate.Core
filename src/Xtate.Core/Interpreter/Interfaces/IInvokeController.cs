namespace Xtate;

public interface IInvokeController
{
	ValueTask Start(InvokeId invokeId, InvokeData invokeData);

	ValueTask Cancel(InvokeId invokeId);

	ValueTask Forward(InvokeId invokeId, IEvent evt);
}