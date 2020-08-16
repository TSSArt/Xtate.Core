#region Copyright © 2019-2020 Sergii Artemenko

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

#endregion

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Xtate
{
	[Serializable]
	public abstract class LazyId : ILazyValue, IObject
	{
		private string? _id;

		protected LazyId() { }

		protected LazyId(string id) => _id = id ?? throw new ArgumentNullException(nameof(id));

		public string Value
		{
			get
			{
				if (_id is { } id)
				{
					return id;
				}

				var newId = GenerateId();

				Debug.Assert(newId is { } && TryGetHashFromId(newId, out var hash) && hash == base.GetHashCode());

				id = Interlocked.CompareExchange(ref _id, newId, comparand: null) ?? newId;

				return id;
			}
		}

	#region Interface ILazyValue

		DataModelValue ILazyValue.Value => new DataModelValue(Value);

	#endregion

	#region Interface IObject

		object IObject.ToObject() => Value;

	#endregion

		public static implicit operator DataModelValue(LazyId? lazyId) => new DataModelValue(lazyId);

		public DataModelValue ToDataModelValue() => this;

		protected abstract string GenerateId();

		[SuppressMessage(category: "ReSharper", checkId: "NonReadonlyMemberInGetHashCode", Justification = "_id used as designed")]
		[SuppressMessage(category: "ReSharper", checkId: "BaseObjectGetHashCodeCallInGetHashCode", Justification = "base.GetHashCode() used as designed")]
		public override int GetHashCode() => _id is { } id ? TryGetHashFromId(id, out var hash) ? hash : id.GetHashCode() : base.GetHashCode();

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

		private static bool TryHexToInt32(ReadOnlySpan<char> span, out int val)
		{
			val = 0;

			foreach (var ch in span)
			{
				val <<= 4;

				if (ch >= '0' && ch <= '9')
				{
					val |= ch - '0';
				}
				else if (ch >= 'a' && ch <= 'f')
				{
					val |= ch - 'a' + 10;
				}
				else
				{
					return false;
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

		public static bool operator ==(LazyId? left, LazyId? right) => Equals(left, right);

		public static bool operator !=(LazyId? left, LazyId? right) => !Equals(left, right);
	}
}