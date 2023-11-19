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

[PublicAPI]
public static class ServiceCollectionExtensions
{
	private static bool IsRegistered(IServiceCollection services, TypeKey serviceKey)
	{
		Infra.Requires(services);

		foreach (var entry in services)
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
			_                      => throw Infra.UnexpectedValueException(sharedWithin)
		};

	private static void AddEntry(IServiceCollection services,
								 TypeKey serviceKey,
								 InstanceScope instanceScope,
								 Delegate factory)
	{
		Infra.Requires(services);

		services.Add(new ServiceEntry(serviceKey, instanceScope, factory));
	}

	public static bool IsRegistered<T, TArg>(this IServiceCollection services) => IsRegistered(services, TypeKey.ServiceKey<T, TArg>());

	public static bool IsRegistered<T>(this IServiceCollection services) => IsRegistered<T, Empty>(services);

	public static bool IsRegistered<T, TArg1, TArg2>(this IServiceCollection services) => IsRegistered<T, (TArg1, TArg2)>(services);

	public static void AddType<T, TArg>(this IServiceCollection services) where T : notnull =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, ImplementationAsyncFactoryProvider<T, TArg>.Delegate());

	public static void AddType<T>(this IServiceCollection services) where T : notnull => AddType<T, Empty>(services);

	public static void AddType<T, TArg1, TArg2>(this IServiceCollection services) where T : notnull => AddType<T, (TArg1, TArg2)>(services);

	public static void AddTypeSync<T, TArg>(this IServiceCollection services) where T : notnull =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, ImplementationSyncFactoryProvider<T, TArg>.Delegate());

	public static void AddTypeSync<T>(this IServiceCollection services) where T : notnull => AddTypeSync<T, Empty>(services);

	public static void AddTypeSync<T, TArg1, TArg2>(this IServiceCollection services) where T : notnull => AddTypeSync<T, (TArg1, TArg2)>(services);

	public static DecoratorImplementation<T, TArg> AddDecorator<T, TArg>(this IServiceCollection services) where T : notnull => new(services, InstanceScope.Transient, synchronous: false);

	public static DecoratorImplementation<T, Empty> AddDecorator<T>(this IServiceCollection services) where T : notnull => AddDecorator<T, Empty>(services);

	public static DecoratorImplementation<T, (TArg1, TArg2)> AddDecorator<T, TArg1, TArg2>(this IServiceCollection services) where T : notnull => AddDecorator<T, (TArg1, TArg2)>(services);

	public static DecoratorImplementation<T, TArg> AddDecoratorSync<T, TArg>(this IServiceCollection services) where T : notnull => new(services, InstanceScope.Transient, synchronous: true);

	public static DecoratorImplementation<T, Empty> AddDecoratorSync<T>(this IServiceCollection services) where T : notnull => AddDecoratorSync<T, Empty>(services);

	public static DecoratorImplementation<T, (TArg1, TArg2)> AddDecoratorSync<T, TArg1, TArg2>(this IServiceCollection services) where T : notnull => AddDecoratorSync<T, (TArg1, TArg2)>(services);

	public static ServiceImplementation<T, TArg> AddImplementation<T, TArg>(this IServiceCollection services) where T : notnull => new(services, InstanceScope.Transient, synchronous: false);

	public static ServiceImplementation<T, Empty> AddImplementation<T>(this IServiceCollection services) where T : notnull => AddImplementation<T, Empty>(services);

	public static ServiceImplementation<T, (TArg1, TArg2)> AddImplementation<T, TArg1, TArg2>(this IServiceCollection services) where T : notnull => AddImplementation<T, (TArg1, TArg2)>(services);

	public static ServiceImplementation<T, TArg> AddImplementationSync<T, TArg>(this IServiceCollection services) where T : notnull => new(services, InstanceScope.Transient, synchronous: true);

	public static ServiceImplementation<T, Empty> AddImplementationSync<T>(this IServiceCollection services) where T : notnull => AddImplementationSync<T, Empty>(services);

	public static ServiceImplementation<T, (TArg1, TArg2)> AddImplementationSync<T, TArg1, TArg2>(this IServiceCollection services) where T : notnull =>
		AddImplementationSync<T, (TArg1, TArg2)>(services);

	public static FactoryImplementation<T> AddFactory<T>(this IServiceCollection services) where T : notnull => new(services, InstanceScope.Transient, synchronous: false);

	public static FactoryImplementation<T> AddFactorySync<T>(this IServiceCollection services) where T : notnull => new(services, InstanceScope.Transient, synchronous: true);

	public static void AddSharedType<T, TArg>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), GetInstanceScope(sharedWithin), ImplementationAsyncFactoryProvider<T, TArg>.Delegate());

	public static void AddSharedType<T>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull => AddSharedType<T, Empty>(services, sharedWithin);

	public static void AddSharedType<T, TArg1, TArg2>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull => AddSharedType<T, (TArg1, TArg2)>(services, sharedWithin);

	public static void AddSharedTypeSync<T, TArg>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), GetInstanceScope(sharedWithin), ImplementationSyncFactoryProvider<T, TArg>.Delegate());

	public static void AddSharedTypeSync<T>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull => AddSharedTypeSync<T, Empty>(services, sharedWithin);

	public static void AddSharedTypeSync<T, TArg1, TArg2>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		AddSharedTypeSync<T, (TArg1, TArg2)>(services, sharedWithin);

	public static DecoratorImplementation<T, TArg> AddSharedDecorator<T, TArg>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		new(services, GetInstanceScope(sharedWithin), synchronous: false);

	public static DecoratorImplementation<T, Empty> AddSharedDecorator<T>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		AddSharedDecorator<T, Empty>(services, sharedWithin);

	public static DecoratorImplementation<T, (TArg1, TArg2)> AddSharedDecorator<T, TArg1, TArg2>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		AddSharedDecorator<T, (TArg1, TArg2)>(services, sharedWithin);

	public static DecoratorImplementation<T, TArg> AddSharedDecoratorSync<T, TArg>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		new(services, GetInstanceScope(sharedWithin), synchronous: true);

	public static DecoratorImplementation<T, Empty> AddSharedDecoratorSync<T>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		AddSharedDecoratorSync<T, Empty>(services, sharedWithin);

	public static DecoratorImplementation<T, (TArg1, TArg2)> AddSharedDecoratorSync<T, TArg1, TArg2>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		AddSharedDecoratorSync<T, (TArg1, TArg2)>(services, sharedWithin);

	public static ServiceImplementation<T, TArg> AddSharedImplementation<T, TArg>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		new(services, GetInstanceScope(sharedWithin), synchronous: false);

	public static ServiceImplementation<T, Empty> AddSharedImplementation<T>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		AddSharedImplementation<T, Empty>(services, sharedWithin);

	public static ServiceImplementation<T, (TArg1, TArg2)> AddSharedImplementation<T, TArg1, TArg2>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		AddSharedImplementation<T, (TArg1, TArg2)>(services, sharedWithin);

	public static ServiceImplementation<T, TArg> AddSharedImplementationSync<T, TArg>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		new(services, GetInstanceScope(sharedWithin), synchronous: true);

	public static ServiceImplementation<T, Empty> AddSharedImplementationSync<T>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		AddSharedImplementationSync<T, Empty>(services, sharedWithin);

	public static ServiceImplementation<T, (TArg1, TArg2)> AddSharedImplementationSync<T, TArg1, TArg2>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		AddSharedImplementationSync<T, (TArg1, TArg2)>(services, sharedWithin);

	public static FactoryImplementation<T> AddSharedFactory<T>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		new(services, GetInstanceScope(sharedWithin), synchronous: false);

	public static FactoryImplementation<T> AddSharedFactorySync<T>(this IServiceCollection services, SharedWithin sharedWithin) where T : notnull =>
		new(services, GetInstanceScope(sharedWithin), synchronous: true);

	public static void AddShared<T>(this IServiceCollection services, SharedWithin sharedWithin, Func<IServiceProvider, ValueTask<T>> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, Empty>(), GetInstanceScope(sharedWithin), new Func<IServiceProvider, Empty, ValueTask<T>>((sp, _) => factory(sp)));

	public static void AddShared<T>(this IServiceCollection services, SharedWithin sharedWithin, Func<IServiceProvider, T> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, Empty>(), GetInstanceScope(sharedWithin), new Func<IServiceProvider, Empty, T>((sp, _) => factory(sp)));

	public static void AddShared<T, TArg>(this IServiceCollection services, SharedWithin sharedWithin, Func<IServiceProvider, TArg, ValueTask<T>> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), GetInstanceScope(sharedWithin), factory);

	public static void AddShared<T, TArg>(this IServiceCollection services, SharedWithin sharedWithin, Func<IServiceProvider, TArg, T> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), GetInstanceScope(sharedWithin), factory);

	public static void AddShared<T, TArg1, TArg2>(this IServiceCollection services, SharedWithin sharedWithin, Func<IServiceProvider, TArg1, TArg2, ValueTask<T>> factory) =>
		AddEntry(
			services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), GetInstanceScope(sharedWithin),
			new Func<IServiceProvider, (TArg1, TArg2), ValueTask<T>>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

	public static void AddShared<T, TArg1, TArg2>(this IServiceCollection services, SharedWithin sharedWithin, Func<IServiceProvider, TArg1, TArg2, T> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), GetInstanceScope(sharedWithin), new Func<IServiceProvider, (TArg1, TArg2), T>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

	public static void AddSharedDecorator<T>(this IServiceCollection services, SharedWithin sharedWithin, Func<IServiceProvider, T, ValueTask<T>> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, Empty>(), GetInstanceScope(sharedWithin), new Func<IServiceProvider, T, Empty, ValueTask<T>>((sp, decorated, _) => factory(sp, decorated)));

	public static void AddSharedDecorator<T>(this IServiceCollection services, SharedWithin sharedWithin, Func<IServiceProvider, T, T> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, Empty>(), GetInstanceScope(sharedWithin), new Func<IServiceProvider, T, Empty, T>((sp, decorated, _) => factory(sp, decorated)));

	public static void AddSharedDecorator<T, TArg>(this IServiceCollection services, SharedWithin sharedWithin, Func<IServiceProvider, T, TArg, ValueTask<T>> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), GetInstanceScope(sharedWithin), factory);

	public static void AddSharedDecorator<T, TArg>(this IServiceCollection services, SharedWithin sharedWithin, Func<IServiceProvider, T, TArg, T> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), GetInstanceScope(sharedWithin), factory);

	public static void AddSharedDecorator<T, TArg1, TArg2>(this IServiceCollection services, SharedWithin sharedWithin, Func<IServiceProvider, T, TArg1, TArg2, ValueTask<T>> factory) =>
		AddEntry(
			services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), GetInstanceScope(sharedWithin),
			new Func<IServiceProvider, T, (TArg1, TArg2), ValueTask<T>>((sp, decorated, arg) => factory(sp, decorated, arg.Item1, arg.Item2)));

	public static void AddSharedDecorator<T, TArg1, TArg2>(this IServiceCollection services, SharedWithin sharedWithin, Func<IServiceProvider, T, TArg1, TArg2, T> factory) =>
		AddEntry(
			services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), GetInstanceScope(sharedWithin),
			new Func<IServiceProvider, T, (TArg1, TArg2), T>((sp, decorated, arg) => factory(sp, decorated, arg.Item1, arg.Item2)));

	public static void AddTransient<T>(this IServiceCollection services, Func<IServiceProvider, ValueTask<T>> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, new Func<IServiceProvider, Empty, ValueTask<T>>((sp, _) => factory(sp)));

	public static void AddTransient<T>(this IServiceCollection services, Func<IServiceProvider, T> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, new Func<IServiceProvider, Empty, T>((sp, _) => factory(sp)));

	public static void AddTransient<T, TArg>(this IServiceCollection services, Func<IServiceProvider, TArg, ValueTask<T>> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

	public static void AddTransient<T, TArg>(this IServiceCollection services, Func<IServiceProvider, TArg, T> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

	public static void AddTransient<T, TArg1, TArg2>(this IServiceCollection services, Func<IServiceProvider, TArg1, TArg2, ValueTask<T>> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Transient, new Func<IServiceProvider, (TArg1, TArg2), ValueTask<T>>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

	public static void AddTransient<T, TArg1, TArg2>(this IServiceCollection services, Func<IServiceProvider, TArg1, TArg2, T> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Transient, new Func<IServiceProvider, (TArg1, TArg2), T>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

	public static void AddTransientDecorator<T>(this IServiceCollection services, Func<IServiceProvider, T, ValueTask<T>> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, new Func<IServiceProvider, T, Empty, ValueTask<T>>((sp, decorated, _) => factory(sp, decorated)));

	public static void AddTransientDecorator<T>(this IServiceCollection services, Func<IServiceProvider, T, T> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, new Func<IServiceProvider, T, Empty, T>((sp, decorated, _) => factory(sp, decorated)));

	public static void AddTransientDecorator<T, TArg>(this IServiceCollection services, Func<IServiceProvider, T, TArg, ValueTask<T>> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

	public static void AddTransientDecorator<T, TArg>(this IServiceCollection services, Func<IServiceProvider, T, TArg, T> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

	public static void AddTransientDecorator<T, TArg1, TArg2>(this IServiceCollection services, Func<IServiceProvider, T, TArg1, TArg2, ValueTask<T>> factory) =>
		AddEntry(
			services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Transient,
			new Func<IServiceProvider, T, (TArg1, TArg2), ValueTask<T>>((sp, decorated, arg) => factory(sp, decorated, arg.Item1, arg.Item2)));

	public static void AddTransientDecorator<T, TArg1, TArg2>(this IServiceCollection services, Func<IServiceProvider, T, TArg1, TArg2, T> factory) =>
		AddEntry(
			services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Transient,
			new Func<IServiceProvider, T, (TArg1, TArg2), T>((sp, decorated, arg) => factory(sp, decorated, arg.Item1, arg.Item2)));

	public static void AddForwarding<T>(this IServiceCollection services, Func<IServiceProvider, ValueTask<T>> evaluator) =>
		AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Forwarding, new Func<IServiceProvider, Empty, ValueTask<T>>((sp, _) => evaluator(sp)));

	public static void AddForwarding<T>(this IServiceCollection services, Func<IServiceProvider, T> evaluator) =>
		AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Forwarding, new Func<IServiceProvider, Empty, T>((sp, _) => evaluator(sp)));

	public static void AddForwarding<T, TArg>(this IServiceCollection services, Func<IServiceProvider, TArg, ValueTask<T>> evaluator) =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Forwarding, evaluator);

	public static void AddForwarding<T, TArg>(this IServiceCollection services, Func<IServiceProvider, TArg, T> evaluator) =>
		AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Forwarding, evaluator);

	public static void AddForwarding<T, TArg1, TArg2>(this IServiceCollection services, Func<IServiceProvider, TArg1, TArg2, ValueTask<T>> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Forwarding, new Func<IServiceProvider, (TArg1, TArg2), ValueTask<T>>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

	public static void AddForwarding<T, TArg1, TArg2>(this IServiceCollection services, Func<IServiceProvider, TArg1, TArg2, T> factory) =>
		AddEntry(services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Forwarding, new Func<IServiceProvider, (TArg1, TArg2), T>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));
}