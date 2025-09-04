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

using System.Diagnostics;

namespace Xtate.Core;

[DebuggerDisplay("IsCancellationRequested = {IsCancellationRequested}")]
public readonly struct DisposeToken(CancellationToken cancellationToken) : IEquatable<DisposeToken>
{
    public CancellationToken Token => cancellationToken;

    public bool IsCancellationRequested => Token.IsCancellationRequested;

#region Interface IEquatable<DisposeToken>

    public bool Equals(DisposeToken other) => Token.Equals(other.Token);

#endregion

    public void ThrowIfCancellationRequested() => Token.ThrowIfCancellationRequested();

    public static implicit operator CancellationToken(DisposeToken disposeToken) => disposeToken.Token;

    public override bool Equals([NotNullWhen(true)] object? other) => other is DisposeToken dToken && Equals(dToken);

    public override int GetHashCode() => Token.GetHashCode();

    public static bool operator ==(DisposeToken left, DisposeToken right) => left.Equals(right);

    public static bool operator !=(DisposeToken left, DisposeToken right) => !left.Equals(right);
}