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

using System.Threading;
using Xtate.Core.Test.Legacy;
using Xtate.DataModel;
using Xtate.IoC;
using Xtate.Persistence;
using Xtate.Test;

namespace Xtate.Core.Test;

[TestClass]
public class StateMachinePersistingInterpreterTest
{
    [TestMethod]
    public async Task TestInterpreterRunAsync()
    {
        var stateMachine = new LocationStateMachine(new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Legacy/test.scxml"));
        var noStateMachineContext = new NoStateMachineContext();

        await using var container = Container.Create<InterpreterModelBuilderModule>(services =>
                                                                                    {
                                                                                        stateMachine.AddServices(services);
                                                                                        services.AddImplementation<TestDataModelHandler>().For<IDataModelHandler>();
                                                                                        services.AddImplementation<InterpreterModelPersistenceTest.DummyResourceLoader>().For<IResourceLoader>();
                                                                                        services.AddConstant<IStateMachineContext>(noStateMachineContext);
                                                                                    });
        var interpreterModelBuilder = await container.GetRequiredService<InterpreterModelBuilder>();
        var interpreterModel = await interpreterModelBuilder.BuildModel(true);

        var inMemoryStorage = new InMemoryStorage(false);

        var caseSensitivityMock = new Mock<ICaseSensitivity>();
        var stateMachineArgumentsMock = new Mock<IStateMachineArguments>();
        var eventQueueReaderMock = new Mock<IEventQueueReader>();
        var loggerMock = new Mock<ILogger<IStateMachineInterpreter>>();
        var notifyStateChangedMock = new Mock<INotifyStateChanged>();
        var unhandledErrorBehaviourMock = new Mock<IUnhandledErrorBehaviour>();
        var invokeControllerMock = new Mock<IInvokeController>();
        var stateMachineSessionId = new Mock<IStateMachineSessionId>();
        var suspendManagerMock = new Mock<ISuspendEventDispatcher>();
        var persistenceOptionsMock = new Mock<IPersistenceOptions>();

        stateMachineSessionId.SetupGet(x => x.SessionId).Returns(SessionId.New());
        unhandledErrorBehaviourMock.Setup(x => x.Behaviour).Returns(UnhandledErrorBehaviour.IgnoreError);

        var stateMachineRuntimeError = new StateMachineRuntimeError { StateMachineSessionId = stateMachineSessionId.Object };

        var interpreter = new StateMachinePersistingInterpreter
                          {
                              CaseSensitivity = caseSensitivityMock.Object,
                              StateMachineRuntimeError = stateMachineRuntimeError,
                              StateMachineArguments = stateMachineArgumentsMock.Object,
                              DataConverter = new DataConverter(caseSensitivityMock.Object),
                              EventQueueReader = eventQueueReaderMock.Object,
                              Logger = loggerMock.Object,
                              Model = interpreterModel,
                              InterpreterModel = interpreterModel,
                              NotifyStateChanged = notifyStateChangedMock.Object,
                              UnhandledErrorBehaviour = unhandledErrorBehaviourMock.Object,
                              StateMachineContext = noStateMachineContext,
                              InvokeController = invokeControllerMock.Object,
                              StateStorage = inMemoryStorage,
                              SuspendEventDispatcher = suspendManagerMock.Object,
                              PersistenceOptions = persistenceOptionsMock.Object
                          };
        await interpreter.RunAsync();

        Assert.AreEqual(expected: 11, inMemoryStorage.GetDataSize());
    }

    [TestMethod]
    public async Task SuspendResumeTest()
    {
        var stateMachine = new ScxmlStringStateMachine(
            """
            <scxml xmlns="http://www.w3.org/2005/07/scxml" xmlns:sys="http://xtate.net/scxml/system" version="1.0">
              <state id="before">
                <transition event="complete" target="after"/>
              </state>
              <final id="after">
                <donedata><content>Hello</content></donedata>
              </final>
            </scxml>
            """);

        await using var container = Container.Create<StateMachineProcessorModule, DebugTraceModule, PersistenceModule>(services =>
                                                                                                                       {
                                                                                                                           var storageProvider = new StateMachinePersistenceTest.TestStorage();
                                                                                                                           services.AddConstant<IStorageProvider>(storageProvider);
                                                                                                                       });

        var stateMachineScopeManager = await container.GetRequiredService<IStateMachineScopeManager>();
        var stateMachineCollection = await container.GetRequiredService<IStateMachineCollection>();
        var executeTask = stateMachineScopeManager.Execute(stateMachine, SecurityContextType.NewStateMachine);

        await stateMachineCollection.Dispatch(stateMachine.SessionId, new IncomingEvent(new EventEntity("complete")) { Type = EventType.External }, CancellationToken.None);

        var result = await executeTask;

        Assert.AreEqual(expected: "Hello", result);
    }
}