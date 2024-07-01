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

using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Text;

namespace Xtate.IoProcessor;


public abstract class HttpIoProcessorBase<THost, TContext> : IoProcessorBase, IDisposable, IAsyncDisposable where THost : HttpIoProcessorHostBase<THost, TContext>
{
	private const string MediaTypeTextPlain                 = @"text/plain";
	private const string MediaTypeApplicationJson           = @"application/json";
	private const string MediaTypeApplicationFormUrlEncoded = @"application/x-www-form-urlencoded";
	private const string EventNameParameterName             = @"_scxmleventname";
	private const string ContentLengthHeaderName            = @"Content-Length";
	private const string ContentTypeHeaderName              = @"Content-Type";
	private const string OriginHeaderName                   = @"Origin";
	private const string ErrorSuffixHeader                  = @"HttpIoProcessor.";

	private static readonly ConcurrentDictionary<IPEndPoint, THost> Hosts = new();

	private readonly Uri        _baseUri;
	private readonly string     _errorSuffix;
	private readonly string     _path;
	private          IPEndPoint _ipEndPoint;

	protected HttpIoProcessorBase(IEventConsumer eventConsumer,
								  Uri baseUri,
								  IPEndPoint ipEndPoint,
								  string id,
								  string? alias,
								  string errorSuffix) : base(eventConsumer, id, alias)
	{
		_baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
		_path = baseUri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
		_ipEndPoint = ipEndPoint;
		_errorSuffix = errorSuffix;
	}

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

	[SuppressMessage(category: "ReSharper", checkId: "SuspiciousTypeConversion.Global")]
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

	[SuppressMessage(category: "ReSharper", checkId: "SuspiciousTypeConversion.Global")]
	protected virtual void Dispose(bool dispose)
	{
		if (Hosts.TryGetValue(_ipEndPoint, out var host))
		{
			var last = host.RemoveProcessor(this, token: default).SynchronousGetResult();
			if (last && Hosts.TryRemove(new KeyValuePair<IPEndPoint, THost>(_ipEndPoint, host)))
			{
				Disposer.Dispose(host);
			}
		}
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
<<<<<<< Updated upstream
					await Disposer.DisposeAsync(host).ConfigureAwait(false);
				}
			}
		}

		[SuppressMessage(category: "ReSharper", checkId: "SuspiciousTypeConversion.Global")]
		protected virtual void Dispose(bool dispose)
		{
			if (Hosts.TryGetValue(_ipEndPoint, out var host))
			{
				var last = host.RemoveProcessor(this, token: default).SynchronousGetResult();
				if (last && Hosts.TryRemove(new KeyValuePair<IPEndPoint, THost>(_ipEndPoint, host)))
				{
					Disposer.Dispose(host);
				}
			}
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
						throw new ProcessorException(Resources.Exception_FoundMoreThenOneInterfaceToListen);
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

		protected override Uri? GetTarget(ServiceId serviceId) =>
			serviceId switch
			{
				SessionId sessionId => new Uri(_baseUri, sessionId.Value),
				_                   => default
			};

		protected override IHostEvent CreateHostEvent(ServiceId senderServiceId, IOutgoingEvent outgoingEvent)
		{
			if (outgoingEvent is null) throw new ArgumentNullException(nameof(outgoingEvent));

			if (outgoingEvent.Target is null)
			{
				throw new ArgumentException(Resources.Exception_TargetIsNotDefined, nameof(outgoingEvent));
			}

			return base.CreateHostEvent(senderServiceId, outgoingEvent);
		}

		protected override async ValueTask OutgoingEvent(IHostEvent hostEvent, CancellationToken token)
		{
			if (hostEvent is null) throw new ArgumentNullException(nameof(hostEvent));

			var targetUri = hostEvent.TargetServiceId?.Value;
			Infra.NotNull(targetUri);

			var content = GetContent(hostEvent, out var eventNameInContent);
			if (!hostEvent.NameParts.IsDefaultOrEmpty && !eventNameInContent)
			{
				targetUri = QueryStringHelper.AddQueryString(targetUri, EventNameParameterName, EventName.ToName(hostEvent.NameParts));
			}

			using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, targetUri) { Content = content };

			if (GetTarget(hostEvent.SenderServiceId) is { } origin)
			{
				httpRequestMessage.Headers.Add(name: @"Origin", origin.ToString());
			}

			using var client = new HttpClient();
			var httpResponseMessage = await client.SendAsync(httpRequestMessage, token).ConfigureAwait(false);
			httpResponseMessage.EnsureSuccessStatusCode();
		}

		private static HttpContent? GetContent(IHostEvent hostEvent, out bool eventNameInContent)
		{
			var data = hostEvent.Data;
			var dataType = data.Type;

			switch (dataType)
			{
				case DataModelValueType.Undefined:
				case DataModelValueType.Null:
					eventNameInContent = !hostEvent.NameParts.IsDefaultOrEmpty;

					return eventNameInContent ? new FormUrlEncodedContent(GetParameters(hostEvent.NameParts, dataModelList: null)) : null;

				case DataModelValueType.String:
					eventNameInContent = false;

					return new StringContent(data.AsString(), Encoding.UTF8, MediaTypeTextPlain);

				case DataModelValueType.List:
				{
					var dataModelList = data.AsList();

					if (IsStringDictionary(dataModelList))
					{
						eventNameInContent = true;
						return new FormUrlEncodedContent(GetParameters(hostEvent.NameParts, dataModelList));
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
			foreach (var pair in dataModelList.KeyValues)
			{
				if (pair.Key is null)
				{
					return false;
				}

				switch (pair.Value.Type)
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
						Infra.Unexpected(pair.Value.Type);
						break;
				}
			}

			return true;
		}

		private static IEnumerable<KeyValuePair<string?, string?>> GetParameters(ImmutableArray<IIdentifier> eventNameParts, DataModelList? dataModelList)
		{
			if (!eventNameParts.IsDefaultOrEmpty)
			{
				yield return new KeyValuePair<string?, string?>(EventNameParameterName, EventName.ToName(eventNameParts));
			}

			if (dataModelList is not null)
			{
				foreach (var pair in dataModelList.KeyValues)
				{
					yield return new KeyValuePair<string?, string?>(pair.Key, Convert.ToString(pair.Value, CultureInfo.InvariantCulture));
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

=======
>>>>>>> Stashed changes
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
					throw new ProcessorException(Resources.Exception_FoundMoreThenOneInterfaceToListen);
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

	protected override Uri? GetTarget(ServiceId serviceId) =>
		serviceId switch
		{
			SessionId sessionId => new Uri(_baseUri, sessionId.Value),
			_                   => default
		};

	protected override IHostEvent CreateHostEvent(ServiceId senderServiceId, IOutgoingEvent outgoingEvent)
	{
		if (outgoingEvent.Target is null)
		{
			throw new ArgumentException(Resources.Exception_TargetIsNotDefined, nameof(outgoingEvent));
		}

		return base.CreateHostEvent(senderServiceId, outgoingEvent);
	}

	protected override async ValueTask OutgoingEvent(IHostEvent hostEvent, CancellationToken token)
	{
		var targetUri = hostEvent.TargetServiceId?.Value;
		Infra.NotNull(targetUri);

		var content = GetContent(hostEvent, out var eventNameInContent);
		if (!hostEvent.NameParts.IsDefaultOrEmpty && !eventNameInContent)
		{
			targetUri = QueryStringHelper.AddQueryString(targetUri, EventNameParameterName, EventName.ToName(hostEvent.NameParts));
		}

		using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, targetUri);
		
		httpRequestMessage.Content = content;
		
		if (GetTarget(hostEvent.SenderServiceId) is { } origin)
		{
			httpRequestMessage.Headers.Add(name: @"Origin", origin.ToString());
		}

		using var client = new HttpClient();
		var httpResponseMessage = await client.SendAsync(httpRequestMessage, token).ConfigureAwait(false);
		httpResponseMessage.EnsureSuccessStatusCode();
	}

	private static HttpContent? GetContent(IHostEvent hostEvent, out bool eventNameInContent)
	{
		var data = hostEvent.Data;
		var dataType = data.Type;

		switch (dataType)
		{
			case DataModelValueType.Undefined:
			case DataModelValueType.Null:
				eventNameInContent = !hostEvent.NameParts.IsDefaultOrEmpty;

				return eventNameInContent ? new FormUrlEncodedContent(GetParameters(hostEvent.NameParts, dataModelList: null)) : null;

			case DataModelValueType.String:
				eventNameInContent = false;

				return new StringContent(data.AsString(), Encoding.UTF8, MediaTypeTextPlain);

			case DataModelValueType.List:
			{
				var dataModelList = data.AsList();

				if (IsStringDictionary(dataModelList))
				{
					eventNameInContent = true;
					return new FormUrlEncodedContent(GetParameters(hostEvent.NameParts, dataModelList));
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
		foreach (var pair in dataModelList.KeyValues)
		{
			if (pair.Key is null)
			{
				return false;
			}

			switch (pair.Value.Type)
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
					Infra.Unexpected(pair.Value.Type);
					break;
			}
		}

		return true;
	}

	private static IEnumerable<KeyValuePair<string?, string?>> GetParameters(ImmutableArray<IIdentifier> eventNameParts, DataModelList? dataModelList)
	{
		if (!eventNameParts.IsDefaultOrEmpty)
		{
			yield return new KeyValuePair<string?, string?>(EventNameParameterName, EventName.ToName(eventNameParts));
		}

		if (dataModelList is not null)
		{
			foreach (var pair in dataModelList.KeyValues)
			{
				yield return new KeyValuePair<string?, string?>(pair.Key, Convert.ToString(pair.Value, CultureInfo.InvariantCulture));
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

		if (await TryGetEventDispatcher(sessionId, token).ConfigureAwait(false) is not { } eventDispatcher)
		{
			return false;
		}

		IEvent? evt;
		try
		{
			evt = await CreateEvent(context, token).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			evt = CreateErrorEvent(context, ex);
		}

		await eventDispatcher.Send(evt, token).ConfigureAwait(false);

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

	protected virtual IEvent CreateErrorEvent(TContext context, Exception exception)
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

		return new EventObject
			   {
				   Type = EventType.External,
				   NameParts = EventName.GetErrorPlatform(ErrorSuffixHeader + _errorSuffix),
				   Data = data,
				   OriginType = IoProcessorId
			   };
	}

	protected abstract THost CreateHost(IPEndPoint ipEndPoint);

	protected abstract string GetPath(TContext context);

	protected abstract string? GetHeaderValue(TContext context, string name);

	protected abstract IPAddress? GetRemoteAddress(TContext context);

	protected abstract string? GetQueryString(TContext context);

	protected abstract Stream GetBody(TContext context);

	protected abstract string GetMethod(TContext context);

	protected virtual async ValueTask<IEvent> CreateEvent(TContext context, CancellationToken token)
	{
		var contentType = GetHeaderValue(context, ContentTypeHeaderName) is { Length: > 0 } contentTypeStr ? new ContentType(contentTypeStr) : new ContentType();
		var encoding = contentType.CharSet is not null ? Encoding.GetEncoding(contentType.CharSet) : Encoding.ASCII;

		string body;
		using (var streamReader = new StreamReader(GetBody(context).InjectCancellationToken(token), encoding))
		{
			body = await streamReader.ReadToEndAsync().ConfigureAwait(false);
		}

		Uri.TryCreate(GetHeaderValue(context, OriginHeaderName), UriKind.Absolute, out var origin);

		var data = CreateData(contentType.MediaType, body, out var eventNameInContent);

		var query = QueryStringHelper.ParseQuery(GetQueryString(context));
		var eventNameInQueryString = query[EventNameParameterName];
		var eventName = eventNameInQueryString is { Length: > 0 } ? eventNameInQueryString : null;

		eventName ??= eventNameInContent ?? GetMethod(context);

		return new EventObject
			   {
				   Type = EventType.External,
				   NameParts = EventName.ToParts(eventName),
				   Data = data,
				   OriginType = IoProcessorId,
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