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

namespace Xtate.IoC;

internal static class StubType
{
	private static readonly Cache<Type, IStub> Instances = new(4);

	private static readonly Any Any = new();

	private static bool IsStubType(Type type) => typeof(IStub).IsAssignableFrom(type);

	private static bool IsStubMatch(Type stubType, Type type) => GetStubInstance(stubType).IsMatch(type);

	private static IStub GetStubInstance(Type stubType)
	{
		if (stubType == typeof(Any))
		{
			return Any;
		}

		return Instances.TryGetValue(stubType, out var stub)
			? stub
			: Instances.GetOrAdd(stubType, stubType.CreateInstance<IStub>());
	}

	public static bool IsResolvedType(Type type)
	{
		Infra.Requires(type);

		if (type.HasElementType)
		{
			return IsResolvedType(type.GetElementType()!);
		}

		if (type.IsGenericParameter || IsStubType(type))
		{
			return false;
		}

		if (!type.IsGenericType)
		{
			return true;
		}

		foreach (var arg in type.GetGenericArguments())
		{
			if (!IsResolvedType(arg))
			{
				return false;
			}
		}

		return true;
	}

	public static bool IsMatch(Type type1, Type type2) => TryMap(typesToMap1: default, typesToMap2: default, type1, type2);

	public static bool TryMap(Type[]? typesToMap1,
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

		if (arg1 is not null && IsStubType(arg1))
		{
			return arg2 is not null && !IsStubType(arg2) && IsStubMatch(arg1, arg2);
		}

		if (arg2 is not null && IsStubType(arg2))
		{
			return arg1 is not null && !IsStubType(arg1) && IsStubMatch(arg2, arg1);
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

	public static bool TryMap(Type[]? typesToMap1,
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