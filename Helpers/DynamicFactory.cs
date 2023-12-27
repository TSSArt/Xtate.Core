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

//namespace Xtate.Core;

//TODO:Delete
/*
public abstract class DynamicFactory
{
	public required IResourceLoader                          ResourceLoader         { private get; [UsedImplicitly] init; }
	public required LocalCache<Uri, DynamicAssembly>         Cache                  { private get; [UsedImplicitly] init; }
	public required Func<Stream, ValueTask<DynamicAssembly>> DynamicAssemblyFactory { private get; [UsedImplicitly] init; }
	public required ILogger<DynamicFactory>        Logger                 { private get; [UsedImplicitly] init; }

	private readonly bool _throwOnError;

	protected DynamicFactory(bool throwOnError) => _throwOnError = throwOnError;

	protected async ValueTask<ImmutableArray<IServiceModule>> GetServiceModules(Uri uri)
	{
		var dynamicAssembly = await LoadAssembly(uri).ConfigureAwait(false);

		if (dynamicAssembly is null)
		{
			return default;
		}

		return await GetCachedFactories(uri, dynamicAssembly).ConfigureAwait(false);
	}

	private async ValueTask<ImmutableArray<TFactory>> GetCachedFactories(Uri uri, DynamicAssembly dynamicAssembly)
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

	private static ImmutableArray<IServiceModule> CreateServiceModules(DynamicAssembly dynamicAssembly)
	{
		var attributes = dynamicAssembly.Assembly.GetCustomAttributes(typeof(ServiceModuleAttribute), inherit: false);

		if (attributes.Length == 0)
		{
			return [];
		}

		var serviceModules = ImmutableArray.CreateBuilder<IServiceModule>(attributes.Length);

		foreach (ServiceModuleAttribute attribute in attributes)
		{
			serviceModules.Add((IServiceModule) Activator.CreateInstance(attribute.ServiceModuleType!)!);
		}

		return serviceModules.MoveToImmutable();
	}

	private async ValueTask<DynamicAssembly?> LoadAssembly(Uri uri)
	{
		try
		{
			if (!Cache.TryGetValue(uri, out var dynamicAssembly))
			{
				var resource = await ResourceLoader.Request(uri).ConfigureAwait(false);

				Stream stream;
				await using (resource.ConfigureAwait(false))
				{
					stream = await resource.GetStream(true).ConfigureAwait(false);
				}

				dynamicAssembly = await DynamicAssemblyFactory(stream).ConfigureAwait(false);

				await Cache.SetValue(uri, dynamicAssembly, ValueOptions.ThreadSafe | ValueOptions.Dispose).ConfigureAwait(false);
			}

			return dynamicAssembly;
		}
		catch (Exception ex)
		{
			if (_throwOnError)
			{
				throw;
			}

			await Logger.Write(Level.Warning, @$"Error on loading assembly ({uri})", ex).ConfigureAwait(false);
		}

		return null;
	}
}
*/