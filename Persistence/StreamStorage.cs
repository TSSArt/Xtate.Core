#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.Persistence
{
	[PublicAPI]
	internal sealed class StreamStorage : ITransactionalStorage
	{
		private const byte SkipMark        = 0;
		private const int  SkipBlockMark   = 2;
		private const byte FinalMark       = 4;
		private const int  FinalMarkLength = 1;

		private static readonly int MaxInt32Length = Encode.GetEncodedLength(int.MaxValue);

		private readonly Stream           _stream;
		private readonly bool             _disposeStream;
		private          bool             _canShrink = true;
		private          InMemoryStorage? _inMemoryStorage;
		private          bool             _disposed;

		private StreamStorage(Stream stream, bool disposeStream)
		{
			_stream = stream ?? throw new ArgumentNullException(nameof(stream));
			_disposeStream = disposeStream;

			if (!stream.CanRead || !stream.CanWrite || !stream.CanSeek)
			{
				throw new ArgumentException(Resources.Exception_StreamShouldSupportReadWriteSeekOperations, nameof(stream));
			}
		}

		public ReadOnlyMemory<byte> Read(ReadOnlySpan<byte> key)
		{
			Infrastructure.Assert(_inMemoryStorage != null);

			return _inMemoryStorage.Read(key);
		}

		public void Write(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
		{
			Infrastructure.Assert(_inMemoryStorage != null);

			_inMemoryStorage.Write(key, value);
		}

		public async ValueTask CheckPoint(int level, CancellationToken token)
		{
			if (level < 0) throw new ArgumentOutOfRangeException(nameof(level));

			Infrastructure.Assert(_inMemoryStorage != null);

			var transactionLogSize = _inMemoryStorage.GetTransactionLogSize();

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

				_inMemoryStorage.WriteTransactionLogToSpan(buf.AsSpan(markSizeLength));

				await _stream.WriteAsync(buf, offset: 0, markSizeLength + transactionLogSize, token).ConfigureAwait(false);
				await _stream.FlushAsync(token).ConfigureAwait(false);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buf);
			}
		}

		public async ValueTask Shrink(CancellationToken token)
		{
			if (_canShrink)
			{
				await ReadStream(_stream, rollbackLevel: 0, shrink: true, token).ConfigureAwait(false);

				_canShrink = false;
			}
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			if (_disposeStream)
			{
				_stream.Dispose();
			}

			_inMemoryStorage?.Dispose();

			_disposed = true;
		}

#if NETSTANDARD2_1
		public async ValueTask DisposeAsync()
		{
			if (_disposed)
			{
				return;
			}

			if (_disposeStream)
			{
				await _stream.DisposeAsync().ConfigureAwait(false);
			}

			_inMemoryStorage?.Dispose();

			_disposed = true;
		}
#else
		public ValueTask DisposeAsync()
		{
			try
			{
				Dispose();

				return default;
			}
			catch (Exception ex)
			{
				return new ValueTask(Task.FromException(ex));
			}
		}
#endif

		public static async ValueTask<StreamStorage> CreateAsync(Stream stream, bool disposeStream = true, CancellationToken token = default) =>
				new StreamStorage(stream, disposeStream)
				{
						_inMemoryStorage = await ReadStream(stream, int.MaxValue, shrink: false, token).ConfigureAwait(false)
				};

		public static async ValueTask<StreamStorage> CreateWithRollbackAsync(Stream stream, int rollbackLevel, bool disposeStream = true, CancellationToken token = default)
		{
			if (rollbackLevel < 0) throw new ArgumentOutOfRangeException(nameof(rollbackLevel));

			return new StreamStorage(stream, disposeStream)
				   {
						   _inMemoryStorage = await ReadStream(stream, rollbackLevel, shrink: false, token).ConfigureAwait(false)
				   };
		}

		[SuppressMessage(category: "ReSharper", checkId: "UncatchableException", Justification = "Handle ArgumentOutOfRangeException")]
		[SuppressMessage(category: "ReSharper", checkId: "CyclomaticComplexity", Justification = "Hard to decompose")]
		private static async ValueTask<InMemoryStorage?> ReadStream(Stream stream, int rollbackLevel, bool shrink, CancellationToken token)
		{
			var total = 0;
			var end = 0;
			var streamTotal = 0;
			var streamEnd = 0;

			var streamLength = (int) stream.Length;

			if (streamLength == 0)
			{
				return shrink ? null : new InMemoryStorage(false);
			}

			var buf = ArrayPool<byte>.Shared.Rent(streamLength + 8 * MaxInt32Length);
			try
			{
				var memoryOffset = 0;

				stream.Seek(offset: 0, SeekOrigin.Begin);
				while (true)
				{
					var len = await stream.ReadAsync(buf, memoryOffset, count: 1, token).ConfigureAwait(false);
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
					if (levelLength > 1 && await stream.ReadAsync(buf, memoryOffset + 1, levelLength - 1, token).ConfigureAwait(false) < levelLength - 1)
					{
						throw GetIncorrectDataFormatException();
					}

					var level = Encode.Decode(buf.AsSpan(memoryOffset, levelLength));
					if (await stream.ReadAsync(buf, memoryOffset, count: 1, token).ConfigureAwait(false) < 1)
					{
						throw GetIncorrectDataFormatException();
					}

					var sizeLength = Encode.GetLength(buf[memoryOffset]);
					if (sizeLength > 1 && await stream.ReadAsync(buf, memoryOffset + 1, sizeLength - 1, token).ConfigureAwait(false) < sizeLength - 1)
					{
						throw GetIncorrectDataFormatException();
					}

					var size = Encode.Decode(buf.AsSpan(memoryOffset, sizeLength));

					if (level == SkipBlockMark)
					{
						stream.Seek(size, SeekOrigin.Current);
						continue;
					}

					if (await stream.ReadAsync(buf, memoryOffset, size, token).ConfigureAwait(false) < size)
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
						stream.SetLength(streamEnd);
						await stream.FlushAsync(token).ConfigureAwait(false);
					}

					return new InMemoryStorage(buf.AsSpan(start: 0, end));
				}

				if (streamTotal < streamLength)
				{
					stream.SetLength(streamTotal);
				}

				using var baseline = new InMemoryStorage(buf.AsSpan(start: 0, end));
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
					stream.Seek(streamEnd, SeekOrigin.Begin);
					await stream.ReadAsync(buf, memoryOffset, tranSize, token).ConfigureAwait(false);
					memoryOffset += tranSize;
				}

				buf[memoryOffset] = FinalMark;
				memoryOffset += FinalMarkLength;

				stream.Seek(offset: 0, SeekOrigin.End);
				var extLength = FinalMarkLength + controlDataSize + dataSize + tranSize;
				await stream.WriteAsync(buf, offset: 0, extLength, token).ConfigureAwait(false);
				await stream.FlushAsync(token).ConfigureAwait(false);

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

				stream.Seek(offset: 0, SeekOrigin.Begin);
				await stream.WriteAsync(buf, memoryOffset, initBlockLength, token).ConfigureAwait(false);
				await stream.FlushAsync(token).ConfigureAwait(false);

				var bufOffset = FinalMarkLength + initBlockLength;
				var bufLength = controlDataSize + dataSize + tranSize + FinalMarkLength - initBlockLength;
				if (bufLength > 0)
				{
					await stream.WriteAsync(buf, bufOffset, bufLength, token).ConfigureAwait(false);
					await stream.FlushAsync(token).ConfigureAwait(false);
				}

				stream.Seek(offset: 0, SeekOrigin.Begin);
				await stream.WriteAsync(buf, FinalMarkLength, initBlockLength, token).ConfigureAwait(false);
				await stream.FlushAsync(token).ConfigureAwait(false);

				stream.SetLength(controlDataSize + dataSize + tranSize);
				await stream.FlushAsync(token).ConfigureAwait(false);

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

		private static int GetMarkSizeLength(int mark, int? size = default) => Encode.GetEncodedLength(mark) + (size != null ? Encode.GetEncodedLength(size.Value) : 0);

		private static void WriteMarkSize(Span<byte> span, int mark, int? size = default)
		{
			Encode.WriteEncodedValue(span, mark);

			if (size != null)
			{
				Encode.WriteEncodedValue(span.Slice(Encode.GetEncodedLength(mark)), size.Value);
			}
		}

		private static PersistenceException GetIncorrectDataFormatException(Exception? ex = default) => new PersistenceException(Resources.Exception_Incorrect_data_format, ex);
	}
}