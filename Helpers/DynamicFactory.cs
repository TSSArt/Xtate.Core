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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public abstract class DynamicFactory<TFactory> where TFactory : class
	{
		private static readonly Type CacheKey = typeof(DynamicFactory<TFactory>);

		private readonly bool _throwOnError;

		private ImmutableDictionary<Uri, Assembly>                      _assemblyCache = ImmutableDictionary<Uri, Assembly>.Empty;
		private ImmutableDictionary<Assembly, ImmutableArray<TFactory>> _factoryCache  = ImmutableDictionary<Assembly, ImmutableArray<TFactory>>.Empty;
		private ImmutableDictionary<object, Uri?>                       _uriCache      = ImmutableDictionary<object, Uri?>.Empty;

		protected DynamicFactory(bool throwOnError) => _throwOnError = throwOnError;

		protected async ValueTask<ImmutableArray<TFactory>> GetFactories(IFactoryContext factoryContext, object key, CancellationToken token)
		{
			if (factoryContext is null) throw new ArgumentNullException(nameof(factoryContext));
			if (key is null) throw new ArgumentNullException(nameof(key));

			var uri = GetCachedUri(key);

			if (uri is null)
			{
				return default;
			}

			var loadedAssembly = await LoadAssembly(factoryContext, uri, token).ConfigureAwait(false);

			var assembly = ResolveAssembly(uri, loadedAssembly);

			if (assembly is null)
			{
				return default;
			}

			return GetCachedFactories(assembly);
		}

		private Assembly? ResolveAssembly(Uri uri, Assembly? loadedAssembly)
		{
			var assemblyCache = _assemblyCache;

			assemblyCache.TryGetValue(uri, out var cachedAssembly);

			var assembly = loadedAssembly ?? cachedAssembly;

			if (assembly is { } && assembly != cachedAssembly)
			{
				_assemblyCache = assemblyCache.SetItem(uri, assembly);

				ClearFactoryCache(cachedAssembly);
			}

			return assembly;
		}

		private void ClearFactoryCache(Assembly assembly)
		{
			foreach (var pair in _assemblyCache)
			{
				if (pair.Value == assembly)
				{
					return;
				}
			}

			_factoryCache = _factoryCache.Remove(assembly);
		}

		private ImmutableArray<TFactory> GetCachedFactories(Assembly assembly)
		{
			var factoryCache = _factoryCache;

			if (factoryCache.TryGetValue(assembly, out ImmutableArray<TFactory> factories))
			{
				return factories;
			}

			try
			{
				factories = CreateFactories(assembly);
				_factoryCache = factoryCache.Add(assembly, factories);
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

		private static ImmutableArray<TFactory> CreateFactories(Assembly assembly)
		{
			var attributes = assembly.GetCustomAttributes(typeof(FactoryAttribute), inherit: false);

			if (attributes.Length == 0)
			{
				return ImmutableArray<TFactory>.Empty;
			}

			var builder = ImmutableArray.CreateBuilder<TFactory>(attributes.Length);

			foreach (FactoryAttribute attribute in attributes)
			{
				var type = attribute.FactoryType;

				if (type is { } && typeof(TFactory).IsAssignableFrom(type))
				{
					builder.Add((TFactory) Activator.CreateInstance(type));
				}
			}

			return builder.ToImmutable();
		}

		private Uri? GetCachedUri(object key)
		{
			var uriCache = _uriCache;

			if (!uriCache.TryGetValue(key, out var uri))
			{
				try
				{
					uri = KeyToUri(key);
					_uriCache = uriCache.Add(key, uri);
				}
				catch (Exception ex)
				{
					if (_throwOnError)
					{
						throw;
					}

					IgnoreException(ex);
				}
			}

			return uri;
		}

		protected virtual void IgnoreException(Exception ex) { }

		private async ValueTask<Assembly?> LoadAssembly(IFactoryContext factoryContext, Uri uri, CancellationToken token)
		{
			try
			{
				Assembly assembly;
				if (!(factoryContext[CacheKey] is Dictionary<Uri, Assembly> cache))
				{
					assembly = await LoadAssembly(factoryContext.ResourceLoaders, uri, token).ConfigureAwait(false);
					factoryContext[CacheKey] = new Dictionary<Uri, Assembly> { { uri, assembly } };
				}
				else if (!cache.TryGetValue(uri, out assembly))
				{
					assembly = await LoadAssembly(factoryContext.ResourceLoaders, uri, token).ConfigureAwait(false);
					cache.Add(uri, assembly);
				}

				return assembly;
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

		private static async ValueTask<Assembly> LoadAssembly(ImmutableArray<IResourceLoader> resourceLoaders, Uri uri, CancellationToken token)
		{
			if (!resourceLoaders.IsDefaultOrEmpty)
			{
				foreach (var resourceLoader in resourceLoaders)
				{
					if (resourceLoader.CanHandle(uri))
					{
						var resource = await resourceLoader.Request(uri, token).ConfigureAwait(false);

						return Assembly.Load(resource.GetBytes());
					}
				}
			}

			throw new ProcessorException(Resources.Exception_Cannot_find_ResourceLoader_to_load_external_resource);
		}
	}
}