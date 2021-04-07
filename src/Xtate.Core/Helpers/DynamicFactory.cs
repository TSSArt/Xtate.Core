#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Core
{
	[PublicAPI]
	public abstract class DynamicFactory<TFactory> where TFactory : class
	{
		[SuppressMessage(category: "ReSharper", checkId: "StaticMemberInGenericType")]
		private static readonly object FactoryCacheKey = new();

		private readonly bool _throwOnError;

		protected DynamicFactory(bool throwOnError) => _throwOnError = throwOnError;

		protected async ValueTask<ImmutableArray<TFactory>> GetFactories(IFactoryContext factoryContext, Uri uri, CancellationToken token)
		{
			if (factoryContext is null) throw new ArgumentNullException(nameof(factoryContext));
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			var dynamicAssembly = await LoadAssembly(factoryContext, uri, token).ConfigureAwait(false);

			if (dynamicAssembly is null)
			{
				return default;
			}

			return await GetCachedFactories(factoryContext, uri, dynamicAssembly).ConfigureAwait(false);
		}

		private async ValueTask<ImmutableArray<TFactory>> GetCachedFactories(IFactoryContext factoryContext, Uri uri, DynamicAssembly dynamicAssembly)
		{
			var securityContext = factoryContext.SecurityContext;

			if (securityContext.TryGetValue(FactoryCacheKey, dynamicAssembly, out ImmutableArray<TFactory> factories))
			{
				return factories;
			}

			try
			{
				factories = await CreateFactories(dynamicAssembly).ConfigureAwait(false);

				await securityContext.SetValue(FactoryCacheKey, dynamicAssembly, factories, ValueOptions.ThreadSafe).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				if (_throwOnError)
				{
					throw;
				}

				await factoryContext.Log(LogLevel.Warning, Resources.Warning_ErrorOnCreationFactories, GetLogArguments(uri), ex, token: default).ConfigureAwait(false);
			}

			return factories;
		}

		private static async ValueTask<ImmutableArray<TFactory>> CreateFactories(DynamicAssembly dynamicAssembly)
		{
			var assembly = await dynamicAssembly.GetAssembly().ConfigureAwait(false);

			var attributes = assembly.GetCustomAttributes(typeof(FactoryAttribute), inherit: false);

			if (attributes.Length == 0)
			{
				return ImmutableArray<TFactory>.Empty;
			}

			var builder = ImmutableArray.CreateBuilder<TFactory>(attributes.Length);

			foreach (FactoryAttribute attribute in attributes)
			{
				var type = attribute.FactoryType;

				if (type is not null && typeof(TFactory).IsAssignableFrom(type))
				{
					var instance = Activator.CreateInstance(type);
					Infra.NotNull(instance);
					builder.Add((TFactory) instance);
				}
			}

			return builder.ToImmutable();
		}

		private async ValueTask<DynamicAssembly?> LoadAssembly(IFactoryContext factoryContext, Uri uri, CancellationToken token)
		{
			try
			{
				var securityContext = factoryContext.SecurityContext;

				if (!securityContext.TryGetValue(DynamicAssembly.AssemblyCacheKey, uri, out DynamicAssembly? dynamicAssembly))
				{
					var resource = await factoryContext.GetResource(uri, token).ConfigureAwait(false);
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

				await factoryContext.Log(LogLevel.Warning, Resources.Warning_ErrorOnLoadingAssembly, GetLogArguments(uri), ex, token).ConfigureAwait(false);
			}

			return null;
		}

		private static DataModelList GetLogArguments(Uri uri) => new() { { @"AssemblyUri", uri.ToString() } };
	}
}