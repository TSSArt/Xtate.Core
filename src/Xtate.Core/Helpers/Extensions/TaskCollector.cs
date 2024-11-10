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

public class TaskCollector : IAsyncDisposable, IDisposable
{
	private readonly MiniDictionary<Task, ValueTuple> _tasks = new();

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}

		while (_tasks.Count > 0)
		{
			foreach (var pair in _tasks)
			{
				var task = pair.Key;

				_tasks.TryRemove(task, out _);

				if (task.IsFaulted || task.IsCanceled)
				{
					task.GetAwaiter().GetResult();
				}
			}
		}
	}

	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}

#region Interface IAsyncDisposable

	async ValueTask IAsyncDisposable.DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);
		
		Dispose(false);
		
		GC.SuppressFinalize(this);
	}

#endregion

	protected virtual async ValueTask DisposeAsyncCore()
	{
		List<Task>? tasks = default;

		while (_tasks.Count > 0)
		{
			tasks ??= new List<Task>(_tasks.Count);

			foreach (var pair in _tasks)
			{
				tasks.Add(pair.Key);
			}

			foreach (var task in tasks)
			{
				_tasks.TryRemove(task, out _);
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);

			tasks.Clear();
		}
	}

	public void Collect(ValueTask valueTask)
	{
		if (valueTask.IsCompleted)
		{
			valueTask.GetAwaiter().GetResult();
		}
		else
		{
			Register(valueTask.AsTask());
		}
	}

	public void Collect<T>(ValueTask<T> valueTask)
	{
		if (valueTask.IsCompleted)
		{
			valueTask.GetAwaiter().GetResult();
		}
		else
		{
			Register(valueTask.AsTask());
		}
	}

	public void Collect(Task task)
	{
		if (task.IsCompleted)
		{
			task.GetAwaiter().GetResult();
		}
		else
		{
			Register(task);
		}
	}

	private void Register(Task task)
	{
		_tasks.TryAdd(task, value: default);

		const TaskContinuationOptions options = TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion;

		task.ContinueWith(t => _tasks.TryRemove(task, out _), options);
	}
}