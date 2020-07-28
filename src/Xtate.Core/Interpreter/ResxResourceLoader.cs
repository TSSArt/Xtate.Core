#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public sealed class ResxResourceLoader : IResourceLoader
	{
		public static readonly IResourceLoader Instance = new ResxResourceLoader();

		private static readonly XmlReaderSettings CloseInputReaderSettings = new XmlReaderSettings { CloseInput = true };

	#region Interface IResourceLoader

		public bool CanHandle(Uri uri)
		{
			if (uri == null) throw new ArgumentNullException(nameof(uri));

			return uri.IsAbsoluteUri && (uri.Scheme == "res" || uri.Scheme == "resx");
		}

		public async ValueTask<Resource> Request(Uri uri, CancellationToken token)
		{
			if (uri == null) throw new ArgumentNullException(nameof(uri));

			var stream = GetResourceStream(uri);
			await using (stream.ConfigureAwait(false))
			{
				using var reader = new StreamReader(stream);
				var content = await reader.ReadToEndAsync().ConfigureAwait(false); //TODO: ReadToEndAsync replace to support CancellationToken  
				return new Resource(uri, contentType: default, content);
			}
		}

		public ValueTask<XmlReader> RequestXmlReader(Uri uri, XmlReaderSettings? readerSettings = default, XmlParserContext? parserContext = default, CancellationToken token = default)
		{
			if (uri == null) throw new ArgumentNullException(nameof(uri));

			try
			{
				var stream = GetResourceStream(uri);

				readerSettings ??= CloseInputReaderSettings;

				if (!readerSettings.CloseInput)
				{
					readerSettings = readerSettings.Clone();
					readerSettings.CloseInput = true;
				}

				return new ValueTask<XmlReader>(XmlReader.Create(stream, readerSettings, parserContext));
			}
			catch (Exception ex)
			{
				return new ValueTask<XmlReader>(Task.FromException<XmlReader>(ex));
			}
		}

	#endregion

		private static Stream GetResourceStream(Uri uri)
		{
			var assemblyName = uri.Host;
			var assembly = Assembly.Load(assemblyName);
			var name = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped).Replace(oldChar: '/', newChar: '.');

			var stream = assembly.GetManifestResourceStream(name);

			if (stream == null)
			{
				throw new ResourceNotFoundException(Res.Format(Resources.Exception_Resource_not_found, uri));
			}

			return stream;
		}
	}
}