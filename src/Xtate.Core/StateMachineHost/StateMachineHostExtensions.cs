#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using Xtate.IoC;
using Xtate.Service;

namespace Xtate.Core;

public static class StateMachineHostExtensions
{
	public static void RegisterStateMachineHost(this IServiceCollection services)
	{
		if (!services.IsRegistered<StateMachineHost>())
		{
			services.RegisterStateMachineFactory();
			services.RegisterStateMachineInterpreter();

			//TODO: tmp ----
			services.AddType<StateMachineHostOptions>();
			//services.AddForwarding(sp => new ServiceLocator(sp));

			//TODO: tmp ----

			services.AddImplementation<InProcEventSchedulerFactory>().For<IEventSchedulerFactory>();

			services.AddSharedImplementation<ScopeManager>(SharedWithin.Scope).For<IScopeManager>();
			services.AddSharedImplementation<StateMachineRuntimeController>(SharedWithin.Scope)
					.For<IStateMachineController>()
					.For<IInvokeController>()
					.For<INotifyStateChanged>()
					.For<IExternalCommunication>();

			services.AddSharedImplementation<StateMachineHost>(SharedWithin.Container).For<StateMachineHost>().For<IStateMachineHost>().For<IServiceFactory>(); //TODO: Make only interface
			services.AddSharedImplementation<StateMachineHostContext>(SharedWithin.Container).For<StateMachineHostContext>().For<IStateMachineHostContext>();   //TODO: Make only interface
		}
	}
}