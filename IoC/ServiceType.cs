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

public readonly struct ServiceType : IEquatable<ServiceType>
{
	private readonly Type? _openGenericType;
	private readonly Type  _type;

	private ServiceType(Type type, bool validate)
	{
		_type = type;
		_openGenericType = type.IsConstructedGenericType ? type.GetGenericTypeDefinition() : default;
	}

	private ServiceType(Type? openGenericType)
	{
		Infra.NotNull(openGenericType);

		_type = _openGenericType = openGenericType;
	}

	public Type Type => _type ?? throw new InvalidOperationException(Resources.Exception_ServiceTypeNotInitialized);

	public bool IsClosedGeneric => _openGenericType is not null && !ReferenceEquals(_type, _openGenericType);

	public bool IsOpenGeneric => ReferenceEquals(_type, _openGenericType);

	public ServiceType Definition => new (_openGenericType);

#region Interface IEquatable<ServiceType>

	public bool Equals(ServiceType other) => _type == other._type;

#endregion

	public static ServiceType TypeOf<T>() => Container<T>.Instance;

	public static ServiceType From(ImplementationType implementationType) => new(implementationType.Type, validate: false);

	public override bool Equals(object? obj) => obj is ServiceType other && _type == other._type;

	public override int GetHashCode() => _type?.GetHashCode() ?? 0;

	public static bool operator ==(ServiceType left, ServiceType right) => left._type == right._type;

	public static bool operator !=(ServiceType left, ServiceType right) => left._type != right._type;

	public override string ToString() => _type?.Name ?? string.Empty;

	private static class Container<T>
	{
		public static readonly ServiceType Instance = new(typeof(T), validate: true);
	}
}