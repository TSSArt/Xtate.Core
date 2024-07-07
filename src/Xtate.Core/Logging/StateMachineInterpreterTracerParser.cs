// Copyright © 2019-2024 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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