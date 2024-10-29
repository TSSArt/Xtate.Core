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

using Xtate.IoC;

namespace Xtate.Core;

public class ScopeManager : IScopeManager1, IDisposable, IAsyncDisposable
{
	private readonly IServiceScope _scope;

	public ScopeManager(Action<IServiceCollection> configureServices, IServiceScopeFactory serviceScopeFactory)
	{
		_scope = serviceScopeFactory.CreateScope(configureServices);

		DisposeScopeOnComplete().Forget();
	}

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IScopeManager1

	public ValueTask<T> GetService<T>() where T : notnull => _scope.ServiceProvider.GetRequiredService<T>();

#endregion

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

	protected virtual async ValueTask DisposeAsyncCore()
	{
		await _scope.DisposeAsync().ConfigureAwait(false);
	}
}