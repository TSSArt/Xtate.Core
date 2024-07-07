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

using System.Globalization;

namespace Xtate.Core.Test;

[TestClass]
public class DataModelDateTimeTest
{
	private const long DefaultTicks = 100 * TimeSpan.TicksPerDay;

	private static readonly TimeSpan PositiveOffset = new(hours: 1, minutes: 0, seconds: 0);
	private static readonly TimeSpan NegativeOffset = new(hours: -1, minutes: 0, seconds: 0);

	private static readonly DataModelDateTime DateTimeWithUnspecifiedKind      = new DateTime(DefaultTicks, DateTimeKind.Unspecified);
	private static readonly DataModelDateTime DateTimeWithUtcKind              = new DateTime(DefaultTicks, DateTimeKind.Utc);
	private static readonly DataModelDateTime DateTimeWithLocalKind            = new DateTime(DefaultTicks, DateTimeKind.Local);
	private static readonly DataModelDateTime DateTimeOffsetWithZeroOffset     = new DateTimeOffset(DefaultTicks, TimeSpan.Zero);
	private static readonly DataModelDateTime DateTimeOffsetWithPositiveOffset = new DateTimeOffset(DefaultTicks, PositiveOffset);
	private static readonly DataModelDateTime DateTimeOffsetWithNegativeOffset = new DateTimeOffset(DefaultTicks, NegativeOffset);
	private static readonly DataModelDateTime DateTimePoint1Utc                = new DateTime(DefaultTicks, DateTimeKind.Utc);
	private static readonly DataModelDateTime DateTimePoint1WithOffset         = new DateTimeOffset(new DateTime(DefaultTicks + PositiveOffset.Ticks), PositiveOffset);
	private static readonly DataModelDateTime DateTimePoint2WithOffset         = new DateTimeOffset(new DateTime(DefaultTicks), PositiveOffset);

	[TestMethod]
	public void Type_ShouldBeDateTime_IfItRepresentsDateTimeWithUnspecifiedKind()
	{
		// assert
		Assert.AreEqual(DataModelDateTimeType.DateTime, DateTimeWithUnspecifiedKind.Type);
	}

	[TestMethod]
	public void Type_ShouldBeDateTime_IfItRepresentsDateTimeWithUtcKind()
	{
		// assert
		Assert.AreEqual(DataModelDateTimeType.DateTime, DateTimeWithUtcKind.Type);
	}

	[TestMethod]
	public void Type_ShouldBeDateTime_IfItRepresentsDateTimeWithLocalKind()
	{
		// assert
		Assert.AreEqual(DataModelDateTimeType.DateTimeOffset, DateTimeWithLocalKind.Type);
	}

	[TestMethod]
	public void Type_ShouldBeDateTime_IfItRepresentsDateTimeOffsetWithZeroOffset()
	{
		// assert
		Assert.AreEqual(DataModelDateTimeType.DateTimeOffset, DateTimeOffsetWithZeroOffset.Type);
	}

	[TestMethod]
	public void Type_ShouldBeDateTime_IfItRepresentsDateTimeOffsetWithPositiveOffset()
	{
		// assert
		Assert.AreEqual(DataModelDateTimeType.DateTimeOffset, DateTimeOffsetWithPositiveOffset.Type);
	}

	[TestMethod]
	public void Type_ShouldBeDateTime_IfItRepresentsDateTimeOffsetWithNegativeOffset()
	{
		// assert
		Assert.AreEqual(DataModelDateTimeType.DateTimeOffset, DateTimeOffsetWithNegativeOffset.Type);
	}

	private static IEnumerable<object[]> ToObjectData()
	{
		yield return [typeof(DateTime), DateTimeWithUnspecifiedKind];
		yield return [typeof(DateTime), DateTimeWithUtcKind];
		yield return [typeof(DateTimeOffset), DateTimeWithLocalKind];
		yield return [typeof(DateTimeOffset), DateTimeOffsetWithZeroOffset];
		yield return [typeof(DateTimeOffset), DateTimeOffsetWithPositiveOffset];
		yield return [typeof(DateTimeOffset), DateTimeOffsetWithNegativeOffset];
	}

	[TestMethod]
	[DynamicData(nameof(ToObjectData), DynamicDataSourceType.Method)]
	public void ToObject_ShouldReturnArg1Type_IfItRepresentArg2(Type type, DataModelDateTime dataModelDateTime)
	{
		// act
		var o = dataModelDateTime.ToObject();

		// assert
		Assert.AreEqual(type, o.GetType());
	}

	private static IEnumerable<object[]> DateTimeRoundtripValidationSet()
	{
		yield return [new DateTime(DefaultTicks, DateTimeKind.Utc)];
		yield return [new DateTime(DefaultTicks, DateTimeKind.Local)];
		yield return [new DateTime(DefaultTicks, DateTimeKind.Unspecified)];
	}

	[TestMethod]
	[DynamicData(nameof(DateTimeRoundtripValidationSet), DynamicDataSourceType.Method)]
	public void DateTime_ShouldRoundtripValue(DateTime dateTime)
	{
		// act
		var roundTripDateTime = DataModelDateTime.FromDateTime(dateTime).ToDateTime();

		// assert
		Assert.AreEqual(dateTime.Ticks, roundTripDateTime.Ticks);
		Assert.AreEqual(dateTime.Kind, roundTripDateTime.Kind);
	}

	private static IEnumerable<object[]> DateTimeOffsetRoundtripValidationSet()
	{
		yield return [new DateTimeOffset(DefaultTicks, TimeSpan.Zero)];
		yield return [new DateTimeOffset(DefaultTicks, PositiveOffset)];
		yield return [new DateTimeOffset(DefaultTicks, NegativeOffset)];
	}

	[TestMethod]
	[DynamicData(nameof(DateTimeOffsetRoundtripValidationSet), DynamicDataSourceType.Method)]
	public void DateTimeOffset_ShouldRoundtripValue(DateTimeOffset dateTimeOffset)
	{
		// act
		var roundtripDateTimeOffset = DataModelDateTime.FromDateTimeOffset(dateTimeOffset).ToDateTimeOffset();

		// assert
		Assert.AreEqual(dateTimeOffset.UtcTicks, roundtripDateTimeOffset.UtcTicks);
		Assert.AreEqual(dateTimeOffset.Offset, roundtripDateTimeOffset.Offset);
	}

	[TestMethod]
	[DataRow(1, "2000-01-01T00:00:00.0000001")]
	[DataRow(10000, "2000-01-01T00:00:00.0010000")]
	[DataRow(-1, "1999-12-31T23:59:59.9999999")]
	public void ToString_ShouldNotLoosePrecision(int addTicks, string expected)
	{
		// arrange
		var v = new DateTime(year: 2000, month: 1, day: 1, hour: 0, minute: 0, second: 0, millisecond: 0, DateTimeKind.Unspecified) + TimeSpan.FromTicks(addTicks);

		// act
		var str = DataModelDateTime.FromDateTime(v).ToString(@"o");

		// assert
		Assert.AreEqual(expected, str);
	}

	[TestMethod]
	[DataRow(1, "2000-01-01T00:00:00.0000001")]
	[DataRow(10000, "2000-01-01T00:00:00.0010000")]
	[DataRow(-1, "1999-12-31T23:59:59.9999999")]
	public void TryParse_ShouldNotLoosePrecision_IfNoOffset(int addTicks, string forParse)
	{
		// arrange
		var v = new DateTime(year: 2000, month: 1, day: 1, hour: 0, minute: 0, second: 0, millisecond: 0, DateTimeKind.Unspecified) + TimeSpan.FromTicks(addTicks);

		// act
		_ = DataModelDateTime.TryParse(forParse, out var d);

		// assert
		Assert.AreEqual(v, d.ToDateTime());
	}

	[TestMethod]
	[DataRow(1, "2000-01-01T00:00:00.0000001+01:00")]
	[DataRow(10000, "2000-01-01T00:00:00.0010000+01:00")]
	[DataRow(-1, "1999-12-31T23:59:59.9999999+01:00")]
	public void TryParse_ShouldNotLoosePrecision_IfOffsetPResent(int addTicks, string forParse)
	{
		// arrange
		var v = new DateTime(year: 2000, month: 1, day: 1, hour: 0, minute: 0, second: 0, millisecond: 0, DateTimeKind.Unspecified) + TimeSpan.FromTicks(addTicks);

		// act
		_ = DataModelDateTime.TryParse(forParse, out var d);

		// assert
		Assert.AreEqual(v, d.ToDateTime());
	}

	[TestMethod]
	public void TryParse_ShouldReturnFalse_IfStringCannotBeParsed()
	{
		// assert
		Assert.IsFalse(DataModelDateTime.TryParse(value: "some", out _));
	}

	[TestMethod]
	[DataRow("2000-01-01T00:00:00.0000001")]
	[DataRow("2000-01-01T00:00:00.0000001Z")]
	[DataRow("2000-01-01T00:00:00.0000001+01:01")]
	[DataRow("2000-01-01T00:00:00.0000001-02:02")]
	[DataRow("0001-01-01T00:00:00.0000000Z")]
	[DataRow("0001-01-01T00:00:00.0000000-00:00")]
	[DataRow("0001-01-01T00:00:00.0000000+00:00")]
	[DataRow("0001-01-01T00:00:00.0000000-01:00")]
	[DataRow("9999-12-31T23:59:59.9999999Z")]
	[DataRow("9999-12-31T23:59:59.9999999-00:00")]
	[DataRow("9999-12-31T23:59:59.9999999+00:00")]
	[DataRow("9999-12-31T23:59:59.9999999+01:00")]
	[DataRow("9999-12-31T23:59:59")]
	[DataRow("9999-12-31 23:59:59")]
	public void TryParse_ShouldReturnTrue_IfStringCanBeParsed(string value)
	{
		// assert
		Assert.IsTrue(DataModelDateTime.TryParse(value, out _));
	}

	[TestMethod]
	[DataRow("2000-01-01T00:00:00.0000001")]
	[DataRow("2000-01-01T00:00:00.0000001Z")]
	[DataRow("2000-01-01T00:00:00.0000001+01:01")]
	[DataRow("2000-01-01T00:00:00.0000001-02:02")]
	[DataRow("0001-01-01T00:00:00.0000000Z")]
	[DataRow("0001-01-01T00:00:00.0000000-00:00")]
	[DataRow("0001-01-01T00:00:00.0000000+00:00")]
	[DataRow("0001-01-01T00:00:00.0000000-01:00")]
	[DataRow("9999-12-31T23:59:59.9999999Z")]
	[DataRow("9999-12-31T23:59:59.9999999-00:00")]
	[DataRow("9999-12-31T23:59:59.9999999+00:00")]
	[DataRow("9999-12-31T23:59:59.9999999+01:00")]
	public void TryParseExact_ShouldReturnTrue_IfStringCanBeParsed(string value)
	{
		// assert
		Assert.IsTrue(DataModelDateTime.TryParseExact(value, format: "o", provider: null, DateTimeStyles.None, out _));
	}

	[TestMethod]
	[DataRow("2000-01-01T00:00:00.0000001")]
	[DataRow("2000-01-01T00:00:00.0000001Z")]
	[DataRow("2000-01-01T00:00:00.0000001+01:01")]
	[DataRow("2000-01-01T00:00:00.0000001-02:02")]
	[DataRow("0001-01-01T00:00:00.0000000Z")]
	[DataRow("0001-01-01T00:00:00.0000000-00:00")]
	[DataRow("0001-01-01T00:00:00.0000000+00:00")]
	[DataRow("0001-01-01T00:00:00.0000000-01:00")]
	[DataRow("9999-12-31T23:59:59.9999999Z")]
	[DataRow("9999-12-31T23:59:59.9999999-00:00")]
	[DataRow("9999-12-31T23:59:59.9999999+00:00")]
	[DataRow("9999-12-31T23:59:59.9999999+01:00")]
	[DataRow("9999-12-31T23:59:59")]
	public void TryParseExact2_ShouldReturnTrue_IfStringCanBeParsed(string value)
	{
		// assert
		Assert.IsTrue(DataModelDateTime.TryParseExact(value, ["o", "s"], provider: null, DateTimeStyles.None, out _));
	}

	[TestMethod]
	[DataRow("2000-01-01T00:00:00.0000001")]
	[DataRow("2000-01-01T00:00:00.0000001Z")]
	[DataRow("2000-01-01T00:00:00.0000001+01:01")]
	[DataRow("2000-01-01T00:00:00.0000001-02:02")]
	[DataRow("0001-01-01T00:00:00.0000000Z")]
	[DataRow("0001-01-01T00:00:00.0000000-00:00")]
	[DataRow("0001-01-01T00:00:00.0000000+00:00")]
	[DataRow("0001-01-01T00:00:00.0000000-01:00")]
	[DataRow("9999-12-31T23:59:59.9999999Z")]
	[DataRow("9999-12-31T23:59:59.9999999-00:00")]
	[DataRow("9999-12-31T23:59:59.9999999+00:00")]
	[DataRow("9999-12-31T23:59:59.9999999+01:00")]
	[DataRow("9999-12-31T23:59:59")]
	[DataRow("9999-12-31 23:59:59")]
	public void Parse_ShouldReturnSameValueAsTryParse_IfStringCanBeParsed(string value)
	{
		// act
		DataModelDateTime.TryParse(value, provider: null, DateTimeStyles.None, out var dttm);

		var parsedDttm = DataModelDateTime.Parse(value);

		// assert
		Assert.AreEqual(dttm, parsedDttm);
	}

	[TestMethod]
	[DataRow("2000-01-01T00:00:00.0000001")]
	[DataRow("2000-01-01T00:00:00.0000001Z")]
	[DataRow("2000-01-01T00:00:00.0000001+01:01")]
	[DataRow("2000-01-01T00:00:00.0000001-02:02")]
	[DataRow("0001-01-01T00:00:00.0000000Z")]
	[DataRow("0001-01-01T00:00:00.0000000-00:00")]
	[DataRow("0001-01-01T00:00:00.0000000+00:00")]
	[DataRow("0001-01-01T00:00:00.0000000-01:00")]
	[DataRow("9999-12-31T23:59:59.9999999Z")]
	[DataRow("9999-12-31T23:59:59.9999999-00:00")]
	[DataRow("9999-12-31T23:59:59.9999999+00:00")]
	[DataRow("9999-12-31T23:59:59.9999999+01:00")]
	public void ParseExact_ShouldReturnSameValueAsTryParse_IfStringCanBeParsed(string value)
	{
		// act
		DataModelDateTime.TryParseExact(value, format: "o", provider: null, DateTimeStyles.None, out var dttm);

		var parsedDttm = DataModelDateTime.ParseExact(value, format: "o", provider: null);

		// assert
		Assert.AreEqual(dttm, parsedDttm);
	}

	[TestMethod]
	[DataRow("2000-01-01T00:00:00.0000001")]
	[DataRow("2000-01-01T00:00:00.0000001Z")]
	[DataRow("2000-01-01T00:00:00.0000001+01:01")]
	[DataRow("2000-01-01T00:00:00.0000001-02:02")]
	[DataRow("0001-01-01T00:00:00.0000000Z")]
	[DataRow("0001-01-01T00:00:00.0000000-00:00")]
	[DataRow("0001-01-01T00:00:00.0000000+00:00")]
	[DataRow("0001-01-01T00:00:00.0000000-01:00")]
	[DataRow("9999-12-31T23:59:59.9999999Z")]
	[DataRow("9999-12-31T23:59:59.9999999-00:00")]
	[DataRow("9999-12-31T23:59:59.9999999+00:00")]
	[DataRow("9999-12-31T23:59:59.9999999+01:00")]
	[DataRow("9999-12-31T23:59:59")]
	public void ParseExact2_ShouldReturnSameValueAsTryParse_IfStringCanBeParsed(string value)
	{
		// act
		DataModelDateTime.TryParseExact(value, ["o", "s"], provider: null, DateTimeStyles.None, out var dttm);

		var parsedDttm = DataModelDateTime.ParseExact(value, ["o", "s"], provider: null);

		// assert
		Assert.AreEqual(dttm, parsedDttm);
	}

	[TestMethod]
	public void Equals_ShouldReturnTrue_IfUtcMatched()
	{
		// arrange
		var d1 = DateTimePoint1Utc;
		var d2 = DateTimePoint1WithOffset;

		// assert
		Assert.IsTrue(d1.Equals(d2));
		Assert.IsTrue(d2.Equals(d1));
		Assert.IsTrue(d1.Equals((object) d2));
		Assert.IsTrue(d2.Equals((object) d1));
		Assert.IsTrue(Equals(d1, d2));
		Assert.IsTrue(Equals(d2, d1));
		Assert.IsTrue(d1 == d2);
		Assert.IsTrue(d2 == d1);
		Assert.IsTrue(!(d1 != d2));
		Assert.IsTrue(!(d2 != d1));
	}

	[TestMethod]
	public void Equals_ShouldReturnFalse_IfUtcNotMatched()
	{
		// arrange
		var d1 = DateTimePoint1Utc;
		var d2 = DateTimePoint2WithOffset;

		// assert
		Assert.IsFalse(d1.Equals(d2));
		Assert.IsFalse(d2.Equals(d1));
		Assert.IsFalse(d1.Equals((object) d2));
		Assert.IsFalse(d2.Equals((object) d1));
		Assert.IsFalse(Equals(d1, d2));
		Assert.IsFalse(Equals(d2, d1));
		Assert.IsFalse(d1 == d2);
		Assert.IsFalse(d2 == d1);
		Assert.IsFalse(!(d1 != d2));
		Assert.IsFalse(!(d2 != d1));
	}

	[TestMethod]
	public void GetHashCode_ShouldHaveSameValue_ForSameUtc()
	{
		// arrange
		var d1 = DateTimePoint1Utc;
		var d2 = DateTimePoint1WithOffset;

		// assert
		Assert.IsTrue(d1.GetHashCode() == d2.GetHashCode());
	}

	[TestMethod]
	public void ExplicitCast_ShouldReturnSameValueAsProperty()
	{
		// arrange
		var d1 = DateTimePoint1Utc;
		var d2 = DateTimePoint1WithOffset;

		// assert
		Assert.AreEqual(d1.ToDateTime(), (DateTime) d1);
		Assert.AreEqual(d2.ToDateTime(), (DateTime) d2);
		Assert.AreEqual(d1.ToDateTimeOffset(), (DateTimeOffset) d1);
		Assert.AreEqual(d2.ToDateTimeOffset(), (DateTimeOffset) d2);
	}

	[TestMethod]
	public void IConvert_ShouldThrowInvalidCastException_ForAllExceptToDateTime()
	{
		// assert
		Assert.ThrowsException<InvalidCastException>(() => ((IConvertible) default(DataModelDateTime)).ToBoolean(null));
		Assert.ThrowsException<InvalidCastException>(() => ((IConvertible) default(DataModelDateTime)).ToByte(null));
		Assert.ThrowsException<InvalidCastException>(() => ((IConvertible) default(DataModelDateTime)).ToChar(null));
		Assert.ThrowsException<InvalidCastException>(() => ((IConvertible) default(DataModelDateTime)).ToDecimal(null));
		Assert.ThrowsException<InvalidCastException>(() => ((IConvertible) default(DataModelDateTime)).ToDouble(null));
		Assert.ThrowsException<InvalidCastException>(() => ((IConvertible) default(DataModelDateTime)).ToInt16(null));
		Assert.ThrowsException<InvalidCastException>(() => ((IConvertible) default(DataModelDateTime)).ToInt32(null));
		Assert.ThrowsException<InvalidCastException>(() => ((IConvertible) default(DataModelDateTime)).ToInt64(null));
		Assert.ThrowsException<InvalidCastException>(() => ((IConvertible) default(DataModelDateTime)).ToSByte(null));
		Assert.ThrowsException<InvalidCastException>(() => ((IConvertible) default(DataModelDateTime)).ToSingle(null));
		Assert.ThrowsException<InvalidCastException>(() => ((IConvertible) default(DataModelDateTime)).ToUInt16(null));
		Assert.ThrowsException<InvalidCastException>(() => ((IConvertible) default(DataModelDateTime)).ToUInt32(null));
		Assert.ThrowsException<InvalidCastException>(() => ((IConvertible) default(DataModelDateTime)).ToUInt64(null));
	}

	[TestMethod]
	public void IConvertToDateTime_ShouldReturnSameValue_ForToDateTime()
	{
		Assert.AreEqual(expected: default, ((IConvertible) default(DataModelDateTime)).ToDateTime(null));
	}

	[TestMethod]
	public void IConvertToType_ShouldReturnSameValue_ForToDateTimeOffset()
	{
		Assert.AreEqual(expected: default, (DateTimeOffset) ((IConvertible) default(DataModelDateTime)).ToType(typeof(DateTimeOffset), provider: null));
	}

	[TestMethod]
	public void IConvertToString_ShouldReturnSameValueAsToString()
	{
		var dataModelDateTime = default(DataModelDateTime);
		Assert.AreEqual(dataModelDateTime.ToString(), ((IConvertible) dataModelDateTime).ToString(null));
	}

	[TestMethod]
	public void IConvertGetTypeCode_ShouldReturnCorrectValue()
	{
		// arrange
		var dataModelDateTimeLocal = (DataModelDateTime) new DateTime(TimeSpan.TicksPerDay, DateTimeKind.Local);
		var dataModelDateTimeUtc = (DataModelDateTime) new DateTime(TimeSpan.TicksPerDay, DateTimeKind.Utc);
		var dataModelDateTimeUnspecified = (DataModelDateTime) new DateTime(TimeSpan.TicksPerDay, DateTimeKind.Unspecified);

		// assert
		Assert.AreEqual(TypeCode.Object, ((IConvertible) dataModelDateTimeLocal).GetTypeCode());
		Assert.AreEqual(TypeCode.DateTime, ((IConvertible) dataModelDateTimeUtc).GetTypeCode());
		Assert.AreEqual(TypeCode.DateTime, ((IConvertible) dataModelDateTimeUnspecified).GetTypeCode());
	}

	private static IEnumerable<object[]> WriteToReadFromData()
	{
		yield return [DateTimeWithUnspecifiedKind];
		yield return [DateTimeWithUtcKind];
		yield return [DateTimeWithLocalKind];
		yield return [DateTimeOffsetWithZeroOffset];
		yield return [DateTimeOffsetWithPositiveOffset];
		yield return [DateTimeOffsetWithNegativeOffset];
	}

	[TestMethod]
	[DynamicData(nameof(WriteToReadFromData), DynamicDataSourceType.Method)]
	public void WriteToReadFrom_ShouldRoundtripValue(DataModelDateTime dateTime)
	{
		// arrange
		var buffer = new byte[10];

		// act
		dateTime.WriteTo(buffer);
		var roundtripDateTime = DataModelDateTime.ReadFrom(buffer);

		Assert.AreEqual(dateTime, roundtripDateTime);
	}

	[TestMethod]
	public void Compare_ShouldReturnCorrectValue()
	{
		// arrange
		var d1 = DateTimePoint1Utc;
		var d1A = DateTimePoint1WithOffset;
		var d2 = DateTimePoint2WithOffset;

		// assert
		Assert.IsTrue(d1 > d2);
		Assert.IsTrue(d1 >= d2);
		Assert.IsTrue(d1 >= d1A);
		Assert.IsTrue(d2 < d1);
		Assert.IsTrue(d2 <= d1);
		Assert.IsTrue(d1 <= d1A);
		Assert.IsTrue(d1.CompareTo(d2) > 0);
		Assert.IsTrue(d2.CompareTo(d1) < 0);
		Assert.IsTrue(d1.CompareTo(d1A) == 0);
		Assert.IsTrue(d1A.CompareTo(d1) == 0);

		var d1Cmp = (IComparable) d1;
		var d2Cmp = (IComparable) d2;
		Assert.IsTrue(d1Cmp.CompareTo(d2Cmp) > 0);
		Assert.IsTrue(d2Cmp.CompareTo(d1Cmp) < 0);
		Assert.IsTrue(d1Cmp.CompareTo(d1Cmp) == 0);
	}
}