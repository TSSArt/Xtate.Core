#region Copyright © 2019-2020 Sergii Artemenko

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

using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate
{
	public sealed partial class StateMachineHost : IHost
	{
	#region Interface IHost

		async ValueTask<IStateMachineController> IHost.StartStateMachineAsync(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters,
																			  ISecurityContext securityContext, DeferredFinalizer finalizer, CancellationToken token)
		{
			if (securityContext is SecurityContext { Type: SecurityContextType.NewStateMachine or SecurityContextType.NewTrustedStateMachine } ctx)
			{
				return await StartStateMachine(sessionId, origin, parameters, ctx, finalizer, token).ConfigureAwait(false);
			}

			throw new StateMachineSecurityException(Resources.Exception_Starting_State_Machine_denied);
		}

		ValueTask IHost.DestroyStateMachine(SessionId sessionId, CancellationToken token) => DestroyStateMachine(sessionId, token);

	#endregion

		private async ValueTask<StateMachineController> StartStateMachine(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters, SecurityContext securityContext,
																		  DeferredFinalizer finalizer, CancellationToken token = default)
		{
			if (sessionId is null) throw new ArgumentNullException(nameof(sessionId));
			if (origin.Type == StateMachineOriginType.None) throw new ArgumentException(Resources.Exception_StateMachine_origin_missed, nameof(origin));

			var context = GetCurrentContext();
			var errorProcessor = CreateErrorProcessor(sessionId, origin);
			finalizer = new DeferredFinalizer(finalizer);
			var controller = await context.CreateAndAddStateMachine(sessionId, origin, parameters, securityContext, finalizer, errorProcessor, token).ConfigureAwait(false);
			context.AddStateMachineController(controller);

			finalizer.Add(static(ctx, ctrl) => ((StateMachineHostContext) ctx).RemoveStateMachineController((StateMachineController) ctrl), context, controller);
			finalizer.Add(controller);

			await using (finalizer.ConfigureAwait(false))
			{
				await controller.StartAsync(token).ConfigureAwait(false);
			}

			return controller;
		}

		private ValueTask DestroyStateMachine(SessionId sessionId, CancellationToken token = default) => GetCurrentContext().DestroyStateMachine(sessionId, token);
	}
}