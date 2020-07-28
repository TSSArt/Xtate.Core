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
using System.Buffers.Binary;
using System.Globalization;
using Xtate.Annotations;

namespace Xtate
{
	public enum DataModelDateTimeType
	{
		DateTime,
		DateTimeOffset
	}

	[PublicAPI]
	[Serializable]
	public readonly struct DataModelDateTime : IConvertible, IFormattable, IEquatable<DataModelDateTime>, IComparable<DataModelDateTime>, IComparable
	{
		private const ulong KindLocal = 0x8000000000000000;
		private const ulong KindUtc   = 0x4000000000000000;
		private const ulong TicksMask = 0x3FFFFFFFFFFFFFFF;
		private const int   KindShift = 62;

		private readonly ulong _data;
		private readonly short _offset;

		private DataModelDateTime(in ReadOnlySpan<byte> span)
		{
			_data = BinaryPrimitives.ReadUInt64LittleEndian(span);
			_offset = BinaryPrimitives.ReadInt16LittleEndian(span.Slice(8));
		}

		private DataModelDateTime(long utcTicks, TimeSpan offset, DateTimeKind kind)
		{
			_data = (ulong) utcTicks | ((ulong) kind << KindShift);
			_offset = (short) (offset.Ticks / TimeSpan.TicksPerMinute);
		}

		private long Ticks => (long) (_data & TicksMask);

		public DataModelDateTimeType Type => (_data & KindLocal) != 0 ? DataModelDateTimeType.DateTimeOffset : DataModelDateTimeType.DateTime;

	#region Interface IComparable

		public int CompareTo(object? value)
		{
			if (value == null) return 1;

			if (!(value is DataModelDateTime))
			{
				throw new ArgumentException(Resources.Exception_Argument_must_be_DataModelDateTime_type);
			}

			return Compare(this, (DataModelDateTime) value);
		}

	#endregion

	#region Interface IComparable<DataModelDateTime>

		public int CompareTo(DataModelDateTime value) => Compare(this, value);

	#endregion

	#region Interface IConvertible

		TypeCode IConvertible.GetTypeCode() =>
				Type switch
				{
						DataModelDateTimeType.DateTime => TypeCode.DateTime,
						DataModelDateTimeType.DateTimeOffset => TypeCode.Object,
						_ => Infrastructure.UnexpectedValue<TypeCode>()
				};

		bool IConvertible.ToBoolean(IFormatProvider provider) => ToDateTime().ToBoolean(provider);

		byte IConvertible.ToByte(IFormatProvider provider) => ToDateTime().ToByte(provider);

		char IConvertible.ToChar(IFormatProvider provider) => ToDateTime().ToChar(provider);

		DateTime IConvertible.ToDateTime(IFormatProvider provider) => ToDateTime().ToDateTime(provider);

		decimal IConvertible.ToDecimal(IFormatProvider provider) => ToDateTime().ToDecimal(provider);

		double IConvertible.ToDouble(IFormatProvider provider) => ToDateTime().ToDouble(provider);

		short IConvertible.ToInt16(IFormatProvider provider) => ToDateTime().ToInt16(provider);

		int IConvertible.ToInt32(IFormatProvider provider) => ToDateTime().ToInt32(provider);

		long IConvertible.ToInt64(IFormatProvider provider) => ToDateTime().ToInt64(provider);

		sbyte IConvertible.ToSByte(IFormatProvider provider) => ToDateTime().ToSByte(provider);

		float IConvertible.ToSingle(IFormatProvider provider) => ToDateTime().ToSingle(provider);

		ushort IConvertible.ToUInt16(IFormatProvider provider) => ToDateTime().ToUInt16(provider);

		uint IConvertible.ToUInt32(IFormatProvider provider) => ToDateTime().ToUInt32(provider);

		ulong IConvertible.ToUInt64(IFormatProvider provider) => ToDateTime().ToUInt64(provider);

		string IConvertible.ToString(IFormatProvider provider) => ToString(format: null, provider);

		object IConvertible.ToType(Type conversionType, IFormatProvider provider) => conversionType == typeof(DateTimeOffset) ? ToDateTimeOffset() : ToDateTime().ToType(conversionType, provider);

	#endregion

	#region Interface IEquatable<DataModelDateTime>

		public bool Equals(DataModelDateTime other) => Ticks == other.Ticks;

	#endregion

	#region Interface IFormattable

		public string ToString(string? format, IFormatProvider? formatProvider) =>
				Type switch
				{
						DataModelDateTimeType.DateTime => ToDateTime().ToString(format, formatProvider),
						DataModelDateTimeType.DateTimeOffset => ToDateTimeOffset().ToString(format, formatProvider),
						_ => Infrastructure.UnexpectedValue<string>()
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

		public void WriteTo(in Span<byte> span)
		{
			BinaryPrimitives.WriteUInt64LittleEndian(span, _data);
			BinaryPrimitives.WriteInt16LittleEndian(span.Slice(8), _offset);
		}

		public static DataModelDateTime ReadFrom(in ReadOnlySpan<byte> span) => new DataModelDateTime(span);

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

		public static DataModelDateTime FromDateTime(DateTime dateTime) => new DataModelDateTime(dateTime.Ticks, TimeSpan.Zero, dateTime.Kind);

		public static DataModelDateTime FromDateTimeOffset(DateTimeOffset dateTimeOffset) => new DataModelDateTime(dateTimeOffset.UtcTicks, dateTimeOffset.Offset, DateTimeKind.Local);

		public string ToString(string format) => ToString(format, formatProvider: null);

		public override string ToString() => ToString(format: null, formatProvider: null);

		public override bool Equals(object? obj) => obj is DataModelDateTime other && Ticks == other.Ticks;

		public override int GetHashCode() => Ticks.GetHashCode();

		public static bool operator ==(DataModelDateTime left, DataModelDateTime right) => left.Ticks == right.Ticks;

		public static bool operator !=(DataModelDateTime left, DataModelDateTime right) => left.Ticks != right.Ticks;

		public static bool operator <(DataModelDateTime left, DataModelDateTime right) => Compare(left, right) < 0;

		public static bool operator <=(DataModelDateTime left, DataModelDateTime right) => Compare(left, right) <= 0;

		public static bool operator >(DataModelDateTime left, DataModelDateTime right) => Compare(left, right) > 0;

		public static bool operator >=(DataModelDateTime left, DataModelDateTime right) => Compare(left, right) >= 0;

		public object ToObject() =>
				Type switch
				{
						DataModelDateTimeType.DateTime => ToDateTime(),
						DataModelDateTimeType.DateTimeOffset => ToDateTimeOffset(),
						_ => Infrastructure.UnexpectedValue<object>()
				};

		public static bool TryParse(string val, out DataModelDateTime dataModelDateTime) => TryParse(val, provider: null, DateTimeStyles.None, out dataModelDateTime);

		public static bool TryParse(string val, IFormatProvider? provider, DateTimeStyles style, out DataModelDateTime dataModelDateTime)
		{
			ParseData data = default;

			data.DateTimeParsed = DateTime.TryParse(val, provider, style | DateTimeStyles.RoundtripKind, out data.DateTime);
			data.DateTimeOffsetParsed = DateTimeOffset.TryParse(val, provider, style, out data.DateTimeOffset);

			return ProcessParseData(ref data, out dataModelDateTime);
		}

		public static bool TryParseExact(string val, string format, IFormatProvider? provider, DateTimeStyles style, out DataModelDateTime dataModelDateTime)
		{
			ParseData data = default;

			data.DateTimeParsed = DateTime.TryParseExact(val, format, provider, style | DateTimeStyles.RoundtripKind, out data.DateTime);
			data.DateTimeOffsetParsed = DateTimeOffset.TryParseExact(val, format, provider, style, out data.DateTimeOffset);

			return ProcessParseData(ref data, out dataModelDateTime);
		}

		public static bool TryParseExact(string val, string[] formats, IFormatProvider? provider, DateTimeStyles style, out DataModelDateTime dataModelDateTime)
		{
			ParseData data = default;

			data.DateTimeParsed = DateTime.TryParseExact(val, formats, provider, style | DateTimeStyles.RoundtripKind, out data.DateTime);
			data.DateTimeOffsetParsed = DateTimeOffset.TryParseExact(val, formats, provider, style, out data.DateTimeOffset);

			return ProcessParseData(ref data, out dataModelDateTime);
		}

		public static DataModelDateTime Parse(string val) => Parse(val, provider: null);

		public static DataModelDateTime Parse(string val, IFormatProvider? provider) => Parse(val, provider, DateTimeStyles.None);

		public static DataModelDateTime Parse(string val, IFormatProvider? provider, DateTimeStyles style)
		{
			var data = new ParseData
					   {
							   DateTimeOffset = DateTimeOffset.Parse(val, provider, style),
							   DateTimeOffsetParsed = true
					   };

			data.DateTimeParsed = DateTime.TryParse(val, provider, style | DateTimeStyles.RoundtripKind, out data.DateTime);

			ProcessParseData(ref data, out var result);

			return result;
		}

		public static DataModelDateTime ParseExact(string val, string format, IFormatProvider? provider) => ParseExact(val, format, provider, DateTimeStyles.None);

		public static DataModelDateTime ParseExact(string val, string format, IFormatProvider? provider, DateTimeStyles style)
		{
			var data = new ParseData
					   {
							   DateTimeOffset = DateTimeOffset.ParseExact(val, format, provider, style),
							   DateTimeOffsetParsed = true
					   };

			data.DateTimeParsed = DateTime.TryParseExact(val, format, provider, style | DateTimeStyles.RoundtripKind, out data.DateTime);

			ProcessParseData(ref data, out var result);

			return result;
		}

		public static DataModelDateTime ParseExact(string val, string[] formats, IFormatProvider? provider) => ParseExact(val, formats, provider, DateTimeStyles.None);

		public static DataModelDateTime ParseExact(string val, string[] formats, IFormatProvider? provider, DateTimeStyles style)
		{
			var data = new ParseData
					   {
							   DateTimeOffset = DateTimeOffset.ParseExact(val, formats, provider, style),
							   DateTimeOffsetParsed = true
					   };

			data.DateTimeParsed = DateTime.TryParseExact(val, formats, provider, style | DateTimeStyles.RoundtripKind, out data.DateTime);

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
			public DateTime       DateTime;
			public DateTimeOffset DateTimeOffset;
			public bool           DateTimeOffsetParsed;
			public bool           DateTimeParsed;
		}
	}
}