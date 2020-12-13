#region Copyright © 2019-2020 Sergii Artemenko

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

using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	internal static class IoBoundTask
	{
		private static readonly SemaphoreSlim _poolSemaphore = new(initialCount: 64, maxCount: 64);

		public static async Task<T> Run<T>(Func<T> func, CancellationToken token)
		{
			await _poolSemaphore.WaitAsync(token).ConfigureAwait(false);

			try
			{
				return await Task.Factory.StartNew(func, token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
			}
			finally
			{
				_poolSemaphore.Release();
			}
		}

		public static async Task<T> Run<T>(Func<object?, T> func, object? state, CancellationToken token)
		{
			await _poolSemaphore.WaitAsync(token).ConfigureAwait(false);

			try
			{
				return await Task.Factory.StartNew(func, state, token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
			}
			finally
			{
				_poolSemaphore.Release();
			}
		}
	}
}