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
using Xtate.Service;

namespace Xtate.Core;

public class ExternalServiceRunner : IExternalServiceRunner, IAsyncDisposable
{
	private readonly AsyncInit _execute;

	private readonly InvokeId _invokeId;

	public ExternalServiceRunner(IStateMachineInvokeId stateMachineInvokeId)
	{
		_invokeId = stateMachineInvokeId.InvokeId;
		_execute = AsyncInit.Run(Execute);
	}

	public required IStateMachineSessionId StateMachineSessionId { private get; [UsedImplicitly] init; }

	public required IExternalService ExternalService { private get; [UsedImplicitly] init; }

	public required IStateMachineHostContext StateMachineHostContext { private get; [UsedImplicitly] init; }

	public required DataConverter DataConverter { private get; [UsedImplicitly] init; }

	public required IEventDispatcher CreatorEventDispatcher { private get; [UsedImplicitly] init; }

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IExternalServiceRunner

	public ValueTask WaitForCompletion() => new(_execute.Task);

#endregion

	protected virtual ValueTask DisposeAsyncCore() => Complete();

	private async ValueTask Execute()
	{
		await StateMachineHostContext.AddService(StateMachineSessionId.SessionId, _invokeId, ExternalService, token: default).ConfigureAwait(false);

		try
		{
			var evt = CreateEventFromResult(await ExternalService.GetResult().ConfigureAwait(false));
			await CreatorEventDispatcher.Send(evt).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var evt = CreateEventFromException(ex);
			await CreatorEventDispatcher.Send(evt).ConfigureAwait(false);
		}
		finally
		{
			await Complete().ConfigureAwait(false);
		}
	}

	private EventObject CreateEventFromResult(DataModelValue result) =>
		new() { Type = EventType.External, Name = EventName.GetDoneInvokeName(_invokeId), Data = result, InvokeId = _invokeId };

	private EventObject CreateEventFromException(Exception ex) =>
		new() { Type = EventType.External, Name = EventName.ErrorExecution, Data = DataConverter.FromException(ex), InvokeId = _invokeId };

	private async ValueTask Complete() => await StateMachineHostContext.TryRemoveService(sessionId: null, _invokeId).ConfigureAwait(false);
}