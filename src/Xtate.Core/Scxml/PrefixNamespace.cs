#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System;

namespace Xtate.Scxml
{
	public readonly struct PrefixNamespace : IEquatable<PrefixNamespace>
	{
		public PrefixNamespace(string prefix, string ns)
		{
			Prefix = prefix;
			Namespace = ns;
		}

		public string Prefix { get; }

		public string Namespace { get; }

	#region Interface IEquatable<PrefixNamespace>

		public bool Equals(PrefixNamespace other) => Prefix == other.Prefix && Namespace == other.Namespace;

	#endregion

		public override bool Equals(object? obj) => obj is PrefixNamespace other && Equals(other);

		public override int GetHashCode() => unchecked((Prefix.GetHashCode() * 397) ^ Namespace.GetHashCode());

		public static bool operator ==(PrefixNamespace left, PrefixNamespace right) => left.Equals(right);

		public static bool operator !=(PrefixNamespace left, PrefixNamespace right) => !left.Equals(right);
	}
}