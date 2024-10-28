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


/// <summary>
/// This class is designed to allow access to the <see cref="Token"/> property without raising an <see cref="ObjectDisposedException"/>.
/// It allows safe access to the <see cref="Token"/> even when it is in a disposed state. On Dispose, it will cancel the token if it has not already been canceled.
/// </summary>
public class DisposingToken : CancellationTokenSource
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisposingToken"/> class.
    /// </summary>
    public DisposingToken() => Token = base.Token;

    /// <summary>
    /// Gets the <see cref="CancellationToken"/> associated with this <see cref="DisposingToken"/>.
    /// </summary>
    public new CancellationToken Token { get; }

	/// <summary>
	/// Cancels the associated <see cref="CancellationToken"/> and releases the unmanaged resources used by the <see cref="DisposingToken"/> and optionally releases the managed resources.
	/// </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>	
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
