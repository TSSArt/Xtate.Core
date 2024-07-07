// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate.Core;

public class DisposingToken : CancellationTokenSource
{
	public DisposingToken() => Token = base.Token;

	public new CancellationToken Token { get; }

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			try
			{
				if (!IsCancellationRequested)
				{
					Cancel();
				}
			}
			catch (ObjectDisposedException)
			{
				// Ignore
			}
		}

		base.Dispose(disposing);
	}
}