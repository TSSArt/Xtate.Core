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

using System.Diagnostics;

namespace Xtate.IoProcessors.NamedPipe.Internal;

public class DelayedTries
{
	private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

	private TimeSpan _currentTimeout;

	public required TimeSpan MinDelay
	{
		get;
		init
		{
			Infra.RequiresPositive(value);

			field = value;
			_currentTimeout = value;
		}
	}

	public required TimeSpan MaxDelay
	{
		get;
		init
		{
			Infra.RequiresPositive(value);

			field = value;
		}
	}

	public async ValueTask Delay(CancellationToken token)
	{
		var delay = _currentTimeout - _stopwatch.Elapsed;

		if (delay > TimeSpan.Zero)
		{
			_currentTimeout += _currentTimeout;
			
			if(_currentTimeout > MaxDelay)
			{
				_currentTimeout = MaxDelay;
			}

			await Task.Delay(delay, token).ConfigureAwait(false);
		}

		_currentTimeout = MinDelay;

		_stopwatch.Restart();
	}
}