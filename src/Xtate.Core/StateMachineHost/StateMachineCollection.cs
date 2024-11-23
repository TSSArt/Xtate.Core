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

namespace Xtate;

using Endpoint = (IEventDispatcher? Dispatcher, IStateMachineController? Controller);

public class StateMachineCollection : IStateMachineCollection
{
	private readonly ConcurrentDictionary<SessionId, Endpoint> _controllers = new();

#region Interface IStateMachineCollection

	public ValueTask Dispatch(SessionId sessionId, IIncomingEvent incomingEvent, CancellationToken token)
	{
		if (_controllers.TryGetValue(sessionId, out var tuple) && tuple.Dispatcher is { } eventDispatcher)
		{
			if (incomingEvent is not IncomingEvent)
			{
				incomingEvent = new IncomingEvent(incomingEvent);
			}

			return eventDispatcher.Dispatch(incomingEvent, token);
		}

		return default;
	}

	public ValueTask Destroy(SessionId sessionId) => _controllers.TryGetValue(sessionId, out var tuple) && tuple.Controller is { } controller ? controller.Destroy() : default;

	public void Register(SessionId sessionId, IEventDispatcher eventDispatcher)
	{
		_controllers.AddOrUpdate(sessionId, static (_, dispatcher) => (dispatcher, default), Update, eventDispatcher);

		return;

		static Endpoint Update(SessionId _, Endpoint tuple, IEventDispatcher eventDispatcher)
		{
			Infra.Assert(tuple.Dispatcher is null);

			return (eventDispatcher, tuple.Controller);
		}
	}

	public void Register(SessionId sessionId, IStateMachineController controller)
	{
		_controllers.AddOrUpdate(sessionId, static (_, controller) => (default, controller), Update, controller);

		return;

		static Endpoint Update(SessionId _, Endpoint tuple, IStateMachineController controller)
		{
			Infra.Assert(tuple.Controller is null);

			return (tuple.Dispatcher, controller);
		}
	}

	public void Unregister(SessionId sessionId) => _controllers.TryRemove(sessionId, out _);

#endregion
}