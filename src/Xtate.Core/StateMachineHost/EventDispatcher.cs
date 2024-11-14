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

public class EventDispatcher : IEventDispatcher
{
	public required IExternalServiceInvokeId? ExternalServiceInvokeId { private get; [UsedImplicitly] init; }

	public required Deferred<IEventQueueWriter> EventQueueWriter { private get; [UsedImplicitly] init; }

	public required Deferred<IExternalService> ExternalService { private get; [UsedImplicitly] init; }

#region Interface IEventDispatcher

	public async ValueTask Dispatch(IIncomingEvent incomingEvent, CancellationToken token)
	{
		var eventDispatcher = ExternalServiceInvokeId is null
			? await EventQueueWriter().ConfigureAwait(false) as IEventDispatcher
			: await ExternalService().ConfigureAwait(false) as IEventDispatcher;

		if (eventDispatcher is not null)
		{
			await eventDispatcher.Dispatch(incomingEvent, token).ConfigureAwait(false);
		}
	}

#endregion
}