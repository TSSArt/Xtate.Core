﻿// Copyright © 2019-2024 Sergii Artemenko
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

using Xtate.IoC;
using Xtate.Service;

namespace Xtate.Core;

public class StateMachineHostModule : Module<StateMachineInterpreterModule>
{
	protected override void AddServices()
	{
		//TODO: tmp ----
		Services.AddType<StateMachineHostOptions>();
		//TODO: tmp ----

		//Services.AddImplementation<SecurityContext>().For<SecurityContext>();
		Services.AddImplementation<InProcEventSchedulerFactory>().For<IEventSchedulerFactory>();

		Services.AddImplementation<StateMachineHostExternalCommunication>().For<IExternalCommunication>();

		//Services.AddImplementation<ScopeManager, Action<IServiceCollection>>().For<IScopeManager>();
		Services.AddSharedImplementation<StateMachineRuntimeController>(SharedWithin.Scope).For<IStateMachineController>();
				//.For<IInvokeController>();
				//.For<INotifyStateChanged>();//
				//.For<IExternalCommunication>();

		Services.AddSharedImplementation<StateMachineRunner>(SharedWithin.Scope).For<IStateMachineRunner>();
		Services.AddSharedImplementation<StateMachineHost>(SharedWithin.Container).For<StateMachineHost>().For<IStateMachineHost>().For<IServiceFactory>().For<IHostController>(); //TODO: Make only interface
		Services.AddSharedImplementation<StateMachineHostContext>(SharedWithin.Container).For<StateMachineHostContext>().For<IStateMachineHostContext>();                //TODO: Make only interface

		Services.AddSharedFactorySync<SecurityContextFactory>(SharedWithin.Container).For<IIoBoundTask>().For<SecurityContextRegistration, SecurityContextType>();
		//Services.AddSharedType<SecurityContextController>(SharedWithin.Container);

	}
}