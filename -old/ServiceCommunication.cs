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

using Xtate.ExternalService;

namespace Xtate.Core;

[Obsolete]
internal class ServiceCommunication(
    IStateMachineHost host,
    FullUri? target,
    FullUri type,
    InvokeId invokeId) : IServiceCommunication
{
#region Interface IServiceCommunication

    public async ValueTask SendToCreator(IOutgoingEvent outgoingEvent, CancellationToken token)
    {
        if (outgoingEvent.Type is not null || outgoingEvent.SendId is not null || outgoingEvent.DelayMs != 0)
        {
            throw new ProcessorException(Resources.Exception_TypeSendIdDelayMsCantBeSpecifiedForThisEvent);
        }

        if (outgoingEvent.Target != Const.ParentTarget && outgoingEvent.Target is not null)
        {
            throw new ProcessorException(Resources.Exception_TargetShouldBeEqualToParentOrNull);
        }

        var newOutgoingEvent = new EventEntity
                               {
                                   Name = outgoingEvent.Name,
                                   Data = outgoingEvent.Data,
                                   Type = type,
                                   Target = target
                               };

        await host.DispatchEvent(invokeId, newOutgoingEvent, token).ConfigureAwait(false);
    }

#endregion
}