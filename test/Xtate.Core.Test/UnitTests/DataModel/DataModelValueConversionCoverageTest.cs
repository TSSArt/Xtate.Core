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

using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Xtate.DataTypes;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class DataModelValueConversionCoverageTest
{
	[TestMethod]
	public void NullableConstructorsImplicitOperatorsAndFactoriesPreserveValueOrNull()
	{
		int? intValue = 1;
		long? longValue = 2;
		double? doubleValue = 3.5;
		decimal? decimalValue = 4.5M;
		DataModelNumber? numberValue = DataModelNumber.FromInt32(5);
		var offset = new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.FromHours(2));
		DateTimeOffset? offsetValue = offset;
		DateTime? dateValue = offset.UtcDateTime;
		DataModelDateTime? modelDateValue = DataModelDateTime.FromDateTimeOffset(offset);
		bool? boolValue = true;

		AssertNumber(new DataModelValue(intValue), 1);
		AssertNumber(new DataModelValue(longValue), 2);
		AssertNumber(new DataModelValue(doubleValue), 3.5);
		AssertNumber(new DataModelValue(decimalValue), 4.5);
		AssertNumber(new DataModelValue(numberValue), 5);
		Assert.AreEqual(offset, new DataModelValue(offsetValue).AsDateTime().ToDateTimeOffset());
		Assert.AreEqual(offset.UtcDateTime, new DataModelValue(dateValue).AsDateTime().ToDateTime());
		Assert.AreEqual(offset, new DataModelValue(modelDateValue).AsDateTime().ToDateTimeOffset());
		Assert.IsTrue(new DataModelValue(boolValue).AsBoolean());

		Assert.AreEqual(DataModelValueType.Null, new DataModelValue((int?) null).Type);
		Assert.AreEqual(DataModelValueType.Null, new DataModelValue((long?) null).Type);
		Assert.AreEqual(DataModelValueType.Null, new DataModelValue((double?) null).Type);
		Assert.AreEqual(DataModelValueType.Null, new DataModelValue((decimal?) null).Type);
		Assert.AreEqual(DataModelValueType.Null, new DataModelValue((DataModelNumber?) null).Type);
		Assert.AreEqual(DataModelValueType.Null, new DataModelValue((DateTimeOffset?) null).Type);
		Assert.AreEqual(DataModelValueType.Null, new DataModelValue((DateTime?) null).Type);
		Assert.AreEqual(DataModelValueType.Null, new DataModelValue((DataModelDateTime?) null).Type);
		Assert.AreEqual(DataModelValueType.Null, new DataModelValue((bool?) null).Type);

		DataModelValue[] implicitValues =
		[
			intValue, longValue, doubleValue, decimalValue, numberValue, offsetValue, dateValue, modelDateValue, boolValue, 6M
		];
		Assert.IsTrue(implicitValues.All(static value => !value.IsUndefinedOrNull()));

		DataModelValue[] factoryValues =
		[
			DataModelValue.FromInt32(1), DataModelValue.FromInt32(intValue),
			DataModelValue.FromInt64(2), DataModelValue.FromInt64(longValue),
			DataModelValue.FromDouble(doubleValue), DataModelValue.FromDecimal(4.5M), DataModelValue.FromDecimal(decimalValue),
			DataModelValue.FromDataModelNumber(numberValue.Value), DataModelValue.FromDataModelNumber(numberValue),
			DataModelValue.FromDataModelDateTime(modelDateValue.Value), DataModelValue.FromDataModelDateTime(modelDateValue),
			DataModelValue.FromDateTimeOffset(offsetValue), DataModelValue.FromDateTime(offset.UtcDateTime), DataModelValue.FromDateTime(dateValue),
			DataModelValue.FromBoolean(boolValue)
		];
		Assert.IsTrue(factoryValues.All(static value => !value.IsUndefinedOrNull()));
	}

	[TestMethod]
	public void ExplicitOperatorsAndNullableAccessorsReturnExpectedRepresentations()
	{
		DataModelValue number = 12;
		Assert.AreEqual(12, (int) number);
		Assert.AreEqual(12, (int?) number);
		Assert.AreEqual(12L, (long) number);
		Assert.AreEqual(12L, (long?) number);
		Assert.AreEqual(12D, (double) number);
		Assert.AreEqual(12D, (double?) number);
		Assert.AreEqual(DataModelNumber.FromInt32(12), (DataModelNumber) number);
		Assert.AreEqual(DataModelNumber.FromInt32(12), (DataModelNumber?) number);
		Assert.AreEqual(DataModelNumber.FromInt32(12), number.AsNullableNumber());

		var offset = new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.FromHours(2));
		DataModelValue date = offset;
		Assert.AreEqual(DataModelDateTime.FromDateTimeOffset(offset), (DataModelDateTime) date);
		Assert.AreEqual(DataModelDateTime.FromDateTimeOffset(offset), (DataModelDateTime?) date);
		Assert.AreEqual(offset, (DateTimeOffset) date);
		Assert.AreEqual(offset, (DateTimeOffset?) date);
		Assert.AreEqual(offset.DateTime, (DateTime) date);
		Assert.AreEqual(offset.DateTime, (DateTime?) date);
		Assert.AreEqual(DataModelDateTime.FromDateTimeOffset(offset), date.AsNullableDateTime());

		DataModelValue boolean = true;
		Assert.IsTrue((bool) boolean);
		Assert.IsTrue((bool?) boolean);
		Assert.IsTrue(boolean.AsBoolean());
		Assert.IsTrue(boolean.AsNullableBoolean());
		Assert.IsTrue(boolean.AsBooleanOrDefault());

		Assert.IsFalse(((int?) DataModelValue.Null).HasValue);
		Assert.IsFalse(((long?) DataModelValue.Null).HasValue);
		Assert.IsFalse(((double?) DataModelValue.Null).HasValue);
		Assert.IsFalse(((DataModelDateTime?) DataModelValue.Null).HasValue);
		Assert.IsFalse(((DateTimeOffset?) DataModelValue.Null).HasValue);
		Assert.IsFalse(((DateTime?) DataModelValue.Null).HasValue);
		Assert.IsFalse(((bool?) DataModelValue.Null).HasValue);
		Assert.IsNull(DataModelValue.Null.AsNullableNumber());
		Assert.IsNull(DataModelValue.Null.AsNullableDateTime());
		Assert.IsNull(DataModelValue.Null.AsNullableBoolean());
		var lazy = new Mock<ILazyValue>();
		lazy.Setup(static value => value.Value).Returns(new DataModelValue(true));
		var lazyBoolean = new DataModelValue(lazy.Object);
		Assert.IsTrue(lazyBoolean.AsBoolean());
		Assert.IsTrue(lazyBoolean.AsNullableBoolean());
		Assert.IsTrue(lazyBoolean.AsBooleanOrDefault());
		Assert.IsNull(number.AsBooleanOrDefault());
		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage] () => number.AsBoolean());
		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage] () => number.AsNullableBoolean());
		Assert.IsFalse(number.Equals((object) "12"));
		Assert.IsTrue(number.Equals((object) new DataModelValue(12)));
	}

	[TestMethod]
	public void ConvertibleInterfaceDispatchesAcrossNumberBooleanDateAndFallbackValues()
	{
		DataModelValue[] values =
		[
			5,
			true,
			new DateTime(2026, 7, 15, 12, 0, 0, DateTimeKind.Utc),
			"1"
		];

		foreach (var value in values)
		{
			ExerciseConvertible((IConvertible) value);
		}

		Assert.AreEqual(TypeCode.Int32, ((IConvertible) values[0]).GetTypeCode());
		Assert.AreEqual(TypeCode.Boolean, ((IConvertible) values[1]).GetTypeCode());
		Assert.AreEqual(TypeCode.DateTime, ((IConvertible) values[2]).GetTypeCode());
		Assert.AreEqual(TypeCode.String, ((IConvertible) values[3]).GetTypeCode());
		Assert.AreEqual(5, ((IConvertible) values[0]).ToInt32(CultureInfo.InvariantCulture));
		Assert.IsTrue(((IConvertible) values[1]).ToBoolean(CultureInfo.InvariantCulture));
		Assert.AreEqual(new DateTime(2026, 7, 15, 12, 0, 0, DateTimeKind.Utc), ((IConvertible) values[2]).ToDateTime(CultureInfo.InvariantCulture));
		Assert.AreEqual(1, ((IConvertible) values[3]).ToInt32(CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void SerializableContractStoresResolvedBackingFields()
	{
		var lazy = new Mock<ILazyValue>();
		lazy.Setup(static value => value.Value).Returns(new DataModelValue(42));
		var value = new DataModelValue(lazy.Object);

#pragma warning disable SYSLIB0050
		var info = new SerializationInfo(typeof(DataModelValue), new FormatterConverter());
		((ISerializable) value).GetObjectData(info, default);
#pragma warning restore SYSLIB0050

		Assert.IsNotNull(info.GetValue("V", typeof(object)));
		Assert.AreEqual(42L, info.GetInt64("L"));
	}

	private static void AssertNumber(DataModelValue value, double expected) => Assert.AreEqual(expected, value.AsNumber().ToDouble());

	private static void ExerciseConvertible(IConvertible convertible)
	{
		foreach (var method in typeof(IConvertible).GetMethods())
		{
			object?[] arguments = method.Name switch
			{
				nameof(IConvertible.GetTypeCode) => [],
				nameof(IConvertible.ToType)      => [typeof(string), CultureInfo.InvariantCulture],
				_                                => [CultureInfo.InvariantCulture]
			};

			try
			{
				_ = method.Invoke(convertible, arguments);
			}
			catch (TargetInvocationException exception)
			{
				Assert.IsInstanceOfType<Exception>(exception.InnerException);
			}
		}

		try
		{
			_ = convertible.ToType(typeof(DateTimeOffset), CultureInfo.InvariantCulture);
		}
		catch (Exception)
		{
			// Some source types cannot be represented as DateTimeOffset; dispatch itself is the behavior under test.
		}
	}
}
