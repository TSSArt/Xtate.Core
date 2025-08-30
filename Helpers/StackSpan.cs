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

using System.Buffers;
using System.Runtime.InteropServices;

namespace Xtate.Core;

/// <summary>
/// Usage pattern:
/// using var ss = new StackSpan&lt;T&gt;(length);
/// var span = ss ? ss : stackalloc T[ss];
/// </summary>
/// <typeparam name="T"></typeparam>
public ref struct StackSpan<T> : IDisposable where T : struct
{
	private const int SafeStackAllocationSizeInBytes = 4096;

	private readonly int _length;

	private T[]? _array;

	[MustDisposeResource]
	public StackSpan(int length)
	{
		_length = length;

		if (length > MaxLengthInStack)
		{
			_array = ArrayPool<T>.Shared.Rent(length);
		}
	}

	public static int MaxLengthInStack { get; } = SafeStackAllocationSizeInBytes / Marshal.SizeOf<T>();

#region Interface IDisposable

	public void Dispose()
	{
		if (_array is not { } array)
		{
			return;
		}

		_array = null;

		ArrayPool<T>.Shared.Return(array);
	}

#endregion
	
	private static T[] UsagePatternException() => throw new InvalidOperationException(@"Incorrect usage pattern");

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator int(StackSpan<T> stackSpan) => stackSpan._length;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Span<T>(StackSpan<T> stackSpan) => new(stackSpan._array ?? UsagePatternException(), start: 0, stackSpan._length);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !(StackSpan<T> stackSpan) => stackSpan._array is null;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator true(StackSpan<T> stackSpan) => stackSpan._array is not null;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator false(StackSpan<T> stackSpan) => stackSpan._array is null;
}