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
	private readonly MiniDictionary<InvokeId, TaskCompletionSource<IExternalService>> _externalServices = new(InvokeId.InvokeUniqueIdComparer);

#region Interface IExternalServiceCollection

	public void Register(InvokeId invokeId)
	{
		var tryAdd = _externalServices.TryAdd(invokeId, new TaskCompletionSource<IExternalService>());

		Infra.Assert(tryAdd);
	}

	public void SetExternalService(InvokeId invokeId, IExternalService externalService)
	{
		_externalServices.TryGetValue(invokeId, out var tcs);

		Infra.Assert(tcs != null);

		tcs.SetResult(externalService);
	}

	public void Unregister(InvokeId invokeId)
	{
		if (_externalServices.TryRemove(invokeId, out var tcs))
		{
			tcs.TrySetCanceled();
		}
	}

	public virtual async ValueTask Dispatch(InvokeId invokeId, IIncomingEvent incomingEvent, CancellationToken token)
	{
		if (_externalServices.TryGetValue(invokeId, out var tcs) && await tcs.Task.ConfigureAwait(false) is IEventDispatcher eventDispatcher)
		{
			if (incomingEvent is not IncomingEvent)
			{
				incomingEvent = new IncomingEvent(incomingEvent);
			}

			await eventDispatcher.Dispatch(incomingEvent, token).ConfigureAwait(false);
		}
	}

#endregion
}