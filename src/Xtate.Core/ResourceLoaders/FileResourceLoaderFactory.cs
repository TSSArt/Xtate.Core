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
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate
{
	[PublicAPI]
	public sealed class FileResourceLoaderFactory : IResourceLoaderFactory
	{
		private readonly Activator              _activator = new();
		public static    IResourceLoaderFactory Instance { get; } = new FileResourceLoaderFactory();

	#region Interface IResourceLoaderFactory

		public ValueTask<IResourceLoaderFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, Uri uri, CancellationToken token)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			return CanHandle(uri) ? new ValueTask<IResourceLoaderFactoryActivator?>(_activator) : default;
		}

	#endregion

		private static bool CanHandle(Uri uri) => !uri.IsAbsoluteUri || uri.IsFile || uri.IsUnc;

		private sealed class Activator : IResourceLoaderFactoryActivator
		{
		#region Interface IResourceLoaderFactoryActivator

			public ValueTask<IResourceLoader> CreateResourceLoader(IFactoryContext factoryContext, CancellationToken token) => new(new ResourceLoader(factoryContext));

		#endregion
		}

		private sealed class ResourceLoader : IResourceLoader
		{
			private const FileOptions OpenFileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

			private readonly IFactoryContext _factoryContext;

			public ResourceLoader(IFactoryContext factoryContext) => _factoryContext = factoryContext;

		#region Interface IResourceLoader

			public async ValueTask<Resource> Request(Uri uri, NameValueCollection? headers, CancellationToken token)
			{
				if (uri is null) throw new ArgumentNullException(nameof(uri));

				Infra.Assert(CanHandle(uri));

				var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

				return new Resource(await OpenFileForReadAsync(path, token).ConfigureAwait(false));
			}

		#endregion

			private Task<FileStream> OpenFileForReadAsync(string path, CancellationToken token) => _factoryContext.SecurityContext.IoBoundTaskFactory.StartNew(CreateFileStream, path, token);

			private static FileStream CreateFileStream(object? path) => new((string) path!, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1, OpenFileOptions);
		}
	}
}