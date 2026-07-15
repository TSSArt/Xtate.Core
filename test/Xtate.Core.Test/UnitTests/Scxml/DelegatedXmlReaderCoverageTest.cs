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

using System.Xml;
using System.Xml.Schema;
using System.IO;
using Xtate.Scxml.Services;

namespace Xtate.Test.UnitTests.Scxml;

[TestClass]
public class DelegatedXmlReaderCoverageTest
{
	[TestMethod]
	public async Task ReaderForwardsPropertiesIndexersNavigationLineInfoAndAsyncOperations()
	{
		var nameTable = new System.Xml.NameTable();
		var settings = new XmlReaderSettings { Async = true };
		var schemaInfo = Mock.Of<IXmlSchemaInfo>();
		var inner = new Mock<XmlReader>();
		var lineInfo = inner.As<IXmlLineInfo>();
		inner.SetupGet(static reader => reader.BaseURI).Returns("urn:base");
		inner.SetupGet(static reader => reader.AttributeCount).Returns(2);
		inner.SetupGet(static reader => reader.Depth).Returns(3);
		inner.SetupGet(static reader => reader.EOF).Returns(true);
		inner.SetupGet(static reader => reader.HasValue).Returns(true);
		inner.SetupGet(static reader => reader.IsDefault).Returns(true);
		inner.SetupGet(static reader => reader.IsEmptyElement).Returns(true);
		inner.SetupGet(reader => reader[1]).Returns("indexed");
		inner.SetupGet(reader => reader["name"]).Returns("named");
		inner.SetupGet(reader => reader["local", "urn:test"]).Returns("qualified");
		inner.SetupGet(static reader => reader.LocalName).Returns("local");
		inner.SetupGet(static reader => reader.Name).Returns("p:local");
		inner.SetupGet(static reader => reader.NamespaceURI).Returns("urn:test");
		inner.SetupGet(reader => reader.NameTable).Returns(nameTable);
		inner.SetupGet(static reader => reader.NodeType).Returns(XmlNodeType.Element);
		inner.SetupGet(static reader => reader.Prefix).Returns("p");
		inner.SetupGet(static reader => reader.QuoteChar).Returns('\'');
		inner.SetupGet(static reader => reader.ReadState).Returns(ReadState.Interactive);
		inner.SetupGet(static reader => reader.Value).Returns("value");
		inner.SetupGet(static reader => reader.XmlLang).Returns("en");
		inner.SetupGet(static reader => reader.XmlSpace).Returns(XmlSpace.Preserve);
		inner.SetupGet(reader => reader.Settings).Returns(settings);
		inner.SetupGet(static reader => reader.ValueType).Returns(typeof(string));
		inner.SetupGet(static reader => reader.HasAttributes).Returns(true);
		inner.SetupGet(reader => reader.SchemaInfo).Returns(schemaInfo);
		lineInfo.Setup(static info => info.HasLineInfo()).Returns(true);
		lineInfo.SetupGet(static info => info.LineNumber).Returns(7);
		lineInfo.SetupGet(static info => info.LinePosition).Returns(11);
		inner.Setup(static reader => reader.Read()).Returns(true);
		inner.Setup(static reader => reader.ReadAsync()).ReturnsAsync(true);
		inner.Setup(static reader => reader.GetValueAsync()).ReturnsAsync("async-value");
		inner.Setup(static reader => reader.GetAttribute(1)).Returns("attribute-index");
		inner.Setup(static reader => reader.GetAttribute("name")).Returns("attribute-name");
		inner.Setup(static reader => reader.GetAttribute("local", "urn:test")).Returns("attribute-qualified");
		inner.Setup(static reader => reader.LookupNamespace("p")).Returns("urn:test");
		inner.Setup(static reader => reader.MoveToAttribute("name")).Returns(true);
		inner.Setup(static reader => reader.MoveToAttribute("local", "urn:test")).Returns(true);
		inner.Setup(static reader => reader.MoveToElement()).Returns(true);
		inner.Setup(static reader => reader.MoveToFirstAttribute()).Returns(true);
		inner.Setup(static reader => reader.MoveToNextAttribute()).Returns(true);
		inner.Setup(static reader => reader.ReadAttributeValue()).Returns(true);
		var reader = new TestDelegatedXmlReader(inner.Object);

		Assert.AreEqual("urn:base", reader.BaseURI);
		Assert.AreEqual(expected: 2, reader.AttributeCount);
		Assert.AreEqual(expected: 3, reader.Depth);
		Assert.IsTrue(reader.EOF);
		Assert.IsTrue(reader.HasValue);
		Assert.IsTrue(reader.IsDefault);
		Assert.IsTrue(reader.IsEmptyElement);
		Assert.AreEqual("indexed", reader[1]);
		Assert.AreEqual("named", reader["name"]);
		Assert.AreEqual("qualified", reader["local", "urn:test"]);
		Assert.AreEqual("local", reader.LocalName);
		Assert.AreEqual("p:local", reader.Name);
		Assert.AreEqual("urn:test", reader.NamespaceURI);
		Assert.AreSame(nameTable, reader.NameTable);
		Assert.AreEqual(XmlNodeType.Element, reader.NodeType);
		Assert.AreEqual("p", reader.Prefix);
		Assert.AreEqual(expected: '\'', reader.QuoteChar);
		Assert.AreEqual(ReadState.Interactive, reader.ReadState);
		Assert.AreEqual("value", reader.Value);
		Assert.AreEqual("en", reader.XmlLang);
		Assert.AreEqual(XmlSpace.Preserve, reader.XmlSpace);
		Assert.AreSame(settings, reader.Settings);
		Assert.AreEqual(typeof(string), reader.ValueType);
		Assert.IsTrue(reader.HasAttributes);
		Assert.AreSame(schemaInfo, reader.SchemaInfo);
		Assert.IsTrue(reader.HasLineInfo());
		Assert.AreEqual(expected: 7, reader.LineNumber);
		Assert.AreEqual(expected: 11, reader.LinePosition);
		Assert.IsTrue(reader.Read());
		Assert.IsTrue(await reader.ReadAsync());
		Assert.AreEqual("async-value", await reader.GetValueAsync());
		Assert.AreEqual("attribute-index", reader.GetAttribute(1));
		Assert.AreEqual("attribute-name", reader.GetAttribute("name"));
		Assert.AreEqual("attribute-qualified", reader.GetAttribute("local", "urn:test"));
		Assert.AreEqual("urn:test", reader.LookupNamespace("p"));
		reader.MoveToAttribute(1);
		Assert.IsTrue(reader.MoveToAttribute("name"));
		Assert.IsTrue(reader.MoveToAttribute("local", "urn:test"));
		Assert.IsTrue(reader.MoveToElement());
		Assert.IsTrue(reader.MoveToFirstAttribute());
		Assert.IsTrue(reader.MoveToNextAttribute());
		Assert.IsTrue(reader.ReadAttributeValue());
		reader.ResolveEntity();
		reader.Close();

		inner.Verify(static value => value.MoveToAttribute(1), Times.Once);
		inner.Verify(static value => value.ResolveEntity(), Times.Once);
		inner.Verify(static value => value.Close(), Times.Once);
	}

	[TestMethod]
	public void LineInformationFallsBackWhenInnerReaderDoesNotImplementInterface()
	{
		var reader = new TestDelegatedXmlReader(Mock.Of<XmlReader>());

		Assert.IsFalse(reader.HasLineInfo());
		Assert.AreEqual(expected: 0, reader.LineNumber);
		Assert.AreEqual(expected: 0, reader.LinePosition);
	}

	[TestMethod]
	public void XmlBaseReaderTracksNestedAndEmptyElementBaseUrisSynchronously()
	{
		const string source = "<root xml:base='a/'><child/><nested xml:base='../b/'></nested></root>";
		using var inner = XmlReader.Create(new StringReader(source), new XmlReaderSettings(), "https://example.test/root/document.scxml");
		using var reader = new XmlBaseReader(inner) { XmlResolver = new XmlUrlResolver() };
		var baseUriProperty = typeof(XmlBaseReader).GetProperty(nameof(XmlReader.BaseURI))!;

		Assert.AreEqual("https://example.test/root/document.scxml", baseUriProperty.GetValue(reader));
		Assert.IsTrue(reader.Read());
		Assert.AreEqual("https://example.test/root/a/", baseUriProperty.GetValue(reader));
		Assert.IsTrue(reader.Read());
		Assert.AreEqual("https://example.test/root/a/", reader.BaseURI);
		Assert.IsTrue(reader.Read());
		Assert.AreEqual("https://example.test/root/b/", reader.BaseURI);
		Assert.IsTrue(reader.Read());
		Assert.AreEqual("https://example.test/root/b/", reader.BaseURI);
		Assert.IsTrue(reader.Read());
		Assert.AreEqual("https://example.test/root/a/", reader.BaseURI);
		Assert.IsFalse(reader.Read());
		Assert.AreEqual("https://example.test/root/document.scxml", baseUriProperty.GetValue(reader));
	}

	[TestMethod]
	public async Task XmlBaseReaderTracksEmptyRootAndMissingBaseAsynchronously()
	{
		const string source = "<root><empty xml:base='relative/'/></root>";
		var settings = new XmlReaderSettings { Async = true };
		using var inner = XmlReader.Create(new StringReader(source), settings, "https://example.test/root/document.scxml");
		using var reader = new XmlBaseReader(inner) { XmlResolver = new XmlUrlResolver() };

		Assert.IsTrue(await reader.ReadAsync());
		Assert.AreEqual("https://example.test/root/document.scxml", reader.BaseURI);
		Assert.IsTrue(await reader.ReadAsync());
		Assert.AreEqual("https://example.test/root/relative/", reader.BaseURI);
		Assert.IsTrue(await reader.ReadAsync());
		Assert.AreEqual("https://example.test/root/document.scxml", reader.BaseURI);
		Assert.IsFalse(await reader.ReadAsync());
	}

	[TestMethod]
	public void XmlBaseReaderShouldResolveXmlBaseAttributeValue()
	{
		using var inner = XmlReader.Create(new StringReader("<root xml:base='relative/'/>"), new XmlReaderSettings(), "https://example.test/root/document.scxml");
		using var reader = new XmlBaseReader(inner) { XmlResolver = new XmlUrlResolver() };

		Assert.IsTrue(reader.Read());
		Assert.AreEqual("https://example.test/root/relative/", reader.BaseURI);
	}

	private sealed class TestDelegatedXmlReader(XmlReader innerReader) : DelegatedXmlReader(innerReader);
}
