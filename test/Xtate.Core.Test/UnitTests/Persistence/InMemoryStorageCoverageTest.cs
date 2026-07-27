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

// ReSharper disable AccessToDisposedClosure

using Xtate.Persistence.Services;

namespace Xtate.Core.Test.UnitTests.Persistence;

[TestClass]
public class InMemoryStorageCoverageTest
{
	[TestMethod]
	public void DataSnapshotAndBaselineRoundTripReplacementRemovalAndPrefixDeletion()
	{
		using var storage = new TestInMemoryStorage(writeOnly: false);
		storage.Set([1, 1], [10]);
		storage.Set([2], [20]);
		storage.Set([1, 1], [11, 12]);
		storage.Remove([2]);
		storage.Set([9, 1], [91]);
		storage.Set([9, 2], [92]);
		storage.Set([8], [80]);
		storage.RemoveAll([9]);

		CollectionAssert.AreEqual(new byte[] { 11, 12 }, storage.Get([1, 1]).ToArray());
		Assert.IsTrue(storage.Get([2]).IsEmpty);
		Assert.IsTrue(storage.Get([9, 1]).IsEmpty);
		Assert.IsTrue(storage.Get([9, 2]).IsEmpty);
		CollectionAssert.AreEqual(new byte[] { 80 }, storage.Get([8]).ToArray());

		var data = new byte[storage.GetDataSize()];
		storage.WriteDataToSpan(data, shrink: false);
		using var restored = new InMemoryStorage(data);
		CollectionAssert.AreEqual(new byte[] { 11, 12 }, restored.Get([1, 1]).ToArray());
		CollectionAssert.AreEqual(new byte[] { 80 }, restored.Get([8]).ToArray());

		var compacted = new byte[storage.GetDataSize()];
		storage.WriteDataToSpan(compacted, shrink: true);
		CollectionAssert.AreEqual(data, compacted);
		storage.RemoveAll([]);
		Assert.AreEqual(expected: 0, storage.GetDataSize());

		// Explicit disposal verifies the protected virtual disposal hook before the using scope exits.
		// ReSharper disable once DisposeOnUsingVariable
		storage.Dispose();
		Assert.IsTrue(storage.DisposeCalled);
	}

	[TestMethod]
	public void TransactionLogCanBeCopiedWithoutOrWithTruncationAndUsedAsBaseline()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		storage.Set([1], [10]);
		storage.Set([2], [20]);
		storage.Remove([1]);
		var size = storage.GetTransactionLogSize();
		var log = new byte[size];

		storage.WriteTransactionLogToSpan(log, truncateLog: false);
		Assert.AreEqual(size, storage.GetTransactionLogSize());
		using var restored = new InMemoryStorage(log);
		Assert.IsTrue(restored.Get([1]).IsEmpty);
		CollectionAssert.AreEqual(new byte[] { 20 }, restored.Get([2]).ToArray());

		var secondCopy = new byte[size];
		storage.WriteTransactionLogToSpan(secondCopy, truncateLog: true);
		CollectionAssert.AreEqual(log, secondCopy);
		Assert.AreEqual(expected: 0, storage.GetTransactionLogSize());

		// Explicit calls cover idempotent disposal before the using scope exits.
		// ReSharper disable once DisposeOnUsingVariable
		storage.Dispose();

		// ReSharper disable once DisposeOnUsingVariable
		storage.Dispose();
	}

	[TestMethod]
	public void StorageRejectsEmptyKeysAndReadOperationsInWriteOnlyMode()
	{
		using var writeOnly = new InMemoryStorage();

		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => writeOnly.Set([], [1]));
		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => writeOnly.Remove([]));
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => writeOnly.Get([1]));
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => writeOnly.GetDataSize());
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => writeOnly.WriteDataToSpan([]));
	}

	private sealed class TestInMemoryStorage(bool writeOnly) : InMemoryStorage(writeOnly)
	{
		public bool DisposeCalled { get; private set; }

		protected override void Dispose(bool disposing)
		{
			DisposeCalled = true;

			base.Dispose(disposing);
		}
	}
}