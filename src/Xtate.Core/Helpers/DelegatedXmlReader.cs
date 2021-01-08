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
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace Xtate.Core
{
	internal abstract class DelegatedXmlReader : XmlReader, IXmlLineInfo
	{
		protected DelegatedXmlReader(XmlReader innerReader) => InnerReader = innerReader ?? throw new ArgumentNullException(nameof(innerReader));

		protected XmlReader InnerReader { get; set; }

		public override string? BaseURI => InnerReader.BaseURI;

		public override int AttributeCount => InnerReader.AttributeCount;

		public override int Depth => InnerReader.Depth;

		public override bool EOF => InnerReader.EOF;

		public override bool HasValue => InnerReader.HasValue;

		public override bool IsDefault => InnerReader.IsDefault;

		public override bool IsEmptyElement => InnerReader.IsEmptyElement;

		public override string this[int i] => InnerReader[i]!;

		public override string? this[string name] => InnerReader[name];

		public override string? this[string name, string? namespaceUri] => InnerReader[name, namespaceUri!];

		public override string LocalName => InnerReader.LocalName;

		public override string Name => InnerReader.Name;

		public override string NamespaceURI => InnerReader.NamespaceURI;

		public override XmlNameTable NameTable => InnerReader.NameTable!;

		public override XmlNodeType NodeType => InnerReader.NodeType;

		public override string Prefix => InnerReader.Prefix;

		public override char QuoteChar => InnerReader.QuoteChar;

		public override ReadState ReadState => InnerReader.ReadState;

		public override string Value => InnerReader.Value;

		public override string XmlLang => InnerReader.XmlLang;

		public override XmlSpace XmlSpace => InnerReader.XmlSpace;

		public override XmlReaderSettings? Settings => InnerReader.Settings;

		public override Type ValueType => InnerReader.ValueType;

		public override bool HasAttributes => InnerReader.HasAttributes;

		public override IXmlSchemaInfo? SchemaInfo => InnerReader.SchemaInfo;

	#region Interface IXmlLineInfo

		public virtual bool HasLineInfo() => InnerReader is IXmlLineInfo lineInfo && lineInfo.HasLineInfo();

		public virtual int LineNumber => InnerReader is IXmlLineInfo lineInfo ? lineInfo.LineNumber : 0;

		public virtual int LinePosition => InnerReader is IXmlLineInfo lineInfo ? lineInfo.LinePosition : 0;

	#endregion

		public override bool Read() => InnerReader.Read();

		public override Task<bool> ReadAsync() => InnerReader.ReadAsync();

		public override Task<string> GetValueAsync() => InnerReader.GetValueAsync();

		public override void Close() => InnerReader.Close();

		public override string GetAttribute(int i) => InnerReader.GetAttribute(i)!;

		public override string? GetAttribute(string name) => InnerReader.GetAttribute(name);

		public override string? GetAttribute(string localName, string? namespaceUri) => InnerReader.GetAttribute(localName, namespaceUri!);

		public override string? LookupNamespace(string prefix) => InnerReader.LookupNamespace(prefix);

		public override void MoveToAttribute(int i) => InnerReader.MoveToAttribute(i);

		public override bool MoveToAttribute(string name) => InnerReader.MoveToAttribute(name);

		public override bool MoveToAttribute(string localName, string? namespaceUri) => InnerReader.MoveToAttribute(localName, namespaceUri!);

		public override bool MoveToElement() => InnerReader.MoveToElement();

		public override bool MoveToFirstAttribute() => InnerReader.MoveToFirstAttribute();

		public override bool MoveToNextAttribute() => InnerReader.MoveToNextAttribute();

		public override bool ReadAttributeValue() => InnerReader.ReadAttributeValue();

		public override void ResolveEntity() => InnerReader.ResolveEntity();
	}
}