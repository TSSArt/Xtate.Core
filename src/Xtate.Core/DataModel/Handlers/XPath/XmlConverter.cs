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
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Xtate.Core;
using Xtate.Scxml;

namespace Xtate.DataModel.XPath
{
	internal static class XmlConverter
	{
		public const string TypeAttributeName     = @"type";
		public const string XPathElementNamespace = @"http://xtate.net/xpath";

		private const string NoKeyElementName    = @"item";
		private const string EmptyKeyElementName = @"empty";
		private const string XPathElementPrefix  = @"x";
		private const string BoolTypeValue       = @"bool";
		private const string DatetimeTypeValue   = @"datetime";
		private const string NumberTypeValue     = @"number";
		private const string NullTypeValue       = @"null";
		private const string UndefinedTypeValue  = @"undefined";

		private static readonly XmlReaderSettings DefaultReaderSettings = new() { ConformanceLevel = ConformanceLevel.Auto };

		public static string ToXml(in DataModelValue value, bool indent)
		{
			using var textWriter = new StringWriter(CultureInfo.InvariantCulture);
			using var xmlWriter = XmlWriter.Create(textWriter, GetOptions(indent, async: false));
			Infrastructure.NotNull(xmlWriter);

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
			if (navigator is { NodeType: XPathNodeType.Element, LocalName: { Length: 0 } })
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
			if (navigator is { NodeType: XPathNodeType.Element, LocalName: { Length: 0 } })
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

		public static DataModelValue FromXml(string xml, object? entity = default)
		{
			entity.Is<XmlNameTable>(out var nameTable);

			nameTable ??= new NameTable();
			var namespaceManager = new XmlNamespaceManager(nameTable);

			if (entity.Is<IXmlNamespacesInfo>(out var namespacesInfo))
			{
				foreach (var prefixUri in namespacesInfo.Namespaces)
				{
					namespaceManager.AddNamespace(prefixUri.Prefix, prefixUri.Namespace);
				}
			}

			var context = new XmlParserContext(nameTable, namespaceManager, xmlLang: null, XmlSpace.None);

			using var reader = new StringReader(xml);
			using var xmlReader = XmlReader.Create(reader, DefaultReaderSettings, context);

			return LoadValue(xmlReader);
		}

		public static DataModelValue FromXmlStream(Stream stream)
		{
			using var xmlReader = XmlReader.Create(stream, DefaultReaderSettings);

			return LoadValue(xmlReader);
		}

		public static async ValueTask<DataModelValue> FromXmlStreamAsync(Stream stream)
		{
			using var xmlReader = XmlReader.Create(stream, DefaultReaderSettings);

			return await LoadValueAsync(xmlReader).ConfigureAwait(false);
		}

		public static string? NsNameToKey(string ns, string localName) =>
				ns != XPathElementNamespace
						? XmlConvert.DecodeName(localName)
						: localName switch
						{
								NoKeyElementName => null,
								EmptyKeyElementName => string.Empty,
								_ => XmlConvert.DecodeName(localName)
						};

		public static string KeyToLocalName(string? key) =>
				key switch
				{
						{ Length: 0 } => EmptyKeyElementName,
						_ => key is not null ? XmlConvert.EncodeLocalName(key)! : NoKeyElementName
				};

		public static string? KeyToNamespaceOrDefault(string? key) =>
				key switch
				{
						null => XPathElementNamespace,
						{ Length: 0 } => XPathElementNamespace,
						_ => null
				};

		public static string? KeyToPrefixOrDefault(string? key) =>
				key switch
				{
						null => XPathElementPrefix,
						{ Length: 0 } => XPathElementPrefix,
						_ => null
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

						list ??= new DataModelList();

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
						Infrastructure.UnexpectedValue(xmlReader.NodeType);
						break;
				}
			} while (await xmlReader.ReadAsync().ConfigureAwait(false));

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

						list ??= new DataModelList();

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
						Infrastructure.UnexpectedValue(xmlReader.NodeType);
						break;
				}
			} while (xmlReader.Read());

			return list;
		}

		private static DataModelValue ToType(in DataModelValue val, string? type)
		{
			return type switch
			{
					null => val,
					BoolTypeValue => XmlConvert.ToBoolean(val.AsString()),
					DatetimeTypeValue => XmlConvert.ToDateTimeOffset(val.AsString()),
					NumberTypeValue => XmlConvert.ToDouble(val.AsString()),
					NullTypeValue => DataModelValue.Null,
					UndefinedTypeValue => default,
					_ => Infrastructure.UnexpectedValue<DataModelValue>(type)
			};
		}

		public static string ToString(in DataModelValue value) =>
				value.Type switch
				{
						DataModelValueType.Undefined => string.Empty,
						DataModelValueType.Null => string.Empty,
						DataModelValueType.String => value.AsString(),
						DataModelValueType.Number => XmlConvert.ToString(value.AsNumber()),
						DataModelValueType.Boolean => value.AsBoolean() ? @"true" : @"false",
						DataModelValueType.DateTime => DateTimeToXmlString(value.AsDateTime()),
						_ => Infrastructure.UnexpectedValue<string>(value.Type)
				};

		private static string DateTimeToXmlString(in DataModelDateTime dttm) =>
				dttm.Type switch
				{
						DataModelDateTimeType.DateTime => XmlConvert.ToString(dttm.ToDateTime(), XmlDateTimeSerializationMode.RoundtripKind),
						DataModelDateTimeType.DateTimeOffset => XmlConvert.ToString(dttm.ToDateTimeOffset()),
						_ => Infrastructure.UnexpectedValue<string>(dttm.Type)
				};

		public static int GetBufferSizeForValue(in DataModelValue value) =>
				value.Type switch
				{
						DataModelValueType.Undefined => 0,
						DataModelValueType.Null => 0,
						DataModelValueType.String => value.AsString().Length,
						DataModelValueType.Number => 24, // -1.2345678901234567e+123 (G17)
						DataModelValueType.DateTime => 33, // YYYY-MM-DDThh:mm:ss.1234567+hh:mm (DateTime with Offset)
						DataModelValueType.Boolean => 5, // 'false' - longest value
						_ => Infrastructure.UnexpectedValue<int>(value.Type)
				};

		public static int WriteValueToSpan(in DataModelValue value, in Span<char> span)
		{
			return value.Type switch
			{
					DataModelValueType.Undefined => 0,
					DataModelValueType.Null => 0,
					DataModelValueType.String => WriteString(value.AsString(), span),
					DataModelValueType.Number => WriteString(XmlConvert.ToString(value.AsNumber()), span),
					DataModelValueType.DateTime => WriteDataModelDateTime(value.AsDateTime(), span),
					DataModelValueType.Boolean => WriteString(value.AsBoolean() ? @"true" : @"false", span),
					_ => Infrastructure.UnexpectedValue<int>(value.Type)
			};

			static int WriteDataModelDateTime(in DataModelDateTime val, in Span<char> span) =>
					val.Type switch
					{
							DataModelDateTimeType.DateTime => WriteString(XmlConvert.ToString(val.ToDateTime(), XmlDateTimeSerializationMode.RoundtripKind), span),
							DataModelDateTimeType.DateTimeOffset => WriteString(XmlConvert.ToString(val.ToDateTimeOffset()), span),
							_ => Infrastructure.UnexpectedValue<int>(val.Type)
					};

			static int WriteString(string val, in Span<char> span)
			{
				val.AsSpan().CopyTo(span);
				return val.Length;
			}
		}

		public static DataModelValue GetTypeValue(in DataModelValue val) =>
				val.Type switch
				{
						DataModelValueType.Boolean => BoolTypeValue,
						DataModelValueType.DateTime => DatetimeTypeValue,
						DataModelValueType.Number => NumberTypeValue,
						DataModelValueType.Null => NullTypeValue,
						DataModelValueType.Undefined => UndefinedTypeValue,
						_ => Infrastructure.UnexpectedValue<bool>(val.Type)
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
}