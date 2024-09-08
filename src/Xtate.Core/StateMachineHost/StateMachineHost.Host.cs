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
using Xtate.Service;

namespace Xtate;

public sealed partial class StateMachineHost : IHostController
{
	//public required Func<ValueTask<IScopeManagerOld>> ScopeManagerFactoryOld { private get; [UsedImplicitly] init; }

	//public required Func<Action<IServiceCollection>, ValueTask<IScopeManager>> ScopeManagerFactory { private get; [UsedImplicitly] init; }

	public required IServiceScopeFactory ServiceScopeFactory { private get; [UsedImplicitly] init; }

#region Interface IHostController

	ValueTask IHostController.StartStateMachine(StateMachineClass stateMachineClass, SecurityContextType securityContextType) => 
		StartStateMachine(stateMachineClass, securityContextType);

	ValueTask<DataModelValue> IHostController.ExecuteStateMachine(StateMachineClass stateMachineClass, SecurityContextType securityContextType) =>
		ExecuteStateMachine(stateMachineClass, securityContextType);

	ValueTask IHostController.DestroyStateMachine(SessionId sessionId, CancellationToken token) => DestroyStateMachine(sessionId, token);

#endregion

	private async ValueTask StartStateMachine(StateMachineClass stateMachineClass, SecurityContextType securityContextType)
	{
		var serviceScope = ServiceScopeFactory.CreateScope(stateMachineClass.AddServices);

		var stateMachineRunner = await serviceScope.ServiceProvider.GetRequiredService<IStateMachineRunner>().ConfigureAwait(false);

		DisposeScopeOnComplete(stateMachineRunner, serviceScope).Forget();
	}

	private async ValueTask<IService> StartStateMachineAsService(StateMachineClass stateMachineClass, SecurityContextType securityContextType)
	{
		var serviceScope = ServiceScopeFactory.CreateScope(stateMachineClass.AddServices);

		try 
		{ 		
			return await serviceScope.ServiceProvider.GetRequiredService<IService>().ConfigureAwait(false);
		}
		finally
		{
			var stateMachineRunner = await serviceScope.ServiceProvider.GetRequiredService<IStateMachineRunner>().ConfigureAwait(false);

			DisposeScopeOnComplete(stateMachineRunner, serviceScope).Forget();
		}
	}

	private static async ValueTask DisposeScopeOnComplete(IStateMachineRunner stateMachineRunner, IServiceScope scope)
	{
		try
		{
			await stateMachineRunner.GetResult().ConfigureAwait(false);
		}
		finally
		{
			await scope.DisposeAsync().ConfigureAwait(false);
		}
	}

	private async ValueTask<DataModelValue> ExecuteStateMachine(StateMachineClass stateMachineClass, SecurityContextType securityContextType)
	{
		var serviceScope = ServiceScopeFactory.CreateScope(stateMachineClass.AddServices);

		await using (serviceScope.ConfigureAwait(false))
		{
			var stateMachineRunner = await serviceScope.ServiceProvider.GetRequiredService<IStateMachineRunner>().ConfigureAwait(false);

			return await stateMachineRunner.GetResult().ConfigureAwait(false);
		}
	}

	private ValueTask DestroyStateMachine(SessionId sessionId, CancellationToken token) => GetCurrentContext().DestroyStateMachine(sessionId, token);
}