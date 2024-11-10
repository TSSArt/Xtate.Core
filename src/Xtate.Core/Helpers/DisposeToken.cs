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

using System.Diagnostics;

namespace Xtate.Core;

[DebuggerDisplay("IsCancellationRequested = {IsCancellationRequested}")]
public readonly struct DisposeToken(CancellationToken cancellationToken) : IEquatable<DisposeToken>
{
	private readonly CancellationToken _token = cancellationToken;

	public static CancellationToken None => default;

	public bool IsCancellationRequested => _token.IsCancellationRequested;

	public bool CanBeCanceled => _token.CanBeCanceled;

	public WaitHandle WaitHandle => _token.WaitHandle;

#region Interface IEquatable<DisposeToken>

	public bool Equals(DisposeToken other) => _token.Equals(other._token);

#endregion

	public static implicit operator CancellationToken(DisposeToken disposeToken) => disposeToken._token;

	public static implicit operator DisposeToken(CancellationToken cancellationToken) => new(cancellationToken);

	public CancellationTokenRegistration Register(Action callback) => _token.Register(callback);

	public CancellationTokenRegistration Register(Action callback, bool useSynchronizationContext) => _token.Register(callback, useSynchronizationContext);

	public CancellationTokenRegistration Register(Action<object?> callback, object? state) => _token.Register(callback, state);

	public CancellationTokenRegistration Register(Action<object?> callback, object? state, bool useSynchronizationContext) => _token.Register(callback, state, useSynchronizationContext);

	public CancellationTokenRegistration UnsafeRegister(Action<object?> callback, object? state) => _token.Register(callback, state);

	public override bool Equals([NotNullWhen(true)] object? other) => (other is DisposeToken dToken && Equals(dToken)) || (other is CancellationToken cToken && Equals(cToken));

	public override int GetHashCode() => _token.GetHashCode();

	public static bool operator ==(DisposeToken left, DisposeToken right) => left.Equals(right);

	public static bool operator !=(DisposeToken left, DisposeToken right) => !(left == right);

	public void ThrowIfCancellationRequested() => _token.ThrowIfCancellationRequested();
}