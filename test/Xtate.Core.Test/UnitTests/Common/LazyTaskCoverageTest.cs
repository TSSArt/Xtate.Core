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

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class LazyTaskCoverageTest
{
	[TestMethod]
	public async Task TaskStartsFactoryOnlyOnceAndReturnsSharedTask()
	{
		var calls = 0;
		var lazyTask = new LazyTask<int>(async () =>
										 {
											 await Task.Yield();
											 Interlocked.Increment(ref calls);

											 return 42;
										 });

		var first = lazyTask.Task;
		var second = lazyTask.Task;

		Assert.AreSame(first, second);
		Assert.AreEqual(expected: 42, await first);
		Assert.AreEqual(expected: 1, calls);
	}

	[TestMethod]
	public async Task TaskPropagatesFactoryException()
	{
		var expected = new InvalidOperationException("factory failed");
		var lazyTask = new LazyTask<int>(() => throw expected);

		var actual = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await lazyTask.Task);

		Assert.AreSame(expected, actual);
	}

	[TestMethod]
	public async Task TaskIsCancelledWhenTokenIsCancelledBeforeFactoryCompletes()
	{
		using var cancellationTokenSource = new CancellationTokenSource();
		var enteredFactory = new TaskCompletionSource();
		var releaseFactory = new TaskCompletionSource();
		var lazyTask = new LazyTask<int>(
			async () =>
			{
				enteredFactory.SetResult();
				await releaseFactory.Task;

				return 42;
			},
			taskMonitor: null,
			cancellationTokenSource.Token);

		var task = lazyTask.Task;
		await enteredFactory.Task;

		cancellationTokenSource.Cancel();
		releaseFactory.SetResult();

		await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await task);
		Assert.IsTrue(task.IsCanceled);
	}

	[TestMethod]
	public async Task TaskUsesFactoryOperationCanceledExceptionToken()
	{
		using var cancellationTokenSource = new CancellationTokenSource();
		var lazyTask = new LazyTask<int>(() => throw new OperationCanceledException(cancellationTokenSource.Token));

		await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await lazyTask.Task);
		Assert.IsTrue(lazyTask.Task.IsCanceled);
	}
}