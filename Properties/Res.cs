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

[ExcludeFromCodeCoverage]
internal static class Res
{
    /// <summary>
    /// Formats the specified string using the provided argument.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="arg">The argument to format.</param>
    /// <returns>The formatted string.</returns>
    public static string Format(string format, object? arg) => string.Format(Resources.Culture, format, arg);

    /// <summary>
    /// Formats the specified string using the provided arguments.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="arg0">The first argument to format.</param>
    /// <param name="arg1">The second argument to format.</param>
    /// <returns>The formatted string.</returns>
    public static string Format(string format, object? arg0, object? arg1) => string.Format(Resources.Culture, format, arg0, arg1);

    /// <summary>
    /// Formats the specified string using the provided arguments.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="arg0">The first argument to format.</param>
    /// <param name="arg1">The second argument to format.</param>
    /// <param name="arg2">The third argument to format.</param>
    /// <returns>The formatted string.</returns>
    public static string Format(string format, object? arg0, object? arg1, object? arg2) =>
        string.Format(Resources.Culture, format, arg0, arg1, arg2);

    /// <summary>
    /// Formats the specified string using the provided arguments.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to format.</param>
    /// <returns>The formatted string.</returns>
    public static string Format(string format, params object?[] args) => string.Format(Resources.Culture, format, args);
}
