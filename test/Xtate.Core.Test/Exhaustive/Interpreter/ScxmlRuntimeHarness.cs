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
using Xtate.Interpreter.Services;
using Xtate.IoC;
using Xtate.Logging;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.DependencyInjection;

namespace Xtate.Core.Test.Exhaustive.Interpreter;

internal static class ScxmlRuntimeHarness
{
	public static ValueTask<DataModelValue> ExecuteAsync(string scxml) => ExecuteAsync(scxml, notification: null, logger: null, actionLogger: null);

	public static ValueTask<DataModelValue> ExecuteAsync(string scxml, INotifyStateChanged? notification) => ExecuteAsync(scxml, notification, logger: null, actionLogger: null);

	public static async ValueTask<DataModelValue> ExecuteAsync(string scxml,
															   INotifyStateChanged? notification,
															   ILogger<StateMachineInterpreter>? logger,
															   ILogger<ILogController>? actionLogger = null)
	{
		await using var container = Container.Create<StateMachineProcessorModule>(services =>
																				  {
																					  if (notification is not null) services.AddConstant(notification);
																					  if (logger is not null) services.AddConstant(logger);
																					  if (actionLogger is not null) services.AddConstant(actionLogger);
																				  });
		var scopeManager = await container.GetRequiredService<IStateMachineScopeManager>();

		return await scopeManager.Execute(new ScxmlStringStateMachine(scxml), SecurityContextType.NewTrustedStateMachine);
	}

	public static ValueTask<DataModelValue> ExecuteWithExternalEventsAsync(string scxml, params string[] eventNames) => ExecuteWithExternalEventsAsync(scxml, logger: null, eventNames);

	public static async ValueTask<DataModelValue> ExecuteWithExternalEventsAsync(string scxml, ILogger<StateMachineInterpreter>? logger, params string[] eventNames)
	{
		await using var container = Container.Create<StateMachineProcessorModule>(services =>
																				  {
																					  if (logger is not null) services.AddConstant(logger);
																				  });
		var scopeManager = await container.GetRequiredService<IStateMachineScopeManager>();
		var collection = await container.GetRequiredService<IStateMachineCollection>();
		var stateMachine = new ScxmlStringStateMachine(scxml);
		var result = await scopeManager.Start(stateMachine, SecurityContextType.NewTrustedStateMachine);

		foreach (var eventName in eventNames)
		{
			await collection.Dispatch(stateMachine.SessionId, new IncomingEvent { Name = EventName.FromString(eventName), Type = EventType.External }, CancellationToken.None);
		}

		return await result.GetResult();
	}
}
