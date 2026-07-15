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

using Xtate.DataTypes;
using Xtate.Persistence;
using Xtate.Persistence.Services;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class DataModelListPersistingControllerCoverageTest
{
	[TestMethod]
	public void RecordsAndRestoresAppendKeySetIndexInsertRemoveLengthMetadataAndReferences()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var root = new Bucket(storage);
		var listBucket = root.Nested("list");
		var referencesBucket = root.Nested("references");
		var source = new DataModelList();

		using (var tracker = new DataModelReferenceTracker(referencesBucket))
		using (var controller = new DataModelListPersistingController(listBucket, tracker, source))
		{
			source.Add("zero");
			source["name", caseInsensitive: false] = "case-sensitive";
			source["NAME", caseInsensitive: true] = "case-insensitive";
			source[0] = new DataModelList { ["nested"] = "changed" };
			source.Insert(index: 1, new DataModelValue("inserted"));
			source.SetMetadata(new DataModelList { ["root"] = "metadata" });
			source.SetLength(source.Count + 2);
			source.RemoveAt(index: 1);
			source.SetLength(length: 2);
		}

		var restored = new DataModelList();

		using (var tracker = new DataModelReferenceTracker(referencesBucket))
		using (var controller = new DataModelListPersistingController(listBucket, tracker, restored))
		{
			Assert.AreEqual(expected: 2, restored.Count);
			Assert.AreEqual("changed", restored[0].AsList()["nested"].AsString());
			Assert.AreEqual("case-insensitive", restored["name", caseInsensitive: true].AsString());
			Assert.AreEqual("metadata", restored.GetMetadata()!["root"].AsString());
		}
	}

	[TestMethod]
	public void ConstructorRejectsNullTrackerAndList()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		using var tracker = new DataModelReferenceTracker(bucket.Nested("references"));

		Assert.ThrowsExactly<ArgumentNullException>(
			[ExcludeFromCodeCoverage] () => new DataModelListPersistingController(bucket, referenceTracker: null!, new DataModelList()));
		Assert.ThrowsExactly<ArgumentNullException>(
			[ExcludeFromCodeCoverage] () => new DataModelListPersistingController(bucket, tracker, dataModelList: null!));
	}
}
