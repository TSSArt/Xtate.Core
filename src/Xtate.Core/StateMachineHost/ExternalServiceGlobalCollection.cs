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

public class ExternalServiceGlobalCollection : IExternalServiceGlobalCollection
{
	private readonly ExtDictionary<UniqueInvokeId, IExternalService> _externalServices = [];

#region Interface IExternalServiceCollection

	public void Register(UniqueInvokeId uniqueInvokeId)
	{
		var tryAddPending = _externalServices.TryAddPending(uniqueInvokeId);

		Infra.Assert(tryAddPending);
	}

	public void SetExternalService(UniqueInvokeId uniqueInvokeId, IExternalService externalService)
	{
		var tryAdd = _externalServices.TryAdd(uniqueInvokeId, externalService);

		Infra.Assert(tryAdd);
	}

	public void Unregister(UniqueInvokeId uniqueInvokeId)
	{
		var tryRemove = _externalServices.TryRemove(uniqueInvokeId, out _);

		Infra.Assert(tryRemove);
	}

	public async ValueTask<bool> TryDispatch(UniqueInvokeId uniqueInvokeId, IIncomingEvent incomingEvent, CancellationToken token)
	{
		var (found, externalService) = await _externalServices.TryGetValueAsync(uniqueInvokeId).ConfigureAwait(false);

		if (!found)
		{
			return false;
		}

		if (externalService is IEventDispatcher eventDispatcher)
		{
			if (incomingEvent is not IncomingEvent)
			{
				incomingEvent = new IncomingEvent(incomingEvent);
			}

			await eventDispatcher.Dispatch(incomingEvent, token).ConfigureAwait(false);
		}

		return true;
	}

#endregion
}