#region Copyright © 2019-2023 Sergii Artemenko

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

namespace Xtate.Core;

public readonly struct LockAsync : IDisposable
{
	private readonly SemaphoreSlim _semaphore;

	private LockAsync(SemaphoreSlim semaphore) => _semaphore = semaphore;

#region Interface IDisposable

	public void Dispose() => _semaphore.Dispose();

#endregion

	public static LockAsync Create() => new(new(initialCount: 1, maxCount: 1));

	public ConfiguredValueTaskAwaitable<LockDisposer> Lock(bool continueOnCapturedContext = false) => LockInternal().ConfigureAwait(continueOnCapturedContext);

	private async ValueTask<LockDisposer> LockInternal()
	{
		await _semaphore.WaitAsync().ConfigureAwait(false);

		return new LockDisposer(_semaphore);
	}

	public readonly struct LockDisposer(SemaphoreSlim semaphore) : IDisposable
	{

		#region Interface IDisposable

		public void Dispose() => semaphore.Release();

#endregion
	}
}