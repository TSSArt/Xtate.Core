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
using Xtate.DataTypes;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class DataModelNumberCoverageTest
{
	[TestMethod]
	public void FactoryMethodsExposeTypeConversionObjectAndFormattingBehavior()
	{
		var int32 = DataModelNumber.FromInt32(42);
		var int64 = DataModelNumber.FromInt64(4_294_967_296L);
		var dbl = DataModelNumber.FromDouble(12.5D);
		var dec = DataModelNumber.FromDecimal(123.456M);

		Assert.AreEqual(DataModelNumberType.Int32, int32.Type);
		Assert.AreEqual(42, int32.ToInt32());
		Assert.AreEqual(42L, int32.ToInt64());
		Assert.AreEqual(42D, int32.ToDouble());
		Assert.AreEqual(42M, int32.ToDecimal());
		Assert.AreEqual(42, int32.ToObject());

		Assert.AreEqual(DataModelNumberType.Int64, int64.Type);
		Assert.AreEqual(4_294_967_296L, int64.ToObject());

		Assert.AreEqual(DataModelNumberType.Double, dbl.Type);
		Assert.AreEqual(12.5D, dbl.ToObject());

		Assert.AreEqual(DataModelNumberType.Decimal, dec.Type);
		Assert.AreEqual(123.456M, dec.ToObject());
		Assert.AreEqual("123.46", dec.ToString("F2", CultureInfo.InvariantCulture));

		Span<char> chars = stackalloc char[16];
		Assert.IsTrue(dbl.TryFormat(chars, out var written, "F1", CultureInfo.InvariantCulture));
		Assert.AreEqual("12.5", chars[.. written].ToString());
	}

	[TestMethod]
	public void WriteToAndReadFromRoundTripPositiveAndMultiByteNumberKinds()
	{
		DataModelNumber[] numbers =
		[
			DataModelNumber.FromInt32(127),
			DataModelNumber.FromInt32(128),
			DataModelNumber.FromInt64(long.MaxValue / 2),
			DataModelNumber.FromDouble(Math.PI),
			DataModelNumber.FromDecimal(7922816251426433759354395033.5M)
		];

		foreach (var number in numbers)
		{
			var bytes = new byte[number.WriteToSize()];

			number.WriteTo(bytes);
			var restored = DataModelNumber.ReadFrom(bytes);

			Assert.AreEqual(number.Type, restored.Type);
			Assert.AreEqual(number, restored);
			Assert.AreEqual(number.GetHashCode(), restored.GetHashCode());
		}
	}

	[TestMethod]
	public void WriteToAndReadFromRoundTripAllNumberKinds()
	{
		DataModelNumber[] numbers =
		[
			DataModelNumber.FromInt32(-128),
			DataModelNumber.FromInt64(long.MinValue / 2),
			DataModelNumber.FromDouble(Math.PI),
			DataModelNumber.FromDecimal(7922816251426433759354395033.5M)
		];

		foreach (var number in numbers)
		{
			var bytes = new byte[number.WriteToSize()];

			number.WriteTo(bytes);
			var restored = DataModelNumber.ReadFrom(bytes);

			Assert.AreEqual(number.Type, restored.Type);
			Assert.AreEqual(number, restored);
			Assert.AreEqual(number.GetHashCode(), restored.GetHashCode());
		}
	}

	[TestMethod]
	public void ComparisonsHandleDifferentStorageKindsFractionsAndNaN()
	{
		var intValue = DataModelNumber.FromInt64(10);
		var decimalValue = DataModelNumber.FromDecimal(10.25M);
		var doubleValue = DataModelNumber.FromDouble(10.5D);
		var nan = DataModelNumber.FromDouble(double.NaN);

		Assert.IsTrue(intValue < decimalValue);
		Assert.IsTrue(decimalValue < doubleValue);
		Assert.IsTrue(doubleValue > intValue);
		Assert.AreEqual(0, DataModelNumber.FromInt32(10).CompareTo(DataModelNumber.FromDecimal(10M)));
		Assert.AreEqual(1, intValue.CompareTo(null));
		Assert.ThrowsExactly<ArgumentException>(() => intValue.CompareTo("10"));
		Assert.IsTrue(nan.IsNaN());
		Assert.IsFalse(doubleValue.IsNaN());
	}

	[TestMethod]
	public void ConvertibleInterfaceUsesUnderlyingNumberKind()
	{
		IConvertible intNumber = DataModelNumber.FromInt32(7);
		IConvertible doubleNumber = DataModelNumber.FromDouble(7.75D);
		IConvertible decimalNumber = DataModelNumber.FromDecimal(7.25M);

		Assert.AreEqual(TypeCode.Int32, intNumber.GetTypeCode());
		Assert.AreEqual(TypeCode.Double, doubleNumber.GetTypeCode());
		Assert.AreEqual(TypeCode.Decimal, decimalNumber.GetTypeCode());
		Assert.AreEqual(true, intNumber.ToBoolean(CultureInfo.InvariantCulture));
		Assert.AreEqual(7.75D, doubleNumber.ToDouble(CultureInfo.InvariantCulture));
		Assert.AreEqual(7.25M, decimalNumber.ToDecimal(CultureInfo.InvariantCulture));
		Assert.AreEqual("7", intNumber.ToString(CultureInfo.InvariantCulture));
		Assert.AreEqual(7L, intNumber.ToType(typeof(long), CultureInfo.InvariantCulture));
	}
}
