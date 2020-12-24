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
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate
{
	public sealed class ResxResourceLoader : IResourceLoader
	{
		public static IResourceLoader Instance { get; } = new ResxResourceLoader();

	#region Interface IResourceLoader

		public bool CanHandle(Uri uri)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			return uri.IsAbsoluteUri && (uri.Scheme == @"res" || uri.Scheme == @"resx");
		}

		public async ValueTask<Resource> Request(Uri uri, NameValueCollection? headers, CancellationToken token)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			return new Resource(await GetResourceStreamAsync(uri, token).ConfigureAwait(false));
		}

	#endregion

		private static ValueTask<Stream> GetResourceStreamAsync(Uri uri, CancellationToken token) => IoBoundTask.DefaultPool.Run(state => GetResourceStream((Uri) state!), uri, token);

		private static Stream GetResourceStream(Uri uri)
		{
			var assemblyName = uri.Host;
			var assembly = Assembly.Load(assemblyName);
			var name = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped).Replace(oldChar: '/', newChar: '.');

			var stream = assembly.GetManifestResourceStream(name);

			if (stream is null)
			{
				throw new ResourceNotFoundException(Res.Format(Resources.Exception_Resource_not_found, uri));
			}

			return stream;
		}
	}
}