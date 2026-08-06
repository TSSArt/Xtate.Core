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
using System.Net.Mime;
using System.Threading;
using Xtate.DataTypes;
using Xtate.ExternalServices.HttpClient.Services;
using Xtate.IoC.Tools;
using Xtate.StateMachineHost;
using Xtate.TaskMonitor;

namespace Xtate.Core.Test.UnitTests.ExternalServices;

[TestClass]
public class HttpClientServiceCoverageTest
{
	[TestMethod]
	public async Task ServiceBuildsRequestUsesMimeHandlersAndReturnsStructuredResponse()
	{
		var parameters = new DataModelList
						 {
							 ["method"] = "post",
							 ["accept"] = "application/json",
							 ["contentType"] = "application/custom",
							 ["headers"] = new DataModelList
										   {
											   ["X-Test"] = "one",
											   ["X-Skip"] = DataModelValue.Null
										   }
						 };
		var responseData = new DataModelList { ["parsed"] = true };
		var mimeHandler = new RecordingMimeHandler(responseData);
		var transport = new RecordingMessageHandler(request =>
													{
														Assert.AreEqual(HttpMethod.Post, request.Method);
														Assert.AreEqual(expected: "one", request.Headers.GetValues("X-Test").Single());
														Assert.AreEqual(expected: "application/json", request.Headers.Accept.Single().MediaType);
														Assert.AreEqual(expected: "mime-content", request.Content!.ReadAsStringAsync().GetAwaiter().GetResult());

														var response = new HttpResponseMessage(HttpStatusCode.Accepted) { ReasonPhrase = "Accepted by test", Content = new StringContent("ignored") };
														response.Headers.TryAddWithoutValidation(name: "X-Response", ["first", "second"]);

														return response;
													});
		using var httpClient = new HttpClient(transport);
		var service = CreateService(httpClient, [mimeHandler], parameters, content: "body");

		var result = (await ((IExternalService)service).GetResult()).AsList();

		Assert.AreEqual(expected: 202, result["statusCode"].AsNumber().ToInt32());
		Assert.AreEqual(expected: "Accepted by test", result["statusDescription"].AsString());
		Assert.IsNull(result["webExceptionStatus"].AsStringOrDefault());
		Assert.IsTrue(result["content"].AsList()["parsed"].AsBoolean());
		var responseHeader = result["headers"].AsList()[0].AsList();
		Assert.AreEqual(expected: "X-Response", responseHeader["name"].AsString());
		Assert.AreEqual(expected: "first, second", responseHeader["value"].AsString());
		Assert.AreEqual(expected: 1, mimeHandler.PrepareCalls);
		Assert.AreEqual(expected: 1, mimeHandler.CreateCalls);
		Assert.AreEqual(expected: 1, mimeHandler.ParseCalls);
	}

	[TestMethod]
	public async Task ServiceUsesArrayHeadersDefaultContentAndUndefinedParserFallback()
	{
		var headers = new DataModelList
					  {
						  new DataModelList { ["name"] = "X-Array", ["value"] = "array-value" },
						  new DataModelList { ["name"] = string.Empty, ["value"] = "ignored" },
						  new DataModelList { ["name"] = "X-Null", ["value"] = DataModelValue.Null }
					  };
		var parameters = new DataModelList { ["headers"] = headers };
		var mimeHandler = new RecordingMimeHandler(result: null);
		var transport = new RecordingMessageHandler(request =>
													{
														Assert.AreEqual(HttpMethod.Get, request.Method);
														Assert.AreEqual(expected: "array-value", request.Headers.GetValues("X-Array").Single());
														Assert.AreEqual(expected: "fallback body", request.Content!.ReadAsStringAsync().GetAwaiter().GetResult());

														return new HttpResponseMessage(HttpStatusCode.NoContent);
													});
		using var httpClient = new HttpClient(transport);
		var service = CreateService(httpClient, [mimeHandler], parameters, content: "fallback body");

		var result = (await ((IExternalService)service).GetResult()).AsList();

		Assert.AreEqual(expected: 204, result["statusCode"].AsNumber().ToInt32());
		Assert.AreEqual(DataModelValueType.Undefined, result["content"].Type);
		Assert.AreSame(DataModelList.Empty, result["headers"].AsList());
	}

	[TestMethod]
	public async Task ServiceConvertsHttpRequestExceptionsIntoResponseStatus()
	{
		var transport = new RecordingMessageHandler(_ => throw new HttpRequestException("transport failed"));
		using var httpClient = new HttpClient(transport);
		var service = CreateService(httpClient, [], new DataModelList(), DataModelValue.Undefined);

		var result = (await ((IExternalService)service).GetResult()).AsList();

		Assert.AreEqual(expected: "transport failed", result["webExceptionStatus"].AsString());
		Assert.AreSame(DataModelList.Empty, result["headers"].AsList());
	}

	private static HttpClientService CreateService(HttpClient httpClient,
												   IList<HttpClientMimeTypeHandler> mimeTypeHandlers,
												   DataModelValue parameters,
												   DataModelValue content) =>
		new()
		{
			HttpClient = httpClient,
			MimeTypeHandlers = mimeTypeHandlers,
			ExternalServiceSourceBase = Mock.Of<IExternalServiceSource>(source => source.Source == new Uri("https://example.test/service") && source.Content == content),
			ExternalServiceParametersBase = Mock.Of<IExternalServiceParameters>(source => source.Parameters == parameters),
			DisposeTokenBase = new DisposeToken(CancellationToken.None),
			TaskMonitorBase = new ImmediateTaskMonitor()
		};

	private sealed class RecordingMimeHandler(DataModelValue? result) : HttpClientMimeTypeHandler
	{
		public int PrepareCalls { get; private set; }

		public int CreateCalls { get; private set; }

		public int ParseCalls { get; private set; }

		public override void PrepareRequest(HttpRequestMessage request,
											ContentType? contentType,
											DataModelList parameters,
											DataModelValue value)
		{
			PrepareCalls ++;

			if (contentType is not null)
			{
				Assert.AreEqual(expected: "application/custom", contentType.MediaType);
			}
		}

		public override HttpContent? TryCreateHttpContent(HttpRequestMessage request,
														  ContentType? contentType,
														  DataModelList parameters,
														  DataModelValue value)
		{
			CreateCalls ++;

			return contentType?.MediaType == "application/custom" ? new StringContent("mime-content") : null;
		}

		public override ValueTask<DataModelValue?> TryParseResponseAsync(HttpResponseMessage response, DataModelList parameters, CancellationToken token)
		{
			ParseCalls ++;

			return new ValueTask<DataModelValue?>(result);
		}
	}

	private sealed class RecordingMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(responseFactory(request));
	}

	private sealed class ImmediateTaskMonitor : ITaskMonitor
	{
	#region Interface ITaskMonitor

		public Task WaitAsync(Task task, CancellationToken token) => task;

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task;

		public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => valueTask;

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) => valueTask;

		public void Forget(Task task) { }

		public void Forget(ValueTask valueTask) { }

		public void Forget<TResult>(ValueTask<TResult> valueTask) { }

	#endregion
	}
}