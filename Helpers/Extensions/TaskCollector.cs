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

/// <summary>
/// Collects and manages tasks, ensuring proper disposal of resources.
/// </summary>
public class TaskCollector : IAsyncDisposable, IDisposable
{
    private MiniDictionary<Task, ValueTuple>? _tasks = new();

    #region Interface IAsyncDisposable

    /// <summary>
    /// Asynchronously disposes of the resources used by the TaskCollector.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(false);

        GC.SuppressFinalize(this);
    }

    #endregion

    #region Interface IDisposable

    /// <summary>
    /// Disposes of the resources used by the TaskCollector.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    #endregion

    /// <summary>
    /// Releases the unmanaged resources used by the TaskCollector and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        if (_tasks is not { } tasks)
        {
            return;
        }

        _tasks = default;

        while (tasks.TryTake(out var task, out _))
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                task.GetAwaiter().GetResult();
            }
        }
    }

    /// <summary>
    /// Asynchronously releases the unmanaged resources used by the TaskCollector and optionally releases the managed resources.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_tasks is not { } tasks)
        {
            return;
        }

        _tasks = default;

        List<Task>? list = default;

        while (true)
        {
            while (tasks.TryTake(out var task, out _))
            {
                list ??= [];

                list.Add(task);
            }

            if (list is null || list.Count == 0)
            {
                break;
            }

            await Task.WhenAll(list).ConfigureAwait(false);

            list.Clear();
        }
    }

    /// <summary>
    /// Collects a ValueTask into a private pool and tracks its successful completion.
    /// </summary>
    /// <param name="valueTask">The ValueTask to collect.</param>
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

    /// <summary>
    /// Collects a ValueTask of type T into a private pool and tracks its successful completion.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the ValueTask.</typeparam>
    /// <param name="valueTask">The ValueTask to collect.</param>
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

    /// <summary>
    /// Collects a Task into a private pool and tracks its successful completion.
    /// </summary>
    /// <param name="task">The Task to collect.</param>
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

    /// <summary>
    /// Registers a Task for management.
    /// </summary>
    /// <param name="task">The Task to register.</param>
    private void Register(Task task)
    {
        var tasks = _tasks;
        Infra.EnsureNotDisposed(tasks is not null, this);

        tasks.TryAdd(task, value: default);

        task.ContinueWith(Cleanup, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    /// <summary>
    /// Cleans up a completed Task.
    /// </summary>
    /// <param name="task">The Task to clean up.</param>
    private void Cleanup(Task task) => _tasks?.TryRemove(task, out _);
}
