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

using System.IO;

namespace Xtate.Persistence.Services;

[InstantiatedByIoC]
public class SharedMemoryStreams<TKey> where TKey : notnull
{
	private readonly ConcurrentDictionary<TKey, (State, MemoryStream)> _streams = new();

	public Stream OpenRead(TKey key)
	{
		var (_, stream) = _streams.AddOrUpdate(key, _ => (State.Reading, new MemoryStream()), UpdateValueFactory);

		return new ReadOnlyMemoryStream(key, stream, this);

		static (State, MemoryStream) UpdateValueFactory(TKey key, (State State, MemoryStream Stream) current)
		{
			if (current.State is State.Writing)
			{
				throw new IOException(Res.Format(Resources.Exception_TheStreamIsCurrentlyBeingWrittenTo, key));
			}

			return (current.State + 1, current.Stream);
		}
	}

	public Stream OpenWrite(TKey key)
	{
		var (_, stream) = _streams.AddOrUpdate(key, _ => (State.Writing, new MemoryStream()), UpdateValueFactory);

		return new ReadWriteMemoryStream(key, stream, this);

		static (State, MemoryStream) UpdateValueFactory(TKey key, (State State, MemoryStream Stream) current)
		{
			if (current.State is not State.Free)
			{
				throw new IOException(Res.Format(Resources.Exception_TheStreamIsCurrentlyBeingReadFromOrWrittenTo, key));
			}

			return (State.Writing, current.Stream);
		}
	}

	public bool Delete(TKey key)
	{
		while (_streams.TryGetValue(key, out (State State, MemoryStream Stream) current))
		{
			if (current.State is not State.Free)
			{
				throw new IOException(Res.Format(Resources.Exception_TheStreamIsCurrentlyBeingReadFromOrWrittenTo, key));
			}

			if (_streams.TryRemove(new KeyValuePair<TKey, (State, MemoryStream)>(key, current)))
			{
				current.Stream.Dispose();

				return true;
			}
		}

		return false;
	}

	public int DeleteAll(Func<TKey, bool> predicate)
	{
		var count = 0;

		foreach (var key in _streams.Keys)
		{
			if (predicate(key) && Delete(key))
			{
				count ++;
			}
		}

		return count;
	}

	private enum State
	{
		Free = 0,

		Reading = 1,

		Writing = -1
	}

	private class ReadOnlyMemoryStream(TKey key, MemoryStream stream, SharedMemoryStreams<TKey> owner)
		: MemoryStream(stream.GetBuffer(), index: 0, (int) stream.Length, writable: false, publiclyVisible: false)
	{
		private bool _disposed;

		private TKey Key { get; } = key;

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;

				while (true)
				{
					var (state, _) = owner._streams[Key];

					Infra.Assert(state is not (State.Free or State.Writing));

					if (owner._streams.TryUpdate(Key, (state - 1, stream), (state, stream)))
					{
						break;
					}
				}
			}

			base.Dispose(disposing);
		}
	}

	private class ReadWriteMemoryStream(TKey key, MemoryStream stream, SharedMemoryStreams<TKey> owner) : Stream
	{
		private bool _opened = true;

		private TKey Key { get; } = key;

		public override bool CanRead => _opened && stream.CanRead;

		public override bool CanSeek => _opened && stream.CanSeek;

		public override bool CanWrite => _opened && stream.CanWrite;

		public override long Length
		{
			get
			{
				EnsureOpened();

				return stream.Length;
			}
		}

		public override long Position
		{
			get
			{
				EnsureOpened();

				return stream.Position;
			}
			set
			{
				EnsureOpened();

				stream.Position = value;
			}
		}

		private void EnsureOpened()
		{
			if (!_opened)
			{
				throw new ObjectDisposedException(nameof(ReadWriteMemoryStream));
			}
		}

		public override void Flush()
		{
			EnsureOpened();

			stream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			EnsureOpened();

			return stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			EnsureOpened();

			return stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			EnsureOpened();

			stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			EnsureOpened();

			stream.Write(buffer, offset, count);
		}

		public override int ReadByte()
		{
			EnsureOpened();

			return stream.ReadByte();
		}

		public override void WriteByte(byte value)
		{
			EnsureOpened();

			stream.WriteByte(value);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _opened)
			{
				_opened = false;

				var result = owner._streams.TryUpdate(Key, (State.Free, stream), (State.Writing, stream));

				Infra.Assert(result);
			}

			base.Dispose(disposing);
		}

		public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
		{
			EnsureOpened();

			return stream.CopyToAsync(destination, bufferSize, cancellationToken);
		}

		public override Task FlushAsync(CancellationToken cancellationToken)
		{
			EnsureOpened();

			return stream.FlushAsync(cancellationToken);
		}

		public override Task<int> ReadAsync(byte[] buffer,
											int offset,
											int count,
											CancellationToken cancellationToken)
		{
			EnsureOpened();

			return stream.ReadAsync(buffer, offset, count, cancellationToken);
		}

		public override async Task WriteAsync(byte[] buffer,
											  int offset,
											  int count,
											  CancellationToken cancellationToken)
		{
			EnsureOpened();

			await stream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
		}

#if NETSTANDARD2_1 || NETCOREAPP2_0_OR_GREATER

		public override void CopyTo(Stream destination, int bufferSize)
		{
			EnsureOpened();

			stream.CopyTo(destination, bufferSize);
		}

		public override int Read(Span<byte> buffer)
		{
			EnsureOpened();

			return stream.Read(buffer);
		}

		public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new())
		{
			EnsureOpened();

			return stream.ReadAsync(buffer, cancellationToken);
		}

		public override void Write(ReadOnlySpan<byte> buffer)
		{
			EnsureOpened();

			stream.Write(buffer);
		}

		public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new())
		{
			EnsureOpened();

			return stream.WriteAsync(buffer, cancellationToken);
		}

#endif
	}
}