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
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Empty = System.ValueTuple;

namespace Xtate.Core.IoC;

public abstract class FactoryFactoryProvider : FactoryProvider
{
	protected static IEnumerable<(MethodInfo MethodInfo, Type[]? TypeArguments, Type[]? MethodArguments)> EnumerateMethods<TArg>(Type implementationType, Type serviceType)
	{
		Infra.Requires(implementationType);
		Infra.Requires(serviceType);

		var allMethods = implementationType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

		foreach (var methodInfo in allMethods)
		{
			if (ValidParameters<TArg>(methodInfo))
			{
				var typeArguments = implementationType.IsGenericType ? implementationType.GetGenericArguments() : default;
				var methodArguments = methodInfo.IsGenericMethod ? methodInfo.GetGenericArguments() : default;
				
				if (TryMap(typeArguments, methodArguments, serviceType, GetReturnType(methodInfo)))
				{
					yield return (methodInfo, typeArguments, methodArguments);
				}
			}
		}
	}

	private static bool ValidParameters<TArg>(MethodInfo methodInfo)
	{
		foreach (var parameterInfo in methodInfo.GetParameters())
		{
			if (parameterInfo.ParameterType == typeof(CancellationToken))
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

	private static Type GetReturnType(MethodInfo methodInfo)
	{
		if (methodInfo.ReturnType is { IsGenericType: true } rt && rt.GetGenericTypeDefinition() == typeof(ValueTask<>))
		{
			return rt.GetGenericArguments()[0];
		}

		return methodInfo.ReturnType;
	}

	protected static Func<TImplementation, CancellationToken, TArg, ValueTask<TService>> GetMethodCaller<TImplementation, TService, TArg>()
	{
		var methods = new List<MethodInfo>();

		foreach (var entry in EnumerateMethods<TArg>(typeof(TImplementation), typeof(TService)))
		{
			if (entry.MethodArguments is null)
			{
				methods.Add(entry.MethodInfo);
			}
			else if (CanCreateInstance(entry.MethodArguments))
			{
				methods.Add(entry.MethodInfo.MakeGenericMethod(entry.MethodArguments));
			}
		}

		MethodInfo? methodInfo;
		try
		{
			methodInfo = methods.Count > 1
				? methods.Single(m => m.GetCustomAttribute<ObsoleteAttribute>(inherit: false) is null)
				: methods.Single();
		}
		catch (Exception ex)
		{
			throw new DependencyInjectionException(Res.Format(Resources.Exception_TypeDoesNotContainsMethodWithSignatureMethodCancellationToken, typeof(TImplementation), typeof(TService)), ex);
		}

		var implPrm = Expression.Parameter(typeof(TImplementation));
		var tokenPrm = Expression.Parameter(typeof(CancellationToken));
		var argPrm = Expression.Parameter(typeof(TArg));

		var parameters = methodInfo.GetParameters();
		var args = new Expression[parameters.Length];
		
		for (var i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].ParameterType == typeof(CancellationToken))
			{
				args[i] = tokenPrm;
			}
			else
			{
				args[i] = argPrm;

				var result = TupleHelper.TryBuild<TArg>(parameters[i].ParameterType, ref args[i]);

				Infra.Assert(result);
			}
		}

		Expression bodyExpression = Expression.Call(implPrm, methodInfo, tokenPrm);

		if (methodInfo.ReturnType == typeof(TService))
		{
			var constructorInfo = typeof(ValueTask<TService>).GetConstructor(new[] { typeof(TService) });
			
			Infra.NotNull(constructorInfo);
			
			bodyExpression = Expression.New(constructorInfo, bodyExpression);
		}

		return Expression.Lambda<Func<TImplementation, CancellationToken, TArg, ValueTask<TService>>>(bodyExpression, implPrm, tokenPrm, argPrm).Compile();
	}
}

public sealed class FactoryFactoryProvider<TImplementation, TService, TArg> : FactoryFactoryProvider where TImplementation : notnull
{
	private static readonly Func<TImplementation, CancellationToken, TArg, ValueTask<TService>> MethodCaller = GetMethodCaller<TImplementation, TService, TArg>();

	public static readonly Func<IServiceProvider, TArg, ValueTask<TService>> Delegate = GetService;

	public override Delegate GetDelegate() => Delegate;

	private static async ValueTask<TService> GetService(IServiceProvider serviceProvider, TArg argument)
	{
		if (serviceProvider.GetImplementationEntry(TypeKey.ImplementationKey<TImplementation, Empty>()) is not { } entry)
		{
			throw Infra.Fail<Exception>();
		}

		var implementation = await entry.GetRequiredService<TImplementation, Empty>(default).ConfigureAwait(false);

		return await MethodCaller(implementation, serviceProvider.DisposeToken, argument).ConfigureAwait(false);
	}
}
/*
public sealed class FactoryFactoryProvider<TImplementation, TService, TImplArg, TArg> : FactoryFactoryProvider where TImplementation : notnull
{
	private static readonly Func<TImplementation, CancellationToken, TArg, ValueTask<TService>> MethodCaller = GetMethodCaller<TImplementation, TService, TArg>();

	private static readonly Func<TArg, TImplArg> ArgumentConverter = TupleHelper.Convert<TArg, TImplArg>;

	public static readonly Func<IServiceProvider, TArg, ValueTask<TService>> Delegate = GetService;

	protected override Delegate GetDelegate() => Delegate;

	private static async ValueTask<TService> GetService(IServiceProvider serviceProvider, TArg argument)
	{
		var entry = serviceProvider.GetImplementationEntry(TypeKey.ImplementationKey<TImplementation, TImplArg>());

		if (entry is null)
		{
			throw ImplementationEntry.MissedServiceException<TService, TImplArg>();
		}

		var service = await entry.GetRequiredService<TImplementation, TImplArg>(ArgumentConverter(argument)).ConfigureAwait(false);

		return await MethodCaller(service, serviceProvider.DisposeToken, argument).ConfigureAwait(false);
	}
}*/