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

#if !NET6_0_OR_GREATER
namespace Xtate.Core;

public static class SortedSetExtensions
{
	public static bool TryGetValue<T>(this SortedSet<T> sortedSet, T equalValue, [MaybeNullWhen(false)] out T actualValue)
	{
		if (sortedSet is null) throw new ArgumentNullException(nameof(sortedSet));

		if (sortedSet.Contains(equalValue))
		{
			using var enumerator = sortedSet.GetViewBetween(equalValue, equalValue).GetEnumerator();

			enumerator.MoveNext();
			actualValue = enumerator.Current;

			return true;
		}

		actualValue = default;

		return false;
	}
}
#endif