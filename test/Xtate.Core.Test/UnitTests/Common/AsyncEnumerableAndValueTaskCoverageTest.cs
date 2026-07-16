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

using System.Threading;
using Xtate.TaskMonitor;

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class AsyncEnumerableAndValueTaskCoverageTest
{
	[TestMethod]
	public async Task AsyncEnumerableExtensionsCollectAndAppendAllItemsIncludingEmptySequences()
	{
		var values = Values(1, 2, 3);

		var immutable = await values.ToImmutableArrayAsync();
		var collection = new List<int> { 0 };
		await Values(1, 2, 3).AppendCollectionAsync(collection);
		var empty = await Values<int>().ToImmutableArrayAsync();

		CollectionAssert.AreEqual(new[] { 1, 2, 3 }, immutable.ToArray());
		CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, collection);
		Assert.IsTrue(empty.IsEmpty);
	}

	[TestMethod]
	public async Task ValueTaskExtensionsCoverGenericAndNonGenericWaitAndMonitorPaths()
	{
		using var cancellation = new CancellationTokenSource();
		var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var pendingWait = new ValueTask(completion.Task).WaitAsync(cancellation.Token);
		completion.SetResult();
		await pendingWait;

		var genericCompletion = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
		var genericWait = new ValueTask<int>(genericCompletion.Task).WaitAsync(cancellation.Token);
		genericCompletion.SetResult(42);
		Assert.AreEqual(expected: 42, await genericWait);

		await ValueTask.CompletedTask.WaitAsync(CancellationToken.None);
		Assert.AreEqual(expected: 7, await new ValueTask<int>(7).WaitAsync(CancellationToken.None));

		var monitor = new Mock<ITaskMonitor>();
		monitor.Setup(m => m.WaitAsync(It.IsAny<ValueTask>(), cancellation.Token))
			   .Returns((ValueTask task, CancellationToken _) => task);
		monitor.Setup(m => m.WaitAsync(It.IsAny<ValueTask<int>>(), cancellation.Token))
			   .Returns((ValueTask<int> task, CancellationToken _) => task);

		await ValueTask.CompletedTask.WaitAsync(monitor.Object, cancellation.Token);
		Assert.AreEqual(expected: 9, await new ValueTask<int>(9).WaitAsync(monitor.Object, cancellation.Token));
		ValueTask.CompletedTask.Forget(monitor.Object);
		new ValueTask<int>(11).Forget(monitor.Object);
		ValueTask.CompletedTask.Forget();
		new ValueTask<int>(13).Forget();

		monitor.Verify(m => m.WaitAsync(It.IsAny<ValueTask>(), cancellation.Token), Times.Once);
		monitor.Verify(m => m.WaitAsync(It.IsAny<ValueTask<int>>(), cancellation.Token), Times.Once);
		monitor.Verify(m => m.Forget(It.IsAny<ValueTask>()), Times.Once);
		monitor.Verify(m => m.Forget(It.IsAny<ValueTask<int>>()), Times.Once);
	}

	private static async IAsyncEnumerable<T> Values<T>(params T[] values)
	{
		await Task.Yield();

		foreach (var value in values)
		{
			yield return value;
		}
	}
}