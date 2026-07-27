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

namespace Xtate.Core.Test.UnitTests.DataModel;

[TestClass]
public class SendStatusTest
{
	[TestMethod]
	public void SendStatus_AllValuesShouldBeDifferent()
	{
		Assert.AreEqual(expected: 3, Enum.GetValues(typeof(SendStatus)).Cast<SendStatus>().Distinct().Count());
	}

	[TestMethod]
	[DataRow(SendStatus.Sent, SendStatus.Sent, true)]
	[DataRow(SendStatus.Sent, SendStatus.Scheduled, false)]
	public void SendStatus_ShouldSupportEquality(SendStatus left, SendStatus right, bool expectedEqual)
	{
		Assert.AreEqual(expectedEqual, left == right);
		Assert.AreEqual(expectedEqual, left.Equals(right));
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
		Assert.AreEqual(nameof(SendStatus.Sent), TestHelper.EnumName(SendStatus.Sent));
		Assert.AreEqual(nameof(SendStatus.Scheduled), TestHelper.EnumName(SendStatus.Scheduled));
		Assert.AreEqual(nameof(SendStatus.ToInternalQueue), TestHelper.EnumName(SendStatus.ToInternalQueue));
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