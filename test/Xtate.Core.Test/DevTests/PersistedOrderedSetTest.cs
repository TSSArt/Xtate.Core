#region Copyright © 2019-2021 Sergii Artemenko

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

#endregion

using Xtate.Core;
using Xtate.Persistence;

namespace Xtate.Test
{
	[TestClass]
	public class PersistedOrderedSetTest
	{
		private Bucket                               _bucket;
		private ImmutableDictionary<int, IEntity>    _map                  = default!;
		private Node                                 _node1                = default!;
		private Node                                 _node2                = default!;
		private Node                                 _node3                = default!;
		private OrderedSetPersistingController<Node> _orderedSetController = default!;
		private OrderedSet<Node>                     _restoredOrderedSet   = default!;
		private OrderedSet<Node>                     _sourceSet            = default!;
		private InMemoryStorage                      _storage              = default!;

		[TestInitialize]
		public void Initialize()
		{
			_storage = new InMemoryStorage(false);
			_bucket = new Bucket(_storage);
			_node1 = new Node(1);
			_node2 = new Node(2);
			_node3 = new Node(3);
			_map = ImmutableDictionary.CreateRange(new Dictionary<int, IEntity>
												   {
													   { 1, _node1 },
													   { 2, _node2 },
													   { 3, _node3 }
												   });

			_sourceSet = [];
			_restoredOrderedSet = [];
			_orderedSetController = new OrderedSetPersistingController<Node>(_bucket, _sourceSet, _map);
		}

		[TestCleanup]
		public void Finalization()
		{
			_orderedSetController.Dispose();
		}

		[TestMethod]
		public void EmptyTest()
		{
			using var restoredController = new OrderedSetPersistingController<Node>(_bucket, _restoredOrderedSet, _map);

			Assert.IsTrue(_restoredOrderedSet.IsEmpty);

			Assert.AreEqual(expected: 0, _storage.GetTransactionLogSize());
		}

		[TestMethod]
		public void ClearTest()
		{
			_sourceSet.AddIfNotExists(_node1);

			Assert.IsFalse(_sourceSet.IsEmpty);
			_sourceSet.Clear();
			Assert.IsTrue(_sourceSet.IsEmpty);

			using var restoredController = new OrderedSetPersistingController<Node>(_bucket, _restoredOrderedSet, _map);

			Assert.IsTrue(_restoredOrderedSet.IsEmpty);
		}

		[TestMethod]
		public void AddTest()
		{
			_sourceSet.AddIfNotExists(_node1);
			_sourceSet.AddIfNotExists(_node2);
			_sourceSet.AddIfNotExists(_node3);

			Assert.IsFalse(_sourceSet.IsEmpty);

			using var restoredController = new OrderedSetPersistingController<Node>(_bucket, _restoredOrderedSet, _map);

			Assert.IsTrue(_restoredOrderedSet.IsMember(_node1));
			Assert.IsTrue(_restoredOrderedSet.IsMember(_node2));
			Assert.IsTrue(_restoredOrderedSet.IsMember(_node3));
		}

		[TestMethod]
		public void RemoveTest()
		{
			_sourceSet.AddIfNotExists(_node1);
			_sourceSet.AddIfNotExists(_node2);
			_sourceSet.AddIfNotExists(_node3);
			_sourceSet.Delete(_node2);

			Assert.IsFalse(_sourceSet.IsEmpty);

			using var restoredController = new OrderedSetPersistingController<Node>(_bucket, _restoredOrderedSet, _map);

			Assert.IsTrue(_restoredOrderedSet.IsMember(_node1));
			Assert.IsFalse(_restoredOrderedSet.IsMember(_node2));
			Assert.IsTrue(_restoredOrderedSet.IsMember(_node3));
		}

		[TestMethod]
		public void DeleteAllTest()
		{
			_sourceSet.AddIfNotExists(_node1);
			_sourceSet.AddIfNotExists(_node2);
			_sourceSet.AddIfNotExists(_node3);
			_sourceSet.Delete(_node1);
			_sourceSet.Delete(_node2);
			_sourceSet.Delete(_node3);

			Assert.IsTrue(_sourceSet.IsEmpty);

			using var restoredController = new OrderedSetPersistingController<Node>(_bucket, _restoredOrderedSet, _map);

			Assert.IsFalse(_restoredOrderedSet.IsMember(_node1));
			Assert.IsFalse(_restoredOrderedSet.IsMember(_node2));
			Assert.IsFalse(_restoredOrderedSet.IsMember(_node3));
		}

		[TestMethod]
		public void ShrinkTest()
		{
			_sourceSet.AddIfNotExists(_node1);
			_sourceSet.AddIfNotExists(_node2);
			_sourceSet.AddIfNotExists(_node3);
			_sourceSet.Delete(_node1);
			_sourceSet.Delete(_node2);

			Assert.IsFalse(_sourceSet.IsEmpty);

			using var restoredController = new OrderedSetPersistingController<Node>(_bucket, _restoredOrderedSet, _map);

			Assert.IsFalse(_restoredOrderedSet.IsMember(_node1));
			Assert.IsFalse(_restoredOrderedSet.IsMember(_node2));
			Assert.IsTrue(_restoredOrderedSet.IsMember(_node3));

			var restoredSet2 = new OrderedSet<Node>();
			using var restoredController2 = new OrderedSetPersistingController<Node>(_bucket, restoredSet2, _map);

			Assert.IsFalse(restoredSet2.IsMember(_node1));
			Assert.IsFalse(restoredSet2.IsMember(_node2));
			Assert.IsTrue(restoredSet2.IsMember(_node3));
		}

		private class Node(int docId) : IEntity, IDocumentId
		{
			#region Interface IDocumentId

			public int DocumentId { get; } = docId;

			#endregion
		}
	}
}