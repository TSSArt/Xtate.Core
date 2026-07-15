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

using System.Collections;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.StateMachine;

[TestClass]
public class CollectionTypesCoverageTest
{
	[TestMethod]
	public void EventDescriptorsSupportFormattingEnumerationHashingAndEquality()
	{
		var first = EventDescriptor.FromString("error.*");
		var second = EventDescriptor.FromString("done.invoke");
		EventDescriptors descriptors = ImmutableArray.Create<IEventDescriptor>(first, second);
		var same = EventDescriptors.Create([first, second]);
		var different = EventDescriptors.Create([second, first]);

		Assert.AreEqual(expected: 2, descriptors.Count);
		Assert.AreSame(first, descriptors[0]);
		Assert.AreSame(second, descriptors[1]);
		CollectionAssert.AreEqual(new[] { "error.*", "done.invoke" }, descriptors.Select(descriptor => descriptor.Value).ToArray());
		var directEnumerator = descriptors.GetEnumerator();
		var directValues = new List<string>();

		while (directEnumerator.MoveNext())
		{
			directValues.Add(directEnumerator.Current.Value);
		}

		var nonGenericEnumerator = ((IEnumerable) descriptors).GetEnumerator();
		var nonGenericValues = new List<string>();

		while (nonGenericEnumerator.MoveNext())
		{
			nonGenericValues.Add(((IEventDescriptor) nonGenericEnumerator.Current!).Value);
		}

		CollectionAssert.AreEqual(new[] { "error.*", "done.invoke" }, directValues);
		CollectionAssert.AreEqual(new[] { "error.*", "done.invoke" }, nonGenericValues);
		Assert.AreEqual(expected: "error.* done.invoke", descriptors.ToString());
		Assert.AreEqual(expected: "error.* done.invoke", descriptors.ToString(format: null, formatProvider: null));
		Assert.AreEqual(expected: "error.* done.invoke", typeof(EventDescriptors).GetMethod(nameof(ToString), [typeof(string), typeof(IFormatProvider)])!.Invoke(descriptors, [null, null]));
		Assert.IsTrue(descriptors.Equals(same));
		Assert.IsTrue(descriptors.Equals((object) same));
		Assert.AreEqual(same.GetHashCode(), descriptors.GetHashCode());
		Assert.IsFalse(descriptors.Equals(different));
		Assert.IsFalse(descriptors.Equals("not descriptors"));

		var destination = new char[32];
		Assert.IsTrue(descriptors.TryFormat(destination, out var charsWritten, format: default, provider: null));
		Assert.AreEqual(expected: "error.* done.invoke", new string(destination, startIndex: 0, charsWritten));
		Assert.IsFalse(descriptors.TryFormat(new char[4], out _, format: default, provider: null));

		EventDescriptors empty = ImmutableArray<IEventDescriptor>.Empty;
		Assert.AreEqual(string.Empty, empty.ToString());
		Assert.IsTrue(default(EventDescriptors).IsDefault);
		Assert.AreEqual(string.Empty, typeof(EventDescriptors).GetMethod(nameof(ToString), [typeof(string), typeof(IFormatProvider)])!.Invoke(default(EventDescriptors), [null, null]));
	}

	[TestMethod]
	public void TargetSupportsFormattingEnumerationHashingAndStronglyTypedEquality()
	{
		var first = Identifier.FromString("state1");
		var second = Identifier.FromString("state2");
		Target target = ImmutableArray.Create<IIdentifier>(first, second);
		var same = Target.Create([first, second]);
		var different = Target.Create([second, first]);

		Assert.AreEqual(expected: 2, target.Count);
		Assert.AreSame(first, target[0]);
		Assert.AreSame(second, target[1]);
		CollectionAssert.AreEqual(new[] { "state1", "state2" }, target.Select(identifier => identifier.Value).ToArray());
		var concreteEnumerator = target.GetEnumerator();
		var concreteValues = new List<string>();

		while (concreteEnumerator.MoveNext())
		{
			concreteValues.Add(concreteEnumerator.Current.Value);
		}

		var nonGenericEnumerator = ((IEnumerable) target).GetEnumerator();
		var nonGenericValues = new List<string>();

		while (nonGenericEnumerator.MoveNext())
		{
			nonGenericValues.Add(((IIdentifier) nonGenericEnumerator.Current).Value);
		}

		CollectionAssert.AreEqual(new[] { "state1", "state2" }, concreteValues);
		CollectionAssert.AreEqual(new[] { "state1", "state2" }, nonGenericValues);
		Assert.AreEqual(expected: "state1 state2", target.ToString());
		Assert.AreEqual(expected: "state1 state2", target.ToString(format: null, formatProvider: null));
		Assert.IsTrue(target.Equals(same));
		Assert.AreEqual(same.GetHashCode(), target.GetHashCode());
		Assert.IsFalse(target.Equals(different));
		Assert.IsFalse(target.Equals("not target"));

		var destination = new char[32];
		Assert.IsTrue(target.TryFormat(destination, out var charsWritten, format: default, provider: null));
		Assert.AreEqual(expected: "state1 state2", new string(destination, startIndex: 0, charsWritten));
		Assert.IsFalse(target.TryFormat(new char[4], out _, format: default, provider: null));

		Target empty = ImmutableArray<IIdentifier>.Empty;
		Assert.AreEqual(string.Empty, empty.ToString());
		Assert.IsTrue(default(Target).IsDefault);
		Assert.AreEqual(string.Empty, default(Target).ToString(format: null, formatProvider: null));
	}

	[TestMethod]
	public void TargetObjectEqualityMatchesStronglyTypedEquality()
	{
		var first = Identifier.FromString("state1");
		var second = Identifier.FromString("state2");
		var target = Target.Create([first, second]);
		var same = Target.Create([first, second]);

		Assert.IsTrue(target.Equals((object) same));
	}
}
