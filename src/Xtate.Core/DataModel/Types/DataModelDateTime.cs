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
using System.Globalization;

namespace Xtate;

public enum DataModelDateTimeType
{
	DateTime,

	DateTimeOffset
}

[Serializable]
public readonly struct DataModelDateTime : IConvertible, ISpanFormattable, IEquatable<DataModelDateTime>, IComparable<DataModelDateTime>, IComparable
{
	private const ulong KindLocal = 0x8000000000000000;

	private const ulong KindUtc = 0x4000000000000000;

	private const ulong TicksMask = 0x3FFFFFFFFFFFFFFF;

	private const int KindShift = 62;

	private readonly ulong _data;

	private readonly short _offset;

	private DataModelDateTime(in ReadOnlySpan<byte> span)
	{
		_data = BinaryPrimitives.ReadUInt64LittleEndian(span);
		_offset = BinaryPrimitives.ReadInt16LittleEndian(span[8..]);
	}

	private DataModelDateTime(long utcTicks, TimeSpan offset, DateTimeKind kind)
	{
		_data = (ulong) utcTicks | ((ulong) kind << KindShift);
		_offset = (short) (offset.Ticks / TimeSpan.TicksPerMinute);
	}

	private long Ticks => (long) (_data & TicksMask);

	public DataModelDateTimeType Type => (_data & KindLocal) != 0 ? DataModelDateTimeType.DateTimeOffset : DataModelDateTimeType.DateTime;

#region Interface IComparable

	public int CompareTo(object? value) =>
		value switch
		{
			null                       => 1,
			DataModelDateTime dateTime => Compare(this, dateTime),
			_                          => throw new ArgumentException(Resources.Exception_ArgumentMustBeDataModelDateTimeType)
		};

#endregion

#region Interface IComparable<DataModelDateTime>

	public int CompareTo(DataModelDateTime value) => Compare(this, value);

#endregion

#region Interface IConvertible

	TypeCode IConvertible.GetTypeCode() =>
		Type switch
		{
			DataModelDateTimeType.DateTime       => TypeCode.DateTime,
			DataModelDateTimeType.DateTimeOffset => TypeCode.Object,
			_                                    => throw Infra.Unmatched(Type)
		};

	bool IConvertible.ToBoolean(IFormatProvider? provider) => ToDateTime().ToBoolean(provider);

	byte IConvertible.ToByte(IFormatProvider? provider) => ToDateTime().ToByte(provider);

	char IConvertible.ToChar(IFormatProvider? provider) => ToDateTime().ToChar(provider);

	DateTime IConvertible.ToDateTime(IFormatProvider? provider) => ToDateTime().ToDateTime(provider);

	decimal IConvertible.ToDecimal(IFormatProvider? provider) => ToDateTime().ToDecimal(provider);

	double IConvertible.ToDouble(IFormatProvider? provider) => ToDateTime().ToDouble(provider);

	short IConvertible.ToInt16(IFormatProvider? provider) => ToDateTime().ToInt16(provider);

	int IConvertible.ToInt32(IFormatProvider? provider) => ToDateTime().ToInt32(provider);

	long IConvertible.ToInt64(IFormatProvider? provider) => ToDateTime().ToInt64(provider);

	sbyte IConvertible.ToSByte(IFormatProvider? provider) => ToDateTime().ToSByte(provider);

	float IConvertible.ToSingle(IFormatProvider? provider) => ToDateTime().ToSingle(provider);

	ushort IConvertible.ToUInt16(IFormatProvider? provider) => ToDateTime().ToUInt16(provider);

	uint IConvertible.ToUInt32(IFormatProvider? provider) => ToDateTime().ToUInt32(provider);

	ulong IConvertible.ToUInt64(IFormatProvider? provider) => ToDateTime().ToUInt64(provider);

	string IConvertible.ToString(IFormatProvider? provider) => ToString(format: null, provider);

	object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => conversionType == typeof(DateTimeOffset) ? ToDateTimeOffset() : ToDateTime().ToType(conversionType, provider);

#endregion

#region Interface IEquatable<DataModelDateTime>

	public bool Equals(DataModelDateTime other) => Ticks == other.Ticks;

#endregion

#region Interface IFormattable

	public string ToString(string? format, IFormatProvider? formatProvider) =>
		Type switch
		{
			DataModelDateTimeType.DateTime       => ToDateTime().ToString(format, formatProvider),
			DataModelDateTimeType.DateTimeOffset => ToDateTimeOffset().ToString(format, formatProvider),
			_                                    => throw Infra.Unmatched(Type)
		};

#endregion

#region Interface ISpanFormattable

	public bool TryFormat(Span<char> destination,
						  out int charsWritten,
						  ReadOnlySpan<char> format,
						  IFormatProvider? formatProvider) =>
		Type switch
		{
			DataModelDateTimeType.DateTime       => ToDateTime().TryFormat(destination, out charsWritten, format, formatProvider),
			DataModelDateTimeType.DateTimeOffset => ToDateTimeOffset().TryFormat(destination, out charsWritten, format, formatProvider),
			_                                    => throw Infra.Unmatched(Type)
		};

#endregion

	public DateTimeOffset ToDateTimeOffset()
	{
		var offsetTicks = _offset * TimeSpan.TicksPerMinute;

		return new DateTimeOffset(Ticks + offsetTicks, new TimeSpan(offsetTicks));
	}

	public DateTime ToDateTime()
	{
		var ticks = Ticks + _offset * TimeSpan.TicksPerMinute;

		if ((_data & KindUtc) != 0)
		{
			return new DateTime(ticks, DateTimeKind.Utc);
		}

		if ((_data & KindLocal) != 0)
		{
			return new DateTime(ticks, DateTimeKind.Local);
		}

		return new DateTime(ticks);
	}

	public static int WriteToSize() => 10;

	public void WriteTo(Span<byte> span)
	{
		BinaryPrimitives.WriteUInt64LittleEndian(span, _data);
		BinaryPrimitives.WriteInt16LittleEndian(span[8..], _offset);
	}

	public static DataModelDateTime ReadFrom(ReadOnlySpan<byte> span) => new(span);

	private static int Compare(in DataModelDateTime t1, in DataModelDateTime t2)
	{
		var ticks1 = t1.Ticks;
		var ticks2 = t2.Ticks;

		if (ticks1 > ticks2)
		{
			return 1;
		}

		if (ticks1 < ticks2)
		{
			return -1;
		}

		return 0;
	}

	public static explicit operator DateTime(DataModelDateTime dataModelDateTime) => dataModelDateTime.ToDateTime();

	public static explicit operator DateTimeOffset(DataModelDateTime dataModelDateTime) => dataModelDateTime.ToDateTimeOffset();

	public static implicit operator DataModelDateTime(DateTime dateTime) => FromDateTime(dateTime);

	public static implicit operator DataModelDateTime(DateTimeOffset dateTimeOffset) => FromDateTimeOffset(dateTimeOffset);

	public static DataModelDateTime FromDateTime(DateTime dateTime) => new(dateTime.Ticks, TimeSpan.Zero, dateTime.Kind);

	public static DataModelDateTime FromDateTimeOffset(DateTimeOffset dateTimeOffset) => new(dateTimeOffset.UtcTicks, dateTimeOffset.Offset, DateTimeKind.Local);

	public string ToString(string format) => ToString(format, formatProvider: null);

	public override string ToString() => ToString(format: null, formatProvider: null);

	public override bool Equals(object? obj) => obj is DataModelDateTime other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(Ticks);

	public static bool operator ==(DataModelDateTime left, DataModelDateTime right) => left.Equals(right);

	public static bool operator !=(DataModelDateTime left, DataModelDateTime right) => !(left == right);

	public static bool operator <(DataModelDateTime left, DataModelDateTime right) => Compare(left, right) < 0;

	public static bool operator <=(DataModelDateTime left, DataModelDateTime right) => Compare(left, right) <= 0;

	public static bool operator >(DataModelDateTime left, DataModelDateTime right) => Compare(left, right) > 0;

	public static bool operator >=(DataModelDateTime left, DataModelDateTime right) => Compare(left, right) >= 0;

	public object ToObject() =>
		Type switch
		{
			DataModelDateTimeType.DateTime       => (object) ToDateTime(),
			DataModelDateTimeType.DateTimeOffset => ToDateTimeOffset(),
			_                                    => throw Infra.Unmatched(Type)
		};

	public static bool TryParse(string value, out DataModelDateTime dataModelDateTime) => TryParse(value, provider: null, DateTimeStyles.None, out dataModelDateTime);

	public static bool TryParse(string value,
								IFormatProvider? provider,
								DateTimeStyles style,
								out DataModelDateTime dataModelDateTime)
	{
		ParseData data = default;

		data.DateTimeParsed = DateTime.TryParse(value, provider, style | DateTimeStyles.RoundtripKind, out data.DateTime);
		data.DateTimeOffsetParsed = DateTimeOffset.TryParse(value, provider, style, out data.DateTimeOffset);

		return ProcessParseData(ref data, out dataModelDateTime);
	}

	public static bool TryParseExact(string value,
									 string format,
									 IFormatProvider? provider,
									 DateTimeStyles style,
									 out DataModelDateTime dataModelDateTime)
	{
		ParseData data = default;

		data.DateTimeParsed = DateTime.TryParseExact(value, format, provider, style | DateTimeStyles.RoundtripKind, out data.DateTime);
		data.DateTimeOffsetParsed = DateTimeOffset.TryParseExact(value, format, provider, style, out data.DateTimeOffset);

		return ProcessParseData(ref data, out dataModelDateTime);
	}

	public static bool TryParseExact(string value,
									 string[] formats,
									 IFormatProvider? provider,
									 DateTimeStyles style,
									 out DataModelDateTime dataModelDateTime)
	{
		ParseData data = default;

		data.DateTimeParsed = DateTime.TryParseExact(value, formats, provider, style | DateTimeStyles.RoundtripKind, out data.DateTime);
		data.DateTimeOffsetParsed = DateTimeOffset.TryParseExact(value, formats, provider, style, out data.DateTimeOffset);

		return ProcessParseData(ref data, out dataModelDateTime);
	}

	public static DataModelDateTime Parse(string value) => Parse(value, provider: null);

	public static DataModelDateTime Parse(string value, IFormatProvider? provider) => Parse(value, provider, DateTimeStyles.None);

	public static DataModelDateTime Parse(string value, IFormatProvider? provider, DateTimeStyles style)
	{
		var data = new ParseData
				   {
					   DateTimeOffset = DateTimeOffset.Parse(value, provider, style),
					   DateTimeOffsetParsed = true
				   };

		data.DateTimeParsed = DateTime.TryParse(value, provider, style | DateTimeStyles.RoundtripKind, out data.DateTime);

		ProcessParseData(ref data, out var result);

		return result;
	}

	public static DataModelDateTime ParseExact(string value, string format, IFormatProvider? provider) => ParseExact(value, format, provider, DateTimeStyles.None);

	public static DataModelDateTime ParseExact(string value,
											   string format,
											   IFormatProvider? provider,
											   DateTimeStyles style)
	{
		var data = new ParseData
				   {
					   DateTimeOffset = DateTimeOffset.ParseExact(value, format, provider, style),
					   DateTimeOffsetParsed = true
				   };

		data.DateTimeParsed = DateTime.TryParseExact(value, format, provider, style | DateTimeStyles.RoundtripKind, out data.DateTime);

		ProcessParseData(ref data, out var result);

		return result;
	}

	public static DataModelDateTime ParseExact(string value, string[] formats, IFormatProvider? provider) => ParseExact(value, formats, provider, DateTimeStyles.None);

	public static DataModelDateTime ParseExact(string value,
											   string[] formats,
											   IFormatProvider? provider,
											   DateTimeStyles style)
	{
		var data = new ParseData
				   {
					   DateTimeOffset = DateTimeOffset.ParseExact(value, formats, provider, style),
					   DateTimeOffsetParsed = true
				   };

		data.DateTimeParsed = DateTime.TryParseExact(value, formats, provider, style | DateTimeStyles.RoundtripKind, out data.DateTime);

		ProcessParseData(ref data, out var result);

		return result;
	}

	private static bool ProcessParseData(ref ParseData data, out DataModelDateTime dataModelDateTime)
	{
		if (!data.DateTimeParsed && !data.DateTimeOffsetParsed)
		{
			dataModelDateTime = default;

			return false;
		}

		if (data.DateTimeParsed && data.DateTimeOffsetParsed)
		{
			dataModelDateTime = data.DateTime.Kind == DateTimeKind.Local || data.DateTimeOffset.Offset != TimeSpan.Zero ? (DataModelDateTime) data.DateTimeOffset : data.DateTime;

			return true;
		}

		dataModelDateTime = data.DateTimeOffsetParsed ? (DataModelDateTime) data.DateTimeOffset : data.DateTime;

		return true;
	}

	private ref struct ParseData
	{
		public DateTime DateTime;

		public DateTimeOffset DateTimeOffset;

		public bool DateTimeOffsetParsed;

		public bool DateTimeParsed;
	}
}