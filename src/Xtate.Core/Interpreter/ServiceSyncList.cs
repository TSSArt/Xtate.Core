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

public class ServiceSyncList<T>(IEnumerable<T> asyncEnumerable) : IReadOnlyList<T>
{
	private readonly ImmutableArray<T> _array = [..asyncEnumerable];

#region Interface IEnumerable

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _array).GetEnumerator();

#endregion

#region Interface IEnumerable<T>

	IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>) _array).GetEnumerator();

#endregion

#region Interface IReadOnlyCollection<T>

	public int Count => _array.Length;

#endregion

#region Interface IReadOnlyList<T>

	public T this[int index] => _array[index];

#endregion

	public ImmutableArray<T>.Enumerator GetEnumerator() => _array.GetEnumerator();
}