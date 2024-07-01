// Copyright © 2019-2023 Sergii Artemenko
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

namespace Xtate.Persistence;

public class InMemoryStorage : IStorage
{
<<<<<<< Updated upstream
	public class InMemoryStorage : IStorage
	{
		private IMemoryOwner<byte>?                         _baselineOwner;
		private Memory<byte>                                _buffer;
		private List<(IMemoryOwner<byte> Owner, int Size)>? _buffers;
		private bool                                        _disposed;
		private IMemoryOwner<byte>?                         _owner;
		private SortedSet<Entry>?                           _readModel;

		public InMemoryStorage(ReadOnlySpan<byte> baseline) : this(false)
		{
			if (!baseline.IsEmpty)
			{
				InitFromBaseline(baseline);
			}
		}

		private void InitFromBaseline(ReadOnlySpan<byte> baseline)
		{
			_baselineOwner = MemoryPool<byte>.Shared.Rent(baseline.Length);
			var memory = _baselineOwner.Memory[..baseline.Length];
			baseline.CopyTo(memory.Span);
			while (!baseline.IsEmpty)
			{
				var keyLengthLength = Encode.GetLength(baseline[0]);
				var keyLength = Encode.Decode(baseline[..keyLengthLength]);
				var key = memory.Slice(keyLengthLength, keyLength);

				var valueLengthLength = Encode.GetLength(baseline[keyLengthLength + keyLength]);
				var valueLength = Encode.Decode(baseline.Slice(keyLengthLength + keyLength, valueLengthLength));
				var value = memory.Slice(keyLengthLength + keyLength + valueLengthLength, valueLength);

				AddToReadModel(key, value);

				var rowSize = keyLengthLength + keyLength + valueLengthLength + valueLength;
				baseline = baseline[rowSize..];
				memory = memory[rowSize..];
			}
=======
	private IMemoryOwner<byte>?                         _baselineOwner;
	private Memory<byte>                                _buffer;
	private List<(IMemoryOwner<byte> Owner, int Size)>? _buffers;
	private bool                                        _disposed;
	private IMemoryOwner<byte>?                         _owner;
	private SortedSet<Entry>?                           _readModel;

	public InMemoryStorage(ReadOnlySpan<byte> baseline) : this(false)
	{
		if (!baseline.IsEmpty)
		{
			InitFromBaseline(baseline);
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
		Dispose(true);

		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IStorage

	public void Set(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
	{
		if (key.IsEmpty) throw new ArgumentException(Resources.Exception_KeyShouldNotBeEmpty, nameof(key));

		Write(key, value);
	}

	public void Remove(ReadOnlySpan<byte> key)
	{
		if (key.IsEmpty) throw new ArgumentException(Resources.Exception_KeyShouldNotBeEmpty, nameof(key));

		Write(key, []);
	}

	public void RemoveAll(ReadOnlySpan<byte> prefix) => Write([], prefix);

	public ReadOnlyMemory<byte> Get(ReadOnlySpan<byte> key)
	{
		if (_readModel is null)
		{
			throw new InvalidOperationException(Resources.Exception_StorageNotAvailableForReadOperations);
>>>>>>> Stashed changes
		}

		var buffer = AllocateBuffer(key.Length, share: true);
		key.CopyTo(buffer.Span);

		return _readModel.TryGetValue(new Entry(buffer), out var result) ? result.Value : ReadOnlyMemory<byte>.Empty;
	}

#endregion

	private void InitFromBaseline(ReadOnlySpan<byte> baseline)
	{
		_baselineOwner = MemoryPool<byte>.Shared.Rent(baseline.Length);
		var memory = _baselineOwner.Memory[..baseline.Length];
		baseline.CopyTo(memory.Span);
		while (!baseline.IsEmpty)
		{
			var keyLengthLength = Encode.GetLength(baseline[0]);
			var keyLength = Encode.Decode(baseline[..keyLengthLength]);
			var key = memory.Slice(keyLengthLength, keyLength);

			var valueLengthLength = Encode.GetLength(baseline[keyLengthLength + keyLength]);
			var valueLength = Encode.Decode(baseline.Slice(keyLengthLength + keyLength, valueLengthLength));
			var value = memory.Slice(keyLengthLength + keyLength + valueLengthLength, valueLength);

			AddToReadModel(key, value);

			var rowSize = keyLengthLength + keyLength + valueLengthLength + valueLength;
			baseline = baseline[rowSize..];
			memory = memory[rowSize..];
		}
	}

<<<<<<< Updated upstream
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				TruncateLog(true);

				_baselineOwner?.Dispose();

				_disposed = true;
			}
		}

	#region Interface IDisposable

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
=======
	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !_disposed)
		{
			TruncateLog(true);

			_baselineOwner?.Dispose();

			_disposed = true;
>>>>>>> Stashed changes
		}
	}

	private void Write(in ReadOnlySpan<byte> key, in ReadOnlySpan<byte> value)
	{
		var keyLengthLength = Encode.GetEncodedLength(key.Length);
		var valueLengthLength = Encode.GetEncodedLength(value.Length);

		var memory = AllocateBuffer(key.Length + value.Length + keyLengthLength + valueLengthLength);

		var keyLenMemory = memory[..keyLengthLength];
		var keyMemory = memory.Slice(keyLengthLength, key.Length);
		var valueLenMemory = memory.Slice(keyLengthLength + key.Length, valueLengthLength);
		var valueMemory = memory.Slice(keyLengthLength + key.Length + valueLengthLength, value.Length);

		Encode.WriteEncodedValue(keyLenMemory.Span, key.Length);
		key.CopyTo(keyMemory.Span);
		Encode.WriteEncodedValue(valueLenMemory.Span, value.Length);
		value.CopyTo(valueMemory.Span);

		AddToReadModel(keyMemory, valueMemory);
	}

	private static SortedSet<Entry> CreateReadModel() => [];

	private void AddToReadModel(in Memory<byte> key, in Memory<byte> value)
	{
		if (_readModel is null)
		{
			return;
		}

		if (key.IsEmpty)
		{
			if (value.IsEmpty)
			{
<<<<<<< Updated upstream
				throw new InvalidOperationException(Resources.Exception_StorageNotAvailableForReadOperations);
			}

			var buffer = AllocateBuffer(key.Length, share: true);
			key.CopyTo(buffer.Span);

			return _readModel.TryGetValue(new Entry(buffer), out var result) ? result.Value : ReadOnlyMemory<byte>.Empty;
		}

	#endregion

		private void Write(in ReadOnlySpan<byte> key, in ReadOnlySpan<byte> value)
		{
			var keyLengthLength = Encode.GetEncodedLength(key.Length);
			var valueLengthLength = Encode.GetEncodedLength(value.Length);

			var memory = AllocateBuffer(key.Length + value.Length + keyLengthLength + valueLengthLength);

			var keyLenMemory = memory[..keyLengthLength];
			var keyMemory = memory.Slice(keyLengthLength, key.Length);
			var valueLenMemory = memory.Slice(keyLengthLength + key.Length, valueLengthLength);
			var valueMemory = memory.Slice(keyLengthLength + key.Length + valueLengthLength, value.Length);

			Encode.WriteEncodedValue(keyLenMemory.Span, key.Length);
			key.CopyTo(keyMemory.Span);
			Encode.WriteEncodedValue(valueLenMemory.Span, value.Length);
			value.CopyTo(valueMemory.Span);

			AddToReadModel(keyMemory, valueMemory);
		}

		private static SortedSet<Entry> CreateReadModel() => new();

		private void AddToReadModel(in Memory<byte> key, in Memory<byte> value)
		{
			if (_readModel is null)
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
					var toDelete = new List<ReadOnlyMemory<byte>>();
					foreach (var entry in _readModel.GetViewBetween(new Entry(value), upperValue: default))
					{
						if (entry.Key.Length < value.Length || !value.Span.SequenceEqual(entry.Key[..value.Length].Span))
						{
							break;
						}

						toDelete.Add(entry.Key);
					}

					foreach (var keyToDelete in toDelete)
					{
						_readModel.Remove(new Entry(keyToDelete));
					}
				}
			}
			else if (value.IsEmpty)
			{
				_readModel.Remove(new Entry(key));
=======
				_readModel.Clear();
>>>>>>> Stashed changes
			}
			else
			{
				var toDelete = new List<ReadOnlyMemory<byte>>();
				foreach (var entry in _readModel.GetViewBetween(new Entry(value), upperValue: default))
				{
					if (entry.Key.Length < value.Length || !value.Span.SequenceEqual(entry.Key[..value.Length].Span))
					{
						break;
					}

					toDelete.Add(entry.Key);
				}

				foreach (var keyToDelete in toDelete)
				{
					_readModel.Remove(new Entry(keyToDelete));
				}
			}
		}
<<<<<<< Updated upstream

		private Memory<byte> AllocateBuffer(int size, bool share = false)
=======
		else if (value.IsEmpty)
>>>>>>> Stashed changes
		{
			_readModel.Remove(new Entry(key));
		}
		else
		{
			_readModel.Remove(new Entry(key));
			_readModel.Add(new Entry(key, value));
		}
	}

	private Memory<byte> AllocateBuffer(int size, bool share = false)
	{
		if (_buffer.Length < size)
		{
			if (_owner is not null)
			{
				_buffers ??= [];

				_buffers.Add((_owner, _owner.Memory.Length - _buffer.Length));
			}

			_owner = MemoryPool<byte>.Shared.Rent();

<<<<<<< Updated upstream
			if (!share)
=======
			if (_owner.Memory.Length < size)
>>>>>>> Stashed changes
			{
				_owner.Dispose();
				_owner = MemoryPool<byte>.Shared.Rent(size);
			}

			_buffer = _owner.Memory;
		}

		var result = _buffer[..size];

		if (!share)
		{
			_buffer = _buffer[size..];
		}

		return result;
	}

	public int GetTransactionLogSize() => (_owner is not null ? _owner.Memory.Length - _buffer.Length : 0) + (_buffers?.Select(tuple => tuple.Size).Sum() ?? 0);

	public void WriteTransactionLogToSpan(Span<byte> span, bool truncateLog = true)
	{
		if (_buffers is not null)
		{
			foreach (var (owner, size) in _buffers)
			{
				owner.Memory[..size].Span.CopyTo(span);
				span = span[size..];
			}
		}

		if (_owner is not null)
		{
			var memory = _owner.Memory.Span;
			memory[..^_buffer.Length].CopyTo(span);
		}

		if (truncateLog)
		{
			TruncateLog(false);
		}
	}

	public int GetDataSize()
	{
		if (_readModel is null)
		{
			throw new InvalidOperationException(Resources.Exception_StorageNotAvailableForReadOperations);
		}

		return _readModel.Sum(entry => Encode.GetEncodedLength(entry.Key.Length) + entry.Key.Length + Encode.GetEncodedLength(entry.Value.Length) + entry.Value.Length);
	}

	public void WriteDataToSpan(Span<byte> span, bool shrink = true)
	{
		if (_readModel is null)
		{
			throw new InvalidOperationException(Resources.Exception_StorageNotAvailableForReadOperations);
		}

		IMemoryOwner<byte>? newBaselineOwner = default;
		var newBaseline = Memory<byte>.Empty;
		SortedSet<Entry>? newReadModel = default;
		if (shrink)
		{
			newBaselineOwner = MemoryPool<byte>.Shared.Rent(_readModel.Sum(entry => entry.Key.Length + entry.Value.Length));
			newBaseline = newBaselineOwner.Memory;
			newReadModel = CreateReadModel();
		}

		foreach (var pair in _readModel)
		{
			var keyLengthLength = Encode.GetEncodedLength(pair.Key.Length);
			var keyLenSpan = span[..keyLengthLength];
			var keySpan = span.Slice(keyLengthLength, pair.Key.Length);

			Encode.WriteEncodedValue(keyLenSpan, pair.Key.Length);
			pair.Key.Span.CopyTo(keySpan);

			var valueLengthLength = Encode.GetEncodedLength(pair.Value.Length);
			var valueLenSpan = span.Slice(keyLengthLength + pair.Key.Length, valueLengthLength);
			var valueSpan = span.Slice(keyLengthLength + pair.Key.Length + valueLengthLength, pair.Value.Length);

			Encode.WriteEncodedValue(valueLenSpan, pair.Value.Length);
			pair.Value.Span.CopyTo(valueSpan);

			span = span[(keyLengthLength + pair.Key.Length + valueLengthLength + pair.Value.Length)..];

			if (shrink)
			{
				var key = newBaseline[..pair.Key.Length];
				var value = newBaseline.Slice(pair.Key.Length, pair.Value.Length);
				pair.Key.CopyTo(key);
				pair.Value.CopyTo(value);
				newReadModel!.Add(new Entry(key, value));
				newBaseline = newBaseline[(pair.Key.Length + pair.Value.Length)..];
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
		if (_readModel is null || forceDispose)
		{
			if (_buffers is not null)
			{
				foreach (var (owner, _) in _buffers)
				{
					owner.Dispose();
				}
			}

			_owner?.Dispose();
			_owner = default;

			_buffers = default;
			_buffer = Memory<byte>.Empty;
		}
		else
		{
			if (_buffers is not null)
			{
				for (var i = 0; i < _buffers.Count; i ++)
				{
					_buffers[i] = (_buffers[i].Owner, 0);
				}
			}

			if (_owner is not null)
			{
				_buffers ??= [];

				_buffers.Add((_owner, 0));
			}

			_owner = default;
			_buffer = Memory<byte>.Empty;
		}
	}

	private readonly struct Entry(ReadOnlyMemory<byte> key, ReadOnlyMemory<byte> value = default) : IComparable<Entry>
	{
		public readonly ReadOnlyMemory<byte> Key   = key;
		public readonly ReadOnlyMemory<byte> Value = value;

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