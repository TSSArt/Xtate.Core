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

using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.StateMachine;

[TestClass]
public class EventNameCoverageTest
{
	[TestMethod]
	public void EventNameSupportsFormattingIndexingEnumerationAndExplicitConversion()
	{
		var eventName = (EventName) "error.platform.io";

		Assert.AreEqual(3, eventName.Count);
		Assert.AreEqual("error", eventName[0].Value);
		Assert.AreEqual("platform", eventName[1].Value);
		Assert.AreEqual("io", eventName[2].Value);
		CollectionAssert.AreEqual(new[] { "error", "platform", "io" }, eventName.Select(part => part.Value).ToArray());
		Assert.AreEqual("error.platform.io", ((IFormattable) eventName).ToString(format: null, formatProvider: null));

		var destination = new char[32];
		Assert.IsTrue(eventName.TryFormat(destination, out var charsWritten, format: default, provider: null));
		Assert.AreEqual("error.platform.io", new string(destination, 0, charsWritten));
		Assert.IsFalse(eventName.TryFormat(new char[4], out _, format: default, provider: null));
	}

	[TestMethod]
	public void EventDescriptorMatchingHandlesWildcardPrefixExactAndMismatchCases()
	{
		var eventName = EventName.FromString("error.platform.io");

		Assert.IsTrue(eventName.IsMatchedToEventDescriptor("*"));
		Assert.IsTrue(eventName.IsMatchedToEventDescriptor("error.*"));
		Assert.IsTrue(eventName.IsMatchedToEventDescriptor("error."));
		Assert.IsTrue(eventName.IsMatchedToEventDescriptor("error.platform"));
		Assert.IsTrue(eventName.IsMatchedToEventDescriptor("error.platform.io"));
		Assert.IsFalse(eventName.IsMatchedToEventDescriptor(string.Empty));
		Assert.IsFalse(eventName.IsMatchedToEventDescriptor("platform"));
		Assert.IsFalse(eventName.IsMatchedToEventDescriptor("error.communication"));
		Assert.IsFalse(eventName.IsMatchedToEventDescriptor("error.platform.io.more"));
	}

	[TestMethod]
	public void DefaultEventNameFormatsAsEmptyAndDoesNotMatchDescriptors()
	{
		// The formatting path treats default(EventName) as empty, but descriptor matching currently does not guard the
		// default backing ImmutableArray before enumeration. This test captures the desired non-throwing behavior.
		EventName eventName = default;
		var destination = new char[8];

		Assert.IsTrue(eventName.IsDefault);
		Assert.AreEqual(string.Empty, ((IFormattable) eventName).ToString(format: null, formatProvider: null));
		Assert.IsTrue(eventName.TryFormat(destination, out var charsWritten, format: default, provider: null));
		Assert.AreEqual(0, charsWritten);
		Assert.IsFalse(eventName.IsMatchedToEventDescriptor("error"));
	}
}
