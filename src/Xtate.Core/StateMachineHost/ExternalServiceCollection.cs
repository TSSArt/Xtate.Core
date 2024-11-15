namespace Xtate.Core;

public class ExternalServiceCollection : IExternalServiceCollection
{
	private readonly MiniDictionary<InvokeId, Func<IIncomingEvent, CancellationToken, ValueTask>> _handlers = new(InvokeId.InvokeUniqueIdComparer);

#region Interface IExternalServiceCollection

	public void Subscribe(InvokeId invokeId, Func<IIncomingEvent, CancellationToken, ValueTask> handler)
	{
		var added = _handlers.TryAdd(invokeId, handler);

		Infra.Assert(added);
	}

	public void Unsubscribe(InvokeId invokeId)
	{
		var removed = _handlers.TryRemove(invokeId, out _);

		Infra.Assert(removed);
	}

	public virtual ValueTask Dispatch(InvokeId invokeId, IIncomingEvent incomingEvent, CancellationToken token)
	{
		if (!_handlers.TryGetValue(invokeId, out var handler))
		{
			return default;
		}

		if (incomingEvent is not IncomingEvent)
		{
			incomingEvent = new IncomingEvent(incomingEvent);
		}

		return handler(incomingEvent, token);
	}

#endregion
}