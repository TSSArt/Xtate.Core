#region Copyright © 2019-2020 Sergii Artemenko

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

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Xtate.IoProcessor
{
	public sealed class NamedIoProcessor : IIoProcessor, IDisposable
	{
		private const int         MaxMessageSize     = 1048576;
		private const string      PipePrefix         = "#SCXML#_";
		private const PipeOptions DefaultPipeOptions = PipeOptions.WriteThrough | PipeOptions.Asynchronous;

		private static readonly Uri IoProcessorId      = new Uri("http://www.w3.org/TR/scxml/#SCXMLEventProcessor");
		private static readonly Uri IoProcessorAliasId = new Uri(uriString: "scxml", UriKind.Relative);

		private static readonly ConcurrentDictionary<string, IEventConsumer> InProcConsumers = new ConcurrentDictionary<string, IEventConsumer>();

		private readonly Uri            _baseUri;
		private readonly IEventConsumer _eventConsumer;
		private readonly Uri            _loopbackBaseUri;
		private readonly string         _name;
		private readonly string         _pipeName;

		private readonly CancellationTokenSource _stopTokenSource = new CancellationTokenSource();

		public NamedIoProcessor(IEventConsumer eventConsumer, string host, string name)
		{
			if (host is null) throw new ArgumentNullException(nameof(host));

			_eventConsumer = eventConsumer ?? throw new ArgumentNullException(nameof(eventConsumer));
			_name = name ?? throw new ArgumentNullException(nameof(name));
			_pipeName = PipePrefix + name;
			_baseUri = new Uri("pipe://" + host + "/" + name);
			_loopbackBaseUri = new Uri("pipe:///" + name);

			if (!InProcConsumers.TryAdd(name, eventConsumer))
			{
				throw new ProcessorException(Res.Format(Resources.Exception_NamedIoProcessor_with_name_already_has_been_registered, name));
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

	#region Interface IIoProcessor

		Uri IIoProcessor.GetTarget(SessionId sessionId) => GetTarget(sessionId);

		ValueTask IIoProcessor.Dispatch(SessionId sessionId, IOutgoingEvent evt, CancellationToken token) => OutgoingEvent(sessionId, evt, token);

		bool IIoProcessor.CanHandle(Uri? type, Uri? target) => type is null || FullUriComparer.Instance.Equals(type, IoProcessorId) || FullUriComparer.Instance.Equals(type, IoProcessorAliasId);

		Uri IIoProcessor.Id => IoProcessorId;

	#endregion

		private Uri GetTarget(SessionId sessionId, bool isLoopback = false) => new Uri(isLoopback ? _loopbackBaseUri : _baseUri, "#_scxml_" + sessionId.Value);

		private async ValueTask OutgoingEvent(SessionId sessionId, IOutgoingEvent evt, CancellationToken token)
		{
			if (evt.Target is null)
			{
				throw new ProcessorException(Resources.Exception_Event_Target_did_not_specified);
			}

			var host = evt.Target.Host;
			var isLoopback = evt.Target.IsLoopback || host == _baseUri.Host;
			var name = evt.Target.GetComponents(UriComponents.Path, UriFormat.Unescaped);

			var targetSessionId = ExtractSessionId(evt.Target);
			var eventObject = new EventObject(EventType.External, evt, GetTarget(sessionId, isLoopback), IoProcessorId);

			if (isLoopback && InProcConsumers.TryGetValue(name, out var eventConsumer))
			{
				if (eventConsumer.TryGetEventDispatcher(targetSessionId, out var eventDispatcher))
				{
					await eventDispatcher.Send(eventObject, token).ConfigureAwait(false);
				}
				else
				{
					throw new ProcessorException(Resources.Exception_Event_dispatcher_not_found);
				}
			}
			else
			{
				await SendEventToPipe(isLoopback ? "." : host, PipePrefix + name, targetSessionId, eventObject, token).ConfigureAwait(false);
			}
		}

		private static async ValueTask SendEventToPipe(string server, string pipeName, SessionId? sessionId, EventObject eventObject, CancellationToken token)
		{
			var pipeStream = new NamedPipeClientStream(server, pipeName, PipeDirection.InOut, DefaultPipeOptions);
			var memoryStream = new MemoryStream();

			await using (pipeStream.ConfigureAwait(false))
			await using (memoryStream.ConfigureAwait(false))
			{
				await pipeStream.ConnectAsync(token).ConfigureAwait(false);

				var message = new EventMessage(sessionId, eventObject);

				Serialize(message, memoryStream);

				await SendMessage(memoryStream, pipeStream, token).ConfigureAwait(false);

				memoryStream.SetLength(0);

				await ReceiveMessage(pipeStream, memoryStream, token).ConfigureAwait(false);

				memoryStream.Position = 0;
				var responseMessage = Deserialize(memoryStream, b => new ResponseMessage(b));

				switch (responseMessage.ErrorType)
				{
					case ErrorType.None:
						break;

					case ErrorType.Exception:
						throw new ProcessorException(Res.Format(Resources.Exception_Error_on_event_consumer_side, responseMessage.ExceptionMessage, responseMessage.ExceptionText));

					case ErrorType.EventDispatcherNotFound:
						throw new ProcessorException(Resources.Exception_Event_dispatcher_not_found);

					default:
						Infrastructure.UnexpectedValue(responseMessage.ErrorType);

						break;
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

					StartListener().Forget();

					await ReceiveMessage(pipeStream, memoryStream, _stopTokenSource.Token).ConfigureAwait(false);

					memoryStream.Position = 0;
					var message = Deserialize(memoryStream, b => new EventMessage(b));

					if (message.SessionId is { } sessionId)
					{
						if (_eventConsumer.TryGetEventDispatcher(sessionId, out var eventDispatcher))
						{
							await eventDispatcher.Send(message.Event, _stopTokenSource.Token).ConfigureAwait(false);
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

		private static SessionId ExtractSessionId(Uri target)
		{
			var fragment = target.GetComponents(UriComponents.Fragment, UriFormat.Unescaped);

			const string prefix = "_scxml_";

			if (fragment.StartsWith(prefix))
			{
				return SessionId.FromString(fragment[prefix.Length..]);
			}

			throw new ProcessorException(Resources.Exception_Target_wrong_format);
		}

#if !NET461 && !NETSTANDARD2_0
		[SuppressMessage(category: "Performance", checkId: "CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'", Justification = "Not available in .Net 4.6")]
#endif
		private static async ValueTask SendMessage(MemoryStream memoryStream, PipeStream pipeStream, CancellationToken token)
		{
			Infrastructure.Assert(pipeStream.IsConnected);

			var sizeBuf = BitConverter.GetBytes((int) memoryStream.Length);
			Debug.Assert(sizeBuf.Length == sizeof(int));
			await pipeStream.WriteAsync(sizeBuf, offset: 0, sizeBuf.Length, token).ConfigureAwait(false);

			memoryStream.TryGetBuffer(out var buffer);
			Debug.Assert(buffer.Array is not null);

			await pipeStream.WriteAsync(buffer.Array, buffer.Offset, buffer.Count, token).ConfigureAwait(false);
		}

#if !NET461 && !NETSTANDARD2_0
		[SuppressMessage(category: "Performance", checkId: "CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'", Justification = "Not available in .Net 4.6")]
#endif
		[SuppressMessage(category: "ReSharper", checkId: "MethodHasAsyncOverloadWithCancellation")]
		private static async ValueTask ReceiveMessage(PipeStream pipeStream, MemoryStream memoryStream, CancellationToken token)
		{
			Infrastructure.Assert(pipeStream.IsConnected);

			var sizeBuf = new byte[sizeof(int)];
			var sizeBufReadCount = await pipeStream.ReadAsync(sizeBuf, offset: 0, sizeBuf.Length, token).ConfigureAwait(false);
			var messageSize = BitConverter.ToInt32(sizeBuf, startIndex: 0);

			if (sizeBufReadCount != sizeBuf.Length || messageSize < 0 || messageSize > MaxMessageSize)
			{
				throw new ProcessorException(Res.Format(Resources.NamedIoProcessor_ReceiveMessage4_Message_size_has_wrong_value_or_missed, messageSize));
			}

			var buffer = ArrayPool<byte>.Shared.Rent(messageSize);

			try
			{
				var count = await pipeStream.ReadAsync(buffer, offset: 0, messageSize, token).ConfigureAwait(false);

				if (count != messageSize)
				{
					throw new ProcessorException(Res.Format(Resources.NamedIoProcessor_ReceiveMessage4_Message_read_partially, count, messageSize));
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

		public ValueTask CheckPipeline(CancellationToken token)
		{
			var eventObject = new EventObject(EventType.External, new EventEntity("$"));

			return SendEventToPipe(server: ".", _pipeName, sessionId: null, eventObject, token);
		}

		private readonly struct EventMessage : IStoreSupport
		{
			public readonly EventObject Event;
			public readonly SessionId?  SessionId;

			public EventMessage(SessionId? sessionId, EventObject evt)
			{
				SessionId = sessionId;
				Event = evt;
			}

			public EventMessage(in Bucket bucket)
			{
				if (!bucket.TryGet(Key.TypeInfo, out TypeInfo storedTypeInfo) || storedTypeInfo != TypeInfo.Message)
				{
					throw new ArgumentException(Resources.Exception_Invalid_TypeInfo_value);
				}

				SessionId = bucket.GetSessionId(Key.SessionId);
				Event = new EventObject(bucket.Nested(Key.Event));
			}

		#region Interface IStoreSupport

			public void Store(Bucket bucket)
			{
				bucket.Add(Key.TypeInfo, TypeInfo.Message);
				bucket.AddId(Key.SessionId, SessionId);
				bucket.AddEntity(Key.Event, Event);
			}

		#endregion
		}

		private enum ErrorType
		{
			None                    = 0,
			Exception               = 1,
			EventDispatcherNotFound = 2
		}

		private readonly struct ResponseMessage : IStoreSupport
		{
			public readonly ErrorType ErrorType;
			public readonly string?   ExceptionMessage;
			public readonly string?   ExceptionText;

			public ResponseMessage(ErrorType errorType)
			{
				ErrorType = errorType;
				ExceptionMessage = null;
				ExceptionText = null;
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
					throw new ArgumentException(Resources.Exception_Invalid_TypeInfo_value);
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
}