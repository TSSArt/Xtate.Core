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
using Xtate.Interpreter;
using Xtate.IoC;
using Xtate.Persistence;
using Xtate.Persistence.DependencyInjection;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Core.Test.UnitTests.Persistence;

[TestClass]
public class PersistentEventQueueCoverageTest
{
	[TestMethod]
	public void SynchronousQueueDisposalOwnsTheTransactionalStorage()
	{
		var storage = new TrackingStorage();
		var queue = new PersistentEventQueue(storage);
		queue.Dispose();
		Assert.AreEqual(1, storage.DisposeCount);
	}

	[TestMethod]
	public async Task PersistenceModuleReplacesTheInMemoryEventQueue()
	{
		await using var container = Container.Create<PersistenceModule>();

		var reader = await container.GetRequiredService<IEventReader>();
		var dispatcher = await container.GetRequiredService<IEventDispatcher>();

		Assert.IsInstanceOfType<PersistentEventQueue>(reader);
		Assert.AreSame<object>(reader, dispatcher);
	}

	[TestMethod]
	public async Task IncomingEventsAreRestoredAfterQueueRecreation()
	{
		await using var container = Container.Create<PersistenceModule>();
		var storageManager = await container.GetRequiredService<StorageManager>();
		var expected = new IncomingEvent(new EventEntity("persisted.event"))
					   {
						   Type = EventType.External,
						   Data = "payload"
					   };

		await using (var firstQueue = new PersistentEventQueue(await storageManager.Factory(StorageType.StateMachineIncomingEvents)))
		{
			await firstQueue.Dispatch(expected, CancellationToken.None);
		}

		await using var resumedQueue = new PersistentEventQueue(await storageManager.Factory(StorageType.StateMachineIncomingEvents));

		Assert.IsTrue(await resumedQueue.WaitToEvent());
		Assert.IsTrue(resumedQueue.TryReadEvent(out var actual));
		Infra.NotNull(actual);
		Assert.AreEqual(expected.Name, actual.Name);
		Assert.AreEqual(expected.Type, actual.Type);
		Assert.AreEqual(expected.Data, actual.Data);
		Assert.IsFalse(resumedQueue.TryReadEvent(out _));
	}

	[TestMethod]
	public async Task IncomingEventsWithAnotherPersistenceFormatAreNormalized()
	{
		await using var container = Container.Create<PersistenceModule>();
		var storageManager = await container.GetRequiredService<StorageManager>();
		var expected = new IncompatiblePersistedIncomingEvent();

		await using (var firstQueue = new PersistentEventQueue(await storageManager.Factory(StorageType.StateMachineIncomingEvents)))
		{
			await firstQueue.Dispatch(expected, CancellationToken.None);
		}

		await using var resumedQueue = new PersistentEventQueue(await storageManager.Factory(StorageType.StateMachineIncomingEvents));

		Assert.IsTrue(await resumedQueue.WaitToEvent());
		Assert.IsTrue(resumedQueue.TryReadEvent(out var actual));
		Infra.NotNull(actual);
		Assert.AreEqual(expected.Name, actual.Name);
	}

	private sealed class TrackingStorage : ITransactionalStorage
	{
		private readonly InMemoryStorage _storage = new(false);
		public int DisposeCount { get; private set; }
		public ReadOnlyMemory<byte> Get(ReadOnlySpan<byte> key) => _storage.Get(key);
		public void Set(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value) => _storage.Set(key, value);
		public void Remove(ReadOnlySpan<byte> key) => _storage.Remove(key);
		public void RemoveAll(ReadOnlySpan<byte> prefix) => _storage.RemoveAll(prefix);
		public ValueTask CheckPoint(int level) => default;
		public ValueTask Shrink() => default;
		public void Dispose()
		{
			DisposeCount++;
			_storage.Dispose();
		}
		public ValueTask DisposeAsync()
		{
			Dispose();
			return default;
		}
	}

	private sealed class IncompatiblePersistedIncomingEvent() : IncomingEvent(new EventEntity("incompatible.event")), IStoreSupport
	{
	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket) => throw new AssertFailedException("The queue must use its own persistence format.");

	#endregion
	}
}
