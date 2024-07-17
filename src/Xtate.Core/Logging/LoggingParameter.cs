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

public readonly struct LoggingParameter(string name, object? value, string? format = default) : IFormattable
{
	public string  Name      { get; } = name;
	public object? Value     { get; } = value;
	public string? Format    { get; } = format;
	public string? Namespace { get; init; }


#region Interface IFormattable

	public string ToString(string? format, IFormatProvider? formatProvider)
	{
		if (string.IsNullOrEmpty(Name))
		{
			return ValueToString(formatProvider);
		}

		return string.IsNullOrEmpty(Namespace)
			? Name + @":" + ValueToString(formatProvider)
			: Namespace + @"::" + Name + @":" + ValueToString(formatProvider);
	}

#endregion

	public string FullName() => string.IsNullOrEmpty(Namespace) ? Name : Namespace + @"::" + Name;
	
	public string ValueToString(IFormatProvider? formatProvider)
	{
		if (Format is not null)
		{
			if (Value is IFormattable formattable)
			{
				return formattable.ToString(Format, formatProvider);
			}
		}
		else if (formatProvider is not null)
		{
			if (Value is IConvertible convertible)
			{
				return convertible.ToString(formatProvider);
			}
		}

		return Value?.ToString() ?? string.Empty;
	}

	public override string ToString() => ToString(format: default, formatProvider: default);
}