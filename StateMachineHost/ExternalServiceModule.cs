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

using Xtate.ExternalService;
using Xtate.IoC;
using Xtate.IoProcessor;

namespace Xtate.Core;

public class ExternalServiceModule : Module
{
    protected override void AddServices()
    {
        Services.AddImplementation<ExternalServiceManager>().For<IExternalServiceManager>();
        Services.AddImplementation<ExternalServiceEventRouter>().For<IEventRouter>();
        Services.AddFactory<ExternalServiceFactory>().For<IExternalService>(SharedWithin.Scope);
        Services.AddType<ExternalServiceClass, InvokeData>();

        Services.AddSharedImplementation<ExternalServiceGlobalCollection>(SharedWithin.Container).For<IExternalServiceGlobalCollection>();
        Services.AddSharedImplementation<ExternalServiceCollection>(SharedWithin.Scope).For<IExternalServiceCollection>();
        Services.AddSharedImplementation<ExternalServiceScopeManager>(SharedWithin.Scope).For<IExternalServiceScopeManager>();
        Services.AddSharedImplementation<ExternalServiceRunner>(SharedWithin.Scope).For<IExternalServiceRunner>();
    }
}