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

using System.Threading;
using Xtate.Class;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.IoC.Tools;
using Xtate.Scxml;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;
using Xtate.TaskMonitor;

namespace Xtate.Test.UnitTests.StateMachineHost;

[TestClass]
public class StateMachineExternalServiceCoverageTest
{
	[TestMethod]
	public async Task RawScxmlExecutionCreatesChildRoutesEventsAndSynchronousDisposeDestroysIt()
	{
		const string scxml = "<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'/>";
		var scopeManager = new Mock<IStateMachineScopeManager>();
		var collection = new Mock<IStateMachineCollection>();
		StateMachineClass? child = null;
		scopeManager.Setup(s => s.Execute(It.IsAny<StateMachineClass>(), SecurityContextType.InvokedService))
					.Callback<StateMachineClass, SecurityContextType>((stateMachineClass, _) => child = stateMachineClass)
					.ReturnsAsync(new DataModelValue("result"));
		var service = CreateService(scopeManager.Object, collection.Object, source: null, rawContent: scxml, DataModelValue.Undefined);

		await ((IEventDispatcher) service).Dispatch(Mock.Of<IIncomingEvent>(), CancellationToken.None);
		collection.VerifyNoOtherCalls();

		Assert.AreEqual(expected: "result", (await ((IExternalService) service).GetResult()).AsString());
		var scxmlChild = child as ScxmlStringChildStateMachine;
		Assert.IsNotNull(scxmlChild);
		Assert.AreEqual(new Uri("https://example.test/parent.scxml"), ((IStateMachineLocation) scxmlChild).Location);
		Assert.AreEqual(expected: "parameters", ((IStateMachineArguments) scxmlChild).Arguments.AsString());

		var incomingEvent = Mock.Of<IIncomingEvent>();
		await ((IEventDispatcher) service).Dispatch(incomingEvent, CancellationToken.None);
		collection.Verify(c => c.Dispatch(scxmlChild.SessionId, incomingEvent, It.IsAny<CancellationToken>()), Times.Once);

		service.Dispose();
		service.Dispose();
		collection.Verify(c => c.Destroy(scxmlChild.SessionId), Times.Once);
	}

	[TestMethod]
	public async Task SourceExecutionCreatesLocationChildAndAsynchronousDisposeDestroysIt()
	{
		var scopeManager = new Mock<IStateMachineScopeManager>();
		var collection = new Mock<IStateMachineCollection>();
		StateMachineClass? child = null;
		scopeManager.Setup(s => s.Execute(It.IsAny<StateMachineClass>(), SecurityContextType.InvokedService))
					.Callback<StateMachineClass, SecurityContextType>((stateMachineClass, _) => child = stateMachineClass)
					.ReturnsAsync(DataModelValue.Null);
		var service = CreateService(
			scopeManager.Object,
			collection.Object,
			new Uri("child.scxml", UriKind.Relative),
			rawContent: null,
			DataModelValue.Undefined);

		await ((IExternalService) service).GetResult();
		var locationChild = child as LocationChildStateMachine;
		Assert.IsNotNull(locationChild);
		Assert.AreEqual(new Uri("https://example.test/child.scxml"), ((IStateMachineLocation) locationChild).Location);
		Assert.AreEqual(expected: "parameters", ((IStateMachineArguments) locationChild).Arguments.AsString());

		await service.DisposeAsync();
		await service.DisposeAsync();
		collection.Verify(c => c.Destroy(locationChild.SessionId), Times.Once);
	}

	[TestMethod]
	public async Task ContentStringIsUsedWhenRawContentIsMissingAndMissingSourceFails()
	{
		var scopeManager = new Mock<IStateMachineScopeManager>();
		scopeManager.Setup(s => s.Execute(It.IsAny<ScxmlStringChildStateMachine>(), SecurityContextType.InvokedService))
					.ReturnsAsync(DataModelValue.Null);
		var collection = Mock.Of<IStateMachineCollection>();
		var fromContent = CreateService(scopeManager.Object, collection, source: null, rawContent: null, new DataModelValue("<scxml/>"));
		var missing = CreateService(scopeManager.Object, collection, source: null, rawContent: null, DataModelValue.Undefined);

		await ((IExternalService) fromContent).GetResult();
		scopeManager.Verify(s => s.Execute(It.IsAny<ScxmlStringChildStateMachine>(), SecurityContextType.InvokedService), Times.Once);
		await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () => await ((IExternalService) missing).GetResult());
	}

	[TestMethod]
	public void ProviderRecognizesScxmlServicePrimaryAndAliasTypes()
	{
		IExternalServiceProvider provider = new StateMachineExternalService.Provider
											  {
												  ServiceFactoryFunc = static () => new ValueTask<StateMachineExternalService>((StateMachineExternalService) null!)
											  };

		Assert.IsNotNull(provider.TryGetActivator(Const.ScxmlServiceTypeId));
		Assert.IsNotNull(provider.TryGetActivator(Const.ScxmlServiceAliasTypeId));
		Assert.IsNull(provider.TryGetActivator(new FullUri("urn:other")));
	}

	private static StateMachineExternalService CreateService(
		IStateMachineScopeManager scopeManager,
		IStateMachineCollection collection,
		Uri? source,
		string? rawContent,
		DataModelValue content)
	{
		var taskMonitor = new PassThroughTaskMonitor();

		return new StateMachineExternalService
			   {
				   StateMachineScopeManager = scopeManager,
				   StateMachineLocation = Mock.Of<IStateMachineLocation>(l => l.Location == new Uri("https://example.test/parent.scxml")),
				   StateMachineCollection = collection,
				   TaskMonitor = taskMonitor,
				   ExternalServiceSourceBase = new ExternalServiceSource(source, rawContent, content),
				   ExternalServiceParametersBase = new ExternalServiceParameters(new DataModelValue("parameters")),
				   TaskMonitorBase = taskMonitor,
				   DisposeTokenBase = new DisposeToken(CancellationToken.None)
			   };
	}

	private sealed class ExternalServiceSource(Uri? source, string? rawContent, DataModelValue content) : IExternalServiceSource
	{
		public Uri? Source { get; } = source;

		public string? RawContent { get; } = rawContent;

		public DataModelValue Content { get; } = content;
	}

	private sealed class ExternalServiceParameters(DataModelValue parameters) : IExternalServiceParameters
	{
		public DataModelValue Parameters { get; } = parameters;
	}

	[ExcludeFromCodeCoverage]
	private sealed class PassThroughTaskMonitor : ITaskMonitor
	{
		public Task WaitAsync(Task task, CancellationToken token) => task;

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task;

		public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => valueTask;

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) => valueTask;

		public void Forget(Task task) { }

		public void Forget(ValueTask valueTask) { }

		public void Forget<TResult>(ValueTask<TResult> valueTask) { }
	}
}
