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

public sealed partial class StateMachineHost(StateMachineHostOptions options) : IAsyncDisposable, IDisposable
{
	private readonly StateMachineHostOptions _options = options ?? throw new ArgumentNullException(nameof(options));

	private bool _asyncOperationInProgress;

	private StateMachineHostContext? _context;

	private bool _disposed;

	public required Func<ValueTask<StateMachineHostContext>> ContextFactory;

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
		{
			_disposed = true;
		}

		if (_context is { } context)
		{
			_context = default;
			await context.DisposeAsync().ConfigureAwait(false);
		}

		await StateMachineHostStopAsync().ConfigureAwait(false);

		_disposed = true;
	}

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		if (_disposed)
		{
			_disposed = true;
		}

		if (_context is { } context)
		{
			_context = default;
			//TODO:
			//context.Dispose();
		}

		//TODO:
		//StateMachineHostStopAsync();

		_disposed = true;
	}

#endregion

#region Interface IHostController

	public async ValueTask StartHost()
	{
		if (_context is not null)
		{
			return;
		}

		if (_asyncOperationInProgress)
		{
			throw new InvalidOperationException(Resources.Exception_AnotherAsynchronousOperationInProgress);
		}

		try
		{
			_asyncOperationInProgress = true;

			var context = await ContextFactory().ConfigureAwait(false); //TODO:? move after startAsync()?

			await StateMachineHostStartAsync().ConfigureAwait(false);

			_context = context;
		}
		finally
		{
			_asyncOperationInProgress = false;
		}
	}

	public async ValueTask StopHost()
	{
		if (_asyncOperationInProgress)
		{
			throw new InvalidOperationException(Resources.Exception_AnotherAsynchronousOperationInProgress);
		}

		var context = _context;

		if (context is null)
		{
			return;
		}

		_asyncOperationInProgress = true;
		_context = default;

		try
		{
			context.Suspend();

			await context.WaitAllAsync(default).ConfigureAwait(false); //TODO:
		}
		catch (OperationCanceledException ex) when (ex.CancellationToken == default) //TODO:
		{
			context.Stop();
		}
		finally
		{
			await StateMachineHostStopAsync().ConfigureAwait(false);
			await context.DisposeAsync().ConfigureAwait(false);

			_asyncOperationInProgress = false;
		}
	}

#endregion

	public async Task WaitAllStateMachinesAsync(CancellationToken token = default)
	{
		var context = _context;

		if (context is not null)
		{
			await context.WaitAllAsync(token).ConfigureAwait(false);
		}
	}
}