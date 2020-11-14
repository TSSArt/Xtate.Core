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
	public sealed class AsyncReaderWriterLock : IDisposable
	{
		private readonly SemaphoreSlim _readSemaphore  = new(initialCount: 1, maxCount: 1);
		private readonly SemaphoreSlim _writeSemaphore = new(initialCount: 1, maxCount: 1);

		private int _readerCount;

	#region Interface IDisposable

		public void Dispose()
		{
			_writeSemaphore.Dispose();
			_readSemaphore.Dispose();
		}

	#endregion

		public async Task AcquireWriterLock(CancellationToken token = default)
		{
			await _writeSemaphore.WaitAsync(token).ConfigureAwait(false);
			await SafeAcquireReadSemaphore(token).ConfigureAwait(false);
		}

		public void ReleaseWriterLock()
		{
			_readSemaphore.Release();
			_writeSemaphore.Release();
		}

		public async Task AcquireReaderLock(CancellationToken token = default)
		{
			await _writeSemaphore.WaitAsync(token).ConfigureAwait(false);

			if (Interlocked.Increment(ref _readerCount) == 1)
			{
				await SafeAcquireReadSemaphore(token).ConfigureAwait(false);
			}

			_writeSemaphore.Release();
		}

		public void ReleaseReaderLock()
		{
			if (Interlocked.Decrement(ref _readerCount) == 0)
			{
				_readSemaphore.Release();
			}
		}

		private async Task SafeAcquireReadSemaphore(CancellationToken token)
		{
			try
			{
				await _readSemaphore.WaitAsync(token).ConfigureAwait(false);
			}
			catch
			{
				_writeSemaphore.Release();

				throw;
			}
		}
	}
}