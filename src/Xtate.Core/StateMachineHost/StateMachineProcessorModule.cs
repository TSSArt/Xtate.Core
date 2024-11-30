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

using Xtate.DataModel;
using Xtate.ExternalService;
using Xtate.IoC;
using Xtate.IoProcessor;

namespace Xtate.Core;

public class StateMachineProcessorModule : Module<ExternalServiceModule, EventSchedulerModule, StateMachineInterpreterModule>
{
	protected override void AddServices()
	{
		Services.AddSharedFactorySync<SecurityContextFactory>(SharedWithin.Container).For<IIoBoundTask>().For<SecurityContextRegistration, SecurityContextType>(Option.DoNotDispose);
		
		Services.AddType<StateMachineExternalService>();
		Services.AddImplementation<StateMachineExternalService.Provider>().For<IExternalServiceProvider>();
		Services.AddImplementation<LocationChildStateMachine, (Uri, DataModelValue)>().For<StateMachineClass>();
		Services.AddImplementation<ScxmlStringChildStateMachine, (string, Uri?, DataModelValue)>().For<StateMachineClass>();

		Services.AddImplementation<ExternalCommunication>().For<IExternalCommunication>();
		
		Services.AddSharedImplementation<StateMachineScopeManager>(SharedWithin.Container).For<IStateMachineScopeManager>();
		Services.AddSharedImplementation<StateMachineCollection>(SharedWithin.Container).For<IStateMachineCollection>();
		Services.AddSharedImplementation<StateMachineRunner>(SharedWithin.Scope).For<IStateMachineRunner>();
		Services.AddSharedImplementation<StateMachineRuntimeController>(SharedWithin.Scope).For<IStateMachineController>();
		Services.AddSharedImplementation<StateMachineStatus>(SharedWithin.Scope).For<INotifyStateChanged>().For<IStateMachineStatus>();

		Services.AddImplementation<ScxmlIoProcessor>().For<IIoProcessor>().For<IEventRouter>();
	}
}