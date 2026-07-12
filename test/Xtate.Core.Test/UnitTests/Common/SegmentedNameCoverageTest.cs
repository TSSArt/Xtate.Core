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
		var first = ImmutableArray.Create("one", "two");
		var same = ImmutableArray.Create("one", "two");
		var differentLength = ImmutableArray.Create("one");
		var differentValue = ImmutableArray.Create("one", "three");

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

		Assert.IsNull(SegmentedName.ToString(defaultSegments, "."));
		Assert.AreEqual(string.Empty, SegmentedName.ToString(ImmutableArray<string>.Empty, "."));
		Assert.AreEqual("one", SegmentedName.ToString(ImmutableArray.Create("one"), "."));
		Assert.AreEqual("one.two", SegmentedName.ToString(ImmutableArray.Create("one", "two"), "."));
		Assert.AreEqual("one.two.three", SegmentedName.ToString(ImmutableArray.Create("one", "two", "three"), "."));
		Assert.AreEqual("one.two..four", SegmentedName.ToString(ImmutableArray.Create("one", "two", null, "four"), "."));
	}

	[TestMethod]
	public void TryFormatWritesSpanFormattableAndStringSegments()
	{
		var destination = new char[32];

		Assert.IsTrue(SegmentedName.TryFormat(ImmutableArray.Create<object?>("one", 23, null, "four"), ".", destination, out var charsWritten));
		Assert.AreEqual("one.23..four", new string(destination, 0, charsWritten));

		Assert.IsTrue(SegmentedName.TryFormat(ImmutableArray<string>.Empty, ".", destination, out charsWritten));
		Assert.AreEqual(0, charsWritten);
	}

	[TestMethod]
	public void TryFormatReturnsFalseWhenDestinationIsTooSmall()
	{
		var destination = new char[4];

		Assert.IsFalse(SegmentedName.TryFormat(ImmutableArray.Create("one", "two"), ".", destination, out var charsWritten));
		Assert.AreEqual(4, charsWritten);
	}
}
