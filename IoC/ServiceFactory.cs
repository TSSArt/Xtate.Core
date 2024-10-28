using Xtate.Service;

namespace Xtate.Core;

public class ServiceFactory
{
	public required IAsyncEnumerable<IServiceFactory> ServiceFactories { private get; [UsedImplicitly] init; }

	public required IServiceDefinition ServiceDefinition{ private get; [UsedImplicitly] init; }

	public async ValueTask<IService> CreateService()
	{
		var serviceActivator = await GetServiceActivator(ServiceDefinition.Type).ConfigureAwait(false);

		return await serviceActivator.StartService().ConfigureAwait(false);
	}

	private async ValueTask<IServiceActivator> GetServiceActivator(Uri type)
	{
		var serviceFactories = ServiceFactories.GetAsyncEnumerator();

		await using (serviceFactories.ConfigureAwait(false))
		{
			while (await serviceFactories.MoveNextAsync().ConfigureAwait(false))
			{
				Infra.NotNull(serviceFactories.Current);

				if (await serviceFactories.Current.TryGetActivator(type).ConfigureAwait(false) is not { } serviceActivator)
				{
					continue;
				}

				while (await serviceFactories.MoveNextAsync().ConfigureAwait(false))
				{
					if (await serviceFactories.Current.TryGetActivator(type).ConfigureAwait(false) is not null)
					{
						Infra.Fail(Res.Format(Resources.Exception_MoreThanOneServiceFactoryRegisteredForPprocessingInvokeType, type));
					}
				}

				return serviceActivator;
			}

			throw Infra.Fail<Exception>(Res.Format(Resources.Exception_ThereIsNoAnyServiceFactoryRegisteredForPprocessingInvokeType, type));
		}
	}

}