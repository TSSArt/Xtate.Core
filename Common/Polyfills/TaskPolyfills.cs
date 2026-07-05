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

#if !NET6_0_OR_GREATER

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks;

internal static class TaskPolyfills
{
	extension(Task task)
	{
		public Task WaitAsync(CancellationToken token) =>
			task.IsCompleted || !token.CanBeCanceled
				? task
				: token.IsCancellationRequested
					? Task.FromCanceled(token)
					: task.ContinueWith(_ => { }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
	}

	extension<TResult>(Task<TResult> task)
	{
		public Task<TResult> WaitAsync(CancellationToken token) =>
			task.IsCompleted || !token.CanBeCanceled
				? task
				: token.IsCancellationRequested
					? Task.FromCanceled<TResult>(token)
					: task.ContinueWith(t => t.Result, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
	}
}

#endif