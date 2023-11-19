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
using System.Collections.Concurrent;

namespace Xtate.Core.IoC;

public sealed class GenericForwardFactoryProvider : FactoryProvider
{
	private readonly ArgumentType                                _argumentType;
	private readonly Delegate                                    _delegate;
	private readonly ConcurrentDictionary<ServiceType, Delegate> _delegates = new();
	private readonly ImplementationType                          _implementationType;

	public GenericForwardFactoryProvider(ImplementationType implementationType, ServiceType serviceType, ArgumentType argumentType)
	{
		_implementationType = implementationType;
		_argumentType = argumentType;
		_delegate = GetGenericFactory;

		if (!implementationType.CanConstruct(serviceType))
		{
			throw new DependencyInjectionException(Res.Format(Resources.Exception_GenericTypeCantBeConstructedFromInterface, implementationType, serviceType));
		}
	}

	public override Delegate GetDelegate() => _delegate;

	private Delegate GetGenericFactory(ServiceType serviceType) => _delegates.GetOrAdd(serviceType, static (type, provider) => provider.Create(type), this);

	private Delegate Create(ServiceType serviceType)
	{
		if (_implementationType.TryConstruct(serviceType, out var resultImplementationType))
		{
			return GetForwardDelegate(resultImplementationType, serviceType, _argumentType);
		}

		throw new DependencyInjectionException(Res.Format(Resources.Exception_TypeCantBeConstructedBasedOnServiceType, _implementationType, serviceType));
	}
}