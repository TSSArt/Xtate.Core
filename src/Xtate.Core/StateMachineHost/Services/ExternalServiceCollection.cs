// Copyright © 2019-2026 Sergii Artemenko
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
using Xtate.StateMachine;

namespace Xtate.StateMachineHost.Services;

[InstantiatedByIoC]
public class ExternalServiceCollection : IExternalServiceCollection
{
	private readonly ExtDictionary<InvokeId, IExternalServiceController> _externalServiceControllers = [];

	public required IExternalServiceGlobalCollection ExternalServiceGlobalCollection { private get; [SetByIoC] init; }

	public required IDeadLetterQueue<IExternalServiceCollection> DeadLetterQueue { private get; [SetByIoC] init; }

#region Interface IExternalServiceCollection

	public void Register(InvokeId invokeId)
	{
		var tryAddPending = _externalServiceControllers.TryAddPending(invokeId);

		Infra.Assert(tryAddPending);

		ExternalServiceGlobalCollection.Register(invokeId.UniqueId);
	}

	public void SetController(InvokeId invokeId, IExternalServiceController externalServiceController)
	{
		var tryAdd = _externalServiceControllers.TryAdd(invokeId, externalServiceController);

		Infra.Assert(tryAdd);

		ExternalServiceGlobalCollection.SetExternalServiceController(invokeId.UniqueId, externalServiceController);
	}

	public void Unregister(InvokeId invokeId)
	{
		ExternalServiceGlobalCollection.Unregister(invokeId.UniqueId);

		_externalServiceControllers.TryRemove(invokeId, out _);
	}

	public async ValueTask Dispatch(InvokeId invokeId, IIncomingEvent incomingEvent, CancellationToken token)
	{
		var (found, externalServiceController) = await _externalServiceControllers.TryGetValueAsync(invokeId).ConfigureAwait(false);

		if (found)
		{
			await externalServiceController.Dispatch(incomingEvent, token).ConfigureAwait(false);

			return;
		}

		if (await ExternalServiceGlobalCollection.TryDispatch(invokeId.UniqueId, incomingEvent, token).ConfigureAwait(false))
		{
			return;
		}

		await DeadLetterQueue.Enqueue(invokeId, incomingEvent).ConfigureAwait(false);
	}

#endregion
}
