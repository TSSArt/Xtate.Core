#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

namespace Xtate;

public sealed partial class StateMachineHost : IHost
{
	public required Func<ValueTask<IScopeManager>> _scopeManagerFactory { private get; [UsedImplicitly] init; }

#region Interface IHost

	async ValueTask<IStateMachineController> IHost.StartStateMachineAsync(SessionId sessionId,
																		  StateMachineOrigin origin,
																		  DataModelValue parameters,
																		  SecurityContextType securityContextType,
																		  //DeferredFinalizer finalizer,
																		  CancellationToken token) =>
		await StartStateMachine(sessionId, origin, parameters, securityContextType, token).ConfigureAwait(false);
	
	ValueTask IHost.DestroyStateMachine(SessionId sessionId, CancellationToken token) => DestroyStateMachine(sessionId, token);

#endregion

	private async ValueTask<IStateMachineController> StartStateMachine(SessionId sessionId,
																	   StateMachineOrigin origin,
																	   DataModelValue parameters,
																	   SecurityContextType securityContextType,
																	   //DeferredFinalizer? finalizer,
																	   CancellationToken token)
	{
		if (sessionId is null) throw new ArgumentNullException(nameof(sessionId));
		if (origin.Type == StateMachineOriginType.None) throw new ArgumentException(Resources.Exception_StateMachineOriginMissed, nameof(origin));

<<<<<<< Updated upstream
		async ValueTask<IStateMachineController> IHost.StartStateMachineAsync(SessionId sessionId,
																			  StateMachineOrigin origin,
																			  DataModelValue parameters,
																			  SecurityContextType securityContextType,
																			  DeferredFinalizer finalizer, 
																			  CancellationToken token)
		{
			return await StartStateMachine(sessionId, origin, parameters, securityContextType, finalizer, token).ConfigureAwait(false);
		}

		ValueTask IHost.DestroyStateMachine(SessionId sessionId, CancellationToken token) => DestroyStateMachine(sessionId, token);

	#endregion

		private async ValueTask<IStateMachineController> StartStateMachine(SessionId sessionId,
																		   StateMachineOrigin origin,
																		   DataModelValue parameters,
																		   SecurityContextType securityContextType,
																		   DeferredFinalizer? finalizer, 
																		   CancellationToken token)
		{
			if (sessionId is null) throw new ArgumentNullException(nameof(sessionId));
			if (origin.Type == StateMachineOriginType.None) throw new ArgumentException(Resources.Exception_StateMachineOriginMissed, nameof(origin));

			var scopeManager = _options.ServiceLocator.GetService<IScopeManager>();
			var stateMachineStartOptions = new StateMachineStartOptions()
										   {
											   Origin = origin, 
											   Parameters = parameters, 
											   SessionId = sessionId, 
											   SecurityContextType = securityContextType
										   };
			
			return await scopeManager.RunStateMachine(stateMachineStartOptions).ConfigureAwait(false);
		}

		private ValueTask DestroyStateMachine(SessionId sessionId, CancellationToken token) => GetCurrentContext().DestroyStateMachine(sessionId, token);
=======
		var stateMachineStartOptions = new StateMachineStartOptions
									   {
										   Origin = origin,
										   Parameters = parameters,
										   SessionId = sessionId,
										   SecurityContextType = securityContextType
									   };

		var scopeManager = await _scopeManagerFactory().ConfigureAwait(false);

		return await scopeManager.RunStateMachine(stateMachineStartOptions).ConfigureAwait(false);
>>>>>>> Stashed changes
	}

	private ValueTask DestroyStateMachine(SessionId sessionId, CancellationToken token) => GetCurrentContext().DestroyStateMachine(sessionId, token);
}