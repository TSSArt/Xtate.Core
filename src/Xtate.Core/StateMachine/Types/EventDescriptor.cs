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

public sealed class EventDescriptor : IEventDescriptor, IEquatable<EventDescriptor>
{
	private static readonly char[] Dot = ['.'];

	private readonly IIdentifier[] _parts;

	private EventDescriptor(string value)
	{
		Infra.RequiresNonEmptyString(value);

		Value = value;

		var parts = value.Split(Dot, StringSplitOptions.None);
		var length = parts.Length;

		if (length > 0 && parts[length - 1] == @"*")
		{
			length --;
		}

		_parts = new IIdentifier[length];

		for (var i = 0; i < _parts.Length; i ++)
		{
			_parts[i] = (Identifier) parts[i];
		}
	}

#region Interface IEquatable<EventDescriptor>

	public bool Equals(EventDescriptor? other) => other is not null && Value == other.Value;

#endregion

#region Interface IEventDescriptor

	public bool IsEventMatch(IEvent evt)
	{
		if (evt.Name.Count < _parts.Length)
		{
			return false;
		}

		for (var i = 0; i < _parts.Length; i ++)
		{
			if (!evt.Name[i].Equals(_parts[i]))
			{
				return false;
			}
		}

		return true;
	}

	public string Value { get; }

#endregion

	public override string ToString() => Value;

	public static explicit operator EventDescriptor(string value) => new(value);

	public static EventDescriptor FromString(string value) => new(value);

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is EventDescriptor other && Equals(other));

	public override int GetHashCode() => Value.GetHashCode();
}