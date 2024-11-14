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

public readonly struct CombinedToken : IDisposable
{
	private readonly CancellationTokenSource? _cancellationTokenSource;

	public CombinedToken(CancellationToken token1, CancellationToken token2)
	{
		if (token1.CanBeCanceled && token2.CanBeCanceled && token1 != token2)
		{
			_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token1, token2);
			Token = _cancellationTokenSource.Token;
		}
		else
		{
			Token = token1.CanBeCanceled ? token1 : token2;
		}
	}

	public CancellationToken Token { get; }

#region Interface IDisposable

	public void Dispose() => _cancellationTokenSource?.Dispose();

#endregion
}