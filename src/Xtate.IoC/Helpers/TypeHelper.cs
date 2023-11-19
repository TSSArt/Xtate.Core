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

using System.Text;

namespace Xtate.IoC;

internal static class TypeHelper
{
	public static Type MakeGenericTypeExt(this Type type, Type arg) => type.MakeGenericType(arg);

	public static Type MakeGenericTypeExt(this Type type, Type arg1, Type arg2) => type.MakeGenericType(arg1, arg2);

	public static Type MakeGenericTypeExt(this Type type,
										  Type arg1,
										  Type arg2,
										  Type arg3) =>
		type.MakeGenericType(arg1, arg2, arg3);

	public static Type MakeGenericTypeExt(this Type type, Type[] arg1, Type arg2)
	{
		var args = new Type[arg1.Length + 1];
		Array.Copy(arg1, args, arg1.Length);
		args[arg1.Length] = arg2;

		return type.MakeGenericType(args);
	}

	public static T CreateInstance<T>(this Type type) => (T) Activator.CreateInstance(type)!;

	private static string? TryGetSimpleName(Type type)
	{
		switch (Type.GetTypeCode(type))
		{
			case TypeCode.Boolean: return @"bool";
			case TypeCode.Byte:    return @"byte";
			case TypeCode.Char:    return @"char";
			case TypeCode.Decimal: return @"decimal";
			case TypeCode.Double:  return @"double";
			case TypeCode.Int16:   return @"short";
			case TypeCode.Int32:   return @"int";
			case TypeCode.Int64:   return @"long";
			case TypeCode.SByte:   return @"sbyte";
			case TypeCode.Single:  return @"float";
			case TypeCode.String:  return @"string";
			case TypeCode.UInt16:  return @"ushort";
			case TypeCode.UInt32:  return @"uint";
			case TypeCode.UInt64:  return @"ulong";
		}

		if (type == typeof(object))
		{
			return @"object";
		}

		if (type == typeof(void))
		{
			return @"void";
		}

		return default;
	}

	public static string FriendlyName(this Type type)
	{
		if (TryGetSimpleName(type) is { } name)
		{
			return name;
		}

		return type.IsGenericType ? AppendGenericType(new StringBuilder(), type).ToString() : type.Name;
	}

	private static void AppendFriendlyName(StringBuilder sb, Type type)
	{
		if (TryGetSimpleName(type) is { } name)
		{
			sb.Append(name);
		}
		else if (!type.IsGenericType)
		{
			sb.Append(type.Name);
		}
		else
		{
			AppendGenericType(sb, type);
		}
	}

	private static StringBuilder AppendGenericType(StringBuilder sb, Type type)
	{
		if (IsTuple(type))
		{
			AppendTupleArgs(sb, prefix: '(', type);

			return sb.Append(')');
		}

		var name = type.Name;

		sb.Append(name, startIndex: 0, name.IndexOf('`'));

		var first = true;
		foreach (var t in type.GetGenericArguments())
		{
			AppendFriendlyName(sb.Append(first ? '<' : ','), t);
			first = false;
		}

		return sb.Append('>');
	}

	private static void AppendTupleArgs(StringBuilder sb, char prefix, Type type)
	{
		var genericArguments = type.GetGenericArguments();

		for (var i = 0; i < genericArguments.Length; i ++)
		{
			if (i == 7)
			{
				AppendTupleArgs(sb, prefix: ',', genericArguments[7]);

				return;
			}

			sb.Append(prefix);
			AppendFriendlyName(sb, genericArguments[i]);
			prefix = ',';
		}
	}

	private static bool IsTuple(Type type)
	{
		var typeDef = type.GetGenericTypeDefinition();

		return typeDef == typeof(ValueTuple<,>) ||
			   typeDef == typeof(ValueTuple<,,>) ||
			   typeDef == typeof(ValueTuple<,,>) ||
			   typeDef == typeof(ValueTuple<,,,>) ||
			   typeDef == typeof(ValueTuple<,,,,>) ||
			   typeDef == typeof(ValueTuple<,,,,,>) ||
			   typeDef == typeof(ValueTuple<,,,,,,>) ||
			   typeDef == typeof(ValueTuple<,,,,,,,>) ||
			   typeDef == typeof(ValueTuple<,,,,,,,>);
	}
}