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

using System.ComponentModel;
#if !NET6_0_OR_GREATER && !DEBUG
using System.Text;
#endif

namespace Xtate.Core;

internal static class IdGenerator
{
    public static string NewSendId(int hash) => NewGuidWithHash(hash);

    public static string NewSessionId(int hash) => NewGuidWithHash(hash);

    public static string NewInvokeUniqueId(int hash) => NewGuidWithHash(hash);

    public static string NewId(int hash) => NewGuidWithHash(hash);

#if DEBUG
    public static string NewInvokeId([Localizable(false)] string id, int hash) => id + @"." + hash.ToString(@"X8");

    private static string NewGuidWithHash(int hash) => hash.ToString(@"X8");

#elif NET6_0_OR_GREATER
    public static string NewInvokeId([Localizable(false)] string id, int hash) =>
        string.Create(
            41 + id.Length, (id, hash), static (span, p) =>
                                        {
                                            p.id.AsSpan().CopyTo(span);
                                            span[p.id.Length] = '.';
                                            WriteNewGuidWithHash(span[(p.id.Length + 1)..], p.hash);
                                        });

    private static string NewGuidWithHash(int hash) => string.Create(length: 40, hash, WriteNewGuidWithHash);

    private static void WriteNewGuidWithHash(Span<char> span, int hash)
    {
        Guid.NewGuid().TryFormat(span, out var pos, format: @"N");
        hash.TryFormat(span[pos..], out pos, format: @"x8");
    }

#else
    public static string NewInvokeId([Localizable(false)] string id, int hash) =>
        new StringBuilder(id.Length + 41)
            .Append(id)
            .Append('.')
            .Append(Guid.NewGuid().ToString("N"))
            .Append(hash.ToString(@"x8"))
            .ToString();

    private static string NewGuidWithHash(int hash) =>
        new StringBuilder(40)
            .Append(Guid.NewGuid().ToString("N"))
            .Append(hash.ToString(@"x8"))
            .ToString();

#endif
}