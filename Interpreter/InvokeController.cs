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

namespace Xtate.Core;

public class InvokeController : IInvokeController
{
	private const int StartInvokeEventId = 1;

	private const int CancelInvokeEventId = 2;

	private const int EventForwardEventId = 3;

	public required IExternalServiceManager ExternalServiceManager { private get; [UsedImplicitly] init; }

	public required ILogger<IInvokeController> Logger { private get; [UsedImplicitly] init; }

	public required StateMachineRuntimeError StateMachineRuntimeError { private get; [UsedImplicitly] init; }

#region Interface IInvokeController

	public async ValueTask Start(InvokeId invokeId, InvokeData invokeData)
	{
		await Logger.Write(Level.Trace, StartInvokeEventId, $@"Start invoke. InvokeId: [{invokeId}]", invokeData).ConfigureAwait(false);

		try
		{
			await ExternalServiceManager.Start(invokeId, invokeData).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw StateMachineRuntimeError.CommunicationError(ex);
		}
	}

	public async ValueTask Cancel(InvokeId invokeId)
	{
		await Logger.Write(Level.Trace, CancelInvokeEventId, $@"Cancel invoke. InvokeId: [{invokeId}]", invokeId).ConfigureAwait(false);

		try
		{
			await ExternalServiceManager.Cancel(invokeId).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw StateMachineRuntimeError.CommunicationError(ex);
		}
	}

	public async ValueTask Forward(InvokeId invokeId, IIncomingEvent incomingEvent)
	{
		var sendId = incomingEvent.SendId;
		var eventName = incomingEvent.Name;
		await Logger.Write(Level.Trace, EventForwardEventId, $@"Forward event. SendId: [{sendId}] Name:'{eventName}'", incomingEvent).ConfigureAwait(false);

		try
		{
			await ExternalServiceManager.Forward(invokeId, incomingEvent).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw StateMachineRuntimeError.CommunicationError(ex);
		}
	}

#endregion
}