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

namespace Xtate.Core;

public readonly struct LoggingParameter(string name, object? value, string? format = default) : ISpanFormattable
{
	private const string NsDelimiter = @"::";
	private const string NmDelimiter = @":";

	public        string Name { get; } = name;

	public object? Value { get; } = value;

	public string? Format { get; } = format;

	public string? Namespace { get; init; }

#region Interface IFormattable

	public string ToString(string? format, IFormatProvider? formatProvider)
	{
		var strValue = ValueToString(formatProvider);

		if (string.IsNullOrEmpty(Name))
		{
			return strValue;
		}

		return string.IsNullOrEmpty(Namespace)
			? Name + NmDelimiter + ValueToString(formatProvider)
			: Namespace.Concat(NsDelimiter, Name, NmDelimiter, ValueToString(formatProvider));
	}

#endregion

#region Interface ISpanFormattable

	public bool TryFormat(Span<char> destination,
						  out int charsWritten,
						  ReadOnlySpan<char> format,
						  IFormatProvider? formatProvider)
	{
		charsWritten = 0;

		if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Namespace))
		{
			if (!Namespace.TryCopyIncremental(ref destination, ref charsWritten))
			{
				return false;
			}

			if (!NsDelimiter.TryCopyIncremental(ref destination, ref charsWritten))
			{
				return false;
			}
		}

		if (!string.IsNullOrEmpty(Name))
		{
			if (!Name.TryCopyIncremental(ref destination, ref charsWritten))
			{
				return false;
			}

			if (!NmDelimiter.TryCopyIncremental(ref destination, ref charsWritten))
			{
				return false;
			}
		}

		if (Value is ISpanFormattable spanFormattable)
		{
			if (!spanFormattable.TryFormat(destination, out var valCharsWritten, Format.AsSpan(), formatProvider))
			{
				return false;
			}

			charsWritten += valCharsWritten;
		}
		else
		{
			if (!ValueToString(formatProvider).TryCopyIncremental(ref destination, ref charsWritten))
			{
				return false;
			}
		}

		return true;
	}

#endregion

	public string FullName() => string.IsNullOrEmpty(Namespace) ? Name : Namespace + NsDelimiter + Name;

	public string ValueToString(IFormatProvider? formatProvider)
	{
		if (Value is IFormattable formattable)
		{
			return formattable.ToString(Format, formatProvider);
		}

		if (Value is IConvertible convertible)
		{
			return convertible.ToString(formatProvider);
		}

		return Value?.ToString() ?? string.Empty;
	}

	public override string ToString() => ToString(format: default, formatProvider: default);
}