﻿// Copyright © 2019-2024 Sergii Artemenko
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

using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Text;

namespace Xtate.IoProcessor;

public abstract class HttpIoProcessorBase<THost, TContext>(
	IEventConsumer eventConsumer,
	Uri baseUri,
	IPEndPoint ipEndPoint,
	FullUri id,
	FullUri? alias,
	string errorSuffix)
	: IoProcessorBase(id, alias), IDisposable, IAsyncDisposable where THost : HttpIoProcessorHostBase<THost, TContext>
{
	private const string MediaTypeTextPlain = @"text/plain";

	private const string MediaTypeApplicationJson = @"application/json";

	private const string MediaTypeApplicationFormUrlEncoded = @"application/x-www-form-urlencoded";

	private const string EventNameParameterName = @"_scxmleventname";

	private const string ContentLengthHeaderName = @"Content-Length";

	private const string ContentTypeHeaderName = @"Content-Type";

	private const string OriginHeaderName = @"Origin";

	private const string ErrorSuffixHeader = @"HttpIoProcessor.";

	private static readonly ConcurrentDictionary<IPEndPoint, THost> Hosts = new();

	private readonly Uri _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));

	private readonly string _errorSuffix = errorSuffix;

	private readonly string _path = baseUri.GetComponents(UriComponents.Path, UriFormat.Unescaped);

	private IPEndPoint _ipEndPoint = ipEndPoint;

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);
		
		Dispose(false);
		
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (Hosts.TryGetValue(_ipEndPoint, out var host))
		{
			var last = await host.RemoveProcessor(this, token: default).ConfigureAwait(false);

			if (last && Hosts.TryRemove(new KeyValuePair<IPEndPoint, THost>(_ipEndPoint, host)))
			{
				await Disposer.DisposeAsync(host).ConfigureAwait(false);
			}
		}
	}

	protected virtual void Dispose(bool dispose)
	{   //TODO:
		/*
		if (Hosts.TryGetValue(_ipEndPoint, out var host))
		{
			var last = host.RemoveProcessor(this, token: default).SynchronousGetResult();

			if (last && Hosts.TryRemove(new KeyValuePair<IPEndPoint, THost>(_ipEndPoint, host)))
			{
				Disposer.Dispose(host);
			}
		}*/
	}

	private static bool IsInterfaceAddress(IPAddress address)
	{
		foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
		{
			var ipInterfaceProperties = networkInterface.GetIPProperties();

			foreach (var ipInterfaceProperty in ipInterfaceProperties.UnicastAddresses)
			{
				if (ipInterfaceProperty.Address.Equals(address))
				{
					return true;
				}
			}
		}

		return false;
	}

	public virtual async ValueTask Start(CancellationToken token)
	{
		if (_ipEndPoint.Address.Equals(IPAddress.None) && _ipEndPoint.Port == 0)
		{
			_ipEndPoint = await FromUri(_baseUri).ConfigureAwait(false);
		}

		var host = Hosts.GetOrAdd(_ipEndPoint, CreateHost);

		await host.AddProcessor(this, token).ConfigureAwait(false);
	}

	private static async ValueTask<IPEndPoint> FromUri(Uri uri)
	{
		if (uri.IsLoopback)
		{
			return new IPEndPoint(IPAddress.Loopback, uri.Port);
		}

		var hostEntry = await Dns.GetHostEntryAsync(uri.DnsSafeHost).ConfigureAwait(false);

		IPAddress? listenAddress = default;

		foreach (var address in hostEntry.AddressList)
		{
			if (IsInterfaceAddress(address))
			{
				if (listenAddress is not null)
				{
					throw new ProcessorException(Resources.Exception_FoundMoreThanOneInterfaceToListen);
				}

				listenAddress = address;
			}
		}

		if (listenAddress is null)
		{
			throw new ProcessorException(Resources.Exception_CantMatchNetworkInterfaceToListen);
		}

		return new IPEndPoint(listenAddress, uri.Port);
	}

	protected override FullUri? GetTarget(ServiceId serviceId) =>
		serviceId switch
		{
			SessionId sessionId => new FullUri(_baseUri, sessionId.Value),
			_                   => default
		};

	protected override ValueTask<IRouterEvent> GetRouterEvent(IOutgoingEvent outgoingEvent, CancellationToken token)
	{
		if (outgoingEvent.Target is null)
		{
			throw new ArgumentException(Resources.Exception_TargetIsNotDefined, nameof(outgoingEvent));
		}

		return base.GetRouterEvent(outgoingEvent, token);
	}

	protected override async ValueTask OutgoingEvent(IRouterEvent routerEvent, CancellationToken token)
	{
		var targetUri = routerEvent.TargetServiceId?.Value;
		Infra.NotNull(targetUri);

		var content = GetContent(routerEvent, out var eventNameInContent);

		if (!routerEvent.Name.IsDefault && !eventNameInContent)
		{
			targetUri = QueryStringHelper.AddQueryString(targetUri, EventNameParameterName, routerEvent.Name.ToString());
		}

		using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, targetUri);

		httpRequestMessage.Content = content;

		if (GetTarget(routerEvent.SenderServiceId) is { } origin)
		{
			httpRequestMessage.Headers.Add(name: @"Origin", origin.ToString());
		}

		using var client = new HttpClient();
		var httpResponseMessage = await client.SendAsync(httpRequestMessage).ConfigureAwait(false);
		httpResponseMessage.EnsureSuccessStatusCode();
	}

	private static HttpContent? GetContent(IRouterEvent routerEvent, out bool eventNameInContent)
	{
		var data = routerEvent.Data;
		var dataType = data.Type;

		switch (dataType)
		{
			case DataModelValueType.Undefined:
			case DataModelValueType.Null:
				eventNameInContent = !routerEvent.Name.IsDefault;

				return eventNameInContent ? new FormUrlEncodedContent(GetParameters(routerEvent.Name, dataModelList: null)) : null;

			case DataModelValueType.String:
				eventNameInContent = false;

				return new StringContent(data.AsString(), Encoding.UTF8, MediaTypeTextPlain);

			case DataModelValueType.List:
			{
				var dataModelList = data.AsList();

				if (IsStringDictionary(dataModelList))
				{
					eventNameInContent = true;

					return new FormUrlEncodedContent(GetParameters(routerEvent.Name, dataModelList));
				}

				eventNameInContent = false;

				var content = new ByteArrayContent(DataModelConverter.ToJsonUtf8Bytes(data));
				content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeApplicationJson) { CharSet = Encoding.UTF8.WebName };

				return content;
			}

			default:
				throw new NotSupportedException(Resources.Exception_DataFormatNotSupported);
		}
	}

	private static bool IsStringDictionary(DataModelList dataModelList)
	{
		foreach (var (key, value) in dataModelList.KeyValues)
		{
			if (key is null)
			{
				return false;
			}

			switch (value.Type)
			{
				case DataModelValueType.List:
				case DataModelValueType.Number:
				case DataModelValueType.DateTime:
				case DataModelValueType.Boolean:
					return false;

				case DataModelValueType.Undefined:
				case DataModelValueType.Null:
				case DataModelValueType.String:
					break;

				default:
					throw Infra.Unmatched(value.Type);
			}
		}

		return true;
	}

	private static IEnumerable<KeyValuePair<string?, string?>> GetParameters(EventName eventName, DataModelList? dataModelList)
	{
		if (!eventName.IsDefault)
		{
			yield return new KeyValuePair<string?, string?>(EventNameParameterName, eventName.ToString());
		}

		if (dataModelList is not null)
		{
			foreach (var (key, value) in dataModelList.KeyValues)
			{
				yield return new KeyValuePair<string?, string?>(key, Convert.ToString(value, CultureInfo.InvariantCulture));
			}
		}
	}

	private static bool GetRelativePath(string path, string basePath, out string relativePath)
	{
		if (basePath.Length == 0)
		{
			relativePath = path;

			return true;
		}

		if (path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
		{
			if (path.Length == basePath.Length || path[basePath.Length] == '/')
			{
				relativePath = path[basePath.Length..];

				return true;
			}
		}

		relativePath = string.Empty;

		return false;
	}

	public virtual async ValueTask<bool> Handle(TContext context, CancellationToken token)
	{
		SessionId sessionId;

		var path = GetPath(context);

		if (string.IsNullOrEmpty(path))
		{
			return false;
		}

		if (_path.Length == 0)
		{
			sessionId = ExtractSessionId(path);
		}
		else if (GetRelativePath(path, _path, out var relativePath))
		{
			sessionId = ExtractSessionId(relativePath);
		}
		else
		{
			return false;
		}

		//if (await TryGetEventDispatcher(sessionId, token).ConfigureAwait(false) is not { } eventDispatcher) //TODO:
		{
			return false;
		}

		IIncomingEvent? incomingEvent;

		try
		{
			incomingEvent = await CreateEvent(context, token).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			incomingEvent = CreateErrorEvent(context, ex);
		}

		//await eventDispatcher.Dispatch(incomingEvent, token).ConfigureAwait(false); //TODO:

		return true;
	}

	private static SessionId ExtractSessionId(string path)
	{
		if (path.Length > 0 && path[0] == '/')
		{
			return SessionId.FromString(path[1..]);
		}

		return SessionId.FromString(path);
	}

	protected virtual IIncomingEvent CreateErrorEvent(TContext context, Exception exception)
	{
		var requestData = new DataModelList
						  {
							  { @"remoteIp", GetRemoteAddress(context) is { } address ? address.ToString() : string.Empty },
							  { @"method", GetMethod(context) },
							  { @"contentType", GetHeaderValue(context, ContentTypeHeaderName) is { Length: > 0 } typeStr ? typeStr : string.Empty },
							  { @"contentLength", GetHeaderValue(context, ContentLengthHeaderName) is { Length: > 0 } lenStr && int.TryParse(lenStr, out var len) ? len : -1 },
							  { @"path", GetPath(context) },
							  { @"query", GetQueryString(context) ?? string.Empty }
						  };

		var exceptionData = new DataModelList
							{
								{ @"message", exception.Message },
								{ @"typeName", exception.GetType().Name },
								{ @"source", exception.Source },
								{ @"typeFullName", exception.GetType().FullName },
								{ @"stackTrace", exception.StackTrace },
								{ @"text", exception.ToString() }
							};

		var data = new DataModelList
				   {
					   { @"request", requestData },
					   { @"exception", exceptionData }
				   };

		exceptionData.MakeDeepConstant();

		return new IncomingEvent
			   {
				   Type = EventType.External,
				   Name = EventName.GetErrorPlatform(ErrorSuffixHeader + _errorSuffix),
				   Data = data,
				   OriginType = id
			   };
	}

	protected abstract THost CreateHost(IPEndPoint ipEndPoint);

	protected abstract string GetPath(TContext context);

	protected abstract string? GetHeaderValue(TContext context, string name);

	protected abstract IPAddress? GetRemoteAddress(TContext context);

	protected abstract string? GetQueryString(TContext context);

	protected abstract Stream GetBody(TContext context);

	protected abstract string GetMethod(TContext context);

	protected virtual async ValueTask<IIncomingEvent> CreateEvent(TContext context, CancellationToken token)
	{
		var contentType = GetHeaderValue(context, ContentTypeHeaderName) is { Length: > 0 } contentTypeStr ? new ContentType(contentTypeStr) : new ContentType();
		var encoding = contentType.CharSet is not null ? Encoding.GetEncoding(contentType.CharSet) : Encoding.ASCII;

		string body;

		using (var streamReader = new StreamReader(GetBody(context).InjectCancellationToken(token), encoding))
		{
			body = await streamReader.ReadToEndAsync().ConfigureAwait(false);
		}

		FullUri.TryCreate(GetHeaderValue(context, OriginHeaderName), out var origin);

		var data = CreateData(contentType.MediaType, body, out var eventNameInContent);

		var query = QueryStringHelper.ParseQuery(GetQueryString(context));
		var eventNameInQueryString = query[EventNameParameterName];
		var eventName = eventNameInQueryString is { Length: > 0 } ? eventNameInQueryString : null;

		eventName ??= eventNameInContent ?? GetMethod(context);

		return new IncomingEvent
			   {
				   Type = EventType.External,
				   Name = (EventName) eventName,
				   Data = data,
				   OriginType = id,
				   Origin = origin
			   };
	}

	protected virtual DataModelValue CreateData(string mediaType, string body, out string? eventName)
	{
		eventName = default;

		if (mediaType == MediaTypeTextPlain)
		{
			return body;
		}

		if (mediaType == MediaTypeApplicationFormUrlEncoded)
		{
			var list = new DataModelList();

			var collection = QueryStringHelper.ParseQuery(body);

			for (var i = 0; i < collection.Count; i ++)
			{
				if (collection.GetKey(i) is not { } key)
				{
					continue;
				}

				if (key == EventNameParameterName)
				{
					eventName = collection[key];
				}
				else if (collection.GetValues(i) is { } values)
				{
					foreach (var value in values)
					{
						list.Add(key, value);
					}
				}
			}

			return list;
		}

		if (mediaType == MediaTypeApplicationJson)
		{
			return DataModelConverter.FromJson(body);
		}

		return default;
	}
}