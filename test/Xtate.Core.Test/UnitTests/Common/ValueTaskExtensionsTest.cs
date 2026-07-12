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

namespace Xtate.Test;

[TestClass]
public class ValueTaskExtensionsTest
{
	[TestMethod]
	[ExcludeFromCodeCoverage]
	public void Forget_OnValueTask_ShouldNotThrow()
	{
		// Arrange
		var valueTask = new ValueTask(Task.CompletedTask);

		// Act & Assert
		try
		{
			valueTask.Forget();
			Assert.IsTrue(true);
		}
		catch (Exception ex)
		{
			Assert.Fail($"Unexpected exception: {ex.Message}");
		}
	}

	[TestMethod]
	[ExcludeFromCodeCoverage]
	public void Forget_OnCompletedValueTask_ShouldNotThrow()
	{
		// Arrange
		var valueTask = new ValueTask();

		// Act & Assert
		try
		{
			valueTask.Forget();
			Assert.IsTrue(true);
		}
		catch (Exception ex)
		{
			Assert.Fail($"Unexpected exception: {ex.Message}");
		}
	}

	[TestMethod]
	public void Forget_WithTaskMonitor_ShouldCallMonitor()
	{
		// Arrange
		var mockMonitor = new MockTaskMonitor();
		var valueTask = new ValueTask(Task.CompletedTask);

		// Act
		valueTask.Forget(mockMonitor);

		// Assert
		Assert.AreEqual(expected: 1, mockMonitor.ForgottenValueTasks.Count);
	}

	[TestMethod]
	public async Task WaitAsync_CompletedValueTask_ShouldCompleteImmediately()
	{
		// Arrange
		var valueTask = new ValueTask();
		using var cts = new CancellationTokenSource();

		// Act
		await valueTask.WaitAsync(cts.Token);

		// Assert
		Assert.IsTrue(true);
	}

	[TestMethod]
	public async Task WaitAsync_WithCancellationToken_ShouldWorkCorrectly()
	{
		// Arrange
		var valueTask = new ValueTask(Task.Delay(100));
		using var cts = new CancellationTokenSource();

		// Act
		await valueTask.WaitAsync(cts.Token);

		// Assert
		Assert.IsTrue(true);
	}

	[TestMethod]
	public async Task WaitAsync_CompleteValueTaskWithToken_ShouldNotCancelEarly()
	{
		// Arrange
		var taskCompletionSource = new TaskCompletionSource();
		taskCompletionSource.SetResult();
		var valueTask = new ValueTask(taskCompletionSource.Task);
		using var cts = new CancellationTokenSource();

		// Act
		await valueTask.WaitAsync(cts.Token);

		// Assert
		Assert.IsTrue(true);
	}

	[ExcludeFromCodeCoverage]
	private class MockTaskMonitor : ITaskMonitor
	{
		public List<ValueTask> ForgottenValueTasks { get; } = [];

	#region Interface ITaskMonitor

		public void Forget(Task task) { }

		public void Forget(ValueTask valueTask)
		{
			ForgottenValueTasks.Add(valueTask);
		}

		public void Forget<TResult>(ValueTask<TResult> valueTask) { }

		public Task WaitAsync(Task task, CancellationToken token) => task;

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task;

		public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => valueTask;

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) => valueTask;

	#endregion
	}
}