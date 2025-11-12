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

using System.Runtime.InteropServices;

namespace Xtate.Core;

internal readonly struct RawDecimal(long lo64, long hi64)
{
    public readonly long Hi64 = hi64;

    public readonly long Lo64 = lo64;

    public bool DoNotCache => (Hi64 & -4294967296) != 0;

    public static explicit operator RawDecimal(decimal value)
    {
        ReadOnlySpan<decimal> buf = [value];

        return MemoryMarshal.Cast<decimal, RawDecimal>(buf)[0];
    }

    public static explicit operator decimal(RawDecimal value)
    {
        ReadOnlySpan<RawDecimal> buf = [value];

        return MemoryMarshal.Cast<RawDecimal, decimal>(buf)[0];
    }

    public static void ResetScale(ref decimal value, out byte scale)
    {
        var raw = (RawDecimal)value;

        scale = (byte)(raw.Hi64 >> 16);

        value = (decimal)new RawDecimal(raw.Lo64, raw.Hi64 & -16711681);
    }
}