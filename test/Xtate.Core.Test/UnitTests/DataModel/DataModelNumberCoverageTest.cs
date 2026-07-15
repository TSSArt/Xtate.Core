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
		Assert.AreEqual(expected: 42, int32.ToInt32());
		Assert.AreEqual(expected: 42L, int32.ToInt64());
		Assert.AreEqual(expected: 42D, int32.ToDouble());
		Assert.AreEqual(expected: 42M, int32.ToDecimal());
		Assert.AreEqual(expected: 42, int32.ToObject());

		Assert.AreEqual(DataModelNumberType.Int64, int64.Type);
		Assert.AreEqual(expected: 4_294_967_296L, int64.ToObject());

		Assert.AreEqual(DataModelNumberType.Double, dbl.Type);
		Assert.AreEqual(expected: 12.5D, dbl.ToObject());

		Assert.AreEqual(DataModelNumberType.Decimal, dec.Type);
		Assert.AreEqual(expected: 123.456M, dec.ToObject());
		Assert.AreEqual(expected: "123.46", dec.ToString(format: "F2", CultureInfo.InvariantCulture));

		Span<char> chars = stackalloc char[16];
		Assert.IsTrue(dbl.TryFormat(chars, out var written, format: "F1", CultureInfo.InvariantCulture));
		Assert.AreEqual(expected: "12.5", chars[.. written].ToString());
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
		Assert.AreEqual(expected: 0, DataModelNumber.FromInt32(10).CompareTo(DataModelNumber.FromDecimal(10M)));
		Assert.AreEqual(expected: 1, intValue.CompareTo(null));
		try
		{
			_ = intValue.CompareTo("10");
			Assert.Fail("An unrelated comparison value did not throw.");
		}
		catch (ArgumentException)
		{
		}
		Assert.IsTrue(nan.IsNaN());
		Assert.IsFalse(doubleValue.IsNaN());
		Assert.IsTrue(intValue.Equals((object) DataModelNumber.FromInt64(10)));
		Assert.IsFalse(intValue.Equals((object) "10"));
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
		Assert.AreEqual(expected: true, intNumber.ToBoolean(CultureInfo.InvariantCulture));
		Assert.AreEqual(expected: 7.75D, doubleNumber.ToDouble(CultureInfo.InvariantCulture));
		Assert.AreEqual(expected: 7.25M, decimalNumber.ToDecimal(CultureInfo.InvariantCulture));
		Assert.AreEqual(expected: "7", intNumber.ToString(CultureInfo.InvariantCulture));
		Assert.AreEqual(expected: 7L, intNumber.ToType(typeof(long), CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void ReflectionCoversConvertibleOperatorsAndConvenienceMembersForAllKinds()
	{
		var int32 = DataModelNumber.FromInt32(65);
		var int64 = DataModelNumber.FromInt64(66);
		var dbl = DataModelNumber.FromDouble(67.5D);
		var dec = DataModelNumber.FromDecimal(68.5M);
		var values = new[] { int32, int64, dbl, dec };

		foreach (var value in values)
		{
			Assert.AreEqual(value.ToInt32(), InvokeConvertible(value, nameof(IConvertible.ToInt32)));
			Assert.AreEqual(value.ToInt64(), InvokeConvertible(value, nameof(IConvertible.ToInt64)));
			Assert.AreEqual(Convert.ToUInt64(value.ToObject(), CultureInfo.InvariantCulture), InvokeConvertible(value, nameof(IConvertible.ToUInt64)));
			Assert.AreEqual(Convert.ToBoolean(value.ToObject(), CultureInfo.InvariantCulture), InvokeConvertible(value, nameof(IConvertible.ToBoolean)));
			Assert.AreEqual(Convert.ToSingle(value.ToObject(), CultureInfo.InvariantCulture), InvokeConvertible(value, nameof(IConvertible.ToSingle)));
			Assert.AreEqual(Convert.ToInt64(value.ToObject(), CultureInfo.InvariantCulture), InvokeConvertible(value, nameof(IConvertible.ToType), typeof(long)));
			Assert.AreEqual(value.ToString("F1", CultureInfo.InvariantCulture), value.ToString("F1"));
			Assert.IsTrue(value.Equals((object) value));
		}

		Assert.AreEqual(TypeCode.Int64, ((IConvertible) int64).GetTypeCode());
		Assert.AreEqual(expected: 'A', InvokeConvertible(int32, nameof(IConvertible.ToChar)));
		Assert.AreEqual(expected: (sbyte) 65, InvokeConvertible(int32, nameof(IConvertible.ToSByte)));
		Assert.AreEqual(expected: (byte) 65, InvokeConvertible(int32, nameof(IConvertible.ToByte)));
		Assert.AreEqual(expected: (short) 65, InvokeConvertible(int32, nameof(IConvertible.ToInt16)));
		Assert.AreEqual(expected: (ushort) 65, InvokeConvertible(int32, nameof(IConvertible.ToUInt16)));
		Assert.AreEqual(expected: (uint) 65, InvokeConvertible(int32, nameof(IConvertible.ToUInt32)));

		var explicitOperators = typeof(DataModelNumber).GetMethods(BindingFlags.Public | BindingFlags.Static)
											   .Where(method => method.Name == "op_Explicit" && method.GetParameters()[0].ParameterType == typeof(DataModelNumber))
											   .ToDictionary(method => method.ReturnType);
		Assert.AreEqual(expected: 65, explicitOperators[typeof(int)].Invoke(obj: null, [int32]));
		Assert.AreEqual(expected: 65L, explicitOperators[typeof(long)].Invoke(obj: null, [int32]));
		Assert.AreEqual(expected: 67.5D, explicitOperators[typeof(double)].Invoke(obj: null, [dbl]));
		Assert.AreEqual(expected: 68.5M, explicitOperators[typeof(decimal)].Invoke(obj: null, [dec]));

		Assert.IsTrue(InvokeOperator("op_Equality", int32, DataModelNumber.FromInt64(65)));
		Assert.IsTrue(InvokeOperator("op_Inequality", int32, int64));
		Assert.IsTrue(InvokeOperator("op_LessThanOrEqual", int32, int64));
		Assert.IsTrue(InvokeOperator("op_GreaterThanOrEqual", int64, int32));
	}

	private static object? InvokeConvertible(DataModelNumber value, string methodName, Type? conversionType = null)
	{
		var map = typeof(DataModelNumber).GetInterfaceMap(typeof(IConvertible));
		var index = Array.FindIndex(map.InterfaceMethods, method => method.Name == methodName);
		var parameters = conversionType is null ? new object?[] { CultureInfo.InvariantCulture } : [conversionType, CultureInfo.InvariantCulture];

		return map.TargetMethods[index].Invoke(value, parameters);
	}

	private static bool InvokeOperator(string methodName, DataModelNumber left, DataModelNumber right) =>
		(bool) typeof(DataModelNumber).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)!.Invoke(obj: null, [left, right])!;
}
