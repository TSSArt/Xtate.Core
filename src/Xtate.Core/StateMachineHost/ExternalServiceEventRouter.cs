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
using Xtate.IoProcessor;

namespace Xtate.Core;

public class ExternalServiceEventRouter : IEventRouter
{
    public required ServiceList<IExternalServiceProvider> ExternalServiceProviders { private get; [UsedImplicitly] init; }

    public required IStateMachineSessionId StateMachineSessionId { private get; [UsedImplicitly] init; }

    public required IExternalServiceCollection ExternalServiceCollection { private get; [UsedImplicitly] init; }

#region Interface IEventRouter

    public bool CanHandle(FullUri? type)
    {
        if (type is null)
        {
            return false;
        }

        foreach (var externalServiceProvider in ExternalServiceProviders)
        {
            if (externalServiceProvider.TryGetActivator(type) is not null)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsInternalTarget(FullUri? target) => false;

    public ValueTask<IRouterEvent> GetRouterEvent(IOutgoingEvent outgoingEvent, CancellationToken token) =>
        new(new RouterEvent(StateMachineSessionId.SessionId, GetInvokeId(outgoingEvent.Target), Const.ScxmlIoProcessorId, Const.ParentTarget, outgoingEvent));

    public ValueTask Dispatch(IRouterEvent routerEvent, CancellationToken token) => ExternalServiceCollection.Dispatch((InvokeId)routerEvent.TargetServiceId!, routerEvent, token);

#endregion

    private static InvokeId? GetInvokeId(FullUri? target)
    {
        var str = target?.ToString();

        return str?.StartsWith(Const.ScxmlIoProcessorInvokeIdPrefix) == true ? InvokeId.FromString(str[Const.ScxmlIoProcessorInvokeIdPrefix.Length..]) : default;
    }
}