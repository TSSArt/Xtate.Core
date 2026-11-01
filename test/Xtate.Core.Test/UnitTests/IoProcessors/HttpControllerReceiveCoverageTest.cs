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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Xtate.DataModel;
using Xtate.IoC.Options;
using Xtate.IoProcessors.Http;
using Xtate.IoProcessors.Http.Services;
using Xtate.StateMachine;
using Xtate.StateMachineHost;

namespace Xtate.Core.Test.UnitTests.IoProcessors;

[TestClass]
public class HttpControllerReceiveCoverageTest
{
	private static readonly HttpClient SharedHttpClient = new();

	[TestMethod]
	[DataRow("empty", "POST")]
	[DataRow("text", "query.event")]
	[DataRow("form", "form.event")]
	[DataRow("form-query", "query.event")]
	[DataRow("json", "POST")]
	[DataRow("xml", "POST")]
	public async Task ReceiveDecodesPayloadAndRoutesMetadata(string kind, string expectedName)
	{
		var dispatcher = new Mock<IExternalEventDispatcher<HttpIoProcessor>>();
		IIncomingEvent? received = null;
		ServiceId? destination = null;
		dispatcher.Setup(value => value.Dispatch(It.IsAny<ServiceId>(), It.IsAny<IIncomingEvent>(), It.IsAny<CancellationToken>()))
			.Callback<ServiceId, IIncomingEvent, CancellationToken>((id, message, _) => { destination = id; received = message; })
			.Returns(default(ValueTask));
		await Exchange(kind, dispatcher.Object, HttpStatusCode.OK);
		Assert.IsNotNull(received);
		Assert.AreEqual(expectedName, received.Name.ToString());
		Assert.AreEqual("receiver", destination!.ToString());
		Assert.AreEqual(EventType.External, received.Type);
		Assert.AreEqual("http://sender.example/", received.Origin!.ToString());
		Assert.AreEqual("send-id", received.SendId!.ToString());
		Assert.AreEqual("invoke-id", received.InvokeId!.ToString());
		switch (kind)
		{
			case "empty": Assert.IsTrue(received.Data.IsUndefined()); break;
			case "text": Assert.AreEqual("hello", received.Data.AsString()); break;
			case "form":
			case "form-query": Assert.AreEqual("hello world", received.Data.AsList()["value"].AsString()); break;
			case "json": Assert.AreEqual("hello", received.Data.AsList()["value"].AsString()); break;
			case "xml": Assert.IsFalse(received.Data.IsUndefined()); break;
		}
	}

	[TestMethod]
	[DataRow("unsupported", HttpStatusCode.UnsupportedMediaType)]
	[DataRow("oversize", HttpStatusCode.RequestEntityTooLarge)]
	[DataRow("invalid-json", HttpStatusCode.InternalServerError)]
	[DataRow("invalid-target", HttpStatusCode.InternalServerError)]
	public async Task InvalidRequestsReturnErrorWithoutDispatch(string kind, HttpStatusCode status)
	{
		var dispatcher = new Mock<IExternalEventDispatcher<HttpIoProcessor>>(MockBehavior.Strict);
		await Exchange(kind, dispatcher.Object, status);
		dispatcher.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task DispatcherFailureReturnsServerError()
	{
		var dispatcher = new Mock<IExternalEventDispatcher<HttpIoProcessor>>();
		dispatcher.Setup(value => value.Dispatch(It.IsAny<ServiceId>(), It.IsAny<IIncomingEvent>(), It.IsAny<CancellationToken>()))
			.Throws(new InvalidOperationException("dispatch failed"));
		await Exchange("empty", dispatcher.Object, HttpStatusCode.InternalServerError);
	}

	private static async Task Exchange(string kind, IExternalEventDispatcher<HttpIoProcessor> dispatcher, HttpStatusCode expected)
	{
		var reservation = new TcpListener(IPAddress.Loopback, 0);
		reservation.Start();
		var port = ((IPEndPoint)reservation.LocalEndpoint).Port;
		reservation.Stop();
		var baseUrl = $"http://localhost:{port}/";
		using var listener = new HttpListener();
		listener.Prefixes.Add(baseUrl);
		listener.Start();
		var options = new HttpIoProcessorOptions { PublicBaseUrl = baseUrl, MaxMessageSize = kind == "oversize" ? 2 : 4096 };
		var controller = new HttpController(Mock.Of<IOptions<HttpIoProcessorOptions>>(value => value.Value == options))
		{
			ExternalEventDispatcher = dispatcher,
			HttpClientFactory = () => new HttpClient()
		};
		using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		var receive = controller.ReceiveAndProcessEvent(listener, cancellation.Token).AsTask();
		var path = kind == "invalid-target" ? "unknown/receiver" : "session/receiver";
		if (kind is "text" or "form-query") path += "?_scxmleventname=query.event";
		if (kind == "oversize") path += "?Content-Length=10";
		using var request = new HttpRequestMessage(HttpMethod.Post, baseUrl + path);
		request.Headers.Add("Origin", "http://sender.example/");
		request.Headers.Add("SCXML-SendId", "send-id");
		request.Headers.Add("SCXML-InvokeId", "invoke-id");
		request.Content = kind switch
		{
			"empty" or "invalid-target" => null,
			"form" or "form-query" => new StringContent("_scxmleventname=form.event&value=hello+world", Encoding.UTF8, "application/x-www-form-urlencoded"),
			"json" => new StringContent("{\"value\":\"hello\"}", Encoding.UTF8, "application/json"),
			"invalid-json" => new StringContent("{", Encoding.UTF8, "application/json"),
			"xml" => new StringContent("<root>hello</root>", Encoding.UTF8, "text/xml"),
			"unsupported" => new StringContent("hello", Encoding.UTF8, "application/octet-stream"),
			_ => new StringContent("hello", Encoding.UTF8, "text/plain")
		};
		using var response = await SharedHttpClient.SendAsync(request, cancellation.Token);
		await receive;
		Assert.AreEqual(expected, response.StatusCode);
	}
}
