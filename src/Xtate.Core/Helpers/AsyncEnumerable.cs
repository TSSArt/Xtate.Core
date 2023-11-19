#region Copyright © 2019-2022 Sergii Artemenko

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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Core;

[PublicAPI]
internal static class AsyncEnumerable
{
	public static IAsyncEnumerable<T> Empty<T>() => EmptyAsyncEnumerable<T>.Instance;

	public static async ValueTask<ImmutableArray<T>> ToImmutableArrayAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
	{
		var builder = ImmutableArray.CreateBuilder<T>();

		await foreach (var item in asyncEnumerable.ConfigureAwait(false))
		{
			builder.Add(item);
		}

		return builder.ToImmutable();
	}

	private sealed class EmptyAsyncEnumerable<T> : IAsyncEnumerable<T>, IAsyncEnumerator<T>
	{
		public static readonly EmptyAsyncEnumerable<T> Instance = new();

	#region Interface IAsyncDisposable

		ValueTask IAsyncDisposable.DisposeAsync() => default;

	#endregion

	#region Interface IAsyncEnumerable<T>

		IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken token) => this;

	#endregion

	#region Interface IAsyncEnumerator<T>

		ValueTask<bool> IAsyncEnumerator<T>.MoveNextAsync() => default;

		T IAsyncEnumerator<T>.Current => default!;

	#endregion
	}
}