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

[InstantiatedByIoC]
public class ServiceReadOnlyList<T> : ReadOnlyList<T>, IAsyncInitialization
{
	private readonly Task _initTask;

	public ServiceReadOnlyList(IAsyncEnumerable<T> asyncEnumerable) => _initTask = Initialize(asyncEnumerable);

#region Interface IAsyncInitialization

	Task IAsyncInitialization.Initialization => _initTask;

#endregion
	
	private async Task Initialize(IAsyncEnumerable<T> asyncEnumerable) => Items = await asyncEnumerable.ToImmutableArrayAsync().ConfigureAwait(false);
}
