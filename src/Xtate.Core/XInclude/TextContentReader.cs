// Copyright © 2019-2024 Sergii Artemenko
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

using System.Xml;

namespace Xtate.XInclude;

internal sealed class TextContentReader(Uri uri, string content) : XmlReader
{
	private ReadState _readState = ReadState.Initial;

	public override int AttributeCount => 0;

	public override string BaseURI { get; } = uri.ToString();

	public override int Depth => _readState == ReadState.Interactive ? 1 : 0;

	public override bool EOF => _readState == ReadState.EndOfFile;

	public override bool HasValue => _readState == ReadState.Interactive;

	public override bool IsDefault => false;

	public override bool IsEmptyElement => false;

	public override string this[int index] => string.Empty;

	public override string this[string name] => string.Empty;

	public override string this[string name, string? namespaceURI] => string.Empty;

	public override string LocalName => string.Empty;

	public override string Name => string.Empty;

	public override string NamespaceURI => string.Empty;

	public override XmlNameTable NameTable => throw new NotSupportedException();

	public override XmlNodeType NodeType => _readState == ReadState.Interactive ? XmlNodeType.Text : XmlNodeType.None;

	public override string Prefix => string.Empty;

	public override char QuoteChar => '"';

	public override ReadState ReadState => _readState;

	public override string Value => _readState == ReadState.Interactive ? content : string.Empty;

	public override string XmlLang => string.Empty;

	public override XmlSpace XmlSpace => XmlSpace.None;

	public override void Close() => _readState = ReadState.Closed;

	public override string GetAttribute(int index) => throw new NotSupportedException();

	public override string? GetAttribute(string name) => default;

	public override string? GetAttribute(string name, string? namespaceURI) => default;

	public override string? LookupNamespace(string prefix) => default;

	public override void MoveToAttribute(int index) { }

	public override bool MoveToAttribute(string name) => false;

	public override bool MoveToAttribute(string name, string? namespaceURI) => false;

	public override bool MoveToElement() => false;

	public override bool MoveToFirstAttribute() => false;

	public override bool MoveToNextAttribute() => false;

	public override bool ReadAttributeValue() => false;

	public override string ReadInnerXml() => _readState == ReadState.Interactive ? content : string.Empty;

	public override string ReadOuterXml() => _readState == ReadState.Interactive ? content : string.Empty;

	public override string ReadString() => _readState == ReadState.Interactive ? content : string.Empty;

	public override void ResolveEntity() { }

	public override bool Read()
	{
		switch (_readState)
		{
			case ReadState.Initial:
				_readState = ReadState.Interactive;
				return true;
			case ReadState.Interactive:
				_readState = ReadState.EndOfFile;
				return false;
			default:
				return false;
		}
	}
}