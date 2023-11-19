#region Copyright © 2019-2023 Sergii Artemenko

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

using System.Reflection;

namespace Xtate.IoC;

internal readonly struct ImplementationType : IEquatable<ImplementationType>
{
	private readonly Type? _openGenericType;
	private readonly Type  _type;

	private ImplementationType(Type type, bool validate)
	{
		if (validate)
		{
			if (type.IsInterface || type.IsAbstract)
			{
				throw new ArgumentException(Resources.Exception_InvalidType, nameof(type));
			}
		}

		_type = type;
		_openGenericType = type.IsGenericType ? type.GetGenericTypeDefinition() : default;
	}

	private ImplementationType(Type? openGenericType)
	{
		Infra.NotNull(openGenericType);

		_type = _openGenericType = openGenericType;
	}

	public Type Type => _type ?? throw new InvalidOperationException(Resources.Exception_ServiceTypeNotInitialized);

	public bool IsGeneric => _openGenericType is not null;

	public ImplementationType Definition => _openGenericType is not null ? new ImplementationType(_openGenericType) : default;

#region Interface IEquatable<ImplementationType>

	public bool Equals(ImplementationType other) => _type == other._type;

#endregion

	public static ImplementationType TypeOf<T>() => Infra.TypeInitHandle(() => Container<T>.Instance);

	public override bool Equals(object? obj) => obj is ImplementationType other && _type == other._type;

	public override int GetHashCode() => _type?.GetHashCode() ?? 0;

	public override string ToString() => _type?.FriendlyName() ?? string.Empty;

	public bool TryConstruct(ServiceType serviceType, out ImplementationType resultImplementationType)
	{
		if (EnumerateContracts(serviceType.Type).FirstOrDefault(static contract => contract.CanCreateType()) is { IsDefault: false } contract)
		{
			resultImplementationType = new ImplementationType(contract.CreateType(), validate: false);

			return true;
		}

		resultImplementationType = default;

		return false;
	}

	private IEnumerable<Contract> EnumerateContracts(Type serviceType)
	{
		var implType = _openGenericType ?? _type;

		for (var type = implType; type is not null; type = type.BaseType)
		{
			if (TryMap(type, serviceType) is { } args)
			{
				yield return new Contract(implType, args);
			}
		}

		foreach (var itf in implType.GetInterfaces())
		{
			if (TryMap(itf, serviceType) is { } args)
			{
				yield return new Contract(implType, args);
			}
		}
	}

	private static Method FindMethod(IEnumerable<Method> methods)
	{
		Method? obsoleteMethod = default;
		Method? actualMethod = default;
		var multipleObsolete = false;
		var multipleActual = false;

		foreach (var method in methods)
		{
			if (!method.CanCreateMethodInfo())
			{
				continue;
			}

			if (method.HasObsoleteAttribute())
			{
				if (obsoleteMethod is null)
				{
					obsoleteMethod = method;
				}
				else
				{
					multipleObsolete = true;
				}
			}
			else
			{
				if (actualMethod is null)
				{
					actualMethod = method;
				}
				else
				{
					multipleActual = true;

					break;
				}
			}
		}

		if (multipleActual || (actualMethod is null && multipleObsolete))
		{
			throw new DependencyInjectionException(Resources.Exception_MoreThanOneMethodFound);
		}

		if ((actualMethod ?? obsoleteMethod) is { } resultMethod)
		{
			return resultMethod;
		}

		throw new DependencyInjectionException(Resources.Exception_NoMethodFound);
	}

	public MethodInfo GetMethodInfo<TService, TArg>(bool synchronousOnly)
	{
		try
		{
			var method = FindMethod(EnumerateMethods<TArg>(typeof(TService), synchronousOnly));

			return method.CreateMethodInfo();
		}
		catch (Exception ex)
		{
			throw new DependencyInjectionException(Res.Format(Resources.Exception_TypeDoesNotContainsMethodWithSignatureMethodCancellationToken, _type, typeof(TService)), ex);
		}
	}

	private IEnumerable<Method> EnumerateMethods<TArg>(Type serviceType, bool synchronousOnly)
	{
		var implType = _openGenericType ?? _type;

		var allMethods = implType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
		var resolvedTypeArguments = _type.IsGenericType ? _type.GetGenericArguments() : default;

		foreach (var methodInfo in allMethods)
		{
			if (ValidParameters<TArg>(methodInfo, synchronousOnly))
			{
				var typeArguments = implType.IsGenericType ? implType.GetGenericArguments() : default;
				var methodArguments = methodInfo.IsGenericMethod ? methodInfo.GetGenericArguments() : default;

				if (StubType.TryMap(typeArguments, methodArguments, serviceType, GetReturnType(methodInfo, synchronousOnly)) &&
					StubType.TryMap(typeArguments, methodArguments, resolvedTypeArguments, typeArguments))
				{
					yield return new Method(implType, methodInfo, typeArguments, methodArguments);
				}
			}
		}
	}

	private static bool ValidParameters<TArg>(MethodBase methodBase, bool synchronousOnly)
	{
		foreach (var parameterInfo in methodBase.GetParameters())
		{
			if (!synchronousOnly && parameterInfo.ParameterType == typeof(CancellationToken))
			{
				continue;
			}

			if (TupleHelper.IsMatch<TArg>(parameterInfo.ParameterType))
			{
				continue;
			}

			return false;
		}

		return true;
	}

	private static Type GetReturnType(MethodInfo methodInfo, bool synchronousOnly)
	{
		if (!synchronousOnly && methodInfo.ReturnType is { IsGenericType: true } rt && rt.GetGenericTypeDefinition() == typeof(ValueTask<>))
		{
			return rt.GetGenericArguments()[0];
		}

		return methodInfo.ReturnType;
	}

	private Type[]? TryMap(Type type1, Type type2)
	{
		var implementationArguments = _openGenericType?.GetGenericArguments() ?? Array.Empty<Type>();

		if (StubType.TryMap(implementationArguments, typesToMap2: default, type1, type2) &&
			StubType.TryMap(typesToMap1: default, typesToMap2: default, implementationArguments, _type.GetGenericArguments()))
		{
			return implementationArguments;
		}

		return default;
	}

	private readonly struct Contract
	{
		private readonly Type[] _args;
		private readonly Type    _type;

		public Contract(Type type, Type[] args)
		{
			_type = type;
			_args = args;
		}

		public bool IsDefault => _type is null;

		public Type CreateType() => _args.Length > 0 ? _type.MakeGenericType(_args) : _type;

		public bool CanCreateType()
		{
			foreach (var type in _args)
			{
				if (!StubType.IsResolvedType(type))
				{
					return false;
				}
			}

			return true;
		}
	}

	private readonly struct Method
	{
		private readonly Type[]?    _methodArguments;
		private readonly MethodInfo _methodInfo;
		private readonly Type       _type;
		private readonly Type[]?    _typeArguments;

		public Method(Type type,
					  MethodInfo methodInfo,
					  Type[]? typeArguments,
					  Type[]? methodArguments)
		{
			_type = type;
			_methodInfo = methodInfo;
			_typeArguments = typeArguments;
			_methodArguments = methodArguments;
		}

		public MethodInfo CreateMethodInfo()
		{
			var methodInfo = _methodInfo;

			if (_typeArguments is not null)
			{
				var metadataToken = _methodInfo.MetadataToken;

				foreach (var mi in _type.MakeGenericType(_typeArguments).GetMethods())
				{
					if (mi.MetadataToken == metadataToken)
					{
						methodInfo = mi;

						break;
					}
				}
			}

			return _methodArguments is null ? methodInfo : methodInfo.MakeGenericMethod(_methodArguments);
		}

		public bool HasObsoleteAttribute() => _methodInfo.GetCustomAttribute<ObsoleteAttribute>(inherit: false) is { IsError: false };

		public bool CanCreateMethodInfo()
		{
			if (_typeArguments is not null)
			{
				foreach (var type in _typeArguments)
				{
					if (!StubType.IsResolvedType(type))
					{
						return false;
					}
				}
			}

			if (_methodArguments is not null)
			{
				foreach (var type in _methodArguments)
				{
					if (!StubType.IsResolvedType(type))
					{
						return false;
					}
				}
			}

			return true;
		}
	}

	private static class Container<T>
	{
		public static readonly ImplementationType Instance = new(typeof(T), validate: true);
	}
}