#region Copyright © 2019-2021 Sergii Artemenko

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

#endregion

using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Core
{
	[PublicAPI]
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

		public static ValueTask<T> WaitAsync<T>(this TaskCompletionSource<T> tcs, CancellationToken token)
		{
			if (tcs.Task.IsCompleted || !token.CanBeCanceled)
			{
				return new ValueTask<T>(tcs.Task);
			}

			if (token.IsCancellationRequested)
			{
				return new ValueTask<T>(Task.FromCanceled<T>(token));
			}

			return WaitAsyncLocal();

			async ValueTask<T> WaitAsyncLocal()
			{
				await Task.WhenAny(tcs.Task, Task.Delay(millisecondsDelay: -1, token)).ConfigureAwait(false);

				token.ThrowIfCancellationRequested();

				return await tcs.Task.ConfigureAwait(false);
			}
		}

		public static void Forget(this ValueTask valueTask) => valueTask.Preserve();

		public static void Forget<T>(this ValueTask<T> valueTask) => valueTask.Preserve();
	}
}