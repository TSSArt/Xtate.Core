#region Copyright © 2019-2021 Sergii Artemenko

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

namespace Xtate.Core
{
	internal static class ConvertibleExtensions
	{
		public static TypeCode GetTypeCode<T>(this T convertible) where T : IConvertible                                            => convertible.GetTypeCode();
		public static bool     ToBoolean<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                   => convertible.ToBoolean(provider);
		public static byte     ToByte<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                      => convertible.ToByte(provider);
		public static char     ToChar<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                      => convertible.ToChar(provider);
		public static DateTime ToDateTime<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                  => convertible.ToDateTime(provider);
		public static decimal  ToDecimal<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                   => convertible.ToDecimal(provider);
		public static double   ToDouble<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                    => convertible.ToDouble(provider);
		public static short    ToInt16<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                     => convertible.ToInt16(provider);
		public static int      ToInt32<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                     => convertible.ToInt32(provider);
		public static long     ToInt64<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                     => convertible.ToInt64(provider);
		public static sbyte    ToSByte<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                     => convertible.ToSByte(provider);
		public static float    ToSingle<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                    => convertible.ToSingle(provider);
		public static ushort   ToUInt16<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                    => convertible.ToUInt16(provider);
		public static uint     ToUInt32<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                    => convertible.ToUInt32(provider);
		public static ulong    ToUInt64<T>(this T convertible, IFormatProvider? provider) where T : IConvertible                    => convertible.ToUInt64(provider);
		public static object   ToType<T>(this T convertible, Type conversionType, IFormatProvider? provider) where T : IConvertible => convertible.ToType(conversionType, provider);
	}
}