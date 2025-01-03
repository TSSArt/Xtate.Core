// Copyright © 2019-2025 Sergii Artemenko
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

using Xtate.Core.Test.Legacy;
using Xtate.DataModel;
using Xtate.IoC;
using Xtate.Persistence;

namespace Xtate.Core.Test;

[TestClass]
public class StateMachinePersistingInterpreterTest
{
	private Mock<ICaseSensitivity> _caseSensitivityMock = null!;

	private Container _container;

	private Mock<IEventQueueReader> _eventQueueReaderMock = null!;

	private InMemoryStorage _inMemoryStorage;

	private IStateMachineInterpreter _interpreter = null!;

	private Mock<IInvokeController> _invokeControllerMock = null!;

	private Mock<ILogger<IStateMachineInterpreter>> _loggerMock = null!;

	private Mock<INotifyStateChanged> _notifyStateChangedMock = null!;

	private Mock<IPersistingInterpreterState> _persistingInterpreterState = null!;

	private Mock<IStateMachineArguments> _stateMachineArgumentsMock = null!;

	private StateMachineRuntimeError _stateMachineRuntimeError;

	private Mock<IStateMachineSessionId> _stateMachineSessionId;

	private Mock<IUnhandledErrorBehaviour> _unhandledErrorBehaviourMock = null!;

	[TestInitialize]
	public async Task Setup()
	{
		//var stateMachine = new ScxmlStringStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'><final id='aa'/></scxml>");
		var stateMachine = new LocationStateMachine(new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Legacy/test.scxml"));
		var noStateMachineContext = new NoStateMachineContext();

		_container = Container.Create<InterpreterModelBuilderModule>(
			services =>
			{
				stateMachine.AddServices(services);
				services.AddImplementation<TestDataModelHandler>().For<IDataModelHandler>();
				services.AddImplementation<InterpreterModelPersistenceTest.DummyResourceLoader>().For<IResourceLoader>();
				services.AddConstant<IStateMachineContext>(noStateMachineContext);
			});
		var interpreterModelBuilder = await _container.GetRequiredService<InterpreterModelBuilder>();
		var interpreterModel = await interpreterModelBuilder.BuildModel(true);

		_inMemoryStorage = new InMemoryStorage(false);

		_caseSensitivityMock = new Mock<ICaseSensitivity>();
		_stateMachineArgumentsMock = new Mock<IStateMachineArguments>();
		_eventQueueReaderMock = new Mock<IEventQueueReader>();
		_loggerMock = new Mock<ILogger<IStateMachineInterpreter>>();
		_notifyStateChangedMock = new Mock<INotifyStateChanged>();
		_unhandledErrorBehaviourMock = new Mock<IUnhandledErrorBehaviour>();
		_persistingInterpreterState = new Mock<IPersistingInterpreterState>();
		_invokeControllerMock = new Mock<IInvokeController>();
		_stateMachineSessionId = new Mock<IStateMachineSessionId>();

		_stateMachineSessionId.SetupGet(x => x.SessionId).Returns(SessionId.New());
		_persistingInterpreterState.Setup(x => x.StateBucket).Returns(new Bucket(_inMemoryStorage));
		_unhandledErrorBehaviourMock.Setup(x => x.Behaviour).Returns(UnhandledErrorBehaviour.IgnoreError);

		_stateMachineRuntimeError = new StateMachineRuntimeError { StateMachineSessionId = _stateMachineSessionId.Object };

		_interpreter = new StateMachinePersistingInterpreter
					   {
						   CaseSensitivity = _caseSensitivityMock.Object,
						   StateMachineRuntimeError = _stateMachineRuntimeError,
						   StateMachineArguments = _stateMachineArgumentsMock.Object,
						   DataConverter = new DataConverter(_caseSensitivityMock.Object),
						   EventQueueReader = _eventQueueReaderMock.Object,
						   Logger = _loggerMock.Object,
						   Model = interpreterModel,
						   InterpreterModel = interpreterModel,
						   NotifyStateChanged = _notifyStateChangedMock.Object,
						   UnhandledErrorBehaviour = _unhandledErrorBehaviourMock.Object,
						   StateMachineContext = noStateMachineContext,
						   PersistingInterpreterState = _persistingInterpreterState.Object,
						   InvokeController = _invokeControllerMock.Object
					   };
	}

	[TestCleanup]
	public async Task Cleanup()
	{
		await _container.DisposeAsync();
	}

    [TestMethod]
    public async Task TestInterpreterRunAsync()
    {
        await _interpreter.RunAsync();

        Assert.AreEqual(expected: 11, _inMemoryStorage.GetDataSize());
    }
}