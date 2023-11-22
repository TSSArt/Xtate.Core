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

using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;

namespace Xtate.Core;

public class FileResourceLoader : IResourceLoader
{
	private const FileOptions OpenFileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

	public required IIoBoundTask ExternalResources { private get; init; }

	public required Func<Stream, ValueTask<Resource>> ResourceFactory { private get; init; }

#region Interface IResourceLoader

	public virtual async ValueTask<Resource> Request(Uri uri, NameValueCollection? headers)
	{
		Infra.Requires(uri);

		var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

		var fileStream = await ExternalResources.Factory.StartNew(() => CreateFileStream(path)).ConfigureAwait(false);

		return await ResourceFactory(fileStream).ConfigureAwait(false);
	}

#endregion

	protected virtual FileStream CreateFileStream(string path)
	{
		Infra.Requires(path);

		return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1, OpenFileOptions);
	}
}