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

using System.Net;
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

		return new HttpClient(GetHandlerEntry().Handler, disposeHandler: false);
	}

	private HandlerEntry GetHandlerEntry()
	{
		TryDisposeNextEntry(_activeEntry);

		HandlerEntry? newEntry = null;

		while (true)
		{
			var entry = _activeEntry;

			if (entry?.IsExpired == false)
			{
				return entry;
			}

			newEntry ??= new HandlerEntry(HandlerLifetime, HandlerGracePeriod);
			newEntry.NextEntry = entry;

			if (Interlocked.CompareExchange(ref _activeEntry, newEntry, entry) == entry)
			{
				return newEntry;
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

#if NETCOREAPP2_1_OR_GREATER
	private class HandlerEntry(TimeSpan handlerLifetime, TimeSpan gracePeriod) : IDisposable
	{
		private readonly SocketsHttpHandler _handler = new() { PooledConnectionLifetime = handlerLifetime, PooledConnectionIdleTimeout = gracePeriod };

		public HandlerEntry? NextEntry;

		public HttpMessageHandler Handler => _handler;

		public CookieContainer CookieContainer
		{
			get => _handler.CookieContainer;
			set => _handler.CookieContainer = value;
		}

	#region Interface IDisposable

		public void Dispose() => _handler.Dispose();

	#endregion

#pragma warning disable CA1822

		public bool IsExpired => false;

		public bool CanDispose => false;

#pragma warning restore CA1822
	}

#else
	private class HandlerEntry(TimeSpan handlerLifetime, TimeSpan gracePeriod) : IDisposable
	{
		private readonly DateTime? _disposeAt = DateTime.UtcNow + handlerLifetime + gracePeriod;

		private readonly DateTime? _expiresAt = DateTime.UtcNow + handlerLifetime;

		private readonly HttpClientHandler _handler = new();

		public HandlerEntry? NextEntry;

		public HttpMessageHandler Handler => _handler;

		public CookieContainer CookieContainer
		{
			get => _handler.CookieContainer;
			set => _handler.CookieContainer = value;
		}

		public bool IsExpired => DateTime.UtcNow >= _expiresAt;

		public bool CanDispose => DateTime.UtcNow >= _disposeAt;

	#region Interface IDisposable

		public void Dispose() => Handler.Dispose();

	#endregion
	}

#endif
}