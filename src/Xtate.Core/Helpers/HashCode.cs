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

#if !NETSTANDARD2_1 && !NETCOREAPP2_1_OR_GREATER
namespace System;

internal struct HashCode
{
	private int _hash;

	public static int Combine<T1>(T1 value1) => value1?.GetHashCode() ?? 0;

	public static int Combine<T1, T2>(T1 value1, T2 value2)
	{
		unchecked
		{
			var hash = value1?.GetHashCode() ?? 0;
			hash = (hash * 397) ^ (value2?.GetHashCode() ?? 0);

			return hash;
		}
	}

	public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
	{
		unchecked
		{
			var hash = value1?.GetHashCode() ?? 0;
			hash = (hash * 397) ^ (value2?.GetHashCode() ?? 0);
			hash = (hash * 397) ^ (value3?.GetHashCode() ?? 0);

			return hash;
		}
	}

	public static int Combine<T1, T2, T3, T4>(T1 value1,
											  T2 value2,
											  T3 value3,
											  T4 value4)
	{
		unchecked
		{
			var hash = value1?.GetHashCode() ?? 0;
			hash = (hash * 397) ^ (value2?.GetHashCode() ?? 0);
			hash = (hash * 397) ^ (value3?.GetHashCode() ?? 0);
			hash = (hash * 397) ^ (value4?.GetHashCode() ?? 0);

			return hash;
		}
	}

	public static int Combine<T1, T2, T3, T4, T5>(T1 value1,
												  T2 value2,
												  T3 value3,
												  T4 value4,
												  T5 value5)
	{
		unchecked
		{
			var hash = value1?.GetHashCode() ?? 0;
			hash = (hash * 397) ^ (value2?.GetHashCode() ?? 0);
			hash = (hash * 397) ^ (value3?.GetHashCode() ?? 0);
			hash = (hash * 397) ^ (value4?.GetHashCode() ?? 0);
			hash = (hash * 397) ^ (value5?.GetHashCode() ?? 0);

			return hash;
		}
	}

	public void Add<T>(T t) => _hash = unchecked((_hash * 397) ^ (t?.GetHashCode() ?? 0));

	public int ToHashCode() => _hash;
}

#endif