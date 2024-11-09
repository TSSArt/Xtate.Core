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

namespace Xtate.Core;

/// <summary>
///     Represents a Uniform Resource Identifier (URI) and extends the <see cref="Uri" /> class
///     to include the fragment part in equality checks.
/// </summary>
public class FullUri : Uri, IEquatable<FullUri>
{
	/// <summary>
	///     Initializes a new instance of the <see cref="FullUri" /> class with the specified URI string.
	/// </summary>
	/// <param name="uriString">A string that identifies the resource.</param>
	public FullUri(string uriString) : base(uriString, UriKind.RelativeOrAbsolute) { }

	/// <summary>
	///     Initializes a new instance of the <see cref="FullUri" /> class with the specified base URI and relative URI.
	/// </summary>
	/// <param name="baseUri">The base URI.</param>
	/// <param name="relativeUri">The relative URI to add to the base URI.</param>
	public FullUri(Uri baseUri, string relativeUri) : base(baseUri, relativeUri) { }

	/// <summary>
	///     Initializes a new instance of the <see cref="FullUri" /> class with the specified base URI and relative URI.
	/// </summary>
	/// <param name="baseUri">The base URI.</param>
	/// <param name="relativeUri">The relative URI to add to the base URI.</param>
	public FullUri(Uri baseUri, Uri relativeUri) : base(baseUri, relativeUri) { }

	/// <summary>
	///     Gets the fragment part of the URI if it is absolute; otherwise, returns <c>null</c>.
	/// </summary>
	private string? FragmentSafe => IsAbsoluteUri ? Fragment : default;

#region Interface IEquatable<FullUri>

	/// <summary>
	///     Determines whether the specified <see cref="FullUri" /> is equal to the current <see cref="FullUri" />.
	/// </summary>
	/// <param name="other">The <see cref="FullUri" /> to compare with the current <see cref="FullUri" />.</param>
	/// <returns>
	///     <c>true</c> if the specified <see cref="FullUri" /> is equal to the current <see cref="FullUri" />; otherwise,
	///     <c>false</c>.
	/// </returns>
	public bool Equals(FullUri? other) => base.Equals(other) && FragmentSafe == other.FragmentSafe;

#endregion

	/// <summary>
	///     Determines whether the specified object is equal to the current <see cref="FullUri" />.
	/// </summary>
	/// <param name="obj">The object to compare with the current <see cref="FullUri" />.</param>
	/// <returns><c>true</c> if the specified object is equal to the current <see cref="FullUri" />; otherwise, <c>false</c>.</returns>
	public override bool Equals(object? obj) => obj is FullUri fullUri && Equals(fullUri);

	/// <summary>
	///     Serves as the default hash function.
	/// </summary>
	/// <returns>A hash code for the current <see cref="FullUri" />.</returns>
	public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), FragmentSafe);

	/// <summary>
	///     Determines whether two specified instances of <see cref="FullUri" /> are equal.
	/// </summary>
	/// <param name="left">The first <see cref="FullUri" /> to compare.</param>
	/// <param name="right">The second <see cref="FullUri" /> to compare.</param>
	/// <returns><c>true</c> if the two <see cref="FullUri" /> instances are equal; otherwise, <c>false</c>.</returns>
	public static bool operator ==(FullUri? left, FullUri? right) => Equals(left, right);

	/// <summary>
	///     Determines whether two specified instances of <see cref="FullUri" /> are not equal.
	/// </summary>
	/// <param name="left">The first <see cref="FullUri" /> to compare.</param>
	/// <param name="right">The second <see cref="FullUri" /> to compare.</param>
	/// <returns><c>true</c> if the two <see cref="FullUri" /> instances are not equal; otherwise, <c>false</c>.</returns>
	public static bool operator !=(FullUri? left, FullUri? right) => !Equals(left, right);

	/// <summary>
	///     Attempts to create a new instance of the <see cref="FullUri" /> class from the specified URI string.
	/// </summary>
	/// <param name="uriString">The URI string to create the <see cref="FullUri" /> from.</param>
	/// <param name="result">
	///     When this method returns, contains the created <see cref="FullUri" /> if the URI string was valid;
	///     otherwise, <c>null</c>.
	/// </param>
	/// <returns><c>true</c> if the URI string was valid and the <see cref="FullUri" /> was created; otherwise, <c>false</c>.</returns>
	public static bool TryCreate([NotNullWhen(true)] string? uriString, [NotNullWhen(true)] out FullUri? result)
	{
		if (TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
		{
			result = uri.IsAbsoluteUri ? new FullUri(uri, (string) null!) : new FullUri(uri.OriginalString);

			return true;
		}

		result = default;

		return false;
	}
}