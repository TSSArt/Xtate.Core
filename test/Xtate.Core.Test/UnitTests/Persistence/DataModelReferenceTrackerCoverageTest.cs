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
public class DataModelReferenceTrackerCoverageTest
{
	[TestMethod]
	public void TracksRestoresReusesAndRemovesListReferences()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage).Nested("references");
		var original = new DataModelList(caseInsensitive: true) { ["name"] = "original" };
		int refId;

		using (var tracker = new DataModelReferenceTracker(bucket))
		{
			refId = tracker.GetRefId(new DataModelValue(original));
			Assert.AreSame(original, tracker.GetValue(refId, DataModelValueType.List, baseList: null));
			Assert.AreSame(original, tracker.GetValue(refId, DataModelValueType.List, original));
			tracker.AddReference(new DataModelValue(original));
			tracker.AddReference(new DataModelValue(original));
			tracker.RemoveReference(new DataModelValue(original));
			var removed = new DataModelList { ["temporary"] = true };
			tracker.AddReference(new DataModelValue(removed));
			tracker.RemoveReference(new DataModelValue(removed));
			tracker.RemoveReference(new DataModelValue(removed));
		}

		using (var restoredTracker = new DataModelReferenceTracker(bucket))
		{
			var restored = (DataModelList) restoredTracker.GetValue(refId, DataModelValueType.List, baseList: null);
			Assert.IsTrue(restored.CaseInsensitive);
			Assert.AreEqual("original", restored["name"].AsString());
			Assert.AreSame(restored, restoredTracker.GetValue(refId, DataModelValueType.List, baseList: null));
		}

		using (var baseTracker = new DataModelReferenceTracker(bucket))
		{
			var supplied = new DataModelList(caseInsensitive: true);
			Assert.AreSame(supplied, baseTracker.GetValue(refId, DataModelValueType.List, supplied));
			Assert.AreEqual("original", supplied["name"].AsString());
			Assert.ThrowsExactly<InvalidOperationException>(
				[ExcludeFromCodeCoverage] () => baseTracker.GetValue(refId, DataModelValueType.List, new DataModelList()));
		}
	}

	[TestMethod]
	public void IgnoresNonListReferenceMutationsAndRejectsUnsupportedReferenceTypes()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		using var tracker = new DataModelReferenceTracker(new Bucket(storage));
		var scalar = new DataModelValue("scalar");

		tracker.AddReference(scalar);
		tracker.RemoveReference(scalar);
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage] () => tracker.GetRefId(scalar));
		Assert.ThrowsExactly<InvalidOperationException>(
			[ExcludeFromCodeCoverage] () => tracker.GetValue(refId: 0, DataModelValueType.String, baseList: null));
		Assert.ThrowsExactly<InvalidOperationException>(
			[ExcludeFromCodeCoverage] () => tracker.GetValue(refId: 0, DataModelValueType.String, new DataModelList()));
	}
}
