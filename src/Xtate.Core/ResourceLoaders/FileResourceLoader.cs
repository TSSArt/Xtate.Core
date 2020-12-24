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
using System.Threading;
using System.Threading.Tasks;

namespace Xtate
{
	public sealed class FileResourceLoader : IResourceLoader
	{
		private const FileOptions OpenFileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

		public static IResourceLoader Instance { get; } = new FileResourceLoader();

	#region Interface IResourceLoader

		public bool CanHandle(Uri uri)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			return !uri.IsAbsoluteUri || uri.IsFile || uri.IsUnc;
		}

		public async ValueTask<Resource> Request(Uri uri, NameValueCollection? headers, CancellationToken token)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

			return new Resource(await OpenFileForReadAsync(path, token).ConfigureAwait(false));
		}

	#endregion

		private static ValueTask<FileStream> OpenFileForReadAsync(string path, CancellationToken token) =>
				IoBoundTask.DefaultPool.Run(state => new FileStream((string) state!, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1, OpenFileOptions), path, token);
	}
}