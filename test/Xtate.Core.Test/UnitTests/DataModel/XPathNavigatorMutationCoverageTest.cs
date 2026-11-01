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
using System.Xml.XPath;
using Xtate.DataModel.XPath.Internal;
using Xtate.DataTypes;

namespace Xtate.Core.Test.UnitTests.DataModel;

[TestClass]
public class XPathNavigatorMutationCoverageTest
{
	[TestMethod]
	public void ReplacingAnElementInsideAnAnonymousListCollapsesBothScalarWrappers()
	{
		var anonymous = new DataModelList { new DataModelList { ["child"] = "old" } };
		var data = new DataModelList { ["root"] = anonymous };
		var navigator = (DataModelXPathNavigator)new DataModelXPathNavigator(data).SelectSingleNode("root/*/child")!;
		navigator.Replace(new XPathObject("new"));
		Assert.AreEqual("new", data["root"].AsString());
	}

	[TestMethod]
	public void DeletingSiblingOfTopLevelTextKeepsTheText()
	{
		var data = new DataModelList();
		data.Add(key: null, "text", metadata: null);
		data.Add("child", "old", metadata: null);
		new DataModelXPathNavigator(data).SelectSingleNode("child")!.DeleteSelf();
		Assert.AreEqual(1, data.Count);
		Assert.AreEqual("text", data.Entries.Single().Value.AsString());
		Assert.IsNull(data.Entries.Single().Key);
	}

	[TestMethod]
	public void InsertingBeforeTopLevelElementKeepsBothEntries()
	{
		var data = new DataModelList { ["child"] = "old" };
		var navigator = (DataModelXPathNavigator)new DataModelXPathNavigator(data).SelectSingleNode("child")!;
		navigator.PreviousSibling(new XPathObject("new"));
		Assert.AreEqual(2, data.Count);
		Assert.AreEqual("new", data.Entries.First().Value.AsString());
		Assert.AreEqual("old", data["child"].AsString());
	}

	[TestMethod]
	public void ReplacingTheOnlyTopLevelElementWithTextPreservesTheReplacement()
	{
		var data = new DataModelList { ["child"] = "old" };
		var navigator = (DataModelXPathNavigator)new DataModelXPathNavigator(data).SelectSingleNode("child")!;
		navigator.Replace(new XPathObject("new"));
		Assert.AreEqual("new", new DataModelXPathNavigator(data).Value);
		Assert.AreEqual(1, data.Count);
		Assert.IsNull(data.Entries.Single().Key);
	}
	[TestMethod]
	[DataRow("first", "newold")]
	[DataRow("last", "oldnew")]
	[DataRow("replace", "new")]
	public void UnnamedRootListSupportsChildInsertionAndReplacement(string operation, string expected)
	{
		var data = new DataModelList { "old" };
		var navigator = new DataModelXPathNavigator(data);
		var value = new XPathObject("new");
		switch (operation)
		{
			case "first": navigator.FirstChild(value); break;
			case "last": navigator.LastChild(value); break;
			case "replace": navigator.ReplaceChildren(value); break;
		}
		Assert.AreEqual(expected, new DataModelXPathNavigator(data).Value);
	}

	[TestMethod]
	public void RootSiblingDeletionAndAttributeOperationsLeaveRootIntact()
	{
		var data = new DataModelList { ["child"] = "value" };
		var navigator = new DataModelXPathNavigator(data);
		navigator.Replace(new XPathObject("ignored"));
		navigator.DeleteSelf();
		navigator.CreateAttribute(null, "ignored", null, "ignored");
		Assert.AreEqual("value", data["child"].AsString());
		Assert.AreEqual(1, data.Count);
		Assert.IsFalse(new DataModelXPathNavigator(data).HasAttributes);
	}

	[TestMethod]
	public void RemovingOnlyChildNormalizesParentWhenAnotherRootPropertyExists()
	{
		var data = new DataModelList { ["root"] = new DataModelList { ["child"] = "value" }, ["spare"] = "preserved" };
		new DataModelXPathNavigator(data).SelectSingleNode("root/child")!.DeleteSelf();
		Assert.IsTrue(data["root"].IsUndefined());
		Assert.AreEqual("preserved", data["spare"].AsString());
	}

	[TestMethod]
	public void InheritedNamespaceIsVisibleOnDescendantAndReturnsToThatDescendant()
	{
		var data = XmlConverter.FromXml("<root xmlns:p='urn:parent'><child/></root>");
		var navigator = new DataModelXPathNavigator(data).SelectSingleNode("root/child")!;
		Assert.IsTrue(navigator.MoveToFirstNamespace(XPathNamespaceScope.ExcludeXml));
		Assert.AreEqual("p", navigator.LocalName);
		Assert.AreEqual("urn:parent", navigator.Value);
		Assert.IsTrue(navigator.MoveToParent());
		Assert.AreEqual("child", navigator.LocalName);
	}

	[TestMethod]
	public void NamespaceLookupFindsAncestorBindingWithoutMovingOriginalNavigator()
	{
		var data = XmlConverter.FromXml("<root xmlns:p='urn:parent'><child/></root>");
		var navigator = new DataModelXPathNavigator(data).SelectSingleNode("root/child")!;
		Assert.AreEqual("urn:parent", navigator.GetNamespace("p"));
		Assert.AreEqual("child", navigator.LocalName);
	}

	[TestMethod]
	[DataRow("first", "newold")]
	[DataRow("last", "oldnew")]
	[DataRow("replace", "new")]
	public void ScalarChildInsertionNormalizesTextWithoutLosingItsValue(string operation, string expected)
	{
		var data = new DataModelList { ["root"] = "old" };
		var navigator = (DataModelXPathNavigator)new DataModelXPathNavigator(data).SelectSingleNode("root")!;
		var value = new XPathObject("new");
		switch (operation)
		{
			case "first": navigator.FirstChild(value); break;
			case "last": navigator.LastChild(value); break;
			case "replace": navigator.ReplaceChildren(value); break;
		}
		Assert.AreEqual(expected, new DataModelXPathNavigator(data).SelectSingleNode("root")!.Value);
	}

	[TestMethod]
	public void AppendingElementsToScalarContentRetainsOriginalText()
	{
		var data = new DataModelList { ["root"] = "old" };
		var navigator = (DataModelXPathNavigator)new DataModelXPathNavigator(data).SelectSingleNode("root")!;
		var value = new XPathObject(new DataModelXPathNavigator(XmlConverter.FromXml("<child>new</child>")).Select("child"));
		navigator.LastChild(value);
		Assert.AreEqual("oldnew", new DataModelXPathNavigator(data).SelectSingleNode("root")!.Value);
		Assert.AreEqual("new", new DataModelXPathNavigator(data).SelectSingleNode("root/child")!.Value);
	}

	[TestMethod]
	public void DeletingLastNestedElementNormalizesParentToUndefined()
	{
		var data = XmlConverter.FromXml("<root><child>old</child></root>");
		new DataModelXPathNavigator(data).SelectSingleNode("root/child")!.DeleteSelf();
		Assert.IsNull(new DataModelXPathNavigator(data).SelectSingleNode("root/child"));
		Assert.IsTrue(data.AsList()["root"].IsUndefined());
	}

	[TestMethod]
	public void ReplacingOnlyNestedElementWithTextNormalizesParentToScalar()
	{
		var data = XmlConverter.FromXml("<root><child>old</child></root>");
		var navigator = (DataModelXPathNavigator)new DataModelXPathNavigator(data).SelectSingleNode("root/child")!;
		navigator.Replace(new XPathObject("new"));
		Assert.AreEqual("new", data.AsList()["root"].AsString());
	}

	[TestMethod]
	public void InsertingDataModelListCopiesEntriesInsteadOfAliasingThem()
	{
		var data = new DataModelList { ["root"] = DataModelValue.Undefined };
		var source = new DataModelList { ["child"] = "value" };
		var wrapper = new Mock<IObject>();
		wrapper.Setup(value => value.ToObject()).Returns(source);
		var navigator = (DataModelXPathNavigator)new DataModelXPathNavigator(data).SelectSingleNode("root")!;
		navigator.ReplaceChildren(wrapper.Object);
		source["child"] = "changed";
		Assert.AreEqual("value", new DataModelXPathNavigator(data).SelectSingleNode("root/child")!.Value);
	}
	[TestMethod]
	public void DeepPositionCanBeClonedAndRestoredAfterReturningToRoot()
	{
		var xml = string.Concat(Enumerable.Repeat("<node>", 20)) + "value" + string.Concat(Enumerable.Repeat("</node>", 20));
		var navigator = new DataModelXPathNavigator(XmlConverter.FromXml(xml));
		for (var depth = 0; depth < 20; depth++)
		{
			Assert.IsTrue(navigator.MoveToFirstChild());
		}
		var clone = navigator.Clone();
		Assert.IsTrue(clone.IsSamePosition(navigator));
		navigator.MoveToRoot();
		Assert.IsFalse(clone.IsSamePosition(navigator));
		Assert.IsTrue(navigator.MoveTo(clone));
		Assert.IsTrue(clone.IsSamePosition(navigator));
		Assert.AreEqual("value", navigator.Value);
		using var reader = new StringReader("<foreign/>");
		var foreign = new XPathDocument(reader).CreateNavigator();
		Assert.IsFalse(navigator.MoveTo(foreign));
		Assert.IsFalse(navigator.IsSamePosition(foreign));
	}

	[TestMethod]
	[DataRow("first", "<root><added>new</added><old>value</old></root>")]
	[DataRow("last", "<root><old>value</old><added>new</added></root>")]
	[DataRow("replace", "<root><added>new</added></root>")]
	public void ChildInsertionPreservesOrderAndReplacementRemovesOldChildren(string operation, string expected)
	{
		var data = XmlConverter.FromXml("<root><old>value</old></root>");
		var navigator = (DataModelXPathNavigator)new DataModelXPathNavigator(data).SelectSingleNode("root")!;
		var added = new XPathObject(new DataModelXPathNavigator(XmlConverter.FromXml("<added>new</added>")).Select("added"));
		switch (operation)
		{
			case "first": navigator.FirstChild(added); break;
			case "last": navigator.LastChild(added); break;
			case "replace": navigator.ReplaceChildren(added); break;
		}
		Assert.AreEqual(expected, XmlConverter.ToXml(data, indent: false));
	}

	[TestMethod]
	[DataRow("before", "<root><added>new</added><old>value</old><tail>end</tail></root>")]
	[DataRow("after", "<root><old>value</old><added>new</added><tail>end</tail></root>")]
	[DataRow("replace", "<root><added>new</added><tail>end</tail></root>")]
	public void SiblingInsertionAndReplacementPreserveOtherSiblings(string operation, string expected)
	{
		var data = XmlConverter.FromXml("<root><old>value</old><tail>end</tail></root>");
		var navigator = (DataModelXPathNavigator)new DataModelXPathNavigator(data).SelectSingleNode("root/old")!;
		var added = new XPathObject(new DataModelXPathNavigator(XmlConverter.FromXml("<added>new</added>")).Select("added"));
		switch (operation)
		{
			case "before": navigator.PreviousSibling(added); break;
			case "after": navigator.NextSibling(added); break;
			case "replace": navigator.Replace(added); break;
		}
		Assert.AreEqual(expected, XmlConverter.ToXml(data, indent: false));
	}

	[TestMethod]
	public void DeletingOneOfMultipleChildrenPreservesTheOthers()
	{
		var data = XmlConverter.FromXml("<root><first>one</first><second>two</second></root>");
		new DataModelXPathNavigator(data).SelectSingleNode("root/first")!.DeleteSelf();
		Assert.AreEqual("<root><second>two</second></root>", XmlConverter.ToXml(data, indent: false));
	}

	[TestMethod]
	public void AttributesCanBeAddedToElementsWithAndWithoutExistingMetadata()
	{
		var data = new DataModelList { ["root"] = "text" };
		var navigator = new DataModelXPathNavigator(data).SelectSingleNode("root")!;
		navigator.CreateAttribute(null, "first", null, "one");
		var secondNavigator = new DataModelXPathNavigator(data).SelectSingleNode("root")!;
		secondNavigator.CreateAttribute(null, "second", null, null);
		var reloaded = new DataModelXPathNavigator(data).SelectSingleNode("root")!;
		Assert.AreEqual("one", reloaded.GetAttribute("first", ""));
		Assert.AreEqual("", reloaded.GetAttribute("second", ""));
		Assert.IsTrue(reloaded.HasAttributes);
	}
}
