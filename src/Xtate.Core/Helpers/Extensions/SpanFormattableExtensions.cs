// Copyright © 2019-2025 Sergii Artemenko
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

namespace Xtate.Core;

internal static class SpanFormattableExtensions
{
#if !NETSTANDARD2_1 && !NETCOREAPP2_1_OR_GREATER
    public static bool TryFormat(this bool val, Span<char> destination, out int charsWritten) => val.ToString().TryCopyTo(destination, out charsWritten);

    public static bool TryFormat(this long val,
                                 Span<char> destination,
                                 out int charsWritten,
                                 ReadOnlySpan<char> format,
                                 IFormatProvider? formatProvider) =>
        val.ToString(format.ToString(), formatProvider).TryCopyTo(destination, out charsWritten);

    public static bool TryFormat(this double val,
                                 Span<char> destination,
                                 out int charsWritten,
                                 ReadOnlySpan<char> format,
                                 IFormatProvider? formatProvider) =>
        val.ToString(format.ToString(), formatProvider).TryCopyTo(destination, out charsWritten);

    public static bool TryFormat(this decimal val,
                                 Span<char> destination,
                                 out int charsWritten,
                                 ReadOnlySpan<char> format,
                                 IFormatProvider? formatProvider) =>
        val.ToString(format.ToString(), formatProvider).TryCopyTo(destination, out charsWritten);

    public static bool TryFormat(this DateTime val,
                                 Span<char> destination,
                                 out int charsWritten,
                                 ReadOnlySpan<char> format,
                                 IFormatProvider? formatProvider) =>
        val.ToString(format.ToString(), formatProvider).TryCopyTo(destination, out charsWritten);

    public static bool TryFormat(this DateTimeOffset val,
                                 Span<char> destination,
                                 out int charsWritten,
                                 ReadOnlySpan<char> format,
                                 IFormatProvider? formatProvider) =>
        val.ToString(format.ToString(), formatProvider).TryCopyTo(destination, out charsWritten);

#endif

    public static bool TryCopyTo(this string? str, Span<char> destination, out int charsWritten)
    {
        charsWritten = 0;

        if (string.IsNullOrEmpty(str))
        {
            return true;
        }

        if (!str.AsSpan().TryCopyTo(destination))
        {
            return false;
        }

        charsWritten = str.Length;

        return true;
    }

    public static bool TryCopyIncremental(this string? str, ref Span<char> destination, ref int charsWritten)
    {
        if (str is null)
        {
            return true;
        }

        if (str.AsSpan().TryCopyTo(destination))
        {
            var length = str.Length;

            charsWritten += length;
            destination = destination[length..];

            return true;
        }

        return false;
    }

    public static bool TryCopyIncremental(this char ch, ref Span<char> destination, ref int charsWritten)
    {
        charsWritten = destination.Length > 0 ? 1 : 0;

        if (charsWritten == 1)
        {
            destination[0] = ch;

            return true;
        }

        return false;
    }
}