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

namespace Xtate.Persistence;

internal static class Encode
{
	public static int GetLength(byte value)
	{
		if ((value & 0x80) == 0x00) return 1;
		if ((value & 0xE0) == 0xC0) return 2;
		if ((value & 0xF0) == 0xE0) return 3;
		if ((value & 0xF8) == 0xF0) return 4;
		if ((value & 0xFC) == 0xF8) return 5;
		if ((value & 0xFE) == 0xFC) return 6;

		throw new ArgumentException(Resources.Exception_IncorrectEncoding, nameof(value));
	}

	public static int Decode(ReadOnlySpan<byte> span)
	{
		switch (span.Length)
		{
			case 1: return span[0] & 0x7F;

			case 2 when (span[0] & 0xE0) == 0xC0 && (span[1] & 0xC0) == 0x80:
				return ((span[0] & 0x1F) << 6) + (span[1] & 0x3F);

			case 3 when (span[0] & 0xF0) == 0xE0 && (span[1] & 0xC0) == 0x80 && (span[2] & 0xC0) == 0x80:
				return ((span[0] & 0x0F) << 12) + ((span[1] & 0x3F) << 6) + (span[2] & 0x3F);

			case 4 when (span[0] & 0xF8) == 0xF0 && (span[1] & 0xC0) == 0x80 && (span[2] & 0xC0) == 0x80 && (span[3] & 0xC0) == 0x80:
				return ((span[0] & 0x07) << 18) + ((span[1] & 0x3F) << 12) + ((span[2] & 0x3F) << 6) + (span[3] & 0x3F);

			case 5 when (span[0] & 0xFC) == 0xF8 && (span[1] & 0xC0) == 0x80 && (span[2] & 0xC0) == 0x80 && (span[3] & 0xC0) == 0x80 && (span[4] & 0xC0) == 0x80:
				return ((span[0] & 0x03) << 24) + ((span[1] & 0x3F) << 18) + ((span[2] & 0x3F) << 12) + ((span[3] & 0x3F) << 6) + (span[4] & 0x3F);

			case 6 when (span[0] & 0xFE) == 0xFC && (span[1] & 0xC0) == 0x80 && (span[2] & 0xC0) == 0x80 && (span[3] & 0xC0) == 0x80 && (span[4] & 0xC0) == 0x80 && (span[5] & 0xC0) == 0x80:
				return ((span[0] & 0x01) << 30) + ((span[1] & 0x3F) << 24) + ((span[2] & 0x3F) << 18) + ((span[3] & 0x3F) << 12) + ((span[4] & 0x3F) << 6) + (span[5] & 0x3F);
		}

		throw new ArgumentException(Resources.Exception_IncorrectEncoding, nameof(span));
	}

	public static int GetEncodedLength(int value)
	{
		if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));

		if (value <= 0x7F) return 1;
		if (value <= 0x7FF) return 2;
		if (value <= 0xFFFF) return 3;
		if (value <= 0x1FFFFF) return 4;
		if (value <= 0x3FFFFFF) return 5;
		return 6;
	}

	private static ulong GetEncodedValue(int value)
	{
		if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));

		var uValue = (ulong) value;

		if (uValue <= 0x7F)
		{
			return uValue;
		}

		if (uValue <= 0x7FF)
		{
			return 0x80C0U + (uValue >> 6) +
				   ((uValue & 0x3FU) << 8);
		}

		if (uValue <= 0xFFFF)
		{
			return 0x8080E0U + (uValue >> 12) +
				   ((uValue & 0xFC0U) << 2) +
				   ((uValue & 0x3FU) << 16);
		}

		if (uValue <= 0x1FFFFF)
		{
			return 0x808080F0U + (uValue >> 18) +
				   ((uValue & 0x3F000U) >> 4) +
				   ((uValue & 0xFC0U) << 10) +
				   ((uValue & 0x3FU) << 24);
		}

		if (uValue <= 0x3FFFFFF)
		{
			return 0x80808080F8UL + (uValue >> 24) +
				   ((uValue & 0xFC0000U) >> 10) +
				   ((uValue & 0x3F000U) << 4) +
				   ((uValue & 0xFC0U) << 18) +
				   ((uValue & 0x3FUL) << 32);
		}

		return 0x8080808080FCUL + (uValue >> 30) +
			   ((uValue & 0x3F000000) >> 16) +
			   ((uValue & 0xFC0000) >> 2) +
			   ((uValue & 0x3F000) << 12) +
			   ((uValue & 0xFC0UL) << 26) +
			   ((uValue & 0x3FUL) << 40);
	}

	public static void WriteEncodedValue(Span<byte> span, int value)
	{
		var encodedValue = GetEncodedValue(value);

		for (var i = 0; i < span.Length; i ++)
		{
			span[i] = (byte) encodedValue;

			encodedValue >>= 8;
		}
	}
}