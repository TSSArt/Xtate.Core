#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.IoC.Test;

[TestClass]
public class TupleHelperTest
{
	[DataTestMethod]
	public void GenericUnwrap2Test()
	{
		// Arrange
		(byte, sbyte) arg = (1, 2);

		// Act
		var result = TupleHelper.TryMatch(ref arg, out (sbyte, byte) value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expected: 2, Convert.ToInt32(value.Item1));
		Assert.AreEqual(expected: 1, Convert.ToInt32(value.Item2));
	}

	[DataTestMethod]
	public void GenericUnwrap3Test()
	{
		// Arrange
		(byte, sbyte, short) arg = (1, 2, 3);

		// Act
		var result = TupleHelper.TryMatch(ref arg, out (short, sbyte, byte) value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expected: 3, Convert.ToInt32(value.Item1));
		Assert.AreEqual(expected: 2, Convert.ToInt32(value.Item2));
		Assert.AreEqual(expected: 1, Convert.ToInt32(value.Item3));
	}

	[DataTestMethod]
	public void GenericUnwrap4Test()
	{
		// Arrange
		(byte, sbyte, short, ushort) arg = (1, 2, 3, 4);

		// Act
		var result = TupleHelper.TryMatch(ref arg, out (ushort, short, sbyte, byte) value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expected: 4, Convert.ToInt32(value.Item1));
		Assert.AreEqual(expected: 3, Convert.ToInt32(value.Item2));
		Assert.AreEqual(expected: 2, Convert.ToInt32(value.Item3));
		Assert.AreEqual(expected: 1, Convert.ToInt32(value.Item4));
	}

	[DataTestMethod]
	public void GenericUnwrap5Test()
	{
		// Arrange
		(byte, sbyte, short, ushort, int) arg = (1, 2, 3, 4, 5);

		// Act
		var result = TupleHelper.TryMatch(ref arg, out (int, ushort, short, sbyte, byte) value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expected: 5, Convert.ToInt32(value.Item1));
		Assert.AreEqual(expected: 4, Convert.ToInt32(value.Item2));
		Assert.AreEqual(expected: 3, Convert.ToInt32(value.Item3));
		Assert.AreEqual(expected: 2, Convert.ToInt32(value.Item4));
		Assert.AreEqual(expected: 1, Convert.ToInt32(value.Item5));
	}

	[DataTestMethod]
	public void GenericUnwrap6Test()
	{
		// Arrange
		(byte, sbyte, short, ushort, int, uint) arg = (1, 2, 3, 4, 5, 6);

		// Act
		var result = TupleHelper.TryMatch(ref arg, out (uint, int, ushort, short, sbyte, byte) value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expected: 6, Convert.ToInt32(value.Item1));
		Assert.AreEqual(expected: 5, Convert.ToInt32(value.Item2));
		Assert.AreEqual(expected: 4, Convert.ToInt32(value.Item3));
		Assert.AreEqual(expected: 3, Convert.ToInt32(value.Item4));
		Assert.AreEqual(expected: 2, Convert.ToInt32(value.Item5));
		Assert.AreEqual(expected: 1, Convert.ToInt32(value.Item6));
	}

	[DataTestMethod]
	public void GenericUnwrap7Test()
	{
		// Arrange
		(byte, sbyte, short, ushort, int, uint, long) arg = (1, 2, 3, 4, 5, 6, 7);

		// Act
		var result = TupleHelper.TryMatch(ref arg, out (long, uint, int, ushort, short, sbyte, byte) value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expected: 7, Convert.ToInt32(value.Item1));
		Assert.AreEqual(expected: 6, Convert.ToInt32(value.Item2));
		Assert.AreEqual(expected: 5, Convert.ToInt32(value.Item3));
		Assert.AreEqual(expected: 4, Convert.ToInt32(value.Item4));
		Assert.AreEqual(expected: 3, Convert.ToInt32(value.Item5));
		Assert.AreEqual(expected: 2, Convert.ToInt32(value.Item6));
		Assert.AreEqual(expected: 1, Convert.ToInt32(value.Item7));
	}

	[DataTestMethod]
	public void GenericUnwrap8Test()
	{
		// Arrange
		(byte, sbyte, short, ushort, int, uint, long, ulong) arg = (1, 2, 3, 4, 5, 6, 7, 8);

		// Act
		var result = TupleHelper.TryMatch(ref arg, out (ulong, long, uint, int, ushort, short, sbyte, byte) value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expected: 8, Convert.ToInt32(value.Item1));
		Assert.AreEqual(expected: 7, Convert.ToInt32(value.Item2));
		Assert.AreEqual(expected: 6, Convert.ToInt32(value.Item3));
		Assert.AreEqual(expected: 5, Convert.ToInt32(value.Item4));
		Assert.AreEqual(expected: 4, Convert.ToInt32(value.Item5));
		Assert.AreEqual(expected: 3, Convert.ToInt32(value.Item6));
		Assert.AreEqual(expected: 2, Convert.ToInt32(value.Item7));
		Assert.AreEqual(expected: 1, Convert.ToInt32(value.Item8));
	}

	[DataTestMethod]
	public void GenericUnwrapNestedTest()
	{
		// Arrange
		(byte, (sbyte, short, (ushort, int), uint), long, ulong) arg = (1, (2, 3, (4, 5), 6), 7, 8);

		// Act
		var result = TupleHelper.TryMatch(ref arg, out (ulong, long, (uint, int), ushort, (short, sbyte), byte) value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expected: 8, Convert.ToInt32(value.Item1));
		Assert.AreEqual(expected: 7, Convert.ToInt32(value.Item2));
		Assert.AreEqual(expected: 6, Convert.ToInt32(value.Item3.Item1));
		Assert.AreEqual(expected: 5, Convert.ToInt32(value.Item3.Item2));
		Assert.AreEqual(expected: 4, Convert.ToInt32(value.Item4));
		Assert.AreEqual(expected: 3, Convert.ToInt32(value.Item5.Item1));
		Assert.AreEqual(expected: 2, Convert.ToInt32(value.Item5.Item2));
		Assert.AreEqual(expected: 1, Convert.ToInt32(value.Item6));
	}

	[TestMethod]
	public void GenericTryMatch1Test()
	{
		// Arrange
		byte arg = 1;

		// Act
		var result = TupleHelper.TryMatch(ref arg, out byte value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expected: 1, Convert.ToInt32(value));
	}

	[DataTestMethod]
	public void GenericTryMatch2Test()
	{
		// Arrange
		(byte, sbyte) arg = (1, 2);

		// Act
		var result1 = TupleHelper.TryMatch(ref arg, out byte value1);
		var result2 = TupleHelper.TryMatch(ref arg, out sbyte value2);

		// Assert
		Assert.IsTrue(result1);
		Assert.AreEqual(expected: 1, Convert.ToInt32(value1));
		Assert.IsTrue(result2);
		Assert.AreEqual(expected: 2, Convert.ToInt32(value2));
	}

	[DataTestMethod]
	public void GenericTryMatch3Test()
	{
		// Arrange
		(byte, sbyte, short) arg = (1, 2, 3);

		// Act
		var result1 = TupleHelper.TryMatch(ref arg, out byte value1);
		var result2 = TupleHelper.TryMatch(ref arg, out sbyte value2);
		var result3 = TupleHelper.TryMatch(ref arg, out short value3);

		// Assert
		Assert.IsTrue(result1);
		Assert.AreEqual(expected: 1, Convert.ToInt32(value1));
		Assert.IsTrue(result2);
		Assert.AreEqual(expected: 2, Convert.ToInt32(value2));
		Assert.IsTrue(result3);
		Assert.AreEqual(expected: 3, Convert.ToInt32(value3));
	}

	[DataTestMethod]
	public void GenericTryMatch4Test()
	{
		// Arrange
		(byte, sbyte, short, ushort) arg = (1, 2, 3, 4);

		// Act
		var result1 = TupleHelper.TryMatch(ref arg, out byte value1);
		var result2 = TupleHelper.TryMatch(ref arg, out sbyte value2);
		var result3 = TupleHelper.TryMatch(ref arg, out short value3);
		var result4 = TupleHelper.TryMatch(ref arg, out ushort value4);

		// Assert
		Assert.IsTrue(result1);
		Assert.AreEqual(expected: 1, Convert.ToInt32(value1));
		Assert.IsTrue(result2);
		Assert.AreEqual(expected: 2, Convert.ToInt32(value2));
		Assert.IsTrue(result3);
		Assert.AreEqual(expected: 3, Convert.ToInt32(value3));
		Assert.IsTrue(result4);
		Assert.AreEqual(expected: 4, Convert.ToInt32(value4));
	}

	[DataTestMethod]
	public void GenericTryMatch5Test()
	{
		// Arrange
		(byte, sbyte, short, ushort, int) arg = (1, 2, 3, 4, 5);

		// Act
		var result1 = TupleHelper.TryMatch(ref arg, out byte value1);
		var result2 = TupleHelper.TryMatch(ref arg, out sbyte value2);
		var result3 = TupleHelper.TryMatch(ref arg, out short value3);
		var result4 = TupleHelper.TryMatch(ref arg, out ushort value4);
		var result5 = TupleHelper.TryMatch(ref arg, out int value5);

		// Assert
		Assert.IsTrue(result1);
		Assert.AreEqual(expected: 1, Convert.ToInt32(value1));
		Assert.IsTrue(result2);
		Assert.AreEqual(expected: 2, Convert.ToInt32(value2));
		Assert.IsTrue(result3);
		Assert.AreEqual(expected: 3, Convert.ToInt32(value3));
		Assert.IsTrue(result4);
		Assert.AreEqual(expected: 4, Convert.ToInt32(value4));
		Assert.IsTrue(result5);
		Assert.AreEqual(expected: 5, Convert.ToInt32(value5));
	}

	[DataTestMethod]
	public void GenericTryMatch6Test()
	{
		// Arrange
		(byte, sbyte, short, ushort, int, uint) arg = (1, 2, 3, 4, 5, 6);

		// Act
		var result1 = TupleHelper.TryMatch(ref arg, out byte value1);
		var result2 = TupleHelper.TryMatch(ref arg, out sbyte value2);
		var result3 = TupleHelper.TryMatch(ref arg, out short value3);
		var result4 = TupleHelper.TryMatch(ref arg, out ushort value4);
		var result5 = TupleHelper.TryMatch(ref arg, out int value5);
		var result6 = TupleHelper.TryMatch(ref arg, out uint value6);

		// Assert
		Assert.IsTrue(result1);
		Assert.AreEqual(expected: 1, Convert.ToInt32(value1));
		Assert.IsTrue(result2);
		Assert.AreEqual(expected: 2, Convert.ToInt32(value2));
		Assert.IsTrue(result3);
		Assert.AreEqual(expected: 3, Convert.ToInt32(value3));
		Assert.IsTrue(result4);
		Assert.AreEqual(expected: 4, Convert.ToInt32(value4));
		Assert.IsTrue(result5);
		Assert.AreEqual(expected: 5, Convert.ToInt32(value5));
		Assert.IsTrue(result6);
		Assert.AreEqual(expected: 6, Convert.ToInt32(value6));
	}

	[DataTestMethod]
	public void GenericTryMatch7Test()
	{
		// Arrange
		(byte, sbyte, short, ushort, int, uint, long) arg = (1, 2, 3, 4, 5, 6, 7);

		// Act
		var result1 = TupleHelper.TryMatch(ref arg, out byte value1);
		var result2 = TupleHelper.TryMatch(ref arg, out sbyte value2);
		var result3 = TupleHelper.TryMatch(ref arg, out short value3);
		var result4 = TupleHelper.TryMatch(ref arg, out ushort value4);
		var result5 = TupleHelper.TryMatch(ref arg, out int value5);
		var result6 = TupleHelper.TryMatch(ref arg, out uint value6);
		var result7 = TupleHelper.TryMatch(ref arg, out long value7);

		// Assert
		Assert.IsTrue(result1);
		Assert.AreEqual(expected: 1, Convert.ToInt32(value1));
		Assert.IsTrue(result2);
		Assert.AreEqual(expected: 2, Convert.ToInt32(value2));
		Assert.IsTrue(result3);
		Assert.AreEqual(expected: 3, Convert.ToInt32(value3));
		Assert.IsTrue(result4);
		Assert.AreEqual(expected: 4, Convert.ToInt32(value4));
		Assert.IsTrue(result5);
		Assert.AreEqual(expected: 5, Convert.ToInt32(value5));
		Assert.IsTrue(result6);
		Assert.AreEqual(expected: 6, Convert.ToInt32(value6));
		Assert.IsTrue(result7);
		Assert.AreEqual(expected: 7, Convert.ToInt32(value7));
	}

	[DataTestMethod]
	public void GenericTryMatch8Test()
	{
		// Arrange
		(byte, sbyte, short, ushort, int, uint, long, ulong) arg = (1, 2, 3, 4, 5, 6, 7, 8);

		// Act
		var result1 = TupleHelper.TryMatch(ref arg, out byte value1);
		var result2 = TupleHelper.TryMatch(ref arg, out sbyte value2);
		var result3 = TupleHelper.TryMatch(ref arg, out short value3);
		var result4 = TupleHelper.TryMatch(ref arg, out ushort value4);
		var result5 = TupleHelper.TryMatch(ref arg, out int value5);
		var result6 = TupleHelper.TryMatch(ref arg, out uint value6);
		var result7 = TupleHelper.TryMatch(ref arg, out long value7);
		var result8 = TupleHelper.TryMatch(ref arg, out ulong value8);

		// Assert
		Assert.IsTrue(result1);
		Assert.AreEqual(expected: 1, Convert.ToInt32(value1));
		Assert.IsTrue(result2);
		Assert.AreEqual(expected: 2, Convert.ToInt32(value2));
		Assert.IsTrue(result3);
		Assert.AreEqual(expected: 3, Convert.ToInt32(value3));
		Assert.IsTrue(result4);
		Assert.AreEqual(expected: 4, Convert.ToInt32(value4));
		Assert.IsTrue(result5);
		Assert.AreEqual(expected: 5, Convert.ToInt32(value5));
		Assert.IsTrue(result6);
		Assert.AreEqual(expected: 6, Convert.ToInt32(value6));
		Assert.IsTrue(result7);
		Assert.AreEqual(expected: 7, Convert.ToInt32(value7));
		Assert.IsTrue(result8);
		Assert.AreEqual(expected: 8, Convert.ToInt32(value8));
	}

	[DataTestMethod]
	public void GenericTryMatchNestedTest()
	{
		// Arrange
		((byte, sbyte), short) arg = ((1, 2), 3);

		// Act
		var result1 = TupleHelper.TryMatch(ref arg, out byte value1);
		var result2 = TupleHelper.TryMatch(ref arg, out sbyte value2);
		var result3 = TupleHelper.TryMatch(ref arg, out short value3);

		// Assert
		Assert.IsTrue(result1);
		Assert.AreEqual(expected: 1, Convert.ToInt32(value1));
		Assert.IsTrue(result2);
		Assert.AreEqual(expected: 2, Convert.ToInt32(value2));
		Assert.IsTrue(result3);
		Assert.AreEqual(expected: 3, Convert.ToInt32(value3));
	}

	[TestMethod]
	public void IsMatch1Test()
	{
		// Arrange

		// Act
		var result = TupleHelper.IsMatch<byte>(typeof(byte));

		// Assert
		Assert.IsTrue(result);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	public void IsMatch2Test(Type type)
	{
		// Arrange

		// Act
		var result = TupleHelper.IsMatch<(byte, sbyte)>(type);

		// Assert
		Assert.IsTrue(result);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	public void IsMatch3Test(Type type)
	{
		// Arrange

		// Act
		var result = TupleHelper.IsMatch<(byte, sbyte, short)>(type);

		// Assert
		Assert.IsTrue(result);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	[DataRow(typeof(ushort))]
	public void IsMatch4Test(Type type)
	{
		// Arrange

		// Act
		var result = TupleHelper.IsMatch<(byte, sbyte, short, ushort)>(type);

		// Assert
		Assert.IsTrue(result);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	[DataRow(typeof(ushort))]
	[DataRow(typeof(int))]
	public void IsMatch5Test(Type type)
	{
		// Arrange

		// Act
		var result = TupleHelper.IsMatch<(byte, sbyte, short, ushort, int)>(type);

		// Assert
		Assert.IsTrue(result);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	[DataRow(typeof(ushort))]
	[DataRow(typeof(int))]
	[DataRow(typeof(uint))]
	public void IsMatch6Test(Type type)
	{
		// Arrange

		// Act
		var result = TupleHelper.IsMatch<(byte, sbyte, short, ushort, int, uint)>(type);

		// Assert
		Assert.IsTrue(result);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	[DataRow(typeof(ushort))]
	[DataRow(typeof(int))]
	[DataRow(typeof(uint))]
	[DataRow(typeof(long))]
	public void IsMatch7Test(Type type)
	{
		// Arrange

		// Act
		var result = TupleHelper.IsMatch<(byte, sbyte, short, ushort, int, uint, long)>(type);

		// Assert
		Assert.IsTrue(result);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	[DataRow(typeof(ushort))]
	[DataRow(typeof(int))]
	[DataRow(typeof(uint))]
	[DataRow(typeof(long))]
	[DataRow(typeof(ulong))]
	public void IsMatch8Test(Type type)
	{
		// Arrange

		// Act
		var result = TupleHelper.IsMatch<(byte, sbyte, short, ushort, int, uint, long, ulong)>(type);

		// Assert
		Assert.IsTrue(result);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	public void IsMatchNestedTest(Type type)
	{
		// Arrange

		// Act
		var result = TupleHelper.IsMatch<((byte, sbyte), short)>(type);

		// Assert
		Assert.IsNotNull(result);
	}

	[TestMethod]
	public void TryMatch1Test()
	{
		// Arrange
		byte arg = 1;

		// Act
		var result = TupleHelper.TryMatch(typeof(byte), ref arg, out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expected: 1, Convert.ToInt32(value));
	}

	[DataTestMethod]
	[DataRow(typeof(byte), 1)]
	[DataRow(typeof(sbyte), 2)]
	public void TryMatch2Test(Type type, object val)
	{
		// Arrange
		(byte, sbyte) arg = (1, 2);

		// Act
		var result = TupleHelper.TryMatch(type, ref arg, out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(Convert.ToInt32(val), Convert.ToInt32(value));
	}

	[DataTestMethod]
	[DataRow(typeof(byte), 1)]
	[DataRow(typeof(sbyte), 2)]
	[DataRow(typeof(short), 3)]
	public void TryMatch3Test(Type type, object val)
	{
		// Arrange
		(byte, sbyte, short) arg = (1, 2, 3);

		// Act
		var result = TupleHelper.TryMatch(type, ref arg, out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(Convert.ToInt32(val), Convert.ToInt32(value));
	}

	[DataTestMethod]
	[DataRow(typeof(byte), 1)]
	[DataRow(typeof(sbyte), 2)]
	[DataRow(typeof(short), 3)]
	[DataRow(typeof(ushort), 4)]
	public void TryMatch4Test(Type type, object val)
	{
		// Arrange
		(byte, sbyte, short, ushort) arg = (1, 2, 3, 4);

		// Act
		var result = TupleHelper.TryMatch(type, ref arg, out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(Convert.ToInt32(val), Convert.ToInt32(value));
	}

	[DataTestMethod]
	[DataRow(typeof(byte), 1)]
	[DataRow(typeof(sbyte), 2)]
	[DataRow(typeof(short), 3)]
	[DataRow(typeof(ushort), 4)]
	[DataRow(typeof(int), 5)]
	public void TryMatch5Test(Type type, object val)
	{
		// Arrange
		(byte, sbyte, short, ushort, int) arg = (1, 2, 3, 4, 5);

		// Act
		var result = TupleHelper.TryMatch(type, ref arg, out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(Convert.ToInt32(val), Convert.ToInt32(value));
	}

	[DataTestMethod]
	[DataRow(typeof(byte), 1)]
	[DataRow(typeof(sbyte), 2)]
	[DataRow(typeof(short), 3)]
	[DataRow(typeof(ushort), 4)]
	[DataRow(typeof(int), 5)]
	[DataRow(typeof(uint), 6)]
	public void TryMatch6Test(Type type, object val)
	{
		// Arrange
		(byte, sbyte, short, ushort, int, uint) arg = (1, 2, 3, 4, 5, 6);

		// Act
		var result = TupleHelper.TryMatch(type, ref arg, out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(Convert.ToInt32(val), Convert.ToInt32(value));
	}

	[DataTestMethod]
	[DataRow(typeof(byte), 1)]
	[DataRow(typeof(sbyte), 2)]
	[DataRow(typeof(short), 3)]
	[DataRow(typeof(ushort), 4)]
	[DataRow(typeof(int), 5)]
	[DataRow(typeof(uint), 6)]
	[DataRow(typeof(long), 7)]
	public void TryMatch7Test(Type type, object val)
	{
		// Arrange
		(byte, sbyte, short, ushort, int, uint, long) arg = (1, 2, 3, 4, 5, 6, 7);

		// Act
		var result = TupleHelper.TryMatch(type, ref arg, out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(Convert.ToInt32(val), Convert.ToInt32(value));
	}

	[DataTestMethod]
	[DataRow(typeof(byte), 1)]
	[DataRow(typeof(sbyte), 2)]
	[DataRow(typeof(short), 3)]
	[DataRow(typeof(ushort), 4)]
	[DataRow(typeof(int), 5)]
	[DataRow(typeof(uint), 6)]
	[DataRow(typeof(long), 7)]
	[DataRow(typeof(ulong), 8)]
	public void TryMatch8Test(Type type, object val)
	{
		// Arrange
		(byte, sbyte, short, ushort, int, uint, long, ulong) arg = (1, 2, 3, 4, 5, 6, 7, 8);

		// Act
		var result = TupleHelper.TryMatch(type, ref arg, out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(Convert.ToInt32(val), Convert.ToInt32(value));
	}

	[DataTestMethod]
	[DataRow(typeof(byte), 1)]
	[DataRow(typeof(sbyte), 2)]
	[DataRow(typeof(short), 3)]
	public void TryMatchNestedTest(Type type, object val)
	{
		// Arrange
		((byte, sbyte), short) arg = ((1, 2), 3);

		// Act
		var result = TupleHelper.TryMatch(type, ref arg, out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(Convert.ToInt32(val), Convert.ToInt32(value));
	}

	[TestMethod]
	public void TryBuild1Test()
	{
		// Arrange
		Expression arg = Expression.Parameter(typeof(byte));

		// Act
		var result = TupleHelper.TryBuild<byte>(typeof(byte), arg);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(typeof(byte), arg.Type);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	public void TryBuild2Test(Type type)
	{
		// Arrange
		Expression arg = Expression.Parameter(typeof((byte, sbyte)));

		// Act
		var result = TupleHelper.TryBuild<(byte, sbyte)>(type, arg);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(type, result.Type);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	public void TryBuild3Test(Type type)
	{
		// Arrange
		Expression arg = Expression.Parameter(typeof((byte, sbyte, short)));

		// Act
		var result = TupleHelper.TryBuild<(byte, sbyte, short)>(type, arg);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(type, result.Type);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	[DataRow(typeof(ushort))]
	public void TryBuild4Test(Type type)
	{
		// Arrange
		Expression arg = Expression.Parameter(typeof((byte, sbyte, short, ushort)));

		// Act
		var result = TupleHelper.TryBuild<(byte, sbyte, short, ushort)>(type, arg);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(type, result.Type);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	[DataRow(typeof(ushort))]
	[DataRow(typeof(int))]
	public void TryBuild5Test(Type type)
	{
		// Arrange
		Expression arg = Expression.Parameter(typeof((byte, sbyte, short, ushort, int)));

		// Act
		var result = TupleHelper.TryBuild<(byte, sbyte, short, ushort, int)>(type, arg);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(type, result.Type);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	[DataRow(typeof(ushort))]
	[DataRow(typeof(int))]
	[DataRow(typeof(uint))]
	public void TryBuild6Test(Type type)
	{
		// Arrange
		Expression arg = Expression.Parameter(typeof((byte, sbyte, short, ushort, int, uint)));

		// Act
		var result = TupleHelper.TryBuild<(byte, sbyte, short, ushort, int, uint)>(type, arg);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(type, result.Type);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	[DataRow(typeof(ushort))]
	[DataRow(typeof(int))]
	[DataRow(typeof(uint))]
	[DataRow(typeof(long))]
	public void TryBuild7Test(Type type)
	{
		// Arrange
		Expression arg = Expression.Parameter(typeof((byte, sbyte, short, ushort, int, uint, long)));

		// Act
		var result = TupleHelper.TryBuild<(byte, sbyte, short, ushort, int, uint, long)>(type, arg);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(type, result.Type);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	[DataRow(typeof(ushort))]
	[DataRow(typeof(int))]
	[DataRow(typeof(uint))]
	[DataRow(typeof(long))]
	[DataRow(typeof(ulong))]
	public void TryBuild8Test(Type type)
	{
		// Arrange
		Expression arg = Expression.Parameter(typeof((byte, sbyte, short, ushort, int, uint, long, ulong)));

		// Act
		var result = TupleHelper.TryBuild<(byte, sbyte, short, ushort, int, uint, long, ulong)>(type, arg);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(type, result.Type);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	[DataRow(typeof(ushort))]
	[DataRow(typeof(int))]
	[DataRow(typeof(uint))]
	[DataRow(typeof(long))]
	[DataRow(typeof(ulong))]
	[DataRow(typeof(string))]
	public void TryBuild9Test(Type type)
	{
		// Arrange
		Expression arg = Expression.Parameter(typeof((byte, sbyte, short, ushort, int, uint, long, ulong, string)));

		// Act
		var result = TupleHelper.TryBuild<(byte, sbyte, short, ushort, int, uint, long, ulong, string)>(type, arg);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(type, result.Type);
	}

	[DataTestMethod]
	[DataRow(typeof(byte))]
	[DataRow(typeof(sbyte))]
	[DataRow(typeof(short))]
	public void TryBuildNestedTest(Type type)
	{
		// Arrange
		Expression arg = Expression.Parameter(typeof(((byte, sbyte), short)));

		// Act
		var result = TupleHelper.TryBuild<((byte, sbyte), short)>(type, arg);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(type, result.Type);
	}
}