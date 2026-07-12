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

public class InternalEventDispatcher<TSource> : IInternalEventDispatcher<TSource>
{
	public required IStateMachineCollection StateMachineCollection { private get; [SetByIoC] init; }

	public required IExternalServiceCollection ExternalServiceCollection { private get; [SetByIoC] init; }

	public required IDeadLetterQueue<TSource> DeadLetterQueue { private get; [SetByIoC] init; }

#region Interface IInternalEventDispatcher<TSource>

	public ValueTask Dispatch(ServiceId serviceId, IIncomingEvent incomingEvent, CancellationToken token) =>
		serviceId switch
		{
			SessionId sessionId => StateMachineCollection.Dispatch(sessionId, incomingEvent, token),
			InvokeId invokeId   => ExternalServiceCollection.Dispatch(invokeId, incomingEvent, token),
			_                   => DeadLetterQueue.Enqueue(serviceId, incomingEvent)
		};

#endregion
}