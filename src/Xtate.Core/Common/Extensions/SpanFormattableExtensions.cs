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

namespace Xtate;

internal static class SpanFormattableExtensions
{
	extension(string? str)
	{
		public bool TryCopyIncremental(ref Span<char> destination, ref int charsWritten)
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

		public bool TryCopyTo(Span<char> destination, out int charsWritten)
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
	}

	extension(char ch)
	{
		public bool TryCopyIncremental(ref Span<char> destination, ref int charsWritten)
		{
			if (destination.Length > 0)
			{
				destination[0] = ch;

				charsWritten ++;
				destination = destination[1..];

				return true;
			}

			return false;
		}
	}
}