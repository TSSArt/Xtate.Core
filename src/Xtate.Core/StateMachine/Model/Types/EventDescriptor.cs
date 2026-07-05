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

namespace Xtate.StateMachine;

public sealed class EventDescriptor : IEventDescriptor, IEquatable<EventDescriptor>
{
	private EventDescriptor(string value) => Value = value;

#region Interface IEquatable<EventDescriptor>

	public bool Equals(EventDescriptor? other) => other is not null && Value == other.Value;

#endregion

#region Interface IEventDescriptor

	public string Value { get; }

#endregion

	public override string ToString() => Value;

	public static explicit operator EventDescriptor(string value) => new(value);

	public static EventDescriptor FromString(string value) => new(value);

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is EventDescriptor other && Equals(other));

	public override int GetHashCode() => Value.GetHashCode();
}