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

internal static class NullabilityHelper
{
	private const int    CanBeNull           = 2;
	private const string NullableAttr        = @"System.Runtime.CompilerServices.NullableAttribute";
	private const string NullableContextAttr = @"System.Runtime.CompilerServices.NullableContextAttribute";

	public static bool IsNullable(ParameterInfo parameter, string path)
	{
		Infra.Requires(parameter);
		Infra.Requires(path);

		return IsNullable(parameter.ParameterType, parameter.CustomAttributes, parameter.Member, path);
	}

	public static bool IsNullable(FieldInfo field, string path)
	{
		Infra.Requires(field);
		Infra.Requires(path);

		return IsNullable(field.FieldType, field.CustomAttributes, field.DeclaringType, path);
	}

	public static bool IsNullable(PropertyInfo property, string path)
	{
		Infra.Requires(property);
		Infra.Requires(path);

		return IsNullable(property.PropertyType, property.CustomAttributes, property.DeclaringType, path);
	}

	private static bool IsNullable(Type memberType,
								   IEnumerable<CustomAttributeData> attributes,
								   MemberInfo? declaringType,
								   string path)
	{
		var index = 0;

		if (FindType(memberType, path, ref index, level: 0) is not { } type)
		{
			return false;
		}

		if (Nullable.GetUnderlyingType(type) is not null)
		{
			return true;
		}

		if (type.IsValueType)
		{
			return false;
		}

		return CheckNullableAttribute(attributes, declaringType, index);
	}

	private static Type? FindType(Type type,
								  string path,
								  ref int index,
								  int level)
	{
		if (level == path.Length)
		{
			return type;
		}

		if (IsBytePresent(ref type))
		{
			index ++;
		}

		if (type.IsGenericType)
		{
			var pos = '0';
			foreach (var argType in type.GetGenericArguments())
			{
				if (pos ++ == path[level])
				{
					return FindType(argType, path, ref index, level + 1);
				}

				Walk(argType, ref index);
			}
		}

		return default;
	}

	private static void Walk(Type type, ref int index)
	{
		if (IsBytePresent(ref type))
		{
			index ++;
		}

		if (type.IsGenericType)
		{
			foreach (var argType in type.GetGenericArguments())
			{
				Walk(argType, ref index);
			}
		}
	}

	private static bool IsBytePresent(ref Type type)
	{
		if (!type.IsValueType)
		{
			return true;
		}

		if (!type.IsGenericType)
		{
			return false;
		}

		if (Nullable.GetUnderlyingType(type) is not { } underlyingType)
		{
			return true;
		}

		type = underlyingType;

		return underlyingType.IsGenericType;
	}

	private static bool CheckNullableAttribute(IEnumerable<CustomAttributeData> attributes, MemberInfo? declaringType, int index)
	{
		if (attributes.FirstOrDefault(data => data.AttributeType.FullName == NullableAttr) is { } nData)
		{
			var argument = nData.ConstructorArguments[0];

			if (argument.ArgumentType == typeof(byte[]))
			{
				var bytes = (IReadOnlyList<CustomAttributeTypedArgument>) argument.Value!;

				return (byte) bytes[index].Value! == CanBeNull;
			}

			return (byte) argument.Value! == CanBeNull;
		}

		for (; declaringType != null; declaringType = declaringType.DeclaringType)
		{
			if (declaringType.CustomAttributes.FirstOrDefault(data => data.AttributeType.FullName == NullableContextAttr) is { } ncData)
			{
				return (byte) ncData.ConstructorArguments[0].Value! == CanBeNull;
			}
		}

		return false;
	}
}