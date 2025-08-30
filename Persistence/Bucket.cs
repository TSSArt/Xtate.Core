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

using System.Buffers.Binary;
using System.Text;

namespace Xtate.Persistence;

public readonly struct Bucket
{
	public static readonly RootType RootKey = RootType.Instance;

	private readonly ulong _block;

	private readonly Node _node;

	public Bucket(IStorage storage)
	{
		_node = new Node(storage);
		_block = 0;
	}

	private Bucket(ulong block, Node node)
	{
		_block = block;
		_node = node;
	}

	public Bucket Nested<TKey>(TKey key) where TKey : notnull
	{
		using var ss = new StackSpan<byte>(KeyHelper<TKey>.Converter.GetLength(key));
		var span = ss ? ss : stackalloc byte[ss];

		KeyHelper<TKey>.Converter.Write(key, span);
		CreateNewEntry(span, out var storage);

		return storage;
	}

	private void CreateNewEntry(Span<byte> bytes, out Bucket bucket)
	{
		var size = GetSize(_block);

		if (bytes.Length + size <= 8)
		{
			var block = _block;

			for (int i = 0, shift = size; i < bytes.Length; i ++, shift ++)
			{
				block |= (ulong) bytes[i] << (shift * 8);
			}

			bucket = new Bucket(block, _node);
		}
		else
		{
			bucket = new Bucket(block: 0, new BlocksBytesNode(_node, _block, bytes));
		}
	}

	private static int GetSize(ulong block)
	{
		if (block == 0L) return 0;
		if (block <= 0xFFL) return 1;
		if (block <= 0xFFFFL) return 2;
		if (block <= 0xFFFFFFL) return 3;
		if (block <= 0xFFFFFFFFL) return 4;
		if (block <= 0xFFFFFFFFFFL) return 5;
		if (block <= 0xFFFFFFFFFFFFL) return 6;
		if (block <= 0xFFFFFFFFFFFFFFL) return 7;

		return 8;
	}

	private int GetFullKeyLength<TKey>(TKey key) where TKey : notnull
	{
		var size = KeyHelper<TKey>.Converter.GetLength(key) + GetSize(_block);

		for (var n = _node; n is not null; n = n.Previous)
		{
			size += n.Size;
		}

		return size;
	}

	private ReadOnlySpan<byte> CreateFullKey<TKey>(Span<byte> buf, TKey key) where TKey : notnull
	{
		var len = KeyHelper<TKey>.Converter.GetLength(key);
		var nextBuf = WritePrevious(_node, len + 8, ref buf);

		var size = GetSize(_block);
		var length = buf.Length - nextBuf.Length + len + size;

		BinaryPrimitives.WriteUInt64LittleEndian(nextBuf, _block);
		KeyHelper<TKey>.Converter.Write(key, nextBuf.Slice(size, len));

		return buf[..length];
	}

	private static Span<byte> WritePrevious(Node? node, int size, ref Span<byte> buf)
	{
		if (node is not null)
		{
			var nextBuf = WritePrevious(node.Previous, size + node.Size, ref buf);
			node.WriteTo(nextBuf);

			return nextBuf[node.Size..];
		}

		if (buf.Length < size)
		{
			buf = new byte[size];
		}

		return buf;
	}

	public void Add<TKey>(TKey key, ReadOnlySpan<byte> value) where TKey : notnull
	{
		if (value.Length == 0)
		{
			Remove(key);

			return;
		}

		using var ss = new StackSpan<byte>(GetFullKeyLength(key));
		var span = ss ? ss : stackalloc byte[ss];

		_node.Storage.Set(CreateFullKey(span, key), value);
	}

	public void Add<TKey, TValue>(TKey key, TValue value) where TKey : notnull
	{
		if (value is null)
		{
			Remove(key);

			return;
		}

		using var ssKey = new StackSpan<byte>(GetFullKeyLength(key));
		var spanKey = ssKey ? ssKey : stackalloc byte[ssKey];

		using var ssVal = new StackSpan<byte>(ValueHelper<TValue>.Converter.GetLength(value));
		var spanVal = ssVal ? ssVal : stackalloc byte[ssVal];

		ValueHelper<TValue>.Converter.Write(value, spanVal);
		_node.Storage.Set(CreateFullKey(spanKey, key), spanVal);
	}

	public void Remove<TKey>(TKey key) where TKey : notnull
	{
		using var ss = new StackSpan<byte>(GetFullKeyLength(key));
		var span = ss ? ss : stackalloc byte[ss];

		_node.Storage.Remove(CreateFullKey(span, key));
	}

	public void RemoveSubtree<TKey>(TKey key) where TKey : notnull
	{
		using var ss = new StackSpan<byte>(GetFullKeyLength(key));
		var span = ss ? ss : stackalloc byte[ss];

		_node.Storage.RemoveAll(CreateFullKey(span, key));
	}

	public bool TryGet<TKey>(TKey key, out ReadOnlyMemory<byte> value) where TKey : notnull
	{
		using var ss = new StackSpan<byte>(GetFullKeyLength(key));
		var span = ss ? ss : stackalloc byte[ss];

		value = _node.Storage.Get(CreateFullKey(span, key));

		return !value.IsEmpty;
	}

	public bool TryGet<TKey, TValue>(TKey key,
									 [NotNullWhen(true)] [MaybeNullWhen(false)]
									 out TValue value) where TKey : notnull
	{
		using var ss = new StackSpan<byte>(GetFullKeyLength(key));
		var span = ss ? ss : stackalloc byte[ss];

		var memory = _node.Storage.Get(CreateFullKey(span, key));

		if (memory.Length == 0)
		{
			value = default;

			return false;
		}

		value = ValueHelper<TValue>.Converter.Read(memory.Span);

		return value is not null;
	}

	public class RootType
	{
		public static readonly RootType Instance = new();

		private RootType() { }
	}

	private static class KeyHelper<T> where T : notnull
	{
		public static readonly ConverterBase<T> Converter = GetKeyConverter();

		private static ConverterBase<T> GetKeyConverter()
		{
			var type = typeof(T);

			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Byte when type.IsEnum:
				case TypeCode.Int16 when type.IsEnum:
				case TypeCode.Int32 when type.IsEnum:
				case TypeCode.SByte when type.IsEnum:
				case TypeCode.UInt16 when type.IsEnum:
				case TypeCode.UInt32 when type.IsEnum:
					return new EnumKeyConverter<T>();

				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
					return new IndexKeyConverter<T>();

				case TypeCode.String:
					return new StringKeyConverter<T>();

				case TypeCode.Object when type == typeof(RootType):
					return new RootKeyConverter<T>();

				default: return new UnsupportedConverter<T>(@"key");
			}
		}
	}

	private static class ValueHelper<T>
	{
		public static readonly ConverterBase<T> Converter = GetValueConverter();

		private static ConverterBase<T> GetValueConverter()
		{
			var type = typeof(T);

			return Type.GetTypeCode(type) switch
				   {
					   TypeCode.Byte                                          => new EnumIntValueConverter<T>(),
					   TypeCode.Int16                                         => new EnumIntValueConverter<T>(),
					   TypeCode.Int32                                         => new EnumIntValueConverter<T>(),
					   TypeCode.SByte                                         => new EnumIntValueConverter<T>(),
					   TypeCode.UInt16                                        => new EnumIntValueConverter<T>(),
					   TypeCode.UInt32                                        => new EnumIntValueConverter<T>(),
					   TypeCode.Double                                        => new DoubleValueConverter<T>(),
					   TypeCode.Boolean                                       => new BooleanValueConverter<T>(),
					   TypeCode.String                                        => new StringValueConverter<T>(),
					   TypeCode.DateTime                                      => new DateTimeValueConverter<T>(),
					   TypeCode.Object when type == typeof(Uri)               => new UriValueConverter<T>(),
					   TypeCode.Object when type == typeof(FullUri)           => new FullUriValueConverter<T>(),
					   TypeCode.Object when type == typeof(DataModelNumber)   => new DataModelNumberValueConverter<T>(),
					   TypeCode.Object when type == typeof(DateTimeOffset)    => new DateTimeOffsetValueConverter<T>(),
					   TypeCode.Object when type == typeof(DataModelDateTime) => new DataModelDateTimeValueConverter<T>(),
					   _                                                      => new UnsupportedConverter<T>(@"value")
				   };
		}
	}

	private class Node
	{
		public readonly Node? Previous;

		public readonly IStorage Storage;

		public Node(IStorage storage)
		{
			Previous = default;
			Storage = storage;
		}

		protected Node(Node previous)
		{
			Previous = previous;
			Storage = previous.Storage;
		}

		public virtual int Size => 0;

		public virtual void WriteTo(Span<byte> buf) { }
	}

	private class BlocksBytesNode : Node
	{
		private readonly ulong _block1;

		private readonly ulong _block2;

		private readonly byte[]? _bytes;

		public BlocksBytesNode(Node node, ulong block, ReadOnlySpan<byte> span) : base(node)
		{
			_block1 = block;

			var length = span.Length;

			if (length > 8)
			{
				_bytes = new byte[length - 8];
				span[8..].CopyTo(_bytes.AsSpan());
				length = 8;
			}

			for (var i = length - 1; i >= 0; i --)
			{
				_block2 = (_block2 << 8) | (0xFFUL & span[i]);
			}
		}

		public override int Size => GetSize(_block1) + GetSize(_block2) + (_bytes?.Length ?? 0);

		private static void WriteBlock(ulong block, ref int index, Span<byte> buf)
		{
			var size = GetSize(block);

			for (var i = 0; i < size; i ++, index ++)
			{
				buf[index] = unchecked((byte) block);
				block >>= 8;
			}
		}

		public override void WriteTo(Span<byte> buf)
		{
			var index = 0;

			WriteBlock(_block1, ref index, buf);
			WriteBlock(_block2, ref index, buf);

			_bytes?.AsSpan().CopyTo(buf[index..]);
		}
	}

	private abstract class ConverterBase<T>
	{
		public abstract int GetLength(T key);

		public abstract void Write(T key, Span<byte> bytes);

		public abstract T Read(ReadOnlySpan<byte> bytes);
	}

	private abstract class KeyConverterBase<TKey, TInternal> : ConverterBase<TKey> where TKey : notnull
	{
		public sealed override int GetLength(TKey key) => GetLength(ConvertHelper<TKey, TInternal>.Convert(key));

		public sealed override void Write(TKey key, Span<byte> bytes) => Write(ConvertHelper<TKey, TInternal>.Convert(key), bytes);

		public sealed override TKey Read(ReadOnlySpan<byte> bytes) => throw new NotSupportedException();

		protected abstract int GetLength(TInternal key);

		protected abstract void Write(TInternal key, Span<byte> bytes);
	}

	private abstract class EnumIndexKeyConverter<TKey> : KeyConverterBase<TKey, int> where TKey : notnull
	{
		protected override int GetLength(int key) => GetEncodedLength(GetValue(key));

		protected abstract ulong GetValue(int key);

		protected override void Write(int key, Span<byte> bytes)
		{
			var value = GetEncodedValue(GetValue(key));

			for (var i = 0; i < bytes.Length; i ++)
			{
				bytes[i] = (byte) value;

				value >>= 8;
			}
		}

		private static int GetEncodedLength(ulong value)
		{
			if (value <= 0x7F) return 1;
			if (value <= 0x7FF) return 2;
			if (value <= 0xFFFF) return 3;
			if (value <= 0x1FFFFF) return 4;
			if (value <= 0x3FFFFFF) return 5;
			if (value <= 0x7FFFFFFF) return 6;
			if (value <= 0xFFFFFFFFF) return 7;

			throw new ArgumentOutOfRangeException(nameof(value));
		}

		private static ulong GetEncodedValue(ulong value)
		{
			if (value <= 0x7F)
			{
				return value;
			}

			if (value <= 0x7FF)
			{
				return 0x80C0U + (value >> 6) +
					   ((value & 0x3FU) << 8);
			}

			if (value <= 0xFFFF)
			{
				return 0x8080E0U + (value >> 12) +
					   ((value & 0xFC0U) << 2) +
					   ((value & 0x3FU) << 16);
			}

			if (value <= 0x1FFFFF)
			{
				return 0x808080F0U + (value >> 18) +
					   ((value & 0x3F000U) >> 4) +
					   ((value & 0xFC0U) << 10) +
					   ((value & 0x3FU) << 24);
			}

			if (value <= 0x3FFFFFF)
			{
				return 0x80808080F8UL + (value >> 24) +
					   ((value & 0xFC0000U) >> 10) +
					   ((value & 0x3F000U) << 4) +
					   ((value & 0xFC0U) << 18) +
					   ((value & 0x3FUL) << 32);
			}

			if (value <= 0x7FFFFFFF)
			{
				return 0x8080808080FCUL + (value >> 30) +
					   ((value & 0x3F000000) >> 16) +
					   ((value & 0xFC0000) >> 2) +
					   ((value & 0x3F000) << 12) +
					   ((value & 0xFC0UL) << 26) +
					   ((value & 0x3FUL) << 40);
			}

			if (value <= 0xFFFFFFFFF)
			{
				return 0x808080808080FEUL + (value >> 36) +
					   ((value & 0xFC0000000) >> 22) +
					   ((value & 0x3F000000) >> 8) +
					   ((value & 0xFC0000) << 6) +
					   ((value & 0x3F000) << 20) +
					   ((value & 0xFC0UL) << 34) +
					   ((value & 0x3FUL) << 48);
			}

			throw new ArgumentOutOfRangeException(nameof(value));
		}
	}

	private class EnumKeyConverter<TEnum> : EnumIndexKeyConverter<TEnum> where TEnum : notnull
	{
		protected override ulong GetValue(int key) => ((ulong) unchecked((uint) key) << 2) + 1;
	}

	private class IndexKeyConverter<TIndex> : EnumIndexKeyConverter<TIndex> where TIndex : notnull
	{
		protected override ulong GetValue(int index) => ((ulong) unchecked((uint) index) << 2) + 2;
	}

	private class StringKeyConverter<TString> : KeyConverterBase<TString, string> where TString : notnull
	{
		protected override int GetLength(string key)
		{
			if (key is null) throw new ArgumentNullException(nameof(key));

			return Encoding.UTF8.GetByteCount(key) + 2;
		}

		protected override void Write(string key, Span<byte> bytes)
		{
			if (key is null) throw new ArgumentNullException(nameof(key));

			bytes[0] = 7;
			var lastByteIndex = bytes.Length - 1;
			bytes[lastByteIndex] = 0xFF;
			var dest = bytes[1..^1];
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1
			Encoding.UTF8.GetBytes(key, dest);
#else
			Encoding.UTF8.GetBytes(key).CopyTo(dest);
#endif
		}
	}

	private class RootKeyConverter<T> : KeyConverterBase<T, RootType> where T : notnull
	{
		protected override int GetLength(RootType key) => 0;

		protected override void Write(RootType key, Span<byte> bytes) { }
	}

	private class UnsupportedConverter<T>(string type) : ConverterBase<T>
	{
		private NotSupportedException GetNotSupportedException() => new(Res.Format(Resources.Exception_UnsupportedType, type, typeof(T)));

		public override int GetLength(T key) => throw GetNotSupportedException();

		public override void Write(T key, Span<byte> bytes) => throw GetNotSupportedException();

		public override T Read(ReadOnlySpan<byte> bytes) => throw GetNotSupportedException();
	}

	private abstract class ValueConverterBase<TValue, TInternal> : ConverterBase<TValue>
	{
		public sealed override int GetLength(TValue value)
		{
			if (value is null) throw new ArgumentNullException(nameof(value));

			return GetLength(ConvertHelper<TValue, TInternal>.Convert(value));
		}

		public sealed override TValue Read(ReadOnlySpan<byte> bytes) => ConvertHelper<TInternal, TValue>.Convert(Get(bytes));

		public sealed override void Write(TValue value, Span<byte> bytes)
		{
			if (value is null) throw new ArgumentNullException(nameof(value));

			Write(ConvertHelper<TValue, TInternal>.Convert(value), bytes);
		}

		protected abstract int GetLength(TInternal value);

		protected abstract TInternal Get(ReadOnlySpan<byte> bytes);

		protected abstract void Write(TInternal value, Span<byte> bytes);
	}

	private class EnumIntValueConverter<TValue> : ValueConverterBase<TValue, int>
	{
		protected override int GetLength(int value)
		{
			var count = 1;

			while (value is < sbyte.MinValue or > sbyte.MaxValue)
			{
				value >>= 8;
				count ++;
			}

			return count;
		}

		protected override void Write(int value, Span<byte> bytes)
		{
			for (var i = 0; i < bytes.Length; i ++)
			{
				bytes[i] = unchecked((byte) value);
				value >>= 8;
			}
		}

		protected override int Get(ReadOnlySpan<byte> bytes)
		{
			var value = (int) bytes[^1];

			for (var i = bytes.Length - 2; i >= 0; i --)
			{
				value = (value << 8) | bytes[i];
			}

			return value;
		}
	}

	private class BooleanValueConverter<TValue> : ValueConverterBase<TValue, bool>
	{
		protected override int GetLength(bool value) => 1;

		protected override void Write(bool value, Span<byte> bytes) => bytes[0] = value ? (byte) 1 : (byte) 0;

		protected override bool Get(ReadOnlySpan<byte> bytes) => bytes[0] != 0;
	}

	private class DoubleValueConverter<TValue> : ValueConverterBase<TValue, double>
	{
		protected override int GetLength(double value) => 8;

		protected override void Write(double value, Span<byte> bytes) => BinaryPrimitives.WriteInt64LittleEndian(bytes, BitConverter.DoubleToInt64Bits(value));

		protected override double Get(ReadOnlySpan<byte> bytes) => BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(bytes));
	}

	private class DataModelNumberValueConverter<TValue> : ValueConverterBase<TValue, DataModelNumber>
	{
		protected override int GetLength(DataModelNumber value) => value.WriteToSize();

		protected override void Write(DataModelNumber value, Span<byte> bytes) => value.WriteTo(bytes);

		protected override DataModelNumber Get(ReadOnlySpan<byte> bytes) => DataModelNumber.ReadFrom(bytes);
	}

	private class DataModelDateTimeValueConverter<TValue> : ValueConverterBase<TValue, DataModelDateTime>
	{
		protected override int GetLength(DataModelDateTime value) => DataModelDateTime.WriteToSize();

		protected override void Write(DataModelDateTime value, Span<byte> bytes) => value.WriteTo(bytes);

		protected override DataModelDateTime Get(ReadOnlySpan<byte> bytes) => DataModelDateTime.ReadFrom(bytes);
	}

	private class DateTimeValueConverter<TValue> : ValueConverterBase<TValue, DateTime>
	{
		protected override int GetLength(DateTime value) => 8;

		protected override void Write(DateTime value, Span<byte> bytes) => BinaryPrimitives.WriteInt64LittleEndian(bytes, value.ToBinary());

		protected override DateTime Get(ReadOnlySpan<byte> bytes) => DateTime.FromBinary(BinaryPrimitives.ReadInt64LittleEndian(bytes));
	}

	private class DateTimeOffsetValueConverter<TValue> : ValueConverterBase<TValue, DateTimeOffset>
	{
		protected override int GetLength(DateTimeOffset value) => 10;

		protected override void Write(DateTimeOffset value, Span<byte> bytes)
		{
			BinaryPrimitives.WriteInt64LittleEndian(bytes, value.Ticks);
			BinaryPrimitives.WriteInt16LittleEndian(bytes[8..], (short) (value.Offset.Ticks / TimeSpan.TicksPerMinute));
		}

		protected override DateTimeOffset Get(ReadOnlySpan<byte> bytes)
		{
			var ticks = BinaryPrimitives.ReadInt64LittleEndian(bytes);
			var offsetMinutes = BinaryPrimitives.ReadInt16LittleEndian(bytes[8..]);

			return new DateTimeOffset(ticks, new TimeSpan(hours: 0, offsetMinutes, seconds: 0));
		}
	}

	private static class StringConverter
	{
		public static int GetLength(string value) => value.Length == 0 ? 1 : Encoding.UTF8.GetByteCount(value);

		public static void Write(string value, Span<byte> bytes)
		{
			if (value.Length == 0)
			{
				bytes[0] = 0xFF;

				return;
			}

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1
			Encoding.UTF8.GetBytes(value, bytes);
#else
			Encoding.UTF8.GetBytes(value).CopyTo(bytes);
#endif
		}

		public static string Get(ReadOnlySpan<byte> bytes)
		{
			if (bytes[0] == 0xFF)
			{
				return string.Empty;
			}

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1
			return Encoding.UTF8.GetString(bytes);
#else
			return Encoding.UTF8.GetString(bytes.ToArray());
#endif
		}
	}

	private class StringValueConverter<TString> : ValueConverterBase<TString, string>
	{
		protected override int GetLength(string value) => StringConverter.GetLength(value);

		protected override void Write(string value, Span<byte> bytes) => StringConverter.Write(value, bytes);

		protected override string Get(ReadOnlySpan<byte> bytes) => StringConverter.Get(bytes);
	}

	private class UriValueConverter<TString> : ValueConverterBase<TString, Uri>
	{
		protected override int GetLength(Uri value) => StringConverter.GetLength(value.ToString());

		protected override void Write(Uri value, Span<byte> bytes) => StringConverter.Write(value.ToString(), bytes);

		protected override Uri Get(ReadOnlySpan<byte> bytes) => new(StringConverter.Get(bytes), UriKind.RelativeOrAbsolute);
	}

	private class FullUriValueConverter<TString> : ValueConverterBase<TString, FullUri>
	{
		protected override int GetLength(FullUri value) => StringConverter.GetLength(value.ToString());

		protected override void Write(FullUri value, Span<byte> bytes) => StringConverter.Write(value.ToString(), bytes);

		protected override FullUri Get(ReadOnlySpan<byte> bytes) => new(StringConverter.Get(bytes));
	}
}