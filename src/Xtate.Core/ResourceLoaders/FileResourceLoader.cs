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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public sealed class FileResourceLoader : IResourceLoader
	{
		public static readonly FileResourceLoader Instance = new FileResourceLoader();

		private ImmutableDictionary<string, WeakReference<Resource>> _cachedFileResources = ImmutableDictionary<string, WeakReference<Resource>>.Empty;

	#region Interface IResourceLoader

		public bool CanHandle(Uri uri)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			return !uri.IsAbsoluteUri || uri.IsFile || uri.IsUnc;
		}

		public async ValueTask<Resource> Request(Uri uri, CancellationToken token)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;
			var modifiedUtc = new FileInfo(path).LastWriteTimeUtc;

			var cachedFileResources = _cachedFileResources;
			if (cachedFileResources.TryGetValue(path, out var weakReference) && weakReference.TryGetTarget(out var resource) && resource.ModifiedDate == modifiedUtc)
			{
				return resource;
			}

			var bytes = await GetBytesFromFile(path, token).ConfigureAwait(false);

			resource = new Resource(uri, modifiedDate: modifiedUtc, bytes: bytes);

			if (weakReference is null)
			{
				weakReference = new WeakReference<Resource>(resource);
			}
			else
			{
				weakReference.SetTarget(resource);
			}

			_cachedFileResources = cachedFileResources.Add(path, weakReference);

			return resource;
		}

		public ValueTask<XmlReader> RequestXmlReader(Uri uri, XmlReaderSettings? readerSettings = default, XmlParserContext? parserContext = default, CancellationToken token = default)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;
			var modifiedUtc = new FileInfo(path).LastWriteTimeUtc;

			Stream stream;
			if (_cachedFileResources.TryGetValue(path, out var weakReference) && weakReference.TryGetTarget(out var resource) && resource.ModifiedDate == modifiedUtc)
			{
				stream = new MemoryStream(resource.GetBytes() ?? Array.Empty<byte>());
			}
			else
			{
				stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1, FileOptions.Asynchronous | FileOptions.SequentialScan);
			}

			try
			{
				return new ValueTask<XmlReader>(XmlReader.Create(stream, readerSettings, parserContext));
			}
			catch (Exception ex)
			{
				return new ValueTask<XmlReader>(Task.FromException<XmlReader>(ex));
			}
		}

	#endregion

		private static async ValueTask<byte[]> GetBytesFromFile(string path, CancellationToken token)
		{
#if NET5_0
			return await File.ReadAllBytesAsync(path, token).ConfigureAwait(false);
#else
			token.ThrowIfCancellationRequested();

			using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1, FileOptions.Asynchronous | FileOptions.SequentialScan);
			var bytes = new byte[stream.Length];
			await stream.ReadAsync(bytes, offset: 0, (int) stream.Length, token).ConfigureAwait(false);

			return bytes;
#endif
		}
	}
}