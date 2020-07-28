#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test
{
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
		[DataRow(data1: 1, "", "")]
		[DataRow(data1: 2, "a", "a")]
		[DataRow(data1: 3, "ab", "ab")]
		[DataRow(data1: 4, " a", "a")]
		[DataRow(data1: 5, "  a", "a")]
		[DataRow(data1: 6, "a ", "a")]
		[DataRow(data1: 7, "a  ", "a")]
		[DataRow(data1: 8, "a b", "a b")]
		[DataRow(data1: 9, "a  b", "a b")]
		[DataRow(data1: 10, " a b ", "a b")]
		[DataRow(data1: 11, " \t\r\n\f\va", "a")]
		[DataRow(data1: 12, "a \t\r\n\f\v", "a")]
		[DataRow(data1: 13, "a \t\r\n\f\vb", "a b")]
		[DataRow(data1: 14, "a\tb", "a b")]
		public void NormalizeSpaces_ShouldRemoveNotExpectedWhiteSpaceCharacters(int num, string value, string expected)
		{
			// arrange
			var _ = num;

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
}