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

namespace Xtate;

public enum DataModelNumberType
{
	Int32,

	Int64,

	Double,

	Decimal
}

public readonly struct DataModelNumber : IConvertible, ISpanFormattable, IEquatable<DataModelNumber>, IComparable<DataModelNumber>, IComparable
{
	private static readonly double[] PowersOf10 = Enumerable.Range(start: 0, count: 29).Select(i => Math.Pow(x: 10, i)).ToArray();

	private readonly long _int64;

	private readonly long _int64Ext;

	private DataModelNumber(DataModelNumberType type, long int64)
	{
		Type = type;
		_int64 = int64;
	}

	private DataModelNumber(DataModelNumberType type, long int64, long int64Ext)
	{
		Type = type;
		_int64 = int64;
		_int64Ext = int64Ext;
	}

	public DataModelNumberType Type { get; }

#region Interface IComparable

	public int CompareTo(object? value) =>
		value switch
		{
			null                   => 1,
			DataModelNumber number => CompareTo(number),
			_                      => throw new ArgumentException(Resources.Exception_ArgumentMustBeDataModelNumberType)
		};

#endregion

#region Interface IComparable<DataModelNumber>

	public int CompareTo(DataModelNumber value) => NumberCompare(this, value);

#endregion

#region Interface IConvertible

	TypeCode IConvertible.GetTypeCode() =>
		Type switch
		{
			DataModelNumberType.Int32   => TypeCode.Int32,
			DataModelNumberType.Int64   => TypeCode.Int64,
			DataModelNumberType.Double  => TypeCode.Double,
			DataModelNumberType.Decimal => TypeCode.Decimal,
			_                           => throw Infra.Unmatched(Type)
		};

	bool IConvertible.ToBoolean(IFormatProvider? provider) =>
		Type switch
		{
			DataModelNumberType.Int32 or DataModelNumberType.Int64 => ToInt64().ToBoolean(provider),
			DataModelNumberType.Double                             => ToDouble().ToBoolean(provider),
			DataModelNumberType.Decimal                            => ToDecimal().ToBoolean(provider),
			_                                                      => throw Infra.Unmatched(Type)
		};

	char IConvertible.ToChar(IFormatProvider? provider) => ToInt64().ToChar(provider);

	sbyte IConvertible.ToSByte(IFormatProvider? provider) => ToInt64().ToSByte(provider);

	byte IConvertible.ToByte(IFormatProvider? provider) => ToInt64().ToByte(provider);

	short IConvertible.ToInt16(IFormatProvider? provider) => ToInt64().ToInt16(provider);

	ushort IConvertible.ToUInt16(IFormatProvider? provider) => ToInt64().ToUInt16(provider);

	int IConvertible.ToInt32(IFormatProvider? provider) => ToInt64().ToInt32(provider);

	uint IConvertible.ToUInt32(IFormatProvider? provider) => ToInt64().ToUInt32(provider);

	long IConvertible.ToInt64(IFormatProvider? provider) => ToInt64();

	ulong IConvertible.ToUInt64(IFormatProvider? provider) =>
		Type switch
		{
			DataModelNumberType.Int32 or DataModelNumberType.Int64 => ToInt64().ToUInt64(provider),
			DataModelNumberType.Double                             => ToDouble().ToUInt64(provider),
			DataModelNumberType.Decimal                            => ToDecimal().ToUInt64(provider),
			_                                                      => throw Infra.Unmatched(Type)
		};

	float IConvertible.ToSingle(IFormatProvider? provider) => ToDouble().ToSingle(provider);

	double IConvertible.ToDouble(IFormatProvider? provider) => ToDouble();

	decimal IConvertible.ToDecimal(IFormatProvider? provider) => ToDecimal();

	DateTime IConvertible.ToDateTime(IFormatProvider? provider) =>
		Type switch
		{
			DataModelNumberType.Int32 or DataModelNumberType.Int64 => ToInt64().ToDateTime(provider),
			DataModelNumberType.Double                             => ToDouble().ToDateTime(provider),
			DataModelNumberType.Decimal                            => ToDecimal().ToDateTime(provider),
			_                                                      => throw Infra.Unmatched(Type)
		};

	string IConvertible.ToString(IFormatProvider? provider) => ToString(format: null, provider);

	object IConvertible.ToType(Type conversionType, IFormatProvider? provider) =>
		Type switch
		{
			DataModelNumberType.Int32 or DataModelNumberType.Int64 => ToInt64().ToType(conversionType, provider),
			DataModelNumberType.Double                             => ToDouble().ToType(conversionType, provider),
			DataModelNumberType.Decimal                            => ToDecimal().ToType(conversionType, provider),
			_                                                      => throw Infra.Unmatched(Type)
		};

#endregion

#region Interface IEquatable<DataModelNumber>

	public bool Equals(DataModelNumber other) =>
		Type switch
		{
			DataModelNumberType.Int32 or DataModelNumberType.Int64
				when other.Type is DataModelNumberType.Int32 or DataModelNumberType.Int64 => _int64.Equals(other._int64),
			DataModelNumberType.Double when other.Type is DataModelNumberType.Double   => ConvertToDouble(_int64).Equals(ConvertToDouble(other._int64)),
			DataModelNumberType.Decimal when other.Type is DataModelNumberType.Decimal => ConvertToDecimal(_int64, _int64Ext).Equals(ConvertToDecimal(other._int64, other._int64Ext)),
			_                                                                          => CompareTo(other) == 0
		};

#endregion

#region Interface IFormattable

	public string ToString(string? format, IFormatProvider? formatProvider) =>
		Type switch
		{
			DataModelNumberType.Int32
				or DataModelNumberType.Int64 => ToInt64().ToString(format, formatProvider),
			DataModelNumberType.Double  => ToDouble().ToString(format, formatProvider),
			DataModelNumberType.Decimal => ToDecimal().ToString(format, formatProvider),
			_                           => throw Infra.Unmatched(Type)
		};

#endregion

#region Interface ISpanFormattable

	public bool TryFormat(Span<char> destination,
						  out int charsWritten,
						  ReadOnlySpan<char> format,
						  IFormatProvider? formatProvider) =>
		Type switch
		{
			DataModelNumberType.Int32
				or DataModelNumberType.Int64 => ToInt64().TryFormat(destination, out charsWritten, format, formatProvider),
			DataModelNumberType.Double  => ToDouble().TryFormat(destination, out charsWritten, format, formatProvider),
			DataModelNumberType.Decimal => ToDecimal().TryFormat(destination, out charsWritten, format, formatProvider),
			_                           => throw Infra.Unmatched(Type)
		};

#endregion

	public static DataModelNumber FromInt32(int value) => new(DataModelNumberType.Int32, value);

	public static DataModelNumber FromInt64(long value) => new(DataModelNumberType.Int64, value);

	public static DataModelNumber FromDouble(double value) => new(DataModelNumberType.Double, ConvertToInt64(value));

	public static DataModelNumber FromDecimal(decimal value) => new(DataModelNumberType.Decimal, ConvertToInt64Ext(value, out var int64Ext), int64Ext);

	public static explicit operator int(DataModelNumber dataModelNumber) => dataModelNumber.ToInt32();

	public static explicit operator long(DataModelNumber dataModelNumber) => dataModelNumber.ToInt64();

	public static explicit operator double(DataModelNumber dataModelNumber) => dataModelNumber.ToDouble();

	public static explicit operator decimal(DataModelNumber dataModelNumber) => dataModelNumber.ToDecimal();

	public static implicit operator DataModelNumber(int val) => FromInt32(val);

	public static implicit operator DataModelNumber(long val) => FromInt64(val);

	public static implicit operator DataModelNumber(double val) => FromDouble(val);

	public static implicit operator DataModelNumber(decimal val) => FromDecimal(val);

	public int ToInt32() =>
		Type switch
		{
			DataModelNumberType.Int32 or DataModelNumberType.Int64 => Convert.ToInt32(_int64),
			DataModelNumberType.Double                             => Convert.ToInt32(ConvertToDouble(_int64)),
			DataModelNumberType.Decimal                            => Convert.ToInt32(ConvertToDecimal(_int64, _int64Ext)),
			_                                                      => throw Infra.Unmatched(Type)
		};

	public long ToInt64() =>
		Type switch
		{
			DataModelNumberType.Int32 or DataModelNumberType.Int64 => _int64,
			DataModelNumberType.Double                             => Convert.ToInt64(ConvertToDouble(_int64)),
			DataModelNumberType.Decimal                            => Convert.ToInt64(ConvertToDecimal(_int64, _int64Ext)),
			_                                                      => throw Infra.Unmatched(Type)
		};

	public double ToDouble() =>
		Type switch
		{
			DataModelNumberType.Double                             => ConvertToDouble(_int64),
			DataModelNumberType.Int32 or DataModelNumberType.Int64 => Convert.ToDouble(_int64),
			DataModelNumberType.Decimal                            => Convert.ToDouble(ConvertToDecimal(_int64, _int64Ext)),
			_                                                      => throw Infra.Unmatched(Type)
		};

	public decimal ToDecimal() =>
		Type switch
		{
			DataModelNumberType.Decimal                            => ConvertToDecimal(_int64, _int64Ext),
			DataModelNumberType.Int32 or DataModelNumberType.Int64 => Convert.ToDecimal(_int64),
			DataModelNumberType.Double                             => Convert.ToDecimal(ConvertToDouble(_int64)),
			_                                                      => throw Infra.Unmatched(Type)
		};

	private static double ConvertToDouble(long int64) => BitConverter.Int64BitsToDouble(int64);

	private static long ConvertToInt64(double value) => BitConverter.DoubleToInt64Bits(value);

	private static decimal ConvertToDecimal(long int64, long int64Ext) => (decimal) new RawDecimal(int64, int64Ext);

	private static long ConvertToInt64Ext(decimal value, out long int64Ext)
	{
		var rawDecimal = (RawDecimal) value;

		int64Ext = rawDecimal.Hi64;

		return rawDecimal.Lo64;
	}

	public int WriteToSize() =>
		Type switch
		{
			DataModelNumberType.Int32 or DataModelNumberType.Int64 => 1 + GetLengthInt64(_int64),
			DataModelNumberType.Double                             => 1 + 8,
			DataModelNumberType.Decimal                            => 1 + 16,
			_                                                      => throw Infra.Unmatched(Type)
		};

	private static int GetLengthInt64(long value)
	{
		var count = 1;

		while (value is < sbyte.MinValue or > sbyte.MaxValue)
		{
			value >>= 8;
			count ++;
		}

		return count;
	}

	public void WriteTo(Span<byte> span)
	{
		span[0] = (byte) Type;
		span = span[1..];

		switch (Type)
		{
			case DataModelNumberType.Int32:
				WriteInt64(_int64, span);

				break;

			case DataModelNumberType.Int64:
				WriteInt64(_int64, span);

				break;

			case DataModelNumberType.Double:
				BinaryPrimitives.WriteInt64LittleEndian(span, _int64);

				break;

			case DataModelNumberType.Decimal:
				BinaryPrimitives.WriteInt64LittleEndian(span, _int64);
				BinaryPrimitives.WriteInt64LittleEndian(span[8..], _int64Ext);

				break;

			default:
				throw Infra.Unmatched(Type);
		}
	}

	private static void WriteInt64(long value, Span<byte> bytes)
	{
		for (var i = 0; i < bytes.Length; i ++)
		{
			bytes[i] = unchecked((byte) value);
			value >>= 8;
		}
	}

	public static DataModelNumber ReadFrom(ReadOnlySpan<byte> span)
	{
		var type = (DataModelNumberType) span[0];
		span = span[1..];

		return type switch
			   {
				   DataModelNumberType.Int32   => new DataModelNumber(DataModelNumberType.Int32, GetInt64(span)),
				   DataModelNumberType.Int64   => new DataModelNumber(DataModelNumberType.Int64, GetInt64(span)),
				   DataModelNumberType.Double  => new DataModelNumber(DataModelNumberType.Double, BinaryPrimitives.ReadInt64LittleEndian(span)),
				   DataModelNumberType.Decimal => new DataModelNumber(DataModelNumberType.Decimal, BinaryPrimitives.ReadInt64LittleEndian(span), BinaryPrimitives.ReadInt64LittleEndian(span[8..])),
				   _                           => throw Infra.Unmatched(type)
			   };
	}

	private static long GetInt64(ReadOnlySpan<byte> bytes)
	{
		var value = (long) bytes[^1];

		for (var i = bytes.Length - 2; i >= 0; i --)
		{
			value = (value << 8) | bytes[i];
		}

		return value;
	}

	private static int NumberCompare(in DataModelNumber n1, in DataModelNumber n2)
	{
		if (CanCastToInt64(n1) && CanCastToInt64(n2))
		{
			return n1.ToInt64().CompareTo(n2.ToInt64());
		}

		if (CanCastToDouble(n1) && CanCastToDouble(n2))
		{
			return n1.ToDouble().CompareTo(n2.ToDouble());
		}

		if (CanCastToDecimal(n1) && CanCastToDecimal(n2))
		{
			return n1.ToDecimal().CompareTo(n2.ToDecimal());
		}

		if (CanCastToInt64(n1) && CanCastToDouble(n2))
		{
			return CompareLongDouble(n1.ToInt64(), n2.ToDouble());
		}

		if (CanCastToDouble(n1) && CanCastToInt64(n2))
		{
			return -CompareLongDouble(n2.ToInt64(), n1.ToDouble());
		}

		if (CanCastToDecimal(n1) && CanCastToDouble(n2))
		{
			return CompareDecimalDouble(n1.ToDecimal(), n2.ToDouble());
		}

		if (CanCastToDouble(n1) && CanCastToDecimal(n2))
		{
			return -CompareDecimalDouble(n2.ToDecimal(), n1.ToDouble());
		}

		throw new InvalidOperationException();

		int CompareLongDouble(long longVal, double doubleVal)
		{
			switch (doubleVal)
			{
				case double.NaN or > 9223372036854775807d: return -1;
				case < -9223372036854775808d:              return 1;
			}

			var truncated = Math.Truncate(doubleVal);
			var fraction = truncated.CompareTo(doubleVal);
			var result = longVal.CompareTo((long) truncated);

			return fraction == 0 ? result : result == 0 ? fraction : result;
		}

		int CompareDecimalDouble(decimal decimalVal, double doubleVal)
		{
			switch (doubleVal)
			{
				case double.NaN or > 79228162514264337593543950335d: return -1;
				case < -79228162514264337593543950335d:              return 1;
			}

			RawDecimal.ResetScale(ref decimalVal, out var scale);
			doubleVal *= PowersOf10[scale];

			var truncated = Math.Truncate(doubleVal);
			var fraction = truncated.CompareTo(doubleVal);
			var result = decimalVal.CompareTo((decimal) truncated);

			return fraction == 0 ? result : result == 0 ? fraction : result;
		}

		bool CanCastToInt64(in DataModelNumber n) => n.Type is DataModelNumberType.Int32 or DataModelNumberType.Int64;

		bool CanCastToDouble(in DataModelNumber n) => n.Type is DataModelNumberType.Int32 or DataModelNumberType.Double;

		bool CanCastToDecimal(in DataModelNumber n) => n.Type is DataModelNumberType.Int32 or DataModelNumberType.Int64 or DataModelNumberType.Decimal;
	}

	public string ToString(string format) => ToString(format, formatProvider: null);

	public override string ToString() => ToString(format: null, formatProvider: null);

	public override bool Equals(object? obj) => obj is DataModelNumber other && Equals(other);

	public override int GetHashCode() =>
		Type switch
		{
			DataModelNumberType.Int32   => HashCode.Combine((int) _int64),
			DataModelNumberType.Int64   => HashCode.Combine(_int64),
			DataModelNumberType.Double  => HashCode.Combine(ConvertToDouble(_int64)),
			DataModelNumberType.Decimal => HashCode.Combine(ConvertToDecimal(_int64, _int64Ext)),
			_                           => throw Infra.Unmatched(Type)
		};

	public static bool operator ==(DataModelNumber left, DataModelNumber right) => left.Equals(right);

	public static bool operator !=(DataModelNumber left, DataModelNumber right) => !(left == right);

	public static bool operator <(DataModelNumber left, DataModelNumber right) => left.CompareTo(right) < 0;

	public static bool operator <=(DataModelNumber left, DataModelNumber right) => left.CompareTo(right) <= 0;

	public static bool operator >(DataModelNumber left, DataModelNumber right) => left.CompareTo(right) > 0;

	public static bool operator >=(DataModelNumber left, DataModelNumber right) => left.CompareTo(right) >= 0;

	public object ToObject() =>
		Type switch
		{
			DataModelNumberType.Int32   => ToInt32(),
			DataModelNumberType.Int64   => ToInt64(),
			DataModelNumberType.Double  => ToDouble(),
			DataModelNumberType.Decimal => ToDecimal(),
			_                           => throw Infra.Unmatched(Type)
		};

	public bool IsNaN() => Type == DataModelNumberType.Double && double.IsNaN(ConvertToDouble(_int64));
}