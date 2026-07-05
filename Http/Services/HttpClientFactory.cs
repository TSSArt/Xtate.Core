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

using System.Net.Http;

namespace Xtate.Http.Services;

[InstantiatedByIoC]
public class HttpClientFactory : IDisposable
{
	private HandlerEntry? _activeEntry;

	private bool _disposed;

	public TimeSpan HandlerLifetime { get; init; } = TimeSpan.FromMinutes(2);

	public TimeSpan HandlerGracePeriod { get; init; } = TimeSpan.FromMinutes(1);

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}

#endregion

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !_disposed)
		{
			_disposed = true;

			for (var entry = _activeEntry; entry != null; entry = entry.NextEntry)
			{
				entry.Dispose();
			}

			_activeEntry = null;
		}
	}

	[CalledByIoC]
	public HttpClient GetClient()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(HttpClientFactory));
		}

		return new HttpClient(GetHandler(), disposeHandler: false);
	}

	private HttpMessageHandler GetHandler()
	{
		TryDisposeNextEntry(_activeEntry);

		HandlerEntry? newEntry = null;

		while (true)
		{
			var entry = _activeEntry;

			if (entry?.IsExpired == false)
			{
				return entry.Handler;
			}

			newEntry ??= new HandlerEntry(HandlerLifetime, HandlerGracePeriod);
			newEntry.NextEntry = entry;

			if (Interlocked.CompareExchange(ref _activeEntry, newEntry, entry) == entry)
			{
				return newEntry.Handler;
			}
		}
	}

	private static void TryDisposeNextEntry(HandlerEntry? entry)
	{
		if (entry is null)
		{
			return;
		}

		TryDisposeNextEntry(entry.NextEntry);

		while (true)
		{
			var nextEntry = entry.NextEntry;

			if (nextEntry?.CanDispose != true)
			{
				return;
			}

			if (Interlocked.CompareExchange(ref entry.NextEntry, nextEntry.NextEntry, nextEntry) == nextEntry)
			{
				nextEntry.Dispose();

				return;
			}
		}
	}

	private class HandlerEntry : IDisposable
	{
		private readonly DateTime? _disposeAt;

		private readonly DateTime? _expiresAt;

		public HandlerEntry? NextEntry;

		public HandlerEntry(TimeSpan handlerLifetime, TimeSpan gracePeriod)
		{
#if NETCOREAPP2_1_OR_GREATER
			Handler = new SocketsHttpHandler { PooledConnectionLifetime = handlerLifetime, PooledConnectionIdleTimeout = gracePeriod };
			_expiresAt = null;
			_disposeAt = null;

#else
			Handler = new HttpClientHandler();
			_expiresAt = DateTime.UtcNow + handlerLifetime;
			_disposeAt = _expiresAt + gracePeriod;
#endif
		}

		public HttpMessageHandler Handler { get; }

		public bool IsExpired => DateTime.UtcNow >= _expiresAt;

		public bool CanDispose => DateTime.UtcNow >= _disposeAt;

	#region Interface IDisposable

		public void Dispose() => Handler.Dispose();

	#endregion
	}
}