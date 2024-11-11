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

using System.Collections.Concurrent;
using Xtate.IoC;

namespace Xtate;

public class StateMachineScopeManager : IStateMachineScopeManager
{
	private readonly ConcurrentDictionary<SessionId, IServiceScope>? _scopes = new();

	public required IStateMachineHostContext StateMachineHostContext { private get; [UsedImplicitly] init; }

	public required IServiceScopeFactory ServiceScopeFactory { private get; [UsedImplicitly] init; }

	public required Func<SecurityContextType, SecurityContextRegistration> SecurityContextRegistrationFactory { private get; [UsedImplicitly] init; }

	public required TaskCollector TaskCollector { private get; [UsedImplicitly] init; }

#region Interface IStateMachineScopeManager

	ValueTask IStateMachineScopeManager.StartStateMachine(StateMachineClass stateMachineClass, SecurityContextType securityContextType) => StartStateMachine(stateMachineClass, securityContextType);

	ValueTask<DataModelValue> IStateMachineScopeManager.ExecuteStateMachine(StateMachineClass stateMachineClass, SecurityContextType securityContextType) =>
		ExecuteStateMachine(stateMachineClass, securityContextType);

	ValueTask IStateMachineScopeManager.DestroyStateMachine(SessionId sessionId) => DestroyStateMachine(sessionId);

#endregion

	private async ValueTask StartStateMachine(StateMachineClass stateMachineClass, SecurityContextType securityContextType)
	{
		var scopes = _scopes;
		Infra.EnsureNotDisposed(scopes is not null, this);

		await using var registration = SecurityContextRegistrationFactory(securityContextType).ConfigureAwait(false);

		var serviceScope = ServiceScopeFactory.CreateScope(stateMachineClass.AddServices);

		if (!scopes.TryAdd(stateMachineClass.SessionId, serviceScope))
		{
			await serviceScope.DisposeAsync().ConfigureAwait(false);
			Infra.Fail(Resources.Exception_MoreThanOneStateMachineWithSameSessionId);
		}

		IStateMachineRunner? runner = default;

		try
		{
			runner = await serviceScope.ServiceProvider.GetRequiredService<IStateMachineRunner, IStateMachineHostContext>(StateMachineHostContext).ConfigureAwait(false);
		}
		finally
		{
			if (runner is null)
			{
				await Cleanup(stateMachineClass.SessionId).ConfigureAwait(false);
			}
			else
			{
				TaskCollector.Collect(WaitAndCleanup(stateMachineClass.SessionId, runner));
			}
		}
	}

	private async ValueTask WaitAndCleanup(SessionId sessionId, IStateMachineRunner runner)
	{
		try
		{
			if (runner is not null)
			{
				await runner.WaitForCompletion().ConfigureAwait(false);
			}
		}
		finally
		{
			await Cleanup(sessionId).ConfigureAwait(false);
		}
	}

	private async ValueTask Cleanup(SessionId sessionId)
	{
		if (_scopes?.TryRemove(sessionId, out var serviceScope) == true)
		{
			await serviceScope.DisposeAsync().ConfigureAwait(false);
		}
	}

	private async ValueTask<DataModelValue> ExecuteStateMachine(StateMachineClass stateMachineClass, SecurityContextType securityContextType)
	{
		var scopes = _scopes;
		Infra.EnsureNotDisposed(scopes is not null, this);

		await using var registration = SecurityContextRegistrationFactory(securityContextType).ConfigureAwait(false);

		var serviceScope = ServiceScopeFactory.CreateScope(stateMachineClass.AddServices);

		await using (serviceScope.ConfigureAwait(false))
		{
			var added = scopes.TryAdd(stateMachineClass.SessionId, serviceScope);
			Infra.Assert(added, Resources.Exception_MoreThanOneStateMachineWithSameSessionId);

			try
			{
				var runner = await serviceScope.ServiceProvider.GetRequiredService<IStateMachineRunner, IStateMachineHostContext>(StateMachineHostContext).ConfigureAwait(false);

				await runner.WaitForCompletion().ConfigureAwait(false);

				var controller = await serviceScope.ServiceProvider.GetRequiredService<IStateMachineController>().ConfigureAwait(false);

				return await controller.GetResult().ConfigureAwait(false);
			}
			finally
			{
				await Cleanup(stateMachineClass.SessionId).ConfigureAwait(false);
			}
		}
	}

	private async ValueTask DestroyStateMachine(SessionId sessionId)
	{
		if (_scopes?.TryGetValue(sessionId, out var serviceScope) == true)
		{
			var controller = await serviceScope.ServiceProvider.GetRequiredService<IStateMachineController>().ConfigureAwait(false);

			await controller.Destroy().ConfigureAwait(false);
		}
	}
}