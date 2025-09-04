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

using Xtate.IoC;

namespace Xtate.DataModel;

public class AssemblyContainerProvider : IAsyncInitialization, IAssemblyContainerProvider, IDisposable
{
    private readonly AsyncInit<IServiceScope> _asyncInitServiceScope;

    private readonly Uri _uri;

    public AssemblyContainerProvider(Uri uri)
    {
        _uri = uri;
        _asyncInitServiceScope = AsyncInit.Run(this, acp => acp.CreateServiceScope());
    }

    public required IServiceScopeFactory ServiceScopeFactory { private get; [UsedImplicitly] init; }

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