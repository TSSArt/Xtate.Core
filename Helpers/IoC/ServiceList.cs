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

using Xtate.IoC;

namespace Xtate.Core;

public class ServiceList<T> : IReadOnlyList<T>, IAsyncInitialization
{
    private readonly Task _initTask;

    private ImmutableArray<T> _array;

    public ServiceList(IAsyncEnumerable<T> asyncEnumerable) => _initTask = Initialize(asyncEnumerable);

#region Interface IAsyncInitialization

    Task IAsyncInitialization.Initialization => _initTask;

#endregion

#region Interface IEnumerable

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_array).GetEnumerator();

#endregion

#region Interface IEnumerable<T>

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();

#endregion

#region Interface IReadOnlyCollection<T>

    public int Count => _array.Length;

#endregion

#region Interface IReadOnlyList<T>

    public T this[int index] => _array[index];

#endregion

    public ImmutableArray<T>.Enumerator GetEnumerator() => _array.GetEnumerator();

    private async Task Initialize(IAsyncEnumerable<T> asyncEnumerable) => _array = await asyncEnumerable.ToImmutableArrayAsync().ConfigureAwait(false);
}