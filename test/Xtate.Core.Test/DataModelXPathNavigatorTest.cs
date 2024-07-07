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
using System.Xml.XPath;
using Xtate.DataModel.XPath;

namespace Xtate.Core.Test;

[TestClass]
public class DataModelXPathNavigatorTest
{
	private readonly Mock<INameTableProvider> NameTableProvider = new();

	[TestInitialize]
	public void Init()
	{
		NameTableProvider.Setup(n => n.GetNameTable()).Returns(new NameTable());
	}

	[TestMethod]
	public void SimpleStringShouldBeConvertedToString_IfRootIsString()
	{
		// arrange
		var v = DataModelValue.FromString("StrVal");

		// act
		var value = new DataModelXPathNavigator(v).Evaluate("string(.)");

		// assert
		Assert.AreEqual(expected: "StrVal", value);
	}

	[TestMethod]
	public void NumberShouldBeConvertedToDouble_IfRootIsString()
	{
		// arrange
		var v = DataModelValue.FromString("5.5");

		// act
		var value = new DataModelXPathNavigator(v).Evaluate("sum(.)");

		// assert
		Assert.AreEqual(expected: 5.5, value);
	}

	[TestMethod]
	public void ConditionShouldBeConvertedToBoolean_IfRootIsString()
	{
		// arrange
		var v = DataModelValue.FromString("5.5");

		// act
		var value = new DataModelXPathNavigator(v).Evaluate("sum(.) > 1.0");

		// assert
		Assert.AreEqual(expected: true, value);
	}

	[TestMethod]
	public void ValueOfObjectPropertyShouldBeAvailableThroughValue_IfRootIsObject()
	{
		// arrange
		var v = DataModelValue.FromObject(new { prop = "value" });

		// act
		var value = new DataModelXPathNavigator(v).Evaluate("string(prop)");

		// assert
		Assert.AreEqual(expected: "value", value);
	}

	[TestMethod]
	public void ValuesOfObjectPropertiesShouldBeConcatenatedThroughValue_IfRootIsComplexObject()
	{
		// arrange
		var v = DataModelValue.FromObject(new { prop = "1", obj = new { prop1 = "1", prop2 = "1" } });

		// act
		var value = new DataModelXPathNavigator(v).Evaluate("string(.)");

		// assert
		Assert.AreEqual(expected: "111", value);
	}

	[TestMethod]
	public void CanSelectSubNode_IfRootIsComplexObject()
	{
		// arrange
		var v = DataModelValue.FromObject(
			new
			{
				/*prop = "value", */obj = new { prop1 = "val1", prop2 = "target" }
			});

		// act
		var value = new DataModelXPathNavigator(v).Evaluate("string(obj/prop2)");

		// assert
		Assert.AreEqual(expected: "target", value);
	}

	[TestMethod]
	public void LocalNameShouldBeText_IfTypeIsString()
	{
		// arrange
		var v = DataModelValue.FromObject("str");

		// act
		var value = new DataModelXPathNavigator(v);

		// assert
		Assert.AreEqual(expected: "#text", value.LocalName);
	}

	[TestMethod]
	public void LocalNameShouldBeText_IfTypeIsNumeric()
	{
		// arrange
		var v = DataModelValue.FromObject(55);

		// act
		var value = new DataModelXPathNavigator(v);

		// assert
		Assert.AreEqual(expected: "#text", value.LocalName);
	}

	[TestMethod]
	public void LocalNameShouldBeEmpty_IfTypeIsList()
	{
		// arrange
		var v = DataModelValue.FromObject(new { key = "value" });

		// act
		var value = new DataModelXPathNavigator(v);

		// assert
		Assert.AreEqual(expected: "", value.LocalName);
	}

	[TestMethod]
	public void LocalNameShouldBePropName_IfTypeIsList()
	{
		// arrange
		var v = DataModelValue.FromObject(new { key = "value" });

		// act
		var value = new DataModelXPathNavigator(v);
		value.MoveToFirstChild();

		// assert
		Assert.AreEqual(expected: "key", value.LocalName);
	}

	[TestMethod]
	public void TempTest()
	{
		// arrange
		var n = DataModelValue.FromObject(new { child1 = "val1", child2 = "val2" });
		var v = DataModelValue.FromObject(new { key = "value" });
		var nNav = new DataModelXPathNavigator(n);
		var vNav = new DataModelXPathNavigator(v);
		vNav.MoveToFirstChild();

		// act
		vNav.ReplaceChildren(new XPathObject(nNav.Evaluate("child::*")));

		// assert
		v.AsList().TryGet(key: "key", caseInsensitive: false, out var v1);
		v1.Value.AsList().TryGet(key: "child1", caseInsensitive: false, out var v2);
		Assert.AreEqual(expected: "val1", v2.Value.AsString());
	}

	[TestMethod]
	public void ArrayTest()
	{
		// arrange
		var list = new DataModelList { new DataModelList { ["key1"] = "val1" }, new DataModelList { ["key2"] = "val2" } };
		var root = new DataModelList { ["root"] = list };
		/*			list.Add("", "empty");
					list.Add(":#$%", "symbol");
					list.Add("b", true);
					list.Add("n", 1.5);
					list.Add("dttm", DateTime.UtcNow);
					list.Add("nl", DataModelValue.Null);
					list.Add("undef", default);
					list.Add(null, default, default);*/
		var nav = new DataModelXPathNavigator(root);

		// act
		_ = (XPathNodeIterator?) nav.Evaluate("/root/node()");

		// assert
		//Assert.AreEqual(expected: "e", xml);
	}

	[TestMethod]
	public void RenderValidXml()
	{
		// arrange
		var root = new DataModelList
				   {
					   {
						   "root",
						   new DataModelList
						   {
							   { "item", "val1" },
							   { "item", "val2" }
						   },
						   new DataModelList
						   {
							   "prefix",
							   "namespace-uri",
							   "attr1", "aVal1", "", "",
							   "attr2", "aVal2", "pfx", "attr-ns",
							   "myNs", "myNamespace", "", "http://www.w3.org/2000/xmlns/"
						   }
					   }
				   };

		// act
		var navigator = new DataModelXPathNavigator(root);

		// assert
		var xml = navigator.InnerXml;

		var value = XmlConverter.FromXml(xml);

		var n2 = new DataModelXPathNavigator(value);
		var xml2 = n2.InnerXml;

		var dataModelValue2 = XmlConverter.FromXml(xml2);

		var n3 = new DataModelXPathNavigator(dataModelValue2);
		_ = n3.InnerXml;
	}

	[TestMethod]
	[SuppressMessage(category: "ReSharper", checkId: "UnusedVariable")]
	public void RenderValidXml2()
	{
#pragma warning disable IDE0059
		const string xpath = "string(/a)";
		var xPathExpression = XPathExpression.Compile(xpath);

		const string s = "<a xmlns:ss='dsf'><ss:eee/></a>";

		var t = XmlConverter.FromXml(s);

		var xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(s);
		var navigatorDoc = xmlDocument.CreateNavigator();

		var navigator = new DataModelXPathNavigator(t);

		var s1 = navigator.MoveToFirstChild();
		var s1a = navigator.MoveToFirstChild();
		var navigatorIsEmptyElement = navigator.IsEmptyElement;
		var s2q = navigator.MoveToFirstAttribute();
		var s2qs = navigator.MoveToNextAttribute();
		var s2qw = navigator.MoveToParent();
		var as2q = navigator.MoveToFirstNamespace(XPathNamespaceScope.Local);
		var as2qs = navigator.MoveToNextNamespace(XPathNamespaceScope.Local);
		var as2qa = navigator.MoveToNextNamespace(XPathNamespaceScope.Local);
		var as2qw = navigator.MoveToParent();

		var s2 = navigator.MoveToNext();
		var s3 = navigator.MoveToParent();

		var navigatorHasAttributes = navigator.HasAttributes;
		var moveToFirstAttribute = navigator.MoveToFirstAttribute();
		if (moveToFirstAttribute)
		{
			navigator.MoveToParent();
		}

		var moveToFirstNamespace = navigator.MoveToFirstNamespace(XPathNamespaceScope.ExcludeXml);
		navigator.MoveToNextNamespace(XPathNamespaceScope.ExcludeXml);
		if (moveToFirstNamespace)
		{
			navigator.MoveToParent();
		}
#pragma warning restore IDE0059
	}
}