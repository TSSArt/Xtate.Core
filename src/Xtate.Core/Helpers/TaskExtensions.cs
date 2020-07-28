#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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
#endregion

using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	internal static class TaskExtensions
	{
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

			return WaitAsyncLocal();

			async Task<T> WaitAsyncLocal()
			{
				await Task.WhenAny(task, Task.Delay(millisecondsDelay: -1, token)).ConfigureAwait(false);

				token.ThrowIfCancellationRequested();

				return await task.ConfigureAwait(false);
			}
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

			return WaitAsyncLocal();

			async ValueTask<T> WaitAsyncLocal()
			{
				var task = valueTask.AsTask();

				await Task.WhenAny(task, Task.Delay(millisecondsDelay: -1, token)).ConfigureAwait(false);

				token.ThrowIfCancellationRequested();

				return await task.ConfigureAwait(false);
			}
		}

		public static void Forget(this ValueTask valueTask)
		{
			if (!valueTask.IsCompleted)
			{
				valueTask.AsTask();
			}
		}

		public static void Forget<T>(this ValueTask<T> valueTask)
		{
			if (!valueTask.IsCompleted)
			{
				valueTask.AsTask();
			}
		}
	}
}