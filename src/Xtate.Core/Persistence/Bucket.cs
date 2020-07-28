#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using Xtate.Annotations;

namespace Xtate.Persistence
{
	[PublicAPI]
	[SuppressMessage(category: "ReSharper", checkId: "SuggestVarOrType_Elsewhere", Justification = "Span<> must be explicit")]
	internal readonly struct Bucket
	{
		public static readonly RootType RootKey = RootType.Instance;

		private readonly ulong _block;
		private readonly Node  _node;

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
			Span<byte> buf = stackalloc byte[KeyHelper<TKey>.Converter.GetLength(key)];
			KeyHelper<TKey>.Converter.Write(key, buf);
			CreateNewEntry(buf, out var storage);
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

		private int GetFullKeySize<TKey>(TKey key) where TKey : notnull
		{
			var size = KeyHelper<TKey>.Converter.GetLength(key) + GetSize(_block);

			for (var n = _node; n != null; n = n.Previous!)
			{
				size += n.Size;
			}

			return size;
		}

		private Span<byte> CreateFullKey<TKey>(Span<byte> buf, TKey key) where TKey : notnull
		{
			var len = KeyHelper<TKey>.Converter.GetLength(key);
			var nextBuf = WritePrevious(_node, len + 8, ref buf);

			var size = GetSize(_block);
			var length = buf.Length - nextBuf.Length + len + size;

			BinaryPrimitives.WriteUInt64LittleEndian(nextBuf, _block);
			KeyHelper<TKey>.Converter.Write(key, nextBuf.Slice(size, len));

			return buf.Slice(start: 0, length);
		}

		private static Span<byte> WritePrevious(Node? node, int size, ref Span<byte> buf)
		{
			if (node != null)
			{
				var nextBuf = WritePrevious(node.Previous, size + node.Size, ref buf);
				node.WriteTo(nextBuf);
				return nextBuf.Slice(node.Size);
			}

			if (buf.Length < size)
			{
				buf = new byte[size];
			}

			return buf;
		}

		public void Add<TKey>(TKey key, Span<byte> value) where TKey : notnull
		{
			if (value.Length == 0)
			{
				Remove(key);
				return;
			}

			Span<byte> buf = stackalloc byte[GetFullKeySize(key)];
			_node.Storage.Write(CreateFullKey(buf, key), value);
		}

		public void Add<TKey, TValue>(TKey key, TValue value) where TKey : notnull
		{
			if (value == null)
			{
				Remove(key);
				return;
			}

			Span<byte> buf = stackalloc byte[GetFullKeySize(key)];

			Span<byte> bufVal = stackalloc byte[ValueHelper<TValue>.Converter.GetLength(value)];
			ValueHelper<TValue>.Converter.Write(value, bufVal);
			_node.Storage.Write(CreateFullKey(buf, key), bufVal);
		}

		public void Remove<TKey>(TKey key) where TKey : notnull
		{
			Span<byte> buf = stackalloc byte[GetFullKeySize(key)];
			_node.Storage.Write(CreateFullKey(buf, key), ReadOnlySpan<byte>.Empty);
		}

		public void RemoveSubtree<TKey>(TKey key) where TKey : notnull
		{
			Span<byte> buf = stackalloc byte[GetFullKeySize(key)];
			_node.Storage.Write(ReadOnlySpan<byte>.Empty, CreateFullKey(buf, key));
		}

		public bool TryGet<TKey>(TKey key, out ReadOnlyMemory<byte> value) where TKey : notnull
		{
			Span<byte> buf = stackalloc byte[GetFullKeySize(key)];
			value = _node.Storage.Read(CreateFullKey(buf, key));
			return !value.IsEmpty;
		}

		public bool TryGet<TKey, TValue>(TKey key, [NotNullWhen(true)] [MaybeNullWhen(false)]
										 out TValue value) where TKey : notnull
		{
			Span<byte> buf = stackalloc byte[GetFullKeySize(key)];
			var memory = _node.Storage.Read(CreateFullKey(buf, key));

			if (memory.Length == 0)
			{
				value = default;
				return false;
			}

			value = ValueHelper<TValue>.Converter.Read(memory.Span);
			return value != null;
		}

		public class RootType
		{
			public static readonly RootType Instance = new RootType();

			private RootType() { }
		}

		private static class TypeConverter<TInput, TOutput>
		{
			public static readonly Func<TInput, TOutput> Convert = CreateConverter();

			private static Func<TInput, TOutput> CreateConverter()
			{
				var parameter = Expression.Parameter(typeof(TInput), typeof(TInput).Name);
				var method = Expression.Lambda<Func<TInput, TOutput>>(Expression.Convert(parameter, typeof(TOutput)), parameter);
				return method.Compile();
			}
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
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Byte:
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.SByte:
					case TypeCode.UInt16:
					case TypeCode.UInt32:
						return new EnumIntValueConverter<T>();

					case TypeCode.Double: return new DoubleValueConverter<T>();
					case TypeCode.Boolean: return new BooleanValueConverter<T>();
					case TypeCode.String: return new StringValueConverter<T>();
					case TypeCode.DateTime: return new DateTimeValueConverter<T>();
					case TypeCode.Object when type == typeof(Uri): return new UriValueConverter<T>();
					case TypeCode.Object when type == typeof(DateTimeOffset): return new DateTimeOffsetValueConverter<T>();
					case TypeCode.Object when type == typeof(DataModelDateTime): return new DataModelDateTimeValueConverter<T>();

					default: return new UnsupportedConverter<T>(@"value");
				}
			}
		}

		private class Node
		{
			public readonly Node?    Previous;
			public readonly IStorage Storage;

			public Node(IStorage storage)
			{
				Previous = null;
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
			private readonly ulong   _block1;
			private readonly ulong   _block2;
			private readonly byte[]? _bytes;

			public BlocksBytesNode(Node node, ulong block, Span<byte> span) : base(node)
			{
				_block1 = block;

				var length = span.Length;

				if (length > 8)
				{
					_bytes = new byte[length - 8];
					span.Slice(8).CopyTo(_bytes.AsSpan());
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

				_bytes?.AsSpan().CopyTo(buf.Slice(index));
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
			public sealed override int GetLength(TKey key) => GetLength(TypeConverter<TKey, TInternal>.Convert(key));

			public sealed override void Write(TKey key, Span<byte> bytes) => Write(TypeConverter<TKey, TInternal>.Convert(key), bytes);

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
				if (key == null) throw new ArgumentNullException(nameof(key));

				return Encoding.UTF8.GetByteCount(key) + 2;
			}

			protected override void Write(string key, Span<byte> bytes)
			{
				if (key == null) throw new ArgumentNullException(nameof(key));

				bytes[0] = 7;
				var lastByteIndex = bytes.Length - 1;
				bytes[lastByteIndex] = 0xFF;
				var dest = bytes.Slice(start: 1, bytes.Length - 2);
#if NETSTANDARD2_1
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

		private class UnsupportedConverter<T> : ConverterBase<T>
		{
			private readonly string _type;

			public UnsupportedConverter(string type) => _type = type;

			private NotSupportedException GetNotSupportedException() => new NotSupportedException(Res.Format(Resources.Exception_UnsupportedType, _type, typeof(T)));

			public override int GetLength(T key) => throw GetNotSupportedException();

			public override void Write(T key, Span<byte> bytes) => throw GetNotSupportedException();

			public override T Read(ReadOnlySpan<byte> bytes) => throw GetNotSupportedException();
		}

		private abstract class ValueConverterBase<TValue, TInternal> : ConverterBase<TValue>
		{
			public sealed override int GetLength(TValue val)
			{
				if (val == null) throw new ArgumentNullException(nameof(val));

				return GetLength(TypeConverter<TValue, TInternal>.Convert(val));
			}

			public sealed override TValue Read(ReadOnlySpan<byte> bytes) => TypeConverter<TInternal, TValue>.Convert(Get(bytes));

			public sealed override void Write(TValue val, Span<byte> bytes)
			{
				if (val == null) throw new ArgumentNullException(nameof(val));

				Write(TypeConverter<TValue, TInternal>.Convert(val), bytes);
			}

			protected abstract int GetLength(TInternal val);

			protected abstract TInternal Get(ReadOnlySpan<byte> bytes);

			protected abstract void Write(TInternal val, Span<byte> bytes);
		}

		private class EnumIntValueConverter<TValue> : ValueConverterBase<TValue, int>
		{
			protected override int GetLength(int val)
			{
				var uval = unchecked((uint) val);
				if (uval <= 0xFFU) return 1;
				if (uval <= 0xFFFFU) return 2;
				if (uval <= 0xFFFFFFU) return 3;
				return 4;
			}

			protected override void Write(int val, Span<byte> bytes)
			{
				var uval = unchecked((uint) val);

				for (var i = 0; i < bytes.Length; i ++)
				{
					bytes[i] = unchecked((byte) uval);
					uval >>= 8;
				}
			}

			protected override int Get(ReadOnlySpan<byte> bytes)
			{
				var uval = 0U;

				for (var i = bytes.Length - 1; i >= 0; i --)
				{
					uval = (uval << 8) | bytes[i];
				}

				return unchecked((int) uval);
			}
		}

		private class BooleanValueConverter<TValue> : ValueConverterBase<TValue, bool>
		{
			protected override int GetLength(bool val) => 1;

			protected override void Write(bool val, Span<byte> bytes) => bytes[0] = val ? (byte) 1 : (byte) 0;

			protected override bool Get(ReadOnlySpan<byte> bytes) => bytes[0] != 0;
		}

		private class DoubleValueConverter<TValue> : ValueConverterBase<TValue, double>
		{
			protected override int GetLength(double val) => 8;

			protected override void Write(double val, Span<byte> bytes) => BinaryPrimitives.WriteInt64LittleEndian(bytes, BitConverter.DoubleToInt64Bits(val));

			protected override double Get(ReadOnlySpan<byte> bytes) => BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(bytes));
		}

		private class DataModelDateTimeValueConverter<TValue> : ValueConverterBase<TValue, DataModelDateTime>
		{
			protected override int GetLength(DataModelDateTime val) => 10;

			protected override void Write(DataModelDateTime val, Span<byte> bytes) => val.WriteTo(bytes);

			protected override DataModelDateTime Get(ReadOnlySpan<byte> bytes) => DataModelDateTime.ReadFrom(bytes);
		}

		private class DateTimeValueConverter<TValue> : ValueConverterBase<TValue, DateTime>
		{
			protected override int GetLength(DateTime val) => 8;

			protected override void Write(DateTime val, Span<byte> bytes) => BinaryPrimitives.WriteInt64LittleEndian(bytes, val.ToBinary());

			protected override DateTime Get(ReadOnlySpan<byte> bytes) => DateTime.FromBinary(BinaryPrimitives.ReadInt64LittleEndian(bytes));
		}

		private class DateTimeOffsetValueConverter<TValue> : ValueConverterBase<TValue, DateTimeOffset>
		{
			protected override int GetLength(DateTimeOffset val) => 10;

			protected override void Write(DateTimeOffset val, Span<byte> bytes)
			{
				BinaryPrimitives.WriteInt64LittleEndian(bytes, val.Ticks);
				BinaryPrimitives.WriteInt16LittleEndian(bytes.Slice(8), (short) (val.Offset.Ticks / TimeSpan.TicksPerMinute));
			}

			protected override DateTimeOffset Get(ReadOnlySpan<byte> bytes)
			{
				var ticks = BinaryPrimitives.ReadInt64LittleEndian(bytes);
				var offsetMinutes = BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(8));
				return new DateTimeOffset(ticks, new TimeSpan(hours: 0, offsetMinutes, seconds: 0));
			}
		}

		private static class StringConverter
		{
			public static int GetLength(string val) => val.Length == 0 ? 1 : Encoding.UTF8.GetByteCount(val);

			public static void Write(string val, Span<byte> bytes)
			{
				if (val.Length == 0)
				{
					bytes[0] = 0xFF;
					return;
				}

#if NETSTANDARD2_1
				Encoding.UTF8.GetBytes(val, bytes);
#else
				Encoding.UTF8.GetBytes(val).CopyTo(bytes);
#endif
			}

			public static string Get(ReadOnlySpan<byte> bytes)
			{
				if (bytes[0] == 0xFF)
				{
					return string.Empty;
				}

#if NETSTANDARD2_1
				return Encoding.UTF8.GetString(bytes);
#else
				return Encoding.UTF8.GetString(bytes.ToArray());
#endif
			}
		}

		private class StringValueConverter<TString> : ValueConverterBase<TString, string>
		{
			protected override int GetLength(string val) => StringConverter.GetLength(val);

			protected override void Write(string val, Span<byte> bytes) => StringConverter.Write(val, bytes);

			protected override string Get(ReadOnlySpan<byte> bytes) => StringConverter.Get(bytes);
		}

		private class UriValueConverter<TString> : ValueConverterBase<TString, Uri>
		{
			protected override int GetLength(Uri val) => StringConverter.GetLength(val.ToString());

			protected override void Write(Uri val, Span<byte> bytes) => StringConverter.Write(val.ToString(), bytes);

			protected override Uri Get(ReadOnlySpan<byte> bytes) => new Uri(StringConverter.Get(bytes), UriKind.RelativeOrAbsolute);
		}
	}
}