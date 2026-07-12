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
		Assert.AreEqual("plain message", messageException.Message);

		var innerException = new InvalidOperationException("inner");
		var wrappingException = new XIncludeException("outer", innerException);
		Assert.AreEqual("outer", wrappingException.Message);
		Assert.AreSame(innerException, wrappingException.InnerException);

		var nullReaderException = new XIncludeException("without reader", xmlReader: null);
		Assert.AreEqual("without reader", nullReaderException.Message);
		Assert.IsNull(nullReaderException.Location);
		Assert.IsNull(nullReaderException.LineNumber);
		Assert.IsNull(nullReaderException.LinePosition);
	}

	[TestMethod]
	public void ReaderConstructorCapturesBaseUriAndLineInfoInMessageAndProperties()
	{
		using var reader = CreateReader("<root>\r\n  <child />\r\n</root>", "file:///tmp/test.scxml");
		while (reader.Read() && reader is not { NodeType: XmlNodeType.Element, LocalName: "child" }) { }

		var exception = new XIncludeException("include failed", reader);

		Assert.AreEqual("file:///tmp/test.scxml", exception.Location);
		Assert.AreEqual(2, exception.LineNumber);
		Assert.AreEqual(4, exception.LinePosition);
		StringAssert.Contains(exception.Message, "include failed");
		StringAssert.Contains(exception.Message, "file:///tmp/test.scxml");
		StringAssert.Contains(exception.Message, "2");
		StringAssert.Contains(exception.Message, "4");
	}

	[TestMethod]
	public void ReaderConstructorUsesOnlyAvailableLocationDetails()
	{
		using var readerWithLineInfoOnly = CreateReader("<root />", baseUri: string.Empty);
		readerWithLineInfoOnly.Read();

		var lineInfoException = new XIncludeException("line only", readerWithLineInfoOnly);

		Assert.AreEqual(string.Empty, lineInfoException.Location);
		Assert.AreEqual(1, lineInfoException.LineNumber);
		Assert.AreEqual(2, lineInfoException.LinePosition);
		StringAssert.Contains(lineInfoException.Message, "line only");
		StringAssert.Contains(lineInfoException.Message, "1");
		StringAssert.Contains(lineInfoException.Message, "2");

		using var readerWithoutLineInfo = XmlReader.Create(new StringReader("<root />"), new XmlReaderSettings { IgnoreWhitespace = true }, "file:///tmp/no-line.scxml");
		var locationOnlyException = new XIncludeException("location only", new NonLineInfoXmlReader(readerWithoutLineInfo));

		Assert.AreEqual("file:///tmp/no-line.scxml", locationOnlyException.Location);
		Assert.IsNull(locationOnlyException.LineNumber);
		Assert.IsNull(locationOnlyException.LinePosition);
		StringAssert.Contains(locationOnlyException.Message, "location only");
		StringAssert.Contains(locationOnlyException.Message, "file:///tmp/no-line.scxml");
	}

	private static XmlReader CreateReader(string xml, string baseUri)
	{
		return XmlReader.Create(new StringReader(xml), new XmlReaderSettings { IgnoreWhitespace = false }, baseUri);
	}

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

		public override string? GetAttribute(int i) => innerReader.GetAttribute(i);

		public override string LookupNamespace(string prefix) => innerReader.LookupNamespace(prefix);

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
