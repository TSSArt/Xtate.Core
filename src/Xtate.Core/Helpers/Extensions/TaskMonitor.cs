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

public class TaskMonitor : ITaskMonitor
{
	public required Deferred<ILogger<ITaskMonitor>> Logger { private get; [UsedImplicitly] init; }

#region Interface ITaskMonitor

	async ValueTask ITaskMonitor.RunAsync(ValueTask valueTask)
	{
		if (!valueTask.IsCompletedSuccessfully)
		{
			_ = ExecuteAsync(valueTask, await Logger().ConfigureAwait(false));
		}
	}

	async ValueTask ITaskMonitor.RunAsync<T>(ValueTask<T> valueTask)
	{
		if (!valueTask.IsCompletedSuccessfully)
		{
			_ = ExecuteAsync(valueTask, await Logger().ConfigureAwait(false));
		}
	}

	async ValueTask ITaskMonitor.RunAsync(Task task)
	{
		if (task.Status != TaskStatus.RanToCompletion)
		{
			_ = ExecuteAsync(task, await Logger().ConfigureAwait(false));
		}
	}

#endregion

	private static async Task ExecuteAsync(ValueTask valueTask, ILogger<ITaskMonitor> logger)
	{
		try
		{
			await valueTask.ConfigureAwait(false);
		}
		catch (OperationCanceledException ex)
		{
			await logger.Write(Level.Warning, eventId: 1, Resources.Message_TaskWasCanceled, ex).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			await logger.Write(Level.Error, eventId: 2, Resources.Message_TaskFailed, ex).ConfigureAwait(false);
		}
	}

	private async Task ExecuteAsync<T>(ValueTask<T> valueTask, ILogger<ITaskMonitor> logger)
	{
		try
		{
			await valueTask.ConfigureAwait(false);
		}
		catch (OperationCanceledException ex)
		{
			await logger.Write(Level.Warning, eventId: 1, Resources.Message_TaskWasCanceled, ex).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			await logger.Write(Level.Error, eventId: 2, Resources.Message_TaskFailed, ex).ConfigureAwait(false);
		}
	}

	private async Task ExecuteAsync(Task task, ILogger<ITaskMonitor> logger)
	{
		try
		{
			await task.ConfigureAwait(false);
		}
		catch (OperationCanceledException ex)
		{
			await logger.Write(Level.Warning, eventId: 1, Resources.Message_TaskWasCanceled, ex).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			await logger.Write(Level.Error, eventId: 2, Resources.Message_TaskFailed, ex).ConfigureAwait(false);
		}
	}
}