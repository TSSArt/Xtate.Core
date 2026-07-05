using Xtate.DataModel;
using Xtate.StateMachine;

namespace Xtate.StateMachineHost.Services;

public class ExternalEventDispatcher<TSource> : IExternalEventDispatcher<TSource>
{
	public required IStateMachineCollection StateMachineCollection { private get; [SetByIoC] init; }

	public required IExternalServiceGlobalCollection ExternalServiceGlobalCollection { private get; [SetByIoC] init; }

	public required IDeadLetterQueue<TSource> DeadLetterQueue { private get; [SetByIoC] init; }

#region Interface IExternalEventDispatcher<TSource>

	public async ValueTask Dispatch(ServiceId serviceId, IIncomingEvent incomingEvent, CancellationToken token)
	{
		switch (serviceId)
		{
			case SessionId sessionId:
				await StateMachineCollection.Dispatch(sessionId, incomingEvent, token).ConfigureAwait(false);

				break;
			case UniqueInvokeId uniqueInvokeId:

				if (!await ExternalServiceGlobalCollection.TryDispatch(uniqueInvokeId, incomingEvent, token).ConfigureAwait(false))
				{
					await DeadLetterQueue.Enqueue(serviceId, incomingEvent).ConfigureAwait(false);
				}

				break;
			default:
				await DeadLetterQueue.Enqueue(serviceId, incomingEvent).ConfigureAwait(false);

				break;
		}
	}

#endregion
}