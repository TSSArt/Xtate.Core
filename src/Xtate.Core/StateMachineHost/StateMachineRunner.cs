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


public class StateMachineRunner : IStateMachineRunner, IDisposable
{
	private readonly IStateMachineHostContext _context;
	private readonly IStateMachineController  _controller;
	private          SessionId?               _sessionId;

	public StateMachineRunner(IStateMachineHostContext context, IStateMachineController controller, IStateMachineSessionId sessionId)
	{
		_context = context;
		_controller = controller;
		_sessionId = sessionId.SessionId;

		_context.AddStateMachineController(_sessionId, controller);
	}

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IStateMachineRunner

	public async ValueTask WaitForCompletion() => await _controller.GetResult().ConfigureAwait(false);

#endregion

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (Interlocked.Exchange(ref _sessionId, value: default) is { } sessionId)
			{
				_context.RemoveStateMachineController(sessionId);
			}
		}
	}
}