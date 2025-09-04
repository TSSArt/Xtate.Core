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

namespace Xtate;

public sealed partial class StateMachineHost : IExternalServiceProvider, IExternalServiceActivator
{
    private static readonly FullUri ServiceFactoryTypeId = new(@"http://www.w3.org/TR/scxml/");

    private static readonly FullUri ServiceFactoryAliasTypeId = new(@"scxml");

#region Interface IExternalServiceActivator

    public ValueTask<IExternalService> Create() => throw new NotImplementedException();

#endregion

#region Interface IExternalServiceProvider

    IExternalServiceActivator? IExternalServiceProvider.TryGetActivator(FullUri type) => CanHandle(type) ? this : null;

#endregion

    private static bool CanHandle(FullUri type) => type == ServiceFactoryTypeId || type == ServiceFactoryAliasTypeId;
    /*
    [Obsolete]
    async ValueTask<IExternalService> IExternalServiceActivator.Create(Uri? baseUri,
                                                                             InvokeData invokeData,
                                                                             IServiceCommunication serviceCommunication)
    {
        Infra.Assert(CanHandle(invokeData.Type));

        var sessionId = SessionId.New();
        var scxml = invokeData.RawContent ?? invokeData.Content.AsStringOrDefault();
        var parameters = invokeData.Parameters;
        var source = invokeData.Source;

        Infra.Assert(scxml is not null || source is not null);

        var stateMachineClass = scxml is not null
            ? (StateMachineClass) new ScxmlStringStateMachine(scxml) { Location = baseUri!, Arguments = parameters }
            : new LocationStateMachine(baseUri.CombineWith(source!)) { Arguments = parameters };

        return await StartStateMachine(stateMachineClass, SecurityContextType.InvokedService).ConfigureAwait(false);
    }*/
}