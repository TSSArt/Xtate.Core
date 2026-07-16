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
using Xtate.Scxml.Internal;

namespace Xtate.Test.UnitTests.Scxml;

[TestClass]
public class TextContentReaderCoverageTest
{
	[TestMethod]
	public void EmptyXmlSurfaceReturnsFixedMetadataAndSupportsNoOpMethods()
	{
		using var reader = new TextContentReader(new Uri("urn:text"), content: "body");

		Assert.IsFalse(reader.IsDefault);
		Assert.IsFalse(reader.IsEmptyElement);
		Assert.AreEqual(string.Empty, reader[index: 0]);
		Assert.AreEqual(string.Empty, reader[name: "attribute"]);
		Assert.AreEqual(string.Empty, reader[name: "attribute", namespaceURI: "urn:namespace"]);
		Assert.AreEqual(string.Empty, reader.LocalName);
		Assert.AreEqual(string.Empty, reader.Name);
		Assert.AreEqual(string.Empty, reader.NamespaceURI);
		Assert.AreEqual(string.Empty, reader.Prefix);
		Assert.AreEqual(expected: '"', reader.QuoteChar);
		Assert.AreEqual(string.Empty, reader.XmlLang);
		Assert.AreEqual(XmlSpace.None, reader.XmlSpace);

		reader.MoveToAttribute(index: 0);
		reader.ResolveEntity();
	}

	[TestMethod]
	public void ReadsOnlyOnceAndReturnsEmptyContentOutsideInteractiveState()
	{
		using var reader = new TextContentReader(new Uri("urn:text"), content: "body");

		Assert.AreEqual(expected: 0, reader.Depth);
		Assert.AreEqual(string.Empty, reader.ReadInnerXml());
		Assert.AreEqual(string.Empty, reader.ReadOuterXml());
		Assert.AreEqual(string.Empty, reader.ReadString());
		Assert.IsTrue(reader.Read());
		Assert.AreEqual(expected: 1, reader.Depth);
		Assert.IsFalse(reader.Read());
		Assert.AreEqual(expected: 0, reader.Depth);
		Assert.AreEqual(string.Empty, reader.ReadInnerXml());
		Assert.AreEqual(string.Empty, reader.ReadOuterXml());
		Assert.AreEqual(string.Empty, reader.ReadString());
		Assert.IsFalse(reader.Read());
	}

	[TestMethod]
	public async Task AsyncValueTracksInteractiveReadState()
	{
		using var reader = new TextContentReader(new Uri("urn:text"), content: "body");

		Assert.AreEqual(string.Empty, await reader.GetValueAsync());
		Assert.IsTrue(await reader.ReadAsync());
		Assert.AreEqual(expected: "body", await reader.GetValueAsync());
		Assert.IsFalse(await reader.ReadAsync());
		Assert.AreEqual(string.Empty, await reader.GetValueAsync());
	}
}