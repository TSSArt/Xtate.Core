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

using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Xtate.DataModel.Services;
using Xtate.DataTypes;
using Xtate.Http;
using Xtate.Interpreter;
using Xtate.IoC.Options;

using Xtate.IoProcessors.Http.Internal;
using Xtate.StateMachine;
using Xtate.StateMachineHost;

namespace Xtate.IoProcessors.Http.Services;

public class HttpController
{
	private const string EventNameParameterName = @"_scxmleventname";

	private const string ContentLength = "Content-Length";

	private const string MediaTypeApplicationFormUrlEncoded = "application/x-www-form-urlencoded";

	private const string MediaTypeApplicationJson = "application/json";

	private const string DefaultContentType = "text/plain; charset=utf-8";

	private const string SessionPrefix = @"session/";

	private const string InvokePrefix = @"invoke/";

	private static readonly FullUri HttpIoProcessorOriginType = new ("http://www.w3.org/TR/scxml/#BasicHTTPEventProcessor") ;

	private static readonly ArrayPool<char> CharArrayPool = ArrayPool<char>.Create(maxArrayLength: 4096, maxArraysPerBucket: 16);

	private readonly Uri _base;

	private readonly Uri _invokeBase;

	private readonly HttpIoProcessorOptions _options;

	private readonly Uri _sessionBase;

	public required IExternalEventDispatcher<HttpIoProcessor> ExternalEventDispatcher { private get; [SetByIoC] init; }

	public HttpController(IOptions<HttpIoProcessorOptions> options)
	{
		_options = options.Value;

		_base = new Uri(new Uri(_options.PublicBaseUrl), relativeUri: @".");
		_sessionBase = new Uri(_base, SessionPrefix);
		_invokeBase = new Uri(_base, InvokePrefix);
	}

	public required Func<HttpClient> HttpClientFactory { private get; [SetByIoC] init; }

	public FullUri ToInvokeTarget(InvokeId invokeId) => new(_invokeBase, invokeId.ToString());

	public FullUri ToSessionTarget(SessionId sessionId) => new(_sessionBase, sessionId.ToString());

	public bool TryMatchTarget(Uri target, [MaybeNullWhen(false)] out ServiceId serviceId)
	{
		serviceId = null;

		if (target.Scheme != _base.Scheme)
		{
			return false;
		}

		if (target.Host != _base.Host)
		{
			return false;
		}

		if (target.Port != _base.Port)
		{
			return false;
		}

		var targetPath = target.AbsolutePath;
		var basePath = _base.AbsolutePath;

		if (!targetPath.StartsWith(basePath, StringComparison.Ordinal))
		{
			return false;
		}

		var startIndex = basePath.Length;
		var idIndex = targetPath.IndexOf(value: '/', startIndex) + 1;

		if (idIndex <= 0)
		{
			return false;
		}

		switch (targetPath.AsSpan()[startIndex..idIndex])
		{
			case SessionPrefix:
				serviceId = SessionId.FromString(targetPath[idIndex..]);

				break;

			case InvokePrefix:
				serviceId = InvokeId.FromString(targetPath[idIndex..]);

				break;

			default:
				return false;
		}

		return true;
	}

	public async ValueTask SendEvent(Uri target, IRouterEvent routerEvent, CancellationToken token)
	{
		var content = GetContent(routerEvent, out var eventNameInContent);

		if (content is not null && _options.MaxMessageSize > 0)
		{
			var writeLimitStream = new WriteLimitStream(_options.MaxMessageSize);
			await content.CopyToAsync(writeLimitStream).ConfigureAwait(false);
		}

		var targetUri = target.ToString();

		if (!routerEvent.Name.IsDefault && !eventNameInContent)
		{
			targetUri = QueryStringHelper.AddQueryString(targetUri, EventNameParameterName, routerEvent.Name.ToString()!);
		}

		using var httpClient = HttpClientFactory();
		httpClient.Timeout = _options.Timeout;

		using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, targetUri);

		httpRequestMessage.Content = content;

		if (GetSenderTarget(routerEvent.SenderServiceId) is { } origin)
		{
			httpRequestMessage.Headers.Add(name: "Origin", origin.ToString());
		}

		if (routerEvent.SendId is { } sendId)
		{
			httpRequestMessage.Headers.Add(name: "SCXML-SendId", sendId.ToString());
		}

		if (routerEvent.InvokeId is { } invokeId)
		{
			httpRequestMessage.Headers.Add(name: "SCXML-InvokeId", invokeId.ToString());
		}


		using var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, token).ConfigureAwait(false);
		httpResponseMessage.EnsureSuccessStatusCode();
	}

	private Uri GetSenderTarget(ServiceId senderId) =>
		senderId switch
		{
			SessionId sessionId => ToSessionTarget(sessionId),
			InvokeId invokeId   => ToInvokeTarget(invokeId),
			_                   => throw Infra.Unmatched(senderId)
		};

	private static HttpContent? GetContent(IRouterEvent routerEvent, out bool eventNameInContent)
	{
		var data = routerEvent.Data;
		eventNameInContent = !routerEvent.Name.IsDefault;

		switch (data.Type)
		{
			case DataModelValueType.Undefined:
			case DataModelValueType.Null:
				return eventNameInContent ? new FormUrlEncodedContent(GetParameters(routerEvent.Name, dataModelList: null)) : null;

			case DataModelValueType.String:
				return new StringContent(data.AsString(), Encoding.UTF8, MediaTypeNames.Text.Plain);

			case DataModelValueType.List:
			{
				var dataModelList = data.AsList();

				if (IsStringDictionary(dataModelList))
				{
					eventNameInContent = true;

					return new FormUrlEncodedContent(GetParameters(routerEvent.Name, dataModelList));
				}

				return new XmlHttpContent(data);
			}

			case DataModelValueType.Boolean:
			case DataModelValueType.Number:
			case DataModelValueType.DateTime:
				return new XmlHttpContent(data);

			default:
				throw Infra.Unmatched(data.Type);
		}
	}

	private static bool IsStringDictionary(DataModelList dataModelList)
	{
		foreach (var (key, value) in dataModelList.KeyValues)
		{
			if (key is null || value.Type is not (DataModelValueType.Undefined or DataModelValueType.Null or DataModelValueType.String))
			{
				return false;
			}
		}

		return true;
	}

	private static IEnumerable<KeyValuePair<string, string>> GetParameters(EventName eventName, DataModelList? dataModelList)
	{
		if (!eventName.IsDefault)
		{
			yield return new KeyValuePair<string, string>(EventNameParameterName, eventName.ToString());
		}

		if (dataModelList is not null)
		{
			foreach (var (key, value) in dataModelList.KeyValues)
			{
				yield return new KeyValuePair<string, string>(key, value.AsStringOrDefault() ?? string.Empty);
			}
		}
	}


	public async ValueTask ReceiveAndProcessEvent(HttpListener httpListener, CancellationToken token)
	{
		var context = await httpListener.GetContextAsync().ConfigureAwait(false);

		try
		{
			var request = context.Request;

			var (eventName, data) = await GetEventNameAndData(request, token).ConfigureAwait(false);

			var eventMessage = new IncomingEvent
							   {
								   Name = EventName.FromString(eventName),
								   Type = EventType.External,
								   Data = data,
								   Origin = request.Headers["Origin"],
								   OriginType = HttpIoProcessorOriginType,
								   InvokeId = request.Headers["SCXML-InvokeId"] is {} invokeId ? InvokeId.FromString(invokeId) : null,
								   SendId = request.Headers["SCXML-SendId"] is {} sendId ? SendId.FromString(sendId) : null
			};

			if (TryMatchTarget(request.Url, out var targetServiceId))
			{

			}

			await ExternalEventDispatcher.Dispatch(targetServiceId, eventMessage, token).ConfigureAwait(false);

			context.Response.StatusCode = (int) HttpStatusCode.OK;
		}
		catch (HttpRequestProcessException ex)
		{
			context.Response.StatusCode = (int) ex.StatusCode;
		}
		catch (Exception)
		{
			context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
		}
		finally
		{
			context.Response.Close();
		}
	}

	private async ValueTask<(string EventName, DataModelValue Data)> GetEventNameAndData(HttpListenerRequest request, CancellationToken token)
	{
		static string? Normalize(string? name) => string.IsNullOrWhiteSpace(name) ? null : name;

		var eventName = Normalize(request.QueryString[EventNameParameterName]);

		if (!request.HasEntityBody)
		{
			return (eventName ?? request.HttpMethod, DataModelValue.Undefined);
		}

		var contentType = new ContentType(request.ContentType ?? DefaultContentType);
		var contentLength = request.QueryString[ContentLength] is { } contentLengthValue ? long.Parse(contentLengthValue) : (long?) null;

		if (contentLength > _options.MaxMessageSize)
		{
			throw new HttpRequestProcessException("Content length exceeds the maximum allowed length.") { StatusCode = HttpStatusCode.RequestEntityTooLarge };
		}

		var stream = _options.MaxMessageSize > 0 ? new ReadLimitStream(request.InputStream, _options.MaxMessageSize) : request.InputStream;

		await using (stream.ConfigureAwait(false))
		{
			DataModelValue data;

			switch (contentType.MediaType)
			{
				case MediaTypeNames.Text.Plain:
				{
					using var reader = new StreamReader(stream, request.ContentEncoding);
					data = await reader.ReadToEndAsync(token).ConfigureAwait(false);

					break;
				}

				case MediaTypeApplicationFormUrlEncoded:
				{
					using var reader = new StreamReader(stream, request.ContentEncoding);
					var formUrl = await reader.ReadToEndAsync(token).ConfigureAwait(false);
					var nameValueCollection = QueryStringHelper.ParseQuery(formUrl);

					var vars = new DataModelList();

					foreach (string key in nameValueCollection.Keys)
					{
						foreach (var value in nameValueCollection.GetValues(key)!)
						{
							if (key == EventNameParameterName)
							{
								eventName ??= Normalize(nameValueCollection[key]);
							}
							else
							{
								vars.Add(key, value);
							}
						}
					}

					data = vars;

					break;
				}
				case MediaTypeNames.Text.Xml:
				{
					data = await DataModelConverter.FromXmlAsync(stream, token).ConfigureAwait(false);


					break;
				}

				case MediaTypeApplicationJson:
				{
					data = await DataModelConverter.FromJsonAsync(stream, token).ConfigureAwait(false);

					break;
				}

				default:
					throw new HttpRequestProcessException($"Unsupported media type: {contentType.MediaType}") { StatusCode = HttpStatusCode.UnsupportedMediaType };
			}

			return (eventName ?? request.HttpMethod, data);
		}
	}
}