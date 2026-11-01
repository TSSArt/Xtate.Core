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
	public async Task DirectHandoffDuringPendingRestoreDoesNotLoseEitherItem()
	{
		CollectionAssert.AreEquivalent(new[] { "stored", "direct" }, await ReadDuringCompetingHandoff());
	}

	[TestMethod]
	public async Task StoredItemIsReadBeforeALaterDirectHandoff()
	{
		CollectionAssert.AreEqual(new[] { "stored", "direct" }, await ReadDuringCompetingHandoff());
	}

	private static async Task<string[]> ReadDuringCompetingHandoff()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		await channel.Writer.WriteAsync("stored");
		storage.BlockNextCheckpoint();
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		var firstWait = channel.Reader.WaitToReadAsync(timeout.Token).AsTask();
		try
		{
			await storage.CheckpointEntered.Task.WaitAsync(timeout.Token);
			var secondWait = channel.Reader.WaitToReadAsync(timeout.Token).AsTask();
			Assert.IsTrue(channel.Writer.TryWrite("direct"));
			Assert.IsTrue(await secondWait);
		}
		finally
		{
			storage.CheckpointRelease.TrySetResult();
		}
		Assert.IsTrue(await firstWait);
		var items = new[] { await channel.Reader.ReadAsync(timeout.Token), await channel.Reader.ReadAsync(timeout.Token) };
		Assert.AreEqual(0, channel.Reader.Count);
		return items;
	}

	[TestMethod]
	public void TryWriteWithoutWaitingReaderDoesNotPersistAndClosedWriterRejectsWrites()
	{
		using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		Assert.IsFalse(channel.Writer.TryWrite("unused"));
		Assert.AreEqual(0, channel.Reader.Count);
		Assert.IsTrue(channel.Writer.TryComplete());
		Assert.ThrowsExactly<ChannelClosedException>(() => channel.Writer.TryWrite("closed"));
		Assert.IsFalse(channel.Reader.TryPeek(out _));
	}

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

	[TestMethod]
	public async Task FailedReadCheckpointRestoresStoredItemAndHeadForRetry()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		await channel.Writer.WriteAsync("preserved");
		var expected = new InvalidOperationException("read checkpoint failed");
		storage.NextCheckpointException = expected;
		var actual = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await channel.Reader.ReadAsync());
		Assert.AreSame(expected, actual);
		Assert.AreEqual(1, channel.Reader.Count);
		Assert.AreEqual("preserved", await CreateChannel(storage).Reader.ReadAsync());
	}

	[TestMethod]
	public async Task ManySequentialItemsAreReadInWriteOrder()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		var expected = Enumerable.Range(0, 100).Select(static i => $"item{i}").ToArray();

		foreach (var item in expected)
		{
			await channel.Writer.WriteAsync(item);
		}

		Assert.AreEqual(expected.Length, channel.Reader.Count);

		var actual = new List<string>();
		while (channel.Reader.Count > 0)
		{
			actual.Add(await channel.Reader.ReadAsync());
		}

		CollectionAssert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task TryReadOnEmptyChannelReturnsFalseAndKeepsChannelUsable()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);

		Assert.IsFalse(channel.Reader.TryRead(out var item));
		Assert.IsNull(item);
		Assert.AreEqual(0, channel.Reader.Count);

		await channel.Writer.WriteAsync("after");
		Assert.AreEqual("after", await channel.Reader.ReadAsync());
	}

	[TestMethod]
	public async Task ReadAsyncWaitsForAnItemWrittenLater()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

		var read = channel.Reader.ReadAsync(timeout.Token).AsTask();
		Assert.IsFalse(read.IsCompleted);

		await channel.Writer.WriteAsync("later", timeout.Token);

		Assert.AreEqual("later", await read);
		Assert.AreEqual(0, channel.Reader.Count);
	}

	[TestMethod]
	public async Task WriteAsyncHandsItemToWaitingReaderWithoutLeavingItInStorage()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

		var read = channel.Reader.ReadAsync(timeout.Token).AsTask();
		await Task.Delay(50, timeout.Token);
		await channel.Writer.WriteAsync("direct", timeout.Token);

		Assert.AreEqual("direct", await read);
		Assert.AreEqual(0, channel.Reader.Count);
		Assert.AreEqual(0, CreateChannel(storage).Reader.Count);
	}

	[TestMethod]
	public async Task CompletingEmptyChannelCompletesReaderAndRejectsWrites()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

		Assert.IsTrue(channel.Writer.TryComplete());

		await channel.Reader.Completion.WaitAsync(timeout.Token);
		Assert.IsFalse(await channel.Reader.WaitToReadAsync(timeout.Token));
		Assert.IsFalse(channel.Reader.TryRead(out _));
		await Assert.ThrowsExactlyAsync<ChannelClosedException>(async () => await channel.Writer.WriteAsync("closed", timeout.Token));
	}

	[TestMethod]
	public async Task CompletionWakesAWaitingReader()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

		var waiting = channel.Reader.WaitToReadAsync(timeout.Token).AsTask();
		await Task.Delay(50, timeout.Token);
		Assert.IsFalse(waiting.IsCompleted);

		Assert.IsTrue(channel.Writer.TryComplete());

		Assert.IsFalse(await waiting);
		await channel.Reader.Completion.WaitAsync(timeout.Token);
	}

	[TestMethod]
	public async Task FaultedCompletionWakesAWaitingReaderWithTheError()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		var failure = new InvalidOperationException("faulted");

		var waiting = channel.Reader.WaitToReadAsync(timeout.Token).AsTask();
		await Task.Delay(50, timeout.Token);

		Assert.IsTrue(channel.Writer.TryComplete(failure));

		var actual = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await waiting);
		Assert.AreSame(failure, actual);
	}

	[TestMethod]
	public async Task ItemsWrittenBeforeCompletionAreStillDelivered()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

		await channel.Writer.WriteAsync("one", timeout.Token);
		await channel.Writer.WriteAsync("two", timeout.Token);
		Assert.IsTrue(channel.Writer.TryComplete());

		Assert.IsTrue(await channel.Reader.WaitToReadAsync(timeout.Token));
		Assert.AreEqual("one", await channel.Reader.ReadAsync(timeout.Token));
		Assert.AreEqual("two", await channel.Reader.ReadAsync(timeout.Token));
		Assert.IsFalse(await channel.Reader.WaitToReadAsync(timeout.Token));
		await channel.Reader.Completion.WaitAsync(timeout.Token);
	}

	[TestMethod]
	public async Task PartiallyConsumedQueueResumesInOrderAfterRepeatedRecreation()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);

		for (var i = 0; i < 5; i++)
		{
			await channel.Writer.WriteAsync($"item{i}");
		}

		Assert.AreEqual("item0", await channel.Reader.ReadAsync());
		Assert.AreEqual("item1", await channel.Reader.ReadAsync());

		channel = CreateChannel(storage);
		Assert.AreEqual(3, channel.Reader.Count);
		await channel.Writer.WriteAsync("item5");
		Assert.AreEqual("item2", await channel.Reader.ReadAsync());

		channel = CreateChannel(storage);
		Assert.AreEqual(3, channel.Reader.Count);

		var rest = new List<string>();
		while (channel.Reader.Count > 0)
		{
			rest.Add(await channel.Reader.ReadAsync());
		}

		CollectionAssert.AreEqual(new[] { "item3", "item4", "item5" }, rest);
	}

	[TestMethod]
	public async Task ConcurrentWritersAndReaderDeliverEveryItemExactlyOnceInPerWriterOrder()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		const int writers = 4;
		const int perWriter = 50;

		var reader = Task.Run(async () =>
							  {
								  var received = new List<string>();
								  while (received.Count < writers * perWriter)
								  {
									  received.Add(await channel.Reader.ReadAsync(timeout.Token));
								  }

								  return received;
							  }, timeout.Token);

		await Task.WhenAll(Enumerable.Range(0, writers).Select(w => Task.Run(async () =>
																			 {
																				 for (var i = 0; i < perWriter; i++)
																				 {
																					 await channel.Writer.WriteAsync($"{w}:{i}", timeout.Token);
																				 }
																			 }, timeout.Token)));

		var received = await reader;

		Assert.HasCount(writers * perWriter, received);
		CollectionAssert.AllItemsAreUnique(received);

		for (var w = 0; w < writers; w++)
		{
			var prefix = $"{w}:";
			var order = received.Where(s => s.StartsWith(prefix, StringComparison.Ordinal)).Select(s => int.Parse(s.Substring(prefix.Length))).ToArray();
			CollectionAssert.AreEqual(Enumerable.Range(0, perWriter).ToArray(), order);
		}

		Assert.AreEqual(0, channel.Reader.Count);
	}

	[TestMethod]
	public async Task ConcurrentReadersEachItemIsDeliveredToExactlyOneReader()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		const int total = 200;

		var readers = Enumerable.Range(0, 4).Select(_ => Task.Run(async () =>
																  {
																	  var received = new List<string>();
																	  while (await channel.Reader.WaitToReadAsync(timeout.Token))
																	  {
																		  if (channel.Reader.TryRead(out var item))
																		  {
																			  received.Add(item);
																		  }
																	  }

																	  return received;
																  }, timeout.Token)).ToArray();

		for (var i = 0; i < total; i++)
		{
			await channel.Writer.WriteAsync($"item{i}", timeout.Token);
		}

		Assert.IsTrue(channel.Writer.TryComplete());

		var all = (await Task.WhenAll(readers)).SelectMany(static r => r).ToList();

		Assert.HasCount(total, all);
		CollectionAssert.AllItemsAreUnique(all);
		await channel.Reader.Completion.WaitAsync(timeout.Token);
	}

	[TestMethod]
	public async Task StoredItemsStayAheadOfADirectHandoffWhileOnlyTheFirstIsBeingRestored()
	{
		await using var storage = new TestTransactionalStorage();
		var channel = CreateChannel(storage);
		await channel.Writer.WriteAsync("stored1");
		await channel.Writer.WriteAsync("stored2");
		storage.BlockNextCheckpoint();
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

		var first = channel.Reader.WaitToReadAsync(timeout.Token).AsTask();
		var directWrite = Task.CompletedTask;
		try
		{
			await storage.CheckpointEntered.Task.WaitAsync(timeout.Token);
			_ = channel.Reader.WaitToReadAsync(timeout.Token).AsTask();
			_ = channel.Reader.WaitToReadAsync(timeout.Token).AsTask();
			await Task.Delay(50, timeout.Token);
			directWrite = channel.Writer.WriteAsync("direct", timeout.Token).AsTask();
		}
		finally
		{
			storage.CheckpointRelease.TrySetResult();
		}

		await directWrite;
		Assert.IsTrue(await first);
		var items = new[]
					{
						await channel.Reader.ReadAsync(timeout.Token),
						await channel.Reader.ReadAsync(timeout.Token),
						await channel.Reader.ReadAsync(timeout.Token)
					};
		CollectionAssert.AreEqual(new[] { "stored1", "stored2", "direct" }, items);
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

		public TaskCompletionSource CheckpointEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public TaskCompletionSource CheckpointRelease { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public void BlockNextCheckpoint() => _blockCheckpoint = true;

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
