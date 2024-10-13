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

using System.Diagnostics;
using InvalidOperationException = System.InvalidOperationException;

namespace Xtate.Core;

internal static class TaskExtensions
{
	public static void SynchronousWait(this ValueTask valueTask)
	{
		if (valueTask.IsCompleted)
		{
			valueTask.GetAwaiter().GetResult();
		}
		else
		{
			valueTask.AsTask().GetAwaiter().GetResult();
		}
	}

	public static T SynchronousGetResult<T>(this ValueTask<T> valueTask)
	{
		if (valueTask.IsCompleted)
		{
			return valueTask.GetAwaiter().GetResult();
		}

		return valueTask.AsTask().GetAwaiter().GetResult();
	}

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

		return new ValueTask(Task.WhenAny(valueTask.AsTask(), Task.Delay(Timeout.Infinite, token)));
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

		return WaitAsyncLocal(Task.WhenAny(valueTask.AsTask(), Task.Delay(Timeout.Infinite, token)));

		static async ValueTask<T> WaitAsyncLocal(Task<Task> waitAnyTask)
		{
			var completedTask = await waitAnyTask.ConfigureAwait(false);

			Debug.Assert(completedTask.IsCompleted);

			if (completedTask is Task<T> task)
			{
				return task.GetAwaiter().GetResult();
			}

			completedTask.GetAwaiter().GetResult();

			throw new InvalidOperationException();
		}
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

		return Task.WhenAny(task, Task.Delay(Timeout.Infinite, token));
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

		return WaitAsyncLocal(Task.WhenAny(task, Task.Delay(Timeout.Infinite, token)));

		static async Task<T> WaitAsyncLocal(Task<Task> waitAnyTask)
		{
			var completedTask = await waitAnyTask.ConfigureAwait(false);

			Debug.Assert(completedTask.IsCompleted);

			if (completedTask is Task<T> task)
			{
				return task.GetAwaiter().GetResult();
			}

			completedTask.GetAwaiter().GetResult();

			throw new InvalidOperationException();
		}
	}

#endif

	/// <summary>
	///     Do not wait ValueTask for completion if it is not completed. Result of execution ignored.
	/// </summary>
	/// <param name="valueTask">Instance of ValueTask</param>
	public static void Forget(this ValueTask valueTask) => valueTask.Preserve();

	/// <summary>
	///     Do not wait ValueTask for completion if it is not completed. Result of execution ignored.
	/// </summary>
	/// <typeparam name="T">Type result</typeparam>
	/// <param name="valueTask">Instance of ValueTask</param>
	public static void Forget<T>(this ValueTask<T> valueTask) => valueTask.Preserve();
}