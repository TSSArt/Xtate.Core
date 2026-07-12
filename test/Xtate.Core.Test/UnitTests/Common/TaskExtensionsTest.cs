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
public class TaskExtensionsTest
{
	[TestMethod]
	[ExcludeFromCodeCoverage]
	public void Forget_OnTask_ShouldNotThrow()
	{
		// Arrange
		var task = Task.CompletedTask;

		// Act; any exception fails the test.
		task.Forget();
	}

	[TestMethod]
	public void Forget_WithTaskMonitor_ShouldCallMonitorForget()
	{
		// Arrange
		var mockMonitor = new MockTaskMonitor();
		var task = Task.CompletedTask;

		// Act
		task.Forget(mockMonitor);

		// Assert
		Assert.AreEqual(expected: 1, mockMonitor.ForgottenTasks.Count);
		Assert.AreSame(task, mockMonitor.ForgottenTasks[0]);
	}

	[TestMethod]
	public async Task WaitAsync_WithTaskMonitor_ShouldWaitForTask()
	{
		// Arrange
		var mockMonitor = new MockTaskMonitor();
		var task = Task.Delay(10);
		using var cts = new CancellationTokenSource();

		// Act
		var result = task.WaitAsync(mockMonitor, cts.Token);
		await result;

		// Assert
		Assert.AreEqual(expected: 1, mockMonitor.WaitingTasks.Count);
		Assert.AreSame(task, mockMonitor.WaitingTasks[0].task);
		Assert.AreEqual(cts.Token, mockMonitor.WaitingTasks[0].token);
	}

	[TestMethod]
	public async Task WaitAsync_GenericTask_WithTaskMonitor_ShouldWaitForTask()
	{
		// Arrange
		var mockMonitor = new MockTaskMonitor();
		var task = Task.FromResult(42);
		using var cts = new CancellationTokenSource();

		// Act
		var result = task.WaitAsync(mockMonitor, cts.Token);
		var value = await result;

		// Assert
		Assert.AreEqual(expected: 42, value);
		Assert.AreEqual(expected: 1, mockMonitor.WaitingTasks.Count);
		Assert.AreSame(task, mockMonitor.WaitingTasks[0].task);
	}

	[TestMethod]
	public async Task WaitAsync_WithCancellationToken_ShouldPassTokenToMonitor()
	{
		// Arrange
		var mockMonitor = new MockTaskMonitor();
		var task = Task.CompletedTask;
		using var cts = new CancellationTokenSource();

		// Act
		await task.WaitAsync(mockMonitor, cts.Token);

		// Assert
		Assert.AreEqual(cts.Token, mockMonitor.WaitingTasks[0].token);
	}

	[ExcludeFromCodeCoverage]
	private class MockTaskMonitor : ITaskMonitor
	{
		public List<Task> ForgottenTasks { get; } = [];

		public List<(Task task, CancellationToken token)> WaitingTasks { get; } = [];

	#region Interface ITaskMonitor

		public void Forget(Task task)
		{
			ForgottenTasks.Add(task);
		}

		public void Forget(ValueTask valueTask) { }

		public void Forget<TResult>(ValueTask<TResult> valueTask) { }

		public Task WaitAsync(Task task, CancellationToken token)
		{
			WaitingTasks.Add((task, token));

			return task;
		}

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token)
		{
			WaitingTasks.Add((task, token));

			return task;
		}

		public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => valueTask;

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) => valueTask;

	#endregion
	}
}