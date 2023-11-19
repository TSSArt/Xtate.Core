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
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Xtate.Core.IoC;

public abstract class ClassFactoryProvider : FactoryProvider
{
	private static readonly MethodInfo GetOptionalServiceMethodInfo;
	private static readonly MethodInfo GetRequiredServiceMethodInfo;
	private static readonly MethodInfo GetServicesMethodInfo;
	private static readonly MethodInfo GetOptionalFactoryMethodInfo;
	private static readonly MethodInfo GetRequiredFactoryMethodInfo;
	private static readonly MethodInfo GetOptionalSyncFactoryMethodInfo;
	private static readonly MethodInfo GetRequiredSyncFactoryMethodInfo;
	private static readonly MethodInfo GetServicesFactoryArgMethodInfo;
	private static readonly MethodInfo GetOptionalFactoryArgMethodInfo;
	private static readonly MethodInfo GetRequiredFactoryArgMethodInfo;
	private static readonly MethodInfo GetOptionalSyncFactoryArgMethodInfo;
	private static readonly MethodInfo GetRequiredSyncFactoryArgMethodInfo;

	private static readonly Dictionary<Type, int> _funcMap = new()
															 {
																 { typeof(Func<,,>), 2 },
																 { typeof(Func<,,,>), 3 },
																 { typeof(Func<,,,,>), 4 },
																 { typeof(Func<,,,,,>), 5 },
																 { typeof(Func<,,,,,,>), 6 },
																 { typeof(Func<,,,,,,,>), 7 },
																 { typeof(Func<,,,,,,,,>), 8 },
																 { typeof(Func<,,,,,,,,,>), 9 },
																 { typeof(Func<,,,,,,,,,,>), 10 },
																 { typeof(Func<,,,,,,,,,,,>), 11 },
																 { typeof(Func<,,,,,,,,,,,,>), 12 },
																 { typeof(Func<,,,,,,,,,,,,,>), 13 },
																 { typeof(Func<,,,,,,,,,,,,,,>), 14 },
																 { typeof(Func<,,,,,,,,,,,,,,,>), 15 },
																 { typeof(Func<,,,,,,,,,,,,,,,,>), 16 }
															 };

	private static readonly Type[] _numToValueTupleMap =
	{
		typeof(ValueTuple),
		typeof(ValueTuple<>),
		typeof(ValueTuple<,>),
		typeof(ValueTuple<,,>),
		typeof(ValueTuple<,,,>),
		typeof(ValueTuple<,,,,>),
		typeof(ValueTuple<,,,,,>),
		typeof(ValueTuple<,,,,,,>),
		typeof(ValueTuple<,,,,,,,>)
	};

	private static readonly string[] _path1Map = { @"0", @"1", @"2", @"3", @"4", @"5", @"6", @"7", @"8", @"9", @":", @";", @"<", @"=", @">", @"?", @"@" };
	private static readonly string[] _path2Map = { @"00", @"10", @"20", @"30", @"40", @"50", @"60", @"70", @"80", @"90", @":0", @";0", @"<0", @"=0", @">0", @"?0", @"@0" };

	static ClassFactoryProvider()
	{
		foreach (var methodInfo in typeof(ClassFactoryProvider).GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
		{
			switch (methodInfo.Name)
			{
				case nameof(GetOptionalServiceWrapper):
					GetOptionalServiceMethodInfo = methodInfo;
					break;

				case nameof(GetRequiredServiceWrapper):
					GetRequiredServiceMethodInfo = methodInfo;
					break;

				case nameof(GetServicesWrapper):
					GetServicesMethodInfo = methodInfo;
					break;

				case nameof(GetOptionalFactoryWrapper):
					GetOptionalFactoryMethodInfo = methodInfo;
					break;

				case nameof(GetRequiredFactoryWrapper):
					GetRequiredFactoryMethodInfo = methodInfo;
					break;

				case nameof(GetOptionalSyncFactoryWrapper):
					GetOptionalSyncFactoryMethodInfo = methodInfo;
					break;

				case nameof(GetRequiredSyncFactoryWrapper):
					GetRequiredSyncFactoryMethodInfo = methodInfo;
					break;

				case nameof(GetServicesFactoryArgWrapper):
					GetServicesFactoryArgMethodInfo = methodInfo;
					break;

				case nameof(GetOptionalFactoryArgWrapper):
					GetOptionalFactoryArgMethodInfo = methodInfo;
					break;

				case nameof(GetRequiredFactoryArgWrapper):
					GetRequiredFactoryArgMethodInfo = methodInfo;
					break;

				case nameof(GetOptionalSyncFactoryArgWrapper):
					GetOptionalSyncFactoryArgMethodInfo = methodInfo;
					break;

				case nameof(GetRequiredSyncFactoryArgWrapper):
					GetRequiredSyncFactoryArgMethodInfo = methodInfo;
					break;
			}
		}

		Infra.NotNull(GetOptionalServiceMethodInfo);
		Infra.NotNull(GetRequiredServiceMethodInfo);
		Infra.NotNull(GetServicesMethodInfo);
		Infra.NotNull(GetOptionalFactoryMethodInfo);
		Infra.NotNull(GetRequiredFactoryMethodInfo);
		Infra.NotNull(GetOptionalSyncFactoryMethodInfo);
		Infra.NotNull(GetRequiredSyncFactoryMethodInfo);
		Infra.NotNull(GetServicesFactoryArgMethodInfo);
		Infra.NotNull(GetOptionalFactoryArgMethodInfo);
		Infra.NotNull(GetRequiredFactoryArgMethodInfo);
		Infra.NotNull(GetOptionalSyncFactoryArgMethodInfo);
		Infra.NotNull(GetRequiredSyncFactoryArgMethodInfo);
	}

	protected static Parameter CreateParameter(ParameterInfo parameterInfo)
	{
		var method = MakeGetterMethod(parameterInfo);

		if (method.ReturnType == typeof(object))
		{
			return new Parameter(parameterInfo.ParameterType, (Func<IServiceProvider, object>) method.CreateDelegate(typeof(Func<IServiceProvider, object>)));
		}

		return new Parameter(parameterInfo.ParameterType, (Func<IServiceProvider, ValueTask<object>>) method.CreateDelegate(typeof(Func<IServiceProvider, ValueTask<object>>)));
	}

	private static MethodInfo MakeGetterMethod(ParameterInfo parameterInfo)
	{
		Infra.Requires(parameterInfo);

		if (IsIAsyncEnumerable(parameterInfo.ParameterType) is { } serviceType1)
		{
			return GetServicesMethodInfo.MakeGenericMethod(serviceType1);
		}

		if (IsFunc(parameterInfo.ParameterType) is { } resultType)
		{
			if (IsValueTask(resultType) is { } serviceType2)
			{
				return NullabilityHelper.IsNullable(parameterInfo, path: @"00")
					? GetOptionalFactoryMethodInfo.MakeGenericMethod(serviceType2)
					: GetRequiredFactoryMethodInfo.MakeGenericMethod(serviceType2);
			}

			return NullabilityHelper.IsNullable(parameterInfo, path: @"0")
				? GetOptionalSyncFactoryMethodInfo.MakeGenericMethod(resultType)
				: GetRequiredSyncFactoryMethodInfo.MakeGenericMethod(resultType);
		}

		if (IsFunc2(parameterInfo.ParameterType) is { Type: { } resultType2, ArgType: { } argType })
		{
			if (IsIAsyncEnumerable(resultType2) is { } serviceType3)
			{
				return GetServicesFactoryArgMethodInfo.MakeGenericMethod(serviceType3, argType);
			}

			if (IsValueTask(resultType2) is { } serviceType4)
			{
				return NullabilityHelper.IsNullable(parameterInfo, @"10")
					? GetOptionalFactoryArgMethodInfo.MakeGenericMethod(serviceType4, argType)
					: GetRequiredFactoryArgMethodInfo.MakeGenericMethod(serviceType4, argType);
			}

			return NullabilityHelper.IsNullable(parameterInfo, path: @"1")
				? GetOptionalSyncFactoryArgMethodInfo.MakeGenericMethod(resultType2, argType)
				: GetRequiredSyncFactoryArgMethodInfo.MakeGenericMethod(resultType2, argType);
		}

		return NullabilityHelper.IsNullable(parameterInfo)
			? GetOptionalServiceMethodInfo.MakeGenericMethod(parameterInfo.ParameterType)
			: GetRequiredServiceMethodInfo.MakeGenericMethod(parameterInfo.ParameterType);
	}

	private static async ValueTask<object> GetRequiredServiceWrapper<T>(IServiceProvider serviceProvider) where T : notnull => await serviceProvider.GetRequiredService<T>().ConfigureAwait(false);

	private static async ValueTask<object?> GetOptionalServiceWrapper<T>(IServiceProvider serviceProvider) => await serviceProvider.GetOptionalService<T>().ConfigureAwait(false);

	private static object GetServicesWrapper<T>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServices<T>();

	private static object GetRequiredFactoryWrapper<T>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredFactory<T>();

	private static object GetOptionalFactoryWrapper<T>(IServiceProvider serviceProvider) => serviceProvider.GetOptionalFactory<T>();

	private static object GetRequiredSyncFactoryWrapper<T>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredSyncFactory<T>();

	private static object GetOptionalSyncFactoryWrapper<T>(IServiceProvider serviceProvider) => serviceProvider.GetOptionalSyncFactory<T>();

	private static object GetRequiredFactoryArgWrapper<T, TArg>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredFactory<T, TArg>();

	private static object GetOptionalFactoryArgWrapper<T, TArg>(IServiceProvider serviceProvider) => serviceProvider.GetOptionalFactory<T, TArg>();

	private static object GetRequiredSyncFactoryArgWrapper<T, TArg>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredSyncFactory<T, TArg>();

	private static object GetOptionalSyncFactoryArgWrapper<T, TArg>(IServiceProvider serviceProvider) => serviceProvider.GetOptionalSyncFactory<T, TArg>();

	private static object GetServicesFactoryArgWrapper<T, TArg>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServicesFactory<T, TArg>();

	private static Type? IsIAsyncEnumerable(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>) ? type.GetGenericArguments()[0] : default;

	private static Type? IsFunc(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<>) ? type.GetGenericArguments()[0] : default;

	private static (Type? Type, Type? ArgType) IsFunc2(Type type) =>
		type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<,>) && type.GetGenericArguments() is { } args ? (args[1], args[0]) : default;

	[Obsolete]
	private static (Type? Type, Type? ArgType, int TypeIndex) IsFunc2tmp(Type type)
	{
		if (!type.IsGenericType)
		{
			return default;
		}

		var genericTypeDefinition = type.GetGenericTypeDefinition();
		var args = type.GetGenericArguments();

		if (genericTypeDefinition == typeof(Func<,>))
		{
			return (args[1], args[0], 1);
		}

		if (_funcMap.TryGetValue(genericTypeDefinition, out var numArgs))
		{
			return (args[numArgs], GetValueTupleType(args, 0, numArgs), numArgs);
		}

		return default;
	}

	private static Type GetValueTupleType(Type[] types, int start, int length)
	{
		if (length <= 7)
		{
			var shortTypes = new Type[length];
			Array.Copy(types, start, shortTypes, 0, length);
			
			return _numToValueTupleMap[length].MakeGenericType(shortTypes);
		}

		var fullTypes = new Type[8];
		Array.Copy(types, start, fullTypes, 0, 7);
		fullTypes[7] = GetValueTupleType(types, start + 7, length - 7);

		return _numToValueTupleMap[8].MakeGenericType(fullTypes);
	}

	private static Type? IsValueTask(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>) ? type.GetGenericArguments()[0] : default;

	protected readonly struct Parameter
	{
		public Parameter(Type parameterType, Func<IServiceProvider, ValueTask<object>> valueGetter)
		{
			ParameterType = parameterType;
			ValueGetterAsync = valueGetter;
			ValueGetterSync = default;
		}

		public Parameter(Type parameterType, Func<IServiceProvider, object> valueGetter)
		{
			ParameterType = parameterType;
			ValueGetterAsync = default;
			ValueGetterSync = valueGetter;
		}

		public readonly Type ParameterType;

		public readonly Func<IServiceProvider, ValueTask<object>>? ValueGetterAsync;

		public readonly Func<IServiceProvider, object>? ValueGetterSync;
	}
}

public abstract class ClassFactoryProvider<TImplementation, TService> : ClassFactoryProvider
{
	private static readonly Func<object?[], TService> _delegate;

	private static readonly Parameter[] _parameters;

	static ClassFactoryProvider()
	{
		var factory = GetFactory(typeof(TImplementation));
		var parameters = factory.GetParameters();
		_delegate = CreateDelegate(factory, parameters);
		_parameters = Array.ConvertAll(parameters, CreateParameter);
	}

	protected static bool IsResolvedType() => ImplementationType.TypeOf<TImplementation>().IsResolvedType();

	protected static bool IsMatch(Type type) => ImplementationType.TypeOf<TImplementation>().IsMatch(type);

	//protected static bool CanUseSyncCall<TArg>() => _parameters.All(p => TupleHelper.IsMatch<TArg>(p.ParameterType) || p.ValueGetterSync is not null) && NoAsyncInitialization();

	//private static bool NoAsyncInitialization() => !AsyncInitialization.IsAsyncInitializationRequired<TImplementation>();

	private static Func<object?[], TService> CreateDelegate(ConstructorInfo factory, ParameterInfo[] parameters)
	{
		var arrayParameter = Expression.Parameter(typeof(object[]));

		var args = new Expression[parameters.Length];
		for (var i = 0; i < args.Length; i ++)
		{
			var itemExpression = Expression.ArrayIndex(arrayParameter, Expression.Constant(i));
			args[i] = Expression.Convert(itemExpression, parameters[i].ParameterType);
		}

		var serviceExpression = Expression.New(factory, args);

		return Expression.Lambda<Func<object?[], TService>>(serviceExpression, arrayParameter).Compile();
	}

	private static ConstructorInfo GetFactory(Type type)
	{
		try
		{
			var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			return constructors.Length > 1
				? constructors.Single(c => c.GetCustomAttribute<ObsoleteAttribute>(false) is null)
				: constructors.Single();
		}
		catch (Exception ex)
		{
			throw new DependencyInjectionException(Res.Format(Resources.Exception_CantFindFactoryForType, type), ex);
		}
	}

	protected static TService GetServiceSync<TArg>(IServiceProvider serviceProvider, TArg arg) => GetServiceSync<TArg, ValueTask>(serviceProvider, arg);

	protected static ValueTask<TService> GetServiceAsync<TArg>(IServiceProvider serviceProvider, TArg arg) => GetServiceAsync<TArg, ValueTask>(serviceProvider, arg);

	protected static TService GetServiceSync<TArg1, TArg2>(IServiceProvider serviceProvider, TArg1? arg1 = default, TArg2? arg2 = default)
	{
		var args = _parameters.Length > 0 ? ArrayPool<object?>.Shared.Rent(_parameters.Length) : Array.Empty<object>();

		try
		{
			for (var i = 0; i < _parameters.Length; i ++)
			{
				var parameter = _parameters[i];

				if (TupleHelper.TryMatch(parameter.ParameterType, ref arg1, out var value1))
				{
					args[i] = value1;
				}
				else if (TupleHelper.TryMatch(parameter.ParameterType, ref arg2, out var value2))
				{
					args[i] = value2;
				}
				else if (parameter.ValueGetterSync is { } syncMethod)
				{
					args[i] = syncMethod(serviceProvider);
				}
				else if (parameter.ValueGetterAsync is not null)
				{
					throw new DependencyInjectionException(Resources.Exception_ServiceCantBeInstantiatedSinceAtLeastOneParameterRequiredAsynchronousCall);
				}
				else
				{
					args[i] = null;
				}
			}

			return _delegate(args);
		}
		catch (Exception ex)
		{
			throw new DependencyInjectionException(Res.Format(Resources.Exception_FactoryOfRaisedException, typeof(TImplementation)), ex);
		}
		finally
		{
			if (args.Length > 0)
			{
				Array.Clear(args, index: 0, _parameters.Length);
				ArrayPool<object?>.Shared.Return(args);
			}
		}
	}	
	
	protected static async ValueTask<TService> GetServiceAsync<TArg1, TArg2>(IServiceProvider serviceProvider, TArg1? arg1 = default, TArg2? arg2 = default)
	{
		var args = _parameters.Length > 0 ? ArrayPool<object?>.Shared.Rent(_parameters.Length) : Array.Empty<object>();

		try
		{
			for (var i = 0; i < _parameters.Length; i ++)
			{
				var parameter = _parameters[i];

				if (TupleHelper.TryMatch(parameter.ParameterType, ref arg1, out var value1))
				{
					args[i] = value1;
				}
				else if (TupleHelper.TryMatch(parameter.ParameterType, ref arg2, out var value2))
				{
					args[i] = value2;
				}
				else if (parameter.ValueGetterSync is { } syncMethod)
				{
					args[i] = syncMethod(serviceProvider);
				}
				else if (parameter.ValueGetterAsync is { } asyncMethod)
				{
					args[i] = await asyncMethod(serviceProvider).ConfigureAwait(false);
				}
				else
				{
					args[i] = null;
				}
			}

			return _delegate(args);
		}
		catch (Exception ex)
		{
			throw new DependencyInjectionException(Res.Format(Resources.Exception_FactoryOfRaisedException, typeof(TImplementation)), ex);
		}
		finally
		{
			if (args.Length > 0)
			{
				Array.Clear(args, index: 0, _parameters.Length);
				ArrayPool<object?>.Shared.Return(args);
			}
		}
	}
}