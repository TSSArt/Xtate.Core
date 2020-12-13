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

namespace Xtate
{
	public sealed partial class StateMachineHost : IHost
	{
	#region Interface IHost

		async ValueTask IHost.StartStateMachineAsync(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters, CancellationToken token) =>
				await StartStateMachine(sessionId, origin, parameters, token).ConfigureAwait(false);

		ValueTask<DataModelValue> IHost.ExecuteStateMachineAsync(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters, CancellationToken token) =>
				ExecuteStateMachine(sessionId, origin, parameters, token);

		ValueTask IHost.DestroyStateMachine(SessionId sessionId, CancellationToken token) => DestroyStateMachine(sessionId, token);

	#endregion

		private async ValueTask<StateMachineController> StartStateMachine(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters, CancellationToken token = default)
		{
			if (sessionId is null) throw new ArgumentNullException(nameof(sessionId));
			if (origin.Type == StateMachineOriginType.None) throw new ArgumentException(Resources.Exception_StateMachine_origin_missed, nameof(origin));

			var context = GetCurrentContext();
			var errorProcessor = CreateErrorProcessor(sessionId, origin);

			var controller = await context.CreateAndAddStateMachine(sessionId, origin, parameters, errorProcessor, token).ConfigureAwait(false);

			await controller.StartAsync(token).ConfigureAwait(false);

			CompleteStateMachine(context, controller).Forget();

			return controller;
		}

		private static async ValueTask CompleteStateMachine(StateMachineHostContext context, StateMachineController controller)
		{
			try
			{
				await controller.GetResult(default).ConfigureAwait(false);
			}
			finally
			{
				await context.RemoveStateMachine(controller.SessionId).ConfigureAwait(false);
			}
		}

		private async ValueTask<DataModelValue> ExecuteStateMachine(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters, CancellationToken token = default)
		{
			var context = GetCurrentContext();
			var errorProcessor = CreateErrorProcessor(sessionId, origin);

			var controller = await context.CreateAndAddStateMachine(sessionId, origin, parameters, errorProcessor, token).ConfigureAwait(false);

			try
			{
				return await controller.ExecuteAsync().ConfigureAwait(false);
			}
			finally
			{
				await context.RemoveStateMachine(sessionId).ConfigureAwait(false);
			}
		}

		private ValueTask DestroyStateMachine(SessionId sessionId, CancellationToken token = default) => GetCurrentContext().DestroyStateMachine(sessionId, token);
	}
}