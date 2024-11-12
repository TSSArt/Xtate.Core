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
	public static ValueTask WaitAsync(this ValueTask valueTask, CancellationToken token)
	{
		if (valueTask.IsCompleted || !token.CanBeCanceled)
		{
			return valueTask;
		}

		if (token.IsCancellationRequested)
		{
			return new ValueTask(Task.FromCanceled(token));
		}

		return new ValueTask(valueTask.AsTask().ContinueWith(_ => { }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current));
	}

	public static ValueTask<T> WaitAsync<T>(this ValueTask<T> valueTask, CancellationToken token)
	{
		if (valueTask.IsCompleted || !token.CanBeCanceled)
		{
			return valueTask;
		}

		if (token.IsCancellationRequested)
		{
			return new ValueTask<T>(Task.FromCanceled<T>(token));
		}

		return new ValueTask<T>(valueTask.AsTask().ContinueWith(t => t.Result, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current));
	}

#if !NET6_0_OR_GREATER
	public static Task WaitAsync(this Task task, CancellationToken token)
	{
		if (task.IsCompleted || !token.CanBeCanceled)
		{
			return task;
		}

		if (token.IsCancellationRequested)
		{
			return Task.FromCanceled(token);
		}

		return task.ContinueWith(_ => { }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
	}

	public static Task<T> WaitAsync<T>(this Task<T> task, CancellationToken token)
	{
		if (task.IsCompleted || !token.CanBeCanceled)
		{
			return task;
		}

		if (token.IsCancellationRequested)
		{
			return Task.FromCanceled<T>(token);
		}

		return task.ContinueWith(t => t.Result, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
	}

#endif
}