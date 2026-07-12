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

using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.IoProcessors.NamedPipe;
using Xtate.Persistence;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;
using Xtate.StateMachine;
using Xtate.StateMachineHost;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class PersistedEventCoverageTest
{
	[TestMethod]
	public void NamedPipeResponseMessageRoundTripsSuccessResponse()
	{
		var timestamp = new DateTime(year: 2026, month: 7, day: 12, hour: 10, minute: 20, second: 30, DateTimeKind.Utc);
		var bucket = CreateBucket();

		new NamedPipeResponseMessage(timestamp).Store(bucket);
		var restored = new NamedPipeResponseMessage(bucket);

		Assert.AreEqual(timestamp, restored.Timestamp);
		Assert.AreEqual(NamedPipeErrorType.None, restored.ErrorType);
		Assert.IsNull(restored.ExceptionMessage);
		Assert.IsNull(restored.ExceptionText);
	}

	[TestMethod]
	public void NamedPipeResponseMessageRoundTripsExceptionResponse()
	{
		var timestamp = new DateTime(year: 2026, month: 7, day: 12, hour: 10, minute: 20, second: 30, DateTimeKind.Utc);
		var bucket = CreateBucket();

		new NamedPipeResponseMessage(timestamp, new InvalidOperationException("failure detail")).Store(bucket);
		var restored = new NamedPipeResponseMessage(bucket);

		Assert.AreEqual(timestamp, restored.Timestamp);
		Assert.AreEqual(NamedPipeErrorType.Exception, restored.ErrorType);
		Assert.AreEqual(expected: "failure detail", restored.ExceptionMessage);
#if DEBUG
		StringAssert.Contains(restored.ExceptionText, substring: "InvalidOperationException");
#else
		Assert.IsNull(restored.ExceptionText);
#endif
	}

	[TestMethod]
	public void NamedPipeResponseMessageRejectsInvalidTypeInformation()
	{
		var bucket = CreateBucket();

		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => _ = new NamedPipeResponseMessage(bucket));
	}

	[TestMethod]
	public void NamedPipeEventMessageRoundTripsEventFields()
	{
		var timestamp = new DateTime(year: 2026, month: 7, day: 12, hour: 10, minute: 20, second: 30, DateTimeKind.Utc);
		var source = CreateIncomingEvent();
		var message = new NamedPipeEventMessage(timestamp, SessionId.FromString("target-session"), source);
		var bucket = CreateBucket();

		((IStoreSupport) message).Store(bucket);
		var restored = new NamedPipeEventMessage(bucket);

		Assert.AreEqual(timestamp, restored.Timestamp);
		Assert.AreEqual(expected: "target-session", restored.TargetServiceId.ToString());
		AssertIncomingEvent(source, restored);
	}

	[TestMethod]
	public void NamedPipeEventMessageRejectsMissingMessageMetadata()
	{
		var bucket = CreateBucket();

		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => _ = new NamedPipeEventMessage(bucket));
	}

	[TestMethod]
	public void PersistedIncomingEventRoundTripsEventFields()
	{
		var source = CreateIncomingEvent();
		var bucket = CreateBucket();

		((IStoreSupport) CreatePersistedIncomingEvent(source)).Store(bucket);
		var restored = new PersistedIncomingEvent(bucket);

		AssertIncomingEvent(source, restored);
	}

	[TestMethod]
	public void PersistedIncomingEventRejectsInvalidTypeInformation()
	{
		var bucket = CreateBucket();

		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => _ = new PersistedIncomingEvent(bucket));
	}

	[TestMethod]
	public void PersistedRouterEventRoundTripsFieldsWithoutTarget()
	{
		var source = new RouterEventSource
					 {
						 SenderServiceId = SessionId.FromString("sender-session"),
						 IoProcessorData = new DataModelList { ["key"] = new DataModelValue("value") },
						 DelayMs = 125,
						 Name = EventName.FromString("router.event"),
						 Type = EventType.External,
						 SendId = SendId.FromString("send-id"),
						 Origin = new FullUri("https://example.test/origin"),
						 OriginType = new FullUri("https://example.test/origin-type"),
						 Data = new DataModelValue("payload")
					 };
		var bucket = CreateBucket();

		((IStoreSupport) CreatePersistedRouterEvent(source)).Store(bucket);
		var restored = new PersistedRouterEvent(bucket);

		Assert.AreEqual(expected: "sender-session", restored.SenderServiceId.ToString());
		Assert.AreEqual(expected: 125, restored.DelayMs);
		Assert.AreEqual(expected: "value", restored.IoProcessorData!["key"].AsString());
		AssertIncomingEvent(source, restored);
		Assert.IsNull(restored.TargetType);
		Assert.IsNull(restored.Target);
	}

	[TestMethod]
	public void PersistedRouterEventRejectsInvalidTypeInformation()
	{
		var bucket = CreateBucket();

		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => _ = new PersistedRouterEvent(bucket));
	}

	[TestMethod]

	//[Ignore("Persisted event writers store InvokeId under Key.InvokeId, while readers use Key.InvokeUniqueId; enable after the persistence keys are made symmetrical.")]
	public void PersistedEventsRoundTripInvokeId()
	{
		// Regression coverage is intentionally retained: all three persisted event readers currently lose InvokeId during a store/read round-trip.
		var source = CreateIncomingEvent(InvokeId.FromString("invoke-id"));
		var incomingBucket = CreateBucket();
		((IStoreSupport) CreatePersistedIncomingEvent(source)).Store(incomingBucket);
		Assert.AreEqual(source.InvokeId, new PersistedIncomingEvent(incomingBucket).InvokeId);

		var messageBucket = CreateBucket();
		((IStoreSupport) new NamedPipeEventMessage(DateTime.UtcNow, SessionId.FromString("target"), source)).Store(messageBucket);
		Assert.AreEqual(source.InvokeId, new NamedPipeEventMessage(messageBucket).InvokeId);
	}

	[TestMethod]

	//[Ignore("PersistedRouterEvent.Store writes Target to Key.TargetType instead of Key.Target; enable after distinct keys are used.")]
	public void PersistedRouterEventRoundTripsTargetUris()
	{
		// Regression coverage is intentionally retained: writing Target currently overwrites TargetType and leaves Target absent.
		var source = new RouterEventSource
					 {
						 SenderServiceId = SessionId.FromString("sender"),
						 TargetType = new FullUri("https://example.test/target-type"),
						 Target = new FullUri("https://example.test/target")
					 };
		var bucket = CreateBucket();
		((IStoreSupport) CreatePersistedRouterEvent(source)).Store(bucket);
		var restored = new PersistedRouterEvent(bucket);

		Assert.AreEqual(source.TargetType, restored.TargetType);
		Assert.AreEqual(source.Target, restored.Target);
	}

	private static Bucket CreateBucket() => new(new InMemoryStorage(writeOnly: false));

	private static PersistedIncomingEvent CreatePersistedIncomingEvent(IIncomingEvent source)
	{
		var bucket = CreateBucket();
		SeedIncomingEvent(bucket, TypeInfo.EventObject, source);

		return new PersistedIncomingEvent(bucket);
	}

	private static PersistedRouterEvent CreatePersistedRouterEvent(IRouterEvent source)
	{
		var bucket = CreateBucket();
		SeedIncomingEvent(bucket, TypeInfo.RouterEvent, source);
		bucket.AddServiceId(Key.SenderServiceId, source.SenderServiceId);
		bucket.AddDataModelValue(Key.RouterEventData, source.IoProcessorData);
		bucket.Add(Key.DelayMs, source.DelayMs);
		bucket.Add(Key.TargetType, source.TargetType);
		bucket.Add(Key.Target, source.Target);

		return new PersistedRouterEvent(bucket);
	}

	private static void SeedIncomingEvent(Bucket bucket, TypeInfo typeInfo, IIncomingEvent source)
	{
		bucket.Add(Key.TypeInfo, typeInfo);
		bucket.AddEventName(Key.Name, source.Name);
		bucket.Add(Key.Type, source.Type);
		bucket.AddId(Key.SendId, source.SendId);
		bucket.Add(Key.Origin, source.Origin);
		bucket.Add(Key.OriginType, source.OriginType);
		bucket.AddId(Key.InvokeId, source.InvokeId);
		bucket.AddDataModelValue(Key.Data, source.Data);
	}

	private static IncomingEvent CreateIncomingEvent(InvokeId? invokeId = null) =>
		new()
		{
			Name = EventName.FromString("event.name"),
			Type = EventType.External,
			SendId = SendId.FromString("send-id"),
			Origin = new FullUri("https://example.test/origin"),
			OriginType = new FullUri("https://example.test/origin-type"),
			InvokeId = invokeId,
			Data = new DataModelValue("payload")
		};

	private static void AssertIncomingEvent(IIncomingEvent expected, IIncomingEvent actual)
	{
		Assert.AreEqual(expected.Name.ToString(), actual.Name.ToString());
		Assert.AreEqual(expected.Type, actual.Type);
		Assert.AreEqual(expected.SendId, actual.SendId);
		Assert.AreEqual(expected.Origin, actual.Origin);
		Assert.AreEqual(expected.OriginType, actual.OriginType);
		Assert.AreEqual(expected.InvokeId, actual.InvokeId);
		Assert.AreEqual(expected.Data, actual.Data);
	}

	private sealed class RouterEventSource : IRouterEvent
	{
	#region Interface IIncomingEvent

		public FullUri? OriginType { get; init; }

		public InvokeId? InvokeId { get; init; }

		public EventName Name { get; init; }

		public SendId? SendId { get; init; }

		public EventType Type { get; init; }

		public DataModelValue Data { get; init; }

		public FullUri? Origin { get; init; }

	#endregion

	#region Interface IRouterEvent

		public ServiceId SenderServiceId { get; init; } = null!;

		public DataModelList? IoProcessorData { get; init; }

		public int DelayMs { get; init; }

		public FullUri? TargetType { get; init; }

		public FullUri? Target { get; init; }

	#endregion
	}
}