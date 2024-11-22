namespace Xtate.Core;

public class ExternalServiceCollection : IExternalServiceCollection
{
	private readonly MiniDictionary<InvokeId, IEventDispatcher> _eventDispatchers = new(InvokeId.InvokeUniqueIdComparer);

#region Interface IExternalServiceCollection

	public void Subscribe(InvokeId invokeId, IEventDispatcher eventDispatcher)
	{
		var added = _eventDispatchers.TryAdd(invokeId, eventDispatcher);

		Infra.Assert(added);
	}

	public void Unsubscribe(InvokeId invokeId)
	{
		var removed = _eventDispatchers.TryRemove(invokeId, out _);

		Infra.Assert(removed);
	}

	public virtual ValueTask Dispatch(InvokeId invokeId, IIncomingEvent incomingEvent, CancellationToken token)
	{
		if (!_eventDispatchers.TryGetValue(invokeId, out var eventDispatcher))
		{
			return default;
		}

		if (incomingEvent is not IncomingEvent)
		{
			incomingEvent = new IncomingEvent(incomingEvent);
		}

		return eventDispatcher.Dispatch(incomingEvent, token);
	}

#endregion
}