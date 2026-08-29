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
public class ExternalServiceGlobalCollection : IExternalServiceGlobalCollection
{
	private readonly ExtDictionary<UniqueInvokeId, IExternalServiceController> _externalServiceControllers = [];

#region Interface IExternalServiceGlobalCollection

	public void Register(UniqueInvokeId uniqueInvokeId)
	{
		var tryAddPending = _externalServiceControllers.TryAddPending(uniqueInvokeId);

		Infra.Assert(tryAddPending);
	}

	public void SetExternalServiceController(UniqueInvokeId uniqueInvokeId, IExternalServiceController externalServiceController)
	{
		var tryAdd = _externalServiceControllers.TryAdd(uniqueInvokeId, externalServiceController);

		Infra.Assert(tryAdd);
	}

	public void Unregister(UniqueInvokeId uniqueInvokeId) => _externalServiceControllers.TryRemove(uniqueInvokeId, out _);

	public async ValueTask<bool> TryDispatch(UniqueInvokeId uniqueInvokeId, IIncomingEvent incomingEvent, CancellationToken token)
	{
		var (found, externalServiceController) = await _externalServiceControllers.TryGetValueAsync(uniqueInvokeId).ConfigureAwait(false);

		if (!found)
		{
			return false;
		}

		await externalServiceController.Dispatch(incomingEvent, token).ConfigureAwait(false);

		return true;
	}

#endregion
}
