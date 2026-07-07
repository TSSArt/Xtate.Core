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

using Xtate;

namespace Xtate.Test;

[TestClass]
public class DateTimeExtensionsTest
{
	[TestMethod]
	public void UniqueUtcNow_ShouldReturnDateTime()
	{
		// Act
		var result = DateTime.UniqueUtcNow;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(DateTimeKind.Utc, result.Kind);
	}

	[TestMethod]
	public void UniqueUtcNow_ShouldReturnUtcKind()
	{
		// Act
		var result = DateTime.UniqueUtcNow;

		// Assert
		Assert.AreEqual(DateTimeKind.Utc, result.Kind);
	}

	[TestMethod]
	public void UniqueUtcNow_ShouldReturnTimesInAscendingOrder()
	{
		// Act
		var first = DateTime.UniqueUtcNow;
		var second = DateTime.UniqueUtcNow;
		var third = DateTime.UniqueUtcNow;

		// Assert
		Assert.IsTrue(first <= second, "First should be less than or equal to second");
		Assert.IsTrue(second < third, "Second should be less than third");
	}

	[TestMethod]
	public void UniqueUtcNow_WithConcurrentCalls_ShouldReturnUniqueValuesInOrder()
	{
		// Arrange
		var results = new List<DateTime>();
		var barrier = new System.Threading.Barrier(10);

		// Act
		for (int i = 0; i < 10; i++)
		{
			Task.Run(() =>
			{
				barrier.SignalAndWait();
				lock (results)
				{
					results.Add(DateTime.UniqueUtcNow);
				}
			});
		}

		// Wait for all tasks
		System.Threading.Thread.Sleep(100);

		// Assert
		for (int i = 1; i < results.Count; i++)
		{
			Assert.IsTrue(results[i - 1] <= results[i], $"Values not in order: {results[i - 1]} should be <= {results[i]}");
		}
	}

	[TestMethod]
	public void UniqueUtcNow_ShouldReturnRecentTime()
	{
		// Arrange
		var before = DateTime.UtcNow;

		// Act
		var result = DateTime.UniqueUtcNow;

		// Assert
		var after = DateTime.UtcNow;
		Assert.IsTrue(result >= before, "Result should be >= before");
		Assert.IsTrue(result <= after.AddSeconds(1), "Result should be <= after + 1 second");
	}

	[TestMethod]
	public void UniqueUtcNow_MultipleCallsAtSameTime_ShouldHaveSequentialTicks()
	{
		// Act
		var times = new List<long>();
		var tasks = new List<Task>();

		for (int i = 0; i < 5; i++)
		{
			tasks.Add(Task.Run(() =>
			{
				times.Add(DateTime.UniqueUtcNow.Ticks);
			}));
		}

		Task.WaitAll(tasks.ToArray());

		// Assert
		times.Sort();
		for (int i = 1; i < times.Count; i++)
		{
			Assert.IsTrue(times[i] >= times[i - 1], "Ticks should be in ascending order");
		}
	}
}
