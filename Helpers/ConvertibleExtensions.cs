using System;

namespace Xtate
{
	internal static class ConvertibleExtensions
	{
		public static TypeCode GetTypeCode<T>(this T convertible) where T : IConvertible                                           => convertible.GetTypeCode();
		public static bool     ToBoolean<T>(this T convertible, IFormatProvider provider) where T : IConvertible                   => convertible.ToBoolean(provider);
		public static byte     ToByte<T>(this T convertible, IFormatProvider provider) where T : IConvertible                      => convertible.ToByte(provider);
		public static char     ToChar<T>(this T convertible, IFormatProvider provider) where T : IConvertible                      => convertible.ToChar(provider);
		public static DateTime ToDateTime<T>(this T convertible, IFormatProvider provider) where T : IConvertible                  => convertible.ToDateTime(provider);
		public static decimal  ToDecimal<T>(this T convertible, IFormatProvider provider) where T : IConvertible                   => convertible.ToDecimal(provider);
		public static double   ToDouble<T>(this T convertible, IFormatProvider provider) where T : IConvertible                    => convertible.ToDouble(provider);
		public static short    ToInt16<T>(this T convertible, IFormatProvider provider) where T : IConvertible                     => convertible.ToInt16(provider);
		public static int      ToInt32<T>(this T convertible, IFormatProvider provider) where T : IConvertible                     => convertible.ToInt32(provider);
		public static long     ToInt64<T>(this T convertible, IFormatProvider provider) where T : IConvertible                     => convertible.ToInt64(provider);
		public static sbyte    ToSByte<T>(this T convertible, IFormatProvider provider) where T : IConvertible                     => convertible.ToSByte(provider);
		public static float    ToSingle<T>(this T convertible, IFormatProvider provider) where T : IConvertible                    => convertible.ToSingle(provider);
		public static ushort   ToUInt16<T>(this T convertible, IFormatProvider provider) where T : IConvertible                    => convertible.ToUInt16(provider);
		public static uint     ToUInt32<T>(this T convertible, IFormatProvider provider) where T : IConvertible                    => convertible.ToUInt32(provider);
		public static ulong    ToUInt64<T>(this T convertible, IFormatProvider provider) where T : IConvertible                    => convertible.ToUInt64(provider);
		public static object   ToType<T>(this T convertible, Type conversionType, IFormatProvider provider) where T : IConvertible => convertible.ToType(conversionType, provider);
	}
}