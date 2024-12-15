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

		Services.AddSharedImplementation<ExternalServicePublicCollection>(SharedWithin.Container).For<IExternalServicePublicCollection>();
		Services.AddSharedImplementation<ExternalServiceCollection>(SharedWithin.Scope).For<IExternalServiceCollection>();
		Services.AddSharedImplementation<ExternalServiceScopeManager>(SharedWithin.Scope).For<IExternalServiceScopeManager>();
		Services.AddSharedImplementation<ExternalServiceRunner>(SharedWithin.Scope).For<IExternalServiceRunner>();
	}
}