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

using System.IO;
using System.Xml;
using Xtate.Scxml;

namespace Xtate.Test.UnitTests.Scxml;

[TestClass]
public class XIncludeExceptionCoverageTest
{
	[TestMethod]
	public void ConstructorsPreserveMessageInnerExceptionAndNullReaderState()
	{
		var defaultException = new XIncludeException();
		Assert.IsNull(defaultException.LineNumber);
		Assert.IsNull(defaultException.LinePosition);
		Assert.IsNull(defaultException.Location);

		var messageException = new XIncludeException("plain message");
		Assert.AreEqual(expected: "plain message", messageException.Message);

		var innerException = new InvalidOperationException("inner");
		var wrappingException = new XIncludeException(message: "outer", innerException);
		Assert.AreEqual(expected: "outer", wrappingException.Message);
		Assert.AreSame(innerException, wrappingException.InnerException);

		var nullReaderException = new XIncludeException(message: "without reader", xmlReader: null);
		Assert.AreEqual(expected: "without reader", nullReaderException.Message);
		Assert.IsNull(nullReaderException.Location);
		Assert.IsNull(nullReaderException.LineNumber);
		Assert.IsNull(nullReaderException.LinePosition);
	}

	[TestMethod]
	public void ReaderConstructorCapturesBaseUriAndLineInfoInMessageAndProperties()
	{
		using var reader = CreateReader(xml: "<root>\r\n  <child />\r\n</root>", baseUri: "file:///tmp/test.scxml");

		while (reader.Read())
		{
			if (reader is { NodeType: XmlNodeType.Element, LocalName: "child" })
			{
				break;
			}
		}

		var exception = new XIncludeException(message: "include failed", reader);

		Assert.AreEqual(expected: "file:///tmp/test.scxml", exception.Location);
		Assert.AreEqual(expected: 2, exception.LineNumber);
		Assert.AreEqual(expected: 4, exception.LinePosition);
		Assert.Contains(substring: "include failed", exception.Message);
		Assert.Contains(substring: "file:///tmp/test.scxml", exception.Message);
		Assert.Contains(substring: "2", exception.Message);
		Assert.Contains(substring: "4", exception.Message);
	}

	[TestMethod]
	public void ReaderConstructorUsesOnlyAvailableLocationDetails()
	{
		using var readerWithLineInfoOnly = CreateReader(xml: "<root />", string.Empty);
		readerWithLineInfoOnly.Read();

		var lineInfoException = new XIncludeException(message: "line only", readerWithLineInfoOnly);

		Assert.AreEqual(string.Empty, lineInfoException.Location);
		Assert.AreEqual(expected: 1, lineInfoException.LineNumber);
		Assert.AreEqual(expected: 2, lineInfoException.LinePosition);
		Assert.Contains(substring: "line only", lineInfoException.Message);
		Assert.Contains(substring: "1", lineInfoException.Message);
		Assert.Contains(substring: "2", lineInfoException.Message);

		using var readerWithoutLineInfo = XmlReader.Create(new StringReader("<root />"), new XmlReaderSettings { IgnoreWhitespace = true }, baseUri: "file:///tmp/no-line.scxml");
		var locationOnlyException = new XIncludeException(message: "location only", new NonLineInfoXmlReader(readerWithoutLineInfo));

		Assert.AreEqual(expected: "file:///tmp/no-line.scxml", locationOnlyException.Location);
		Assert.IsNull(locationOnlyException.LineNumber);
		Assert.IsNull(locationOnlyException.LinePosition);
		Assert.Contains(substring: "location only", locationOnlyException.Message);
		Assert.Contains(substring: "file:///tmp/no-line.scxml", locationOnlyException.Message);
	}

	private static XmlReader CreateReader(string xml, string baseUri) => XmlReader.Create(new StringReader(xml), new XmlReaderSettings { IgnoreWhitespace = false }, baseUri);

	[ExcludeFromCodeCoverage]
	private sealed class NonLineInfoXmlReader(XmlReader innerReader) : XmlReader
	{
		public override int AttributeCount => innerReader.AttributeCount;

		public override string BaseURI => innerReader.BaseURI;

		public override int Depth => innerReader.Depth;

		public override bool EOF => innerReader.EOF;

		public override bool HasValue => innerReader.HasValue;

		public override bool IsEmptyElement => innerReader.IsEmptyElement;

		public override string LocalName => innerReader.LocalName;

		public override string NamespaceURI => innerReader.NamespaceURI;

		public override XmlNameTable NameTable => innerReader.NameTable;

		public override XmlNodeType NodeType => innerReader.NodeType;

		public override string Prefix => innerReader.Prefix;

		public override ReadState ReadState => innerReader.ReadState;

		public override string Value => innerReader.Value;

		public override string? GetAttribute(string name) => innerReader.GetAttribute(name);

		public override string? GetAttribute(string name, string? namespaceURI) => innerReader.GetAttribute(name, namespaceURI);

		public override string GetAttribute(int i) => innerReader.GetAttribute(i);

		public override string? LookupNamespace(string prefix) => innerReader.LookupNamespace(prefix);

		public override bool MoveToAttribute(string name) => innerReader.MoveToAttribute(name);

		public override bool MoveToAttribute(string name, string? ns) => innerReader.MoveToAttribute(name, ns);

		public override void MoveToAttribute(int i) => innerReader.MoveToAttribute(i);

		public override bool MoveToElement() => innerReader.MoveToElement();

		public override bool MoveToFirstAttribute() => innerReader.MoveToFirstAttribute();

		public override bool MoveToNextAttribute() => innerReader.MoveToNextAttribute();

		public override bool Read() => innerReader.Read();

		public override bool ReadAttributeValue() => innerReader.ReadAttributeValue();

		public override void ResolveEntity() => innerReader.ResolveEntity();
	}
}