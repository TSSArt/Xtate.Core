using Xtate.IoC;

namespace Xtate.DataModel;

public class AssemblyContainerProvider : IAsyncInitialization, IAssemblyContainerProvider, IDisposable
{
	private readonly AsyncInit<IServiceScope> _asyncInitServiceScope;
	private readonly Uri                      _uri;

	public AssemblyContainerProvider(Uri uri)
	{
		_uri = uri;
		_asyncInitServiceScope = AsyncInit.Run(this, acp => acp.CreateServiceScope());
	}

	public required IServiceScopeFactory                  ServiceScopeFactory    { private get; [UsedImplicitly] init; }
	public required Func<Uri, ValueTask<DynamicAssembly>> DynamicAssemblyFactory { private get; [UsedImplicitly] init; }

#region Interface IAssemblyContainerProvider

	public virtual IAsyncEnumerable<IDataModelHandlerProvider> GetDataModelHandlerProviders() => _asyncInitServiceScope.Value.ServiceProvider.GetServices<IDataModelHandlerProvider>();

#endregion

#region Interface IAsyncInitialization

	public Task Initialization => _asyncInitServiceScope.Task;

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

	private async ValueTask<IServiceScope> CreateServiceScope()
	{
		var dynamicAssembly = await DynamicAssemblyFactory(_uri).ConfigureAwait(false);

		return ServiceScopeFactory.CreateScope(dynamicAssembly.Register);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_asyncInitServiceScope.Value.Dispose();
		}
	}
}