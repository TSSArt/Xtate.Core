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
using System.Collections.Generic;
using System.Linq;

namespace Xtate.Core.IoC;

public readonly struct ImplementationType : IEquatable<ImplementationType>
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
		_openGenericType = type.IsConstructedGenericType ? type.GetGenericTypeDefinition() : default;
	}

	private ImplementationType(Type? openGenericType)
	{
		Infra.NotNull(openGenericType);

		_type = _openGenericType = openGenericType;
	}

	public Type Type => _type ?? throw new InvalidOperationException(Resources.Exception_ServiceTypeNotInitialized);

	public bool IsClosedGeneric => _openGenericType is not null && !ReferenceEquals(_type, _openGenericType);

	public bool IsOpenGeneric => ReferenceEquals(_type, _openGenericType);

	public ImplementationType Definition => new(_openGenericType);

	#region Interface IEquatable<ImplementationType>

	public bool Equals(ImplementationType other) => _type == other._type;

#endregion

	public static ImplementationType TypeOf<T>() => Container<T>.Instance;

	public override bool Equals(object? obj) => obj is ImplementationType other && _type == other._type;

	public override int GetHashCode() => _type?.GetHashCode() ?? 0;

	public static bool operator ==(ImplementationType left, ImplementationType right) => left._type == right._type;

	public static bool operator !=(ImplementationType left, ImplementationType right) => left._type != right._type;

	public override string ToString() => _type?.Name ?? string.Empty;

	public bool IsResolvedType() => StubType.IsResolvedType(_type);

	public bool IsMatch(Type type) => TryMap(_type, type) is not null;

	public bool TryConstruct(ServiceType serviceType, out ImplementationType resultImplementationType)
	{
		Infra.NotNull(_type);

		if (EnumerateArguments(serviceType.Type).FirstOrDefault(static types => CanCreateInstance(types)) is { } args)
		{
			resultImplementationType = new ImplementationType(_type.MakeGenericType(args), validate: false);

			return true;
		}

		resultImplementationType = default;

		return false;
	}

	public bool CanConstruct(ServiceType serviceType)
	{
		Infra.NotNull(_type);

		return EnumerateArguments(serviceType.Type).Any();
	}

	private static bool CanCreateInstance(Type[] types) => Array.TrueForAll(types, static type => StubType.IsResolvedType(type));

	private IEnumerable<Type[]> EnumerateArguments(Type serviceType)
	{
		if (TryMap(_type, serviceType) is { } args)
		{
			yield return args;
		}

		foreach (var itf in _type.GetInterfaces())
		{
			if (TryMap(itf, serviceType) is { } args2)
			{
				yield return args2;
			}
		}
	}

	private Type[]? TryMap(Type type1, Type type2)
	{
		var implementationArguments = _type.GetGenericArguments();

		if (StubType.TryMap(implementationArguments, typesToMap2: default, type1, type2))
		{
			return implementationArguments;
		}

		return default;
	}

	private static class Container<T>
	{
		public static readonly ImplementationType Instance = new(typeof(T), validate: true);
	}
}