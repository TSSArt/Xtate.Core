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

namespace Xtate.Core.Test.UnitTests.Common;

[TestClass]
public class DateTimeExtensionsTest
{
	[TestMethod]
	public void UniqueUtcNow_ShouldReturnDateTime()
	{
		// Act
		var result = DateTime.UniqueUtcNow;

		// Assert
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
		Assert.IsTrue(first <= second, message: "First should be less than or equal to second");
		Assert.IsTrue(second < third, message: "Second should be less than third");
	}

	[TestMethod]
	[ExcludeFromCodeCoverage]
	public async Task UniqueUtcNow_WithConcurrentCalls_ShouldReturnUniqueValuesInOrder()
	{
		// Arrange
		var results = new DateTime[10];
		var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		// Act
		var tasks = Enumerable.Range(start: 0, count: results.Length)
						  .Select(index => Task.Run(async () =>
													  {
														  await start.Task;
														  results[index] = DateTime.UniqueUtcNow;
													  }))
						  .ToArray();
		start.SetResult(true);
		await Task.WhenAll(tasks);
		Array.Sort(results);

		// Assert
		for (var i = 1; i < results.Length; i++)
		{
			Assert.IsTrue(results[i - 1] < results[i], $"Values not in order: {results[i - 1]} should be < {results[i]}");
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
		Assert.IsTrue(result >= before, message: "Result should be >= before");
		Assert.IsTrue(result <= after.AddMinutes(1), message: "Result should be <= after + 1 minute");
	}

	[TestMethod]
	public async Task UniqueUtcNow_MultipleCallsAtSameTime_ShouldHaveSequentialTicks()
	{
		// Act
		var times = new long[5];
		var tasks = Enumerable.Range(start: 0, count: times.Length)
						  .Select(index => Task.Run(() => times[index] = DateTime.UniqueUtcNow.Ticks))
						  .ToArray();
		await Task.WhenAll(tasks);

		// Assert
		Array.Sort(times);

		for (var i = 1; i < times.Length; i++)
		{
			Assert.IsTrue(times[i] >= times[i - 1], message: "Ticks should be in ascending order");
		}
	}
}
