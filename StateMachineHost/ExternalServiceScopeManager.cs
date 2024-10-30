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

namespace Xtate.Core;

public class ExternalServiceScopeManager : IExternalServiceScopeManager
{
	public required Func<InvokeId, InvokeData, ValueTask<ExternalIExternalServiceScopeProxy>> ServiceScopeProxyFactory { private get; [UsedImplicitly] init; }

	public required IServiceScopeFactory ServiceScopeFactory { private get; [UsedImplicitly] init; }

	public required Func<SecurityContextType, SecurityContextRegistration> SecurityContextRegistrationFactory { private get; [UsedImplicitly] init; }

#region Interface IExternalServiceScopeManager

	public async ValueTask StartService(InvokeId invokeId, InvokeData invokeData)
	{
		var serviceScopeProxy = await ServiceScopeProxyFactory(invokeId, invokeData).ConfigureAwait(false);

		await using var registration = SecurityContextRegistrationFactory(SecurityContextType.InvokedService).ConfigureAwait(false);

		var serviceScope = ServiceScopeFactory.CreateScope(
			services =>
			{
				services.AddConstant<IStateMachineInvokeId>(serviceScopeProxy);
				services.AddConstant<IExternalServiceDefinition>(serviceScopeProxy);
				services.AddConstant<IEventDispatcher>(serviceScopeProxy);
				services.AddConstant<IStateMachineSessionId>(serviceScopeProxy);
				services.AddConstant<IStateMachineLocation>(serviceScopeProxy);
				services.AddConstant<ICaseSensitivity>(serviceScopeProxy);
			});

		IExternalServiceRunner? runner = default;

		try
		{
			runner = await serviceScope.ServiceProvider.GetRequiredService<IExternalServiceRunner>().ConfigureAwait(false);
		}
		finally
		{
			DisposeScopeOnComplete(runner, serviceScope).Forget();
		}
	}

#endregion

	private static async ValueTask DisposeScopeOnComplete(IExternalServiceRunner? runner, IServiceScope scope)
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
}