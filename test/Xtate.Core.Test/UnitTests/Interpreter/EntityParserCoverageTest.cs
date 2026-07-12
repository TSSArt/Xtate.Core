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
using Xtate.Interpreter.Services;
using Xtate.Logging;
using Xtate.Logging.Provider;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.Interpreter;

[TestClass]
public class EntityParserCoverageTest
{
	[TestMethod]
	public void IncomingEventCopiesIncomingEventProperties()
	{
		var source = new IncomingEvent
					 {
						 Ancestor = new object(),
						 Name = EventName.FromString("event.name"),
						 Type = EventType.Platform,
						 Origin = new FullUri("https://example.test/origin"),
						 OriginType = new FullUri("https://example.test/origin-type"),
						 SendId = SendId.FromString("send-id"),
						 InvokeId = InvokeId.FromString("invoke-id"),
						 Data = new DataModelValue("payload")
					 };

		var copy = new IncomingEvent(source);

		Assert.AreEqual(source.Name.ToString(), copy.Name.ToString());
		Assert.AreEqual(source.Type, copy.Type);
		Assert.AreEqual(source.Origin, copy.Origin);
		Assert.AreEqual(source.OriginType, copy.OriginType);
		Assert.AreEqual(source.SendId, copy.SendId);
		Assert.AreEqual(source.InvokeId, copy.InvokeId);
		Assert.AreEqual(source.Data, copy.Data);
		Assert.IsNull(copy.Ancestor);
	}

	[TestMethod]
	public void IncomingEventCopiesRelevantOutgoingEventProperties()
	{
		var source = new OutgoingEventSource
					 {
						 Name = EventName.FromString("outgoing.event"),
						 SendId = SendId.FromString("send-id"),
						 Target = new FullUri("https://example.test/target"),
						 Type = new FullUri("https://example.test/type"),
						 DelayMs = 100,
						 Data = new DataModelValue("payload")
					 };

		var incomingEvent = new IncomingEvent(source);

		Assert.AreEqual(source.Name.ToString(), incomingEvent.Name.ToString());
		Assert.AreEqual(source.SendId, incomingEvent.SendId);
		Assert.AreEqual(source.Data, incomingEvent.Data);
		Assert.AreEqual(expected: default, incomingEvent.Type);
		Assert.IsNull(incomingEvent.Origin);
		Assert.IsNull(incomingEvent.OriginType);
		Assert.IsNull(incomingEvent.InvokeId);
	}

	[TestMethod]
	public void EventEntityParserEnumeratesEveryPopulatedProperty()
	{
		IEntityParserHandler parser = new EventEntityParser();
		var incomingEvent = new IncomingEvent
							{
								Name = EventName.FromString("event.name"),
								Type = EventType.External,
								Origin = new FullUri("https://example.test/origin"),
								OriginType = new FullUri("https://example.test/origin-type"),
								SendId = SendId.FromString("send-id"),
								InvokeId = InvokeId.FromString("invoke-id")
							};

		var properties = Parse(parser, incomingEvent);

		Assert.AreEqual(Level.Info, parser.Level);
		Assert.AreEqual(expected: 6, properties.Count);
		Assert.AreEqual(expected: "event.name", properties["EventName"].Value!.ToString());
		Assert.AreEqual(EventType.External, properties["EventType"].Value);
		Assert.AreEqual(incomingEvent.Origin, properties["Origin"].Value);
		Assert.AreEqual(incomingEvent.OriginType, properties["OriginType"].Value);
		Assert.AreEqual(incomingEvent.SendId, properties["SendId"].Value);
		Assert.AreEqual(incomingEvent.InvokeId, properties["InvokeId"].Value);
	}

	[TestMethod]
	public void EventEntityParserOmitsOptionalProperties()
	{
		IEntityParserHandler parser = new EventEntityParser();

		var properties = Parse(parser, new IncomingEvent { Type = EventType.Internal });

		Assert.HasCount(expected: 1, properties);
		Assert.AreEqual(EventType.Internal, properties["EventType"].Value);
		Assert.IsNull(parser.EnumerateProperties("wrong entity type"));
	}

	[TestMethod]
	public void InvokeDataEntityParserEnumeratesTypeAndSource()
	{
		IEntityParserHandler parser = new InvokeDataEntityParser();
		var invokeData = new InvokeData(
			InvokeId.FromString("invoke-id"),
			new FullUri("https://example.test/invoke-type"),
			new Uri("https://example.test/source"),
			RawContent: null,
			Content: default,
			Parameters: default);

		var properties = Parse(parser, invokeData);

		Assert.AreEqual(Level.Info, parser.Level);
		Assert.HasCount(expected: 2, properties);
		Assert.AreEqual(invokeData.Type, properties["InvokeType"].Value);
		Assert.AreEqual(invokeData.Source, properties["InvokeSource"].Value);
	}

	[TestMethod]
	public void InvokeDataEntityParserOmitsMissingSource()
	{
		IEntityParserHandler parser = new InvokeDataEntityParser();
		var invokeData = new InvokeData(
			InvokeId.FromString("invoke-id"),
			new FullUri("https://example.test/invoke-type"),
			Source: null,
			RawContent: null,
			Content: default,
			Parameters: default);

		var properties = Parse(parser, invokeData);

		Assert.HasCount(expected: 1, properties);
		Assert.IsTrue(properties.ContainsKey("InvokeType"));
	}

	[TestMethod]
	public void DataModelValueEntityParserHandlesDefinedAndUndefinedValues()
	{
		IEntityParserHandler parser = new DataModelValueEntityParser();
		var value = new DataModelValue("payload");

		var properties = Parse(parser, value);

		Assert.HasCount(expected: 1, properties);
		Assert.AreEqual(value, properties["Parameter"].Value);
		Assert.IsEmpty(parser.EnumerateProperties(default(DataModelValue))!);
		Assert.IsNull(parser.EnumerateProperties("wrong entity type"));
	}

	[TestMethod]
	public void InvokeIdEntityParserHandlesPresentAndNullValues()
	{
		IEntityParserHandler parser = new InvokeIdEntityParser();
		var invokeId = InvokeId.FromString("invoke-id");

		var properties = Parse(parser, invokeId);

		Assert.HasCount(expected: 1, properties);
		Assert.AreSame(invokeId, properties["InvokeId"].Value);
		Assert.IsNull(parser.EnumerateProperties<InvokeId?>(null));
	}

	[TestMethod]
	public void SendIdEntityParserHandlesPresentAndNullValues()
	{
		IEntityParserHandler parser = new SendIdEntityParser();
		var sendId = SendId.FromString("send-id");

		var properties = Parse(parser, sendId);

		Assert.HasCount(expected: 1, properties);
		Assert.AreSame(sendId, properties["SendId"].Value);
		Assert.IsNull(parser.EnumerateProperties<SendId?>(null));
	}

	[TestMethod]
	public void InterpreterStateParserExposesStateAtInfoLevel()
	{
		IEntityParserHandler parser = new InterpreterStateParser();

		var properties = Parse(parser, StateMachineInterpreterState.Waiting);

		Assert.AreEqual(Level.Info, parser.Level);
		Assert.HasCount(expected: 1, properties);
		Assert.AreSame(StateMachineInterpreterState.Waiting, properties["InterpreterState"].Value);
		Assert.IsNull(parser.EnumerateProperties("wrong entity type"));
	}

	[TestMethod]
	public void StateMachineInterpreterStateUsesIdentityAndDisplayName()
	{
		var state = StateMachineInterpreterState.Started;
		var same = StateMachineInterpreterState.Started;
		var different = StateMachineInterpreterState.Completed;

		Assert.AreEqual(expected: "Started", state.ToString());
		Assert.IsTrue(state.Equals(same));
		Assert.IsFalse(state.Equals(different));
		Assert.IsFalse(state.Equals(new object()));
		Assert.IsTrue(state == same);
		Assert.IsFalse(state != same);
		Assert.IsFalse(state == different);
		Assert.IsTrue(state != different);
		Assert.AreEqual(RuntimeHelpers.GetHashCode(state), state.GetHashCode());
	}

	private static Dictionary<string, LoggingParameter> Parse<TEntity>(IEntityParserHandler parser, TEntity entity) => parser.EnumerateProperties(entity)!.ToDictionary(parameter => parameter.Name);

	private sealed class OutgoingEventSource : IOutgoingEvent
	{
	#region Interface IOutgoingEvent

		public SendId? SendId { get; init; }

		public EventName Name { get; init; }

		public FullUri? Target { get; init; }

		public FullUri? Type { get; init; }

		public int DelayMs { get; init; }

		public DataModelValue Data { get; init; }

	#endregion
	}
}