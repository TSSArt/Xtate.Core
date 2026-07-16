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
using System.Net;

namespace Xtate.IoProcessors.Http.Internal;

internal class ReadLimitStream(Stream stream, long maxReadBytes) : CounterStream(stream)
{
	private long _totalCount;

	protected override void PreRead(ref int count)
	{
		if (count == 0)
		{
			return;
		}

		if (_totalCount >= maxReadBytes)
		{
			throw new HttpRequestProcessException(Res.Format(Resources.Exception_ReadLimitExceeded, _totalCount + count, maxReadBytes)) { StatusCode = HttpStatusCode.RequestEntityTooLarge };
		}

		if (count > maxReadBytes - _totalCount)
		{
			count = (int) (maxReadBytes - _totalCount);
		}
	}

	protected override void PostRead(int count)
	{
		_totalCount += count;
	}
}