#region Copyright © 2019-2022 Sergii Artemenko

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

namespace Xtate.Core.IoC;

public readonly struct ServiceImplementation<TImplementation, TArg> where TImplementation : notnull
{
	private readonly IServiceCollection _serviceCollection;

	public ServiceImplementation(IServiceCollection serviceCollection, InstanceScope instanceScope)
	{
		Infra.Requires(serviceCollection);

		_serviceCollection = serviceCollection;

		serviceCollection.Add(new ServiceEntry(TypeKey.ImplementationKey<TImplementation, TArg>(), instanceScope, ImplementationAsyncFactoryProvider<TImplementation, TArg>.Delegate));
	}

	public ServiceImplementation<TImplementation, TArg> For<TService>() where TService : class
	{
		_serviceCollection.Add(new ServiceEntry(TypeKey.ServiceKey<TService, TArg>(), InstanceScope.Forwarding, ForwardFactoryProvider<TImplementation, TService, TArg>.Delegate));

		return this;
	}
}