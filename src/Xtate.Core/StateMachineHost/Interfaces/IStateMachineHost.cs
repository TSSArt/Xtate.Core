#region Copyright © 2019-2021 Sergii Artemenko

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

#endregion

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.IoProcessor;

namespace Xtate.Core
{
	internal interface IStateMachineHost
	{
		ImmutableArray<IIoProcessor> GetIoProcessors();
		ValueTask<SendStatus>        DispatchEvent(SessionId sessionId, IOutgoingEvent evt, bool skipDelay, CancellationToken token);
		ValueTask                    StartInvoke(SessionId sessionId, InvokeData invokeData, SecurityContext securityContext, CancellationToken token);
		ValueTask                    CancelInvoke(SessionId sessionId, InvokeId invokeId, CancellationToken token);
		bool                         IsInvokeActive(SessionId sessionId, InvokeId invokeId);
		ValueTask                    ForwardEvent(SessionId sessionId, IEvent evt, InvokeId invokeId, CancellationToken token);
	}
}