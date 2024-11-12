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

using System.Buffers;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using Xtate.Persistence;

namespace Xtate.IoProcessor;

public class NamedPipeIoProcessor : IoProcessorBase, IDisposable
{
	private const string SessionIdPrefix = "#_session_";

	private const string InvokeIdPrefix = "#_invoke_";

	private const string PipePrefix = "Xtate/";

	private const PipeOptions DefaultPipeOptions = PipeOptions.WriteThrough | PipeOptions.Asynchronous;

	private const string Id = @"http://www.w3.org/TR/scxml/#NamedPipeEventProcessor";

	private const string Alias = @"net.pipe";

	private static readonly IncomingEvent CheckPipelineEvent = new() { Type = EventType.External, Name = (EventName) @"$" };

	private static readonly ConcurrentDictionary<string, IEventConsumer> InProcConsumers = new();

	private readonly Uri _baseUri;

	private readonly IEventConsumer _eventConsumer;

	private readonly int _maxMessageSize;

	private readonly string _name;

	private readonly string _pipeName;

	private readonly CancellationTokenSource _stopTokenSource = new();

	public required TaskCollector TaskCollector { private get; [UsedImplicitly] init; }

	public NamedPipeIoProcessor(IEventConsumer eventConsumer,
								[Localizable(false)] string host,
								[Localizable(false)] string name,
								int maxMessageSize) : base(eventConsumer, Id, Alias)
	{
		if (host is null) throw new ArgumentNullException(nameof(host));
		if (maxMessageSize < 0) throw new ArgumentOutOfRangeException(nameof(maxMessageSize));

		_eventConsumer = eventConsumer ?? throw new ArgumentNullException(nameof(eventConsumer));
		_name = name ?? throw new ArgumentNullException(nameof(name));
		_pipeName = PipePrefix + name;
		_baseUri = new Uri(@"net.pipe://" + host + @"/" + name + @"/");
		_maxMessageSize = maxMessageSize;

		if (!InProcConsumers.TryAdd(name, eventConsumer))
		{
			throw new ProcessorException(Res.Format(Resources.Exception_NamedPipeIoProcessorWithNameAlreadyHasBeenRegistered, name));
		}
	}

#region Interface IDisposable

	public void Dispose()
	{
		_stopTokenSource.Cancel();

		InProcConsumers.TryRemove(_name, out _);

		_stopTokenSource.Dispose();
	}

#endregion

	protected override IRouterEvent CreateRouterEvent(IOutgoingEvent outgoingEvent)
	{
		if (outgoingEvent.Target is null)
		{
			throw new ProcessorException(Resources.Exception_EventTargetDidNotSpecify);
		}

		return base.CreateRouterEvent(outgoingEvent);
	}

	protected override async ValueTask OutgoingEvent(IRouterEvent routerEvent)
	{
		var target = ((UriId?) routerEvent.TargetServiceId)?.Uri;

		Infra.NotNull(target);

		var host = target.Host;
		var isLoopback = target.IsLoopback || host == _baseUri.Host;
		var name = target.GetComponents(UriComponents.Path, UriFormat.Unescaped);

		var targetServiceId = GetServiceId(target);

		if (isLoopback && InProcConsumers.TryGetValue(name, out var eventConsumer))
		{
			if (await eventConsumer.TryGetEventDispatcher(targetServiceId, token: default).ConfigureAwait(false) is { } eventDispatcher)
			{
				await eventDispatcher.Dispatch(routerEvent).ConfigureAwait(false);
			}
			else
			{
				throw new ProcessorException(Resources.Exception_EventDispatcherNotFound);
			}
		}
		else
		{
			await SendEventToPipe(isLoopback ? @"." : host, PipePrefix + name, targetServiceId, routerEvent, token: default).ConfigureAwait(false);
		}
	}

	protected override FullUri? GetTarget(ServiceId serviceId)
	{
		return serviceId switch
			   {
				   SessionId sessionId => new FullUri(_baseUri, SessionIdPrefix + sessionId.Value),
				   InvokeId invokeId   => new FullUri(_baseUri, InvokeIdPrefix + invokeId.Value),
				   UriId uriId         => new FullUri(_baseUri, uriId.Uri),
				   _                   => default
			   };
	}

	private async ValueTask SendEventToPipe(string server,
											string pipeName,
											ServiceId? targetServiceId,
											IIncomingEvent incomingEvent,
											CancellationToken token)
	{
		var pipeStream = new NamedPipeClientStream(server, pipeName, PipeDirection.InOut, DefaultPipeOptions);
		var memoryStream = new MemoryStream();

		await using (pipeStream.ConfigureAwait(false))
		await using (memoryStream.ConfigureAwait(false))
		{
			await pipeStream.ConnectAsync(token).ConfigureAwait(false);

			var message = new EventMessage(targetServiceId, incomingEvent);

			Serialize(message, memoryStream);

			await SendMessage(memoryStream, pipeStream, token).ConfigureAwait(false);

			memoryStream.SetLength(0);

			await ReceiveMessage(pipeStream, memoryStream, token).ConfigureAwait(false);

			memoryStream.Position = 0;
			var responseMessage = Deserialize(memoryStream, bucket => new ResponseMessage(bucket));

			switch (responseMessage.ErrorType)
			{
				case ErrorType.None:
					break;

				case ErrorType.Exception:
					throw new ProcessorException(Res.Format(Resources.Exception_ErrorOnEventConsumerSide, responseMessage.ExceptionMessage, responseMessage.ExceptionText));

				case ErrorType.EventDispatcherNotFound:
					throw new ProcessorException(Resources.Exception_EventDispatcherNotFound);

				default:
					throw Infra.Unmatched(responseMessage.ErrorType);
			}
		}
	}

	public async ValueTask StartListener()
	{
		var pipeStream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, DefaultPipeOptions);
		var memoryStream = new MemoryStream();

		await using (pipeStream.ConfigureAwait(false))
		await using (memoryStream.ConfigureAwait(false))
		{
			ResponseMessage responseMessage = default;

			try
			{
				await pipeStream.WaitForConnectionAsync(_stopTokenSource.Token).ConfigureAwait(false);

				TaskCollector.Collect(StartListener());

				await ReceiveMessage(pipeStream, memoryStream, _stopTokenSource.Token).ConfigureAwait(false);

				memoryStream.Position = 0;
				var message = Deserialize(memoryStream, bucket => new EventMessage(bucket));

				if (message.TargetServiceId is { } targetServiceId)
				{
					if (await _eventConsumer.TryGetEventDispatcher(targetServiceId, _stopTokenSource.Token).ConfigureAwait(false) is { } eventDispatcher)
					{
						await eventDispatcher.Dispatch(message).ConfigureAwait(false);
					}
					else
					{
						responseMessage = new ResponseMessage(ErrorType.EventDispatcherNotFound);
					}
				}
			}
			catch (Exception ex)
			{
				responseMessage = new ResponseMessage(ex);
			}

			memoryStream.SetLength(0);
			Serialize(responseMessage, memoryStream);

			memoryStream.Position = 0;
			await SendMessage(memoryStream, pipeStream, _stopTokenSource.Token).ConfigureAwait(false);
		}
	}

	private static string GetTargetString(FullUri target) => target.IsAbsoluteUri ? target.Fragment : target.OriginalString;

	private static bool IsTargetSessionId(FullUri target, [NotNullWhen(true)] out SessionId? sessionId)
	{
		var value = GetTargetString(target);

		if (value.StartsWith(SessionIdPrefix, StringComparison.Ordinal))
		{
			sessionId = SessionId.FromString(value[SessionIdPrefix.Length..]);

			return true;
		}

		sessionId = default;

		return false;
	}

	private static bool IsTargetInvokeId(FullUri target, [NotNullWhen(true)] out InvokeId? invokeId)
	{
		var value = GetTargetString(target);

		if (value.StartsWith(InvokeIdPrefix, StringComparison.Ordinal))
		{
			invokeId = InvokeId.FromString(value[InvokeIdPrefix.Length..]);

			return true;
		}

		invokeId = default;

		return false;
	}

	private static ServiceId GetServiceId(FullUri target)
	{
		if (IsTargetSessionId(target, out var sessionId))
		{
			return sessionId;
		}

		if (IsTargetInvokeId(target, out var invokeId))
		{
			return invokeId;
		}

		return UriId.FromUri(target);
	}

	private static async ValueTask SendMessage(MemoryStream memoryStream, PipeStream pipeStream, CancellationToken token)
	{
		Infra.Assert(pipeStream.IsConnected);

		var sizeBuf = BitConverter.GetBytes((int) memoryStream.Length);
		Debug.Assert(sizeBuf.Length == sizeof(int));
		await pipeStream.WriteAsync(sizeBuf, offset: 0, sizeBuf.Length, token).ConfigureAwait(false);

		memoryStream.TryGetBuffer(out var buffer);
		Debug.Assert(buffer.Array is not null);

		await pipeStream.WriteAsync(buffer.Array, buffer.Offset, buffer.Count, token).ConfigureAwait(false);
	}

	[SuppressMessage(category: "ReSharper", checkId: "MethodHasAsyncOverloadWithCancellation")]
	private async ValueTask ReceiveMessage(PipeStream pipeStream, MemoryStream memoryStream, CancellationToken token)
	{
		Infra.Assert(pipeStream.IsConnected);

		var sizeBuf = new byte[sizeof(int)];
		var sizeBufReadCount = await pipeStream.ReadAsync(sizeBuf, offset: 0, sizeBuf.Length, token).ConfigureAwait(false);
		var messageSize = BitConverter.ToInt32(sizeBuf, startIndex: 0);

		if (sizeBufReadCount != sizeBuf.Length || messageSize < 0 || (_maxMessageSize > 0 && messageSize > _maxMessageSize))
		{
			throw new ProcessorException(Res.Format(Resources.Exception_NamedPipeIoProcessorMessageSizeHasWrongValueOrMissed, messageSize));
		}

		var buffer = ArrayPool<byte>.Shared.Rent(messageSize);

		try
		{
			var count = await pipeStream.ReadAsync(buffer, offset: 0, messageSize, token).ConfigureAwait(false);

			if (count != messageSize)
			{
				throw new ProcessorException(Res.Format(Resources.Exception_NamedPipeIoProcessorMessageReadPartially, count, messageSize));
			}

			memoryStream.Write(buffer, offset: 0, messageSize);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}

	private static T Deserialize<T>(MemoryStream memoryStream, Func<Bucket, T> creator)
	{
		memoryStream.TryGetBuffer(out var buffer);

		using var inMemoryStorage = new InMemoryStorage(buffer);
		var bucket = new Bucket(inMemoryStorage);

		memoryStream.Seek(offset: 0, SeekOrigin.End);

		return creator(bucket);
	}

	private static void Serialize<T>(T message, MemoryStream memoryStream) where T : IStoreSupport
	{
		using var inMemoryStorage = new InMemoryStorage();
		var bucket = new Bucket(inMemoryStorage);
		message.Store(bucket);

		var size = inMemoryStorage.GetTransactionLogSize();

		if (memoryStream.Length < memoryStream.Position + size)
		{
			memoryStream.SetLength(memoryStream.Position + size);
		}

		memoryStream.TryGetBuffer(out var buffer);
		Debug.Assert(buffer.Array is not null);

		var span = new Span<byte>(buffer.Array, buffer.Offset + (int) memoryStream.Position, buffer.Count);
		inMemoryStorage.WriteTransactionLogToSpan(span, truncateLog: false);

		memoryStream.Seek(size, SeekOrigin.Current);
	}

	public ValueTask CheckPipeline(CancellationToken token) => SendEventToPipe(server: @".", _pipeName, targetServiceId: default, CheckPipelineEvent, token);

	private class EventMessage : IncomingEvent
	{
		public EventMessage(ServiceId? targetServiceId, IIncomingEvent incomingEvent) : base(incomingEvent) => TargetServiceId = targetServiceId;

		public EventMessage(in Bucket bucket) : base(bucket)
		{
			if (!bucket.TryGet(Key.TypeInfo, out TypeInfo storedTypeInfo) || storedTypeInfo != TypeInfo.Message)
			{
				throw new ArgumentException(Resources.Exception_InvalidTypeInfoValue);
			}

			TargetServiceId = bucket.TryGetServiceId(Key.Target, out var serviceId) ? serviceId : default;
		}

		public ServiceId? TargetServiceId { get; }

		protected override TypeInfo TypeInfo => TypeInfo.Message;

		public override void Store(Bucket bucket)
		{
			base.Store(bucket);

			bucket.AddServiceId(Key.Target, TargetServiceId);
		}
	}

	private enum ErrorType
	{
		None = 0,

		Exception = 1,

		EventDispatcherNotFound = 2
	}

	private readonly struct ResponseMessage : IStoreSupport
	{
		public readonly ErrorType ErrorType;

		public readonly string? ExceptionMessage;

		public readonly string? ExceptionText;

		public ResponseMessage(ErrorType errorType)
		{
			ErrorType = errorType;
			ExceptionMessage = default;
			ExceptionText = default;
		}

		public ResponseMessage(Exception exception)
		{
			ErrorType = ErrorType.Exception;
			ExceptionMessage = exception.Message;
			ExceptionText = exception.ToString();
		}

		public ResponseMessage(in Bucket bucket)
		{
			if (!bucket.TryGet(Key.TypeInfo, out TypeInfo storedTypeInfo) || storedTypeInfo != TypeInfo.Message)
			{
				throw new ArgumentException(Resources.Exception_InvalidTypeInfoValue);
			}

			ErrorType = bucket.TryGet(Key.Type, out ErrorType errorType) ? errorType : ErrorType.None;
			ExceptionMessage = bucket.TryGet(Key.Message, out string? message) ? message : null;
			ExceptionText = bucket.TryGet(Key.Exception, out string? text) ? text : null;
		}

	#region Interface IStoreSupport

		public void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.Message);

			if (ErrorType != ErrorType.None)
			{
				bucket.Add(Key.Type, ErrorType);
				bucket.Add(Key.Message, ExceptionMessage);
				bucket.Add(Key.Exception, ExceptionText);
			}
		}

	#endregion
	}
}