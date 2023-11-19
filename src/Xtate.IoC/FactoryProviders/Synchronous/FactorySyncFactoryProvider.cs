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

internal sealed class FactorySyncFactoryProvider<TImplementation, TService, TArg> : FactoryProviderBase where TImplementation : notnull
{
	public static Delegate Delegate() => Infra.TypeInitHandle(() => Nested.DelegateField);

	protected override Delegate GetDelegateInstance() => GetService;

	private static Func<TImplementation, TArg, TService> GetMethodCaller()
	{
		var methodInfo = ImplementationType.TypeOf<TImplementation>().GetMethodInfo<TService, TArg>(true);

		var implPrm = Expression.Parameter(typeof(TImplementation));
		var argPrm = Expression.Parameter(typeof(TArg));

		var parameters = methodInfo.GetParameters();
		var args = parameters.Length is var length and > 0 ? new Expression[length] : Array.Empty<Expression>();

		for (var i = 0; i < parameters.Length; i ++)
		{
			var parameter = TupleHelper.TryBuild<TArg>(parameters[i].ParameterType, argPrm);

			Infra.NotNull(parameter);

			args[i] = parameter;
		}

		Expression bodyExpression = Expression.Call(implPrm, methodInfo, args);

		return Expression.Lambda<Func<TImplementation, TArg, TService>>(bodyExpression, implPrm, argPrm).Compile();
	}

	private static TService GetService(IServiceProvider serviceProvider, TArg argument)
	{
		var entry = serviceProvider.GetImplementationEntry(TypeKey.ImplementationKeyFast<TImplementation, Empty>());

		Infra.NotNull(entry);

		var implementation = entry.GetRequiredServiceSync<TImplementation, Empty>(default);

		return NestedCaller.MethodCaller(implementation, argument);
	}

	private static class Nested
	{
		public static readonly Delegate DelegateField = ServiceType.TypeOf<TService>().IsGeneric ? Resolver.GetResolver : new Func<IServiceProvider, TArg, TService>(GetService);
	}

	private static class NestedCaller
	{
		public static readonly Func<TImplementation, TArg, TService> MethodCaller = GetMethodCaller();
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

			var methodInfo = ImplementationType.TypeOf<TImplementation>().GetMethodInfo<TResolvedService, TResolvedArg>(true);
			var resolvedImplementationType = methodInfo.DeclaringType!;

			Infra.Assert(StubType.IsMatch(typeof(TImplementation), resolvedImplementationType));

			return GetDelegateForType(typeof(FactorySyncFactoryProvider<,,>), resolvedImplementationType, typeof(TResolvedService), typeof(TResolvedArg));
		}
	}
}