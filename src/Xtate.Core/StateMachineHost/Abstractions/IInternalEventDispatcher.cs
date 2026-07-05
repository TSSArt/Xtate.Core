using Xtate.DataModel;
using Xtate.StateMachine;

namespace Xtate.StateMachineHost;

public interface IInternalEventDispatcher<TSource>
{
	ValueTask Dispatch(ServiceId serviceId, IIncomingEvent incomingEvent, CancellationToken token);
}