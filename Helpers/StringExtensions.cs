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

using System.Buffers;

namespace Xtate.Core;

public static class StringExtensions
{
	/// <summary>
	///     Returns string where leading and trailing whitespace characters are removed and sequence of whitespace characters
	///     replaced to single space character.
	///     If source string does not expect any normalization then instance of original string will be returned.
	/// </summary>
	/// <param name="str">String to normalize whitespaces</param>
	/// <returns>Normalized string</returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static string NormalizeSpaces(this string str)
	{
		Infra.Requires(str);

		if (str.Length == 0) return string.Empty;

		if (str.Length <= 32768)
		{
			Span<char> buf = stackalloc char[str.Length];

			return RemoveSpaces(str, buf);
		}

		var array = ArrayPool<char>.Shared.Rent(str.Length);

		try
		{
			return RemoveSpaces(str, array);
		}
		finally
		{
			ArrayPool<char>.Shared.Return(array, clearArray: true);
		}
	}

	private static string RemoveSpaces(string str, Span<char> buf)
	{
		var isInWhiteSpace = true;
		var addSpace = false;
		var normalized = false;
		var count = 0;

		foreach (var ch in str)
		{
			if (char.IsWhiteSpace(ch))
			{
				if (ch != ' ')
				{
					normalized = true;
				}

				if (!isInWhiteSpace)
				{
					isInWhiteSpace = true;
					addSpace = true;
				}

				continue;
			}

			if (isInWhiteSpace)
			{
				isInWhiteSpace = false;

				if (addSpace)
				{
					buf[count ++] = ' ';
					addSpace = false;
				}
			}

			buf[count ++] = ch;
		}

		return str.Length == count && !normalized ? str : buf[..count].ToString();
	}

	public static string Concat(this string str0,
								string str1,
								string str2,
								string str3,
								string str4)
	{
		var totalLength = str0.Length + str1.Length + str2.Length + str3.Length + str4.Length;

#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER

		return string.Create(totalLength, (str0, str1, str2, str3, str4), Factory);

		void Factory(Span<char> span, (string str0, string str1, string str2, string str3, string str4) state)
		{
			var (s0, s1, s2, s3, s4) = state;

			s0.AsSpan().CopyTo(span);
			span = span[s0.Length..];
			s1.AsSpan().CopyTo(span);
			span = span[s1.Length..];
			s2.AsSpan().CopyTo(span);
			span = span[s2.Length..];
			s3.AsSpan().CopyTo(span);
			span = span[s3.Length..];
			s4.AsSpan().CopyTo(span);
		}

#else
		return string.Concat(str0, str1, str2, str3, str4);

#endif
	}
}