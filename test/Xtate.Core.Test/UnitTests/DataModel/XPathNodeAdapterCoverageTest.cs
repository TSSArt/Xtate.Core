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

using System.Xml.XPath;
using Xtate.DataModel.XPath.Internal;
using Xtate.DataTypes;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class XPathNodeAdapterCoverageTest
{
	[TestMethod]
	public void NodeAdapterDefaultsReturnEmptyValuesAndNoRelatedNodes()
	{
		var adapter = new DefaultNodeAdapter();
		var node = new DataModelXPathNavigator.Node(new DataModelValue("value"), adapter);
		var related = node;
		Span<char> buffer = stackalloc char[8];

		Assert.AreEqual(XPathNodeType.Root, node.GetNodeType());
		Assert.AreEqual(string.Empty, node.GetValue());
		Assert.AreEqual(expected: 0, adapter.GetBufferSizeForValue(node));
		Assert.AreEqual(expected: 0, adapter.WriteValueToSpan(node, buffer));
		Assert.AreEqual(string.Empty, node.GetLocalName());
		Assert.AreEqual(string.Empty, node.GetName());
		Assert.AreEqual(string.Empty, node.GetPrefix());
		Assert.AreEqual(string.Empty, node.GetNamespaceUri());
		Assert.IsTrue(node.IsEmptyElement());
		Assert.IsFalse(node.GetFirstChild(out related));
		Assert.IsFalse(node.GetNextChild(ref related));
		Assert.IsFalse(node.GetPreviousChild(ref related));
		Assert.IsFalse(node.GetFirstAttribute(out related));
		Assert.IsFalse(node.GetNextAttribute(ref related));
		Assert.IsFalse(node.GetFirstNamespace(out related));
		Assert.IsFalse(node.GetNextNamespace(ref related));
	}

	[TestMethod]
	public void TypeAttributeNodeAdapterExposesXPathTypeMetadataAndValue()
	{
		var adapter = new TypeAttributeNodeAdapter();
		var node = new DataModelXPathNavigator.Node(new DataModelValue("number"), adapter);

		Assert.AreEqual(XPathNodeType.Attribute, node.GetNodeType());
		Assert.AreEqual(XmlConverter.TypeAttributeName, node.GetLocalName());
		Assert.AreEqual(XmlConverter.TypeAttributeName, node.GetName());
		Assert.AreEqual(XmlConverter.XPathElementNamespace, node.GetNamespaceUri());
		Assert.AreEqual("number", node.GetValue());
	}

	[TestMethod]
	public void XmlnsXmlNodeAdapterExposesBuiltInXmlNamespace()
	{
		var adapter = new XmlnsXmlNodeAdapter();
		var node = new DataModelXPathNavigator.Node(DataModelValue.Undefined, adapter);

		Assert.AreEqual(XPathNodeType.Namespace, node.GetNodeType());
		Assert.AreEqual("xml", node.GetLocalName());
		Assert.AreEqual("xml", node.GetName());
		Assert.AreEqual("http://www.w3.org/XML/1998/namespace", node.GetValue());
	}

	[TestMethod]
	public void NamespaceNodeAdapterUsesParentPropertyAndValue()
	{
		var adapter = new NamespaceNodeAdapter();
		var node = new DataModelXPathNavigator.Node(new DataModelValue("urn:test"), adapter, parentProperty: "prefix");

		Assert.AreEqual(XPathNodeType.Namespace, node.GetNodeType());
		Assert.AreEqual("prefix", node.GetLocalName());
		Assert.AreEqual("prefix", node.GetName());
		Assert.AreEqual("urn:test", node.GetValue());
	}

	[TestMethod]
	public void AdapterFactorySelectsAdaptersForEverySupportedValueKind()
	{
		DataModelValue[] simpleValues =
		[
			DataModelValue.Undefined,
			DataModelValue.Null,
			new("text"),
			new(12D),
			new(true),
			new(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc))
		];

		foreach (var value in simpleValues)
		{
			Assert.IsInstanceOfType<SimpleTypeNodeAdapter>(AdapterFactory.GetDefaultAdapter(value));
		}

		Assert.IsInstanceOfType<ListNodeAdapter>(AdapterFactory.GetDefaultAdapter(new DataModelValue([])));
		Assert.IsNull(AdapterFactory.GetSimpleTypeAdapter(DataModelValue.Undefined));
		Assert.IsNull(AdapterFactory.GetSimpleTypeAdapter(DataModelValue.Null));
		Assert.IsNull(AdapterFactory.GetSimpleTypeAdapter(new DataModelValue(string.Empty)));
		Assert.IsInstanceOfType<SimpleTypeNodeAdapter>(AdapterFactory.GetSimpleTypeAdapter(new DataModelValue("text")));
		Assert.IsInstanceOfType<SimpleTypeNodeAdapter>(AdapterFactory.GetSimpleTypeAdapter(new DataModelValue(1D)));
		Assert.IsInstanceOfType<SimpleTypeNodeAdapter>(AdapterFactory.GetSimpleTypeAdapter(new DataModelValue(false)));
		Assert.IsInstanceOfType<SimpleTypeNodeAdapter>(AdapterFactory.GetSimpleTypeAdapter(new DataModelValue(DateTime.UnixEpoch)));

		var items = new DataModelList
					{
						DataModelValue.Undefined,
						DataModelValue.Null,
						"text",
						1D,
						true,
						DateTime.UnixEpoch,
						new DataModelList()
					};
		Type[] expectedTypes =
		[
			typeof(ItemNodeAdapter), typeof(ItemNodeAdapter),
			typeof(SimpleTypeItemNodeAdapter), typeof(SimpleTypeItemNodeAdapter),
			typeof(SimpleTypeItemNodeAdapter), typeof(SimpleTypeItemNodeAdapter),
			typeof(ListItemNodeAdapter)
		];

		for (var index = 0; index < expectedTypes.Length; index ++)
		{
			Assert.IsTrue(items.TryGet(index, out var entry));
			Assert.AreEqual(expectedTypes[index], AdapterFactory.GetItemAdapter(entry).GetType());
		}
	}

	[TestMethod]
	public void XPathMetadataReturnsEmptyForMissingDataAndStoredStringForPresentData()
	{
		var metadata = new DataModelList { "prefix", "urn:test" };

		Assert.AreEqual(string.Empty, XPathMetadata.GetValue(metadata: null, index: 0, offset: 0));
		Assert.AreEqual("prefix", XPathMetadata.GetValue(metadata, index: 0, offset: 0));
		Assert.AreEqual("urn:test", XPathMetadata.GetValue(metadata, index: 0, offset: 1));
		Assert.AreEqual(string.Empty, XPathMetadata.GetValue(metadata, index: 5, offset: 1));
	}

	[TestMethod]
	public void ListAndListItemAdaptersNavigateBothDirectionsAndAggregateTextValues()
	{
		var nested = new DataModelList { ["inner"] = "nested-value" };
		var list = new DataModelList
				   {
					   ["first"] = "one",
					   ["second"] = "two",
					   ["nested"] = nested
				   };
		var parent = new DataModelXPathNavigator.Node(new DataModelValue(list), new ListNodeAdapter());

		Assert.AreEqual(XPathNodeType.Element, parent.GetNodeType());
		Assert.IsFalse(parent.IsEmptyElement());
		Assert.IsTrue(parent.GetFirstChild(out var first));
		Assert.AreEqual("first", first.GetLocalName());
		Assert.AreEqual("one", first.GetValue());
		Assert.IsTrue(parent.GetNextChild(ref first));
		Assert.AreEqual("second", first.GetLocalName());
		var second = first;
		Assert.IsTrue(parent.GetNextChild(ref first));
		Assert.AreEqual("nested", first.GetLocalName());
		Assert.IsInstanceOfType<ListItemNodeAdapter>(first.Adapter);
		Assert.IsTrue(first.GetFirstChild(out var inner));
		Assert.AreEqual("inner", inner.GetLocalName());
		Assert.AreEqual("nested-value", inner.GetValue());
		Assert.IsFalse(first.GetNextChild(ref inner));
		Assert.IsTrue(parent.GetPreviousChild(ref second));
		Assert.AreEqual("first", second.GetLocalName());

		Assert.AreEqual("onetwonested-value", parent.GetValue());
		Assert.AreEqual(expected: 18, parent.Adapter.GetBufferSizeForValue(parent));
		Span<char> buffer = stackalloc char[18];
		Assert.AreEqual(expected: 18, parent.Adapter.WriteValueToSpan(parent, buffer));
		Assert.AreEqual("onetwonested-value", buffer.ToString());

		var empty = new DataModelXPathNavigator.Node(new DataModelValue([]), new ListNodeAdapter());
		Assert.IsTrue(empty.IsEmptyElement());
		Assert.IsFalse(empty.GetFirstChild(out _));
	}

	[TestMethod]
	public void ItemAndAttributeAdaptersExposeMetadataAttributesNamespacesAndTypeAttribute()
	{
		var metadata = new DataModelList
					   {
						   "elementPrefix", "urn:element",
						   "attribute", "attribute-value", "a", "urn:attribute",
						   "ns", "urn:declared", string.Empty, XPathMetadata.XmlnsNamespace
					   };
		var item = new DataModelXPathNavigator.Node(
			new DataModelValue("text"), new ItemNodeAdapter(), parentProperty: "item", metadata: metadata);

		Assert.AreEqual("item", item.GetLocalName());
		Assert.AreEqual("elementPrefix", item.GetPrefix());
		Assert.AreEqual("elementPrefix:item", item.GetName());
		Assert.AreEqual("urn:element", item.GetNamespaceUri());
		Assert.IsTrue(item.GetFirstAttribute(out var attribute));
		Assert.IsInstanceOfType<AttributeNodeAdapter>(attribute.Adapter);
		Assert.AreEqual("attribute", attribute.GetLocalName());
		Assert.AreEqual("a", attribute.GetPrefix());
		Assert.AreEqual("a:attribute", attribute.GetName());
		Assert.AreEqual("urn:attribute", attribute.GetNamespaceUri());
		Assert.AreEqual("attribute-value", attribute.GetValue());
		Assert.IsFalse(item.GetNextAttribute(ref attribute));

		Assert.IsTrue(item.GetFirstNamespace(out var namespaceNode));
		Assert.IsInstanceOfType<NamespaceNodeAdapter>(namespaceNode.Adapter);
		Assert.AreEqual("ns", namespaceNode.GetLocalName());
		Assert.AreEqual("urn:declared", namespaceNode.GetValue());
		Assert.IsFalse(item.GetNextNamespace(ref namespaceNode));

		var number = new DataModelXPathNavigator.Node(new DataModelValue(12D), new ItemNodeAdapter(), parentProperty: "number");
		Assert.IsTrue(number.GetFirstAttribute(out var typeAttribute));
		Assert.IsInstanceOfType<TypeAttributeNodeAdapter>(typeAttribute.Adapter);
		Assert.AreEqual(XmlConverter.TypeAttributeName, typeAttribute.GetLocalName());
		Assert.AreEqual("number", typeAttribute.GetValue());
		Assert.IsFalse(number.GetNextAttribute(ref typeAttribute));
		Assert.IsFalse(number.GetFirstNamespace(out _));
	}

	private sealed class DefaultNodeAdapter : NodeAdapter
	{
		public override XPathNodeType GetNodeType() => XPathNodeType.Root;
	}
}
