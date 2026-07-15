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
using Xtate.DataTypes.Extensions;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class IConvertibleExtensionsCoverageTest
{
	[TestMethod]
	public void ExtensionMethodsForwardEveryConvertibleOperation()
	{
		var value = new ExplicitConvertible(65);
		var provider = CultureInfo.InvariantCulture;

		Assert.AreEqual(TypeCode.Int32, value.GetTypeCode());
		Assert.IsTrue(value.ToBoolean(provider));
		Assert.AreEqual(expected: 65, value.ToByte(provider));
		Assert.AreEqual(expected: 'A', value.ToChar(provider));
		Assert.AreEqual(new DateTime(ticks: 65), value.ToDateTime(provider));
		Assert.AreEqual(expected: 65M, value.ToDecimal(provider));
		Assert.AreEqual(expected: 65D, value.ToDouble(provider));
		Assert.AreEqual(expected: 65, value.ToInt16(provider));
		Assert.AreEqual(expected: 65, value.ToInt32(provider));
		Assert.AreEqual(expected: 65L, value.ToInt64(provider));
		Assert.AreEqual(expected: 65, value.ToSByte(provider));
		Assert.AreEqual(expected: 65F, value.ToSingle(provider));
		Assert.AreEqual(expected: 65, value.ToUInt16(provider));
		Assert.AreEqual(expected: 65U, value.ToUInt32(provider));
		Assert.AreEqual(expected: 65UL, value.ToUInt64(provider));
		Assert.AreEqual(expected: "65", value.ToType(typeof(string), provider));
	}

	private readonly struct ExplicitConvertible(int value) : IConvertible
	{
		TypeCode IConvertible.GetTypeCode() => TypeCode.Int32;

		bool IConvertible.ToBoolean(IFormatProvider? provider) => value != 0;

		byte IConvertible.ToByte(IFormatProvider? provider) => (byte) value;

		char IConvertible.ToChar(IFormatProvider? provider) => (char) value;

		DateTime IConvertible.ToDateTime(IFormatProvider? provider) => new(ticks: value);

		decimal IConvertible.ToDecimal(IFormatProvider? provider) => value;

		double IConvertible.ToDouble(IFormatProvider? provider) => value;

		short IConvertible.ToInt16(IFormatProvider? provider) => (short) value;

		int IConvertible.ToInt32(IFormatProvider? provider) => value;

		long IConvertible.ToInt64(IFormatProvider? provider) => value;

		sbyte IConvertible.ToSByte(IFormatProvider? provider) => (sbyte) value;

		float IConvertible.ToSingle(IFormatProvider? provider) => value;

		string IConvertible.ToString(IFormatProvider? provider) => value.ToString(provider);

		object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => Convert.ChangeType(value, conversionType, provider);

		ushort IConvertible.ToUInt16(IFormatProvider? provider) => (ushort) value;

		uint IConvertible.ToUInt32(IFormatProvider? provider) => (uint) value;

		ulong IConvertible.ToUInt64(IFormatProvider? provider) => (ulong) value;
	}
}
