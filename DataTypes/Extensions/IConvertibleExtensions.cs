// Copyright © 2019-2026 Sergii Artemenko
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

namespace Xtate.DataTypes.Extensions;

internal static class IConvertibleExtensions
{
	extension<T>(T convertible) where T : IConvertible
	{
		public TypeCode GetTypeCode() => convertible.GetTypeCode();

		public bool ToBoolean(IFormatProvider? provider) => convertible.ToBoolean(provider);

		public byte ToByte(IFormatProvider? provider) => convertible.ToByte(provider);

		public char ToChar(IFormatProvider? provider) => convertible.ToChar(provider);

		public DateTime ToDateTime(IFormatProvider? provider) => convertible.ToDateTime(provider);

		public decimal ToDecimal(IFormatProvider? provider) => convertible.ToDecimal(provider);

		public double ToDouble(IFormatProvider? provider) => convertible.ToDouble(provider);

		public short ToInt16(IFormatProvider? provider) => convertible.ToInt16(provider);

		public int ToInt32(IFormatProvider? provider) => convertible.ToInt32(provider);

		public long ToInt64(IFormatProvider? provider) => convertible.ToInt64(provider);

		public sbyte ToSByte(IFormatProvider? provider) => convertible.ToSByte(provider);

		public float ToSingle(IFormatProvider? provider) => convertible.ToSingle(provider);

		public ushort ToUInt16(IFormatProvider? provider) => convertible.ToUInt16(provider);

		public uint ToUInt32(IFormatProvider? provider) => convertible.ToUInt32(provider);

		public ulong ToUInt64(IFormatProvider? provider) => convertible.ToUInt64(provider);

		public object ToType(Type conversionType, IFormatProvider? provider) => convertible.ToType(conversionType, provider);
	}
}