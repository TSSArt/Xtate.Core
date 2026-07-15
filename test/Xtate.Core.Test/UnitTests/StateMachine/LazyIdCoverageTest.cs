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
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.StateMachine;

[TestClass]
public class LazyIdCoverageTest
{
	[TestMethod]
	public void LazyIdGeneratesValueOnceAndExposesDataModelRepresentations()
	{
		var lazyId = new TestLazyId();
		var hash = lazyId.GetHashCode();

		var value = lazyId.Value;

		Assert.AreEqual(expected: 1, lazyId.GenerateCount);
		Assert.AreEqual($"generated-{hash:x8}", value);
		Assert.AreSame(value, lazyId.Value);
		Assert.AreEqual(value, lazyId.ToString());
		Assert.AreEqual(hash, lazyId.GetHashCode());
		Assert.AreEqual(value, ((IObject) lazyId).ToObject());
		Assert.AreEqual(value, ((ILazyValue) lazyId).Value.AsString());
		Assert.AreEqual(value, lazyId.ToDataModelValue().AsString());

		DataModelValue implicitValue = lazyId;
		Assert.AreEqual(value, implicitValue.AsString());
	}

	[TestMethod]
	public void LazyIdEqualityRequiresSameConcreteTypeAndMaterializedIds()
	{
		var left = new TestLazyId("id-0000002a");
		var same = new TestLazyId("id-0000002a");
		var different = new TestLazyId("id-0000002b");
		var otherType = new OtherLazyId("id-0000002a");
		var unmaterialized = new TestLazyId();

		Assert.IsTrue(left.Equals(left));
		Assert.IsTrue(left.Equals(same));
		Assert.IsTrue(left == same);
		Assert.IsFalse(left != same);
		Assert.IsFalse(left.Equals(different));
		Assert.IsFalse(left == different);
		Assert.IsTrue(left != different);
		Assert.IsFalse(left.Equals(otherType));
		Assert.IsFalse(left.Equals(null));
		Assert.IsFalse(unmaterialized.Equals(new TestLazyId()));
		Assert.AreEqual(expected: 42, left.GetHashCode());
		Assert.AreEqual(expected: "id-0000002a", left.ToString());
	}

	[TestMethod]
	[SuppressMessage("ReSharper", "StringLiteralTypo")]
	public void LazyIdParsesTrailingHexHashAndFallsBackWhenItCannot()
	{
		Assert.IsTrue(TestLazyId.TryReadHash(id: "prefix-7fffffff", out var lowerHash));
		Assert.AreEqual(int.MaxValue, lowerHash);
		Assert.IsTrue(TestLazyId.TryReadHash(id: "prefix-FFFFFFFF", out var upperHash));
		Assert.AreEqual(expected: -1, upperHash);
		Assert.IsFalse(TestLazyId.TryReadHash(id: "short", out var shortHash));
		Assert.AreEqual(expected: 0, shortHash);
		Assert.IsFalse(TestLazyId.TryReadHash(id: "prefix-xxxxxxxz", out var invalidHash));
		Assert.AreEqual(expected: 0, invalidHash);
		Assert.AreEqual("not-a-hex-tail".GetHashCode(), new TestLazyId("not-a-hex-tail").GetHashCode());
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => new TestLazyId(null!));
	}

	private sealed class TestLazyId : LazyId
	{
		public TestLazyId() { }

		public TestLazyId(string id) : base(id) { }

		public int GenerateCount { get; private set; }

		public static bool TryReadHash(string id, out int hash) => TryGetHashFromId(id, out hash);

		protected override string GenerateId()
		{
			GenerateCount ++;

			return $"generated-{GetHashCode():x8}";
		}
	}

	private sealed class OtherLazyId(string id) : LazyId(id)
	{
		[ExcludeFromCodeCoverage]
		protected override string GenerateId() => $"other-{GetHashCode():x8}";
	}
}