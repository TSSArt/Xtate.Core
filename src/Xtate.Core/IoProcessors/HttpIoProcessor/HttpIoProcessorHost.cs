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

using System.Net;

namespace Xtate.IoProcessor;

internal sealed class HttpIoProcessorHost : HttpIoProcessorHostBase<HttpIoProcessorHost, HttpListenerContext>, IDisposable
{
	private const int FreeSlotsCount = 2;

	private readonly HttpListener _httpListener;

	public HttpIoProcessorHost(IPEndPoint ipEndPoint)
	{
		_httpListener = new HttpListener();

		if (ipEndPoint.Address.Equals(IPAddress.Any) || ipEndPoint.Address.Equals(IPAddress.IPv6Any))
		{
			_httpListener.Prefixes.Add(@$"http://*:{ipEndPoint.Port}/");
		}
		else
		{
			_httpListener.Prefixes.Add(@$"http://{ipEndPoint}/");
		}
	}

#region Interface IDisposable

	public void Dispose() => ((IDisposable) _httpListener).Dispose();

#endregion

	private async void HandleRequest(HttpListenerContext context)
	{
		foreach (var processor in Processors)
		{
			if (await processor.Handle(context, token: default).ConfigureAwait(false))
			{
				context.Response.StatusCode = (int) HttpStatusCode.NoContent;
				context.Response.Close();

				return;
			}
		}

		context.Response.StatusCode = (int) HttpStatusCode.NotFound;
		context.Response.Close();
	}

	protected override ValueTask StartHost(CancellationToken token)
	{
		_httpListener.Start();

		for (var i = 0; i < FreeSlotsCount; i ++)
		{
			StartWaiter();
		}

		return default;
	}

	private void StartWaiter()
	{
		_httpListener.GetContextAsync()
					 .ContinueWith(
						 task =>
						 {
							 StartWaiter();
							 HandleRequest(task.Result);
						 },
						 TaskContinuationOptions.OnlyOnRanToCompletion);
	}

	protected override ValueTask StopHost(CancellationToken token)
	{
		_httpListener.Stop();

		return default;
	}
}