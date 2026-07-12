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

using Xtate.StateMachine.Internal;

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class SegmentedNameCoverageTest
{
	[TestMethod]
	public void EqualsHandlesDefaultEmptyEqualAndDifferentSegments()
	{
		ImmutableArray<string> defaultSegments = default;
		var empty = ImmutableArray<string>.Empty;
		var first = ImmutableArray.Create(item1: "one", item2: "two");
		var same = ImmutableArray.Create(item1: "one", item2: "two");
		var differentLength = ImmutableArray.Create("one");
		var differentValue = ImmutableArray.Create(item1: "one", item2: "three");

		Assert.IsTrue(SegmentedName.Equals(defaultSegments, defaultSegments));
		Assert.IsFalse(SegmentedName.Equals(defaultSegments, empty));
		Assert.IsFalse(SegmentedName.Equals(first, differentLength));
		Assert.IsFalse(SegmentedName.Equals(first, differentValue));
		Assert.IsTrue(SegmentedName.Equals(first, same));
		Assert.AreEqual(SegmentedName.GetHashCode(first), SegmentedName.GetHashCode(same));
	}

	[TestMethod]
	public void ToStringCoversDefaultEmptySmallAndLongSegmentLists()
	{
		ImmutableArray<string> defaultSegments = default;

		Assert.IsNull(SegmentedName.ToString(defaultSegments, separator: "."));
		Assert.AreEqual(string.Empty, SegmentedName.ToString(ImmutableArray<string>.Empty, separator: "."));
		Assert.AreEqual(expected: "one", SegmentedName.ToString(ImmutableArray.Create("one"), separator: "."));
		Assert.AreEqual(expected: "one.two", SegmentedName.ToString(ImmutableArray.Create(item1: "one", item2: "two"), separator: "."));
		Assert.AreEqual(expected: "one.two.three", SegmentedName.ToString(ImmutableArray.Create(item1: "one", item2: "two", item3: "three"), separator: "."));
		Assert.AreEqual(expected: "one.two..four", SegmentedName.ToString(ImmutableArray.Create(item1: "one", item2: "two", item3: null, item4: "four"), separator: "."));
	}

	[TestMethod]
	public void TryFormatWritesSpanFormattableAndStringSegments()
	{
		var destination = new char[32];

		Assert.IsTrue(SegmentedName.TryFormat(ImmutableArray.Create<object?>(item1: "one", item2: 23, item3: null, item4: "four"), separator: ".", destination, out var charsWritten));
		Assert.AreEqual(expected: "one.23..four", new string(destination, startIndex: 0, charsWritten));

		Assert.IsTrue(SegmentedName.TryFormat(ImmutableArray<string>.Empty, separator: ".", destination, out charsWritten));
		Assert.AreEqual(expected: 0, charsWritten);
	}

	[TestMethod]
	public void TryFormatReturnsFalseWhenDestinationIsTooSmall()
	{
		var destination = new char[4];

		Assert.IsFalse(SegmentedName.TryFormat(ImmutableArray.Create(item1: "one", item2: "two"), separator: ".", destination, out var charsWritten));
		Assert.AreEqual(expected: 4, charsWritten);
	}
}