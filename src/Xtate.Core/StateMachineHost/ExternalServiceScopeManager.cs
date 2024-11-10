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

using Xtate.DataModel;
using Xtate.IoC;

namespace Xtate.ExternalService;

public class ExternalServiceScopeManager : IExternalServiceScopeManager, IDisposable, IAsyncDisposable
{
	private MiniDictionary<InvokeId, IServiceScope> _scopes = new(InvokeId.InvokeUniqueIdComparer);

	public required Func<InvokeId, InvokeData, ValueTask<ExternalServiceBridge>> ExternalServiceBridgeFactory { private get; [UsedImplicitly] init; }

	public required IServiceScopeFactory ServiceScopeFactory { private get; [UsedImplicitly] init; }

	public required Func<SecurityContextType, SecurityContextRegistration> SecurityContextRegistrationFactory { private get; [UsedImplicitly] init; }

	public required ExternalServiceEventRouter ExternalServiceEventRouter { private get; [UsedImplicitly] init; }

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

	public virtual async ValueTask Start(InvokeId invokeId, InvokeData invokeData)
	{
		var scopes = _scopes;
		Infra.EnsureNotDisposed(scopes is not null, this);

		var externalServiceBridge = await ExternalServiceBridgeFactory(invokeId, invokeData).ConfigureAwait(false);

		await using var registration = SecurityContextRegistrationFactory(SecurityContextType.InvokedService).ConfigureAwait(false);

		var serviceScope = ServiceScopeFactory.CreateScope(
			services =>
			{
				services.AddConstant<IStateMachineSessionId>(externalServiceBridge);
				services.AddConstant<IStateMachineLocation>(externalServiceBridge);
				services.AddConstant<ICaseSensitivity>(externalServiceBridge);
				services.AddConstant<IExternalServiceInvokeId>(externalServiceBridge);
				services.AddConstant<IExternalServiceType>(externalServiceBridge);
				services.AddConstant<IExternalServiceSource>(externalServiceBridge);
				services.AddConstant<IExternalServiceParameters>(externalServiceBridge);
				services.AddConstant<IParentStateMachineSessionId>(externalServiceBridge);
				services.AddConstant<IParentEventDispatcher>(externalServiceBridge);
			});

		var adding = scopes.TryAdd(invokeId, serviceScope);
		Infra.Assert(adding, Resources.Exception_MoreThanOneExternalServicesExecutingWithSameInvokeId);

		IExternalServiceRunner? runner = default;

		try
		{
			runner = await serviceScope.ServiceProvider.GetRequiredService<IExternalServiceRunner>().ConfigureAwait(false);

			if (await serviceScope.ServiceProvider.GetService<IEventDispatcher>().ConfigureAwait(false) is { } eventDispatcher)
			{
				externalServiceBridge.EventDispatcher = eventDispatcher;
				ExternalServiceEventRouter.Subscribe(invokeId, externalServiceBridge.IncomingEventHandler);
			}
		}
		finally
		{
			if (runner is null)
			{
				await Cleanup(invokeId, externalServiceBridge).ConfigureAwait(false);
			}
			else
			{
				WaitAndCleanup(invokeId, runner, externalServiceBridge).Forget();
			}
		}
	}

	public virtual ValueTask Cancel(InvokeId invokeId) => _scopes?.TryRemove(invokeId, out var serviceScope) == true ? serviceScope.DisposeAsync() : default;

#endregion

	private async ValueTask WaitAndCleanup(InvokeId invokeId, IExternalServiceRunner externalServiceRunner, ExternalServiceBridge externalServiceBridge)
	{
		try
		{
			await externalServiceRunner.WaitForCompletion().ConfigureAwait(false);
		}
		finally
		{
			await Cleanup(invokeId, externalServiceBridge).ConfigureAwait(false);
		}
	}

	private async ValueTask Cleanup(InvokeId invokeId, ExternalServiceBridge externalServiceBridge)
	{
		if (externalServiceBridge.EventDispatcher is not null)
		{
			ExternalServiceEventRouter.Unsubscribe(invokeId, externalServiceBridge.IncomingEventHandler);
			externalServiceBridge.EventDispatcher = default;
		}

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

			foreach (var pair in scopes)
			{
				pair.Value.Dispose();
			}
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (_scopes is { } scopes)
		{
			_scopes = default;

			foreach (var pair in scopes)
			{
				await pair.Value.DisposeAsync().ConfigureAwait(false);
			}
		}
	}
}