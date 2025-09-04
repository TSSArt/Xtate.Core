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

namespace Xtate.ExternalService;

public class ExternalServiceFactory
{
    public required IAsyncEnumerable<IExternalServiceProvider> ServiceFactories { private get; [UsedImplicitly] init; }

    public required IExternalServiceType ExternalServiceType { private get; [UsedImplicitly] init; }

    [UsedImplicitly]
    public async ValueTask<IExternalService> CreateService()
    {
        var serviceActivator = await GetServiceActivator(ExternalServiceType.Type).ConfigureAwait(false);

        return await serviceActivator.Create().ConfigureAwait(false);
    }

    private async ValueTask<IExternalServiceActivator> GetServiceActivator(FullUri type)
    {
        var serviceFactories = ServiceFactories.GetAsyncEnumerator();

        await using (serviceFactories.ConfigureAwait(false))
        {
            while (await serviceFactories.MoveNextAsync().ConfigureAwait(false))
            {
                Infra.NotNull(serviceFactories.Current);

                if (serviceFactories.Current.TryGetActivator(type) is not { } serviceActivator)
                {
                    continue;
                }

                while (await serviceFactories.MoveNextAsync().ConfigureAwait(false))
                {
                    if (serviceFactories.Current.TryGetActivator(type) is not null)
                    {
                        Infra.Fail(Res.Format(Resources.Exception_MoreThanOneServiceFactoryRegisteredForPprocessingInvokeType, type));
                    }
                }

                return serviceActivator;
            }

            throw Infra.Fail<Exception>(Res.Format(Resources.Exception_ThereIsNoAnyServiceFactoryRegisteredForPprocessingInvokeType, type));
        }
    }
}