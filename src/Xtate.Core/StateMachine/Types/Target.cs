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

namespace Xtate;

/// <summary>
///     Represents an event name in the state machine.
/// </summary>
/// <remarks>
///     This struct is immutable and provides various functionalities to handle event names,
///     including comparison, formatting, and conversion from strings.
/// </remarks>
[CollectionBuilder(typeof(Target), nameof(Create))]
[Serializable]
public readonly struct Target : IReadOnlyList<IIdentifier>, IEquatable<Target>, ISpanFormattable
{
	private const string Separator = @" ";

	private readonly ImmutableArray<IIdentifier> _targets;

	private Target(ImmutableArray<IIdentifier> targets) => _targets = targets;

	public bool IsDefault => _targets.IsDefault;

	public ImmutableArray<IIdentifier> Array => _targets;

#region Interface IEnumerable

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _targets).GetEnumerator();

#endregion

#region Interface IEnumerable<IIdentifier>

	IEnumerator<IIdentifier> IEnumerable<IIdentifier>.GetEnumerator() => ((IEnumerable<IIdentifier>) _targets).GetEnumerator();

#endregion

#region Interface IEquatable<Target>

	public bool Equals(Target other) => SegmentedName.Equals(_targets, other._targets);

#endregion

#region Interface IFormattable

	public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

#endregion

#region Interface IReadOnlyCollection<IIdentifier>

	public int Count => _targets.Length;

#endregion

#region Interface IReadOnlyList<IIdentifier>

	public IIdentifier this[int index] => _targets[index];

#endregion

#region Interface ISpanFormattable

	public bool TryFormat(Span<char> destination,
						  out int charsWritten,
						  ReadOnlySpan<char> format,
						  IFormatProvider? provider) =>
		SegmentedName.TryFormat(_targets, Separator, destination, out charsWritten);

#endregion

	public static implicit operator Target(ImmutableArray<IIdentifier> values) => new(values);

	public static Target Create(ReadOnlySpan<IIdentifier> values) => new([.. values]);

	public ImmutableArray<IIdentifier>.Enumerator GetEnumerator() => _targets.GetEnumerator();

	public override int GetHashCode() => SegmentedName.GetHashCode(_targets);

	public override string ToString() => SegmentedName.ToString(_targets, Separator);

	public override bool Equals(object? obj) => obj is EventName other && Equals(other);
}