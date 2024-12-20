﻿// Copyright © 2019-2024 Sergii Artemenko
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

using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Xtate.DataModel.XPath;

public static class XmlConverter
{
	public const string TypeAttributeName = @"type";

	public const string XPathElementNamespace = @"http://xtate.net/xpath";

	private const string NoKeyElementName = @"item";

	private const string EmptyKeyElementName = @"empty";

	private const string XPathElementPrefix = @"x";

	private const string BoolTypeValue = @"bool";

	private const string DatetimeTypeValue = @"datetime";

	private const string NumberTypeValue = @"number";

	private const string NullTypeValue = @"null";

	private const string UndefinedTypeValue = @"undefined";

	private static readonly XmlReaderSettings DefaultReaderSettings = new() { ConformanceLevel = ConformanceLevel.Auto };

	public static string ToXml(in DataModelValue value, bool indent)
	{
		using var textWriter = new StringWriter(CultureInfo.InvariantCulture);
		using var xmlWriter = XmlWriter.Create(textWriter, GetOptions(indent, async: false));
		Infra.NotNull(xmlWriter);

		var navigator = new DataModelXPathNavigator(value);

		WriteNode(xmlWriter, navigator);

		xmlWriter.Flush();

		return textWriter.ToString();
	}

	public static async Task AsXmlToStreamAsync(DataModelValue value, bool indent, Stream stream)
	{
		var xmlWriter = XmlWriter.Create(stream, GetOptions(indent, async: true));

		await using (xmlWriter.ConfigureAwait(false))
		{
			var navigator = new DataModelXPathNavigator(value);

			await WriteNodeAsync(xmlWriter, navigator).ConfigureAwait(false);

			await xmlWriter.FlushAsync().ConfigureAwait(false);
		}
	}

	public static void AsXmlToStream(DataModelValue value, bool indent, Stream stream)
	{
		using var xmlWriter = XmlWriter.Create(stream, GetOptions(indent, async: false));
		var navigator = new DataModelXPathNavigator(value);

		WriteNode(xmlWriter, navigator);

		xmlWriter.Flush();
	}

	private static XmlWriterSettings GetOptions(bool indent, bool async) =>
		new()
		{
			Indent = indent,
			OmitXmlDeclaration = true,
			ConformanceLevel = ConformanceLevel.Auto,
			Async = async
		};

	private static void WriteNode(XmlWriter xmlWriter, XPathNavigator navigator)
	{
		if (navigator is { NodeType: XPathNodeType.Element, LocalName.Length: 0 })
		{
			if (navigator.HasChildren)
			{
				for (var moved = navigator.MoveToFirstChild(); moved; moved = navigator.MoveToNext())
				{
					WriteNode(xmlWriter, navigator);
				}

				navigator.MoveToParent();
			}
		}
		else
		{
			xmlWriter.WriteNode(navigator, defattr: true);
		}
	}

	private static async Task WriteNodeAsync(XmlWriter xmlWriter, XPathNavigator navigator)
	{
		if (navigator is { NodeType: XPathNodeType.Element, LocalName.Length: 0 })
		{
			if (navigator.HasChildren)
			{
				for (var moved = navigator.MoveToFirstChild(); moved; moved = navigator.MoveToNext())
				{
					await WriteNodeAsync(xmlWriter, navigator).ConfigureAwait(false);
				}

				navigator.MoveToParent();
			}
		}
		else
		{
			await xmlWriter.WriteNodeAsync(navigator, defattr: true).ConfigureAwait(false);
		}
	}

	public static DataModelValue FromXml(string xml, XmlParserContext? context = default)
	{
		using var reader = new StringReader(xml);
		using var xmlReader = XmlReader.Create(reader, DefaultReaderSettings, context);

		return LoadValue(xmlReader);
	}

	public static DataModelValue FromXmlStream(Stream stream, XmlParserContext? context = default)
	{
		using var xmlReader = XmlReader.Create(stream, DefaultReaderSettings, context);

		return LoadValue(xmlReader);
	}

	public static async ValueTask<DataModelValue> FromXmlStreamAsync(Stream stream, XmlParserContext? context = default)
	{
		using var xmlReader = XmlReader.Create(stream, DefaultReaderSettings, context);

		return await LoadValueAsync(xmlReader).ConfigureAwait(false);
	}

	public static string? NsNameToKey(string ns, string localName) =>
		ns == XPathElementNamespace
			? localName switch
			  {
				  NoKeyElementName    => null,
				  EmptyKeyElementName => string.Empty,
				  _                   => XmlConvert.DecodeName(localName)
			  }
			: XmlConvert.DecodeName(localName);

	public static string KeyToLocalName(string? key)
	{
		if (key is null)
		{
			return NoKeyElementName;
		}

		if (key.Length == 0)
		{
			return EmptyKeyElementName;
		}

		var localName = XmlConvert.EncodeLocalName(key);

		Infra.NotNull(localName);

		return localName;
	}

	public static string? KeyToNamespaceOrDefault(string? key) =>
		key switch
		{
			null          => XPathElementNamespace,
			{ Length: 0 } => XPathElementNamespace,
			_             => null
		};

	public static string? KeyToPrefixOrDefault(string? key) =>
		key switch
		{
			null          => XPathElementPrefix,
			{ Length: 0 } => XPathElementPrefix,
			_             => null
		};

	private static async ValueTask<DataModelValue> LoadValueAsync(XmlReader xmlReader)
	{
		DataModelList? list = default;

		do
		{
			await xmlReader.MoveToContentAsync().ConfigureAwait(false);

			switch (xmlReader.NodeType)
			{
				case XmlNodeType.Element:

					var key = NsNameToKey(xmlReader.NamespaceURI, xmlReader.LocalName);

					var metadata = GetMetaData(xmlReader);

					list ??= [];

					if (!xmlReader.IsEmptyElement)
					{
						var type = xmlReader.GetAttribute(TypeAttributeName, XPathElementNamespace);

						await ReadStartElementAsync(xmlReader).ConfigureAwait(false);
						var value = await LoadValueAsync(xmlReader).ConfigureAwait(false);

						list.Add(key, ToType(value, type), metadata);
					}
					else
					{
						var type = xmlReader.GetAttribute(TypeAttributeName, XPathElementNamespace);

						list.Add(key, ToType(string.Empty, type), metadata);
					}

					break;

				case XmlNodeType.EndElement:
					await ReadEndElementAsync(xmlReader).ConfigureAwait(false);

					return list;

				case XmlNodeType.Text:
					var text = xmlReader.Value;
					await xmlReader.ReadAsync().ConfigureAwait(false);

					return text;

				case XmlNodeType.None:
					return list;

				default:
					throw Infra.Unmatched(xmlReader.NodeType);
			}
		}
		while (await xmlReader.ReadAsync().ConfigureAwait(false));

		return list;
	}

	private static async ValueTask ReadStartElementAsync(XmlReader xmlReader)
	{
		if (xmlReader.NodeType != XmlNodeType.Element)
		{
			await xmlReader.MoveToContentAsync().ConfigureAwait(false);
		}

		xmlReader.ReadStartElement();
	}

	private static async ValueTask ReadEndElementAsync(XmlReader xmlReader)
	{
		if (xmlReader.NodeType != XmlNodeType.EndElement)
		{
			await xmlReader.MoveToContentAsync().ConfigureAwait(false);
		}

		xmlReader.ReadEndElement();
	}

	private static DataModelValue LoadValue(XmlReader xmlReader)
	{
		DataModelList? list = default;

		do
		{
			xmlReader.MoveToContent();

			switch (xmlReader.NodeType)
			{
				case XmlNodeType.Element:

					var key = NsNameToKey(xmlReader.NamespaceURI, xmlReader.LocalName);

					var metadata = GetMetaData(xmlReader);

					list ??= [];

					if (!xmlReader.IsEmptyElement)
					{
						var type = xmlReader.GetAttribute(TypeAttributeName, XPathElementNamespace);

						xmlReader.ReadStartElement();
						var value = LoadValue(xmlReader);

						list.Add(key, ToType(value, type), metadata);
					}
					else
					{
						var type = xmlReader.GetAttribute(TypeAttributeName, XPathElementNamespace);

						list.Add(key, ToType(string.Empty, type), metadata);
					}

					break;

				case XmlNodeType.EndElement:
					xmlReader.ReadEndElement();

					return list;

				case XmlNodeType.Text:
					var text = xmlReader.Value;
					xmlReader.Read();

					return text;

				case XmlNodeType.None:

					return list;

				default:
					throw Infra.Unmatched(xmlReader.NodeType);
			}
		}
		while (xmlReader.Read());

		return list;
	}

	private static DataModelValue ToType(in DataModelValue value, string? type)
	{
		return type switch
			   {
				   null               => value,
				   BoolTypeValue      => XmlConvert.ToBoolean(value.AsString()),
				   DatetimeTypeValue  => XmlConvert.ToDateTimeOffset(value.AsString()),
				   NumberTypeValue    => XmlConvert.ToDouble(value.AsString()),
				   NullTypeValue      => DataModelValue.Null,
				   UndefinedTypeValue => default,
				   _                  => throw Infra.Unmatched(type)
			   };
	}

	public static string ToString(in DataModelValue value) =>
		value.Type switch
		{
			DataModelValueType.Undefined => string.Empty,
			DataModelValueType.Null      => string.Empty,
			DataModelValueType.Boolean   => value.AsBoolean() ? @"true" : @"false",
			DataModelValueType.String    => value.AsString(),
			DataModelValueType.Number    => NumberToXmlString(value.AsNumber()),
			DataModelValueType.DateTime  => DateTimeToXmlString(value.AsDateTime()),
			_                            => throw Infra.Unmatched(value.Type)
		};

	private static string NumberToXmlString(in DataModelNumber number) =>
		number.Type switch
		{
			DataModelNumberType.Int32   => XmlConvert.ToString(number.ToInt32()),
			DataModelNumberType.Int64   => XmlConvert.ToString(number.ToInt64()),
			DataModelNumberType.Double  => XmlConvert.ToString(number.ToDouble()),
			DataModelNumberType.Decimal => XmlConvert.ToString(number.ToDecimal()),
			_                           => throw Infra.Unmatched(number.Type)
		};

	private static string DateTimeToXmlString(in DataModelDateTime dttm) =>
		dttm.Type switch
		{
			DataModelDateTimeType.DateTime       => XmlConvert.ToString(dttm.ToDateTime(), XmlDateTimeSerializationMode.RoundtripKind),
			DataModelDateTimeType.DateTimeOffset => XmlConvert.ToString(dttm.ToDateTimeOffset()),
			_                                    => throw Infra.Unmatched(dttm.Type)
		};

	public static int GetBufferSizeForValue(in DataModelValue value) =>
		value.Type switch
		{
			DataModelValueType.Undefined => 0,
			DataModelValueType.Null      => 0,
			DataModelValueType.String    => value.AsString().Length,
			DataModelValueType.Number    => 24, // -1.2345678901234567e+123 (G17)
			DataModelValueType.DateTime  => 33, // YYYY-MM-DDThh:mm:ss.1234567+hh:mm (DateTime with Offset)
			DataModelValueType.Boolean   => 5,  // 'false' - longest value
			_                            => throw Infra.Unmatched(value.Type)
		};

	public static int WriteValueToSpan(in DataModelValue value, in Span<char> span)
	{
		return value.Type switch
			   {
				   DataModelValueType.Undefined => 0,
				   DataModelValueType.Null      => 0,
				   DataModelValueType.String    => WriteString(value.AsString(), span),
				   DataModelValueType.Number    => WriteDataModelNumber(value.AsNumber(), span),
				   DataModelValueType.DateTime  => WriteDataModelDateTime(value.AsDateTime(), span),
				   DataModelValueType.Boolean   => WriteString(value.AsBoolean() ? @"true" : @"false", span),
				   _                            => throw Infra.Unmatched(value.Type)
			   };

		static int WriteDataModelNumber(in DataModelNumber value, in Span<char> span) =>
			value.Type switch
			{
				DataModelNumberType.Int32  => WriteString(XmlConvert.ToString(value.ToInt32()), span),
				DataModelNumberType.Int64  => WriteString(XmlConvert.ToString(value.ToInt64()), span),
				DataModelNumberType.Double => WriteString(XmlConvert.ToString(value.ToDouble()), span),
				_                          => throw Infra.Unmatched(value.Type)
			};

		static int WriteDataModelDateTime(in DataModelDateTime value, in Span<char> span) =>
			value.Type switch
			{
				DataModelDateTimeType.DateTime       => WriteString(XmlConvert.ToString(value.ToDateTime(), XmlDateTimeSerializationMode.RoundtripKind), span),
				DataModelDateTimeType.DateTimeOffset => WriteString(XmlConvert.ToString(value.ToDateTimeOffset()), span),
				_                                    => throw Infra.Unmatched(value.Type)
			};

		static int WriteString(string value, in Span<char> span)
		{
			value.AsSpan().CopyTo(span);

			return value.Length;
		}
	}

	public static DataModelValue GetTypeValue(in DataModelValue value) =>
		value.Type switch
		{
			DataModelValueType.Boolean   => BoolTypeValue,
			DataModelValueType.DateTime  => DatetimeTypeValue,
			DataModelValueType.Number    => NumberTypeValue,
			DataModelValueType.Null      => NullTypeValue,
			DataModelValueType.Undefined => UndefinedTypeValue,
			_                            => throw Infra.Unmatched(value.Type)
		};

	private static DataModelList? GetMetaData(XmlReader xmlReader)
	{
		var elementPrefix = xmlReader.Prefix;
		var elementNs = xmlReader.NamespaceURI;

		if (elementPrefix.Length == 0 && elementNs.Length == 0 && !xmlReader.HasAttributes)
		{
			return null;
		}

		var metadata = new DataModelList { elementPrefix, elementNs };

		if (xmlReader.HasAttributes)
		{
			for (var ok = xmlReader.MoveToFirstAttribute(); ok; ok = xmlReader.MoveToNextAttribute())
			{
				if (xmlReader.NamespaceURI != XPathMetadata.XmlnsNamespace)
				{
					metadata.Add(xmlReader.LocalName);
					metadata.Add(xmlReader.Value);
					metadata.Add(xmlReader.Prefix);
					metadata.Add(xmlReader.NamespaceURI);
				}
				else if (xmlReader.LocalName != XPathMetadata.Xmlns)
				{
					metadata.Add(xmlReader.LocalName);
					metadata.Add(xmlReader.Value);
					metadata.Add(string.Empty);
					metadata.Add(xmlReader.NamespaceURI);
				}
				else
				{
					metadata.Add(string.Empty);
					metadata.Add(xmlReader.Value);
					metadata.Add(string.Empty);
					metadata.Add(xmlReader.NamespaceURI);
				}
			}

			xmlReader.MoveToElement();
		}

		return metadata;
	}
}