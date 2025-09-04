// Copyright © 2019-2025 Sergii Artemenko
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

namespace Xtate.Core.Test;

[TestClass]
public class StringExtensionsTest
{
    [TestMethod]
    public void NormalizeSpaces_ShouldRaiseArgumentException_IfInputIsNull()
    {
        // assert
        Assert.ThrowsException<ArgumentNullException>(() => StringExtensions.NormalizeSpaces(null!));
    }

    [TestMethod]
    [DataRow(1, "", "")]
    [DataRow(2, "a", "a")]
    [DataRow(3, "ab", "ab")]
    [DataRow(4, " a", "a")]
    [DataRow(5, "  a", "a")]
    [DataRow(6, "a ", "a")]
    [DataRow(7, "a  ", "a")]
    [DataRow(8, "a b", "a b")]
    [DataRow(9, "a  b", "a b")]
    [DataRow(10, " a b ", "a b")]
    [DataRow(11, " \t\r\n\f\va", "a")]
    [DataRow(12, "a \t\r\n\f\v", "a")]
    [DataRow(13, "a \t\r\n\f\vb", "a b")]
    [DataRow(14, "a\tb", "a b")]
    public void NormalizeSpaces_ShouldRemoveNotExpectedWhiteSpaceCharacters(int num, string value, string expected)
    {
        // arrange
        _ = num;

        // act
        var normalized = value.NormalizeSpaces();

        // assert
        Assert.AreEqual(expected, normalized);
    }

    [TestMethod]
    public void NormalizeSpaces_LongStackAllocString_ShouldNotFail()
    {
        // arrange
        var expected = new string(c: '-', count: 30000);
        var value = expected + ' ';

        // act
        var normalized = value.NormalizeSpaces();

        // assert
        Assert.AreEqual(expected, normalized);
    }

    [TestMethod]
    public void NormalizeSpaces_LongArrayPoolString_ShouldNotFail()
    {
        // arrange
        var expected = new string(c: '-', count: 60000);
        var value = expected + ' ';

        // act
        var normalized = value.NormalizeSpaces();

        // assert
        Assert.AreEqual(expected, normalized);
    }
}