#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using Xtate.IoC;

namespace Xtate.Core;

public class StateMachineRunner : IStateMachineRunner, IDisposable
{
	private readonly object                   _sync = new();
	private          bool                     _disposed;
	public required  IStateMachineHostContext _context    { private get; [UsedImplicitly] init; }
	public required  IStateMachineController  _controller { private get; [UsedImplicitly] init; }

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IStateMachineRunner

	public async ValueTask<IStateMachineController> Run(CancellationToken token)
	{
		lock (_sync)
		{
			XtateObjectDisposedException.ThrowIf(_disposed, this);

			_context.AddStateMachineController(_controller);
		}

		await _controller.StartAsync(token).ConfigureAwait(false);

		return _controller;
	}

	public async ValueTask Wait(CancellationToken token)
	{
		IStateMachineController? controller;

		lock (_sync)
		{
			if (_disposed)
			{
				return;
			}

			controller = _controller;
		}

		if (controller is not null)
		{
			await controller.GetResult(token).ConfigureAwait(false);
		}
	}

#endregion

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			lock (_sync)
			{
				if (_disposed)
				{
					return;
				}

				if (_controller is { } controller)
				{
					_context.RemoveStateMachineController(controller);
				}

				_disposed = true;
			}
		}
	}
}