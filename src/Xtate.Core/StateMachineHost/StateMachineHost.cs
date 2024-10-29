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

namespace Xtate;

public sealed partial class StateMachineHost : IHostController
{
	public required IStateMachineHostContext StateMachineHostContext { private get; [UsedImplicitly] init; }

	public required IServiceScopeFactory ServiceScopeFactory { private get; [UsedImplicitly] init; }

	public required Func<SecurityContextType, SecurityContextRegistration> SecurityContextRegistrationFactory { private get; [UsedImplicitly] init; }

#region Interface IHostController

	ValueTask<IStateMachineController> IHostController.StartStateMachine(StateMachineClass stateMachineClass, SecurityContextType securityContextType) =>
		StartStateMachine(stateMachineClass, securityContextType);

	ValueTask<DataModelValue> IHostController.ExecuteStateMachine(StateMachineClass stateMachineClass, SecurityContextType securityContextType) =>
		ExecuteStateMachine(stateMachineClass, securityContextType);

	ValueTask IHostController.DestroyStateMachine(SessionId sessionId) => DestroyStateMachine(sessionId);

#endregion

	private async ValueTask<IStateMachineController> StartStateMachine(StateMachineClass stateMachineClass, SecurityContextType securityContextType)
	{
		await using var registration = SecurityContextRegistrationFactory(securityContextType).ConfigureAwait(false);

		var scope = ServiceScopeFactory.CreateScope(stateMachineClass.AddServices);

		IStateMachineRunner? runner = default;

		try
		{
			runner = await scope.ServiceProvider.GetRequiredService<IStateMachineRunner, IStateMachineHostContext>(StateMachineHostContext).ConfigureAwait(false);

			return await scope.ServiceProvider.GetRequiredService<IStateMachineController>().ConfigureAwait(false);
		}
		finally
		{
			DisposeScopeOnComplete(runner, scope).Forget();
		}
	}

	private static async ValueTask DisposeScopeOnComplete(IStateMachineRunner? runner, IServiceScope scope)
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
			await scope.DisposeAsync().ConfigureAwait(false);
		}
	}

	private async ValueTask<DataModelValue> ExecuteStateMachine(StateMachineClass stateMachineClass, SecurityContextType securityContextType)
	{
		await using var registration = SecurityContextRegistrationFactory(securityContextType).ConfigureAwait(false);

		var scope = ServiceScopeFactory.CreateScope(stateMachineClass.AddServices);

		await using (scope.ConfigureAwait(false))
		{
			var runner = await scope.ServiceProvider.GetRequiredService<IStateMachineRunner, IStateMachineHostContext>(StateMachineHostContext).ConfigureAwait(false);

			await runner.WaitForCompletion().ConfigureAwait(false);

			var controller = await scope.ServiceProvider.GetRequiredService<IStateMachineController>().ConfigureAwait(false);

			return await controller.GetResult().ConfigureAwait(false);
		}
	}

	private ValueTask DestroyStateMachine(SessionId sessionId) => GetCurrentContext().DestroyStateMachine(sessionId);
}