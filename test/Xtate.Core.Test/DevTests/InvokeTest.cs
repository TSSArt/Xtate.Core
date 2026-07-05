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
using Xtate.DataModel;
using Xtate.Interpreter;
using Xtate.Interpreter.DependencyInjection;
using Xtate.IoC;
using Xtate.Logging;
using Xtate.Logging.Provider;
using Xtate.StateMachine;

namespace Xtate.Test;

[TestClass]
public class InvokeTest
{
    private Mock<IExternalServiceManager> _externalCommunicationMock = null!;

    private Mock<IInvokeController> _invokeControllerMock = null!;

    private Mock<ILogProvider<IEventController>> _loggerMockE = null!;

    private Mock<ILogProvider<IStateMachineInterpreter>> _loggerMockI = null!;

    private Mock<ILogProvider<ILogController>> _loggerMockL = null!;

    private Mock<ILogProvider<IInvokeController>> _loggerMockV = null!;

    private StateMachineEntity _stateMachine;

    [TestInitialize]
    public void Initialize()
    {
        _stateMachine = new StateMachineEntity
                        {
                            States =
                            [
                                new StateEntity
                                {
                                    Id = (Identifier)"S1",
                                    Invoke =
                                    [
                                        new InvokeEntity
                                        {
                                            Id = "invoke_id",
                                            Source = new Uri("proto://src"),
                                            Type = new FullUri("proto://type"),
                                            Content = new ContentEntity { Body = new ContentBody { Value = "content" } },
                                            Finalize = new FinalizeEntity { Action = [new LogEntity { Label = "FinalizeExecuted" }] }
                                        }
                                    ],
                                    Transitions = [new TransitionEntity { EventDescriptors = [(EventDescriptor)"ToF"], Target = [(Identifier)"F"] }]
                                },
                                new FinalEntity { Id = (Identifier)"F" }
                            ]
                        };

        _invokeControllerMock = new Mock<IInvokeController>();
        _loggerMockL = new Mock<ILogProvider<ILogController>>();
        _loggerMockL.Setup(s => s.IsEnabled(Level.Info)).Returns(true);
        _loggerMockL.Setup(s => s.IsEnabled(Level.Trace)).Returns(true);
        _loggerMockI = new Mock<ILogProvider<IStateMachineInterpreter>>();
        _loggerMockI.Setup(s => s.IsEnabled(Level.Info)).Returns(true);
        _loggerMockI.Setup(s => s.IsEnabled(Level.Trace)).Returns(true);
        _loggerMockE = new Mock<ILogProvider<IEventController>>();
        _loggerMockE.Setup(s => s.IsEnabled(Level.Info)).Returns(true);
        _loggerMockE.Setup(s => s.IsEnabled(Level.Trace)).Returns(true);
        _loggerMockV = new Mock<ILogProvider<IInvokeController>>();
        _loggerMockV.Setup(s => s.IsEnabled(Level.Info)).Returns(true);
        _loggerMockV.Setup(s => s.IsEnabled(Level.Trace)).Returns(true);

        _externalCommunicationMock = new Mock<IExternalServiceManager>();
    }

    private static IncomingEvent CreateEventObject(string name, InvokeId? invokeId = null) =>
        new()
        {
            Type = EventType.External,
            Name = (EventName)name,
            InvokeId = invokeId
        };

    [TestMethod]
    public async Task SimpleTest()
    {
        var invokeUniqueId = "";
        _externalCommunicationMock.Setup(l => l.Start(It.IsAny<InvokeData>()))
                                  .Callback<InvokeData>(data => invokeUniqueId = data.InvokeId.UniqueId.Value);

        var services = new ServiceCollection();
        services.AddModule<StateMachineInterpreterModule>();
        services.AddConstant<IStateMachine>(_stateMachine);
        services.AddConstant(_loggerMockL.Object);
        services.AddConstant(_loggerMockI.Object);
        services.AddConstant(_loggerMockE.Object);
        services.AddConstant(_loggerMockV.Object);
        services.AddConstant(_externalCommunicationMock.Object);

        var serviceProvider = services.BuildProvider();
        var stateMachineInterpreter = await serviceProvider.GetRequiredService<IStateMachineInterpreter>();
        var eventQueueWriter = await serviceProvider.GetRequiredService<IEventDispatcher>();

        var task = stateMachineInterpreter.Run();

        await eventQueueWriter.Dispatch(CreateEventObject(name: "fromInvoked", InvokeId.FromString(invokeId: "invoke_id", invokeUniqueId)), token: CancellationToken.None);
        await eventQueueWriter.Dispatch(CreateEventObject("ToF"), token: CancellationToken.None);
        await task;

        _externalCommunicationMock.Verify(l => l.Start(It.IsAny<InvokeData>()));
        _externalCommunicationMock.Verify(l => l.Cancel(InvokeId.FromString("invoke_id", invokeUniqueId)));
        _externalCommunicationMock.VerifyNoOtherCalls();

        _loggerMockL.Verify(l => l.Write(Level.Info, 1, "FinalizeExecuted", It.IsAny<IEnumerable<LoggingParameter>>()));
        _loggerMockV.Verify(l => l.Write(Level.Trace, 1, It.Is<string>(v => v.StartsWith("Start")), It.IsAny<IEnumerable<LoggingParameter>>()));
        _loggerMockV.Verify(l => l.Write(Level.Trace, 2, It.Is<string>(v => v.StartsWith("Cancel")), It.IsAny<IEnumerable<LoggingParameter>>()));
        _loggerMockL.Verify(l => l.IsEnabled(It.IsAny<Level>()));
        _loggerMockV.Verify(l => l.IsEnabled(It.IsAny<Level>()));
        _loggerMockV.Verify(l => l.Write(Level.Trace, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IEnumerable<LoggingParameter>>()));
        _loggerMockL.VerifyNoOtherCalls();
        _loggerMockE.VerifyNoOtherCalls();
        _loggerMockV.VerifyNoOtherCalls();
    }
}