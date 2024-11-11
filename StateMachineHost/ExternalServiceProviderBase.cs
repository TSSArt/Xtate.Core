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

namespace Xtate.ExternalService;

public abstract class ExternalServiceProviderBase<TService>(FullUri uri, FullUri? aliasUri = default) : IExternalServiceProvider, IExternalServiceActivator
	where TService : IExternalService
{
	protected ExternalServiceProviderBase(string type, string? alias = default) : this(new FullUri(type), alias is not null ? new FullUri(alias) : default) { }

	public required Func<ValueTask<TService>> ServiceFactoryFunc { private get; [UsedImplicitly] init; }

#region Interface IExternalServiceActivator

	async ValueTask<IExternalService> IExternalServiceActivator.Create() => await ServiceFactoryFunc().ConfigureAwait(false);

#endregion

#region Interface IExternalServiceProvider

	IExternalServiceActivator? IExternalServiceProvider.TryGetActivator(FullUri typeUri) => typeUri == uri || (typeUri is not null && typeUri == aliasUri) ? this : default;

#endregion
}