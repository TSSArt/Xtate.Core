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

namespace Xtate.Core.IoC;

public abstract class FactoryProvider
{
	public abstract Delegate GetDelegate();
	
	protected static void Validate<TImplementation, TService>()
	{
		if (!typeof(TService).IsAssignableFrom(typeof(TImplementation)))
		{
			throw new DependencyInjectionException(string.Format(Resources.Exception_TypeCantBeCastedTo, typeof(TImplementation), typeof(TService)));
		}
	}

	public static Delegate GetAsyncImplementationDelegate(ImplementationType implementationType, ArgumentType argumentType)
	{
		if (implementationType.IsOpenGeneric)
		{
			return new GenericImplementationFactoryProvider(implementationType, argumentType).GetDelegate();
		}

		var factoryProviderType = typeof(ImplementationAsyncFactoryProvider<,>).MakeGenericType(implementationType.Type, argumentType.Type);

		return ((FactoryProvider) Activator.CreateInstance(factoryProviderType)!).GetDelegate();
	}

	public static Delegate GetForwardDelegate(ImplementationType implementationType, ServiceType serviceType, ArgumentType argumentType)
	{
		if (implementationType.IsOpenGeneric || serviceType.IsOpenGeneric)
		{
			return new GenericForwardFactoryProvider(implementationType, serviceType, argumentType).GetDelegate();
		}

		var factoryProviderType = typeof(ForwardFactoryProvider<,,>).MakeGenericType(implementationType.Type, serviceType.Type, argumentType.Type);

		return ((FactoryProvider) Activator.CreateInstance(factoryProviderType)!).GetDelegate();
	}

	public static Delegate GetFactoryDelegate(ImplementationType implementationType, ServiceType serviceType, ArgumentType argumentType)
	{
		Infra.Requires(serviceType);

		if (implementationType.IsOpenGeneric || serviceType.IsOpenGeneric)
		{
			return new GenericFactoryFactoryProvider(implementationType, serviceType, argumentType).GetDelegate();
		}

		var factoryProviderType = typeof(FactoryFactoryProvider<,,>).MakeGenericType(implementationType.Type, serviceType.Type, argumentType.Type);

		return ((FactoryProvider) Activator.CreateInstance(factoryProviderType)!).GetDelegate();
	}

	public static Delegate GetDecoratorDelegate(ImplementationType implementationType, ServiceType serviceType, ArgumentType argumentType)
	{
		var factoryProviderType = typeof(DecoratorFactoryProvider<,,>).MakeGenericType(implementationType.Type, serviceType.Type, argumentType.Type);

		return ((FactoryProvider) Activator.CreateInstance(factoryProviderType)!).GetDelegate();
	}

	protected static bool CanCreateInstance(Type[] types)
	{
		Infra.Requires(types);

		return Array.TrueForAll(types, t => !t.ContainsGenericParameters);
	}

	protected static bool TryMap(Type[]? typesToMap1,
								 Type[]? typesToMap2,
								 Type? arg1,
								 Type? arg2)
	{
		if (arg1 == arg2)
		{
			return true;
		}

		if (arg1?.IsGenericParameter == true)
		{
			UpdateType(typesToMap1, arg1, arg2);
			UpdateType(typesToMap2, arg1, arg2);

			return true;
		}

		if (arg2?.IsGenericParameter == true)
		{
			UpdateType(typesToMap1, arg2, arg1);
			UpdateType(typesToMap2, arg2, arg1);

			return true;
		}

		if ((arg1?.IsArray ?? true) && (arg2?.IsArray ?? true))
		{
			return TryMap(typesToMap1, typesToMap2, arg1?.GetElementType(), arg2?.GetElementType());
		}

		if ((arg1?.IsGenericType ?? true) && (arg2?.IsGenericType ?? true))
		{
			if (arg1 is not null && arg2 is not null && arg1.GetGenericTypeDefinition() != arg2.GetGenericTypeDefinition())
			{
				return false;
			}

			return TryMap(
				typesToMap1, typesToMap2,
				arg1?.IsGenericTypeDefinition == false ? arg1.GetGenericArguments() : default,
				arg2?.IsGenericTypeDefinition == false ? arg2.GetGenericArguments() : default);
		}

		return arg1 is null || arg2 is null;
	}

	private static bool TryMap(Type[]? typesToMap1,
							   Type[]? typesToMap2,
							   Type[]? args1,
							   Type[]? args2)
	{
		if (args1 is not null && args2 is not null && args1.Length != args2.Length)
		{
			return false;
		}

		var length = args1?.Length ?? args2?.Length ?? 0;

		for (var i = 0; i < length; i ++)
		{
			if (!TryMap(typesToMap1, typesToMap2, args1?[i], args2?[i]))
			{
				return false;
			}
		}

		return true;
	}

	private static void UpdateType(Type[]? typesToMap, Type from, Type? to)
	{
		if (typesToMap is not null && Array.IndexOf(typesToMap, from) is var index and >= 0)
		{
			typesToMap[index] = to ?? typeof(void);
		}
	}
}