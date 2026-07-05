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
using Xtate.Interpreter;
using Xtate.Interpreter.DependencyInjection;
using Xtate.IoBoundTask;
using Xtate.IoC;
using Xtate.StateMachineHost.Services;
using Xtate.TaskMonitor.DependencyInjection;

namespace Xtate.StateMachineHost.DependencyInjection;

public class StateMachineProcessorModule : Module<ExternalServiceModule, EventSchedulerModule, StateMachineInterpreterModule, TaskMonitorModule>
{
	protected override void AddServices()
	{
		Services.AddSharedFactorySync<SecurityContextFactory>(SharedWithin.Container).For<IIoBoundTask>().For<SecurityContextRegistration, SecurityContextType>(Option.DoNotDispose);

		Services.AddImplementation<ExternalCommunication>().For<IExternalCommunication>();

		Services.AddSharedImplementation<StateMachineScopeManager>(SharedWithin.Container).For<IStateMachineScopeManager>();
		Services.AddSharedImplementation<StateMachineCollection>(SharedWithin.Container).For<IStateMachineCollection>();
		
		Services.AddFactory<StateMachineDestroyOnIdle>().For<INotifyStateChanged>();
		Services.AddSharedImplementation<StateMachineRuntimeController>(SharedWithin.Scope).For<IStateMachineController>();
		Services.AddSharedImplementation<StateMachineStatus>(SharedWithin.Scope).For<INotifyStateChanged>().For<IStateMachineStatus>();

		Services.AddSharedImplementation<ScxmlIoProcessor>(SharedWithin.Scope).For<IIoProcessor>().For<IEventRouter>();
		Services.AddImplementation<InternalEventDispatcher<Any>>().For<IInternalEventDispatcher<Any>>();
		Services.AddImplementation<ExternalEventDispatcher<Any>>().For<IExternalEventDispatcher<Any>>();

		Services.AddImplementation<DeadLetterQueue<Any>>().For<IDeadLetterQueue<Any>>();
		Services.AddImplementation<StateMachineHostNew>().For<IStateMachineHostNew>();
	}
}