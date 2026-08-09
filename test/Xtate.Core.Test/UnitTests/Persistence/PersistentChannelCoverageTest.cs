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
using System.Threading.Channels;
using Xtate.Persistence;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Services;

namespace Xtate.Core.Test.UnitTests.Persistence;

[TestClass]
public class PersistentChannelCoverageTest
{
	[TestMethod]
	public async Task BufferedItemsAreReadInOrderAndCompletionFinishesAfterTheLastItem()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);

		await channel.Writer.WriteAsync("one");
		await channel.Writer.WriteAsync("two");
		Assert.AreEqual(expected: 2, channel.Reader.Count);
		Assert.IsTrue(channel.Writer.TryComplete());
		Assert.IsFalse(channel.Writer.TryComplete());
		Assert.IsFalse(channel.Reader.Completion.IsCompleted);

		Assert.AreEqual(expected: "one", await channel.Reader.ReadAsync());
		Assert.AreEqual(expected: 1, channel.Reader.Count);
		Assert.AreEqual(expected: "two", await channel.Reader.ReadAsync());
		Assert.AreEqual(expected: 0, channel.Reader.Count);

		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		await channel.Reader.Completion.WaitAsync(timeout.Token);
		Assert.IsFalse(await channel.Reader.WaitToReadAsync(timeout.Token));
		Assert.IsFalse(await channel.Writer.WaitToWriteAsync(timeout.Token));
		await Assert.ThrowsExactlyAsync<ChannelClosedException>(async () => await channel.Writer.WriteAsync(item: "three", timeout.Token));
	}

	[TestMethod]
	public async Task PersistedItemsAndHeadPositionSurviveChannelRecreation()
	{
		await using var storage = new TestTransactionalStorage();
		var first = CreateChannel(storage);

		await first.Writer.WriteAsync("one");
		await first.Writer.WriteAsync("two");

		var second = CreateChannel(storage);
		Assert.AreEqual(expected: 2, second.Reader.Count);
		Assert.AreEqual(expected: "one", await second.Reader.ReadAsync());

		var third = CreateChannel(storage);
		Assert.AreEqual(expected: 1, third.Reader.Count);
		Assert.AreEqual(expected: "two", await third.Reader.ReadAsync());
		Assert.AreEqual(expected: 0, CreateChannel(storage).Reader.Count);
	}

	[TestMethod]
	public async Task TryWriteHandsAnItemDirectlyToAnActiveReader()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		var waitingReader = channel.Reader.WaitToReadAsync().AsTask();

		while (!channel.Writer.TryWrite("direct"))
		{
			await Task.Yield();
		}

		Assert.IsTrue(await waitingReader);
		Assert.IsTrue(channel.Reader.TryRead(out var item));
		Assert.AreEqual(expected: "direct", item);
		Assert.AreEqual(expected: 0, channel.Reader.Count);
	}

	[TestMethod]
	public async Task CancellationDoesNotConsumeCapacityOrPersistAnItem()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		using var cancellation = new CancellationTokenSource();
		var waitingReader = channel.Reader.WaitToReadAsync(cancellation.Token).AsTask();
		await cancellation.CancelAsync();

		await AssertCanceled(waitingReader);
		Assert.AreEqual(expected: 0, channel.Reader.Count);

		using var writeCancellation = new CancellationTokenSource();
		await writeCancellation.CancelAsync();
		await AssertCanceled(channel.Writer.WriteAsync(item: "canceled", writeCancellation.Token).AsTask());
		await AssertCanceled(() => channel.Writer.WaitToWriteAsync(writeCancellation.Token).AsTask());
		Assert.AreEqual(expected: 0, channel.Reader.Count);
	}

	[TestMethod]
	public async Task FaultedAndCanceledCompletionArePublishedToReaders()
	{
		await using var failedStorage = new TestTransactionalStorage();
		var failedChannel = CreateChannel(failedStorage);
		var failure = new InvalidOperationException("channel failed");

		await failedChannel.Writer.WriteAsync("buffered");
		Assert.IsTrue(failedChannel.Writer.TryComplete(failure));
		Assert.IsFalse(failedChannel.Reader.Completion.IsCompleted);
		Assert.AreEqual(expected: "buffered", await failedChannel.Reader.ReadAsync());
		var completionFailure = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await failedChannel.Reader.Completion);
		var readFailure = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await failedChannel.Reader.WaitToReadAsync());
		Assert.AreSame(failure, completionFailure);
		Assert.AreSame(failure, readFailure);

		await using var canceledStorage = new TestTransactionalStorage();
		var canceledChannel = CreateChannel(canceledStorage);
		using var cancellation = new CancellationTokenSource();
		await cancellation.CancelAsync();
		Assert.IsTrue(canceledChannel.Writer.TryComplete(new OperationCanceledException(cancellation.Token)));
		await AssertCanceled(canceledChannel.Reader.Completion);
	}

	[TestMethod]
	public async Task FailedWriteAndRestoreRollBackIndexesAndAllowRetry()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		var checkpointFailure = new InvalidOperationException("checkpoint failed");
		storage.NextCheckpointException = checkpointFailure;

		var writeFailure = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await channel.Writer.WriteAsync("failed"));
		Assert.AreSame(checkpointFailure, writeFailure);
		Assert.AreEqual(expected: 0, channel.Reader.Count);

		var restoreFailure = new InvalidOperationException("restore failed");
		var failRestore = true;
		var retryChannel = new PersistentChannel<string>(
			storage,
			static (bucket, item) => bucket.Add(key: "value", item),

			// ReSharper disable once AccessToModifiedClosure
			bucket => failRestore ? throw restoreFailure : bucket.GetString("value")!);
		await retryChannel.Writer.WriteAsync("retry");

		var readFailure = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await retryChannel.Reader.ReadAsync());
		Assert.AreSame(restoreFailure, readFailure);
		Assert.AreEqual(expected: 1, retryChannel.Reader.Count);

		failRestore = false;
		Assert.AreEqual(expected: "retry", await retryChannel.Reader.ReadAsync());
		Assert.AreEqual(expected: 0, retryChannel.Reader.Count);
	}

	private static PersistentChannel<string> CreateChannel(ITransactionalStorage storage) =>
		new(
			storage,
			static (bucket, item) => bucket.Add(key: "value", item),
			static bucket => bucket.GetString("value")!);

	private static Task AssertCanceled(Task task) => AssertCanceled(() => task);

	private static async Task AssertCanceled(Func<Task> operation)
	{
		try
		{
			await operation();
			Assert.Fail("The operation was not canceled.");
		}
		catch (OperationCanceledException) { }
	}

	private sealed class TestTransactionalStorage : ITransactionalStorage
	{
		private readonly InMemoryStorage _storage = new(writeOnly: false);

		private bool _blockCheckpoint;

		private TaskCompletionSource CheckpointEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		private TaskCompletionSource CheckpointRelease { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public Exception? NextCheckpointException { private get; set; }

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			Dispose();

			return ValueTask.CompletedTask;
		}

	#endregion

	#region Interface IDisposable

		public void Dispose() => _storage.Dispose();

	#endregion

	#region Interface IStorage

		public ReadOnlyMemory<byte> Get(ReadOnlySpan<byte> key) => _storage.Get(key);

		public void Set(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value) => _storage.Set(key, value);

		public void Remove(ReadOnlySpan<byte> key) => _storage.Remove(key);

		public void RemoveAll(ReadOnlySpan<byte> prefix) => _storage.RemoveAll(prefix);

	#endregion

	#region Interface ITransactionalStorage

		public async ValueTask CheckPoint(int level)
		{
			if (_blockCheckpoint)
			{
				_blockCheckpoint = false;
				CheckpointEntered.SetResult();
				await CheckpointRelease.Task;
			}

			if (NextCheckpointException is { } exception)
			{
				NextCheckpointException = null;

				throw exception;
			}
		}

		public ValueTask Shrink() => ValueTask.CompletedTask;

	#endregion
	}
}
