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

public class StateMachineCollection : IStateMachineCollection
{
	private readonly ExtDictionary<SessionId, IStateMachineController> _controllers = [];

	public required IDeadLetterQueue<IStateMachineCollection> DeadLetterQueue { private get; [UsedImplicitly] init; }

#region Interface IStateMachineCollection

	public async ValueTask Dispatch(SessionId sessionId, IIncomingEvent incomingEvent, CancellationToken token)
	{
		var (found, controller) = await _controllers.TryGetValueAsync(sessionId).ConfigureAwait(false);

		if (found)
		{
			if (incomingEvent is not IncomingEvent)
			{
				incomingEvent = new IncomingEvent(incomingEvent);
			}

			await controller.Dispatch(incomingEvent, token).ConfigureAwait(false);
		}
		else
		{
			await DeadLetterQueue.Enqueue(sessionId, incomingEvent).ConfigureAwait(false);
		}
	}

	public async ValueTask Destroy(SessionId sessionId)
	{
		var (found, controller) = await _controllers.TryGetValueAsync(sessionId).ConfigureAwait(false);

		if (found)
		{
			await controller.Destroy().ConfigureAwait(false);
		}
	}

	public void Register(SessionId sessionId)
	{
		var tryAddPending = _controllers.TryAddPending(sessionId);

		Infra.Assert(tryAddPending);
	}

	public void SetController(SessionId sessionId, IStateMachineController controller)
	{
		var tryAdd = _controllers.TryAdd(sessionId, controller);

		Infra.Assert(tryAdd);
	}

	public void Unregister(SessionId sessionId) => _controllers.TryRemove(sessionId, out _);

#endregion
}