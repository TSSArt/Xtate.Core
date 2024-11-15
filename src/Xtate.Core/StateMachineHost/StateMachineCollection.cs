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

using System.Collections.Concurrent;

namespace Xtate;

public class StateMachineCollection : IStateMachineCollection
{
	private readonly ConcurrentDictionary<SessionId, IStateMachineController> _controllers = new();

#region Interface IStateMachineCollection

	public ValueTask Dispatch(SessionId sessionId, IIncomingEvent incomingEvent, CancellationToken token)
	{
		if (!_controllers.TryGetValue(sessionId, out var stateMachineController))
		{
			return default;
		}

		if (incomingEvent is not IncomingEvent)
		{
			incomingEvent = new IncomingEvent(incomingEvent);
		}

		return stateMachineController.Dispatch(incomingEvent, token);
	}

	public ValueTask Destroy(SessionId sessionId) => _controllers.TryGetValue(sessionId, out var stateMachineController) ? stateMachineController.Destroy() : default;

	public void Register(SessionId sessionId, IStateMachineController controller)
	{
		var added = _controllers.TryAdd(sessionId, controller);

		Infra.Assert(added);
	}

	public void Unregister(SessionId sessionId)
	{
		_controllers.TryRemove(sessionId, out _);
	}

#endregion
}