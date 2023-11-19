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

internal sealed class FactoryAsyncFactoryProvider<TImplementation, TService, TArg> : FactoryProviderBase where TImplementation : notnull
{
	public static Delegate Delegate() => Infra.TypeInitHandle(() => Nested.DelegateField);

	protected override Delegate GetDelegateInstance() => GetService;

	private static Func<TImplementation, CancellationToken, TArg, ValueTask<TService>> GetMethodCaller()
	{
		var methodInfo = ImplementationType.TypeOf<TImplementation>().GetMethodInfo<TService, TArg>(false);

		var implPrm = Expression.Parameter(typeof(TImplementation));
		var tokenPrm = Expression.Parameter(typeof(CancellationToken));
		var argPrm = Expression.Parameter(typeof(TArg));

		var parameters = methodInfo.GetParameters();
		var args = parameters.Length is var length and > 0 ? new Expression[length] : Array.Empty<Expression>();

		for (var i = 0; i < parameters.Length; i ++)
		{
			if (parameters[i].ParameterType == typeof(CancellationToken))
			{
				args[i] = tokenPrm;
			}
			else
			{
				var parameter = TupleHelper.TryBuild<TArg>(parameters[i].ParameterType, argPrm);

				Infra.NotNull(parameter);

				args[i] = parameter;
			}
		}

		Expression bodyExpression = Expression.Call(implPrm, methodInfo, args);

		if (methodInfo.ReturnType == typeof(TService))
		{
			var constructorInfo = typeof(ValueTask<TService>).GetConstructor(new[] { typeof(TService) });

			Infra.NotNull(constructorInfo);

			bodyExpression = Expression.New(constructorInfo, bodyExpression);
		}

		return Expression.Lambda<Func<TImplementation, CancellationToken, TArg, ValueTask<TService>>>(bodyExpression, implPrm, tokenPrm, argPrm).Compile();
	}

	private static async ValueTask<TService> GetService(IServiceProvider serviceProvider, TArg argument)
	{
		var entry = serviceProvider.GetImplementationEntry(TypeKey.ImplementationKeyFast<TImplementation, Empty>());

		Infra.NotNull(entry);

		var implementation = await entry.GetRequiredService<TImplementation, Empty>(default).ConfigureAwait(false);

		return await NestedCaller.MethodCaller(implementation, serviceProvider.DisposeToken, argument).ConfigureAwait(false);
	}

	private static class Nested
	{
		public static readonly Delegate DelegateField = ServiceType.TypeOf<TService>().IsGeneric ? Resolver.GetResolver : new Func<IServiceProvider, TArg, ValueTask<TService>>(GetService);
	}

	private static class NestedCaller
	{
		public static readonly Func<TImplementation, CancellationToken, TArg, ValueTask<TService>> MethodCaller = GetMethodCaller();
	}

	private class Resolver : DelegateFactory
	{
		private static readonly Resolver Instance = new();

		public override Delegate? GetDelegate<TResolved, TResolvedArg>() => Infra.TypeInitHandle(() => Resolved<TResolved, TResolvedArg>.ResolvedDelegate);

		public static DelegateFactory GetResolver() => Instance;
	}

	private static class Resolved<TResolvedService, TResolvedArg>
	{
		[SuppressMessage(category: "ReSharper", checkId: "StaticMemberInGenericType")]
		public static readonly Delegate? ResolvedDelegate = GetResolvedDelegate();

		private static Delegate? GetResolvedDelegate()
		{
			if (!StubType.IsMatch(typeof(TService), typeof(TResolvedService)))
			{
				return default;
			}

			if (!StubType.IsMatch(typeof(TArg), typeof(TResolvedArg)))
			{
				return default;
			}

			var methodInfo = ImplementationType.TypeOf<TImplementation>().GetMethodInfo<TResolvedService, TResolvedArg>(false);
			var resolvedImplementationType = methodInfo.DeclaringType!;

			Infra.Assert(StubType.IsMatch(typeof(TImplementation), resolvedImplementationType));

			return GetDelegateForType(typeof(FactoryAsyncFactoryProvider<,,>), resolvedImplementationType, typeof(TResolvedService), typeof(TResolvedArg));
		}
	}
}