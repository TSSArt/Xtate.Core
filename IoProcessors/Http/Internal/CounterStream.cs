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

using System.IO;
using Xtate.ResourceLoaders.Internal;

namespace Xtate.IoProcessors.Http.Internal;

internal class CounterStream(Stream stream) : DelegatedStream(stream)
{
	private int PreReadInt(int count)
	{
		var newCount = count;

		PreRead(ref newCount);

		if (count == 0)
		{
			return newCount == 0 ? 0 : throw new InvalidOperationException(Resources.Exception_PreReadModifiedTheCountFrom0ToANonZeroValue);
		}

		return newCount >= 1 && newCount <= count
			? newCount
			: throw new InvalidOperationException(Res.Format(Resources.Exception_PreReadModifiedTheCountToAnInvalidValueValidRangeIs1ToCount, newCount, count));
	}

	private int PostReadInt(int count)
	{
		PostRead(count);

		return count;
	}

	protected virtual void PreRead(ref int count) { }

	protected virtual void PostRead(int count) { }

	protected virtual void PreWrite(int count) { }

	protected virtual void PostWrite(int count) { }

	public override int Read(byte[] buffer, int offset, int count) => PostReadInt(base.Read(buffer, offset, PreReadInt(count)));

	public override async Task<int> ReadAsync(byte[] buffer,
											  int offset,
											  int count,
											  CancellationToken cancellationToken) =>
		PostReadInt(await base.ReadAsync(buffer, offset, PreReadInt(count), cancellationToken).ConfigureAwait(false));

	public override int ReadByte()
	{
		PreReadInt(1);

		var result = base.ReadByte();

		PostReadInt(result >= 0 ? 1 : 0);

		return result;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		PreWrite(count);

		base.Write(buffer, offset, count);

		PostWrite(count);
	}

	public override async Task WriteAsync(byte[] buffer,
										  int offset,
										  int count,
										  CancellationToken cancellationToken)
	{
		PreWrite(count);

		await base.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);

		PostWrite(count);
	}

	public override void WriteByte(byte value)
	{
		PreWrite(1);

		base.WriteByte(value);

		PostWrite(1);
	}

	public override int Read(Span<byte> buffer) => PostReadInt(base.Read(buffer[..PreReadInt(buffer.Length)]));

	public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new()) =>
		PostReadInt(await base.ReadAsync(buffer[..PreReadInt(buffer.Length)], cancellationToken).ConfigureAwait(false));

	public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new())
	{
		PreWrite(buffer.Length);

		await base.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);

		PostWrite(buffer.Length);
	}

	public override void Write(ReadOnlySpan<byte> buffer)
	{
		PreWrite(buffer.Length);

		base.Write(buffer);

		PostWrite(buffer.Length);
	}
}