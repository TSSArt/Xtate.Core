using Xtate.IoC;

namespace Xtate.Core;

public class ScopeManager : IScopeManager1, IDisposable, IAsyncDisposable
{
	private readonly IServiceScope  _scope;
	
	
	public ScopeManager(Action<IServiceCollection> configureServices, IServiceScopeFactory  serviceScopeFactory)
	{
		_scope = serviceScopeFactory.CreateScope(configureServices);

		DisposeScopeOnComplete().Forget();
	}

	private async ValueTask DisposeScopeOnComplete()
	{
		try
		{
			var keepAliveServices = _scope.ServiceProvider.GetServices<IKeepAlive>().ConfigureAwait(false);

			await foreach (var keepAliveService in keepAliveServices.ConfigureAwait(false))
			{
				await keepAliveService.Wait().ConfigureAwait(false);
			}
		}
		finally
		{
			await DisposeAsync().ConfigureAwait(false);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_scope.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		await _scope.DisposeAsync().ConfigureAwait(false);
	}

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);
		GC.SuppressFinalize(this);
	}

	public ValueTask<T> GetService<T>() where T : notnull => _scope.ServiceProvider.GetRequiredService<T>();
}