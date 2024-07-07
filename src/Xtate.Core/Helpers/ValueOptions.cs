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

namespace Xtate;

[Flags]
public enum ValueOptions
{
	/// <summary>
	///     Calls <see cref="IDisposable.Dispose" /> or <see cref="IAsyncDisposable.DisposeAsync" /> on value object when
	///     object leaves the cache.
	/// </summary>
	Dispose = 1,

	/// <summary>
	///     Uses <see cref="WeakReference" /> for storing value object. It means object can be collected by GC at any time.
	/// </summary>
	WeakRef = 2,

	/// <summary>
	///     Value is a thread-safe object, therefore it will be stored in Global cache, otherwise it will be stored in Local
	///     cache and will not be available for other Local caches.
	/// </summary>
	ThreadSafe = 4
}