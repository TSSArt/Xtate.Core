#region Copyright © 2019-2020 Sergii Artemenko

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
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate
{
	internal static class DynamicFactoryGlobal
	{
		public static readonly object AssemblyCacheKey = new();
	}

	[PublicAPI]
	public abstract class DynamicFactory<TFactory> where TFactory : class
	{
		private static readonly object FactoryCacheKey = new();

		private readonly bool _throwOnError;

		protected DynamicFactory(bool throwOnError) => _throwOnError = throwOnError;

		protected async ValueTask<ImmutableArray<TFactory>> GetFactories(IFactoryContext factoryContext, object key, CancellationToken token)
		{
			if (factoryContext is null) throw new ArgumentNullException(nameof(factoryContext));
			if (key is null) throw new ArgumentNullException(nameof(key));

			var uri = GetUri(key);

			if (uri is null)
			{
				return default;
			}

			var dynamicAssembly = await LoadAssembly(factoryContext, uri, token).ConfigureAwait(false);

			if (dynamicAssembly is null)
			{
				return default;
			}

			return await GetCachedFactories(factoryContext.SecurityContext, dynamicAssembly).ConfigureAwait(false);
		}

		private async ValueTask<ImmutableArray<TFactory>> GetCachedFactories(SecurityContext securityContext, DynamicAssembly dynamicAssembly)
		{
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

				IgnoreException(ex);
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
					Infrastructure.NotNull(instance);
					builder.Add((TFactory) instance);
				}
			}

			return builder.ToImmutable();
		}

		private Uri? GetUri(object key)
		{
			try
			{
				return KeyToUri(key);
			}
			catch (Exception ex)
			{
				if (_throwOnError)
				{
					throw;
				}

				IgnoreException(ex);
			}

			return default;
		}

		protected virtual void IgnoreException(Exception ex) { }

		private async ValueTask<DynamicAssembly?> LoadAssembly(IFactoryContext factoryContext, Uri uri, CancellationToken token)
		{
			try
			{
				var securityContext = factoryContext.SecurityContext;

				if (!securityContext.TryGetValue(DynamicFactoryGlobal.AssemblyCacheKey, uri, out DynamicAssembly? dynamicAssembly))
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

					await securityContext.SetValue(DynamicFactoryGlobal.AssemblyCacheKey, uri, dynamicAssembly, ValueOptions.ThreadSafe | ValueOptions.Dispose).ConfigureAwait(false);
				}

				return dynamicAssembly;
			}
			catch (Exception ex)
			{
				if (_throwOnError)
				{
					throw;
				}

				IgnoreException(ex);
			}

			return null;
		}

		protected abstract Uri? KeyToUri(object key);
	}
}