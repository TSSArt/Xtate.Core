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

using Xtate.Persistence.Internal;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class EncodeCoverageTest
{
	[TestMethod]
	public void EncodingLengthIsSelectedFromUtf8StyleLeadingByte()
	{
		Assert.AreEqual(expected: 1, Encode.GetLength(0x00));
		Assert.AreEqual(expected: 1, Encode.GetLength(0x7F));
		Assert.AreEqual(expected: 2, Encode.GetLength(0xC2));
		Assert.AreEqual(expected: 3, Encode.GetLength(0xE0));
		Assert.AreEqual(expected: 4, Encode.GetLength(0xF0));
		Assert.AreEqual(expected: 5, Encode.GetLength(0xF8));
		Assert.AreEqual(expected: 6, Encode.GetLength(0xFC));
		Assert.ThrowsExactly<ArgumentException>(() => Encode.GetLength(0x80));
		Assert.ThrowsExactly<ArgumentException>(() => Encode.GetLength(0xFE));
	}

	[TestMethod]
	public void EncodedLengthUsesAllIntegerBoundaries()
	{
		Assert.AreEqual(expected: 0, Encode.GetEncodedLength(-1));
		Assert.AreEqual(expected: 1, Encode.GetEncodedLength(0x7F));
		Assert.AreEqual(expected: 2, Encode.GetEncodedLength(0x80));
		Assert.AreEqual(expected: 2, Encode.GetEncodedLength(0x7FF));
		Assert.AreEqual(expected: 3, Encode.GetEncodedLength(0x800));
		Assert.AreEqual(expected: 3, Encode.GetEncodedLength(0xFFFF));
		Assert.AreEqual(expected: 4, Encode.GetEncodedLength(0x10000));
		Assert.AreEqual(expected: 4, Encode.GetEncodedLength(0x1FFFFF));
		Assert.AreEqual(expected: 5, Encode.GetEncodedLength(0x200000));
		Assert.AreEqual(expected: 5, Encode.GetEncodedLength(0x3FFFFFF));
		Assert.AreEqual(expected: 6, Encode.GetEncodedLength(0x4000000));
		Assert.AreEqual(expected: 6, Encode.GetEncodedLength(int.MaxValue));
	}

	[TestMethod]
	public void WriteEncodedValueRoundTripsThroughDecodeForAllLengths()
	{
		foreach (var value in new[] { 0, 0x7F, 0x80, 0x7FF, 0x800, 0xFFFF, 0x10000, 0x1FFFFF, 0x200000, 0x3FFFFFF, 0x4000000, int.MaxValue })
		{
			var buffer = new byte[Encode.GetEncodedLength(value)];

			Encode.WriteEncodedValue(buffer, value);

			Assert.AreEqual(buffer.Length, Encode.GetLength(buffer[0]));
			Assert.AreEqual(value, Encode.Decode(buffer), $"Failed round-trip for value {value}.");
		}
	}

	[TestMethod]
	public void DecodeRejectsMalformedOrUnsupportedSpans()
	{
		Assert.ThrowsExactly<ArgumentException>(() => Encode.Decode([]));
		Assert.ThrowsExactly<ArgumentException>(() => Encode.Decode([0xC2, 0x00]));
		Assert.ThrowsExactly<ArgumentException>(() => Encode.Decode([0xE0, 0x80]));
		Assert.ThrowsExactly<ArgumentException>(() => Encode.Decode([0xF0, 0x80, 0x80]));
		Assert.ThrowsExactly<ArgumentException>(() => Encode.Decode([0xF8, 0x80, 0x80, 0x80]));
		Assert.ThrowsExactly<ArgumentException>(() => Encode.Decode([0xFC, 0x80, 0x80, 0x80, 0x80]));
		Assert.ThrowsExactly<ArgumentException>(() => Encode.Decode([0xFC, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80]));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Encode.WriteEncodedValue(new byte[1], value: -1));
	}
}