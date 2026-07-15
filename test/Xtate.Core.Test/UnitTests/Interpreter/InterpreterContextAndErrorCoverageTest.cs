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

using System.Collections;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.Interpreter.Internal;
using Xtate.Interpreter.Services;
using Xtate.IoC.Tools;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.Interpreter;

[TestClass]
public class InterpreterContextAndErrorCoverageTest
{
	[TestMethod]
	public void KeyListStoresRaisesEventsLooksUpAndSupportsBothEnumeratorInterfaces()
	{
		var key = Mock.Of<IEntity>();
		var missing = Mock.Of<IEntity>();
		var first = new List<int> { 1, 2 };
		var replacement = new List<int> { 3 };
		var keyList = new KeyList<int>();
		var changes = new List<(KeyList<int>.ChangedAction Action, IEntity Entity, List<int> Values)>();
		keyList.Changed += (action, entity, values) => changes.Add((action, entity, values));

		keyList.Set(key, first);
		keyList.Set(key, replacement);

		Assert.IsTrue(keyList.TryGetValue(key, out var stored));
		Assert.AreSame(replacement, stored);
		Assert.IsFalse(keyList.TryGetValue(missing, out _));
		Assert.AreEqual(expected: 2, changes.Count);
		Assert.AreEqual(KeyList<int>.ChangedAction.Set, changes[0].Action);
		Assert.AreSame(first, changes[0].Values);
		Assert.AreEqual(expected: 1, keyList.Count());
		Assert.AreEqual(expected: 1, ((IEnumerable<KeyValuePair<IEntity, List<int>>>) keyList).Count());
		Assert.AreEqual(expected: 1, ((IEnumerable) keyList).Cast<object>().Count());
	}

	[TestMethod]
	public void StateMachineContextCreatesAndCachesSystemDataWithAndWithoutProviders()
	{
		var ioProcessor = new Mock<IIoProcessor>();
		ioProcessor.SetupGet(static p => p.Id).Returns(new FullUri("processor"));
		ioProcessor.SetupGet(static p => p.Target).Returns(new FullUri("https://example.test/events"));
		var platformProperty = new Mock<IXDataModelProperty>();
		platformProperty.SetupGet(static p => p.Name).Returns("property");
		platformProperty.SetupGet(static p => p.Value).Returns(new DataModelValue("platform-value"));
		var context = CreateContext([ioProcessor.Object], [platformProperty.Object]);

		var dataModel = context.DataModel;

		Assert.AreSame(dataModel, context.DataModel);
		Assert.AreEqual("machine", dataModel["_name"].AsString());
		Assert.AreEqual("session", dataModel["_sessionid"].AsString());
		Assert.AreEqual(DataModelValueType.Undefined, dataModel["_event"].Type);
		Assert.AreEqual("https://example.test/events", dataModel["_ioprocessors"].AsListOrEmpty()["processor"].AsListOrEmpty()["location"].AsString());
		Assert.AreEqual("platform-value", dataModel["_x"].AsListOrEmpty()["property"].AsString());
		Assert.IsTrue(dataModel.ContainsKey("_NAME", caseInsensitive: true));
		Assert.AreEqual(expected: 0, context.Configuration.Count);
		Assert.AreEqual(expected: 0, context.HistoryValue.Count());
		Assert.AreEqual(expected: 0, context.InternalQueue.Count);
		Assert.AreEqual(expected: 0, context.StatesToInvoke.Count);
		Assert.AreEqual(expected: 0, context.ActiveInvokes.Count);

		var emptyContext = CreateContext([], []);
		Assert.AreEqual(expected: 0, emptyContext.DataModel["_ioprocessors"].AsListOrEmpty().Count);
		Assert.AreEqual(expected: 0, emptyContext.DataModel["_x"].AsListOrEmpty().Count);
	}

	[TestMethod]
	public void RuntimeErrorCreatesClassifiesAndRejectsErrorsByScopeOwnership()
	{
		var errors = new StateMachineRuntimeError(new ScopeObject());
		var otherScope = new StateMachineRuntimeError(new ScopeObject());
		var inner = new InvalidOperationException("failure");
		var sendId = SendId.FromString("send-id");

		var liveLock = errors.LiveLockError();
		var queueClosed = errors.QueueClosedError();
		var destroySignal = errors.DestroySignalError(inner);
		Assert.AreEqual(DestroyReason.LiveLock, liveLock.Reason);
		Assert.AreEqual(DestroyReason.QueueClosed, queueClosed.Reason);
		Assert.AreEqual(DestroyReason.DestroySignal, destroySignal.Reason);
		Assert.AreSame(inner, destroySignal.InnerException);
		Assert.IsTrue(errors.IsDestroyError(liveLock));
		Assert.IsTrue(errors.IsDestroyError(new Exception("outer", queueClosed)));
		Assert.IsFalse(otherScope.IsDestroyError(liveLock));

		var platformMessage = errors.PlatformError("message");
		var platformInner = errors.PlatformError(inner);
		Assert.AreEqual("message", platformMessage.Message);
		Assert.AreSame(inner, platformInner.InnerException);
		Assert.IsTrue(errors.IsPlatformError(platformMessage));
		Assert.IsFalse(otherScope.IsPlatformError(platformMessage));

		var communication = errors.CommunicationError(inner, sendId);
		Assert.IsTrue(errors.IsCommunicationError(communication, out var actualSendId));
		Assert.AreSame(sendId, actualSendId);
		Assert.IsFalse(otherScope.IsCommunicationError(communication, out actualSendId));
		Assert.IsNull(actualSendId);
		Assert.IsTrue(errors.IsCommunicationError(errors.NoExternalConnections(), out actualSendId));
		Assert.IsNull(actualSendId);
	}

	private static StateMachineContext CreateContext(IReadOnlyCollection<IIoProcessor> ioProcessors, IReadOnlyCollection<IXDataModelProperty> properties) =>
		new()
		{
			CaseSensitivity = Mock.Of<ICaseSensitivity>(static c => c.CaseInsensitive),
			StateMachine = Mock.Of<IStateMachine>(static machine => machine.Name == "machine"),
			IoProcessors = ioProcessors,
			XDataModelProperties = properties,
			StateMachineSessionId = Mock.Of<IStateMachineSessionId>(static session => session.SessionId == SessionId.FromString("session"))
		};
}
