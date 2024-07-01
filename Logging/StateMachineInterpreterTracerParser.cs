namespace Xtate.Core;

public class StateMachineInterpreterTracerParser : ILogEntityParser<SendId>, ILogEntityParser<InvokeId>, ILogEntityParser<InvokeData>, ILogEntityParser<IOutgoingEvent>, ILogEntityParser<IEvent>,
												   ILogEntityParser<IStateEntity>, ILogEntityParser<StateMachineInterpreterState>
{
#region Interface ILogEntityParser<IEvent>

	public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(IEvent evt)
	{
		if (!evt.NameParts.IsDefaultOrEmpty)
		{
			yield return (@"EventName", EventName.ToName(evt.NameParts));
		}

		yield return (@"EventType", evt.Type);

		if (evt.Origin is { } origin)
		{
			yield return (@"Origin", origin);
		}

		if (evt.OriginType is { } originType)
		{
			yield return (@"OriginType", originType);
		}

		if (evt.SendId is { } sendId)
		{
			yield return (@"SendId", sendId);
		}

		if (evt.InvokeId is { } invokeId)
		{
			yield return (@"InvokeId", invokeId);
		}
	}

#endregion

#region Interface ILogEntityParser<InvokeData>

	public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(InvokeData invokeData)
	{
		if (invokeData.InvokeId is { } invokeId)
		{
			yield return (@"InvokeId", invokeId);
		}

		yield return (@"InvokeType", invokeData.Type);

		if (invokeData.Source is { } source)
		{
			yield return (@"InvokeSource", source);
		}
	}

#endregion

#region Interface ILogEntityParser<InvokeId>

	public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(InvokeId invokeId)
	{
		yield return (@"InvokeId", invokeId);
	}

#endregion

#region Interface ILogEntityParser<IOutgoingEvent>

	public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(IOutgoingEvent outgoingEvent)
	{
		if (!outgoingEvent.NameParts.IsDefaultOrEmpty)
		{
			yield return (@"EventName", EventName.ToName(outgoingEvent.NameParts));
		}

		if (outgoingEvent.Type is { } type)
		{
			yield return (@"EventType", type);
		}

		if (outgoingEvent.Target is { } target)
		{
			yield return (@"EventTarget", target);
		}

		if (outgoingEvent.SendId is { } sendId)
		{
			yield return (@"SendId", sendId);
		}

		if (outgoingEvent.DelayMs > 0)
		{
			yield return (@"Delay", outgoingEvent.DelayMs);
		}
	}

#endregion

#region Interface ILogEntityParser<IStateEntity>

	public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(IStateEntity stateEntity)
	{
		if (stateEntity.Id is { } stateId)
		{
			yield return (@"StateId", stateId);
		}
	}

#endregion

#region Interface ILogEntityParser<SendId>

	public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(SendId sendId)
	{
		yield return (@"SendId", sendId);
	}

#endregion

#region Interface ILogEntityParser<StateMachineInterpreterState>

	public IEnumerable<(string Name, object Value)> EnumerateProperties(StateMachineInterpreterState state)
	{
		yield return (@"InterpreterState", state);
	}

#endregion
}