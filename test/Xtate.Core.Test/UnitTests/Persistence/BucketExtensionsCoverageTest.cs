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
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class BucketExtensionsCoverageTest
{
	[TestMethod]
	public void EntityAndEntityListStorageCoversNullEmptyRestoreAndInvalidItems()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		var first = new StoredEntity(11);
		var second = new StoredEntity(22);

		bucket.AddEntity("entity", first);
		bucket.AddEntity<string, StoredEntity>("missing-entity", entity: null);
		Assert.AreEqual(expected: 11, bucket.Nested("entity").GetInt32("value"));
		Assert.IsNull(bucket.Nested("missing-entity").GetString("value"));

		bucket.AddEntityList("default-list", default(ImmutableArray<StoredEntity>));
		bucket.AddEntityList("empty-list", ImmutableArray<StoredEntity>.Empty);
		bucket.AddEntityList("entities", ImmutableArray.Create(first, second));
		Assert.IsFalse(bucket.TryGet("default-list", out int _));
		Assert.IsFalse(bucket.TryGet("empty-list", out int _));
		CollectionAssert.AreEqual(
			new[] { 11, 22 },
			bucket.RestoreList("entities", static itemBucket => new StoredEntity(itemBucket.GetInt32("value"))).Select(static item => item.Value).ToArray());
		Assert.IsTrue(bucket.RestoreList("absent", static _ => new StoredEntity(value: 0)).IsDefault);

		bucket.Add("invalid-list", 1);
		Assert.ThrowsExactly<PersistenceException>(
			[ExcludeFromCodeCoverage] () => bucket.RestoreList<string, StoredEntity>("invalid-list", static _ => null));
	}

	[TestMethod]
	public void IdentifierEventUriEnumAndRequiredValueHelpersRoundTripAndHandleMissingValues()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		var sessionId = SessionId.FromString("session-id");
		var sendId = SendId.FromString("send-id");
		var uniqueInvokeId = InvokeId.FromString("unique-invoke-id");
		var explicitInvokeId = InvokeId.FromString("invoke-id", "generated-unique-id");
		var relativeUri = new Uri("relative/path", UriKind.Relative);
		var fullUri = new FullUri("https://example.test/path");

		bucket.AddId("session", sessionId);
		bucket.AddId("send", sendId);
		bucket.AddId("unique-invoke", uniqueInvokeId);
		bucket.AddId("explicit-invoke", explicitInvokeId);
		bucket.AddId("null-session", (SessionId?) null);
		bucket.AddId("null-send", (SendId?) null);
		bucket.AddId("null-invoke", (InvokeId?) null);
		bucket.AddEventName("event", (EventName) "coverage.event");
		bucket.AddEventName("default-event", default);
		bucket.Add("uri", relativeUri);
		bucket.Add("full-uri", fullUri);
		bucket.Add("number", 42);
		bucket.Add("flag", true);
		bucket.Add("enum", SampleEnum.Second);

		Assert.AreEqual(sessionId, bucket.GetSessionId("session"));
		Assert.AreEqual(sendId, bucket.GetSendId("send"));
		Assert.AreEqual(uniqueInvokeId, bucket.GetInvokeId("unique-invoke"));
		var restoredExplicit = bucket.GetInvokeId("explicit-invoke");
		Assert.AreEqual("invoke-id", restoredExplicit!.Value);
		Assert.AreEqual("generated-unique-id", restoredExplicit.UniqueId.Value);
		Assert.IsNull(bucket.GetSessionId("null-session"));
		Assert.IsNull(bucket.GetSendId("null-send"));
		Assert.IsNull(bucket.GetInvokeId("null-invoke"));
		Assert.AreEqual("coverage.event", bucket.GetEventName("event").ToString());
		Assert.IsTrue(bucket.GetEventName("default-event").IsDefault);
		Assert.AreEqual(relativeUri, bucket.GetUri("uri"));
		Assert.AreEqual(fullUri, bucket.GetFullUri("full-uri"));
		Assert.IsNull(bucket.GetUri("missing-uri"));
		Assert.IsNull(bucket.GetFullUri("missing-full-uri"));
		Assert.AreEqual(expected: 42, bucket.GetInt32("number"));
		Assert.IsTrue(bucket.GetBoolean("flag"));
		Assert.AreEqual(SampleEnum.Second, bucket.GetEnum("enum").As<SampleEnum>());
		Assert.IsNull(bucket.GetString("missing-string"));
		Assert.ThrowsExactly<KeyNotFoundException>([ExcludeFromCodeCoverage] () => bucket.GetInt32("missing-number"));
		Assert.ThrowsExactly<KeyNotFoundException>([ExcludeFromCodeCoverage] () => bucket.GetBoolean("missing-flag"));
		Assert.ThrowsExactly<KeyNotFoundException>([ExcludeFromCodeCoverage] () => bucket.GetEnum("missing-enum").As<SampleEnum>());
	}

	[TestMethod]
	public void ServiceIdentifierHelpersPreferSessionThenInvokeAndHandleMissingValues()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		var sessionId = SessionId.FromString("session-service");
		var invokeId = InvokeId.FromString("invoke-service", "invoke-service-unique");

		bucket.AddServiceId("session", sessionId);
		bucket.AddServiceId("invoke", invokeId);
		bucket.AddServiceId("missing", serviceId: null);

		Assert.IsTrue(bucket.TryGetServiceId("session", out var restoredSession));
		Assert.AreEqual(sessionId, restoredSession);
		Assert.IsTrue(bucket.TryGetServiceId("invoke", out var restoredInvoke));
		Assert.AreEqual(invokeId, restoredInvoke);
		Assert.IsFalse(bucket.TryGetServiceId("missing", out var missing));
		Assert.IsNull(missing);
		Assert.AreEqual(sessionId, bucket.GetServiceId("session"));
		Assert.AreEqual(invokeId, bucket.GetServiceId("invoke"));
		Assert.IsNull(bucket.GetServiceId("missing"));
	}

	[TestMethod]
	public void DataModelValueHelpersRoundTripEverySupportedTypeAndSharedLists()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		var dateTime = new DateTime(2026, 7, 13, 10, 20, 30, DateTimeKind.Utc);
		var list = new DataModelList { ["name"] = "shared", ["number"] = 17 };

		bucket.AddDataModelValue("undefined", DataModelValue.Undefined);
		bucket.AddDataModelValue("null", DataModelValue.Null);
		bucket.AddDataModelValue("string", new DataModelValue("text"));
		bucket.AddDataModelValue("number", new DataModelValue(123.5));
		bucket.AddDataModelValue("date", new DataModelValue(dateTime));
		bucket.AddDataModelValue("boolean", new DataModelValue(true));
		bucket.AddDataModelValue("list", new DataModelValue(list));

		Assert.IsTrue(bucket.GetDataModelValue("undefined").IsUndefined());
		Assert.AreEqual(DataModelValue.Null, bucket.GetDataModelValue("null"));
		Assert.AreEqual("text", bucket.GetDataModelValue("string").AsString());
		Assert.AreEqual(expected: 123.5, bucket.GetDataModelValue("number").AsNumber());
		Assert.AreEqual(dateTime, bucket.GetDataModelValue("date").AsDateTime().ToDateTime());
		Assert.IsTrue(bucket.GetDataModelValue("boolean").AsBoolean());
		var restoredList = bucket.GetDataModelValue("list").AsList();
		Assert.AreEqual("shared", restoredList["name"].AsString());
		Assert.AreEqual(expected: 17, restoredList["number"].AsNumber());
	}

	private sealed class StoredEntity(int value) : IStoreSupport
	{
		public int Value { get; } = value;

		public void Store(Bucket bucket) => bucket.Add("value", Value);
	}

	private enum SampleEnum
	{
		First,
		Second
	}
}
