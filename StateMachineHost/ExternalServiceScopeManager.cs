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

namespace Xtate.ExternalService;

public class ExternalServiceScopeManager : IExternalServiceScopeManager, IDisposable, IAsyncDisposable
{
    private ExtDictionary<InvokeId, IServiceScope>? _scopes = [];

    public required Func<InvokeData, ValueTask<ExternalServiceClass>> ExternalServiceClassFactory { private get; [UsedImplicitly] init; }

    public required IServiceScopeFactory ServiceScopeFactory { private get; [UsedImplicitly] init; }

    public required Func<SecurityContextType, SecurityContextRegistration> SecurityContextRegistrationFactory { private get; [UsedImplicitly] init; }

    public required IExternalServiceCollection ExternalServiceCollection { private get; [UsedImplicitly] init; }

    public required TaskMonitor TaskMonitor { private get; [UsedImplicitly] init; }

#region Interface IAsyncDisposable

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(false);

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

#region Interface IExternalServiceScopeManager

    public virtual async ValueTask Start(InvokeData invokeData, CancellationToken token)
    {
        await using var registration = SecurityContextRegistrationFactory(SecurityContextType.InvokedService).ConfigureAwait(false);

        IExternalServiceRunner? runner = default;

        try
        {
            runner = await Start(invokeData).WaitAsync(TaskMonitor, token).ConfigureAwait(false);
        }
        finally
        {
            if (runner is not null)
            {
                WaitAndCleanup(invokeData.InvokeId, runner).Forget(TaskMonitor);
            }
            else
            {
                await Cleanup(invokeData.InvokeId).ConfigureAwait(false);
            }
        }
    }

    public virtual ValueTask Cancel(InvokeId invokeId, CancellationToken token) => _scopes?.TryRemove(invokeId, out var serviceScope) == true ? serviceScope.DisposeAsync() : default;

#endregion

    private async ValueTask<IExternalServiceRunner> Start(InvokeData invokeData)
    {
        var externalServiceClass = await ExternalServiceClassFactory(invokeData).ConfigureAwait(false);

        var serviceScope = CreateServiceScope(invokeData.InvokeId, externalServiceClass);

        ExternalServiceCollection.Register(invokeData.InvokeId);

        var runner = await serviceScope.ServiceProvider.GetRequiredService<IExternalServiceRunner>().ConfigureAwait(false);
        var externalService = await serviceScope.ServiceProvider.GetRequiredService<IExternalService>().ConfigureAwait(false);

        ExternalServiceCollection.SetExternalService(invokeData.InvokeId, externalService);

        return runner;
    }

    private IServiceScope CreateServiceScope(InvokeId invokeId, ExternalServiceClass externalServiceClass)
    {
        var scopes = _scopes;
        Infra.EnsureNotDisposed(scopes is not null, this);

        var serviceScope = ServiceScopeFactory.CreateScope(externalServiceClass.AddServices);

        if (scopes.TryAdd(invokeId, serviceScope))
        {
            return serviceScope;
        }

        serviceScope.Dispose();

        throw Infra.Fail<Exception>(Resources.Exception_MoreThanOneExternalServicesExecutingWithSameInvokeId);
    }

    private async ValueTask WaitAndCleanup(InvokeId invokeId, IExternalServiceRunner externalServiceRunner)
    {
        try
        {
            await externalServiceRunner.WaitForCompletion().ConfigureAwait(false);
        }
        finally
        {
            await Cleanup(invokeId).ConfigureAwait(false);
        }
    }

    private async ValueTask Cleanup(InvokeId invokeId)
    {
        ExternalServiceCollection.Unregister(invokeId);

        if (_scopes?.TryRemove(invokeId, out var serviceScope) == true)
        {
            await serviceScope.DisposeAsync().ConfigureAwait(false);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && _scopes is { } scopes)
        {
            _scopes = default;

            while (scopes.TryTake(out _, out var serviceScope))
            {
                serviceScope.Dispose();
            }
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_scopes is { } scopes)
        {
            _scopes = default;

            while (scopes.TryTake(out _, out var serviceScope))
            {
                await serviceScope.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}