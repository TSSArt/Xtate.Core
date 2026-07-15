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

using System.IO;
using System.Threading;
using System.Xml;
using Xtate.Actions.System;
using Xtate.Class;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.IoC.Tools;
using Xtate.StateMachine;
using Xtate.StateMachine.Validator;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;
using Xtate.TaskMonitor;
using MonitoredTask = Xtate.TaskMonitor.Services.TaskMonitor;

namespace Xtate.Test.UnitTests.Actions;

[TestClass]
public class SystemActionCoverageTest
{
	[TestMethod]
	public void StartActionValidatesAttributesAndExposesValueAndLocationDescriptors()
	{
		var missingErrors = new ErrorCollector<StartAction>();
		_ = CreateStart("<start />", missingErrors);
		Assert.HasCount(expected: 1, missingErrors.Messages);

		var conflictingErrors = new ErrorCollector<StartAction>();
		_ = CreateStart("<start url='child.scxml' urlExpr='expr' sessionId='' sessionIdExpr='sessionExpr' />", conflictingErrors);
		Assert.HasCount(expected: 3, conflictingErrors.Messages);

		var invalidErrors = new ErrorCollector<StartAction>();
		_ = CreateStart("<start url='http://[invalid' />", invalidErrors);
		Assert.HasCount(expected: 1, invalidErrors.Messages);

		var validErrors = new ErrorCollector<StartAction>();
		var action = CreateStart("<start url='child.scxml' sessionId='child-session' sessionIdLocation='result' trusted='true' />", validErrors);
		var contract = (IAction) action;
		var values = contract.GetValues().ToArray();
		var locations = contract.GetLocations().ToArray();

		Assert.IsEmpty(validErrors.Messages);
		Assert.HasCount(expected: 2, values);
		Assert.IsNull(values[0].Expression);
		Assert.IsNull(values[1].Expression);
		Assert.HasCount(expected: 1, locations);
	}

	[TestMethod]
	public async Task StartActionResolvesLocationRoutesSecurityAndAssignsSessionId()
	{
		var scopeManager = new Mock<IStateMachineScopeManager>();
		var starts = new List<(StateMachineClass StateMachine, SecurityContextType SecurityContext)>();
		scopeManager
			.Setup(static manager => manager.Start(It.IsAny<StateMachineClass>(), It.IsAny<SecurityContextType>()))
			.Callback<StateMachineClass, SecurityContextType>((stateMachine, securityContext) => starts.Add((stateMachine, securityContext)))
			.Returns(new ValueTask<StateMachineResult>(new StateMachineResult(Task.FromResult(DataModelValue.Undefined))));
		var taskMonitor = new MonitoredTask { Logger = null! };
		var baseLocation = new Uri("https://example.test/machines/root.scxml");
		var assignedLocation = new CapturingLocationEvaluator();
		var trusted = CreateStart(
			"<start url='child.scxml' sessionId='child-session' sessionIdLocation='result' trusted='true' />",
			new ErrorCollector<StartAction>(), baseLocation, scopeManager.Object, taskMonitor);
		((IAction) trusted).GetLocations().Single().SetEvaluator(assignedLocation);

		await ((IAction) trusted).Execute();

		var untrusted = CreateStart("<start url='other.scxml' />", new ErrorCollector<StartAction>(), baseLocation, scopeManager.Object, taskMonitor);
		await ((IAction) untrusted).Execute();

		Assert.HasCount(expected: 2, starts);
		Assert.AreEqual(SecurityContextType.NewTrustedStateMachine, starts[0].SecurityContext);
		Assert.AreEqual(SecurityContextType.NewStateMachine, starts[1].SecurityContext);
		Assert.AreEqual(new Uri("https://example.test/machines/child.scxml"), ((IStateMachineLocation) starts[0].StateMachine).Location);
		Assert.AreEqual(new Uri("https://example.test/machines/other.scxml"), ((IStateMachineLocation) starts[1].StateMachine).Location);
		Assert.AreEqual("child-session", starts[0].StateMachine.SessionId.ToString());
		Assert.IsFalse(string.IsNullOrEmpty(starts[1].StateMachine.SessionId.ToString()));
		Assert.AreEqual("child-session", DataModelValue.FromObject(assignedLocation.Value!.ToObject()).AsString());
	}

	[TestMethod]
	public async Task StartActionRejectsInvalidLocationAndEmptyEvaluatedSessionAtExecution()
	{
		var invalidLocation = CreateStart("<start url='http://[invalid' />", new ErrorCollector<StartAction>());
		var emptySession = CreateStart("<start url='child.scxml' sessionId='' />", new ErrorCollector<StartAction>());

		await Assert.ThrowsExactlyAsync<ProcessorException>([ExcludeFromCodeCoverage] async () => await ((IAction) invalidLocation).Execute());
		await Assert.ThrowsExactlyAsync<ProcessorException>([ExcludeFromCodeCoverage] async () => await ((IAction) emptySession).Execute());
	}

	[TestMethod]
	public async Task DestroyActionValidatesDescriptorsDestroysLiteralSessionAndRejectsEmptySession()
	{
		var missingErrors = new ErrorCollector<DestroyAction>();
		_ = CreateDestroy("<destroy />", missingErrors);
		Assert.HasCount(expected: 1, missingErrors.Messages);

		var conflictErrors = new ErrorCollector<DestroyAction>();
		_ = CreateDestroy("<destroy sessionId='' sessionIdExpr='expr' />", conflictErrors);
		Assert.HasCount(expected: 2, conflictErrors.Messages);

		var collection = new Mock<IStateMachineCollection>();
		SessionId? destroyed = null;
		collection.Setup(static value => value.Destroy(It.IsAny<SessionId>()))
				  .Callback<SessionId>(sessionId => destroyed = sessionId)
				  .Returns(ValueTask.CompletedTask);
		var action = CreateDestroy(
			"<destroy sessionId='child-session' />", new ErrorCollector<DestroyAction>(), collection.Object,
			new MonitoredTask { Logger = null! });

		Assert.HasCount(expected: 1, ((IAction) action).GetValues().ToArray());
		Assert.IsEmpty(((IAction) action).GetLocations());
		await ((IAction) action).Execute();
		Assert.AreEqual("child-session", destroyed!.ToString());

		var empty = CreateDestroy("<destroy sessionId='' />", new ErrorCollector<DestroyAction>());
		await Assert.ThrowsExactlyAsync<ProcessorException>([ExcludeFromCodeCoverage] async () => await ((IAction) empty).Execute());
	}

	[TestMethod]
	public void StartActionPreservesSessionIdLocationExpression()
	{
		// Current product defect: StartAction should pass sessionIdLocation to its Location constructor.
		var action = CreateStart("<start url='child.scxml' sessionIdLocation='result.location' />", new ErrorCollector<StartAction>());

		Assert.AreEqual("result.location", ((IAction) action).GetLocations().Single().Expression);
	}

	private static StartAction CreateStart(
		string xml,
		ErrorCollector<StartAction> errors,
		Uri? location = null,
		IStateMachineScopeManager? scopeManager = null,
		ITaskMonitor? taskMonitor = null)
	{
		using var reader = CreateReader(xml);
		return new StartAction(reader, errors)
			   {
				   DisposeToken = default,
				   TaskMonitor = taskMonitor is null ? null! : () => new ValueTask<ITaskMonitor>(taskMonitor),
				   StateMachineLocation = location is null ? null! : new StateMachineLocation(location),
				   StateMachineScopeManager = scopeManager is null ? null! : () => new ValueTask<IStateMachineScopeManager>(scopeManager)
			   };
	}

	private static DestroyAction CreateDestroy(
		string xml,
		ErrorCollector<DestroyAction> errors,
		IStateMachineCollection? collection = null,
		ITaskMonitor? taskMonitor = null)
	{
		using var reader = CreateReader(xml);
		return new DestroyAction(reader, errors)
			   {
				   DisposeToken = default,
				   TaskMonitor = taskMonitor!,
				   StateMachineCollection = collection!
			   };
	}

	private static XmlReader CreateReader(string xml)
	{
		var reader = XmlReader.Create(new StringReader(xml));
		reader.MoveToContent();
		return reader;
	}

	private sealed class ErrorCollector<T> : IErrorProcessorService<T>
	{
		public List<string> Messages { get; } = [];

		public void AddError(object? entity, string message, Exception? exception = null) => Messages.Add(message);
	}

	private sealed class StateMachineLocation(Uri location) : IStateMachineLocation
	{
		public Uri? Location { get; } = location;
	}

	private sealed class CapturingLocationEvaluator : ILocationEvaluator
	{
		public IObject? Value { get; private set; }

		public ValueTask SetValue(IObject value)
		{
			Value = value;
			return ValueTask.CompletedTask;
		}

		[ExcludeFromCodeCoverage]
		public ValueTask<IObject> GetValue() => new(Value ?? DataModelValue.Undefined);

		[ExcludeFromCodeCoverage]
		public ValueTask<string> GetName() => new("result");
	}
}
