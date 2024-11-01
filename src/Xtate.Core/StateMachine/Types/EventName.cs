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

using System.Buffers;
using System.Xml;

namespace Xtate;

/// <summary>
///     Represents an event name in the state machine.
/// </summary>
/// <remarks>
///     This struct is immutable and provides various functionalities to handle event names,
///     including comparison, formatting, and conversion from strings.
/// </remarks>
[CollectionBuilder(typeof(EventName), nameof(Create))]
[Serializable]
public readonly struct EventName : IReadOnlyList<IIdentifier>, IEquatable<EventName>, ISpanFormattable
{
	private const string Separator = @".";

	private const char Dot = '.';

	private static readonly IIdentifier DoneIdentifier = Identifier.FromString("done");

	private static readonly IIdentifier StateIdentifier = Identifier.FromString("state");

	private static readonly IIdentifier ErrorIdentifier = Identifier.FromString("error");

	private static readonly IIdentifier InvokeIdentifier = Identifier.FromString("invoke");

	private static readonly IIdentifier PlatformIdentifier = Identifier.FromString("platform");

	public static readonly EventName ErrorExecution = [ErrorIdentifier, Identifier.FromString("execution")];

	public static readonly EventName ErrorCommunication = [ErrorIdentifier, Identifier.FromString("communication")];

	public static readonly EventName ErrorPlatform = [ErrorIdentifier, Identifier.FromString("platform")];

	private readonly ImmutableArray<IIdentifier> _parts;

	private EventName(ImmutableArray<IIdentifier> parts) => _parts = parts;

	public bool IsDefault => _parts.IsDefault;

#region Interface IEnumerable

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _parts).GetEnumerator();

#endregion

#region Interface IEnumerable<IIdentifier>

	IEnumerator<IIdentifier> IEnumerable<IIdentifier>.GetEnumerator() => ((IEnumerable<IIdentifier>) _parts).GetEnumerator();

#endregion

#region Interface IEquatable<EventName>

	public bool Equals(EventName other) => SegmentedName.Equals(_parts, other._parts);

#endregion

#region Interface IFormattable

	public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

#endregion

#region Interface IReadOnlyCollection<IIdentifier>

	public int Count => _parts.Length;

#endregion

#region Interface IReadOnlyList<IIdentifier>

	public IIdentifier this[int index] => _parts[index];

#endregion

#region Interface ISpanFormattable

	public bool TryFormat(Span<char> destination,
						  out int charsWritten,
						  ReadOnlySpan<char> format,
						  IFormatProvider? provider) =>
		SegmentedName.TryFormat(_parts, Separator, destination, out charsWritten);

#endregion

	public static EventName Create(ReadOnlySpan<IIdentifier> values) => new([.. values]);

	public ImmutableArray<IIdentifier>.Enumerator GetEnumerator() => _parts.GetEnumerator();

	public override int GetHashCode() => SegmentedName.GetHashCode(_parts);

	public override string ToString() => SegmentedName.ToString(_parts, Separator);

	public override bool Equals(object? obj) => obj is EventName other && Equals(other);

	public static bool operator ==(EventName left, EventName right) => left.Equals(right);

	public static bool operator !=(EventName left, EventName right) => !left.Equals(right);

	public static explicit operator EventName(string name) => FromString(name);

	private static int GetCount(string id)
	{
		var count = 1;
		var pos = 0;

		while ((pos = id.IndexOf(Dot, pos) + 1) > 0)
		{
			count ++;
		}

		return count;
	}

	private static void SetParts(Span<IIdentifier> span, string? id)
	{
		if (id is null)
		{
			return;
		}

		var pos = 0;
		int pos2;
		var index = 0;

		while ((pos2 = id.IndexOf(Dot, pos)) >= 0)
		{
			span[index ++] = (Identifier) id[pos..pos2];

			pos = pos2 + 1;
		}

		span[index] = (Identifier) id[pos..];
	}

	public static EventName FromString(string name)
	{
		if (name is null)
		{
			return default;
		}

		if (name.Length == 0)
		{
			return [];
		}

		var length = GetCount(name);

		var buf = ArrayPool<IIdentifier>.Shared.Rent(length);

		try
		{
			var parts = buf.AsSpan(start: 0, length);
			SetParts(parts, name);

			return [.. parts];
		}
		finally
		{
			ArrayPool<IIdentifier>.Shared.Return(buf, clearArray: true);
		}
	}

	private static EventName GetEventName(IIdentifier id1, IIdentifier id2, string name)
	{
		if (name is null)
		{
			return default;
		}

		var count = GetCount(name);

		var buf = ArrayPool<IIdentifier>.Shared.Rent(2 + count);

		try
		{
			buf[0] = id1;
			buf[1] = id2;

			var parts = buf.AsSpan(start: 2, count);

			SetParts(parts, name);

			return [.. buf.AsSpan(start: 0, 2 + count)];
		}
		finally
		{
			ArrayPool<IIdentifier>.Shared.Return(buf, clearArray: true);
		}
	}

	public bool IsError() => !_parts.IsDefaultOrEmpty && _parts[0].Equals(ErrorIdentifier);

	public static EventName GetErrorPlatform(string suffix) => GetEventName(ErrorIdentifier, PlatformIdentifier, suffix);

	internal static EventName GetDoneStateName(IIdentifier id) => GetEventName(DoneIdentifier, StateIdentifier, id.Value);

	internal static EventName GetDoneInvokeName(InvokeId invokeId) => GetEventName(DoneIdentifier, InvokeIdentifier, invokeId.Value);

	public void WriteTo(XmlWriter writer)
	{
		if (!_parts.IsDefault)
		{
			writer.WriteString(_parts[0].Value);

			for (var i = 1; i < _parts.Length; i ++)
			{
				writer.WriteString(@".");
				writer.WriteString(_parts[i].Value);
			}
		}
	}
}