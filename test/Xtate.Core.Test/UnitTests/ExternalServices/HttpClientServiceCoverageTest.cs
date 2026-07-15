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

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Xtate.DataTypes;
using Xtate.ExternalServices.HttpClient.Internal;
using Xtate.ExternalServices.HttpClient.Services;
using Xtate.IoC.Tools;
using Xtate.StateMachineHost;
using Xtate.TaskMonitor;

namespace Xtate.Test.UnitTests.ExternalServices;

[TestClass]
public class HttpClientServiceCoverageTest
{
	[TestMethod]
	public async Task PostUsesHandlersHeadersCookiesAndProjectsSuccessfulResponse()
	{
		await using var server = new HttpLoopbackServer(
			"HTTP/1.1 201 Created\r\nContent-Type: application/custom\r\nX-Result: first\r\nX-Result: second\r\nContent-Length: 7\r\nConnection: close\r\n\r\nignored");
		var firstHandler = new RecordingMimeTypeHandler();
		using var requestContent = new StringContent("handler-content", Encoding.UTF8, "application/custom");
		var secondHandler = new RecordingMimeTypeHandler
							{
								RequestContent = requestContent,
								ResponseContent = new DataModelValue("parsed-response")
							};
		var options = HttpClientServiceOptions.CreateDefault();
		options.MimeTypeHandlers.Clear();
		options.MimeTypeHandlers.Add(firstHandler);
		options.MimeTypeHandlers.Add(secondHandler);
		var expires = new DateTime(2030, 1, 2, 3, 4, 5, DateTimeKind.Utc);
		var parameters = new DataModelList
						 {
							 ["method"] = "POST",
							 ["autoRedirect"] = false,
							 ["accept"] = "application/custom",
							 ["contentType"] = "application/custom",
							 ["headers"] = new DataModelList
										   {
											   ["X-Request"] = "object-header",
											   ["X-Skipped"] = DataModelValue.Null
										   },
							 ["cookies"] = new DataModelList
										   {
											   new DataModelList
											   {
												   ["name"] = "session",
												   ["value"] = "abc",
												   ["path"] = "/",
												   ["domain"] = "127.0.0.1",
												   ["expires"] = expires,
												   ["httpOnly"] = true,
												   ["secure"] = false
											   }
										   }
						 };
		var service = CreateService(options, server.Uri, parameters, new DataModelValue("original-content"));

		var result = (await ((IExternalService) service).GetResult()).AsList();
		var request = await server.Request;

		Assert.AreEqual("POST", request.Method);
		Assert.AreEqual("/service", request.Path);
		Assert.AreEqual("handler-content", request.Body);
		Assert.Contains("X-Request: object-header", request.Headers);
		Assert.Contains("Accept: application/custom", request.Headers);
		Assert.Contains("Cookie: session=abc", request.Headers);
		Assert.AreEqual(expected: 1, firstHandler.PrepareCount);
		Assert.AreEqual(expected: 1, firstHandler.CreateCount);
		Assert.AreEqual(expected: 1, firstHandler.ParseCount);
		Assert.AreEqual(expected: 1, secondHandler.PrepareCount);
		Assert.AreEqual(expected: 1, secondHandler.CreateCount);
		Assert.AreEqual(expected: 1, secondHandler.ParseCount);
		Assert.AreEqual(expected: 201, result["statusCode"].AsNumber());
		Assert.AreEqual("Created", result["statusDescription"].AsString());
		Assert.IsNull(result["webExceptionStatus"].AsStringOrDefault());
		Assert.AreEqual("parsed-response", result["content"].AsString());
		var responseHeader = result["headers"].AsList().Single(item => item.AsList()["name"].AsString() == "X-Result").AsList();
		Assert.Contains("first", responseHeader["value"].AsString());
		Assert.Contains("second", responseHeader["value"].AsString());
		var responseCookie = result["cookies"].AsList().Single().AsList();
		Assert.AreEqual("session", responseCookie["name"].AsString());
		Assert.AreEqual("abc", responseCookie["value"].AsString());
		Assert.AreEqual("/", responseCookie["path"].AsString());
		Assert.AreEqual("127.0.0.1", responseCookie["domain"].AsString());
		Assert.IsTrue(responseCookie["httpOnly"].AsBoolean());
		Assert.IsFalse(responseCookie["secure"].AsBoolean());
		Assert.AreEqual(expires, responseCookie["expires"].AsDateTime().ToDateTime());
	}

	[TestMethod]
	public async Task ProtocolErrorUsesDefaultContentArrayHeadersAndEmptyResponseCollections()
	{
		await using var server = new HttpLoopbackServer(
			"HTTP/1.1 422 Unprocessable Entity\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
		var options = HttpClientServiceOptions.CreateDefault();
		options.MimeTypeHandlers.Clear();
		var parameters = new DataModelList
						 {
							 ["method"] = "PUT",
							 ["headers"] = new DataModelList
										   {
											   new DataModelList { ["name"] = "X-Array", ["value"] = "array-header" },
											   new DataModelList { ["name"] = string.Empty, ["value"] = "ignored" },
											   new DataModelList { ["name"] = "X-Null", ["value"] = DataModelValue.Null }
										   }
						 };
		var service = CreateService(options, server.Uri, parameters, new DataModelValue("default-content"));

		var result = (await ((IExternalService) service).GetResult()).AsList();
		var request = await server.Request;

		Assert.AreEqual("PUT", request.Method);
		Assert.AreEqual("default-content", request.Body);
		Assert.Contains("X-Array: array-header", request.Headers);
		Assert.AreEqual(expected: 422, result["statusCode"].AsNumber());
		Assert.AreEqual("Unprocessable Entity", result["statusDescription"].AsString());
		Assert.AreEqual("ProtocolError", result["webExceptionStatus"].AsString());
		Assert.AreEqual(expected: 0, result["cookies"].AsList().Count);
		Assert.IsTrue(result["content"].IsUndefined());
	}

	[TestMethod]
	public async Task ConnectionClosedWithoutResponseReturnsEmptyResult()
	{
		await using var server = new HttpLoopbackServer(response: null);
		var options = HttpClientServiceOptions.CreateDefault();
		options.MimeTypeHandlers.Clear();
		var service = CreateService(
			options,
			server.Uri,
			new DataModelList { ["method"] = "POST" },
			DataModelValue.Undefined);

		var result = (await ((IExternalService) service).GetResult()).AsList();
		await server.Request;

		Assert.AreEqual(expected: 0, result["statusCode"].AsNumber());
		Assert.IsNull(result["statusDescription"].AsStringOrDefault());
		Assert.IsNull(result["webExceptionStatus"].AsStringOrDefault());
		Assert.AreEqual(expected: 0, result["headers"].AsList().Count);
		Assert.AreEqual(expected: 0, result["cookies"].AsList().Count);
		Assert.IsTrue(result["content"].IsUndefined());
	}

	[TestMethod]
	public async Task DefaultGetMethodCanExecuteWithoutRequestContent()
	{
		// Current product defect: WriteContent must skip opening the request stream for methods that do not permit
		// an entity body. Retain this test so the documented default GET behavior can be enabled after that fix.
		await using var server = new HttpLoopbackServer(
			"HTTP/1.1 204 No Content\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
		var options = HttpClientServiceOptions.CreateDefault();
		options.MimeTypeHandlers.Clear();
		var service = CreateService(options, server.Uri, new DataModelList(), DataModelValue.Undefined);

		var result = (await ((IExternalService) service).GetResult()).AsList();

		Assert.AreEqual(expected: 204, result["statusCode"].AsNumber());
	}

	private static HttpClientService CreateService(
		HttpClientServiceOptions options,
		Uri source,
		DataModelValue parameters,
		DataModelValue content)
	{
		var monitor = new PassThroughTaskMonitor();

		return new HttpClientService(options)
			   {
				   ExternalServiceSourceBase = new ExternalServiceSource(source, content),
				   ExternalServiceParametersBase = new ExternalServiceParameters(parameters),
				   TaskMonitorBase = monitor,
				   DisposeTokenBase = new DisposeToken(CancellationToken.None)
			   };
	}

	private sealed class RecordingMimeTypeHandler : HttpClientMimeTypeHandler
	{
		public HttpContent? RequestContent { get; init; }

		public DataModelValue? ResponseContent { get; init; }

		public int PrepareCount { get; private set; }

		public int CreateCount { get; private set; }

		public int ParseCount { get; private set; }

		public override void PrepareRequest(WebRequest webRequest, string? contentType, DataModelList parameters, DataModelValue value) => PrepareCount ++;

		public override HttpContent? TryCreateHttpContent(WebRequest webRequest, string? contentType, DataModelList parameters, DataModelValue value)
		{
			CreateCount ++;

			return RequestContent;
		}

		public override ValueTask<DataModelValue?> TryParseResponseAsync(WebResponse webResponse, DataModelList parameters, CancellationToken token)
		{
			ParseCount ++;

			return new ValueTask<DataModelValue?>(ResponseContent);
		}
	}

	private sealed class ExternalServiceSource(Uri source, DataModelValue content) : IExternalServiceSource
	{
		public Uri Source { get; } = source;

		public string? RawContent => null;

		public DataModelValue Content { get; } = content;
	}

	private sealed class ExternalServiceParameters(DataModelValue parameters) : IExternalServiceParameters
	{
		public DataModelValue Parameters { get; } = parameters;
	}

	private sealed class PassThroughTaskMonitor : ITaskMonitor
	{
		public Task WaitAsync(Task task, CancellationToken token) => task;

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task;

		public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => valueTask;

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) => valueTask;

		public void Forget(Task task) { }

		public void Forget(ValueTask valueTask) { }

		public void Forget<TResult>(ValueTask<TResult> valueTask) { }
	}

	private sealed class HttpLoopbackServer : IAsyncDisposable
	{
		private readonly TcpListener _listener = new(IPAddress.Loopback, port: 0);

		private readonly Task<HttpRequestData> _request;

		public HttpLoopbackServer(string? response)
		{
			_listener.Start();
			var endpoint = (IPEndPoint) _listener.LocalEndpoint;
			Uri = new Uri($"http://127.0.0.1:{endpoint.Port}/service");
			_request = Serve(response);
		}

		public Uri Uri { get; }

		public Task<HttpRequestData> Request => _request;

		public async ValueTask DisposeAsync()
		{
			_listener.Stop();

			try
			{
				await _request.ConfigureAwait(false);
			}
			catch (SocketException)
			{
				// Stopping a listener that was never reached is expected during assertion failures.
			}
		}

		private async Task<HttpRequestData> Serve(string? response)
		{
			using var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
			await using var stream = client.GetStream();
			var request = await ReadRequest(stream).ConfigureAwait(false);

			if (response is not null)
			{
				var responseBytes = Encoding.ASCII.GetBytes(response);
				await stream.WriteAsync(responseBytes).ConfigureAwait(false);
			}
			else
			{
				// HttpWebRequest can retry when a connection closes before returning a status line. Stop accepting
				// connections before closing this client so a retry fails instead of waiting in the listener backlog.
				_listener.Stop();
			}

			return request;
		}

		private static async Task<HttpRequestData> ReadRequest(Stream stream)
		{
			using var request = new MemoryStream();
			var buffer = new byte[4096];
			var headerEnd = -1;
			var contentLength = 0;

			while (true)
			{
				var read = await stream.ReadAsync(buffer).ConfigureAwait(false);

				if (read == 0)
				{
					break;
				}

				request.Write(buffer, offset: 0, read);
				var bytes = request.ToArray();

				if (headerEnd < 0 && FindHeaderEnd(bytes) is var end and >= 0)
				{
					headerEnd = end;
					var headers = Encoding.ASCII.GetString(bytes, index: 0, headerEnd);
					contentLength = GetContentLength(headers);
				}

				if (headerEnd >= 0 && request.Length >= headerEnd + 4 + contentLength)
				{
					break;
				}
			}

			var data = request.ToArray();
			var headerText = Encoding.ASCII.GetString(data, index: 0, headerEnd);
			var firstLineEnd = headerText.IndexOf("\r\n", StringComparison.Ordinal);
			var requestLine = headerText[..firstLineEnd].Split(' ');
			var body = contentLength > 0 ? Encoding.UTF8.GetString(data, headerEnd + 4, contentLength) : string.Empty;

			return new HttpRequestData(requestLine[0], requestLine[1], headerText, body);
		}

		private static int FindHeaderEnd(byte[] bytes)
		{
			for (var i = 0; i <= bytes.Length - 4; i ++)
			{
				if (bytes[i] == '\r' && bytes[i + 1] == '\n' && bytes[i + 2] == '\r' && bytes[i + 3] == '\n')
				{
					return i;
				}
			}

			return -1;
		}

		private static int GetContentLength(string headers)
		{
			foreach (var line in headers.Split("\r\n"))
			{
				const string prefix = "Content-Length:";

				if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				{
					return int.Parse(line[prefix.Length..].Trim());
				}
			}

			return 0;
		}
	}

	private sealed record HttpRequestData(string Method, string Path, string Headers, string Body);
}
