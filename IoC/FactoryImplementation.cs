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

using System;
using Empty = System.ValueTuple;

namespace Xtate.Core.IoC;

public readonly struct FactoryImplementation<TImplementation> where TImplementation : notnull
{
	private readonly IServiceCollection _serviceCollection;

	public FactoryImplementation(IServiceCollection serviceCollection, InstanceScope instanceScope)
	{
		Infra.Requires(serviceCollection);

		_serviceCollection = serviceCollection;

		serviceCollection.Add(new ServiceEntry(TypeKey.ImplementationKey<TImplementation, Empty>(), instanceScope, ImplementationAsyncFactoryProvider<TImplementation, Empty>.Delegate));
	}

	public FactoryImplementation<TImplementation> For<TService>() where TService : class
	{
		_serviceCollection.Add(new ServiceEntry(TypeKey.ServiceKey<TService, Empty>(), InstanceScope.Forwarding, FactoryFactoryProvider<TImplementation, TService, Empty>.Delegate));

		return this;
	}

	public FactoryImplementation<TImplementation> For<TService, TArg>() where TService : class
	{
		_serviceCollection.Add(new ServiceEntry(TypeKey.ServiceKey<TService, TArg>(), InstanceScope.Forwarding, FactoryFactoryProvider<TImplementation, TService, TArg>.Delegate));

		return this;
	}
}

public readonly struct FactoryImplementation<TImplementation, TImplArg> where TImplementation : notnull
{
	private readonly IServiceCollection _serviceCollection;

	public FactoryImplementation(IServiceCollection serviceCollection, InstanceScope instanceScope)
	{
		Infra.Requires(serviceCollection);

		_serviceCollection = serviceCollection;

		serviceCollection.Add(new ServiceEntry(TypeKey.ImplementationKey<TImplementation, TImplArg>(), instanceScope, ImplementationAsyncFactoryProvider<TImplementation, TImplArg>.Delegate));
	}

	public FactoryImplementation<TImplementation, TImplArg> For<TService, TArg>() where TService : class
	{
		_serviceCollection.Add(new ServiceEntry(TypeKey.ServiceKey<TService, TArg>(), InstanceScope.Forwarding, FactoryFactoryProvider<TImplementation, TService, TArg>.Delegate));

		return this;
	}
	public FactoryImplementation<TImplementation, TImplArg> For<TService>() where TService : class
	{
		throw new NotImplementedException(); //TODO:ADD
	}
}