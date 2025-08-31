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

namespace Xtate.Core;
	
/// <summary>
/// Provides a fast dynamic conversion between two generic types.
/// </summary>
/// <typeparam name="TFrom">The source type to convert from.</typeparam>
/// <typeparam name="TTo">The target type to convert to.</typeparam>
internal static class ConvertHelper<TFrom, TTo>
{
    /// <summary>
    /// A cached delegate that performs the conversion from <typeparamref name="TFrom"/> to <typeparamref name="TTo"/>.
    /// </summary>
    private static readonly Func<TFrom, TTo> Converter = CreateConverter();

    /// <summary>
    /// Converts a value of type <typeparamref name="TFrom"/> to <typeparamref name="TTo"/>.
    /// </summary>
    /// <param name="from">The value to convert.</param>
    /// <returns>The converted value of type <typeparamref name="TTo"/>.</returns>
    public static TTo Convert(TFrom from) => Converter(from);

    /// <summary>
    /// Creates a compiled delegate that converts from <typeparamref name="TFrom"/> to <typeparamref name="TTo"/> using expression trees.
    /// </summary>
    /// <returns>A delegate that performs the conversion.</returns>
    private static Func<TFrom, TTo> CreateConverter()
    {
        var arg = Expression.Parameter(typeof(TFrom));
        var body = Expression.Convert(arg, typeof(TTo));

        return Expression.Lambda<Func<TFrom, TTo>>(body, arg).Compile();
    }
}