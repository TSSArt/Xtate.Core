// Copyright © 2019-2026 Sergii Artemenko
// 
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

using System.Collections.Specialized;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Xml;
using Xtate.ResourceLoaders;
using Xtate.Scxml.Internal;

namespace Xtate.Scxml.Services;

public class XIncludeReader : DelegatedXmlReader
{
	private static readonly Type ResourceType = typeof(Resource);

	private readonly Func<XmlReader, XmlBaseReader> _xmlBaseReaderFactory;

	private string? _acceptLanguageValue;

	private string? _acceptValue;

	private string? _encodingValue;

	private string? _hrefValue;

	private string? _parseValue;

	private Stack<XmlReader>? _sourceReaders;

	private Strings _strings;

	public XIncludeReader(XmlReader innerReader, Func<XmlReader, XmlBaseReader> xmlBaseReaderFactory) : base(xmlBaseReaderFactory(innerReader))
	{
		Infra.Requires(innerReader);
		Infra.Requires(xmlBaseReaderFactory);

		_xmlBaseReaderFactory = xmlBaseReaderFactory;

		var nameTable = innerReader.NameTable;

		Infra.NotNull(nameTable);

		_strings = new Strings(nameTable);
	}

	public required Func<Stream, ContentType?, Resource> ResourceFactory { private get; [SetByIoC] init; }

	public required XmlResolver XmlResolver { private get; [SetByIoC] init; }

	public required IXIncludeOptions? XIncludeOptions { private get; [SetByIoC] init; }

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

	public override bool Read()
	{
		var valueTask = Read(false);

		Infra.Assert(valueTask.IsCompleted);

		return valueTask.GetAwaiter().GetResult();
	}

	public override Task<bool> ReadAsync() => Read(true).AsTask();

	private async ValueTask<bool> Read(bool useAsync)
	{
		while (true)
		{
			var result = await ReadNext(useAsync).ConfigureAwait(false);

			switch (result)
			{
				case ProcessNodeResult.Ready:    return true;
				case ProcessNodeResult.Complete: return false;
				case ProcessNodeResult.Continue: break;

				default: throw Infra.Unmatched(result);
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
		_hrefValue = null;
		_parseValue = null;
		_encodingValue = null;
		_acceptValue = null;
		_acceptLanguageValue = null;

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

		if (_parseValue is null or @"xml")
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

			return XmlResolver.ResolveUri(baseUri!, href);
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
	private async ValueTask<Resource> LoadAcquiredData(Uri uri, bool useAsync)
	{
		object? resource;

		try
		{
			if (XmlResolver is IExternalEntityGetter externalEntityGetter && externalEntityGetter.SupportsType(uri, ResourceType))
			{
				NameValueCollection? headers = null;

				if (!string.IsNullOrEmpty(_acceptValue))
				{
					(headers ??= new NameValueCollection()).Add(name: @"Accept", _acceptValue);
				}

				if (!string.IsNullOrEmpty(_acceptLanguageValue))
				{
					(headers ??= new NameValueCollection()).Add(name: @"Accept-Language", _acceptLanguageValue);
				}

				resource = useAsync
					? await externalEntityGetter.GetEntityAsync(uri, headers, ResourceType).ConfigureAwait(false)
					: externalEntityGetter.GetEntity(uri, headers, ResourceType);
			}
			else if (XmlResolver.SupportsType(uri, ResourceType))
			{
				resource = useAsync
					? await XmlResolver.GetEntityAsync(uri, role: null, ResourceType).ConfigureAwait(false)
					: XmlResolver.GetEntity(uri, role: null, ResourceType);
			}
			else
			{
				resource = useAsync
					? await XmlResolver.GetEntityAsync(uri, role: null, ofObjectToReturn: null).ConfigureAwait(false)
					: XmlResolver.GetEntity(uri, role: null, ofObjectToReturn: null);
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
			return ResourceFactory(stream, arg2: null);
		}

		return (Resource) resource;
	}

	private async ValueTask<bool> ProcessInterDocXmlInclusion(Uri uri, bool useAsync)
	{
		var resource = await LoadAcquiredData(uri, useAsync).ConfigureAwait(false);
		var stream = await resource.GetStream(true).ConfigureAwait(false);
		var reader = Create(stream, GetXmlReaderSettings(useAsync), uri.ToString());

		PushInnerReader(_xmlBaseReaderFactory(reader));

		return await Read(useAsync).ConfigureAwait(false);
	}

	private XmlReaderSettings GetXmlReaderSettings(bool useAsync)
	{
		var settings = Settings?.Clone() ?? new XmlReaderSettings();

		settings.NameTable = NameTable;
		settings.CloseInput = true;
		settings.Async = useAsync;
		settings.XmlResolver = XmlResolver;

		return settings;
	}

	private void PushInnerReader(XmlReader newInnerReader)
	{
		_sourceReaders ??= new Stack<XmlReader>();

		var maxNestingLevel = XIncludeOptions?.MaxNestingLevel ?? 0;

		if (maxNestingLevel > 0 && _sourceReaders.Count > maxNestingLevel)
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
			? await ReadStreamAsXml(resource, useAsync).ConfigureAwait(false)
			: await ReadStreamAsText(resource, useAsync).ConfigureAwait(false);

		PushInnerReader(new TextContentReader(uri, content));

		return await Read(useAsync).ConfigureAwait(false);
	}

	[SuppressMessage(category: "ReSharper", checkId: "MethodHasAsyncOverload")]
	[SuppressMessage(category: "ReSharper", checkId: "UseAwaitUsing")]
	private static async ValueTask<string> ReadStreamAsXml(Resource resource, bool useAsync)
	{
		var stream = await resource.GetStream(true).ConfigureAwait(false);

		await using (stream.ConfigureAwait(false))
		{
			using var xmlReader = Create(stream);

			var stringBuilder = new StringBuilder();

			using (var xmlWriter = XmlWriter.Create(stringBuilder))
			{
				if (useAsync)
				{
					while (await xmlReader.ReadAsync().ConfigureAwait(false))
					{
						xmlWriter.WriteNode(xmlReader, defattr: false);
					}
				}
				else
				{
					while (xmlReader.Read())
					{
						xmlWriter.WriteNode(xmlReader, defattr: false);
					}
				}
			}

			return stringBuilder.ToString();
		}
	}

	private async ValueTask<string> ReadStreamAsText(Resource resource, bool useAsync)
	{
		var stream = await resource.GetStream(true).ConfigureAwait(false);

		await using (stream.ConfigureAwait(false))
		{
			using var streamReader = new StreamReader(stream, GetEncoding(resource), detectEncodingFromByteOrderMarks: true);

			return useAsync
				? await streamReader.ReadToEndAsync().ConfigureAwait(false)
				: streamReader.ReadToEnd();
		}
	}

	private static bool IsXml(Resource resource)
	{
		var mediaType = resource.ContentType?.MediaType;

		if (mediaType is null)
		{
			return false;
		}

		const StringComparison ct = StringComparison.OrdinalIgnoreCase;
		const string textXml = "text/xml";
		const string applicationXml = "application/xml";
		const string text = "text/";
		const string application = "application/";
		const string xml = "+xml";

		return string.Equals(mediaType, applicationXml, ct)
			   || string.Equals(mediaType, textXml, ct)
			   || ((mediaType.StartsWith(text, ct) || mediaType.StartsWith(application, ct)) && mediaType.EndsWith(xml, ct));
	}

	private Encoding GetEncoding(Resource resource)
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

	private struct Strings(XmlNameTable nameTable)
	{
		public string Accept => field ??= nameTable.Add(@"accept");

		public string AcceptLanguage => field ??= nameTable.Add(@"accept-language");

		public string Encoding => field ??= nameTable.Add(@"encoding");

		public string Href => field ??= nameTable.Add(@"href");

		public string Include => field ??= nameTable.Add(@"include");

		public string Parse => field ??= nameTable.Add(@"parse");

		public string XInclude1Ns => field ??= nameTable.Add(@"http://www.w3.org/2001/XInclude");

		public string XInclude2Ns => field ??= nameTable.Add(@"http://www.w3.org/2003/XInclude");
	}

	private enum ProcessNodeResult
	{
		Ready,

		Continue,

		Complete
	}
}