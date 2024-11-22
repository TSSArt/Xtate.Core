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

internal static class TaskExtensions
{
	public static ValueTask Forget(this ValueTask valueTask, ITaskMonitor taskMonitor) => taskMonitor.RunAsync(valueTask);

	public static ValueTask Forget<T>(this ValueTask<T> valueTask, ITaskMonitor taskMonitor) => taskMonitor.RunAsync(valueTask);

	public static ValueTask Forget(this Task task, ITaskMonitor taskMonitor)=> taskMonitor.RunAsync(task);

	public static ValueTask WaitAsync(this ValueTask valueTask, DisposeToken disposeToken) => WaitAsync(valueTask, disposeToken.TaskMonitor, disposeToken.Token);

	public static ValueTask<T> WaitAsync<T>(this ValueTask<T> valueTask, DisposeToken disposeToken) => WaitAsync(valueTask, disposeToken.TaskMonitor, disposeToken.Token);

	public static Task WaitAsync(this Task task, DisposeToken disposeToken) => WaitAsync(task, disposeToken.TaskMonitor, disposeToken.Token);

	public static Task<T> WaitAsync<T>(this Task<T> task, DisposeToken disposeToken) => WaitAsync(task, disposeToken.TaskMonitor, disposeToken.Token);

	public static ValueTask WaitAsync(this ValueTask valueTask, ITaskMonitor taskMonitor, CancellationToken token)
	{
		if (valueTask.IsCompleted || !token.CanBeCanceled)
		{
			return valueTask;
		}

		if (token.IsCancellationRequested)
		{
			return new ValueTask(Task.FromCanceled(token));
		}

		return new ValueTask(WaitWithMonitor(valueTask.AsTask(), taskMonitor, token));
	}

	public static ValueTask<T> WaitAsync<T>(this ValueTask<T> valueTask, ITaskMonitor taskMonitor, CancellationToken token)
	{
		if (valueTask.IsCompleted || !token.CanBeCanceled)
		{
			return valueTask;
		}

		if (token.IsCancellationRequested)
		{
			return new ValueTask<T>(Task.FromCanceled<T>(token));
		}

		return new ValueTask<T>(WaitWithMonitor(valueTask.AsTask(), taskMonitor, token));
	}
	public static Task WaitAsync(this Task task, ITaskMonitor taskMonitor, CancellationToken token)
	{
		if (task.IsCompleted || !token.CanBeCanceled)
		{
			return task;
		}

		if (token.IsCancellationRequested)
		{
			return Task.FromCanceled(token);
		}

		return WaitWithMonitor(task, taskMonitor, token);
	}

	public static Task<T> WaitAsync<T>(this Task<T> task, ITaskMonitor taskMonitor, CancellationToken token)
	{
		if (task.IsCompleted || !token.CanBeCanceled)
		{
			return task;
		}

		if (token.IsCancellationRequested)
		{
			return Task.FromCanceled<T>(token);
		}
		
		return WaitWithMonitor(task, taskMonitor, token);
	}

	private static Task WaitWithMonitor(Task task, ITaskMonitor? taskMonitor, CancellationToken token)
	{
#if NET6_0_OR_GREATER
		var cancellableTask = task.WaitAsync(token);
#else
		var cancellableTask = task.ContinueWith(t => { }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
#endif

		return taskMonitor is not null ? Monitor() : cancellableTask;

		async Task Monitor()
		{
			await taskMonitor.RunAsync(task).ConfigureAwait(false);

			await cancellableTask.ConfigureAwait(false);
		}
	}

	private static Task<T> WaitWithMonitor<T>(Task<T> task, ITaskMonitor? taskMonitor, CancellationToken token)
	{
#if NET6_0_OR_GREATER
		var cancellableTask = task.WaitAsync(token);
#else
		var cancellableTask = task.ContinueWith(t => t.Result, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
#endif

		return taskMonitor is not null ? Monitor() : cancellableTask;

		async Task<T> Monitor()
		{
			await taskMonitor.RunAsync(task).ConfigureAwait(false);

			return await cancellableTask.ConfigureAwait(false);
		}
	}
}