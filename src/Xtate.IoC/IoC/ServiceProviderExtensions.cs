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
[SuppressMessage(category: "Design", checkId: "CA1062:Validate arguments of public methods", Justification = "No need to check")]
public static class ServiceProviderExtensions
{
	public static ValueTask<T> GetRequiredService<T>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredService<T, Empty>(default);

	public static ValueTask<T> GetRequiredService<T, TArg>(this IServiceProvider serviceProvider, TArg arg) where T : notnull =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetRequiredService<T, TArg>(arg) ?? ImplementationEntry.MissedServiceExceptionTask<T, TArg>();

	public static ValueTask<T> GetRequiredService<T, TArg1, TArg2>(this IServiceProvider serviceProvider, TArg1 arg1, TArg2 arg2) where T : notnull =>
		serviceProvider.GetRequiredService<T, (TArg1, TArg2)>((arg1, arg2));

	public static ValueTask<T?> GetOptionalService<T>(this IServiceProvider serviceProvider) => serviceProvider.GetOptionalService<T, Empty>(default);

	public static ValueTask<T?> GetOptionalService<T, TArg>(this IServiceProvider serviceProvider, TArg arg) =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetOptionalService<T, TArg>(arg) ?? default;

	public static ValueTask<T?> GetOptionalService<T, TArg1, TArg2>(this IServiceProvider serviceProvider, TArg1 arg1, TArg2 arg2) =>
		serviceProvider.GetOptionalService<T, (TArg1, TArg2)>((arg1, arg2));

	public static T GetRequiredServiceSync<T>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredServiceSync<T, Empty>(default);

	public static T GetRequiredServiceSync<T, TArg>(this IServiceProvider serviceProvider, TArg arg) where T : notnull =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>()) is { } entry
			? entry.GetRequiredServiceSync<T, TArg>(arg)
			: throw ImplementationEntry.MissedServiceException<T, TArg>();

	public static T GetRequiredServiceSync<T, TArg1, TArg2>(this IServiceProvider serviceProvider, TArg1 arg1, TArg2 arg2) where T : notnull =>
		serviceProvider.GetRequiredServiceSync<T, (TArg1, TArg2)>((arg1, arg2));

	public static T? GetOptionalServiceSync<T>(this IServiceProvider serviceProvider) => serviceProvider.GetOptionalServiceSync<T, Empty>(default);

	public static T? GetOptionalServiceSync<T, TArg>(this IServiceProvider serviceProvider, TArg arg) =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>()) is { } entry ? entry.GetOptionalServiceSync<T, TArg>(arg) : default;

	public static T? GetOptionalServiceSync<T, TArg1, TArg2>(this IServiceProvider serviceProvider, TArg1 arg1, TArg2 arg2) => serviceProvider.GetOptionalServiceSync<T, (TArg1, TArg2)>((arg1, arg2));

	public static IAsyncEnumerable<T> GetServices<T>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServices<T, Empty>(default);

	public static IAsyncEnumerable<T> GetServices<T, TArg>(this IServiceProvider serviceProvider, TArg arg) where T : notnull =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServices<T, TArg>(arg) ?? AsyncEnumerable.Empty<T>();

	public static IAsyncEnumerable<T> GetServices<T, TArg1, TArg2>(this IServiceProvider serviceProvider, TArg1 arg1, TArg2 arg2) where T : notnull =>
		serviceProvider.GetServices<T, (TArg1, TArg2)>((arg1, arg2));

	public static IEnumerable<T> GetServicesSync<T>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServicesSync<T, Empty>(default);

	public static IEnumerable<T> GetServicesSync<T, TArg>(this IServiceProvider serviceProvider, TArg arg) where T : notnull =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServicesSync<T, TArg>(arg) ?? Array.Empty<T>();

	public static IEnumerable<T> GetServicesSync<T, TArg1, TArg2>(this IServiceProvider serviceProvider, TArg1 arg1, TArg2 arg2) where T : notnull =>
		serviceProvider.GetServicesSync<T, (TArg1, TArg2)>((arg1, arg2));

	private static TDelegate GetServicesFactoryBase<T, TArg, TDelegate>(this IServiceProvider serviceProvider, TDelegate emptyDelegate) where T : notnull where TDelegate : Delegate =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServicesDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

	public static Func<TArg, IAsyncEnumerable<T>> GetServicesFactory<T, TArg>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetServicesFactoryBase<T, TArg, Func<TArg, IAsyncEnumerable<T>>>(static _ => AsyncEnumerable.Empty<T>());

	public static Func<TArg1, TArg2, IAsyncEnumerable<T>> GetServicesFactory<T, TArg1, TArg2>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetServicesFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, IAsyncEnumerable<T>>>(static (_, _) => AsyncEnumerable.Empty<T>());

	private static TDelegate GetServicesSyncFactoryBase<T, TArg, TDelegate>(this IServiceProvider serviceProvider, TDelegate emptyDelegate) where T : notnull where TDelegate : Delegate =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServicesSyncDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

	public static Func<TArg, IEnumerable<T>> GetServicesSyncFactory<T, TArg>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetServicesSyncFactoryBase<T, TArg, Func<TArg, IEnumerable<T>>>(static _ => Array.Empty<T>());

	public static Func<TArg1, TArg2, IEnumerable<T>> GetServicesSyncFactory<T, TArg1, TArg2>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetServicesSyncFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, IEnumerable<T>>>(static (_, _) => Array.Empty<T>());

	private static TDelegate GetRequiredFactoryBase<T, TArg, TDelegate>(this IServiceProvider serviceProvider) where T : notnull where TDelegate : Delegate =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetRequiredServiceDelegate<T, TArg, TDelegate>() ?? throw ImplementationEntry.MissedServiceException<T, TArg>();

	public static Func<ValueTask<T>> GetRequiredFactory<T>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredFactoryBase<T, Empty, Func<ValueTask<T>>>();

	public static Func<TArg, ValueTask<T>> GetRequiredFactory<T, TArg>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetRequiredFactoryBase<T, TArg, Func<TArg, ValueTask<T>>>();

	public static Func<TArg1, TArg2, ValueTask<T>> GetRequiredFactory<T, TArg1, TArg2>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetRequiredFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, ValueTask<T>>>();

	private static TDelegate GetOptionalFactoryBase<T, TArg, TDelegate>(this IServiceProvider serviceProvider, TDelegate emptyDelegate) where TDelegate : Delegate =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetOptionalServiceDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

	public static Func<ValueTask<T?>> GetOptionalFactory<T>(this IServiceProvider serviceProvider) => serviceProvider.GetOptionalFactoryBase<T, Empty, Func<ValueTask<T?>>>(static () => default);

	public static Func<TArg, ValueTask<T?>> GetOptionalFactory<T, TArg>(this IServiceProvider serviceProvider) =>
		serviceProvider.GetOptionalFactoryBase<T, TArg, Func<TArg, ValueTask<T?>>>(static _ => default);

	public static Func<TArg1, TArg2, ValueTask<T?>> GetOptionalFactory<T, TArg1, TArg2>(this IServiceProvider serviceProvider) =>
		serviceProvider.GetOptionalFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, ValueTask<T?>>>(static (_, _) => default);

	private static TDelegate GetRequiredSyncFactoryBase<T, TArg, TDelegate>(this IServiceProvider serviceProvider) where T : notnull where TDelegate : Delegate =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetRequiredServiceSyncDelegate<T, TArg, TDelegate>() ?? throw ImplementationEntry.MissedServiceException<T, TArg>();

	public static Func<T> GetRequiredSyncFactory<T>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredSyncFactoryBase<T, Empty, Func<T>>();

	public static Func<TArg, T> GetRequiredSyncFactory<T, TArg>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredSyncFactoryBase<T, TArg, Func<TArg, T>>();

	public static Func<TArg1, TArg2, T> GetRequiredSyncFactory<T, TArg1, TArg2>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetRequiredSyncFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, T>>();

	private static TDelegate GetOptionalSyncFactoryBase<T, TArg, TDelegate>(this IServiceProvider serviceProvider, TDelegate emptyDelegate) where TDelegate : Delegate =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetOptionalServiceSyncDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

	public static Func<T?> GetOptionalSyncFactory<T>(this IServiceProvider serviceProvider) => serviceProvider.GetOptionalSyncFactoryBase<T, Empty, Func<T?>>(static () => default);

	public static Func<TArg, T?> GetOptionalSyncFactory<T, TArg>(this IServiceProvider serviceProvider) => serviceProvider.GetOptionalSyncFactoryBase<T, TArg, Func<TArg, T?>>(static _ => default);

	public static Func<TArg1, TArg2, T?> GetOptionalSyncFactory<T, TArg1, TArg2>(this IServiceProvider serviceProvider) =>
		serviceProvider.GetOptionalSyncFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, T?>>(static (_, _) => default);
}