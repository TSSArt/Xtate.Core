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

[Serializable]
public abstract class LazyId : ILazyValue, IObject
{
	private string? _id;

	protected LazyId() { }

	protected LazyId(string id) => _id = id ?? throw new ArgumentNullException(nameof(id));

	public virtual string Value
	{
		get
		{
			if (_id is { } id)
			{
				return id;
			}

			var newId = GenerateId();

			Debug.Assert(TryGetHashFromId(newId, out var hash) && hash == base.GetHashCode());

			id = Interlocked.CompareExchange(ref _id, newId, comparand: null) ?? newId;

			return id;
		}
	}

#region Interface ILazyValue

	DataModelValue ILazyValue.Value => new(Value);

#endregion

#region Interface IObject

	object IObject.ToObject() => Value;

#endregion

	public static implicit operator DataModelValue(LazyId? lazyId) => new(lazyId);

	public DataModelValue ToDataModelValue() => this;

	protected abstract string GenerateId();

	[SuppressMessage(category: "ReSharper", checkId: "BaseObjectGetHashCodeCallInGetHashCode")]
	[SuppressMessage(category: "ReSharper", checkId: "NonReadonlyMemberInGetHashCode")]
	public override int GetHashCode() => _id is { } id ? TryGetHashFromId(id, out var hash) ? hash : id.GetHashCode() : base.GetHashCode();

	public override string ToString() => Value;

	protected static bool TryGetHashFromId(string id, out int hash)
	{
		if (id is null) throw new ArgumentNullException(nameof(id));

		var start = id.Length - 8;
		if (start >= 0 && TryHexToInt32(id.AsSpan(start), out hash))
		{
			return true;
		}

		hash = 0;
		return false;
	}

	private static bool TryHexToInt32(ReadOnlySpan<char> span, out int value)
	{
		value = 0;

		foreach (var ch in span)
		{
			value <<= 4;

			switch (ch)
			{
				case >= '0' and <= '9':
					value |= ch - '0';
					break;
				case >= 'a' and <= 'f':
					value |= ch - 'a' + 10;
					break;
				default: return false;
			}
		}

		return true;
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(this, obj))
		{
			return true;
		}

		if (obj is null || _id is null || GetType() != obj.GetType())
		{
			return false;
		}

		return _id == ((LazyId) obj)._id;
	}

	protected bool FastEqualsNoTypeCheck(LazyId? lazyId)
	{
		if (ReferenceEquals(this, lazyId))
		{
			return true;
		}

		if (lazyId is null || _id is null)
		{
			return false;
		}

		return _id == lazyId._id;
	}

	public static bool operator ==(LazyId? left, LazyId? right) => Equals(left, right);

	public static bool operator !=(LazyId? left, LazyId? right) => !Equals(left, right);
}