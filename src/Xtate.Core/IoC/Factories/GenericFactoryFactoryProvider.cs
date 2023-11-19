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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtate.DataModel.XPath;

namespace Xtate.Core.IoC;

public sealed class GenericFactoryFactoryProvider : FactoryFactoryProvider
{
	private readonly ArgumentType       _argumentType;
	private readonly Delegate           _delegate;
	private readonly ImplementationType _implementationType;

	private readonly ConcurrentDictionary<Type, Delegate> _delegates = new();

	public GenericFactoryFactoryProvider(ImplementationType implementationType, ServiceType serviceType, ArgumentType argumentType)
	{
		_implementationType = implementationType;
		_argumentType = argumentType;
		_delegate = GetGenericFactory;

		ValidateGeneric(implementationType.Type, serviceType.Type);
	}

	public override Delegate GetDelegate() => _delegate;

	private static void ValidateGeneric(Type implementationType, Type serviceType)
	{
		if (EnumerateMethods<ValueTuple>(implementationType, serviceType).Any())
		{
			return;
		}

		throw new DependencyInjectionException(Res.Format(Resources.Exception_TypeDoesNotContainsMethodWithSignatureMethodCancellationToken, implementationType, serviceType));
	}

	private Delegate GetGenericFactory(Type type)
	{
		var delegates = _delegates;

		if (!delegates.TryGetValue(type, out var factory))
		{
			//factory = GetFactoryDelegate( default/*MapToImplementation(type)*/, type, _argumentType);v
			//_delegates = delegates.Add(type, factory);
		}

		return factory;
	}
	/*
	private Type MapToImplementation(Type type)
	{
		var methods = new List<(MethodInfo MethodInfo, Type[]? TypeArguments)>();

		foreach (var entry in EnumerateMethods(_implementationType, type))
		{
			if (entry.MethodArguments is null || CanCreateInstance(entry.MethodArguments))
			{
				if (entry.TypeArguments is null || CanCreateInstance(entry.TypeArguments))
				{
					methods.Add((entry.MethodInfo, entry.TypeArguments));
				}
			}
		}

		Type[]? typeArguments;
		try
		{
			typeArguments = methods.Count > 1
				? methods.Single(m => m.MethodInfo.GetCustomAttribute<ObsoleteAttribute>(false) is null).TypeArguments
				: methods.Single().TypeArguments;
		}
		catch (Exception ex)
		{
			throw new DependencyInjectionException(Res.Format(Resources.Exception_TypeDoesNotContainsMethodWithSignatureMethodCancellationToken, _implementationType, type), ex);
		}

		if (typeArguments is null)
		{
			return _implementationType;
		}

		return _implementationType.MakeGenericType(typeArguments);
	}*/
}