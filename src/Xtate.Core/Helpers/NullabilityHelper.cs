using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xtate.Core
{
	internal static class NullabilityHelper
	{
		private const int    CanBeNull           = 2;
		private const string NullableAttr        = @"System.Runtime.CompilerServices.NullableAttribute";
		private const string NullableContextAttr = @"System.Runtime.CompilerServices.NullableContextAttribute";

		public static bool IsNullable(ParameterInfo parameter, string path = "")
		{
			if (parameter is null) throw new ArgumentNullException(nameof(parameter));
			if (path is null) throw new ArgumentNullException(nameof(path));

			var index = 0;

			if (FindType(parameter.ParameterType, path, ref index, level: 0) is not { } type)
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

			return CheckNullableAttribute(parameter.CustomAttributes, parameter.Member, index);
		}

		private static Type? FindType(Type type, string path, ref int index, int level)
		{
			if (level == path.Length)
			{
				return type;
			}

			if (IsBytePresent(ref type))
			{
				index++;
			}

			if (type.IsGenericType)
			{
				var pos = '0';
				foreach (var argType in type.GetGenericArguments())
				{
					if (pos++ == path[level])
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
				index++;
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
}
