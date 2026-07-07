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
public class EventDescriptorTest
{
	[TestMethod]
	public void EventDescriptor_FromString_ShouldCreateInstance()
	{
		// Arrange
		const string eventName = "test.event";

		// Act
		var descriptor = EventDescriptor.FromString(eventName);

		// Assert
		Assert.IsNotNull(descriptor);
	}

	[TestMethod]
	public void EventDescriptor_Value_ShouldPreserveInputString()
	{
		// Arrange
		const string eventName = "test.event";

		// Act
		var descriptor = EventDescriptor.FromString(eventName);

		// Assert
		Assert.AreEqual(eventName, descriptor.Value);
	}

	[TestMethod]
	public void EventDescriptor_ToString_ShouldReturnValue()
	{
		// Arrange
		const string eventName = "test.event";

		// Act
		var descriptor = EventDescriptor.FromString(eventName);
		var result = descriptor.ToString();

		// Assert
		Assert.AreEqual(eventName, result);
	}

	[TestMethod]
	public void EventDescriptor_ExplicitCast_ShouldCreateInstance()
	{
		// Arrange
		const string eventName = "test.event";

		// Act
		var descriptor = (EventDescriptor) eventName;

		// Assert
		Assert.IsNotNull(descriptor);
		Assert.AreEqual(eventName, descriptor.Value);
	}

	[TestMethod]
	public void EventDescriptor_Equals_ShouldReturnTrueForSameValue()
	{
		// Arrange
		const string eventName = "test.event";
		var descriptor1 = EventDescriptor.FromString(eventName);
		var descriptor2 = EventDescriptor.FromString(eventName);

		// Act & Assert
		Assert.AreEqual(descriptor1, descriptor2);
		Assert.IsTrue(descriptor1.Equals(descriptor2));
	}

	[TestMethod]
	public void EventDescriptor_Equals_ShouldReturnFalseForDifferentValues()
	{
		// Arrange
		var descriptor1 = EventDescriptor.FromString("event1");
		var descriptor2 = EventDescriptor.FromString("event2");

		// Act & Assert
		Assert.AreNotEqual(descriptor1, descriptor2);
		Assert.IsFalse(descriptor1.Equals(descriptor2));
	}

	[TestMethod]
	public void EventDescriptor_GetHashCode_ShouldBeSameForEqualValues()
	{
		// Arrange
		const string eventName = "test.event";
		var descriptor1 = EventDescriptor.FromString(eventName);
		var descriptor2 = EventDescriptor.FromString(eventName);

		// Act
		var hash1 = descriptor1.GetHashCode();
		var hash2 = descriptor2.GetHashCode();

		// Assert
		Assert.AreEqual(hash1, hash2);
	}

	[TestMethod]
	public void EventDescriptor_GetHashCode_ShouldBeDifferentForDifferentValues()
	{
		// Arrange
		var descriptor1 = EventDescriptor.FromString("event1");
		var descriptor2 = EventDescriptor.FromString("event2");

		// Act
		var hash1 = descriptor1.GetHashCode();
		var hash2 = descriptor2.GetHashCode();

		// Assert - different strings typically produce different hashes
		Assert.AreNotEqual(hash1, hash2);
	}

	[TestMethod]
	public void EventDescriptor_EqualsObject_ShouldWorkWithObjectComparison()
	{
		// Arrange
		const string eventName = "test.event";
		var descriptor1 = EventDescriptor.FromString(eventName);
		object descriptor2 = EventDescriptor.FromString(eventName);

		// Act
		var equals = descriptor1.Equals(descriptor2);

		// Assert
		Assert.IsTrue(equals);
	}

	[TestMethod]
	public void EventDescriptor_EqualsObject_ShouldReturnFalseForDifferentType()
	{
		// Arrange
		var descriptor = EventDescriptor.FromString("test.event");

		// Act
		var equals = descriptor.Equals("test.event");

		// Assert
		Assert.IsFalse(equals);
	}

	[TestMethod]
	public void EventDescriptor_EqualsNull_ShouldReturnFalse()
	{
		// Arrange
		var descriptor = EventDescriptor.FromString("test.event");

		// Act
		var equals = descriptor.Equals(null);

		// Assert
		Assert.IsFalse(equals);
	}

	[TestMethod]
	public void EventDescriptor_MultipleConstructions_ShouldBeConsistent()
	{
		// Arrange
		const string eventName = "internal.event";

		// Act
		var descriptors = new[]
		{
			EventDescriptor.FromString(eventName),
			(EventDescriptor) eventName,
			EventDescriptor.FromString(eventName)
		};

		// Assert
		Assert.AreEqual(descriptors[0], descriptors[1]);
		Assert.AreEqual(descriptors[1], descriptors[2]);
		Assert.AreEqual(descriptors[0].GetHashCode(), descriptors[1].GetHashCode());
	}

	[TestMethod]
	public void EventDescriptor_EmptyString_ShouldBeAllowed()
	{
		// Arrange
		const string eventName = "";

		// Act
		var descriptor = EventDescriptor.FromString(eventName);

		// Assert
		Assert.IsNotNull(descriptor);
		Assert.AreEqual(eventName, descriptor.Value);
	}

	[TestMethod]
	public void EventDescriptor_SpecialCharacters_ShouldBePreserved()
	{
		// Arrange
		const string eventName = "test.event#special$chars^123";

		// Act
		var descriptor = EventDescriptor.FromString(eventName);

		// Assert
		Assert.AreEqual(eventName, descriptor.Value);
	}
}
