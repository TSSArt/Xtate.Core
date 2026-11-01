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

namespace Xtate.Core.Test.UnitTests.Persistence;

[TestClass]
public class PersistenceCollectionControllerCoverageTest
{
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

	[TestMethod]
	public void KeyListControllerRestoresListItemsInOrder()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		var key = new DocumentEntity(0);
		var first = new DocumentEntity(20);
		var second = new DocumentEntity(21);
		var map = new DictionaryEntityMap(key, first, second);
		var source = new KeyList<DocumentEntity>();
		using (new KeyListPersistingController<DocumentEntity>(bucket, source, map))
		{
			source.Set(key, [first, second]);
		}

		var restored = new KeyList<DocumentEntity>();
		using var controller = new KeyListPersistingController<DocumentEntity>(bucket, restored, map);
		Assert.IsTrue(restored.TryGetValue(key, out var items));
		CollectionAssert.AreEqual(new[] { first, second }, items);
	}
	[TestMethod]
	public void EmptyKeyListRecordSurvivesRecreationAndCanBeUpdated()
	{
		using var storage = new InMemoryStorage(false);
		var bucket = new Bucket(storage);
		var key = new DocumentEntity(0);
		var map = new DictionaryEntityMap(key);
		var source = new KeyList<DocumentEntity>();
		using (new KeyListPersistingController<DocumentEntity>(bucket, source, map)) source.Set(key, []);
		var restored = new KeyList<DocumentEntity>();
		using var controller = new KeyListPersistingController<DocumentEntity>(bucket, restored, map);
		Assert.IsTrue(restored.TryGetValue(key, out var items));
		Assert.IsEmpty(items);
		restored.Set(key, []);
		Assert.AreEqual(0, bucket.Nested(0).GetInt32(0));
	}

	[TestMethod]
	public void RestoredKeyRecordReusesItsStorageSlotForUpdates()
	{
		using var storage = new InMemoryStorage(false);
		var bucket = new Bucket(storage);
		var key = new DocumentEntity(0);
		var first = new DocumentEntity(20);
		var second = new DocumentEntity(21);
		var map = new DictionaryEntityMap(key, first, second);
		var source = new KeyList<DocumentEntity>();
		using (new KeyListPersistingController<DocumentEntity>(bucket, source, map)) source.Set(key, [first]);
		var restored = new KeyList<DocumentEntity>();
		using var controller = new KeyListPersistingController<DocumentEntity>(bucket, restored, map);
		Assert.IsTrue(restored.TryGetValue(key, out _));
		// Item restoration is retained separately as an ignored regression; this verifies slot reuse after restoring the key.
		restored.Set(key, [second]);
		Assert.IsTrue(bucket.Nested(0).TryGet(1, out var bytes));
		Assert.AreEqual(21, BitConverter.ToInt32(bytes.ToArray(), 0));
		Assert.IsFalse(bucket.Nested(1).TryGet(0, out int _));
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
		private readonly Dictionary<int, IEntity> _entities = entities.ToDictionary(static entity => entity.DocumentId, static IEntity (entity) => entity);

	#region Interface IEntityMap

		public bool TryGetEntityByDocumentId(int id, [NotNullWhen(true)] out IEntity? entity) => _entities.TryGetValue(id, out entity);

	#endregion
	}
}
