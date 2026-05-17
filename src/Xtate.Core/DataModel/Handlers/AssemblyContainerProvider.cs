// Copyright © 2019-2026 Sergii Artemenko
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

namespace Xtate.DataModel;

public class AssemblyContainerProvider(Uri uri) : IAsyncInitialization, IAssemblyContainerProvider, IDisposable
{
	private readonly AsyncInit<AssemblyContainerProvider, IServiceScope> _serviceScope = new(CreateServiceScopeAsync);

	private readonly Uri _uri = uri;

	public required IServiceScopeFactory ServiceScopeFactory { private get; [SetByIoC] init; }

	public required Func<Uri, ValueTask<DynamicAssembly>> DynamicAssemblyFactory { private get; [SetByIoC] init; }

#region Interface IAssemblyContainerProvider

	public virtual IAsyncEnumerable<IDataModelHandlerProvider> GetDataModelHandlerProviders() => _serviceScope.Value.ServiceProvider.GetServices<IDataModelHandlerProvider>();

#endregion

#region Interface IAsyncInitialization

	public virtual ValueTask InitializeAsync() => AsyncInit.For(this).Run(_serviceScope);

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

	private static async ValueTask<IServiceScope> CreateServiceScopeAsync(AssemblyContainerProvider provider)
	{
		var dynamicAssembly = await provider.DynamicAssemblyFactory(provider._uri).ConfigureAwait(false);

		return provider.ServiceScopeFactory.CreateScope(dynamicAssembly.Register);
	}

	private async ValueTask<IServiceScope> CreateServiceScope()
	{
		var dynamicAssembly = await DynamicAssemblyFactory(_uri).ConfigureAwait(false);

		return ServiceScopeFactory.CreateScope(dynamicAssembly.Register);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_serviceScope.Value.Dispose();
		}
	}
}