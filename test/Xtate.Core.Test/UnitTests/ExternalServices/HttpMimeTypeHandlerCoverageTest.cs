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
using System.Text;
using System.Threading;
using Xtate.DataTypes;
using Xtate.ExternalServices.HttpClient.Internal;
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
		string? accept = null;

		Assert.IsFalse(handler.EqualsContentType(contentTypeA: null, "application/json"));
		Assert.IsFalse(handler.EqualsContentType(string.Empty, "application/json"));
		Assert.IsTrue(handler.EqualsContentType("Application/Json; charset=utf-8", "application/json; q=1"));
		Assert.IsFalse(handler.EqualsContentType("application/json", "text/plain"));
		Assert.IsFalse(handler.EqualsContentType("application/json", "application/problem+json"));

		handler.Append(ref accept, "application/json");
		Assert.AreEqual("application/json", accept);
		handler.Append(ref accept, "APPLICATION/JSON");
		Assert.AreEqual("application/json", accept);
		accept = "text/plain";
		handler.Append(ref accept, "application/xml");
		Assert.AreEqual("text/plain, application/xml", accept);
		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => handler.Append(ref accept, string.Empty));

		var request = new TestWebRequest();
		handler.PrepareRequest(request, contentType: null, EmptyParameters, DataModelValue.Undefined);
		Assert.IsNull(handler.TryCreateHttpContent(request, contentType: null, EmptyParameters, DataModelValue.Undefined));
		await using var response = new TestWebResponse("text/plain", "ignored");
		Assert.IsNull(await handler.TryParseResponseAsync(response, EmptyParameters, CancellationToken.None));
	}

	[TestMethod]
	public void MimeTypeBaseAppendsAfterParameterizedNonMatchingAcceptEntry()
	{
		// Current product defect: enabling this exact case never returns. The state-1 delimiter branch must advance
		// to the next character/state instead of jumping back into case 1 for the same character.
		var handler = new TestMimeTypeHandler();
		var accept = "text/plain; q=0.5, application/json";

		handler.Append(ref accept, "application/xml");

		Assert.AreEqual("text/plain; q=0.5, application/json, application/xml", accept);
	}

	[TestMethod]
	public async Task JsonHandlerPreparesCreatesAndParsesJsonContent()
	{
		var handler = HttpClientJsonHandler.Instance;
		var request = CreateHttpRequest();
		var value = new DataModelList { ["name"] = "value", ["number"] = 17 };

		handler.PrepareRequest(request, contentType: null, EmptyParameters, value);
		handler.PrepareRequest(request, contentType: null, EmptyParameters, value);
		Assert.AreEqual("application/json", request.Accept);
		Assert.IsNull(handler.TryCreateHttpContent(request, "text/plain", EmptyParameters, value));

		using var content = handler.TryCreateHttpContent(request, "Application/Json; charset=utf-8", EmptyParameters, value);
		Assert.IsInstanceOfType<JsonHttpContent>(content);
		Assert.AreEqual("application/json", content.Headers.ContentType!.MediaType);
		StringAssert.Contains(await content.ReadAsStringAsync(), "\"name\":\"value\"");

		await using var wrongResponse = new TestWebResponse("text/plain", "{}");
		Assert.IsNull(await handler.TryParseResponseAsync(wrongResponse, EmptyParameters, CancellationToken.None));
		await using var response = new TestWebResponse("application/json; charset=utf-8", "{\"name\":\"parsed\",\"number\":19}");
		var parsed = await handler.TryParseResponseAsync(response, EmptyParameters, CancellationToken.None);
		Assert.IsTrue(parsed.HasValue);
		Assert.AreEqual("parsed", parsed.Value.AsList()["name"].AsString());
		Assert.AreEqual(expected: 19, parsed.Value.AsList()["number"].AsNumber());
		request.Abort();
	}

	[TestMethod]
	public async Task XmlHandlerRecognizesStandardAndStructuredXmlMediaTypes()
	{
		var handler = HttpClientXmlHandler.Instance;
		var request = CreateHttpRequest();
		var value = new DataModelList { ["name"] = "value" };

		handler.PrepareRequest(request, contentType: null, EmptyParameters, value);
		Assert.AreEqual("application/xml", request.Accept);
		Assert.IsNull(handler.TryCreateHttpContent(request, contentType: null, EmptyParameters, value));
		Assert.IsNull(handler.TryCreateHttpContent(request, "image/svg+xml", EmptyParameters, value));

		using var applicationContent = handler.TryCreateHttpContent(request, "application/xml", EmptyParameters, value);
		using var textContent = handler.TryCreateHttpContent(request, "text/xml; charset=utf-8", EmptyParameters, value);
		using var structuredContent = handler.TryCreateHttpContent(request, "application/problem+xml", EmptyParameters, value);
		Assert.IsInstanceOfType<XmlHttpContent>(applicationContent);
		Assert.IsInstanceOfType<XmlHttpContent>(textContent);
		Assert.IsInstanceOfType<XmlHttpContent>(structuredContent);
		StringAssert.Contains(await applicationContent.ReadAsStringAsync(), "name");

		await using var wrongResponse = new TestWebResponse("text/plain", "{}");
		Assert.IsNull(await handler.TryParseResponseAsync(wrongResponse, EmptyParameters, CancellationToken.None));
		await using var response = new TestWebResponse("application/problem+xml", "{\"name\":\"parsed\"}");
		var parsed = await handler.TryParseResponseAsync(response, EmptyParameters, CancellationToken.None);
		Assert.IsTrue(parsed.HasValue);
		Assert.AreEqual("parsed", parsed.Value.AsList()["name"].AsString());
		request.Abort();
	}

	[TestMethod]
	public async Task FormUrlEncodedHandlerCreatesObjectAndArrayFormsAndParsesRepeatedValues()
	{
		var handler = HttpClientFormUrlEncodedHandler.Instance;
		var request = new TestWebRequest();
		var objectValue = new DataModelList { ["one"] = "1", ["empty"] = DataModelValue.Null };
		var arrayValue = new DataModelList
						 {
							 new DataModelList { ["name"] = "two", ["value"] = "2" },
							 new DataModelList { ["name"] = string.Empty, ["value"] = "ignored" },
							 new DataModelList { ["name"] = "missing", ["value"] = DataModelValue.Null }
						 };

		Assert.IsNull(handler.TryCreateHttpContent(request, "text/plain", EmptyParameters, objectValue));
		using var objectContent = handler.TryCreateHttpContent(request, "application/x-www-form-urlencoded; charset=ascii", EmptyParameters, objectValue);
		using var arrayContent = handler.TryCreateHttpContent(request, "application/x-www-form-urlencoded", EmptyParameters, arrayValue);
		Assert.IsInstanceOfType<FormUrlEncodedContent>(objectContent);
		Assert.IsNotNull(arrayContent);
		Assert.AreEqual("one=1", await objectContent.ReadAsStringAsync());
		Assert.AreEqual("two=2", await arrayContent.ReadAsStringAsync());

		await using var wrongResponse = new TestWebResponse("text/plain", "one=1");
		Assert.IsNull(await handler.TryParseResponseAsync(wrongResponse, EmptyParameters, CancellationToken.None));
		await using var response = new TestWebResponse("application/x-www-form-urlencoded", "one=1&one=2&encoded=hello+world&=ignored");
		var parsed = await handler.TryParseResponseAsync(response, EmptyParameters, CancellationToken.None);
		Assert.IsTrue(parsed.HasValue);
		var list = parsed.Value.AsList();
		Assert.AreEqual(expected: 2, list.KeyValues.Count(static pair => pair.Key == "one"));
		Assert.AreEqual("hello world", list["encoded"].AsString());
	}

	private static HttpWebRequest CreateHttpRequest()
	{
#pragma warning disable SYSLIB0014
		return WebRequest.CreateHttp("https://example.test/");
#pragma warning restore SYSLIB0014
	}

	private sealed class TestMimeTypeHandler : HttpClientMimeTypeHandler
	{
		public bool EqualsContentType(string? contentTypeA, string? contentTypeB) => ContentTypeEquals(contentTypeA, contentTypeB);

		public void Append(ref string? accept, string contentType) => AppendAcceptHeader(ref accept, contentType);
	}

#pragma warning disable SYSLIB0014
	private sealed class TestWebRequest : WebRequest;
#pragma warning restore SYSLIB0014

	private sealed class TestWebResponse(string contentType, string content) : WebResponse, IAsyncDisposable
	{
		private readonly MemoryStream _stream = new(Encoding.UTF8.GetBytes(content));

		public override string ContentType { get; set; } = contentType;

		public override Stream GetResponseStream() => _stream;

		public ValueTask DisposeAsync()
		{
			Dispose();
			return ValueTask.CompletedTask;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_stream.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
