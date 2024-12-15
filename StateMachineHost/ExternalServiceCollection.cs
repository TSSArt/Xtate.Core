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

public class ExternalServiceCollection : IExternalServiceCollection
{
	private readonly ExtDictionary<InvokeId, IExternalService> _externalServices = [];

	public required IExternalServicePublicCollection ExternalServicePublicCollection { private get; [UsedImplicitly] init; }

#region Interface IExternalServiceCollection

	public void Register(InvokeId invokeId)
	{
		ExternalServicePublicCollection.Register(invokeId.UniqueId);

		if (invokeId is not UniqueInvokeId)
		{
			var tryAddPending = _externalServices.TryAddPending(invokeId);

			Infra.Assert(tryAddPending);
		}
	}

	public void SetExternalService(InvokeId invokeId, IExternalService externalService)
	{
		ExternalServicePublicCollection.SetExternalService(invokeId.UniqueId, externalService);

		if (invokeId is not UniqueInvokeId)
		{
			var tryAdd = _externalServices.TryAdd(invokeId, externalService);

			Infra.Assert(tryAdd);
		}
	}

	public void Unregister(InvokeId invokeId)
	{
		ExternalServicePublicCollection.Unregister(invokeId.UniqueId);

		if (invokeId is not UniqueInvokeId)
		{
			var tryRemove = _externalServices.TryRemove(invokeId, out _);

			Infra.Assert(tryRemove);
		}
	}

	public async ValueTask<bool> TryDispatch(InvokeId invokeId, IIncomingEvent incomingEvent, CancellationToken token)
	{
		var (found, externalService) = await _externalServices.TryGetValueAsync(invokeId).ConfigureAwait(false);

		if (found)
		{
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

		return await ExternalServicePublicCollection.TryDispatch(invokeId.UniqueId, incomingEvent, token).ConfigureAwait(false);
	}

#endregion
}