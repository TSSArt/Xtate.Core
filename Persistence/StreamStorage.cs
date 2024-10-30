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
using System.IO;
using Xtate.IoC;

namespace Xtate.Persistence;

public class StreamStorage : ITransactionalStorage, IAsyncInitialization
{
	private const byte SkipMark = 0;

	private const int SkipBlockMark = 2;

	private const byte FinalMark = 4;

	private const int FinalMarkLength = 1;

	private static readonly int MaxInt32Length = Encode.GetEncodedLength(int.MaxValue);

	private readonly bool _disposeStream;

	private readonly DisposingToken _disposingToken = new();

	private readonly AsyncInit<InMemoryStorage> _inMemoryStorageAsyncInit;

	private readonly int _rollbackLevel;

	private readonly Stream _stream;

	private bool _canShrink = true;

	private bool _disposed;

	public StreamStorage(Stream stream, bool disposeStream = true, int? rollbackLevel = default)
	{
		Infra.Requires(stream);

		_stream = stream;
		_disposeStream = disposeStream;
		_rollbackLevel = rollbackLevel ?? int.MaxValue;

		if (!stream.CanRead || !stream.CanWrite || !stream.CanSeek)
		{
			throw new ArgumentException(Resources.Exception_StreamShouldSupportReadWriteSeekOperations, nameof(stream));
		}

		_inMemoryStorageAsyncInit = AsyncInit.Run(this, storage => storage.Init());
	}

	public required Func<bool, InMemoryStorage> InMemoryStorageFactory { private get; [UsedImplicitly] init; }

	public required Func<ReadOnlyMemory<byte>, InMemoryStorage> InMemoryStorageBaselineFactory { private get; [UsedImplicitly] init; }

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IAsyncInitialization

	public Task Initialization => _inMemoryStorageAsyncInit.Task;

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IStorage

	public ReadOnlyMemory<byte> Get(ReadOnlySpan<byte> key) => _inMemoryStorageAsyncInit.Value.Get(key);

	public void Set(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value) => _inMemoryStorageAsyncInit.Value.Set(key, value);

	public void Remove(ReadOnlySpan<byte> key) => _inMemoryStorageAsyncInit.Value.Remove(key);

	public void RemoveAll(ReadOnlySpan<byte> prefix) => _inMemoryStorageAsyncInit.Value.RemoveAll(prefix);

#endregion

#region Interface ITransactionalStorage

	public async ValueTask CheckPoint(int level)
	{
		Infra.RequiresNonNegative(level);

		var transactionLogSize = _inMemoryStorageAsyncInit.Value.GetTransactionLogSize();

		if (transactionLogSize == 0)
		{
			return;
		}

		if (level == 0)
		{
			_canShrink = true;
		}

		var buf = ArrayPool<byte>.Shared.Rent(transactionLogSize + 2 * MaxInt32Length);

		try
		{
			var mark = (level << 1) + 1;
			var markSizeLength = GetMarkSizeLength(mark, transactionLogSize);
			WriteMarkSize(buf.AsSpan(start: 0, markSizeLength), mark, transactionLogSize);

			_inMemoryStorageAsyncInit.Value.WriteTransactionLogToSpan(buf.AsSpan(markSizeLength));

			await _stream.WriteAsync(buf, offset: 0, markSizeLength + transactionLogSize, _disposingToken.Token).ConfigureAwait(false);
			await _stream.FlushAsync(_disposingToken.Token).ConfigureAwait(false);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buf);
		}
	}

	public async ValueTask Shrink()
	{
		if (_canShrink)
		{
			await ReadStream(rollbackLevel: 0, shrink: true).ConfigureAwait(false);

			_canShrink = false;
		}
	}

#endregion

	private ValueTask<InMemoryStorage> Init() => ReadStream(_rollbackLevel, shrink: false)!;

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			_disposingToken.Dispose();

			if (_disposeStream)
			{
				_stream.Dispose();
			}

			_disposed = true;
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (!_disposed)
		{
			_disposingToken.Dispose();

			if (_disposeStream)
			{
				await _stream.DisposeAsync().ConfigureAwait(false);
			}

			_disposed = true;
		}
	}

	private async ValueTask<InMemoryStorage?> ReadStream(int rollbackLevel, bool shrink)
	{
		var total = 0;
		var end = 0;
		var streamTotal = 0;
		var streamEnd = 0;

		var streamLength = (int) _stream.Length;

		if (streamLength == 0)
		{
			return shrink ? null : InMemoryStorageFactory(false);
		}

		var buf = ArrayPool<byte>.Shared.Rent(streamLength + 8 * MaxInt32Length);

		try
		{
			var token = _disposingToken.Token;
			var memoryOffset = 0;

			_stream.Seek(offset: 0, SeekOrigin.Begin);

			while (true)
			{
				var len = await _stream.ReadAsync(buf, memoryOffset, count: 1, token).ConfigureAwait(false);

				if (len == 0)
				{
					break;
				}

				var byteMark = buf[memoryOffset];

				if (byteMark == FinalMark)
				{
					break;
				}

				if (byteMark == SkipMark)
				{
					continue;
				}

				var levelLength = Encode.GetLength(byteMark);

				if (levelLength > 1 && await _stream.ReadAsync(buf, memoryOffset + 1, levelLength - 1, token).ConfigureAwait(false) < levelLength - 1)
				{
					throw GetIncorrectDataFormatException();
				}

				var level = Encode.Decode(buf.AsSpan(memoryOffset, levelLength));

				if (await _stream.ReadAsync(buf, memoryOffset, count: 1, token).ConfigureAwait(false) < 1)
				{
					throw GetIncorrectDataFormatException();
				}

				var sizeLength = Encode.GetLength(buf[memoryOffset]);

				if (sizeLength > 1 && await _stream.ReadAsync(buf, memoryOffset + 1, sizeLength - 1, token).ConfigureAwait(false) < sizeLength - 1)
				{
					throw GetIncorrectDataFormatException();
				}

				var size = Encode.Decode(buf.AsSpan(memoryOffset, sizeLength));

				if (level == SkipBlockMark)
				{
					_stream.Seek(size, SeekOrigin.Current);

					continue;
				}

				if (await _stream.ReadAsync(buf, memoryOffset, size, token).ConfigureAwait(false) < size)
				{
					throw GetIncorrectDataFormatException();
				}

				total += size;
				streamTotal += levelLength + sizeLength + size;
				memoryOffset += size;

				if (level >> 1 <= rollbackLevel)
				{
					end = total;
					streamEnd = streamTotal;
				}
			}

			if (!shrink)
			{
				if (streamEnd < streamLength)
				{
					_stream.SetLength(streamEnd);
					await _stream.FlushAsync(token).ConfigureAwait(false);
				}

				return InMemoryStorageBaselineFactory(buf.AsMemory(start: 0, end));
			}

			if (streamTotal < streamLength)
			{
				_stream.SetLength(streamTotal);
			}

			using var baseline = InMemoryStorageBaselineFactory(buf.AsMemory(start: 0, end));
			var dataSize = baseline.GetDataSize();

			if (dataSize >= end)
			{
				return null;
			}

			buf[0] = FinalMark;
			memoryOffset = FinalMarkLength;

			var tranSize = streamTotal - streamEnd;

			var controlDataSize = dataSize > 0 ? GetMarkSizeLength(mark: 1, dataSize) : 0;

			if (dataSize > 0)
			{
				WriteMarkSize(buf.AsSpan(memoryOffset, controlDataSize), mark: 1, dataSize);
				memoryOffset += controlDataSize;

				baseline.WriteDataToSpan(buf.AsSpan(memoryOffset, dataSize));
				memoryOffset += dataSize;
			}

			if (tranSize > 0)
			{
				_stream.Seek(streamEnd, SeekOrigin.Begin);
				var count = await _stream.ReadAsync(buf, memoryOffset, tranSize, token).ConfigureAwait(false);
				Infra.Assert(count == tranSize);
				memoryOffset += tranSize;
			}

			buf[memoryOffset] = FinalMark;
			memoryOffset += FinalMarkLength;

			_stream.Seek(offset: 0, SeekOrigin.End);
			var extLength = FinalMarkLength + controlDataSize + dataSize + tranSize;
			await _stream.WriteAsync(buf, offset: 0, extLength, token).ConfigureAwait(false);
			await _stream.FlushAsync(token).ConfigureAwait(false);

			var bypassLength = streamTotal + FinalMarkLength;
			var initBlockLength1 = GetMarkSizeLength(SkipBlockMark, bypassLength);
			var initBlockLength = bypassLength < initBlockLength1 ? bypassLength : initBlockLength1;
			Array.Clear(buf, memoryOffset, initBlockLength);

			if (bypassLength >= initBlockLength1)
			{
				bypassLength -= initBlockLength1;
				var initBlockLength2 = GetMarkSizeLength(SkipBlockMark, bypassLength);
				var delta = initBlockLength1 - initBlockLength2;
				WriteMarkSize(buf.AsSpan(memoryOffset + delta, initBlockLength - delta), SkipBlockMark, bypassLength);
			}

			_stream.Seek(offset: 0, SeekOrigin.Begin);
			await _stream.WriteAsync(buf, memoryOffset, initBlockLength, token).ConfigureAwait(false);
			await _stream.FlushAsync(token).ConfigureAwait(false);

			var bufOffset = FinalMarkLength + initBlockLength;
			var bufLength = controlDataSize + dataSize + tranSize + FinalMarkLength - initBlockLength;

			if (bufLength > 0)
			{
				await _stream.WriteAsync(buf, bufOffset, bufLength, token).ConfigureAwait(false);
				await _stream.FlushAsync(token).ConfigureAwait(false);
			}

			_stream.Seek(offset: 0, SeekOrigin.Begin);
			await _stream.WriteAsync(buf, FinalMarkLength, initBlockLength, token).ConfigureAwait(false);
			await _stream.FlushAsync(token).ConfigureAwait(false);

			_stream.SetLength(controlDataSize + dataSize + tranSize);
			await _stream.FlushAsync(token).ConfigureAwait(false);

			return null;
		}
		catch (ArgumentOutOfRangeException ex)
		{
			throw GetIncorrectDataFormatException(ex);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buf);
		}
	}

	private static int GetMarkSizeLength(int mark, int? size = default) => Encode.GetEncodedLength(mark) + (size is { } s ? Encode.GetEncodedLength(s) : 0);

	private static void WriteMarkSize(Span<byte> span, int mark, int? size = default)
	{
		Encode.WriteEncodedValue(span, mark);

		if (size is { } s)
		{
			Encode.WriteEncodedValue(span[Encode.GetEncodedLength(mark)..], s);
		}
	}

	private static PersistenceException GetIncorrectDataFormatException(Exception? ex = default) => new(Resources.Exception_IncorrectDataFormat, ex);
}