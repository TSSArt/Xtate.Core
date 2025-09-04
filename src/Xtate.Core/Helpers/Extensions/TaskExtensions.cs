// Copyright © 2019-2025 Sergii Artemenko
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
    public static Task WaitAsync(this Task task, TaskMonitor taskMonitor, CancellationToken token) => taskMonitor.WaitAsync(task, token);

    public static Task<TResult> WaitAsync<TResult>(this Task<TResult> task, TaskMonitor taskMonitor, CancellationToken token) => taskMonitor.WaitAsync(task, token);

    public static ValueTask WaitAsync(this ValueTask valueTask, TaskMonitor taskMonitor, CancellationToken token) => taskMonitor.WaitAsync(valueTask, token);

    public static ValueTask<TResult> WaitAsync<TResult>(this ValueTask<TResult> valueTask, TaskMonitor taskMonitor, CancellationToken token) => taskMonitor.WaitAsync(valueTask, token);

    public static void Forget(this Task task) { }

    public static void Forget(this ValueTask valueTask)
    {
        _ = valueTask.Preserve();
    }

    public static void Forget<TResult>(this ValueTask<TResult> valueTask)
    {
        _ = valueTask.Preserve();
    }

    public static void Forget(this Task task, TaskMonitor taskMonitor) => taskMonitor.Forget(task);

    public static void Forget(this ValueTask valueTask, TaskMonitor taskMonitor) => taskMonitor.Forget(valueTask);

    public static void Forget<TResult>(this ValueTask<TResult> valueTask, TaskMonitor taskMonitor) => taskMonitor.Forget(valueTask);

    public static ValueTask WaitAsync(this ValueTask valueTask, CancellationToken token) =>
        valueTask.IsCompleted || !token.CanBeCanceled ? valueTask : new ValueTask(valueTask.AsTask().WaitAsync(token));

    public static ValueTask<TResult> WaitAsync<TResult>(this ValueTask<TResult> valueTask, CancellationToken token) =>
        valueTask.IsCompleted || !token.CanBeCanceled ? valueTask : new ValueTask<TResult>(valueTask.AsTask().WaitAsync(token));

#if !NET6_0_OR_GREATER
    public static Task WaitAsync(this Task task, CancellationToken token) =>
        task.IsCompleted || !token.CanBeCanceled
            ? task
            : token.IsCancellationRequested
                ? Task.FromCanceled(token)
                : task.ContinueWith(t => { }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

    public static Task<TResult> WaitAsync<TResult>(this Task<TResult> task, CancellationToken token) =>
        task.IsCompleted || !token.CanBeCanceled
            ? task
            : token.IsCancellationRequested
                ? Task.FromCanceled<TResult>(token)
                : task.ContinueWith(t => t.Result, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

#endif
}