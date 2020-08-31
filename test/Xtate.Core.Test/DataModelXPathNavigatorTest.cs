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

using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xtate.DataModel.XPath;

namespace Xtate.Core.Test
{
	[TestClass]
	public class DataModelXPathNavigatorTest
	{
		[TestMethod]
		public void SimpleStringShouldBeConvertedToString_IfRootIsString()
		{
			// arrange
			var v = DataModelValue.FromString("StrVal");

			// act
			var val = new DataModelXPathNavigator(v).Evaluate("string(.)");

			// assert
			Assert.AreEqual(expected: "StrVal", val);
		}

		[TestMethod]
		public void NumberShouldBeConvertedToDouble_IfRootIsString()
		{
			// arrange
			var v = DataModelValue.FromString("5.5");

			// act
			var val = new DataModelXPathNavigator(v).Evaluate("sum(.)");

			// assert
			Assert.AreEqual(expected: 5.5, val);
		}

		[TestMethod]
		public void ConditionShouldBeConvertedToBoolean_IfRootIsString()
		{
			// arrange
			var v = DataModelValue.FromString("5.5");

			// act
			var val = new DataModelXPathNavigator(v).Evaluate("sum(.) > 1.0");

			// assert
			Assert.AreEqual(expected: true, val);
		}

		[TestMethod]
		public void ValueOfObjectPropertyShouldBeAvailableThroughValue_IfRootIsObject()
		{
			// arrange
			var v = DataModelValue.FromObject(new { prop = "val" });

			// act
			var val = new DataModelXPathNavigator(v).Evaluate("string(prop)");

			// assert
			Assert.AreEqual(expected: "val", val);
		}

		[TestMethod]
		public void ValuesOfObjectPropertiesShouldBeConcatenatedThroughValue_IfRootIsComplexObject()
		{
			// arrange
			var v = DataModelValue.FromObject(new { prop = "1", obj = new { prop1 = "1", prop2 = "1" } });

			// act
			var val = new DataModelXPathNavigator(v).Evaluate("string(.)");

			// assert
			Assert.AreEqual(expected: "111", val);
		}

		[TestMethod]
		public void CanSelectSubNode_IfRootIsComplexObject()
		{
			// arrange
			var v = DataModelValue.FromObject(new
											  {
													  /*prop = "val", */obj = new { prop1 = "val1", prop2 = "target" }
											  });

			// act
			var val = new DataModelXPathNavigator(v).Evaluate("string(obj/prop2)");

			// assert
			Assert.AreEqual(expected: "target", val);
		}

		[TestMethod]
		public void LocalNameShouldBeText_IfTypeIsString()
		{
			// arrange
			var v = DataModelValue.FromObject("str");

			// act
			var val = new DataModelXPathNavigator(v);

			// assert
			Assert.AreEqual(expected: "#text", val.LocalName);
		}

		[TestMethod]
		public void LocalNameShouldBeText_IfTypeIsNumeric()
		{
			// arrange
			var v = DataModelValue.FromObject(55);

			// act
			var val = new DataModelXPathNavigator(v);

			// assert
			Assert.AreEqual(expected: "#text", val.LocalName);
		}

		[TestMethod]
		public void LocalNameShouldBeEmpty_IfTypeIsList()
		{
			// arrange
			var v = DataModelValue.FromObject(new { key = "val" });

			// act
			var val = new DataModelXPathNavigator(v);

			// assert
			Assert.AreEqual(expected: "", val.LocalName);
		}

		[TestMethod]
		public void LocalNameShouldBePropName_IfTypeIsList()
		{
			// arrange
			var v = DataModelValue.FromObject(new { key = "val" });

			// act
			var val = new DataModelXPathNavigator(v);
			val.MoveToFirstChild();

			// assert
			Assert.AreEqual(expected: "key", val.LocalName);
		}

		[TestMethod]
		public void TempTest()
		{
			// arrange
			var n = DataModelValue.FromObject(new { child1 = "val1", child2 = "val2" });
			var v = DataModelValue.FromObject(new { key = "val" });
			var nNav = new DataModelXPathNavigator(n);
			var vNav = new DataModelXPathNavigator(v);
			vNav.MoveToFirstChild();

			// act
			vNav.ReplaceChildren(new XPathObject(nNav.Evaluate("child::*")!));

			// assert
			Assert.AreEqual(expected: "val1", v.AsObject()["key"].AsObject()["child1"].AsString());
		}

		[TestMethod]
		public void RenderValidXml()
		{
			// arrange
			var root = new DataModelObject
					   {
							   {
									   "root",
									   new DataModelObject
									   {
											   { "item", "val1" },
											   { "item", "val2" }
									   },
									   new DataModelArray
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
			var val = new DataModelXPathNavigator(root);

			// assert
			var xml = val.InnerXml;

			var dataModelValue = XmlConverter.FromXml(xml);

			var n2 = new DataModelXPathNavigator(dataModelValue);
			var xml2 = n2.InnerXml;

			var dataModelValue2 = XmlConverter.FromXml(xml2);

			var n3 = new DataModelXPathNavigator(dataModelValue2);
			var _ = n3.InnerXml;
		}

		[TestMethod]
		[SuppressMessage(category: "ReSharper", checkId: "UnusedVariable")]
		public void RenderValidXml2()
		{
#pragma warning disable IDE0059
			var xpath = "string(/a)";
			var xPathExpression = XPathExpression.Compile(xpath);

			var s = "<a xmlns:ss='dsf'><ss:eee/></a>";

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
}