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
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate
{
	[PublicAPI]
	public sealed class ResxResourceLoaderFactory : IResourceLoaderFactory
	{
		private readonly Activator              _activator = new();
		public static    IResourceLoaderFactory Instance { get; } = new ResxResourceLoaderFactory();

	#region Interface IResourceLoaderFactory

		public ValueTask<IResourceLoaderFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, Uri uri, CancellationToken token)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			return CanHandle(uri) ? new ValueTask<IResourceLoaderFactoryActivator?>(_activator) : default;
		}

	#endregion

		private static bool CanHandle(Uri uri) => uri.IsAbsoluteUri && (uri.Scheme == @"res" || uri.Scheme == @"resx");

		private sealed class Activator : IResourceLoaderFactoryActivator
		{
		#region Interface IResourceLoaderFactoryActivator

			public ValueTask<IResourceLoader> CreateResourceLoader(IFactoryContext factoryContext, CancellationToken token) => new(new ResourceLoader(factoryContext));

		#endregion
		}

		private sealed class ResourceLoader : IResourceLoader
		{
			private readonly IFactoryContext _factoryContext;

			public ResourceLoader(IFactoryContext factoryContext) => _factoryContext = factoryContext;

		#region Interface IResourceLoader

			public async ValueTask<Resource> Request(Uri uri, NameValueCollection? headers, CancellationToken token)
			{
				if (uri is null) throw new ArgumentNullException(nameof(uri));

				Infra.Assert(CanHandle(uri));

				return new Resource(await GetResourceStreamAsync(uri, token).ConfigureAwait(false));
			}

		#endregion

			private Task<Stream> GetResourceStreamAsync(Uri uri, CancellationToken token) => _factoryContext.SecurityContext.IoBoundTaskFactory.StartNew(GetResourceStream, uri, token);

			private static Stream GetResourceStream(object? state)
			{
				var uri = (Uri) state!;

				var assemblyName = uri.Host;
				var assembly = Assembly.Load(assemblyName);
				var name = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped).Replace(oldChar: '/', newChar: '.');

				var stream = assembly.GetManifestResourceStream(name);

				if (stream is null)
				{
					throw new ResourceNotFoundException(Res.Format(Resources.Exception_ResourceNotFound, uri));
				}

				return stream;
			}
		}
	}
}