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

namespace Xtate;

public sealed partial class StateMachineHost : IHostController
{
	public required Func<ValueTask<IScopeManager>> ScopeManagerFactory { private get; [UsedImplicitly] init; }

#region Interface IHost

	async ValueTask<IStateMachineController> IHostController.StartStateMachine(SessionId sessionId,
																		  StateMachineOrigin origin,
																		  DataModelValue parameters,
																		  SecurityContextType securityContextType,
																		  CancellationToken token) =>
		await StartStateMachine(sessionId, origin, parameters, securityContextType, token).ConfigureAwait(false);

	ValueTask IHostController.DestroyStateMachine(SessionId sessionId, CancellationToken token) => DestroyStateMachine(sessionId, token);

#endregion

	private async ValueTask<IStateMachineController> StartStateMachine(SessionId sessionId,
																	   StateMachineOrigin origin,
																	   DataModelValue parameters,
																	   SecurityContextType securityContextType,
																	   CancellationToken token)
	{
		var stateMachineStartOptions = new StateMachineStartOptions
									   {
										   Origin = origin,
										   Parameters = parameters,
										   SessionId = sessionId,
										   SecurityContextType = securityContextType
									   };

		var scopeManager = await ScopeManagerFactory().ConfigureAwait(false);

		return await scopeManager.RunStateMachine(stateMachineStartOptions).ConfigureAwait(false);
	}

	private ValueTask DestroyStateMachine(SessionId sessionId, CancellationToken token) => GetCurrentContext().DestroyStateMachine(sessionId, token);
}