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
using Xtate.Interpreter;
using Xtate.Interpreter.Services;
using Xtate.Logging;
using Xtate.Logging.Provider;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.Interpreter;

[TestClass]
public class InterpreterServiceCoverageTest
{
	[TestMethod]
	public void NoStateMachineArgumentsReturnsUndefinedDataModelValue()
	{
		IStateMachineArguments arguments = new NoStateMachineArguments();

		Assert.AreEqual(DataModelValueType.Undefined, arguments.Arguments.Type);
		Assert.IsTrue(arguments.Arguments.IsUndefined());
	}

	[TestMethod]
	public void OutgoingEventEntityParserEnumeratesPopulatedProperties()
	{
		IEntityParserHandler parser = new OutgoingEventEntityParser();
		var outgoingEvent = new OutgoingEventSource
							{
								Name = EventName.FromString("event.name"),
								SendId = SendId.FromString("send-id"),
								Type = new FullUri("https://example.test/type"),
								Target = new FullUri("https://example.test/target"),
								DelayMs = 250
							};

		var properties = parser.EnumerateProperties(outgoingEvent)!.ToDictionary(parameter => parameter.Name);

		Assert.AreEqual(Level.Info, parser.Level);
		Assert.AreEqual(expected: 5, properties.Count);
		Assert.AreEqual(outgoingEvent.Name.ToString(), properties["EventName"].Value!.ToString());
		Assert.AreEqual(outgoingEvent.SendId, properties["SendId"].Value);
		Assert.AreEqual(outgoingEvent.Type, properties["EventType"].Value);
		Assert.AreEqual(outgoingEvent.Target, properties["Target"].Value);
		Assert.AreEqual(expected: 250, properties["DelayMs"].Value);
	}

	[TestMethod]
	public void OutgoingEventEntityParserOmitsDefaultPropertiesAndRejectsOtherEntities()
	{
		IEntityParserHandler parser = new OutgoingEventEntityParser();

		Assert.IsEmpty(parser.EnumerateProperties(new OutgoingEventSource())!);
		Assert.IsNull(parser.EnumerateProperties("not an outgoing event"));
	}

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