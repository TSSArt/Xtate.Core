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

namespace Xtate.Core;

[CollectionBuilder(typeof(EventDescriptors), nameof(Create))]
public readonly struct EventDescriptors : IReadOnlyList<IEventDescriptor>, IEquatable<EventDescriptors>, ISpanFormattable
{
    private const string Separator = @" ";

    private readonly ImmutableArray<IEventDescriptor> _eventDescriptors;

    private EventDescriptors(ImmutableArray<IEventDescriptor> eventDescriptors) => _eventDescriptors = eventDescriptors;

    public bool IsDefault => _eventDescriptors.IsDefault;

    public ImmutableArray<IEventDescriptor> Array => _eventDescriptors;

#region Interface IEnumerable

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_eventDescriptors).GetEnumerator();

#endregion

#region Interface IEnumerable<IEventDescriptor>

    IEnumerator<IEventDescriptor> IEnumerable<IEventDescriptor>.GetEnumerator() => ((IEnumerable<IEventDescriptor>)_eventDescriptors).GetEnumerator();

#endregion

#region Interface IEquatable<EventDescriptors>

    public bool Equals(EventDescriptors other) => SegmentedName.Equals(_eventDescriptors, other._eventDescriptors);

#endregion

#region Interface IFormattable

    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

#endregion

#region Interface IReadOnlyCollection<IEventDescriptor>

    public int Count => _eventDescriptors.Length;

#endregion

#region Interface IReadOnlyList<IEventDescriptor>

    public IEventDescriptor this[int index] => _eventDescriptors[index];

#endregion

#region Interface ISpanFormattable

    public bool TryFormat(Span<char> destination,
                          out int charsWritten,
                          ReadOnlySpan<char> format,
                          IFormatProvider? provider) =>
        SegmentedName.TryFormat(_eventDescriptors, Separator, destination, out charsWritten);

#endregion

    public static implicit operator EventDescriptors(ImmutableArray<IEventDescriptor> values) => new(values);

    public static EventDescriptors Create(ReadOnlySpan<IEventDescriptor> values) => new([.. values]);

    public ImmutableArray<IEventDescriptor>.Enumerator GetEnumerator() => _eventDescriptors.GetEnumerator();

    public override bool Equals(object? obj) => obj is EventDescriptors other && Equals(other);

    public override int GetHashCode() => SegmentedName.GetHashCode(_eventDescriptors);

    public override string ToString() => SegmentedName.ToString(_eventDescriptors, Separator);
}