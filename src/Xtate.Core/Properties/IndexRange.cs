#region Copyright © 2019-2021 Sergii Artemenko

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

#if NET461 || NETSTANDARD2_0
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Xtate;

namespace System
{
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	internal readonly struct Index : IEquatable<Index>
	{
		private readonly int _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Index(int value, bool fromEnd = false)
		{
			if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));

			_value = fromEnd ? ~value : value;
		}

		private Index(int value) => _value = value;

		public static Index Start => new(0);

		public static Index End => new(~0);

		public int Value => _value < 0 ? ~_value : _value;

		public bool IsFromEnd => _value < 0;

	#region Interface IEquatable<Index>

		public bool Equals(Index other) => _value == other._value;

	#endregion

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Index FromStart(int value)
		{
			if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));

			return new Index(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Index FromEnd(int value)
		{
			if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));

			return new Index(~value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetOffset(int length) => IsFromEnd ? _value + length + 1 : _value;

		public override bool Equals(object? value) => value is Index index && _value == index._value;

		public override int GetHashCode() => _value;

		public static implicit operator Index(int value) => FromStart(value);

		[SuppressMessage(category: "Globalization", checkId: "CA1305:Specify IFormatProvider", Justification = "Does not required culture formatting")]
		public override string ToString() => IsFromEnd ? @"^" + (uint) Value : ((uint) Value).ToString();
	}

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	internal readonly struct Range : IEquatable<Range>
	{
		public Range(Index start, Index end)
		{
			Start = start;
			End = end;
		}

		public Index Start { get; }

		public Index End { get; }

		public static Range All => new(Index.Start, Index.End);

	#region Interface IEquatable<Range>

		public bool Equals(Range other) => other.Start.Equals(Start) && other.End.Equals(End);

	#endregion

		public override bool Equals(object? value) => value is Range r && r.Start.Equals(Start) && r.End.Equals(End);

		public override int GetHashCode() => Start.GetHashCode() * 31 + End.GetHashCode();

		public override string ToString() => Start + @".." + End;

		public static Range StartAt(Index start) => new(start, Index.End);

		public static Range EndAt(Index end) => new(Index.Start, end);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int Offset, int Length) GetOffsetAndLength(int length)
		{
			var start = Start.IsFromEnd ? length - Start.Value : Start.Value;
			var end = End.IsFromEnd ? length - End.Value : End.Value;

			if ((uint) end > (uint) length || (uint) start > (uint) end)
			{
				throw new ArgumentOutOfRangeException(nameof(length));
			}

			return (start, end - start);
		}
	}
}

#endif