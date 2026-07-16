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
using Xtate.IoC.Options;

namespace Xtate.IoProcessors.Http.Services;

[InstantiatedByIoC]
public class HttpIoProcessorHost : ResilientIoProcessorHostBase, IDisposable
{
	public HttpIoProcessorHost(IOptions<HttpIoProcessorOptions> options)
	{
		Listener = new HttpListener();

		if (options.Value.ListenUrl is { } listenUrl)
		{
			Listener.Prefixes.Add(listenUrl);
		}
	}

	protected HttpListener Listener { get; }

	public required HttpController HttpController { private get; [SetByIoC] init; }

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}

#endregion

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			StopListener();

			Listener.Close();
		}
	}

	protected override async Task ProtectedBackgroundProcess()
	{
		if (Listener == null)
		{
			return;
		}

		StartListener();

		while (!Token.IsCancellationRequested)
		{
			await HttpController.ReceiveAndProcessEvent(Listener, Token).ConfigureAwait(false);
		}

		StopListener();
	}

	protected virtual void StartListener() => Listener.Start();

	protected virtual void StopListener() => Listener.Stop();
}