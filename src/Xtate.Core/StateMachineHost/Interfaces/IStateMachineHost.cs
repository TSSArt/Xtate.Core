// Copyright © 2019-2025 Sergii Artemenko
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

using Xtate.IoProcessor;

namespace Xtate.Core;

public interface IStateMachineHost : IHostEventDispatcher
{
    ImmutableArray<IEventRouter> GetIoProcessors();

    ValueTask<SendStatus> DispatchEvent(ServiceId serviceId, IOutgoingEvent outgoingEvent, CancellationToken token);

    ValueTask CancelEvent(SessionId sessionId, SendId sendId, CancellationToken token);

    ValueTask StartInvoke(SessionId sessionId,
                          Uri? location,
                          InvokeData invokeData,
                          CancellationToken token);

    ValueTask CancelInvoke(SessionId sessionId, InvokeId invokeId, CancellationToken token);

    ValueTask ForwardEvent(SessionId sessionId,
                           InvokeId invokeId,
                           IIncomingEvent incomingEvent,
                           CancellationToken token);
}