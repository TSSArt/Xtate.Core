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

namespace Xtate.Core;

//TODO: do something with it

public abstract class DynamicFactory
{
	[SuppressMessage(category: "ReSharper", checkId: "StaticMemberInGenericType")]
	private static readonly object FactoryCacheKey = new();

	private readonly bool _throwOnError;

	protected DynamicFactory(bool throwOnError) => _throwOnError = throwOnError;

	/*
	protected async ValueTask<ImmutableArray<TFactory>> GetFactories(ServiceLocator serviceLocator, Uri uri, CancellationToken token)
	{
		if (uri is null) throw new ArgumentNullException(nameof(uri));

		var dynamicAssembly = await LoadAssembly(serviceLocator, uri, token).ConfigureAwait(false);

		if (dynamicAssembly is null)
		{
			return default;
		}

		return await GetCachedFactories(serviceLocator, uri, dynamicAssembly).ConfigureAwait(false);
	}

	private async ValueTask<ImmutableArray<TFactory>> GetCachedFactories(ServiceLocator serviceLocator, Uri uri, DynamicAssembly dynamicAssembly)
	{
		var securityContext = serviceLocator.GetService<ISecurityContext>();
		var logEvent = serviceLocator.GetService<ILogEvent>();

		if (securityContext.TryGetValue(FactoryCacheKey, dynamicAssembly, out ImmutableArray<TFactory> factories))
		{
			return factories;
		}

		try
		{
			factories = await CreateModules(dynamicAssembly).ConfigureAwait(false);

			await securityContext.SetValue(FactoryCacheKey, dynamicAssembly, factories, ValueOptions.ThreadSafe).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			if (_throwOnError)
			{
				throw;
			}

			await logEvent.Log(LogLevel.Warning, Resources.Warning_ErrorOnCreationFactories, GetLogArguments(uri), ex, token: default).ConfigureAwait(false);
		}

		return factories;
	}

	private static async ValueTask<ImmutableArray<IServiceModule>> CreateModules(DynamicAssembly dynamicAssembly)
	{
		var assembly = await dynamicAssembly.GetAssembly().ConfigureAwait(false);

		var attributes = assembly.GetCustomAttributes(typeof(ServiceModuleAttribute), inherit: false);

		if (attributes.Length == 0)
		{
			return ImmutableArray<IServiceModule>.Empty;
		}

		var builder = ImmutableArray.CreateBuilder<IServiceModule>(attributes.Length);

		foreach (ServiceModuleAttribute attribute in attributes)
		{
			var type = attribute.ServiceModuleType;

			if (type is not null && typeof(IServiceModule).IsAssignableFrom(type))
			{
				var instance = Activator.CreateInstance(type);
				Infra.NotNull(instance);
				builder.Add((IServiceModule) instance);
			}
		}

		return builder.ToImmutable();
	}

	private async ValueTask<DynamicAssembly?> LoadAssembly(ServiceLocator serviceLocator, Uri uri, CancellationToken token)
	{
		try
		{
			var resourceLoaderService = serviceLocator.GetService<IResourceLoaderService>();
			var securityContext = serviceLocator.GetService<ISecurityContext>();

			if (!securityContext.TryGetValue(DynamicAssembly.AssemblyCacheKey, uri, out DynamicAssembly? dynamicAssembly))
			{
				var resource = await resourceLoaderService.GetResource(uri, token).ConfigureAwait(false);
				byte[] bytes;
				await using (resource.ConfigureAwait(false))
				{
					bytes = await resource.GetBytes(token).ConfigureAwait(false);
				}

				dynamicAssembly = new DynamicAssembly(securityContext.IoBoundTaskFactory, bytes);
				try
				{
					await dynamicAssembly.GetAssembly().ConfigureAwait(false);
				}
				catch
				{
					await dynamicAssembly.DisposeAsync().ConfigureAwait(false);

					throw;
				}

				await securityContext.SetValue(DynamicAssembly.AssemblyCacheKey, uri, dynamicAssembly, ValueOptions.ThreadSafe | ValueOptions.Dispose).ConfigureAwait(false);
			}

			return dynamicAssembly;
		}
		catch (Exception ex)
		{
			if (_throwOnError)
			{
				throw;
			}
			var logEvent = serviceLocator.GetService<ILogEvent>();
			await logEvent.Log(LogLevel.Warning, Resources.Warning_ErrorOnLoadingAssembly, GetLogArguments(uri), ex, token).ConfigureAwait(false);
		}

		return null;
	}
	*/
	private static DataModelList GetLogArguments(Uri uri) => new() { { @"AssemblyUri", uri.ToString() } };
}