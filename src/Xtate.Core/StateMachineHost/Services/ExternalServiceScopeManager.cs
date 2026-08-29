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
using Xtate.StateMachine;
using Xtate.TaskMonitor;

namespace Xtate.StateMachineHost.Services;

[InstantiatedByIoC]
public class ExternalServiceScopeManager : IExternalServiceScopeManager, IDisposable, IAsyncDisposable
{
	private ExtDictionary<InvokeId, IServiceScope>? _scopes = [];

	public required IServiceScopeFactory ServiceScopeFactory { private get; [SetByIoC] init; }

	public required Func<SecurityContextType, SecurityContextRegistration> SecurityContextRegistrationFactory { private get; [SetByIoC] init; }

	public required IExternalServiceCollection ExternalServiceCollection { private get; [SetByIoC] init; }

	public required ITaskMonitor TaskMonitor { private get; [SetByIoC] init; }

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

	public virtual async ValueTask<ExternalServiceResult> Start(ExternalServiceClass externalServiceClass, CancellationToken token)
	{
		await using var registration = SecurityContextRegistrationFactory(SecurityContextType.InvokedService).ConfigureAwait(false);

		var serviceProvider = CreateServiceScope(externalServiceClass).ServiceProvider;

		IExternalServiceController? controller = null;
		var resultTcs = new TaskCompletionSource();

		try
		{
			controller = await Start(serviceProvider, externalServiceClass.InvokeId).WaitAsync(TaskMonitor, token).ConfigureAwait(false);

			return new ExternalServiceResult(resultTcs.Task);
		}
		finally
		{
			if (controller is not null)
			{
				WaitAndCleanup(externalServiceClass.InvokeId, controller, resultTcs).Forget(TaskMonitor);
			}
			else
			{
				await Cleanup(externalServiceClass.InvokeId).ConfigureAwait(false);
			}
		}
	}

	public virtual ValueTask Cancel(InvokeId invokeId, CancellationToken token) => _scopes?.TryRemove(invokeId, out var serviceScope) == true ? serviceScope.DisposeAsync() : default;

#endregion

	private async ValueTask<IExternalServiceController> Start(IServiceProvider serviceProvider, InvokeId invokeId)
	{
		ExternalServiceCollection.Register(invokeId);

		var externalServiceController = await serviceProvider.GetRequiredService<IExternalServiceController>().ConfigureAwait(false);

		ExternalServiceCollection.SetController(invokeId, externalServiceController);

		return externalServiceController;
	}

	private IServiceScope CreateServiceScope(ExternalServiceClass externalServiceClass)
	{
		var scopes = _scopes;
		Infra.EnsureNotDisposed(scopes is not null, this);

		var serviceScope = ServiceScopeFactory.CreateScope(externalServiceClass.AddServices);

		if (scopes.TryAdd(externalServiceClass.InvokeId, serviceScope))
		{
			return serviceScope;
		}

		serviceScope.Dispose();

		throw Infra.Fail<Exception>(Resources.Exception_MoreThanOneExternalServicesExecutingWithSameInvokeId);
	}

	private async ValueTask Cleanup(InvokeId invokeId)
	{
		ExternalServiceCollection.Unregister(invokeId);

		if (_scopes?.TryRemove(invokeId, out var serviceScope) == true)
		{
			await serviceScope.DisposeAsync().ConfigureAwait(false);
		}
	}

	private async ValueTask WaitAndCleanup(InvokeId invokeId, IExternalServiceController externalServiceController, TaskCompletionSource resultTcs)
	{
		Exception? exception = null;

		try
		{
			await externalServiceController.WaitForCompletion().ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		await Cleanup(invokeId).ConfigureAwait(false);

		switch (exception)
		{
			case null:
				resultTcs.SetResult();

				break;

			case OperationCanceledException ocx when resultTcs.TrySetCanceled(ocx.CancellationToken):
				break;

			default:
				resultTcs.SetException(exception);

				break;
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && _scopes is { } scopes)
		{
			_scopes = null;

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
			_scopes = null;

			while (scopes.TryTake(out _, out var serviceScope))
			{
				await serviceScope.DisposeAsync().ConfigureAwait(false);
			}
		}
	}
}
