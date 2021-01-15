#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Diagnostics.CodeAnalysis;
using Xtate.Core;

namespace Xtate
{
	[PublicAPI]
	public sealed class Identifier : LazyId, IIdentifier, IEquatable<IIdentifier>
	{
		private Identifier() { }

		private Identifier(string val) : base(val) { }

	#region Interface IEquatable<IIdentifier>

		public bool Equals(IIdentifier? other) => other is Identifier identifier && SameTypeEquals(identifier);

	#endregion

		public static explicit operator Identifier(string val) => FromString(val);

		public static Identifier FromString(string val)
		{
			if (string.IsNullOrEmpty(val)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(val));

			foreach (var ch in val)
			{
				if (char.IsWhiteSpace(ch))
				{
					throw new ArgumentException(Resources.Exception_IdentifierCannotContainsWhitespace, nameof(val));
				}
			}

			return new Identifier(val);
		}

		public static bool TryCreate(string? val, [NotNullWhen(true)] [MaybeNullWhen(false)]
									 out Identifier? identifier)
		{
			if (string.IsNullOrEmpty(val))
			{
				identifier = null;

				return false;
			}

			foreach (var ch in val)
			{
				if (char.IsWhiteSpace(ch))
				{
					identifier = null;

					return false;
				}
			}

			identifier = new Identifier(val);

			return true;
		}

		protected override string GenerateId() => IdGenerator.NewId(GetHashCode());

		public static IIdentifier New() => new Identifier();
	}
}