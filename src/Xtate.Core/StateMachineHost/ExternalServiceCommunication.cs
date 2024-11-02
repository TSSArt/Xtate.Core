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

using Xtate.ExternalService;

namespace Xtate.Core;

public class ExternalServiceCommunication : IExternalServiceCommunication
{
	public required IStateMachineSessionId StateMachineSessionId { private get; [UsedImplicitly] init; }

	public required IStateMachineHost StateMachineHost { private get; [UsedImplicitly] init; }

	public required IExternalServiceScopeManager ExternalServiceScopeManager { private get; [UsedImplicitly] init; }

	private SessionId SessionId => StateMachineSessionId.SessionId;

#region Interface IExternalServiceCommunication

	public ValueTask Forward(InvokeId invokeId, IIncomingEvent incomingEvent) => StateMachineHost.ForwardEvent(SessionId, invokeId, incomingEvent, token: default);

	public ValueTask Start(InvokeId invokeId, InvokeData invokeData) => ExternalServiceScopeManager.StartService(invokeId, invokeData);

	public ValueTask Cancel(InvokeId invokeId) => StateMachineHost.CancelInvoke(SessionId, invokeId, token: default);

#endregion

	/*public ValueTask CancelInvoke(InvokeId invokeId)
	{
		var context = GetCurrentContext();

		context.ValidateSessionId(sessionId, out _);

		if (await context.TryRemoveService(sessionId, invokeId).ConfigureAwait(false) is { } service)
		{
			await service.Destroy().ConfigureAwait(false);

			await DisposeInvokedService(service).ConfigureAwait(false);
		}
	}*/
}