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

#if !NETSTANDARD2_1 && !NETCOREAPP2_1_OR_GREATER

using Xtate;

// ReSharper disable once CheckNamespace
namespace System;

internal static class SpanFormattablePolyfills
{
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
}

#endif