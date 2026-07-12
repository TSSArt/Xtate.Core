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

using Xtate.Class;
using Xtate.DataTypes;
using Xtate.IoC;
using Xtate.StateMachine;
using Xtate.TaskMonitor;

namespace Xtate.StateMachineHost.Services;

[InstantiatedByIoC]
public class StateMachineScopeManager : IStateMachineScopeManager, IDisposable, IAsyncDisposable
{
	private ConcurrentDictionary<SessionId, IServiceScope>? _scopes = new();

	public required IServiceScopeFactory ServiceScopeFactory { private get; [SetByIoC] init; }

	public required IStateMachineCollection StateMachineCollection { private get; [SetByIoC] init; }

	public required Func<SecurityContextType, SecurityContextRegistration> SecurityContextRegistrationFactory { private get; [SetByIoC] init; }

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

#region Interface IStateMachineScopeManager

	public virtual async ValueTask<StateMachineResult> Start(StateMachineClass stateMachineClass, SecurityContextType securityContextType)
	{
		await using var registration = SecurityContextRegistrationFactory(securityContextType).ConfigureAwait(false);

		var serviceProvider = CreateServiceScope(stateMachineClass).ServiceProvider;

		IStateMachineController? controller = null;
		var resultTcs = new TaskCompletionSource<DataModelValue>();

		try
		{
			controller = await Start(serviceProvider, stateMachineClass.SessionId).ConfigureAwait(false);

			return new StateMachineResult(resultTcs.Task);
		}
		finally
		{
			if (controller is not null)
			{
				GetResultAndCleanup(stateMachineClass.SessionId, controller, resultTcs).Forget(TaskMonitor);
			}
			else
			{
				await Cleanup(stateMachineClass.SessionId).ConfigureAwait(false);
			}
		}
	}

	public virtual async ValueTask<DataModelValue> Execute(StateMachineClass stateMachineClass, SecurityContextType securityContextType)
	{
		await using var registration = SecurityContextRegistrationFactory(securityContextType).ConfigureAwait(false);

		var serviceProvider = CreateServiceScope(stateMachineClass).ServiceProvider;

		IStateMachineController? controller = null;

		try
		{
			controller = await Start(serviceProvider, stateMachineClass.SessionId).ConfigureAwait(false);

			return await controller.GetResult().ConfigureAwait(false);
		}
		finally
		{
			if (controller is not null)
			{
				await WaitAndCleanup(stateMachineClass.SessionId, controller).ConfigureAwait(false);
			}
			else
			{
				await Cleanup(stateMachineClass.SessionId).ConfigureAwait(false);
			}
		}
	}

	public virtual ValueTask Destroy(SessionId sessionId) => _scopes?.TryGetValue(sessionId, out var serviceScope) == true ? Destroy(serviceScope) : ValueTask.CompletedTask;

	public virtual ValueTask DestroyAll() => new(Task.WhenAll(DestroyTasks()));

	public virtual ValueTask Terminate(SessionId sessionId) => _scopes?.TryRemove(sessionId, out var serviceScope) == true ? serviceScope.DisposeAsync() : ValueTask.CompletedTask;

#endregion

	private async ValueTask GetResultAndCleanup(SessionId sessionId, IStateMachineController controller, TaskCompletionSource<DataModelValue> resultTcs)
	{
		try
		{
			var result = await controller.GetResult().ConfigureAwait(false);

			await Cleanup(sessionId).ConfigureAwait(false);

			resultTcs.SetResult(result);
		}
		catch (Exception ex)
		{
			await Cleanup(sessionId).ConfigureAwait(false);

			resultTcs.SetException(ex);
		}
	}

	private IEnumerable<Task> DestroyTasks()
	{
		if (_scopes is not { } scopes)
		{
			yield break;
		}

		foreach (var scope in scopes)
		{
			if (Destroy(scope.Value) is { IsCompletedSuccessfully: false } valueTask)
			{
				yield return valueTask.AsTask();
			}
		}
	}

	private static async ValueTask Destroy(IServiceScope serviceScope)
	{
		var stateMachineController = await serviceScope.ServiceProvider.GetRequiredService<IStateMachineController>().ConfigureAwait(false);

		await stateMachineController.Destroy().ConfigureAwait(false);
	}

	private async ValueTask<IStateMachineController> Start(IServiceProvider serviceProvider, SessionId sessionId)
	{
		StateMachineCollection.Register(sessionId);

		var stateMachineController = await serviceProvider.GetRequiredService<IStateMachineController>().ConfigureAwait(false);

		StateMachineCollection.SetController(sessionId, stateMachineController);

		return stateMachineController;
	}

	private IServiceScope CreateServiceScope(StateMachineClass stateMachineClass)
	{
		var scopes = _scopes;
		Infra.EnsureNotDisposed(scopes is not null, this);

		var serviceScope = ServiceScopeFactory.CreateScope(stateMachineClass.AddServices);

		if (scopes.TryAdd(stateMachineClass.SessionId, serviceScope))
		{
			return serviceScope;
		}

		serviceScope.Dispose();

		throw Infra.Fail<Exception>(Resources.Exception_MoreThanOneStateMachineWithSameSessionId);
	}

	private async ValueTask WaitAndCleanup(SessionId sessionId, IStateMachineController controller)
	{
		try
		{
			await controller.GetResult().ConfigureAwait(false);
		}
		finally
		{
			await Cleanup(sessionId).ConfigureAwait(false);
		}
	}

	private async ValueTask Cleanup(SessionId sessionId)
	{
		StateMachineCollection.Unregister(sessionId);

		if (_scopes?.TryRemove(sessionId, out var serviceScope) == true)
		{
			await serviceScope.DisposeAsync().ConfigureAwait(false);
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