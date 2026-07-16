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
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Services;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class BucketConverterCoverageTest
{
	[TestMethod]
	public void BucketRoundTripsSupportedPrimitiveUriAndDataModelValues()
	{
		AssertSupportedPrimitiveUriAndDataModelValues(uint.MaxValue);
	}

	[TestMethod]
	public void BucketRoundTripsSupportedPrimitiveUriAndDataModelValuesWithinCurrentUInt32Range() => AssertSupportedPrimitiveUriAndDataModelValues(uintValueToPersist: 123456U);

	private static void AssertSupportedPrimitiveUriAndDataModelValues(uint uintValueToPersist)
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		var dateTime = new DateTime(year: 2026, month: 2, day: 3, hour: 4, minute: 5, second: 6, DateTimeKind.Utc);
		var dateTimeOffset = new DateTimeOffset(year: 2026, month: 2, day: 3, hour: 4, minute: 5, second: 6, TimeSpan.FromHours(2));
		var dataModelNumber = DataModelNumber.FromDecimal(123.45M);
		var dataModelDateTime = DataModelDateTime.FromDateTimeOffset(dateTimeOffset);

		bucket.Add(key: "byte", (byte) 250);
		bucket.Add(key: "short", (short) -1234);
		bucket.Add(key: "int", value: -123456789);
		bucket.Add(key: "uint", uintValueToPersist);
		bucket.Add(key: "bool", value: true);
		bucket.Add(key: "double", Math.PI);
		bucket.Add(key: "empty", string.Empty);
		bucket.Add(key: "unicode", value: "zażółć");
		bucket.Add(key: "date", dateTime);
		bucket.Add(key: "offset", dateTimeOffset);
		bucket.Add(key: "uri", new Uri(uriString: "relative/path", UriKind.Relative));
		bucket.Add(key: "full-uri", new FullUri("https://example.test/a#fragment"));
		bucket.Add(key: "number", dataModelNumber);
		bucket.Add(key: "model-date", dataModelDateTime);
		bucket.Add(key: "enum", SampleEnum.Large);

		Assert.IsTrue(bucket.TryGet(key: "byte", out byte byteValue));
		Assert.AreEqual((byte) 250, byteValue);
		Assert.IsTrue(bucket.TryGet(key: "short", out short shortValue));
		Assert.AreEqual((short) -1234, shortValue);
		Assert.IsTrue(bucket.TryGet(key: "int", out int intValue));
		Assert.AreEqual(expected: -123456789, intValue);
		Assert.IsTrue(bucket.TryGet(key: "uint", out uint uintValue));
		Assert.AreEqual(uintValueToPersist, uintValue);
		Assert.IsTrue(bucket.TryGet(key: "bool", out bool boolValue));
		Assert.IsTrue(boolValue);
		Assert.IsTrue(bucket.TryGet(key: "double", out double doubleValue));
		Assert.AreEqual(Math.PI, doubleValue);
		Assert.IsTrue(bucket.TryGet(key: "empty", out string? empty));
		Assert.AreEqual(string.Empty, empty);
		Assert.IsTrue(bucket.TryGet(key: "unicode", out string? unicode));
		Assert.AreEqual(expected: "zażółć", unicode);
		Assert.IsTrue(bucket.TryGet(key: "date", out DateTime actualDate));
		Assert.AreEqual(dateTime, actualDate);
		Assert.IsTrue(bucket.TryGet(key: "offset", out DateTimeOffset actualOffset));
		Assert.AreEqual(dateTimeOffset, actualOffset);
		Assert.IsTrue(bucket.TryGet(key: "uri", out Uri? uri));
		Assert.AreEqual(expected: "relative/path", uri!.ToString());
		Assert.IsTrue(bucket.TryGet(key: "full-uri", out FullUri? fullUri));
		Assert.AreEqual(expected: "https://example.test/a#fragment", fullUri!.ToString());
		Assert.IsTrue(bucket.TryGet(key: "number", out DataModelNumber actualNumber));
		Assert.AreEqual(dataModelNumber, actualNumber);
		Assert.IsTrue(bucket.TryGet(key: "model-date", out DataModelDateTime actualModelDate));
		Assert.AreEqual(dataModelDateTime, actualModelDate);
		Assert.AreEqual(SampleEnum.Large, bucket.GetEnum("enum").As<SampleEnum>());
	}

	[TestMethod]
	public void BucketCoversIndexEnumStringNestedRawRemovalAndUnsupportedConverters()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		int[] keys = [0, 31, 1_000, 100_000, 10_000_000, int.MaxValue];

		foreach (var key in keys)
		{
			bucket.Add(key, key.ToString());
			Assert.IsTrue(bucket.TryGet(key, out string? value));
			Assert.AreEqual(key.ToString(), value);
		}

		bucket.Add(SampleEnum.Small, value: "enum-key");
		Assert.IsTrue(bucket.TryGet(SampleEnum.Small, out string? enumKeyValue));
		Assert.AreEqual(expected: "enum-key", enumKeyValue);
		var nested = bucket.Nested("a-very-long-parent-key").Nested("another-long-child-key");
		nested.Add(key: "value", value: 42);
		Assert.IsTrue(nested.TryGet(key: "value", out int nestedValue));
		Assert.AreEqual(expected: 42, nestedValue);

		bucket.Add(key: "raw", [1, 2, 3]);
		Assert.IsTrue(bucket.TryGet(key: "raw", out var raw));
		CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, raw.ToArray());
		bucket.Add(key: "raw", ReadOnlySpan<byte>.Empty);
		Assert.IsFalse(bucket.TryGet(key: "raw", out raw));

		bucket.Add(key: "nullable", value: "present");
		bucket.Add<string, string?>(key: "nullable", value: null);
		Assert.IsFalse(bucket.TryGet(key: "nullable", out string? _));

		var tree = bucket.Nested("tree");
		tree.Add(key: "one", value: 1);
		tree.Add(key: "two", value: 2);
		bucket.RemoveSubtree("tree");
		Assert.IsFalse(tree.TryGet(key: "one", out int _));
		Assert.IsFalse(tree.TryGet(key: "two", out int _));

		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => bucket.Add(Guid.Parse("00000000-0000-0000-0000-000000000001"), value: 1));
		Assert.ThrowsExactly<NotSupportedException>([ExcludeFromCodeCoverage]() => bucket.Add(key: "decimal", value: 1M));
	}

	private enum SampleEnum
	{
		Small = 1,

		Large = int.MaxValue
	}
}