// Copyright © 2019-2024 Sergii Artemenko
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
using System.Reflection;
using System.Text;

namespace Xtate.Service;

public class HttpClientService(HttpClientServiceOptions options) : ExternalServiceBase
{
	private static readonly FieldInfo DomainTableField = typeof(CookieContainer).GetField(name: "m_domainTable", BindingFlags.Instance | BindingFlags.NonPublic)!;

	private static readonly FieldInfo ListField = typeof(CookieContainer).Assembly.GetType("System.Net.PathList")!.GetField(name: "m_list", BindingFlags.Instance | BindingFlags.NonPublic)!;

	private static NameValueCollection? CreateHeadersCollection(in DataModelValue value)
	{
		var headers = value.AsListOrEmpty();

		var pairs = DataModelConverter.IsObject(headers)
			? from pair in headers.KeyValues select (Name: pair.Key, Value: pair.Value.AsStringOrDefault())
			: from item in headers select (Name: item.AsListOrEmpty()["name"].AsStringOrDefault(), Value: item.AsListOrEmpty()["value"].AsStringOrDefault());

		NameValueCollection? collection = default;

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
		List<Cookie>? list = default;

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
		var autoRedirect = parameters["autoRedirect"].AsBooleanOrDefault() ?? true;
		var accept = parameters["accept"].AsStringOrDefault();
		var contentType = parameters["contentType"].AsStringOrDefault();
		var headers = CreateHeadersCollection(parameters["headers"]);
		var cookies = CreateCookieList(parameters["cookies"]);

		var response = await DoRequest(method, autoRedirect, contentType, accept, headers, cookies).ConfigureAwait(false);

		return new DataModelList
			   {
				   { "statusCode", response.StatusCode },
				   { "statusDescription", response.StatusDescription },
				   { "webExceptionStatus", response.WebExceptionStatus },
				   { "headers", GetResponseHeaderList(response) },
				   { "cookies", GetResponseCookieList(response) },
				   { "content", response.Content }
			   };
	}

	private static DataModelList GetResponseCookieList(Response response)
	{
		if (response.Cookies is not { } responseCookies)
		{
			return DataModelList.Empty;
		}

		DataModelList? list = default;

		foreach (var cookie in responseCookies)
		{
			Infra.NotNull(cookie);

			(list ??= []).Add(
				new DataModelList
				{
					{ "name", cookie.Name },
					{ "value", cookie.Value },
					{ "path", cookie.Path },
					{ "domain", cookie.Domain },
					{ "httpOnly", cookie.HttpOnly },
					{ "port", cookie.Port },
					{ "secure", cookie.Secure },
					{ "expires", cookie.Expires != default ? cookie.Expires : default(DataModelValue) }
				});
		}

		return list ?? DataModelList.Empty;
	}

	private static DataModelList GetResponseHeaderList(Response response)
	{
		if (response.Headers is not { } responseHeaders)
		{
			return DataModelList.Empty;
		}

		DataModelList? list = default;

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

			foreach (var value in values)
			{
				(list ??= []).Add(new DataModelList { { "name", name }, { "value", value } });
			}
		}

		return list ?? DataModelList.Empty;
	}

	private static Cookie CreateCookie(in DataModelValue value)
	{
		var cookie = value.AsListOrEmpty();

		return new Cookie
			   {
				   Name = cookie["name"].AsStringOrDefault() ?? string.Empty,
				   Value = cookie["value"].AsStringOrDefault(),
				   Path = cookie["path"].AsStringOrDefault(),
				   Domain = cookie["domain"].AsStringOrDefault(),
				   Port = cookie["port"].AsStringOrDefault(),
				   Expires = cookie["expires"].AsDateTimeOrDefault()?.ToDateTime() ?? default,
				   HttpOnly = cookie["httpOnly"].AsBooleanOrDefault() ?? false,
				   Secure = cookie["secure"].AsBooleanOrDefault() ?? false
			   };
	}

	private static HttpContent CreateDefaultContent(in DataModelValue content) => new StringContent(content.ToObject()?.ToString() ?? string.Empty, Encoding.UTF8);

	private async ValueTask<Response> DoRequest(string method,
												bool autoRedirect,
												string? contentType,
												string? accept,
												NameValueCollection? headers,
												List<Cookie>? cookies)
	{
		Infra.NotNull(Source);

		var request = WebRequest.CreateHttp(Source);

		request.Method = method;
		request.AllowAutoRedirect = autoRedirect;

		if (headers is not null)
		{
			request.Headers.Add(headers);
		}

		if (accept is not null)
		{
			request.Accept = accept;
		}

		CookieContainer? cookieContainer = default;
		if (cookies is not null)
		{
			foreach (var cookie in cookies)
			{
				(cookieContainer ??= new CookieContainer()).Add(cookie);
			}

			request.CookieContainer = cookieContainer;
		}

		foreach (var handler in options.MimeTypeHandlers)
		{
			handler.PrepareRequest(request, contentType, Parameters.AsListOrEmpty(), Content);
		}

		await WriteContent(request, contentType).ConfigureAwait(false);

		string? webExceptionStatus = default;
		HttpWebResponse response;
		try
		{
			response = await GetResponse(request, DestroyToken).ConfigureAwait(false);
		}
		catch (WebException ex)
		{
			if (ex.Response is null)
			{
				return new Response();
			}

			response = (HttpWebResponse) ex.Response;

			webExceptionStatus = ex.Status.ToString();
		}

		using (response)
		{
			List<Cookie>? cookieCollection = default;

			if (cookieContainer is not null)
			{
				foreach (var pathList in ((Hashtable) DomainTableField.GetValue(cookieContainer)!).Values)
				{
					foreach (IEnumerable cookieList in ((SortedList) ListField.GetValue(pathList)!).Values)
					{
						foreach (Cookie cookie in cookieList)
						{
							(cookieCollection ??= []).Add(cookie);
						}
					}
				}
			}

			return new Response
				   {
					   StatusCode = (int) response.StatusCode,
					   StatusDescription = response.StatusDescription,
					   WebExceptionStatus = webExceptionStatus,
					   Headers = response.Headers,
					   Cookies = cookieCollection,
					   Content = await ReadContent(response, DestroyToken).ConfigureAwait(false)
				   };
		}
	}

	private async ValueTask<DataModelValue> ReadContent(WebResponse response, CancellationToken token)
	{
		foreach (var handler in options.MimeTypeHandlers)
		{
			if (await handler.TryParseResponseAsync(response, Parameters.AsListOrEmpty(), token).ConfigureAwait(false) is { } data)
			{
				return data;
			}
		}

		return default;
	}

	private static async Task<HttpWebResponse> GetResponse(HttpWebRequest request, CancellationToken token)
	{
		var registration = token.Register(request.Abort, useSynchronizationContext: false);
		await using (registration.ConfigureAwait(false))
		{
			try
			{
				return (HttpWebResponse) await request.GetResponseAsync().ConfigureAwait(false);
			}
			catch (WebException ex)
			{
				if (token.IsCancellationRequested)
				{
					throw new OperationCanceledException(ex.Message, ex, token);
				}

				throw;
			}
		}
	}

	private async ValueTask WriteContent(WebRequest request, string? contentType)
	{
		request.ContentType = contentType;

		HttpContent? httpContent = default;

		foreach (var handler in options.MimeTypeHandlers)
		{
			httpContent = handler.TryCreateHttpContent(request, contentType, Parameters.AsListOrEmpty(), Content);

			if (httpContent != null)
			{
				break;
			}
		}

		httpContent ??= CreateDefaultContent(Content);

		var stream = await request.GetRequestStreamAsync().ConfigureAwait(false);
		await using (stream.ConfigureAwait(false))
		{
			await httpContent.CopyToAsync(stream, DestroyToken).ConfigureAwait(false);
		}
	}

	private record Response
	{
		public int                  StatusCode         { get; init; }
		public string?              StatusDescription  { get; init; }
		public string?              WebExceptionStatus { get; init; }
		public DataModelValue       Content            { get; init; }
		public NameValueCollection? Headers            { get; init; }
		public List<Cookie>?        Cookies            { get; init; }
	}
}