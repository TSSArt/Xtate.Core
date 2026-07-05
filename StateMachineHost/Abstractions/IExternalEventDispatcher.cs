using Xtate.DataModel;
using Xtate.StateMachine;

namespace Xtate.StateMachineHost;

public interface IExternalEventDispatcher<TSource>
{
	ValueTask Dispatch(ServiceId serviceId, IIncomingEvent incomingEvent, CancellationToken token);
}