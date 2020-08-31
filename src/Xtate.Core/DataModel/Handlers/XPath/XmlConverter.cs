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

using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Xtate.Scxml;

namespace Xtate.DataModel.XPath
{
	internal static class XmlConverter
	{
		private static readonly XmlWriterSettings DefaultWriterSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Auto };
		private static readonly XmlReaderSettings DefaultReaderSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Auto };

		public static string ToXml(in DataModelValue dataModelValue)
		{
			using var textWriter = new StringWriter(CultureInfo.InvariantCulture);
			using var xmlWriter = XmlWriter.Create(textWriter, DefaultWriterSettings);
			Infrastructure.NotNull(xmlWriter);

			var navigator = new DataModelXPathNavigator(dataModelValue);

			WriteNode(xmlWriter, navigator);

			xmlWriter.Flush();

			return textWriter.ToString();
		}

		private static void WriteNode(XmlWriter xmlWriter, XPathNavigator navigator)
		{
			if (navigator.NodeType == XPathNodeType.Element && navigator.LocalName is {Length: 0})
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

		public static DataModelValue FromXml(string xml, object? entity = null)
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

		private static DataModelValue LoadValue(XmlReader xmlReader)
		{
			DataModelObject? obj = null;

			do
			{
				xmlReader.MoveToContent();
				switch (xmlReader.NodeType)
				{
					case XmlNodeType.Element:

						var name = XmlConvert.DecodeName(xmlReader.LocalName);
						var metadata = GetMetaData(xmlReader);

						obj ??= new DataModelObject();

						if (!xmlReader.IsEmptyElement)
						{
							xmlReader.ReadStartElement();
							var value = LoadValue(xmlReader);

							obj.Add(name, value, metadata);
						}
						else
						{
							obj.Add(name, string.Empty, metadata);
						}

						break;

					case XmlNodeType.EndElement:
						xmlReader.ReadEndElement();

						return obj;

					case XmlNodeType.Text:
						var text = xmlReader.Value;
						xmlReader.Read();

						return text;

					case XmlNodeType.None:
						return obj;

					default:
						Infrastructure.UnexpectedValue();
						break;
				}
			} while (xmlReader.Read());

			return obj;
		}

		private static DataModelList? GetMetaData(XmlReader xmlReader)
		{
			var elementPrefix = xmlReader.Prefix;
			var elementNs = xmlReader.NamespaceURI;

			if (elementPrefix.Length == 0 && elementNs.Length == 0 && !xmlReader.HasAttributes)
			{
				return null;
			}

			var metadata = new DataModelArray { elementPrefix, elementNs };

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