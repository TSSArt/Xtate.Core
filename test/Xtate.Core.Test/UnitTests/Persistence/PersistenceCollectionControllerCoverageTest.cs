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

using Xtate.Interpreter;
using Xtate.Interpreter.Internal;
using Xtate.Persistence;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class PersistenceCollectionControllerCoverageTest
{
	[TestMethod]
	public void InvokeIdSetControllerPersistsAddsRemovalsAndFinalClear()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		var first = InvokeId.FromString(invokeId: "first", uniqueInvokeId: "unique-first");
		var second = InvokeId.FromString(invokeId: "second", uniqueInvokeId: "unique-second");
		var source = new InvokeIdSet();

		using (new InvokeIdSetPersistingController(bucket, source))
		{
			source.Add(first);
			source.Add(second);
		}

		var restored = new InvokeIdSet();

		using (new InvokeIdSetPersistingController(bucket, restored))
		{
			Assert.AreEqual(expected: 2, restored.Count);
			Assert.IsTrue(restored.Contains(first));
			Assert.IsTrue(restored.Contains(second));

			restored.Remove(first);
			restored.Remove(second);
		}

		var empty = new InvokeIdSet();
		using var emptyController = new InvokeIdSetPersistingController(bucket, empty);
		Assert.AreEqual(expected: 0, empty.Count);
	}

	[TestMethod]
	public void InvokeIdSetControllerRestoresAfterPersistedNonFinalRemoval()
	{
		// Current product defect: constructor compaction at InvokeIdSetPersistingController.cs writes the surviving
		// InvokeId with the unsupported generic converter. Keep this exact round-trip disabled until AddId is used there.
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		var first = InvokeId.FromString(invokeId: "first", uniqueInvokeId: "unique-first");
		var second = InvokeId.FromString(invokeId: "second", uniqueInvokeId: "unique-second");
		var source = new InvokeIdSet();

		using (new InvokeIdSetPersistingController(bucket, source))
		{
			source.Add(first);
			source.Add(second);
			source.Remove(first);
		}

		var restored = new InvokeIdSet();
		using var controller = new InvokeIdSetPersistingController(bucket, restored);
		Assert.AreEqual(expected: 1, restored.Count);
		Assert.IsTrue(restored.Contains(second));
	}

	[TestMethod]
	public void EntityQueueControllerRoundTripsStoredEntitiesAndCompactsAfterDequeues()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		var source = new EntityQueue<StoredEntity>();

		using (new EntityQueuePersistingController<StoredEntity>(bucket, source, CreateStoredEntity))
		{
			source.Enqueue(new StoredEntity("first"));
			source.Enqueue(new StoredEntity("second"));
			source.Enqueue(new StoredEntity("third"));
			Assert.AreEqual(expected: "first", source.Dequeue().Value);
		}

		var restored = new EntityQueue<StoredEntity>();

		using (new EntityQueuePersistingController<StoredEntity>(bucket, restored, CreateStoredEntity))
		{
			Assert.HasCount(expected: 2, restored);
			Assert.AreEqual(expected: "second", restored.Dequeue().Value);
			Assert.AreEqual(expected: "third", restored.Dequeue().Value);
		}

		var empty = new EntityQueue<StoredEntity>();
		using var emptyController = new EntityQueuePersistingController<StoredEntity>(bucket, empty, CreateStoredEntity);
		Assert.IsEmpty(empty);
	}

	[TestMethod]
	public void KeyListControllerWritesAndUpdatesAStableRecord()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		var keyList = new KeyList<DocumentEntity>();
		var key = new DocumentEntity(documentId: 0);
		var first = new DocumentEntity(documentId: 20);
		var second = new DocumentEntity(documentId: 21);
		var entityMap = new DictionaryEntityMap(key, first, second);

		using (new KeyListPersistingController<DocumentEntity>(bucket, keyList, entityMap))
		{
			keyList.Set(key, [first, second]);
			keyList.Set(key, [second]);
		}

		Assert.IsTrue(bucket.Nested(0).TryGet(key: 0, out int storedKeyId));
		Assert.AreEqual(expected: 0, storedKeyId);
		Assert.IsTrue(bucket.Nested(0).TryGet(key: 1, out var storedList));
		Assert.AreEqual(sizeof(int), storedList.Length);
	}

	[TestMethod]
	public void OrderedSetControllerCompactsInitialItemsRestoresDeletesAndClears()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		var first = new DocumentEntity(documentId: 30);
		var second = new DocumentEntity(documentId: 31);
		var map = new DictionaryEntityMap(first, second);
		var source = new OrderedSet<DocumentEntity> { first };

		using (new OrderedSetPersistingController<DocumentEntity>(bucket, source, map))
		{
			source.Add(second);
		}

		var restored = new OrderedSet<DocumentEntity>();

		using (new OrderedSetPersistingController<DocumentEntity>(bucket, restored, map))
		{
			CollectionAssert.AreEqual(new[] { first, second }, restored.ToArray());
			restored.Delete(first);
			restored.Delete(second);
			restored.Add(first);
			restored.Clear();
		}

		var empty = new OrderedSet<DocumentEntity>();
		using var emptyController = new OrderedSetPersistingController<DocumentEntity>(bucket, empty, map);
		Assert.IsEmpty(empty);
	}

	private static StoredEntity CreateStoredEntity(Bucket bucket) => new(bucket.GetString(key: 0)!);

	private sealed class StoredEntity(string value) : IStoreSupport
	{
		public string Value { get; } = value;

	#region Interface IStoreSupport

		public void Store(Bucket bucket) => bucket.Add(key: 0, Value);

	#endregion
	}

	private sealed class DocumentEntity(int documentId) : IEntity, IDocumentId
	{
	#region Interface IDocumentId

		public int DocumentId { get; } = documentId;

	#endregion
	}

	private sealed class DictionaryEntityMap(params DocumentEntity[] entities) : IEntityMap
	{
		private readonly Dictionary<int, IEntity> _entities = entities.ToDictionary(static entity => entity.DocumentId, static entity => (IEntity) entity);

	#region Interface IEntityMap

		public bool TryGetEntityByDocumentId(int id, [NotNullWhen(true)] out IEntity? entity) => _entities.TryGetValue(id, out entity);

	#endregion
	}
}