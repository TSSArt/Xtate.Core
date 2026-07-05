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

#if !NETSTANDARD2_1 && !NETCOREAPP2_1_OR_GREATER

using System.Runtime.InteropServices;

namespace Xtate;

internal static class BitConverterPolyfills
{
	extension(BitConverter)
	{
		/// <summary>
		///     Converts a Boolean into a span of bytes.
		/// </summary>
		/// <param name="destination">When this method returns, the bytes representing the converted Boolean.</param>
		/// <param name="value">The Boolean to convert.</param>
		/// <returns><see langword="true" /> if the conversion was successful; <see langword="false" /> otherwise.</returns>
		public static bool TryWriteBytes(Span<byte> destination, bool value)
		{
			if (destination.Length < sizeof(byte))
			{
				return false;
			}

			Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value ? (byte) 1 : (byte) 0);

			return true;
		}

		/// <summary>
		///     Converts a character into a span of bytes.
		/// </summary>
		/// <param name="destination">When this method returns, the bytes representing the converted character.</param>
		/// <param name="value">The character to convert.</param>
		/// <returns><see langword="true" /> if the conversion was successful; <see langword="false" /> otherwise.</returns>
		public static bool TryWriteBytes(Span<byte> destination, char value)
		{
			if (destination.Length < sizeof(char))
			{
				return false;
			}

			Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);

			return true;
		}

		/// <summary>
		///     Converts a 16-bit signed integer into a span of bytes.
		/// </summary>
		/// <param name="destination">When this method returns, the bytes representing the converted 16-bit signed integer.</param>
		/// <param name="value">The 16-bit signed integer to convert.</param>
		/// <returns><see langword="true" /> if the conversion was successful; <see langword="false" /> otherwise.</returns>
		public static bool TryWriteBytes(Span<byte> destination, short value)
		{
			if (destination.Length < sizeof(short))
			{
				return false;
			}

			Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);

			return true;
		}

		/// <summary>
		///     Converts a 32-bit signed integer into a span of bytes.
		/// </summary>
		/// <param name="destination">When this method returns, the bytes representing the converted 32-bit signed integer.</param>
		/// <param name="value">The 32-bit signed integer to convert.</param>
		/// <returns><see langword="true" /> if the conversion was successful; <see langword="false" /> otherwise.</returns>
		public static bool TryWriteBytes(Span<byte> destination, int value)
		{
			if (destination.Length < sizeof(int))
			{
				return false;
			}

			Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);

			return true;
		}

		/// <summary>
		///     Converts a 64-bit signed integer into a span of bytes.
		/// </summary>
		/// <param name="destination">When this method returns, the bytes representing the converted 64-bit signed integer.</param>
		/// <param name="value">The 64-bit signed integer to convert.</param>
		/// <returns><see langword="true" /> if the conversion was successful; <see langword="false" /> otherwise.</returns>
		public static bool TryWriteBytes(Span<byte> destination, long value)
		{
			if (destination.Length < sizeof(long))
			{
				return false;
			}

			Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);

			return true;
		}

		/// <summary>
		///     Converts a 16-bit unsigned integer into a span of bytes.
		/// </summary>
		/// <param name="destination">When this method returns, the bytes representing the converted 16-bit unsigned integer.</param>
		/// <param name="value">The 16-bit unsigned integer to convert.</param>
		/// <returns><see langword="true" /> if the conversion was successful; <see langword="false" /> otherwise.</returns>
		[CLSCompliant(false)]
		public static bool TryWriteBytes(Span<byte> destination, ushort value)
		{
			if (destination.Length < sizeof(ushort))
			{
				return false;
			}

			Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);

			return true;
		}

		/// <summary>
		///     Converts a 32-bit unsigned integer into a span of bytes.
		/// </summary>
		/// <param name="destination">When this method returns, the bytes representing the converted 32-bit unsigned integer.</param>
		/// <param name="value">The 32-bit unsigned integer to convert.</param>
		/// <returns><see langword="true" /> if the conversion was successful; <see langword="false" /> otherwise.</returns>
		[CLSCompliant(false)]
		public static bool TryWriteBytes(Span<byte> destination, uint value)
		{
			if (destination.Length < sizeof(uint))
			{
				return false;
			}

			Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);

			return true;
		}

		/// <summary>
		///     Converts a 64-bit unsigned integer into a span of bytes.
		/// </summary>
		/// <param name="destination">When this method returns, the bytes representing the converted 64-bit unsigned integer.</param>
		/// <param name="value">The 64-bit unsigned integer to convert.</param>
		/// <returns><see langword="true" /> if the conversion was successful; <see langword="false" /> otherwise.</returns>
		[CLSCompliant(false)]
		public static bool TryWriteBytes(Span<byte> destination, ulong value)
		{
			if (destination.Length < sizeof(ulong))
			{
				return false;
			}

			Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);

			return true;
		}

		/// <summary>
		///     Converts a single-precision floating-point value into a span of bytes.
		/// </summary>
		/// <param name="destination">
		///     When this method returns, the bytes representing the converted single-precision
		///     floating-point value.
		/// </param>
		/// <param name="value">The single-precision floating-point value to convert.</param>
		/// <returns><see langword="true" /> if the conversion was successful; <see langword="false" /> otherwise.</returns>
		public static bool TryWriteBytes(Span<byte> destination, float value)
		{
			if (destination.Length < sizeof(float))
			{
				return false;
			}

			Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);

			return true;
		}

		/// <summary>
		///     Converts a double-precision floating-point value into a span of bytes.
		/// </summary>
		/// <param name="destination">
		///     When this method returns, the bytes representing the converted double-precision
		///     floating-point value.
		/// </param>
		/// <param name="value">The double-precision floating-point value to convert.</param>
		/// <returns><see langword="true" /> if the conversion was successful; <see langword="false" /> otherwise.</returns>
		public static bool TryWriteBytes(Span<byte> destination, double value)
		{
			if (destination.Length < sizeof(double))
			{
				return false;
			}

			Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);

			return true;
		}
	}
}

#endif