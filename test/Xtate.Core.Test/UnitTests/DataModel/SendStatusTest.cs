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

using Xtate.DataModel;

namespace Xtate.Test;

[TestClass]
public class SendStatusTest
{
	[TestMethod]
	public void SendStatus_ShouldHaveSentValue()
	{
		// Act
		var status = SendStatus.Sent;

		// Assert
		Assert.AreEqual(SendStatus.Sent, status);
	}

	[TestMethod]
	public void SendStatus_ShouldHaveScheduledValue()
	{
		// Act
		var status = SendStatus.Scheduled;

		// Assert
		Assert.AreEqual(SendStatus.Scheduled, status);
	}

	[TestMethod]
	public void SendStatus_ShouldHaveToInternalQueueValue()
	{
		// Act
		var status = SendStatus.ToInternalQueue;

		// Assert
		Assert.AreEqual(SendStatus.ToInternalQueue, status);
	}

	[TestMethod]
	public void SendStatus_AllValuesShouldBeDifferent()
	{
		// Act & Assert
		Assert.AreNotEqual(SendStatus.Sent, SendStatus.Scheduled);
		Assert.AreNotEqual(SendStatus.Sent, SendStatus.ToInternalQueue);
		Assert.AreNotEqual(SendStatus.Scheduled, SendStatus.ToInternalQueue);
	}

	[TestMethod]
	public void SendStatus_ShouldSupportEquality()
	{
		// Act
		var sent1 = SendStatus.Sent;
		var sent2 = SendStatus.Sent;
		var scheduled = SendStatus.Scheduled;

		// Assert
		Assert.IsTrue(sent1 == sent2);
		Assert.IsFalse(sent1 == scheduled);
		Assert.IsTrue(sent1.Equals(sent2));
		Assert.IsFalse(sent1.Equals(scheduled));
	}

	[TestMethod]
	public void SendStatus_ShouldHaveCorrectCount()
	{
		// Act
		var values = Enum.GetValues(typeof(SendStatus));

		// Assert
		Assert.AreEqual(expected: 3, values.Length);
	}

	[TestMethod]
	public void SendStatus_EnumNames_ShouldMatch()
	{
		// Act
		var names = Enum.GetNames(typeof(SendStatus));

		// Assert
		Assert.IsTrue(names.Contains("Sent"));
		Assert.IsTrue(names.Contains("Scheduled"));
		Assert.IsTrue(names.Contains("ToInternalQueue"));
	}

	[TestMethod]
	public void SendStatus_ShouldHaveValidStringRepresentation()
	{
		// Act & Assert
		Assert.AreEqual(expected: "Sent", SendStatus.Sent.ToString());
		Assert.AreEqual(expected: "Scheduled", SendStatus.Scheduled.ToString());
		Assert.AreEqual(expected: "ToInternalQueue", SendStatus.ToInternalQueue.ToString());
	}

	[TestMethod]
	public void SendStatus_GetNames_ShouldReturnValidNames()
	{
		// Act
		var names = Enum.GetNames(typeof(SendStatus));

		// Assert
		Assert.IsTrue(names.Length == 3);
	}
}