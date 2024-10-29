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

public class ExternalServiceRunner : IExternalServiceRunner, IDisposable
{
	private readonly AsyncInit _actionOnComplete;

	private readonly IExternalService _externalService;

	private readonly IStateMachineHostContext _stateMachineHostContext;

	private InvokeId? _invokeId;

	//private          ValueTask _actionOnComplete;

	public ExternalServiceRunner(IStateMachineSessionId stateMachineSessionId,
								 IStateMachineInvokeId stateMachineInvokeId,
								 IExternalService externalService,
								 IStateMachineHostContext stateMachineHostContext)
	{
		_invokeId = stateMachineInvokeId.InvokeId;
		_externalService = externalService;
		_stateMachineHostContext = stateMachineHostContext;
		_stateMachineHostContext.AddService(stateMachineSessionId.SessionId, _invokeId, _externalService, token: default);

		//_actionOnComplete = ActionOnComplete().Preserve();

		_actionOnComplete = AsyncInit.Run(ActionOnComplete);
	}

	public required DataConverter DataConverter { private get; init; }

	public required IEventDispatcher Creator { private get; init; }

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IExternalServiceRunner

	public ValueTask WaitForCompletion() => new(_actionOnComplete.Task);

#endregion

	private async ValueTask ActionOnComplete()
	{
		try
		{
			var result = await _externalService.GetResult().ConfigureAwait(false);

			var nameParts = EventName.GetDoneInvokeNameParts(_invokeId);
			var evt = new EventObject { Type = EventType.External, NameParts = nameParts, Data = result, InvokeId = _invokeId };
			await Creator.Send(evt, token: default).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var evt = new EventObject
					  {
						  Type = EventType.External,
						  NameParts = EventName.ErrorExecution,
						  Data = DataConverter.FromException(ex),
						  InvokeId = _invokeId
					  };
			await Creator.Send(evt, token: default).ConfigureAwait(false);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (Interlocked.Exchange(ref _invokeId, value: default) is { } invokeId)
			{
				_stateMachineHostContext.TryRemoveService(sessionId: null, invokeId);
			}
		}
	}
}