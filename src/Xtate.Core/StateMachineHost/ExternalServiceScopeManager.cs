using Xtate.DataModel;
using Xtate.IoC;

namespace Xtate.Core;

public class ExternalServiceScopeManager : IExternalServiceScopeManager
{
	public required Func<InvokeId, InvokeData, ValueTask<ExternalIExternalServiceScopeProxy>>     ServiceScopeProxyFactory    { private get; [UsedImplicitly] init; }

	public required IServiceScopeFactory    ServiceScopeFactory    { private get; [UsedImplicitly] init; }


	public required Func<SecurityContextType, SecurityContextRegistration> SecurityContextRegistrationFactory { private get; [UsedImplicitly] init; }

	public async ValueTask StartService(InvokeId invokeId, InvokeData invokeData)
	{
		var serviceScopeProxy = await ServiceScopeProxyFactory(invokeId, invokeData).ConfigureAwait(false);
		
		await using var registration = SecurityContextRegistrationFactory(SecurityContextType.InvokedService).ConfigureAwait(false);

		var serviceScope = ServiceScopeFactory.CreateScope(
			services =>
			{
				services.AddConstant<IStateMachineInvokeId>(serviceScopeProxy);
				services.AddConstant<IExternalServiceDefinition>(serviceScopeProxy);
				services.AddConstant<IEventDispatcher>(serviceScopeProxy);
				services.AddConstant<IStateMachineSessionId>(serviceScopeProxy);
				services.AddConstant<IStateMachineLocation>(serviceScopeProxy);
				services.AddConstant<ICaseSensitivity>(serviceScopeProxy);
			});

		IExternalServiceRunner? runner = default;

		try
		{
			runner = await serviceScope.ServiceProvider.GetRequiredService<IExternalServiceRunner>().ConfigureAwait(false);
		}
		finally
		{
			DisposeScopeOnComplete(runner, serviceScope).Forget();	
		}
	}


	private static async ValueTask DisposeScopeOnComplete(IExternalServiceRunner? runner, IServiceScope scope)
	{
		try
		{
			if (runner is not null)
			{
				await runner.WaitForCompletion().ConfigureAwait(false);
			}
		}
		finally
		{
			await scope.DisposeAsync().ConfigureAwait(false);
		}
	}
}