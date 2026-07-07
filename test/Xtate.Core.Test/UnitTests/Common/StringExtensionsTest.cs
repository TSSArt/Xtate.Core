// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate;

namespace Xtate.Test;

[TestClass]
public class StringExtensionsTest
{
	[TestMethod]
	public void NormalizeSpaces_WithEmptyString_ShouldReturnEmptyString()
	{
		// Arrange
		const string input = "";

		// Act
		var result = input.NormalizeSpaces();

		// Assert
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void NormalizeSpaces_WithSingleSpace_ShouldReturnSingleSpace()
	{
		// Arrange
		const string input = " ";

		// Act
		var result = input.NormalizeSpaces();

		// Assert
		Assert.AreEqual("", result);
	}

	[TestMethod]
	public void NormalizeSpaces_WithNoSpaces_ShouldReturnSameString()
	{
		// Arrange
		const string input = "hello";

		// Act
		var result = input.NormalizeSpaces();

		// Assert
		Assert.AreEqual(input, result);
		Assert.AreSame(input, result);
	}

	[TestMethod]
	public void NormalizeSpaces_WithLeadingSpaces_ShouldRemoveLeadingSpaces()
	{
		// Arrange
		const string input = "   hello";

		// Act
		var result = input.NormalizeSpaces();

		// Assert
		Assert.AreEqual("hello", result);
	}

	[TestMethod]
	public void NormalizeSpaces_WithTrailingSpaces_ShouldRemoveTrailingSpaces()
	{
		// Arrange
		const string input = "hello   ";

		// Act
		var result = input.NormalizeSpaces();

		// Assert
		Assert.AreEqual("hello", result);
	}

	[TestMethod]
	public void NormalizeSpaces_WithLeadingAndTrailingSpaces_ShouldRemoveBoth()
	{
		// Arrange
		const string input = "   hello world   ";

		// Act
		var result = input.NormalizeSpaces();

		// Assert
		Assert.AreEqual("hello world", result);
	}

	[TestMethod]
	public void NormalizeSpaces_WithMultipleSpacesBetweenWords_ShouldReduceToSingleSpace()
	{
		// Arrange
		const string input = "hello    world";

		// Act
		var result = input.NormalizeSpaces();

		// Assert
		Assert.AreEqual("hello world", result);
	}

	[TestMethod]
	public void NormalizeSpaces_WithMixedWhitespace_ShouldNormalize()
	{
		// Arrange
		const string input = "hello\t\tworld\n\nfoo";

		// Act
		var result = input.NormalizeSpaces();

		// Assert
		Assert.AreEqual("hello world foo", result);
	}

	[TestMethod]
	public void NormalizeSpaces_WithSpacesOnlyString_ShouldReturnSingleSpace()
	{
		// Arrange
		const string input = "     ";

		// Act
		var result = input.NormalizeSpaces();

		// Assert
		Assert.AreEqual("", result);
	}

	[TestMethod]
	public void NormalizeSpaces_ComplexString_ShouldNormalizeCorrectly()
	{
		// Arrange
		const string input = "  The   quick\t\tbrown\nfox   jumps  ";

		// Act
		var result = input.NormalizeSpaces();

		// Assert
		Assert.AreEqual("The quick brown fox jumps", result);
	}

	[TestMethod]
	public void NormalizeSpaces_StringWithNoNormalization_ShouldReturnSameInstance()
	{
		// Arrange
		const string input = "hello world";

		// Act
		var result = input.NormalizeSpaces();

		// Assert
		Assert.AreSame(input, result);
	}
}
