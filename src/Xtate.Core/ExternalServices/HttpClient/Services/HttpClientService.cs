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

using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using Xtate.DataModel.Services;
using Xtate.DataTypes;

namespace Xtate.ExternalServices.HttpClient.Services;

[InstantiatedByIoC]
public class HttpClientService : ExternalServiceBase
{
	[InstantiatedByIoC]
	public class Provider() : ExternalServiceProviderBase<HttpClientService>(type: @"http://xtate.net/scxml/service/#HTTPClient", alias: @"http");

	private static readonly FieldInfo DomainTableField = typeof(CookieContainer).GetField(name: @"m_domainTable", BindingFlags.Instance | BindingFlags.NonPublic)!;

	private static readonly FieldInfo ListField = typeof(CookieContainer).Assembly.GetType(@"System.Net.PathList")!.GetField(name: @"m_list", BindingFlags.Instance | BindingFlags.NonPublic)!;

	public required IList<HttpClientMimeTypeHandler> MimeTypeHandlers { private get; [SetByIoC] init; }

	public required System.Net.Http.HttpClient HttpClient { private get; [SetByIoC] init; }

	private static NameValueCollection? CreateHeadersCollection(in DataModelValue value)
	{
		var headers = value.AsListOrEmpty();

		var pairs = DataModelConverter.IsObject(headers)
			? from pair in headers.KeyValues select (Name: pair.Key, Value: pair.Value.AsStringOrDefault())
			: from item in headers select (Name: item.AsListOrEmpty()["name"].AsStringOrDefault(), Value: item.AsListOrEmpty()["value"].AsStringOrDefault());

		NameValueCollection? collection = null;

		foreach (var (nm, val) in pairs)
		{
			if (!string.IsNullOrEmpty(nm) && val is not null)
			{
				(collection ??= []).Add(nm, val);
			}
		}

		return collection;
	}

	private static List<Cookie>? CreateCookieList(in DataModelValue cookiesValue)
	{
		List<Cookie>? list = null;

		foreach (var cookie in cookiesValue.AsListOrEmpty())
		{
			(list ??= []).Add(CreateCookie(cookie));
		}

		return list;
	}

	protected override async ValueTask<DataModelValue> Execute()
	{
		var parameters = Parameters.AsListOrEmpty();
		var method = parameters["method"].AsStringOrDefault() ?? @"get";
		var accept = parameters["accept"].AsStringOrDefault();
		var contentType = parameters["contentType"].AsStringOrDefault();
		var headers = CreateHeadersCollection(parameters["headers"]);

		var response = await DoRequest(method, contentType, accept, headers).ConfigureAwait(false);

		return new DataModelList
			   {
				   { "statusCode", response.StatusCode },
				   { "statusDescription", response.StatusDescription },
				   { "webExceptionStatus", response.ExceptionStatus },
				   { "headers", GetResponseHeaderList(response) },
				   { "content", response.Content }
			   };
	}

	private static DataModelList GetResponseHeaderList(Response response)
	{
		if (response.Headers is not { } responseHeaders)
		{
			return DataModelList.Empty;
		}

		DataModelList? list = null;

		for (var i = 0; i < responseHeaders.Count; i ++)
		{
			if (responseHeaders.GetKey(i) is not { } name)
			{
				continue;
			}

			if (responseHeaders.GetValues(i) is not { Length: > 0 } values)
			{
				continue;
			}

			(list ??= []).Add(new DataModelList { { "name", name }, { "value", string.Join(separator: @", ", values) } });
		}

		return list ?? DataModelList.Empty;
	}

	private static Cookie CreateCookie(in DataModelValue value)
	{
		var cookieProps = value.AsListOrEmpty();

		var cookie = new Cookie
					 {
						 Name = cookieProps["name"].AsStringOrDefault() ?? string.Empty,
						 Value = cookieProps["value"].AsStringOrDefault() ?? string.Empty,
						 Expires = cookieProps["expires"].AsDateTimeOrDefault()?.ToDateTime() ?? DateTime.MinValue,
						 HttpOnly = cookieProps["httpOnly"].AsBooleanOrDefault() ?? false,
						 Secure = cookieProps["secure"].AsBooleanOrDefault() ?? false
					 };

		if (cookieProps["path"].AsStringOrDefault() is { } path)
		{
			cookie.Path = path;
		}

		if (cookieProps["domain"].AsStringOrDefault() is { } domain)
		{
			cookie.Domain = domain;
		}

		if (cookieProps["port"].AsStringOrDefault() is { } port)
		{
			cookie.Port = port;
		}

		return cookie;
	}

	private static StringContent? CreateDefaultContent(in DataModelValue content) => content.ToObject()?.ToString() is { Length: > 0 } body ? new StringContent(body, Encoding.UTF8) : null;

	private async ValueTask<Response> DoRequest(string method,
												string? contentType,
												string? accept,
												NameValueCollection? headers)
	{
		Infra.NotNull(Source);

		using var request = new HttpRequestMessage(new HttpMethod(method), Source);

		if (headers is not null)
		{
			foreach (var key in headers.AllKeys)
			{
				foreach (var value in headers.GetValues(key)!)
				{
					request.Headers.Add(key!, value);
				}
			}
		}

		if (accept is not null)
		{
			request.Headers.Accept.ParseAdd(accept);
		}

		var contentTypeObj = contentType is not null ? new ContentType(contentType) : null;

		foreach (var handler in MimeTypeHandlers)
		{
			handler.PrepareRequest(request, contentTypeObj, Parameters.AsListOrEmpty(), Content);
		}

		SetContent(request, contentTypeObj);

		HttpResponseMessage response;

		try
		{
			response = await HttpClient.SendAsync(request, DestroyToken).ConfigureAwait(false);
		}
		catch (HttpRequestException ex)
		{
			return new Response { ExceptionStatus = ex.Message };
		}

		using (response)
		{
			var responseHeaders = new NameValueCollection();

			foreach (var pair in response.Headers)
			{
				foreach (var value in pair.Value)
				{
					responseHeaders.Add(pair.Key, value);
				}
			}

			return new Response
				   {
					   StatusCode = (int)response.StatusCode,
					   StatusDescription = response.ReasonPhrase,
					   Headers = responseHeaders,
					   Content = await ReadContent(response, DestroyToken).ConfigureAwait(false)
				   };
		}
	}

	private async ValueTask<DataModelValue> ReadContent(HttpResponseMessage response, CancellationToken token)
	{
		foreach (var handler in MimeTypeHandlers)
		{
			if (await handler.TryParseResponseAsync(response, Parameters.AsListOrEmpty(), token).ConfigureAwait(false) is { } data)
			{
				return data;
			}
		}

		return default;
	}

	private void SetContent(HttpRequestMessage request, ContentType? contentType)
	{
		HttpContent? httpContent = null;

		foreach (var handler in MimeTypeHandlers)
		{
			httpContent = handler.TryCreateHttpContent(request, contentType, Parameters.AsListOrEmpty(), Content);

			if (httpContent != null)
			{
				break;
			}
		}

		httpContent ??= CreateDefaultContent(Content);

		if (httpContent is null)
		{
			return;
		}

		request.Content = httpContent;
	}

	private record Response
	{
		public int StatusCode { get; init; }

		public string? StatusDescription { get; init; }

		public string? ExceptionStatus { get; init; }

		public DataModelValue Content { get; init; }

		public NameValueCollection? Headers { get; init; }
	}
}