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
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using Xtate.DataModel;
using Xtate.IoC.Options;
using Xtate.Persistence;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.IoProcessors.NamedPipe.Services;

[InstantiatedByIoC]
public class NamedPipeController(IOptions<NamedPipeIoProcessorOptions> options)
{
	private const string PipePrefix = "Xtate_";

	private const string SessionIdPrefix = "#_session_";

	private const string InvokeIdPrefix = "#_invoke_";

	private const PipeOptions DefaultPipeOptions = PipeOptions.WriteThrough | PipeOptions.Asynchronous;

	private static readonly IdnMapping IdnMapping = new();

	private readonly NamedPipeIoProcessorOptions _options = options.Value;

	public bool IsNamedPipeIoProcessorEnabled => _options.Name is not null;

	private static string GetPipeName(string name) => PipePrefix + name.ToLowerInvariant();

	public FullUri ToInvokeTarget(InvokeId invokeId) => new(@$"net.pipe://{_options.Host}/{_options.Name}/{InvokeIdPrefix}{invokeId}");

	public FullUri ToSessionTarget(SessionId sessionId) => new(@$"net.pipe://{_options.Host}/{_options.Name}/{SessionIdPrefix}{sessionId}");

	public bool TryParseTarget(FullUri target,
							   out string? host,
							   out string? name,
							   out ServiceId serviceId)
	{
		if (target.IsAbsoluteUri && target is { Scheme: "net.pipe", IsDefaultPort: true })
		{
			host = target.IsLoopback || HostEquals(target, _options.Host) ? null : target.Host;

			if (target.Segments is ["/", var seg])
			{
				var len = seg.Length;

				if (seg[len - 1] == '/')
				{
					len --;
				}

				if (len > 0)
				{
					name = seg[..len];

					if (host is null && NameEquals(name, _options.Name))
					{
						name = null;
					}

					var fragment = target.Fragment;

					if (target.IsAbsoluteUri && target is { Scheme: "net.pipe", IsDefaultPort: true })
					{
						if (fragment.StartsWith(SessionIdPrefix, StringComparison.Ordinal))
						{
							serviceId = SessionId.FromString(fragment[SessionIdPrefix.Length..]);

							return true;
						}

						if (fragment.StartsWith(InvokeIdPrefix, StringComparison.Ordinal))
						{
							serviceId = InvokeId.FromString(fragment[InvokeIdPrefix.Length..]);

							return true;
						}
					}
				}
			}
		}

		host = null!;
		name = null!;
		serviceId = null!;

		return false;
	}

	private static bool NameEquals(string name1, string name2) => string.Equals(name1, name2, StringComparison.OrdinalIgnoreCase);

	private static bool HostEquals(FullUri uri, string host) => uri.Host == host || string.Compare(uri.Host, host, StringComparison.OrdinalIgnoreCase) == 0 || uri.IdnHost == IdnMapping.GetAscii(host);

	public async ValueTask SendEvent(string? host,
									 string? name,
									 ServiceId targetServiceId,
									 IIncomingEvent incomingEvent,
									 CancellationToken token)
	{
		var pipeStream = new NamedPipeClientStream(host ?? @".", GetPipeName(name ?? _options.Name), PipeDirection.InOut, DefaultPipeOptions);

		await using (pipeStream.ConfigureAwait(false))
		{
			using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
			timeoutCts.CancelAfter(_options.Timeout);
			token = timeoutCts.Token;

			await pipeStream.ConnectAsync(token).ConfigureAwait(false);

			var eventMessage = new NamedPipeEventMessage(DateTime.UniqueUtcNow, targetServiceId, incomingEvent);

			await SendMessage(eventMessage, pipeStream, token).ConfigureAwait(false);

			var responseMessage = await ReceiveMessage(pipeStream, bucket => new NamedPipeResponseMessage(bucket), token).ConfigureAwait(false);

			switch (responseMessage.ErrorType)
			{
				case NamedPipeErrorType.None:
					if (responseMessage.Timestamp is not null && eventMessage.Timestamp != responseMessage.Timestamp)
					{
						throw new ProcessorException(Res.Format(Resources.Exception_InvalidTimestampActualExpected, responseMessage.Timestamp, eventMessage.Timestamp));
					}

					break;

				case NamedPipeErrorType.Exception:
					throw new ProcessorException(Res.Format(Resources.Exception_ErrorOnEventConsumerSide, responseMessage.ExceptionMessage, responseMessage.ExceptionText));

				default:
					throw Infra.Unmatched(responseMessage.ErrorType);
			}
		}
	}

	public async ValueTask ReceiveAndProcessEvent(Func<NamedPipeEventMessage, ValueTask> processEvent, CancellationToken token)
	{
		var pipeStream = new NamedPipeServerStream(GetPipeName(_options.Name), PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, DefaultPipeOptions);

		await using (pipeStream.ConfigureAwait(false))
		{
			await pipeStream.WaitForConnectionAsync(token).ConfigureAwait(false);

			NamedPipeEventMessage? eventMessage = null;

			try
			{
				eventMessage = await ReceiveMessage(pipeStream, bucket => new NamedPipeEventMessage(bucket), token).ConfigureAwait(false);

				await processEvent(eventMessage).ConfigureAwait(false);

				await SendMessage(new NamedPipeResponseMessage(eventMessage.Timestamp), pipeStream, token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await SendMessage(new NamedPipeResponseMessage(eventMessage?.Timestamp, ex), pipeStream, token).ConfigureAwait(false);

				throw;
			}
		}
	}

	private static async ValueTask<long> ReadAtLeast(PipeStream pipeStream,
													 byte[] buffer,
													 long length,
													 CancellationToken token)
	{
		if (length < int.MaxValue)
		{
			var offset = 0;

			while (length > 0)
			{
				var read = await pipeStream.ReadAsync(buffer, offset, (int) length - offset, token).ConfigureAwait(false);

				if (read == 0)
				{
					return offset;
				}

				length -= read;
				offset += read;
			}

			return offset;
		}
		else
		{
			var offset = 0L;

			var interBuffer = new byte[4096];

			while (length > 0)
			{
				var toRead = (int) Math.Min(length, interBuffer.Length);
				var read = await pipeStream.ReadAsync(interBuffer, offset: 0, toRead, token).ConfigureAwait(false);

				if (read == 0)
				{
					return offset;
				}

				length -= read;
				offset += read;
			}

			return offset;
		}
	}

	private async ValueTask<T> ReceiveMessage<T>(PipeStream pipeStream, Func<Bucket, T> factory, CancellationToken token)
	{
		var buffer = new byte[sizeof(long)];
		var sizeBufReadCount = await ReadAtLeast(pipeStream, buffer, sizeof(long), token).ConfigureAwait(false);
		var messageSize = BitConverter.ToInt64(buffer, startIndex: 0);

		var maxMessageSize = _options.MaxMessageSize > 0 ? _options.MaxMessageSize : (long?) null;

		if (sizeBufReadCount != sizeof(long) || messageSize < 0 || messageSize > maxMessageSize)
		{
			throw new ProcessorException(Res.Format(Resources.Exception_NamedPipeIoProcessorMessageSizeHasWrongValueOrMissed, messageSize));
		}

		buffer = messageSize <= int.MaxValue ? ArrayPool<byte>.Shared.Rent((int) messageSize) : new byte[messageSize];

		try
		{
			var count = await ReadAtLeast(pipeStream, buffer, messageSize, token).ConfigureAwait(false);

			if (count != messageSize)
			{
				throw new ProcessorException(Res.Format(Resources.Exception_NamedPipeIoProcessorMessageReadPartially, count, messageSize));
			}

			if (count > int.MaxValue)
			{
				throw new ProcessorException("Processing messages larger than 2GB is not supported.");
			}

			using var inMemoryStorage = new InMemoryStorage(buffer.AsSpan()[..(int) count]);

			return factory(new Bucket(inMemoryStorage));
		}
		finally
		{
			if (buffer.LongLength <= int.MaxValue)
			{
				ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
			}
		}
	}

	private async ValueTask SendMessage<T>(T message, PipeStream pipeStream, CancellationToken token) where T : IStoreSupport
	{
		using var inMemoryStorage = new InMemoryStorage();
		var bucket = new Bucket(inMemoryStorage);
		message.Store(bucket);

		var messageSize = inMemoryStorage.GetTransactionLogSize();

		var maxMessageSize = _options.MaxMessageSize;

		if (maxMessageSize > 0 && messageSize > maxMessageSize)
		{
			throw new ProcessorException(Res.Format(Resources.Exception_NamedPipeIoProcessorMessageSizeExceedsLimit, messageSize, maxMessageSize));
		}

		var buffer = ArrayPool<byte>.Shared.Rent(sizeof(long) + messageSize);

		try
		{
			BitConverter.TryWriteBytes(buffer, (long) messageSize);
			inMemoryStorage.WriteTransactionLogToSpan(buffer.AsSpan()[sizeof(long)..], truncateLog: false);

			await pipeStream.WriteAsync(buffer, offset: 0, sizeof(long) + messageSize, token).ConfigureAwait(false);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
		}
	}
}