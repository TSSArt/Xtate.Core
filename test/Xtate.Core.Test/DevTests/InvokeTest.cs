// Copyright © 2019-2024 Sergii Artemenko
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

using Xtate.Core;
using Xtate.IoC;

namespace Xtate.Test;

[TestClass]
public class InvokeTest
{
	private Mock<IInvokeController> _invokeControllerMock = default!;
	private Mock<ILogWriter>        _loggerMock           = default!;
	private StateMachineEntity      _stateMachine;

	[TestInitialize]
	public void Initialize()
	{
		_stateMachine = new StateMachineEntity
						{
							States =
							[
								new StateEntity
								{
									Id = (Identifier) "S1",
									Invoke =
									[
										new InvokeEntity
										{
											Id = "invoke_id",
											Source = new Uri("proto://src"),
											Type = new Uri("proto://type"),
											Content = new ContentEntity { Body = new ContentBody { Value = "content" } },
											Finalize = new FinalizeEntity { Action = [new LogEntity { Label = "FinalizeExecuted" }] }
										}
									],
									Transitions = [new TransitionEntity { EventDescriptors = [(EventDescriptor) "ToF"], Target = [(Identifier) "F"] }]
								},
								new FinalEntity { Id = (Identifier) "F" }
							]
						};

		_invokeControllerMock = new Mock<IInvokeController>();
		_loggerMock = new Mock<ILogWriter>();
		_loggerMock.Setup(s => s.IsEnabled(Level.Info)).Returns(true);
		_loggerMock.Setup(s => s.IsEnabled(Level.Trace)).Returns(true);
	}

	private static EventObject CreateEventObject(string name, InvokeId? invokeId = default) =>
		new()
		{
			Type = EventType.External,
			NameParts = EventName.ToParts(name),
			InvokeId = invokeId
		};

	[TestMethod]
	public async Task SimpleTest()
	{
		var invokeUniqueId = "";
		_invokeControllerMock.Setup(l => l.Start(It.IsAny<InvokeData>()))
							 .Callback<InvokeData>(data => invokeUniqueId = data.InvokeId.InvokeUniqueIdValue);

		var services = new ServiceCollection();
		services.AddForwarding<IStateMachine>(_ => _stateMachine);
		services.AddForwarding(_ => _invokeControllerMock.Object);
		services.AddForwarding<ILogWriter, Type>((_, _) => _loggerMock.Object);
		services.RegisterStateMachineInterpreter();

		var serviceProvider = services.BuildProvider();
		var stateMachineInterpreter = await serviceProvider.GetRequiredService<IStateMachineInterpreter>();
		var eventQueueWriter = await serviceProvider.GetRequiredService<IEventQueueWriter>();

		var task = stateMachineInterpreter.RunAsync();

		await eventQueueWriter.WriteAsync(CreateEventObject(name: "fromInvoked", InvokeId.FromString(invokeId: "invoke_id", invokeUniqueId)));
		await eventQueueWriter.WriteAsync(CreateEventObject("ToF"));
		await task;

		_invokeControllerMock.Verify(l => l.Start(It.IsAny<InvokeData>()));
		_invokeControllerMock.Verify(l => l.Cancel(InvokeId.FromString("invoke_id", invokeUniqueId)));
		_invokeControllerMock.VerifyNoOtherCalls();

		_loggerMock.Verify(l => l.Write(Level.Info, 1, "FinalizeExecuted", It.IsAny<IEnumerable<LoggingParameter>>()));
		_loggerMock.Verify(l => l.Write(Level.Trace, 1, It.Is<string>(v => v.StartsWith("Start")), It.IsAny<IEnumerable<LoggingParameter>>()));
		_loggerMock.Verify(l => l.Write(Level.Trace, 2, It.Is<string>(v => v.StartsWith("Cancel")), It.IsAny<IEnumerable<LoggingParameter>>()));
		_loggerMock.Verify(l => l.IsEnabled(It.IsAny<Level>()));
		_loggerMock.Verify(l => l.Write(Level.Trace, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IEnumerable<LoggingParameter>>()));
		_loggerMock.VerifyNoOtherCalls();
	}
}