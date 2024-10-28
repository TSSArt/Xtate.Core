namespace Xtate.Service;

public abstract class ServiceFactory<TService>(Uri type) : IServiceFactory, IServiceActivator where TService : IService
{
	public required Func<ValueTask<TService>> ServiceFactoryFunc { private get; [UsedImplicitly] init; }

	public ValueTask<IServiceActivator?> TryGetActivator(Uri type1) => FullUriComparer.Instance.Equals(type, type1) ? new ValueTask<IServiceActivator?>(this) : default;

	public async ValueTask<IService> StartService() => await ServiceFactoryFunc().ConfigureAwait(false);

	[Obsolete]
	public ValueTask<IService> StartService(Uri? baseUri, InvokeData invokeData, IServiceCommunication serviceCommunication) => throw new NotImplementedException();
}