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
using System.Net.Mime;
using System.Text;
using System.Threading;
using Xtate.DataTypes;
using Xtate.ExternalServices.HttpClient.Services;
using Xtate.Http;

namespace Xtate.Test.UnitTests.ExternalServices;

[TestClass]
public class HttpMimeTypeHandlerCoverageTest
{
	private static readonly DataModelList EmptyParameters = [];

	[TestMethod]
	public async Task MimeTypeBaseMatchesParametersAppendsAcceptAndReturnsDefaultResults()
	{
		var handler = new TestMimeTypeHandler();

		using var request = CreateHttpRequest();

		handler.PrepareRequest(request, contentType: null, EmptyParameters, DataModelValue.Undefined);
		Assert.IsNull(handler.TryCreateHttpContent(request, contentType: null, EmptyParameters, DataModelValue.Undefined));
		using var response = CreateHttpResponse(contentType: "text/plain", content: "ignored");
		Assert.IsNull(await handler.TryParseResponseAsync(response, EmptyParameters, CancellationToken.None));
	}

	[TestMethod]
	public async Task JsonHandlerPreparesCreatesAndParsesJsonContent()
	{
		var handler = new HttpClientJsonHandler();
		using var request = CreateHttpRequest();
		var value = new DataModelList { ["name"] = "value", ["number"] = 17 };

		handler.PrepareRequest(request, contentType: null, EmptyParameters, value);
		handler.PrepareRequest(request, contentType: null, EmptyParameters, value);
		Assert.AreEqual(expected: "application/json; q=1.0", string.Join(separator: ", ", request.Headers.Accept));
		Assert.IsNull(handler.TryCreateHttpContent(request, new ContentType("text/plain"), EmptyParameters, value));

		using var content = handler.TryCreateHttpContent(request, new ContentType("Application/Json; charset=utf-8"), EmptyParameters, value);
		Assert.IsInstanceOfType<JsonHttpContent>(content);
		Assert.AreEqual(expected: "application/json", content.Headers.ContentType!.MediaType);
		StringAssert.Contains(await content.ReadAsStringAsync(), substring: "\"name\":\"value\"");

		using var wrongResponse = CreateHttpResponse(contentType: "text/plain", content: "{}");
		Assert.IsNull(await handler.TryParseResponseAsync(wrongResponse, EmptyParameters, CancellationToken.None));
		using var response = CreateHttpResponse(contentType: "application/json; charset=utf-8", content: "{\"name\":\"parsed\",\"number\":19}");
		var parsed = await handler.TryParseResponseAsync(response, EmptyParameters, CancellationToken.None);
		Assert.IsTrue(parsed.HasValue);
		Assert.AreEqual(expected: "parsed", parsed.Value.AsList()["name"].AsString());
		Assert.AreEqual(expected: 19, parsed.Value.AsList()["number"].AsNumber());
	}

	[TestMethod]
	public async Task XmlHandlerRecognizesStandardAndStructuredXmlMediaTypes()
	{
		var handler = new HttpClientXmlHandler();
		using var request = CreateHttpRequest();
		var value = new DataModelList { ["name"] = "value" };

		handler.PrepareRequest(request, contentType: null, EmptyParameters, value);
		Assert.AreEqual(expected: "text/xml; q=1.0, application/xml; q=1.0", string.Join(separator: ", ", request.Headers.Accept));
		Assert.IsNull(handler.TryCreateHttpContent(request, contentType: null, EmptyParameters, value));
		Assert.IsNull(handler.TryCreateHttpContent(request, new ContentType("image/svg+xml"), EmptyParameters, value));

		using var applicationContent = handler.TryCreateHttpContent(request, new ContentType("application/xml"), EmptyParameters, value);
		using var textContent = handler.TryCreateHttpContent(request, new ContentType("text/xml; charset=utf-8"), EmptyParameters, value);
		Assert.IsInstanceOfType<XmlHttpContent>(applicationContent);
		Assert.IsInstanceOfType<XmlHttpContent>(textContent);
		StringAssert.Contains(await applicationContent.ReadAsStringAsync(), substring: "name");

		using var wrongResponse = CreateHttpResponse(contentType: "text/plain", content: "{}");
		Assert.IsNull(await handler.TryParseResponseAsync(wrongResponse, EmptyParameters, CancellationToken.None));
		using var response = CreateHttpResponse(contentType: "application/xml", content: "{\"name\":\"parsed\"}");
		var parsed = await handler.TryParseResponseAsync(response, EmptyParameters, CancellationToken.None);
		Assert.IsTrue(parsed.HasValue);
		Assert.AreEqual(expected: "parsed", parsed.Value.AsList()["name"].AsString());
	}

	[TestMethod]
	public async Task FormUrlEncodedHandlerCreatesObjectAndArrayFormsAndParsesRepeatedValues()
	{
		var handler = new HttpClientFormUrlEncodedHandler();
		using var request = CreateHttpRequest();
		var objectValue = new DataModelList { ["one"] = "1", ["empty"] = DataModelValue.Null };
		var arrayValue = new DataModelList
						 {
							 new DataModelList { ["name"] = "two", ["value"] = "2" },
							 new DataModelList { ["name"] = string.Empty, ["value"] = "ignored" },
							 new DataModelList { ["name"] = "missing", ["value"] = DataModelValue.Null }
						 };

		Assert.IsNull(handler.TryCreateHttpContent(request, new ContentType("text/plain"), EmptyParameters, objectValue));
		using var objectContent = handler.TryCreateHttpContent(request, new ContentType("application/x-www-form-urlencoded; charset=ascii"), EmptyParameters, objectValue);
		using var arrayContent = handler.TryCreateHttpContent(request, new ContentType("application/x-www-form-urlencoded"), EmptyParameters, arrayValue);
		Assert.IsInstanceOfType<FormUrlEncodedContent>(objectContent);
		Assert.IsNotNull(arrayContent);
		Assert.AreEqual(expected: "one=1", await objectContent.ReadAsStringAsync());
		Assert.AreEqual(expected: "two=2", await arrayContent.ReadAsStringAsync());

		using var wrongResponse = CreateHttpResponse(contentType: "text/plain", content: "one=1");
		Assert.IsNull(await handler.TryParseResponseAsync(wrongResponse, EmptyParameters, CancellationToken.None));
		using var response = CreateHttpResponse(contentType: "application/x-www-form-urlencoded", content: "one=1&one=2&encoded=hello+world&=ignored");
		var parsed = await handler.TryParseResponseAsync(response, EmptyParameters, CancellationToken.None);
		Assert.IsTrue(parsed.HasValue);
		var list = parsed.Value.AsList();
		Assert.AreEqual(expected: 2, list.KeyValues.Count(static pair => pair.Key == "one"));
		Assert.AreEqual(expected: "hello world", list["encoded"].AsString());
	}

	private static HttpRequestMessage CreateHttpRequest() => new(HttpMethod.Get, requestUri: "https://example.test/");

	private static HttpResponseMessage CreateHttpResponse(string contentType, string content)
	{
		var response = new HttpResponseMessage { Content = new StringContent(content, Encoding.UTF8) };
		response.Content.Headers.Remove("Content-Type");
		response.Content.Headers.TryAddWithoutValidation(name: "Content-Type", contentType);

		return response;
	}

	private sealed class TestMimeTypeHandler : HttpClientMimeTypeHandler
	{
		public override void PrepareRequest(HttpRequestMessage request,
											ContentType? contentType,
											DataModelList parameters,
											DataModelValue value) { }

		public override HttpContent? TryCreateHttpContent(HttpRequestMessage request,
														  ContentType? contentType,
														  DataModelList parameters,
														  DataModelValue value) =>
			null;

		public override ValueTask<DataModelValue?> TryParseResponseAsync(HttpResponseMessage response, DataModelList parameters, CancellationToken token) => default;
	}
}