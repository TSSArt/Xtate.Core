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

internal static class SegmentedName
{
	public static bool Equals<T>(ImmutableArray<T> segments1, ImmutableArray<T> segments2)
	{
		if (segments1 == segments2)
		{
			return true;
		}

		if (segments1.IsDefault || segments2.IsDefault)
		{
			return false;
		}

		if (segments1.Length != segments2.Length)
		{
			return false;
		}

		for (var i = 0; i < segments1.Length; i ++)
		{
			if (!EqualityComparer<T>.Default.Equals(segments1[i], segments2[i]))
			{
				return false;
			}
		}

		return true;
	}

	public static int GetHashCode<T>(ImmutableArray<T> segments)
	{
		var hashCode = new HashCode();

		foreach (var t in segments)
		{
			hashCode.Add(t);
		}

		return hashCode.ToHashCode();
	}

	public static string? ToString<T>(ImmutableArray<T> segments, string separator) =>
		segments switch
		{
			{ IsDefault: true }      => default,
			{ IsEmpty: true }        => string.Empty,
			[var t]                  => t?.ToString() ?? string.Empty,
			[var t1, var t2]         => string.Concat(t1?.ToString(), separator, t2?.ToString()),
			[var t1, var t2, var t3] => StringExtensions.Concat(t1?.ToString(), separator, t2?.ToString(), separator, t3?.ToString()),
			_                        => string.Join(separator, segments.Select(t => t?.ToString()))
		};

	public static bool TryFormat<T>(ImmutableArray<T> segments,
									string separator,
									Span<char> destination,
									out int charsWritten)
	{
		charsWritten = 0;

		if (segments.IsDefaultOrEmpty)
		{
			return true;
		}

		for (var i = 0; i < segments.Length; i ++)
		{
			if (i > 0 && !separator.TryCopyIncremental(ref destination, ref charsWritten))
			{
				return false;
			}

			if (segments[i] is not { } t)
			{
				continue;
			}

			if (t is ISpanFormattable spanFormattable)
			{
				if (!spanFormattable.TryFormat(destination, out var written, format: default, provider: default))
				{
					return false;
				}

				destination = destination[written..];
				charsWritten += written;
			}
			else if (!t.ToString().TryCopyIncremental(ref destination, ref charsWritten))
			{
				return false;
			}
		}

		return true;
	}
}