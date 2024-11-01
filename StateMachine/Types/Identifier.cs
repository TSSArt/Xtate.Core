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

public sealed class Identifier : LazyId, IIdentifier, IEquatable<Identifier>
{
	private Identifier() { }

	private Identifier(string value) : base(value) { }

#region Interface IEquatable<Identifier>

	public bool Equals(Identifier? other) => FastEqualsNoTypeCheck(other);

#endregion

	public override bool Equals(object? obj) => obj is Identifier identifier ? Equals(identifier) : obj?.Equals(this) == true;

	public override int GetHashCode() => base.GetHashCode();

	public static explicit operator Identifier([Localizable(false)] string value) => FromString(value);

	public static Identifier FromString([Localizable(false)] string value)
	{
		foreach (var ch in value)
		{
			if (char.IsWhiteSpace(ch))
			{
				throw new ArgumentException(Resources.Exception_IdentifierCannotContainWhitespace, nameof(value));
			}
		}

		return new Identifier(value);
	}

	public static bool TryCreate([Localizable(false)] string? value, [NotNullWhen(true)] out Identifier? identifier)
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