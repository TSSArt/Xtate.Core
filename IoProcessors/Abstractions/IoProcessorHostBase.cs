// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.IoC.Tools;
using Xtate.StateMachineHost;
using Xtate.TaskMonitor;

namespace Xtate.IoProcessors;

public abstract class IoProcessorHostBase : IIoProcessorHost
{
	private CancellationTokenSource? _cts;

	protected CancellationToken Token { get; private set; } = new(true);

	public required DisposeToken DisposeTokenBase { private get; [SetByIoC] init; }

	public required ITaskMonitor TaskMonitorBase { private get; [SetByIoC] init; }

#region Interface IIoProcessorHost

	ValueTask IIoProcessorHost.Start() => Start();

	ValueTask IIoProcessorHost.Stop() => Stop();

#endregion

	protected virtual async ValueTask Start()
	{
		while (_cts?.IsCancellationRequested == true)
		{
			await Stop().ConfigureAwait(false);
		}

		if (_cts is null)
		{
			_cts = CancellationTokenSource.CreateLinkedTokenSource(DisposeTokenBase);
			Token = _cts.Token;

			StartNewBackgroundProcess().Forget(TaskMonitorBase);
		}
	}

	protected virtual async ValueTask Stop()
	{
		if (_cts is { } cts)
		{
			Token = new CancellationToken(true);
			_cts = null;

			await cts.CancelAsync().ConfigureAwait(false);

			cts.Dispose();
		}
	}

	protected virtual Task StartNewBackgroundProcess()
	{
		var task = Task.Factory.StartNew(static state => ((IoProcessorHostBase) state!).BackgroundProcess(), this, Token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);

		return task.Unwrap();
	}

	protected abstract Task BackgroundProcess();
}