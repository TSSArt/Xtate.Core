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

using System.ComponentModel;

namespace Xtate;

public sealed class Identifier : LazyId, IIdentifier, IEquatable<IIdentifier>
{
	private Identifier() { }

	private Identifier(string value) : base(value) { }

	public static IEqualityComparer<IIdentifier> EqualityComparer { get; } = new IdentifierEqualityComparer();

#region Interface IEquatable<IIdentifier>

	public bool Equals(IIdentifier? other) => other is Identifier identifier && FastEqualsNoTypeCheck(identifier);

#endregion

	public static explicit operator Identifier([Localizable(false)] string value) => FromString(value);

	public static Identifier FromString([Localizable(false)] string value)
	{
		foreach (var ch in value)
		{
			if (char.IsWhiteSpace(ch))
			{
				throw new ArgumentException(Resources.Exception_IdentifierCannotContainsWhitespace, nameof(value));
			}
		}

		return new Identifier(value);
	}

	public static bool TryCreate([Localizable(false)] string? value, [NotNullWhen(true)] out Identifier? identifier)
	{
		if (Infra.IsNullOrEmpty(value))
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

	public static string? ToString(ImmutableArray<IIdentifier> identifiers)
	{
		if (identifiers.IsDefaultOrEmpty)
		{
			return null;
		}

		if (identifiers.Length == 1)
		{
			return identifiers[0].Value;
		}

		return string.Join(separator: @" ", identifiers.Select(d => d.Value));
	}

	private class IdentifierEqualityComparer : IEqualityComparer<IIdentifier>
	{
	#region Interface IEqualityComparer<IIdentifier>

		public bool Equals(IIdentifier? x, IIdentifier? y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (x is null || y is null)
			{
				return false;
			}

			return x.As<IEquatable<IIdentifier>>().Equals(y.As<IEquatable<IIdentifier>>());
		}

		public int GetHashCode(IIdentifier obj) => obj.As<IEquatable<IIdentifier>>().GetHashCode();

	#endregion
	}
}