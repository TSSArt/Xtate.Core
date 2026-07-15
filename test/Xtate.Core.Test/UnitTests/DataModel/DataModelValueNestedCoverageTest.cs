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

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class DataModelValueNestedCoverageTest
{
	[TestMethod]
	public void ObjectContainerExposesSourceAndCachesConvertedValue()
	{
		var source = new CountingObject("value");
		var container = new DataModelValue.ObjectContainer(source);

		Assert.AreSame(source, container.GetIObject());
		var first = container.Value;
		var second = container.Value;

		Assert.AreEqual(expected: "value", first.AsString());
		Assert.AreEqual(first, second);
		Assert.AreEqual(expected: 1, source.ToObjectCalls);
	}

	[TestMethod]
	public void MarkerValuesProvideStableEqualityHashingAndDistinctUndefinedNullIdentity()
	{
		var undefined = DataModelValue.Undefined;
		var anotherUndefined = default(DataModelValue);
		var nullValue = DataModelValue.Null;

		Assert.AreEqual(undefined, anotherUndefined);
		Assert.AreEqual(undefined.GetHashCode(), anotherUndefined.GetHashCode());
		Assert.AreNotEqual(undefined, nullValue);
		Assert.AreNotEqual(undefined.GetHashCode(), nullValue.GetHashCode());
		Assert.AreEqual(nullValue, DataModelValue.Null);
	}

	[TestMethod]
	public void NumberAndDateTimeNestedValuesCoverAllRepresentationsCachedAndNoncachedOffsets() => AssertNumberAndDateTimeNestedValues(assertCrossRepresentationHash: true);

	[TestMethod]
	public void NumberAndDateTimeNestedValuesCoverRepresentationsWithinCurrentHashBehavior() => AssertNumberAndDateTimeNestedValues(assertCrossRepresentationHash: false);

	private static void AssertNumberAndDateTimeNestedValues(bool assertCrossRepresentationHash)
	{
		DataModelValue[] equalNumbers =
		[
			new(10),
			new(10L),
			new(10D),
			new(10M)
		];

		foreach (var number in equalNumbers)
		{
			Assert.AreEqual(equalNumbers[0], number);

			if (assertCrossRepresentationHash)
			{
				Assert.AreEqual(equalNumbers[0].GetHashCode(), number.GetHashCode());
			}
		}

		var largeDecimal = new DataModelValue(decimal.MaxValue / 10M);
		Assert.AreEqual(decimal.MaxValue / 10M, largeDecimal.AsNumber().ToDecimal());
		Assert.AreNotEqual(equalNumbers[0], largeDecimal);

		var utc = new DateTime(year: 2026, month: 3, day: 4, hour: 5, minute: 6, second: 7, DateTimeKind.Utc);
		var cachedOffset = new DateTimeOffset(utc).ToOffset(TimeSpan.FromHours(2));
		var noncachedOffset = new DateTimeOffset(utc).ToOffset(TimeSpan.FromMinutes(7));
		var utcValue = new DataModelValue(utc);
		var cachedValue = new DataModelValue(cachedOffset);
		var noncachedValue = new DataModelValue(noncachedOffset);

		Assert.AreEqual(utc, utcValue.AsDateTime().ToDateTime());
		Assert.AreEqual(cachedOffset, cachedValue.AsDateTime().ToDateTimeOffset());
		Assert.AreEqual(noncachedOffset, noncachedValue.AsDateTime().ToDateTimeOffset());
		Assert.AreEqual(cachedValue, new DataModelValue(cachedOffset));
		Assert.AreEqual(noncachedValue, new DataModelValue(noncachedOffset));
	}

	private sealed class CountingObject(object? value) : IObject
	{
		public int ToObjectCalls { get; private set; }

	#region Interface IObject

		public object? ToObject()
		{
			ToObjectCalls ++;

			return value;
		}

	#endregion
	}
}