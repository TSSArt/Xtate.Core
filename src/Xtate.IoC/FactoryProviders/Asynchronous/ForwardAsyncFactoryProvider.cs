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

internal class ForwardAsyncFactoryProvider<TImplementation, TService, TArg> : FactoryProviderBase where TImplementation : notnull where TService : class
{
	public static Delegate Delegate() => Infra.TypeInitHandle(() => Nested.DelegateField);

	protected override Delegate GetDelegateInstance() => GetService;

	private static Delegate GetDelegate()
	{
		if (typeof(TService).IsAssignableFrom(typeof(TImplementation)))
		{
			return ServiceType.TypeOf<TService>().IsGeneric ? Resolver.GetResolver : GetService;
		}

		throw new DependencyInjectionException(string.Format(Resources.Exception_TypeCantBeCastedTo, typeof(TImplementation), typeof(TService)));
	}

	private static async ValueTask<TService> GetService(IServiceProvider serviceProvider, TArg argument)
	{
		var entry = serviceProvider.GetImplementationEntry(TypeKey.ImplementationKeyFast<TImplementation, TArg>());

		Infra.NotNull(entry);

		var implementation = await entry.GetRequiredService<TImplementation, TArg>(argument).ConfigureAwait(false);

		return ConvertHelper<TImplementation, TService>.Convert(implementation);
	}

	private static class Nested
	{
		[SuppressMessage(category: "ReSharper", checkId: "StaticMemberInGenericType")]
		public static readonly Delegate DelegateField = GetDelegate();
	}

	private class Resolver : DelegateFactory
	{
		private static readonly Resolver Instance = new();

		public override Delegate? GetDelegate<TResolved, TResolvedArg>() => Infra.TypeInitHandle(() => Resolved<TResolved, TResolvedArg>.ResolvedDelegateField);

		public static DelegateFactory GetResolver() => Instance;
	}

	private static class Resolved<TResolved, TResolvedArg>
	{
		[SuppressMessage(category: "ReSharper", checkId: "StaticMemberInGenericType")]
		public static readonly Delegate? ResolvedDelegateField = GetResolvedDelegate();

		private static Delegate? GetResolvedDelegate()
		{
			if (!StubType.IsMatch(typeof(TService), typeof(TResolved)))
			{
				return default;
			}

			if (!StubType.IsMatch(typeof(TArg), typeof(TResolvedArg)))
			{
				return default;
			}

			if (ImplementationType.TypeOf<TImplementation>().TryConstruct(ServiceType.TypeOf<TResolved>(), out var implementationType))
			{
				return GetDelegateForType(typeof(ForwardAsyncFactoryProvider<,,>), implementationType.Type, typeof(TResolved), typeof(TResolvedArg));
			}

			throw new DependencyInjectionException(Res.Format(Resources.Exception_TypeCantBeConstructedBasedOnServiceType, typeof(TImplementation), typeof(TService)));
		}
	}
}