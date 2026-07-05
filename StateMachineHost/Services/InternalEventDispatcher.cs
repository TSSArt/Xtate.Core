using Xtate.DataModel;
using Xtate.StateMachine;

namespace Xtate.StateMachineHost.Services;

public class InternalEventDispatcher<TSource> : IInternalEventDispatcher<TSource>
{
	public required IStateMachineCollection StateMachineCollection { private get; [SetByIoC] init; }

	public required IExternalServiceCollection ExternalServiceCollection { private get; [SetByIoC] init; }

	public required IDeadLetterQueue<TSource> DeadLetterQueue { private get; [SetByIoC] init; }

#region Interface IInternalEventDispatcher<TSource>

	public ValueTask Dispatch(ServiceId serviceId, IIncomingEvent incomingEvent, CancellationToken token) =>
		serviceId switch
		{
			SessionId sessionId => StateMachineCollection.Dispatch(sessionId, incomingEvent, token),
			InvokeId invokeId   => ExternalServiceCollection.Dispatch(invokeId, incomingEvent, token),
			_                   => DeadLetterQueue.Enqueue(serviceId, incomingEvent)
		};

#endregion
}