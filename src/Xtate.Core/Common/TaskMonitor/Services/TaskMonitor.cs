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

using Xtate.Logging;

namespace Xtate.TaskMonitor.Services;

[InstantiatedByIoC]
public class TaskMonitor : ITaskMonitor
{
	public required ILogger<TaskMonitor> Logger { protected get; [SetByIoC] init; }

#region Interface ITaskMonitor

	public Task WaitAsync(Task task, CancellationToken token) => task.IsCompleted || !token.CanBeCanceled ? task : WaitAndMonitor(task, token);

	public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task.IsCompleted || !token.CanBeCanceled ? task : WaitAndMonitor(task, token);

	public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => valueTask.IsCompleted || !token.CanBeCanceled ? valueTask : new ValueTask(WaitAndMonitor(AsTask(valueTask), token));

	public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) =>
		valueTask.IsCompleted || !token.CanBeCanceled ? valueTask : new ValueTask<TResult>(WaitAndMonitor(AsTask(valueTask), token));

#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER
	private static Task AsTask(ValueTask valueTask) => valueTask.AsTask();
	
	private static Task<TResult> AsTask<TResult>(ValueTask<TResult> valueTask) => valueTask.AsTask();

#else
	private static async Task AsTask(ValueTask valueTask) => await valueTask.ConfigureAwait(false);

	private static async Task<TResult> AsTask<TResult>(ValueTask<TResult> valueTask) => await valueTask.ConfigureAwait(false);
#endif

	public void Forget(Task task)
	{
		if (task.Status is TaskStatus.RanToCompletion)
		{
			return;
		}

		if (task.Status is TaskStatus.Faulted or TaskStatus.Canceled)
		{
			task.GetAwaiter().GetResult();

			return;
		}

		_ = MonitorTaskCompletion(task);
	}

	public void Forget(ValueTask valueTask)
	{
		if (valueTask.IsCompleted)
		{
			valueTask.GetAwaiter().GetResult();

			return;
		}

		_ = MonitorTaskCompletion(valueTask);
	}

	public void Forget<TResult>(ValueTask<TResult> valueTask)
	{
		if (valueTask.IsCompleted)
		{
			valueTask.GetAwaiter().GetResult();

			return;
		}

		_ = MonitorTaskCompletion(valueTask);
	}

#endregion

	private async Task WaitAndMonitor(Task task, CancellationToken token)
	{
		var wait = task.WaitAsync(token);

		try
		{
			await wait.ConfigureAwait(false);
		}
		catch (OperationCanceledException ex) when (token.IsCancellationRequested && ex.CancellationToken == token && !ReferenceEquals(wait, task))
		{
			_ = MonitorTaskCompletion(task);

			throw;
		}
	}

	private async Task<TResult> WaitAndMonitor<TResult>(Task<TResult> task, CancellationToken token)
	{
		var wait = task.WaitAsync(token);

		try
		{
			return await wait.ConfigureAwait(false);
		}
		catch (OperationCanceledException ex) when (token.IsCancellationRequested && ex.CancellationToken == token && !ReferenceEquals(wait, task))
		{
			_ = MonitorTaskCompletion(task);

			throw;
		}
	}

	private async Task MonitorTaskCompletion(Task task)
	{
		try
		{
			await task.ConfigureAwait(false);
		}
		catch (OperationCanceledException ex)
		{
			await TaskCancelled(ex).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			await TaskFailed(ex).ConfigureAwait(false);
		}
	}

	private async Task MonitorTaskCompletion(ValueTask valueTask)
	{
		try
		{
			await valueTask.ConfigureAwait(false);
		}
		catch (OperationCanceledException ex)
		{
			await TaskCancelled(ex).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			await TaskFailed(ex).ConfigureAwait(false);
		}
	}

	private async Task MonitorTaskCompletion<TResult>(ValueTask<TResult> valueTask)
	{
		try
		{
			await valueTask.ConfigureAwait(false);
		}
		catch (OperationCanceledException ex)
		{
			await TaskCancelled(ex).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			await TaskFailed(ex).ConfigureAwait(false);
		}
	}

	protected virtual ValueTask TaskCancelled(OperationCanceledException ex) => Logger.Write(Level.Warning, eventId: 1, Resources.Message_TaskWasCanceled, ex);

	protected virtual ValueTask TaskFailed(Exception ex) => Logger.Write(Level.Error, eventId: 2, Resources.Message_TaskFailed, ex);
}
