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

namespace Xtate.Core;

/// <summary>
///     Used as partner interface in Decorator pattern by chaining instances into ancestors chain.
///     It is used to lookup implemented interface all the way of decorators chain. See
///     <see cref="AncestorProviderExtensions.As{T}" /> and <see cref="AncestorProviderExtensions.Is{T}(object?)" />"/>
/// </summary>
public interface IAncestorProvider
{
	/// <summary>
	///     Return reference to ancestor instance when Decorator patter is implemented or <see langword="null" /> if current
	///     instance has no ancestor
	/// </summary>
	object? Ancestor { get; }
}