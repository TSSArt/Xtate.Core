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

namespace Xtate.Test;

[TestClass]
public class EventTypeTest
{
	[TestMethod]
	public void EventType_ShouldHaveNoneValue()
	{
		// Act
		var eventType = EventType.None;

		// Assert
		Assert.AreEqual(EventType.None, eventType);
	}

	[TestMethod]
	public void EventType_ShouldHavePlatformValue()
	{
		// Act
		var eventType = EventType.Platform;

		// Assert
		Assert.AreEqual(EventType.Platform, eventType);
	}

	[TestMethod]
	public void EventType_ShouldHaveInternalValue()
	{
		// Act
		var eventType = EventType.Internal;

		// Assert
		Assert.AreEqual(EventType.Internal, eventType);
	}

	[TestMethod]
	public void EventType_ShouldHaveExternalValue()
	{
		// Act
		var eventType = EventType.External;

		// Assert
		Assert.AreEqual(EventType.External, eventType);
	}

	[TestMethod]
	public void EventType_AllValuesShouldBeDifferent()
	{
		// Act & Assert
		Assert.AreNotEqual(EventType.None, EventType.Platform);
		Assert.AreNotEqual(EventType.Platform, EventType.Internal);
		Assert.AreNotEqual(EventType.Internal, EventType.External);
		Assert.AreNotEqual(EventType.None, EventType.External);
	}

	[TestMethod]
	public void EventType_EqualityComparison_ShouldWork()
	{
		// Act
		var type1 = EventType.Internal;
		var type2 = EventType.Internal;
		var type3 = EventType.External;

		// Assert
		Assert.IsTrue(type1 == type2);
		Assert.IsFalse(type1 == type3);
		Assert.IsFalse(type1 != type2);
		Assert.IsTrue(type1 != type3);
	}

	[TestMethod]
	public void EventType_ToString_ShouldReturnValidString()
	{
		// Act
		var noneStr = EventType.None.ToString();
		var platformStr = EventType.Platform.ToString();
		var internalStr = EventType.Internal.ToString();
		var externalStr = EventType.External.ToString();

		// Assert
		Assert.AreEqual(expected: "None", noneStr);
		Assert.AreEqual(expected: "Platform", platformStr);
		Assert.AreEqual(expected: "Internal", internalStr);
		Assert.AreEqual(expected: "External", externalStr);
	}

	[TestMethod]
	public void EventType_GetValues_ShouldReturnAllValues()
	{
		// Act
		var values = Enum.GetValues(typeof(EventType));

		// Assert
		Assert.AreEqual(expected: 4, values.Length);
	}

	[TestMethod]
	public void EventType_GetNames_ShouldReturnAllNames()
	{
		// Act
		var names = Enum.GetNames(typeof(EventType));

		// Assert
		Assert.AreEqual(expected: 4, names.Length);
		Assert.IsTrue(names.Contains("None"));
		Assert.IsTrue(names.Contains("Platform"));
		Assert.IsTrue(names.Contains("Internal"));
		Assert.IsTrue(names.Contains("External"));
	}

	[TestMethod]
	public void EventType_GetHashCode_ShouldBeConsistent()
	{
		// Arrange
		var type1 = EventType.Platform;

		// Act
		var hash1 = type1.GetHashCode();
		var hash2 = type1.GetHashCode();

		// Assert
		Assert.AreEqual(hash1, hash2);
	}

	[TestMethod]
	public void EventType_CastToInt_ShouldWork()
	{
		// Act
		var noneInt = (int) EventType.None;
		var platformInt = (int) EventType.Platform;
		var internalInt = (int) EventType.Internal;
		var externalInt = (int) EventType.External;

		// Assert
		Assert.AreEqual(expected: 0, noneInt);
		Assert.AreEqual(expected: 1, platformInt);
		Assert.AreEqual(expected: 2, internalInt);
		Assert.AreEqual(expected: 3, externalInt);
	}

	[TestMethod]
	public void EventType_CastFromInt_ShouldWork()
	{
		// Act
		var eventType = (EventType) 2;

		// Assert
		Assert.AreEqual(EventType.Internal, eventType);
	}
}