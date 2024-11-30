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
	private readonly ConcurrentDictionary<SessionId, TaskCompletionSource<IStateMachineController>> _controllers = new();

#region Interface IStateMachineCollection

	public async ValueTask Dispatch(SessionId sessionId, IIncomingEvent incomingEvent, CancellationToken token)
	{
		if (_controllers.TryGetValue(sessionId, out var tcs))
		{
			var controller = await tcs.Task.ConfigureAwait(false);

			if (incomingEvent is not IncomingEvent)
			{
				incomingEvent = new IncomingEvent(incomingEvent);
			}

			await controller.Dispatch(incomingEvent, token).ConfigureAwait(false);
		}
	}

	public async ValueTask Destroy(SessionId sessionId)
	{
		if (_controllers.TryGetValue(sessionId, out var tcs))
		{
			var controller = await tcs.Task.ConfigureAwait(false);
			await controller.Destroy().ConfigureAwait(false);
		}
	}

	public void Register(SessionId sessionId)
	{
		var tryAdd = _controllers.TryAdd(sessionId, new TaskCompletionSource<IStateMachineController>());

		Infra.Assert(tryAdd);
	}

	public void SetController(SessionId sessionId, IStateMachineController controller)
	{
		if (_controllers.TryGetValue(sessionId, out var tcs))
		{
			tcs.SetResult(controller);
		}
		else
		{
			Infra.Fail();
		}
	}

	public void Unregister(SessionId sessionId)
	{
		if (_controllers.TryRemove(sessionId, out var tcs))
		{
			tcs.TrySetCanceled();
		}
	}

#endregion
}