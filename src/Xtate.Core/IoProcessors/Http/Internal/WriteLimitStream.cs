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

namespace Xtate.IoProcessors.Http.Internal;

internal class WriteLimitStream(long maxWriteBytes) : CounterStream(Null)
{
	private long _totalCount;

	protected override void PreWrite(int count)
	{
		if (count > maxWriteBytes - _totalCount)
		{
			throw new IOException($"Write limit exceeded: {_totalCount + count} > {maxWriteBytes} bytes");
		}
	}

	protected override void PostWrite(int count) => _totalCount += count;
}