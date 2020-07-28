#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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
using System.Collections.Generic;
using System.Linq;

namespace Xtate.Persistence
{
	internal class InMemoryStorage : IStorage
	{
		private IMemoryOwner<byte>?                         _baselineOwner;
		private Memory<byte>                                _buffer;
		private List<(IMemoryOwner<byte> Owner, int Size)>? _buffers;
		private bool                                        _disposed;
		private IMemoryOwner<byte>?                         _owner;
		private SortedSet<Entry>?                           _readModel;

		public InMemoryStorage(ReadOnlySpan<byte> baseline)
		{
			_readModel = CreateReadModel();

			if (!baseline.IsEmpty)
			{
				_baselineOwner = MemoryPool<byte>.Shared.Rent(baseline.Length);
				var memory = _baselineOwner.Memory.Slice(start: 0, baseline.Length);
				baseline.CopyTo(memory.Span);
				while (!baseline.IsEmpty)
				{
					var keyLengthLength = Encode.GetLength(baseline[0]);
					var keyLength = Encode.Decode(baseline.Slice(start: 0, keyLengthLength));
					var key = memory.Slice(keyLengthLength, keyLength);

					var valueLengthLength = Encode.GetLength(baseline[keyLengthLength + keyLength]);
					var valueLength = Encode.Decode(baseline.Slice(keyLengthLength + keyLength, valueLengthLength));
					var value = memory.Slice(keyLengthLength + keyLength + valueLengthLength, valueLength);

					AddToReadModel(key, value);

					var rowSize = keyLengthLength + keyLength + valueLengthLength + valueLength;
					baseline = baseline.Slice(rowSize);
					memory = memory.Slice(rowSize);
				}
			}
		}

		public InMemoryStorage(bool writeOnly = true)
		{
			if (!writeOnly)
			{
				_readModel = CreateReadModel();
			}
		}

	#region Interface IDisposable

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			TruncateLog(true);

			_baselineOwner?.Dispose();

			_disposed = true;
		}

	#endregion

	#region Interface IStorage

		public void Write(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
		{
			var keyLengthLength = Encode.GetEncodedLength(key.Length);
			var valueLengthLength = Encode.GetEncodedLength(value.Length);

			var memory = AllocateBuffer(key.Length + value.Length + keyLengthLength + valueLengthLength);

			var keyLenMemory = memory.Slice(start: 0, keyLengthLength);
			var keyMemory = memory.Slice(keyLengthLength, key.Length);
			var valueLenMemory = memory.Slice(keyLengthLength + key.Length, valueLengthLength);
			var valueMemory = memory.Slice(keyLengthLength + key.Length + valueLengthLength, value.Length);

			Encode.WriteEncodedValue(keyLenMemory.Span, key.Length);
			key.CopyTo(keyMemory.Span);
			Encode.WriteEncodedValue(valueLenMemory.Span, value.Length);
			value.CopyTo(valueMemory.Span);

			AddToReadModel(keyMemory, valueMemory);
		}

		public ReadOnlyMemory<byte> Read(ReadOnlySpan<byte> key)
		{
			if (_readModel == null)
			{
				throw new InvalidOperationException(Resources.Exception_Storage_not_available_for_read_operations);
			}

			var buffer = AllocateBuffer(key.Length, shared: true);
			key.CopyTo(buffer.Span);

			return ReadModelTryGetValue(_readModel, new Entry(buffer), out var result) ? result.Value : ReadOnlyMemory<byte>.Empty;
		}

	#endregion

		private static bool ReadModelTryGetValue(SortedSet<Entry> readModel, Entry equalEntry, out Entry actualEntry)
		{
#if NETSTANDARD2_1
			return readModel.TryGetValue(equalEntry, out actualEntry);
#else
			foreach (var entry in readModel.GetViewBetween(equalEntry, equalEntry))
			{
				actualEntry = entry;

				return true;
			}

			actualEntry = default;

			return false;
#endif
		}

		private static SortedSet<Entry> CreateReadModel() => new SortedSet<Entry>();

		private void AddToReadModel(Memory<byte> key, Memory<byte> value)
		{
			if (_readModel == null)
			{
				return;
			}

			if (key.IsEmpty)
			{
				if (value.IsEmpty)
				{
					_readModel.Clear();
				}
				else
				{
					var from = new Entry(value);
					var to = new Entry(GetTo(value));

					if (ReadModelTryGetValue(_readModel, to, out var toValue))
					{
						_readModel.GetViewBetween(from, to).Clear();
						_readModel.Add(toValue);
					}
					else
					{
						_readModel.GetViewBetween(from, to).Clear();
					}
				}
			}
			else if (value.IsEmpty)
			{
				_readModel.Remove(new Entry(key));
			}
			else
			{
				_readModel.Remove(new Entry(key));
				_readModel.Add(new Entry(key, value));
			}
		}

		private Memory<byte> GetTo(Memory<byte> from)
		{
			var to = AllocateBuffer(from.Length, shared: true);
			from.CopyTo(to);
			var span = to.Span;

			for (var i = span.Length - 1; i >= 0; i --)
			{
				if (span[i] == 0xFF)
				{
					continue;
				}

				span[i] ++;

				return to.Slice(start: 0, i + 1);
			}

			return Memory<byte>.Empty;
		}

		private Memory<byte> AllocateBuffer(int size, bool shared = false)
		{
			if (_buffer.Length < size)
			{
				if (_owner != null)
				{
					_buffers ??= new List<(IMemoryOwner<byte> Owner, int Size)>();

					_buffers.Add((_owner, _owner.Memory.Length - _buffer.Length));
				}

				_owner = MemoryPool<byte>.Shared.Rent();

				if (_owner.Memory.Length < size)
				{
					_owner.Dispose();
					_owner = MemoryPool<byte>.Shared.Rent(size);
				}

				_buffer = _owner.Memory;
			}

			var result = _buffer.Slice(start: 0, size);

			if (!shared)
			{
				_buffer = _buffer.Slice(size);
			}

			return result;
		}

		public int GetTransactionLogSize() => (_owner != null ? _owner.Memory.Length - _buffer.Length : 0) + (_buffers?.Select(b => b.Size).Sum() ?? 0);

		public void WriteTransactionLogToSpan(Span<byte> span, bool truncateLog = true)
		{
			if (_buffers != null)
			{
				foreach (var (owner, size) in _buffers)
				{
					owner.Memory.Slice(start: 0, size).Span.CopyTo(span);
					span = span.Slice(size);
				}
			}

			if (_owner != null)
			{
				var memory = _owner.Memory.Span;
				memory.Slice(start: 0, memory.Length - _buffer.Length).CopyTo(span);
			}

			if (truncateLog)
			{
				TruncateLog(false);
			}
		}

		public int GetDataSize()
		{
			if (_readModel == null)
			{
				throw new InvalidOperationException(Resources.Exception_Storage_not_available_for_read_operations);
			}

			return _readModel.Sum(p => Encode.GetEncodedLength(p.Key.Length) + p.Key.Length + Encode.GetEncodedLength(p.Value.Length) + p.Value.Length);
		}

		public void WriteDataToSpan(Span<byte> span, bool shrink = true)
		{
			if (_readModel == null)
			{
				throw new InvalidOperationException(Resources.Exception_Storage_not_available_for_read_operations);
			}

			IMemoryOwner<byte>? newBaselineOwner = null;
			var newBaseline = Memory<byte>.Empty;
			SortedSet<Entry>? newReadModel = null;
			if (shrink)
			{
				newBaselineOwner = MemoryPool<byte>.Shared.Rent(_readModel.Sum(p => p.Key.Length + p.Value.Length));
				newBaseline = newBaselineOwner.Memory;
				newReadModel = CreateReadModel();
			}

			foreach (var pair in _readModel)
			{
				var keyLengthLength = Encode.GetEncodedLength(pair.Key.Length);
				var keyLenSpan = span.Slice(start: 0, keyLengthLength);
				var keySpan = span.Slice(keyLengthLength, pair.Key.Length);

				Encode.WriteEncodedValue(keyLenSpan, pair.Key.Length);
				pair.Key.Span.CopyTo(keySpan);

				var valueLengthLength = Encode.GetEncodedLength(pair.Value.Length);
				var valueLenSpan = span.Slice(keyLengthLength + pair.Key.Length, valueLengthLength);
				var valueSpan = span.Slice(keyLengthLength + pair.Key.Length + valueLengthLength, pair.Value.Length);

				Encode.WriteEncodedValue(valueLenSpan, pair.Value.Length);
				pair.Value.Span.CopyTo(valueSpan);

				if (shrink)
				{
					var key = newBaseline.Slice(start: 0, pair.Key.Length);
					var value = newBaseline.Slice(pair.Key.Length, pair.Value.Length);
					pair.Key.CopyTo(key);
					pair.Value.CopyTo(value);
					newReadModel!.Add(new Entry(key, value));
					newBaseline = newBaseline.Slice(pair.Key.Length + pair.Value.Length);
				}
			}

			if (shrink)
			{
				TruncateLog(true);

				_readModel = newReadModel!;
				_baselineOwner?.Dispose();
				_baselineOwner = newBaselineOwner!;
			}
		}

		private void TruncateLog(bool forceDispose)
		{
			if (_readModel == null || forceDispose)
			{
				if (_buffers != null)
				{
					foreach (var (owner, _) in _buffers)
					{
						owner.Dispose();
					}
				}

				_owner?.Dispose();
				_owner = null;

				_buffers = null;
				_buffer = Memory<byte>.Empty;
			}
			else
			{
				if (_buffers != null)
				{
					for (var i = 0; i < _buffers.Count; i ++)
					{
						_buffers[i] = (_buffers[i].Owner, 0);
					}
				}

				if (_owner != null)
				{
					_buffers ??= new List<(IMemoryOwner<byte> Owner, int Size)>();

					_buffers.Add((_owner, 0));
				}

				_owner = null;
				_buffer = Memory<byte>.Empty;
			}
		}

		private readonly struct Entry : IComparable<Entry>
		{
			public readonly ReadOnlyMemory<byte> Key;
			public readonly ReadOnlyMemory<byte> Value;
			public Entry(ReadOnlyMemory<byte> key) : this() => Key = key;

			public Entry(ReadOnlyMemory<byte> key, ReadOnlyMemory<byte> value)
			{
				Key = key;
				Value = value;
			}

		#region Interface IComparable<Entry>

			public int CompareTo(Entry other)
			{
				if (Key.IsEmpty)
				{
					return other.Key.IsEmpty ? 0 : 1;
				}

				if (other.Key.IsEmpty)
				{
					return -1;
				}

				return Key.Span.SequenceCompareTo(other.Key.Span);
			}

		#endregion
		}
	}
}