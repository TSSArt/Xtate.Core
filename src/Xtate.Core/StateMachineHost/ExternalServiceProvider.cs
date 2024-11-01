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

public abstract class ExternalServiceProvider<TService>(string type, string? alias = default) : IExternalServiceProvider, IExternalServiceActivator where TService : IExternalService
{
	private readonly Uri? _aliasUri = alias is not null ? new Uri(alias, UriKind.RelativeOrAbsolute) : default;

	private readonly Uri _typeUri = new(type, UriKind.RelativeOrAbsolute);

	public required Func<ValueTask<TService>> ServiceFactoryFunc { private get; [UsedImplicitly] init; }

#region Interface IExternalServiceActivator

	async ValueTask<IExternalService> IExternalServiceActivator.StartService() => await ServiceFactoryFunc().ConfigureAwait(false);

#endregion

#region Interface IExternalServiceProvider

	ValueTask<IExternalServiceActivator?> IExternalServiceProvider.TryGetActivator(Uri typeUri) =>
		FullUriComparer.Instance.Equals(_typeUri, typeUri) || (_aliasUri is not null && FullUriComparer.Instance.Equals(_aliasUri, typeUri))
			? new ValueTask<IExternalServiceActivator?>(this)
			: default;

#endregion
}