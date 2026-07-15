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
using Xtate.Interpreter.Internal;
using Xtate.Interpreter.Model;
using Xtate.Interpreter.Services;
using Xtate.IoC.Tools;
using Xtate.Logging;
using Xtate.Logging.Provider;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.Interpreter;

[TestClass]
public class InterpreterLoggingAndConnectionsCoverageTest
{
	[TestMethod]
	public async Task NoExternalConnectionsRejectsEveryCommunicationAndServiceOperation()
	{
		var noConnections = new NoExternalConnections { StateMachineRuntimeError = new StateMachineRuntimeError(new ScopeObject()) };
		var communication = (IExternalCommunication) noConnections;
		var services = (IExternalServiceManager) noConnections;
		var outgoingEvent = Mock.Of<IOutgoingEvent>();
		var incomingEvent = Mock.Of<IIncomingEvent>();
		var sendId = SendId.FromString("send");
		var invokeId = InvokeId.FromString("invoke");
		var invokeData = new InvokeData(invokeId, new FullUri("service"), Source: null, RawContent: null, DataModelValue.Undefined, DataModelValue.Undefined);

		await Assert.ThrowsExactlyAsync<CommunicationException>([ExcludeFromCodeCoverage] async () => await communication.TrySend(outgoingEvent));
		await Assert.ThrowsExactlyAsync<CommunicationException>([ExcludeFromCodeCoverage] async () => await communication.Cancel(sendId));
		await Assert.ThrowsExactlyAsync<CommunicationException>([ExcludeFromCodeCoverage] async () => await services.Start(invokeData));
		await Assert.ThrowsExactlyAsync<CommunicationException>([ExcludeFromCodeCoverage] async () => await services.Forward(invokeId, incomingEvent));
		await Assert.ThrowsExactlyAsync<CommunicationException>([ExcludeFromCodeCoverage] async () => await services.Cancel(invokeId));
	}

	[TestMethod]
	public void EventVerboseParserHandlesUndefinedFallbackAndDataModelTextConversion()
	{
		var handler = new Mock<IDataModelHandler>();
		handler.Setup(static h => h.ConvertToText(It.IsAny<DataModelValue>())).Returns("converted");
		var parser = new EventVerboseEntityParser { DataModelHandler = () => handler.Object };
		var parserHandler = (IEntityParserHandler) parser;

		Assert.AreEqual(Level.Verbose, parserHandler.Level);
		Assert.IsEmpty(parserHandler.EnumerateProperties(new IncomingEvent { Data = DataModelValue.Undefined })!);
		var parameters = parserHandler.EnumerateProperties(new IncomingEvent { Data = new DataModelValue(12D) })!.ToArray();

		Assert.HasCount(expected: 2, parameters);
		Assert.AreEqual("Data", parameters[0].Name);
		Assert.AreEqual("DataText", parameters[1].Name);
		Assert.AreEqual("converted", parameters[1].Value);

		var fallbackParser = new EventVerboseEntityParser { DataModelHandler = static () => null };
		var fallback = ((IEntityParserHandler) fallbackParser).EnumerateProperties(new IncomingEvent { Data = new DataModelValue(true) })!.ToArray();
		Assert.AreEqual("True", fallback[1].Value);
	}

	[TestMethod]
	public void InterpreterInfoEnricherReportsOnlyNonemptySessionIds()
	{
		var empty = new InterpreterInfoLogEnricher<object> { StateMachineSessionId = static () => null };
		var session = Mock.Of<IStateMachineSessionId>(static value => value.SessionId == SessionId.FromString("session"));
		var populated = new InterpreterInfoLogEnricher<object> { StateMachineSessionId = () => session };

		Assert.AreEqual(Level.Info, populated.Level);
		Assert.AreEqual("ctx", populated.Namespace);
		Assert.IsEmpty(empty.EnumerateProperties());
		var parameter = populated.EnumerateProperties().Single();
		Assert.AreEqual("SessionId", parameter.Name);
		Assert.AreEqual("session", parameter.Value!.ToString());
	}

	[TestMethod]
	public void InterpreterDebugEnricherReportsMachineNameAndActiveStates()
	{
		var stateMachine = Mock.Of<IStateMachine>(static machine => machine.Name == "machine");
		var configuration = new OrderedSet<StateEntityNode>
							{
								new TestStateNode(Identifier.FromString("first")),
								new TestStateNode(Identifier.FromString("second"))
							};
		var context = new Mock<IStateMachineContext>();
		context.SetupGet(static value => value.Configuration).Returns(configuration);
		var enricher = new InterpreterDebugLogEnricher<object>
					   {
						   StateMachine = () => stateMachine,
						   StateMachineContext = () => context.Object
					   };

		Assert.AreEqual(Level.Debug, enricher.Level);
		Assert.AreEqual("ctx", enricher.Namespace);
		var parameters = enricher.EnumerateProperties().ToArray();
		Assert.HasCount(expected: 2, parameters);
		Assert.AreEqual("StateMachineName", parameters[0].Name);
		Assert.AreEqual("ActiveStates", parameters[1].Name);

		var empty = new InterpreterDebugLogEnricher<object>
					{
						StateMachine = static () => null,
						StateMachineContext = static () => null
					};
		Assert.IsEmpty(empty.EnumerateProperties());
	}

	private sealed class TestStateNode(IIdentifier id) : StateEntityNode(new DocumentIdNode(list: null))
	{
		public override IIdentifier Id => id;
	}
}
