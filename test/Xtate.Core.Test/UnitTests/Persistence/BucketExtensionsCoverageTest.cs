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

		bucket.AddEntity(key: "entity", first);
		bucket.AddEntity<string, StoredEntity>(key: "missing-entity", entity: null);
		Assert.AreEqual(expected: 11, bucket.Nested("entity").GetInt32("value"));
		Assert.IsNull(bucket.Nested("missing-entity").GetString("value"));

		bucket.AddEntityList(key: "default-list", default(ImmutableArray<StoredEntity>));
		bucket.AddEntityList(key: "empty-list", ImmutableArray<StoredEntity>.Empty);
		bucket.AddEntityList(key: "entities", ImmutableArray.Create(first, second));
		Assert.IsFalse(bucket.TryGet(key: "default-list", out int _));
		Assert.IsFalse(bucket.TryGet(key: "empty-list", out int _));
		CollectionAssert.AreEqual(
			new[] { 11, 22 },
			bucket.RestoreList(key: "entities", static itemBucket => new StoredEntity(itemBucket.GetInt32("value"))).Select(static item => item.Value).ToArray());
		Assert.IsTrue(bucket.RestoreList(key: "absent", static _ => new StoredEntity(value: 0)).IsDefault);

		bucket.Add(key: "invalid-list", value: 1);
		Assert.ThrowsExactly<PersistenceException>([ExcludeFromCodeCoverage]() => bucket.RestoreList<string, StoredEntity>(key: "invalid-list", static _ => null));
	}

	[TestMethod]
	public void IdentifierEventUriEnumAndRequiredValueHelpersRoundTripAndHandleMissingValues()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		var sessionId = SessionId.FromString("session-id");
		var sendId = SendId.FromString("send-id");
		var uniqueInvokeId = InvokeId.FromString("unique-invoke-id");
		var explicitInvokeId = InvokeId.FromString(invokeId: "invoke-id", uniqueInvokeId: "generated-unique-id");
		var relativeUri = new Uri(uriString: "relative/path", UriKind.Relative);
		var fullUri = new FullUri("https://example.test/path");

		bucket.AddId(key: "session", sessionId);
		bucket.AddId(key: "send", sendId);
		bucket.AddId(key: "unique-invoke", uniqueInvokeId);
		bucket.AddId(key: "explicit-invoke", explicitInvokeId);
		bucket.AddId(key: "null-session", (SessionId?) null);
		bucket.AddId(key: "null-send", (SendId?) null);
		bucket.AddId(key: "null-invoke", (InvokeId?) null);
		bucket.AddEventName(key: "event", (EventName) "coverage.event");
		bucket.AddEventName(key: "default-event", eventName: default);
		bucket.Add(key: "uri", relativeUri);
		bucket.Add(key: "full-uri", fullUri);
		bucket.Add(key: "number", value: 42);
		bucket.Add(key: "flag", value: true);
		bucket.Add(key: "enum", SampleEnum.Second);

		Assert.AreEqual(sessionId, bucket.GetSessionId("session"));
		Assert.AreEqual(sendId, bucket.GetSendId("send"));
		Assert.AreEqual(uniqueInvokeId, bucket.GetInvokeId("unique-invoke"));
		var restoredExplicit = bucket.GetInvokeId("explicit-invoke");
		Assert.AreEqual(expected: "invoke-id", restoredExplicit!.Value);
		Assert.AreEqual(expected: "generated-unique-id", restoredExplicit.UniqueId.Value);
		Assert.IsNull(bucket.GetSessionId("null-session"));
		Assert.IsNull(bucket.GetSendId("null-send"));
		Assert.IsNull(bucket.GetInvokeId("null-invoke"));
		Assert.AreEqual(expected: "coverage.event", bucket.GetEventName("event").ToString());
		Assert.IsTrue(bucket.GetEventName("default-event").IsDefault);
		Assert.AreEqual(relativeUri, bucket.GetUri("uri"));
		Assert.AreEqual(fullUri, bucket.GetFullUri("full-uri"));
		Assert.IsNull(bucket.GetUri("missing-uri"));
		Assert.IsNull(bucket.GetFullUri("missing-full-uri"));
		Assert.AreEqual(expected: 42, bucket.GetInt32("number"));
		Assert.IsTrue(bucket.GetBoolean("flag"));
		Assert.AreEqual(SampleEnum.Second, bucket.GetEnum("enum").As<SampleEnum>());
		Assert.IsNull(bucket.GetString("missing-string"));
		Assert.ThrowsExactly<KeyNotFoundException>([ExcludeFromCodeCoverage]() => bucket.GetInt32("missing-number"));
		Assert.ThrowsExactly<KeyNotFoundException>([ExcludeFromCodeCoverage]() => bucket.GetBoolean("missing-flag"));
		Assert.ThrowsExactly<KeyNotFoundException>([ExcludeFromCodeCoverage]() => bucket.GetEnum("missing-enum").As<SampleEnum>());
	}

	[TestMethod]
	public void ServiceIdentifierHelpersPreferSessionThenInvokeAndHandleMissingValues()
	{
		using var storage = new InMemoryStorage(writeOnly: false);
		var bucket = new Bucket(storage);
		var sessionId = SessionId.FromString("session-service");
		var invokeId = InvokeId.FromString(invokeId: "invoke-service", uniqueInvokeId: "invoke-service-unique");

		bucket.AddServiceId(key: "session", sessionId);
		bucket.AddServiceId(key: "invoke", invokeId);
		bucket.AddServiceId(key: "missing", serviceId: null);

		Assert.IsTrue(bucket.TryGetServiceId(key: "session", out var restoredSession));
		Assert.AreEqual(sessionId, restoredSession);
		Assert.IsTrue(bucket.TryGetServiceId(key: "invoke", out var restoredInvoke));
		Assert.AreEqual(invokeId, restoredInvoke);
		Assert.IsFalse(bucket.TryGetServiceId(key: "missing", out var missing));
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
		var dateTime = new DateTime(year: 2026, month: 7, day: 13, hour: 10, minute: 20, second: 30, DateTimeKind.Utc);
		var list = new DataModelList { ["name"] = "shared", ["number"] = 17 };

		bucket.AddDataModelValue(key: "undefined", DataModelValue.Undefined);
		bucket.AddDataModelValue(key: "null", DataModelValue.Null);
		bucket.AddDataModelValue(key: "string", new DataModelValue("text"));
		bucket.AddDataModelValue(key: "number", new DataModelValue(123.5));
		bucket.AddDataModelValue(key: "date", new DataModelValue(dateTime));
		bucket.AddDataModelValue(key: "boolean", new DataModelValue(true));
		bucket.AddDataModelValue(key: "list", new DataModelValue(list));

		Assert.IsTrue(bucket.GetDataModelValue("undefined").IsUndefined());
		Assert.AreEqual(DataModelValue.Null, bucket.GetDataModelValue("null"));
		Assert.AreEqual(expected: "text", bucket.GetDataModelValue("string").AsString());
		Assert.AreEqual(expected: 123.5, bucket.GetDataModelValue("number").AsNumber());
		Assert.AreEqual(dateTime, bucket.GetDataModelValue("date").AsDateTime().ToDateTime());
		Assert.IsTrue(bucket.GetDataModelValue("boolean").AsBoolean());
		var restoredList = bucket.GetDataModelValue("list").AsList();
		Assert.AreEqual(expected: "shared", restoredList["name"].AsString());
		Assert.AreEqual(expected: 17, restoredList["number"].AsNumber());
	}

	private sealed class StoredEntity(int value) : IStoreSupport
	{
		public int Value { get; } = value;

	#region Interface IStoreSupport

		public void Store(Bucket bucket) => bucket.Add(key: "value", Value);

	#endregion
	}

	private enum SampleEnum
	{
		First,

		Second
	}
}