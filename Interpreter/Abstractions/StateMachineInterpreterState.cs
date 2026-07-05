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

namespace Xtate.Interpreter;

public class StateMachineInterpreterState
{
	private readonly string _displayName;

	protected StateMachineInterpreterState(string displayName) => _displayName = displayName;

	public static StateMachineInterpreterState Initializing { get; } = new(nameof(Initializing));

	public static StateMachineInterpreterState Accepted { get; } = new(nameof(Accepted));

	public static StateMachineInterpreterState Started { get; } = new(nameof(Started));

	public static StateMachineInterpreterState Completed { get; } = new(nameof(Completed));

	public static StateMachineInterpreterState Waiting { get; } = new(nameof(Waiting));

	public static StateMachineInterpreterState Proceed { get; } = new(nameof(Proceed));

	public static StateMachineInterpreterState Destroying { get; } = new(nameof(Destroying));

	public static StateMachineInterpreterState Terminated { get; } = new(nameof(Terminated));

	public sealed override bool Equals(object? obj) => ReferenceEquals(this, obj);

	public sealed override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

	public static bool operator ==(StateMachineInterpreterState? left, StateMachineInterpreterState? right) => ReferenceEquals(left, right);

	public static bool operator !=(StateMachineInterpreterState? left, StateMachineInterpreterState? right) => !ReferenceEquals(left, right);

	public override string ToString() => _displayName;
}