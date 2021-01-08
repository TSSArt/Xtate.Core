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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Core;

namespace Xtate.XInclude
{
	internal sealed class XIncludeReader : DelegatedXmlReader
	{
		private static readonly Type ResourceType = typeof(IXIncludeResource);

		private readonly int               _maxNestingLevel;
		private readonly XmlReaderSettings _xmlReaderSettings;
		private readonly XmlResolver       _xmlResolver;

		private string?           _acceptLanguageValue;
		private string?           _acceptValue;
		private string?           _encodingValue;
		private string?           _hrefValue;
		private string?           _parseValue;
		private Stack<XmlReader>? _sourceReaders;
		private Strings           _strings;

		public XIncludeReader(XmlReader innerReader, XmlReaderSettings xmlReaderSettings, XmlResolver xmlResolver, int maxNestingLevel)
				: base(new XmlBaseReader(innerReader, xmlResolver))
		{
			if (innerReader is null) throw new ArgumentNullException(nameof(innerReader));
			if (maxNestingLevel < 0) throw new ArgumentOutOfRangeException(nameof(maxNestingLevel));

			_xmlReaderSettings = xmlReaderSettings ?? throw new ArgumentNullException(nameof(xmlReaderSettings));
			_xmlResolver = xmlResolver ?? throw new ArgumentNullException(nameof(xmlResolver));
			_maxNestingLevel = maxNestingLevel;

			var nameTable = innerReader.NameTable;
			Infrastructure.NotNull(nameTable);
			_strings = new Strings(nameTable);
		}

		public override int Depth
		{
			get
			{
				var depth = base.Depth;

				if (_sourceReaders?.Count > 0)
				{
					foreach (var reader in _sourceReaders)
					{
						depth += reader.Depth;
					}
				}

				return depth;
			}
		}

		public override void Close()
		{
			if (_sourceReaders is not null)
			{
				while (_sourceReaders.Count > 0)
				{
					InnerReader.Close();
					InnerReader = _sourceReaders.Pop();
				}
			}

			base.Close();
		}

		public override bool Read() => Read(false).SynchronousGetResult();

		public override Task<bool> ReadAsync() => Read(true).AsTask();

		private async ValueTask<bool> Read(bool useAsync)
		{
			while (true)
			{
				var result = await ReadNext(useAsync).ConfigureAwait(false);

				switch (result)
				{
					case ProcessNodeResult.Ready: return true;
					case ProcessNodeResult.Complete: return false;
					case ProcessNodeResult.Continue: break;
					default: return Infrastructure.UnexpectedValue<bool>(result);
				}
			}
		}

		private ValueTask<bool> ReadInnerReader(bool useAsync) => useAsync ? new ValueTask<bool>(InnerReader.ReadAsync()) : new ValueTask<bool>(InnerReader.Read());

		private async ValueTask<ProcessNodeResult> ReadNext(bool useAsync)
		{
			var read = await ReadInnerReader(useAsync).ConfigureAwait(false);

			if (!read)
			{
				if (_sourceReaders?.Count > 0)
				{
					InnerReader.Close();

					InnerReader = _sourceReaders.Pop();

					return ProcessNodeResult.Continue;
				}

				return ProcessNodeResult.Complete;
			}

			switch (InnerReader.NodeType)
			{
				case XmlNodeType.XmlDeclaration:
				case XmlNodeType.Document:
				case XmlNodeType.DocumentType:
				case XmlNodeType.DocumentFragment:
					return _sourceReaders?.Count > 0 ? ProcessNodeResult.Continue : ProcessNodeResult.Ready;

				case XmlNodeType.Element when IsIncludeElement():
					var result = await ProcessIncludeElement(useAsync).ConfigureAwait(false);

					return result ? ProcessNodeResult.Ready : ProcessNodeResult.Complete;

				default:
					return ProcessNodeResult.Ready;
			}
		}

		private bool IsIncludeElement() =>
				(
						ReferenceEquals(InnerReader.NamespaceURI, _strings.XInclude1Ns) ||
						ReferenceEquals(InnerReader.NamespaceURI, _strings.XInclude2Ns)
				)
				&& ReferenceEquals(InnerReader.LocalName, _strings.Include)
				&& InnerReader.IsEmptyElement;

		private void ExtractIncludeElementAttributes()
		{
			_hrefValue = default;
			_parseValue = default;
			_encodingValue = default;
			_acceptValue = default;
			_acceptLanguageValue = default;

			for (var ok = InnerReader.MoveToFirstAttribute(); ok; ok = InnerReader.MoveToNextAttribute())
			{
				if (!string.IsNullOrEmpty(InnerReader.NamespaceURI))
				{
					continue;
				}

				if (ReferenceEquals(InnerReader.LocalName, _strings.Href))
				{
					_hrefValue = InnerReader.Value;
				}
				else if (ReferenceEquals(InnerReader.LocalName, _strings.Parse))
				{
					_parseValue = InnerReader.Value;
				}
				else if (ReferenceEquals(InnerReader.LocalName, _strings.Encoding))
				{
					_encodingValue = InnerReader.Value;
				}
				else if (ReferenceEquals(InnerReader.LocalName, _strings.Accept))
				{
					_acceptValue = InnerReader.Value;
				}
				else if (ReferenceEquals(InnerReader.LocalName, _strings.AcceptLanguage))
				{
					_acceptLanguageValue = InnerReader.Value;
				}
			}
		}

		private async ValueTask<bool> ProcessIncludeElement(bool useAsync)
		{
			ExtractIncludeElementAttributes();

			if (string.IsNullOrEmpty(_hrefValue))
			{
				throw new XIncludeException(Resources.Exception_IndocumentReferencesNotSupported, InnerReader);
			}

			if (_parseValue is null || _parseValue == @"xml")
			{
				return await ProcessInterDocXmlInclusion(ResolveHref(_hrefValue), useAsync).ConfigureAwait(false);
			}

			if (_parseValue == @"text")
			{
				return await ProcessInterDocTextInclusion(ResolveHref(_hrefValue), useAsync).ConfigureAwait(false);
			}

			throw new XIncludeException(Resources.Exception_UnknownParseAttrValue, InnerReader);
		}

		private Uri ResolveHref(string href)
		{
			try
			{
				var baseUri = InnerReader.BaseURI is { Length: > 0 } uri ? new Uri(uri) : null;

				return _xmlResolver.ResolveUri(baseUri!, href);
			}
			catch (UriFormatException ex)
			{
				throw new XIncludeException(Res.Format(Resources.Exception_InvalidURI, href), ex);
			}
			catch (Exception ex)
			{
				throw new XIncludeException(Res.Format(Resources.Exception_UnresolvableURI, href), ex);
			}
		}

		[SuppressMessage(category: "ReSharper", checkId: "MethodHasAsyncOverload")]
		private async ValueTask<IXIncludeResource> LoadAcquiredData(Uri uri, bool useAsync)
		{
			object? resource;
			try
			{
				if (_xmlResolver is IXIncludeXmlResolver resolver)
				{
					resource = useAsync
							? await resolver.GetEntityAsync(uri, _acceptValue, _acceptLanguageValue, ResourceType).ConfigureAwait(false)
							: resolver.GetEntity(uri, _acceptValue, _acceptLanguageValue, ResourceType);
				}
				else if (_xmlResolver.SupportsType(uri, ResourceType))
				{
					resource = useAsync
							? await _xmlResolver.GetEntityAsync(uri, role: null, ResourceType).ConfigureAwait(false)
							: _xmlResolver.GetEntity(uri, role: null, ResourceType);
				}
				else
				{
					resource = useAsync
							? await _xmlResolver.GetEntityAsync(uri, role: null, ofObjectToReturn: default).ConfigureAwait(false)
							: _xmlResolver.GetEntity(uri, role: null, ofObjectToReturn: default);
				}
			}
			catch (Exception ex)
			{
				throw new XIncludeException(Resources.Exception_XmlResolverGetEntity, ex);
			}

			if (resource is null)
			{
				throw new XIncludeException(Resources.Exception_XmlResolverReturnedNull);
			}

			if (resource is Stream stream)
			{
				return new StreamResource(stream);
			}

			return (IXIncludeResource) resource;
		}

		private async ValueTask<bool> ProcessInterDocXmlInclusion(Uri uri, bool useAsync)
		{
			var resource = await LoadAcquiredData(uri, useAsync).ConfigureAwait(false);
			var stream = await resource.GetStream().ConfigureAwait(false);
			var reader = Create(stream, GetXmlReaderSettings(), uri.ToString());

			PushInnerReader(new XmlBaseReader(reader, _xmlResolver));

			return await Read(useAsync).ConfigureAwait(false);
		}

		private XmlReaderSettings GetXmlReaderSettings()
		{
			var settings = _xmlReaderSettings.Clone();

			settings.CloseInput = true;
			settings.XmlResolver = _xmlResolver;

			return settings;
		}

		private void PushInnerReader(XmlReader newInnerReader)
		{
			_sourceReaders ??= new Stack<XmlReader>();

			if (_maxNestingLevel > 0 && _sourceReaders.Count > _maxNestingLevel)
			{
				throw new XIncludeException(Resources.Exception_NestingReachedLevelInclusion, InnerReader);
			}

			_sourceReaders.Push(InnerReader);

			InnerReader = newInnerReader;
		}

		private async ValueTask<bool> ProcessInterDocTextInclusion(Uri uri, bool useAsync)
		{
			var resource = await LoadAcquiredData(uri, useAsync).ConfigureAwait(false);

			var content = IsXml(resource)
					? await ReadStreamAsXml(resource).ConfigureAwait(false)
					: await ReadStreamAsText(resource).ConfigureAwait(false);

			PushInnerReader(new TextContentReader(uri, content));

			return await Read(useAsync).ConfigureAwait(false);
		}

		[SuppressMessage(category: "ReSharper", checkId: "MethodHasAsyncOverload")]
		[SuppressMessage(category: "ReSharper", checkId: "UseAwaitUsing")]
		private static async ValueTask<string> ReadStreamAsXml(IXIncludeResource resource)
		{
			var stream = await resource.GetStream().ConfigureAwait(false);

			await using (stream.ConfigureAwait(false))
			{
				using var xmlReader = Create(stream);

				var stringBuilder = new StringBuilder();
				using (var xmlWriter = XmlWriter.Create(stringBuilder))
				{
					while (await xmlReader.ReadAsync().ConfigureAwait(false))
					{
						xmlWriter.WriteNode(xmlReader, defattr: false);
					}
				}

				return stringBuilder.ToString();
			}
		}

		private async ValueTask<string> ReadStreamAsText(IXIncludeResource resource)
		{
			var stream = await resource.GetStream().ConfigureAwait(false);

			await using (stream.ConfigureAwait(false))
			{
				using var streamReader = new StreamReader(stream, GetEncoding(resource), detectEncodingFromByteOrderMarks: true);

				return await streamReader.ReadToEndAsync().ConfigureAwait(false);
			}
		}

		private static bool IsXml(IXIncludeResource resource)
		{
			switch (resource.ContentType?.MediaType)
			{
				case "text/xml":
				case "application/xml":
				case { } mt when (mt.StartsWith(value: @"text/", StringComparison.Ordinal)
								  || mt.StartsWith(value: @"application/", StringComparison.Ordinal))
								 && mt.EndsWith(value: @"+xml", StringComparison.Ordinal):
					return true;
			}

			return false;
		}

		private Encoding GetEncoding(IXIncludeResource resource)
		{
			if (resource.ContentType?.CharSet is { Length: > 0 } charSet)
			{
				return Encoding.GetEncoding(charSet);
			}

			if (_encodingValue is { Length: > 0 } encoding)
			{
				return Encoding.GetEncoding(encoding);
			}

			return Encoding.UTF8;
		}

		private struct Strings
		{
			private readonly XmlNameTable _nameTable;

			private string? _accept;
			private string? _acceptLanguage;
			private string? _encoding;
			private string? _href;
			private string? _include;
			private string? _parse;
			private string? _xInclude1Ns;
			private string? _xInclude2Ns;

			public Strings(XmlNameTable nameTable) : this() => _nameTable = nameTable;

			public string Accept         => _accept ??= _nameTable.Add(@"accept");
			public string AcceptLanguage => _acceptLanguage ??= _nameTable.Add(@"accept-language");
			public string Encoding       => _encoding ??= _nameTable.Add(@"encoding");
			public string Href           => _href ??= _nameTable.Add(@"href");
			public string Include        => _include ??= _nameTable.Add(@"include");
			public string Parse          => _parse ??= _nameTable.Add(@"parse");
			public string XInclude1Ns    => _xInclude1Ns ??= _nameTable.Add(@"http://www.w3.org/2001/XInclude");
			public string XInclude2Ns    => _xInclude2Ns ??= _nameTable.Add(@"http://www.w3.org/2003/XInclude");
		}

		private class StreamResource : IXIncludeResource
		{
			private readonly Stream _stream;

			public StreamResource(Stream stream) => _stream = stream;

		#region Interface IXIncludeResource

			public ValueTask<Stream> GetStream() => new(_stream);

			public ContentType? ContentType => null;

		#endregion
		}

		private enum ProcessNodeResult
		{
			Ready,
			Continue,
			Complete
		}
	}
}