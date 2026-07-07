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

using System.Collections.Concurrent;
using System.Threading;
using Xtate;

namespace Xtate.Test;

[TestClass]
public class ConcurrentDictionaryExtensionsTest
{
	[TestMethod]
	public void TryTake_FromEmptyDictionary_ShouldReturnFalse()
	{
		// Arrange
		var dict = new ConcurrentDictionary<string, int>();

		// Act
		var result = dict.TryTake(out var key, out var value);

		// Assert
		Assert.IsFalse(result);
		Assert.IsNull(key);
		Assert.AreEqual(0, value);
	}

	[TestMethod]
	public void TryTake_FromNonEmptyDictionary_ShouldReturnTrue()
	{
		// Arrange
		var dict = new ConcurrentDictionary<string, int>();
		dict.TryAdd("key1", 1);

		// Act
		var result = dict.TryTake(out var key, out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual("key1", key);
		Assert.AreEqual(1, value);
	}

	[TestMethod]
	public void TryTake_ShouldRemoveItemFromDictionary()
	{
		// Arrange
		var dict = new ConcurrentDictionary<string, int>();
		dict.TryAdd("key1", 1);

		// Act
		dict.TryTake(out _, out _);

		// Assert
		Assert.IsTrue(dict.IsEmpty);
	}

	[TestMethod]
	public void TryTake_WithMultipleItems_ShouldRemoveOne()
	{
		// Arrange
		var dict = new ConcurrentDictionary<string, int>();
		dict.TryAdd("key1", 1);
		dict.TryAdd("key2", 2);

		// Act
		var result = dict.TryTake(out var key, out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.IsNotNull(key);
		Assert.IsTrue(dict.Count == 1);
	}

	[TestMethod]
	public void TryTake_SequentialCalls_ShouldReturnFalseWhenEmpty()
	{
		// Arrange
		var dict = new ConcurrentDictionary<string, int>();
		dict.TryAdd("key1", 1);

		// Act
		dict.TryTake(out _, out _);
		var secondResult = dict.TryTake(out _, out _);

		// Assert
		Assert.IsFalse(secondResult);
	}

	[TestMethod]
	public void TryTake_WithStringValues_ShouldWorkCorrectly()
	{
		// Arrange
		var dict = new ConcurrentDictionary<int, string>();
		dict.TryAdd(1, "value1");

		// Act
		var result = dict.TryTake(out var key, out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(1, key);
		Assert.AreEqual("value1", value);
	}

	[TestMethod]
	public void TryTake_Concurrent_ShouldHandleMultipleThreads()
	{
		// Arrange
		var dict = new ConcurrentDictionary<int, string>();
		var tasks = new List<Task>();

		// Add items
		for (int i = 0; i < 10; i++)
		{
			dict.TryAdd(i, $"value{i}");
		}

		var removedCount = 0;

		// Act - Remove items concurrently
		for (int i = 0; i < 10; i++)
		{
			tasks.Add(Task.Run(() =>
			{
				if (dict.TryTake(out _, out _))
				{
					Interlocked.Increment(ref removedCount);
				}
			}));
		}

		Task.WaitAll(tasks.ToArray());

		// Assert
		Assert.AreEqual(10, removedCount);
		Assert.IsTrue(dict.IsEmpty);
	}
}
