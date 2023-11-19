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

internal sealed class ClassAsyncFactoryProvider : ClassFactoryProvider
{
	private static readonly MethodInfo GetOptionalAsyncService;
	private static readonly MethodInfo GetRequiredAsyncService;

	static ClassAsyncFactoryProvider()
	{
		GetOptionalAsyncService = GetMethodInfo<ClassAsyncFactoryProvider>(nameof(GetOptionalServiceWrapper));
		GetRequiredAsyncService = GetMethodInfo<ClassAsyncFactoryProvider>(nameof(GetRequiredServiceWrapper));
	}

	public ClassAsyncFactoryProvider(Type implementationType) : base(implementationType) { }

	protected override MethodInfo GetOptionalService => GetOptionalAsyncService;

	protected override MethodInfo GetRequiredService => GetRequiredAsyncService;

	private static async ValueTask<object> GetRequiredServiceWrapper<T>(IServiceProvider serviceProvider) where T : notnull => await serviceProvider.GetRequiredService<T>().ConfigureAwait(false);

	private static async ValueTask<object?> GetOptionalServiceWrapper<T>(IServiceProvider serviceProvider) => await serviceProvider.GetOptionalService<T>().ConfigureAwait(false);

	private async ValueTask FillParameters<TArg>(object?[] args, IServiceProvider serviceProvider, TArg? arg)
	{
		for (var i = 0; i < Parameters.Length; i ++)
		{
			if (TupleHelper.TryMatch(Parameters[i].MemberType, ref arg, out var value))
			{
				args[i] = value;
			}
			else if (Parameters[i].AsyncValueGetter is { } asyncValueGetter)
			{
				args[i] = await asyncValueGetter(serviceProvider).ConfigureAwait(false);
			}
			else
			{
				args[i] = Parameters[i].SyncValueGetter!(serviceProvider);
			}
		}
	}

	private async ValueTask SetRequiredMembers<TArg>(object service, IServiceProvider serviceProvider, TArg? arg)
	{
		for (var i = 0; i < RequiredMembers.Length; i ++)
		{
			var setter = RequiredMembers[i].MemberSetter;
			Infra.NotNull(setter);

			if (TupleHelper.TryMatch(RequiredMembers[i].MemberType, ref arg, out var value))
			{
				setter(service, value);
			}
			else if (RequiredMembers[i].AsyncValueGetter is { } asyncValueGetter)
			{
				setter(service, await asyncValueGetter(serviceProvider).ConfigureAwait(false));
			}
			else
			{
				setter(service, RequiredMembers[i].SyncValueGetter!(serviceProvider));
			}
		}
	}

	public ValueTask<TService> GetDecoratorService<TService, TArg>(IServiceProvider serviceProvider, TService? service, TArg? arg) =>
		GetService<TService, (TService?, TArg?)>(serviceProvider, (service, arg));

	public async ValueTask<TService> GetService<TService, TArg>(IServiceProvider serviceProvider, TArg? arg)
	{
		var args = RentArray();

		try
		{
			if (Parameters.Length > 0)
			{
				await FillParameters(args, serviceProvider, arg).ConfigureAwait(false);
			}

			var service = ((Func<object?[], TService>) Delegate)(args);

			if (RequiredMembers.Length > 0)
			{
				await SetRequiredMembers(service!, serviceProvider, arg).ConfigureAwait(false);
			}

			return service;
		}
		catch (Exception ex)
		{
			throw GetFactoryException(ex);
		}
		finally
		{
			ReturnArray(args);
		}
	}
}

internal static class ClassAsyncFactoryProvider<TImplementation, TService>
{
	public static Delegate GetServiceDelegate<TArg>() => Infra.TypeInitHandle(() => Nested.ProviderField).GetService<TService, TArg>;

	public static Delegate GetDecoratorServiceDelegate<TArg>() => Infra.TypeInitHandle(() => Nested.ProviderField).GetDecoratorService<TService, TArg>;

	private static class Nested
	{
		public static readonly ClassAsyncFactoryProvider ProviderField = new(typeof(TImplementation));
	}
}