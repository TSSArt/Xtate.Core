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

		private Identifier(string value) : base(value) { }

	#region Interface IEquatable<IIdentifier>

		public bool Equals(IIdentifier? other) => other is Identifier identifier && SameTypeEquals(identifier);

	#endregion

		public static explicit operator Identifier(string value) => FromString(value);

		public static Identifier FromString(string value)
		{
			if (string.IsNullOrEmpty(value)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(value));

			foreach (var ch in value)
			{
				if (char.IsWhiteSpace(ch))
				{
					throw new ArgumentException(Resources.Exception_IdentifierCannotContainsWhitespace, nameof(value));
				}
			}

			return new Identifier(value);
		}

		public static bool TryCreate(string? value,
									 [NotNullWhen(true)] [MaybeNullWhen(false)]
									 out Identifier? identifier)
		{
			if (string.IsNullOrEmpty(value))
			{
				identifier = default;

				return false;
			}

			foreach (var ch in value)
			{
				if (char.IsWhiteSpace(ch))
				{
					identifier = default;

					return false;
				}
			}

			identifier = new Identifier(value);

			return true;
		}

		protected override string GenerateId() => IdGenerator.NewId(GetHashCode());

		public static IIdentifier New() => new Identifier();
	}
}