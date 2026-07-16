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
using Xtate.DataModel.XPath.Internal;
using Xtate.DataTypes;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class XPathObjectCoverageTest
{
	[TestMethod]
	public void ScalarObjectsExposeTypeObjectAndXPathConversions()
	{
		var text = new XPathObject("12");
		var number = new XPathObject(3.75d);
		var booleanText = new XPathObject("true");
		var trueValue = new XPathObject(true);
		var falseValue = new XPathObject(false);

		Assert.AreEqual(XPathObjectType.String, text.Type);
		Assert.AreEqual(expected: "12", text.ToObject());
		Assert.AreEqual(expected: 12, text.AsInteger());
		Assert.IsTrue(booleanText.AsBoolean());
		Assert.AreEqual(XPathObjectType.Number, number.Type);
		Assert.AreEqual(expected: 3, number.AsInteger());
		Assert.AreEqual(expected: "3.75", number.AsString());
		Assert.IsTrue(number.AsBoolean());
		Assert.AreEqual(XPathObjectType.Boolean, trueValue.Type);
		Assert.AreEqual(expected: 1, trueValue.AsInteger());
		Assert.AreEqual(expected: "true", trueValue.AsString());
		Assert.IsTrue(trueValue.AsBoolean());
		Assert.AreEqual(expected: 0, falseValue.AsInteger());
		Assert.IsFalse(falseValue.AsBoolean());

		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => _ = new XPathObject(new object()));
		Assert.ThrowsExactly<InvalidCastException>([ExcludeFromCodeCoverage]() => text.AsIterator());
	}

	[TestMethod]
	public void EmptyAndTextNodeIteratorsExposeFirstValueConcatenationAndClones()
	{
		var list = new DataModelList { ["first"] = "1", ["second"] = "2" };
		var navigator = new DataModelXPathNavigator(list);
		var empty = new XPathObject(navigator.Select("missing/text()"));
		var single = new XPathObject(navigator.Select("first/text()"));
		var multiple = new XPathObject(navigator.Select("first/text() | second/text()"));

		Assert.AreEqual(XPathObjectType.NodeSet, empty.Type);
		Assert.IsNull(empty.ToObject());
		Assert.AreEqual(string.Empty, empty.AsString());
		Assert.AreEqual(expected: "1", single.ToObject());
		Assert.AreEqual(expected: 1, single.AsInteger());
		Assert.IsTrue(single.AsBoolean());
		Assert.AreEqual(expected: "12", multiple.ToObject());
		Assert.AreEqual(expected: "1", multiple.AsString());

		var firstClone = multiple.AsIterator();
		var secondClone = multiple.AsIterator();
		Assert.IsTrue(firstClone.MoveNext());
		Assert.IsTrue(secondClone.MoveNext());
		Assert.AreNotSame(firstClone, secondClone);
	}

	[TestMethod]
	public void ElementIteratorConvertsNodesAndMetadataToWritableDataModelList()
	{
		const string xml = "<root xmlns:p='urn:test'><p:first attr='value'>one</p:first><second>two</second></root>";
		var data = XmlConverter.FromXml(xml);
		var navigator = new DataModelXPathNavigator(data);
		var namespaces = new XmlNamespaceManager(new System.Xml.NameTable());
		namespaces.AddNamespace(prefix: "p", uri: "urn:test");
		var expression = navigator.Compile("root/p:first | root/second");
		expression.SetContext(namespaces);
		var iterator = navigator.Select(expression);
		var xpathObject = new XPathObject(iterator);

		var result = (DataModelList) xpathObject.ToObject()!;
		Assert.AreEqual(expected: "one", result["first"].AsString());
		Assert.AreEqual(expected: "two", result["second"].AsString());
		Assert.IsNotNull(result.GetMetadata("first"));
		result["first"] = "changed";
		Assert.AreEqual(expected: "changed", result["first"].AsString());
	}

	[TestMethod]
	public void NavigatorCoversIdentitySiblingAttributeMutationAndUnsupportedIdOperations()
	{
		var list = new DataModelList { "first", "second" };
		var navigator = new DataModelXPathNavigator(list);

		Assert.AreEqual(string.Empty, navigator.BaseURI);
		Assert.IsFalse(navigator.MoveToId("missing"));
		Assert.IsFalse(navigator.MoveToNext());
		Assert.IsFalse(navigator.MoveToPrevious());
		Assert.IsFalse(navigator.MoveToNextAttribute());
		navigator.SetValue("ignored-at-root");
		Assert.AreEqual(expected: "first", list[0].AsString());

		Assert.IsTrue(navigator.MoveToFirstChild());
		navigator.SetValue("changed");
		Assert.AreEqual(expected: "changed", list[0].AsString());
		Assert.IsTrue(navigator.MoveToNext());
		Assert.AreEqual(expected: "second", navigator.Value);
		Assert.IsTrue(navigator.MoveToPrevious());
		Assert.AreEqual(expected: "changed", navigator.Value);

		var xmlData = XmlConverter.FromXml("<root first='1' second='2'/>");
		var element = new DataModelXPathNavigator(xmlData).SelectSingleNode("root")!;
		Assert.IsTrue(element.MoveToFirstAttribute());
		Assert.IsTrue(element.MoveToNextAttribute());
		Assert.IsFalse(element.MoveToNextAttribute());
	}
}