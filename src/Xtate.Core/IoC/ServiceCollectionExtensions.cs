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
using System.Threading.Tasks;
using Empty = System.ValueTuple;

namespace Xtate.Core.IoC;

[PublicAPI]
public static class ServiceCollectionExtensions
{
	private static bool IsRegistered(IServiceCollection serviceCollection, TypeKey serviceKey)
	{
		Infra.Requires(serviceCollection);

		foreach (var entry in serviceCollection)
		{
			if (entry.Key == serviceKey)
			{
				return true;
			}
		}

		return false;
	}

	private static InstanceScope GetInstanceScope(SharedWithin sharedWithin) =>
		sharedWithin switch
		{
			SharedWithin.Container => InstanceScope.Singleton,
			SharedWithin.Scope     => InstanceScope.Scoped,
			_                      => Infra.Unexpected<InstanceScope>(sharedWithin)
		};

	private static void AddEntry(IServiceCollection serviceCollection,
								 TypeKey serviceKey,
								 InstanceScope instanceScope,
								 Delegate factory)
	{
		Infra.Requires(serviceCollection);

		serviceCollection.Add(new ServiceEntry(serviceKey, instanceScope, factory));
	}

	public static bool IsRegistered<T, TArg>(this IServiceCollection serviceCollection) => IsRegistered(serviceCollection, TypeKey.ServiceKey<T, TArg>());

	public static bool IsRegistered<T>(this IServiceCollection serviceCollection) => IsRegistered<T, Empty>(serviceCollection);

	public static bool IsRegistered<T, TArg1, TArg2>(this IServiceCollection serviceCollection) => IsRegistered<T, (TArg1, TArg2)>(serviceCollection);
	

	public static void AddType<T, TArg>(this IServiceCollection serviceCollection) where T : notnull =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, ImplementationAsyncFactoryProvider<T, TArg>.Delegate);

	public static void AddType<T>(this IServiceCollection serviceCollection) where T : notnull => AddType<T, Empty>(serviceCollection);
	
	public static void AddType<T, TArg1, TArg2>(this IServiceCollection serviceCollection) where T : notnull => AddType<T, (TArg1, TArg2)>(serviceCollection);
	

	public static void AddTypeSync<T, TArg>(this IServiceCollection serviceCollection) where T : notnull =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, ImplementationSyncFactoryProvider<T, TArg>.Delegate);

	public static void AddTypeSync<T>(this IServiceCollection serviceCollection) where T : notnull => AddTypeSync<T, Empty>(serviceCollection);
	
	public static void AddTypeSync<T, TArg1, TArg2>(this IServiceCollection serviceCollection) where T : notnull => AddTypeSync<T, (TArg1, TArg2)>(serviceCollection);
	
	
	public static DecoratorImplementation<T, TArg> AddDecorator<T, TArg>(this IServiceCollection serviceCollection) where T : notnull => new(serviceCollection, InstanceScope.Transient);

	public static DecoratorImplementation<T, Empty> AddDecorator<T>(this IServiceCollection serviceCollection) where T : notnull => AddDecorator<T, Empty>(serviceCollection);

	public static DecoratorImplementation<T, (TArg1, TArg2)> AddDecorator<T, TArg1, TArg2>(this IServiceCollection serviceCollection) where T : notnull => AddDecorator<T, (TArg1, TArg2)>(serviceCollection);
	

	public static ServiceImplementation<T, TArg> AddImplementation<T, TArg>(this IServiceCollection serviceCollection) where T : notnull => new(serviceCollection, InstanceScope.Transient);

	public static ServiceImplementation<T, Empty> AddImplementation<T>(this IServiceCollection serviceCollection) where T : notnull => AddImplementation<T, Empty>(serviceCollection);

	public static ServiceImplementation<T, (TArg1, TArg2)> AddImplementation<T, TArg1, TArg2>(this IServiceCollection serviceCollection) where T : notnull => AddImplementation<T, (TArg1, TArg2)>(serviceCollection);


	public static FactoryImplementation<T, TArg> AddFactory<T, TArg>(this IServiceCollection serviceCollection) where T : notnull => new(serviceCollection, InstanceScope.Transient);

	public static FactoryImplementation<T, Empty> AddFactory<T>(this IServiceCollection serviceCollection) where T : notnull => AddFactory<T, Empty>(serviceCollection);

	public static FactoryImplementation<T, (TArg1, TArg2)> AddFactory<T, TArg1, TArg2>(this IServiceCollection serviceCollection) where T : notnull => AddFactory<T, (TArg1, TArg2)>(serviceCollection);


	public static void AddSharedType<T, TArg>(this IServiceCollection serviceCollection, SharedWithin sharedWithin) where T : notnull =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), GetInstanceScope(sharedWithin), ImplementationAsyncFactoryProvider<T, TArg>.Delegate);

	public static void AddSharedType<T>(this IServiceCollection serviceCollection, SharedWithin sharedWithin) where T : notnull => AddSharedType<T, Empty>(serviceCollection, sharedWithin);

	public static void AddSharedType<T, TArg1, TArg2>(this IServiceCollection serviceCollection, SharedWithin sharedWithin) where T : notnull =>
		AddSharedType<T, (TArg1, TArg2)>(serviceCollection, sharedWithin);


	public static DecoratorImplementation<T, TArg> AddSharedDecorator<T, TArg>(this IServiceCollection serviceCollection, SharedWithin sharedWithin) where T : notnull =>
		new(serviceCollection, GetInstanceScope(sharedWithin));

	public static DecoratorImplementation<T, Empty> AddSharedDecorator<T>(this IServiceCollection serviceCollection, SharedWithin sharedWithin) where T : notnull =>
		AddSharedDecorator<T, Empty>(serviceCollection, sharedWithin);

	public static DecoratorImplementation<T, (TArg1, TArg2)> AddSharedDecorator<T, TArg1, TArg2>(this IServiceCollection serviceCollection, SharedWithin sharedWithin) where T : notnull =>
		AddSharedDecorator<T, (TArg1, TArg2)>(serviceCollection, sharedWithin);


	public static ServiceImplementation<T, TArg> AddSharedImplementation<T, TArg>(this IServiceCollection serviceCollection, SharedWithin sharedWithin) where T : notnull =>
		new(serviceCollection, GetInstanceScope(sharedWithin));

	public static ServiceImplementation<T, Empty> AddSharedImplementation<T>(this IServiceCollection serviceCollection, SharedWithin sharedWithin) where T : notnull =>
		AddSharedImplementation<T, Empty>(serviceCollection, sharedWithin);

	public static ServiceImplementation<T, (TArg1, TArg2)> AddSharedImplementation<T, TArg1, TArg2>(this IServiceCollection serviceCollection, SharedWithin sharedWithin) where T : notnull =>
		AddSharedImplementation<T, (TArg1, TArg2)>(serviceCollection, sharedWithin);


	public static FactoryImplementation<T, TArg> AddSharedFactory<T, TArg>(this IServiceCollection serviceCollection, SharedWithin sharedWithin) where T : notnull =>
		new(serviceCollection, GetInstanceScope(sharedWithin));

	public static FactoryImplementation<T, Empty> AddSharedFactory<T>(this IServiceCollection serviceCollection, SharedWithin sharedWithin) where T : notnull =>
		AddSharedFactory<T, Empty>(serviceCollection, sharedWithin);

	public static FactoryImplementation<T, (TArg1, TArg2)> AddSharedFactory<T, TArg1, TArg2>(this IServiceCollection serviceCollection, SharedWithin sharedWithin) where T : notnull =>
		AddSharedFactory<T, (TArg1, TArg2)>(serviceCollection, sharedWithin);


	public static void AddSingleton<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, ValueTask<T>> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Singleton, factory);

	public static void AddSingleton<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, T> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Singleton, factory);

	public static void AddSingleton<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, TArg, ValueTask<T>> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Singleton, factory);

	public static void AddSingleton<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, TArg, T> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Singleton, factory);

	public static void AddSingletonDecorator<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, T, ValueTask<T>> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Singleton, factory);

	public static void AddSingletonDecorator<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, T, T> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Singleton, factory);

	public static void AddSingletonDecorator<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, T, TArg, ValueTask<T>> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Singleton, factory);

	public static void AddSingletonDecorator<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, T, TArg, T> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Singleton, factory);

	public static void AddScoped<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, ValueTask<T>> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Scoped, factory);

	public static void AddScoped<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, T> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Scoped, factory);

	public static void AddScoped<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, TArg, ValueTask<T>> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Scoped, factory);

	public static void AddScoped<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, TArg, T> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Scoped, factory);

	public static void AddScopedDecorator<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, T, ValueTask<T>> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Scoped, factory);

	public static void AddScopedDecorator<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, T, T> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Scoped, factory);

	public static void AddScopedDecorator<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, T, TArg, ValueTask<T>> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Scoped, factory);

	public static void AddScopedDecorator<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, T, TArg, T> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Scoped, factory);

	public static void AddTransient<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, ValueTask<T>> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, factory);

	public static void AddTransient<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, T> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, factory);

	public static void AddTransient<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, TArg, ValueTask<T>> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

	public static void AddTransient<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, TArg, T> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

	public static void AddTransientDecorator<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, T, ValueTask<T>> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, factory);

	public static void AddTransientDecorator<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, T, T> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, factory);

	public static void AddTransientDecorator<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, T, TArg, ValueTask<T>> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

	public static void AddTransientDecorator<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, T, TArg, T> factory) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

	public static void AddForwarding<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, ValueTask<T>> evaluator) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Forwarding, evaluator);

	public static void AddForwarding<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, T> evaluator) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Forwarding, evaluator);

	public static void AddForwarding<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, TArg, ValueTask<T>> evaluator) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Forwarding, evaluator);

	public static void AddForwarding<T, TArg>(this IServiceCollection serviceCollection, Func<IServiceProvider, TArg, T> evaluator) =>
		AddEntry(serviceCollection, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Forwarding, evaluator);
}