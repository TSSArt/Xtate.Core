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
using System.Xml.XPath;
using Xtate.DataModel.XPath.Internal;

namespace Xtate.Core.Test.UnitTests.DataModel;

[TestClass]
public class XPathNavigatorNamespaceScopeTest
{
	private static XPathNavigator Select(string xml, string path) => new DataModelXPathNavigator(XmlConverter.FromXml(xml)).SelectSingleNode(path)!;

	private static List<(string Prefix, string Uri)> Enumerate(XPathNavigator navigator, XPathNamespaceScope scope)
	{
		var result = new List<(string, string)>();
		var element = navigator.LocalName;

		if (navigator.MoveToFirstNamespace(scope))
		{
			do
			{
				result.Add((navigator.LocalName, navigator.Value));

				// Every namespace node, inherited or not, must hang off the context element.
				var probe = navigator.Clone();
				Assert.IsTrue(probe.MoveToParent());
				Assert.AreEqual(element, probe.LocalName);
			}
			while (navigator.MoveToNextNamespace(scope));

			Assert.IsTrue(navigator.MoveToParent());
		}

		Assert.AreEqual(element, navigator.LocalName);

		return result;
	}

	[TestMethod]
	public void RedeclaredPrefixHidesOuterBinding()
	{
		var navigator = Select(xml: "<root xmlns:p='urn:outer'><child xmlns:p='urn:inner'/></root>", path: "root/child");

		CollectionAssert.AreEqual(new[] { ("p", "urn:inner") }, Enumerate(navigator, XPathNamespaceScope.ExcludeXml));
	}

	[TestMethod]
	public void OuterBindingsWithOtherPrefixesStayVisible()
	{
		var navigator = Select(xml: "<root xmlns:a='urn:a' xmlns:p='urn:outer'><child xmlns:p='urn:inner' xmlns:b='urn:b'/></root>", path: "root/child");

		var result = Enumerate(navigator, XPathNamespaceScope.ExcludeXml);

		CollectionAssert.AreEquivalent(new[] { ("p", "urn:inner"), ("b", "urn:b"), ("a", "urn:a") }, result);
	}

	[TestMethod]
	public void InnermostOfThreeDeclarationsWins()
	{
		var navigator = Select(xml: "<root xmlns:p='1'><mid xmlns:p='2'><leaf xmlns:p='3'/></mid></root>", path: "root/mid/leaf");

		CollectionAssert.AreEqual(new[] { ("p", "3") }, Enumerate(navigator, XPathNamespaceScope.ExcludeXml));
	}

	[TestMethod]
	public void MiddleDeclarationHidesRootForUndeclaringLeaf()
	{
		var navigator = Select(xml: "<root xmlns:p='1'><mid xmlns:p='2'><leaf/></mid></root>", path: "root/mid/leaf");

		CollectionAssert.AreEqual(new[] { ("p", "2") }, Enumerate(navigator, XPathNamespaceScope.ExcludeXml));
	}

	[TestMethod]
	public void ShadowingIsPerPrefixAcrossLevels()
	{
		var navigator = Select(xml: "<root xmlns:a='ra' xmlns:b='rb'><mid xmlns:a='ma' xmlns:c='mc'><leaf xmlns:b='lb'/></mid></root>", path: "root/mid/leaf");

		var result = Enumerate(navigator, XPathNamespaceScope.ExcludeXml);

		CollectionAssert.AreEquivalent(new[] { ("b", "lb"), ("a", "ma"), ("c", "mc") }, result);
	}

	[TestMethod]
	public void RedeclaringWithSameValueIsReportedOnce()
	{
		var navigator = Select(xml: "<root xmlns:p='urn:same'><child xmlns:p='urn:same'/></root>", path: "root/child");

		CollectionAssert.AreEqual(new[] { ("p", "urn:same") }, Enumerate(navigator, XPathNamespaceScope.ExcludeXml));
	}

	[TestMethod]
	public void RedeclaredDefaultNamespaceIsReportedOnce()
	{
		var navigator = Select(xml: "<root xmlns='urn:outer'><child xmlns='urn:inner'/></root>", path: "*/*[local-name()='child']");

		var result = Enumerate(navigator, XPathNamespaceScope.ExcludeXml);

		Assert.HasCount(expected: 1, result);
		Assert.AreEqual(expected: "urn:inner", result[0].Uri);
	}

	[TestMethod]
	public void LocalScopeIgnoresAncestors()
	{
		var navigator = Select(xml: "<root xmlns:a='urn:a'><child xmlns:b='urn:b'/></root>", path: "root/child");

		CollectionAssert.AreEqual(new[] { ("b", "urn:b") }, Enumerate(navigator, XPathNamespaceScope.Local));
	}

	[TestMethod]
	public void LocalScopeWithoutOwnDeclarationsFindsNothingAndKeepsPosition()
	{
		var navigator = Select(xml: "<root xmlns:a='urn:a'><child/></root>", path: "root/child");

		Assert.IsFalse(navigator.MoveToFirstNamespace(XPathNamespaceScope.Local));
		Assert.AreEqual(expected: "child", navigator.LocalName);
	}

	[TestMethod]
	public void FailedLookupWithoutAnyDeclarationsKeepsPosition()
	{
		var navigator = Select(xml: "<root><child/></root>", path: "root/child");

		Assert.IsFalse(navigator.MoveToFirstNamespace(XPathNamespaceScope.ExcludeXml));
		Assert.AreEqual(expected: "child", navigator.LocalName);
		Assert.IsFalse(navigator.MoveToNextNamespace(XPathNamespaceScope.ExcludeXml));
		Assert.AreEqual(expected: "child", navigator.LocalName);
	}

	[TestMethod]
	public void AllScopeAppendsImplicitXmlNamespaceLast()
	{
		var navigator = Select(xml: "<root xmlns:p='urn:outer'><child xmlns:p='urn:inner'/></root>", path: "root/child");

		var result = Enumerate(navigator, XPathNamespaceScope.All);

		Assert.HasCount(expected: 2, result);
		Assert.AreEqual(("p", "urn:inner"), result[0]);
		Assert.AreEqual(expected: "xml", result[1].Prefix);
		Assert.AreEqual(expected: "http://www.w3.org/XML/1998/namespace", result[1].Uri);
	}

	[TestMethod]
	public void AllScopeReturnsOnlyXmlNamespaceWhenNothingIsDeclared()
	{
		var navigator = Select(xml: "<root><child/></root>", path: "root/child");

		var result = Enumerate(navigator, XPathNamespaceScope.All);

		Assert.HasCount(expected: 1, result);
		Assert.AreEqual(expected: "xml", result[0].Prefix);
	}

	[TestMethod]
	public void ExcludeXmlScopeNeverReturnsXmlNamespace()
	{
		var navigator = Select(xml: "<root xmlns:p='urn:p'><child/></root>", path: "root/child");

		Assert.IsFalse(Enumerate(navigator, XPathNamespaceScope.ExcludeXml).Exists(static n => n.Prefix == "xml"));
	}

	[TestMethod]
	public void NextAfterXmlNamespaceReturnsFalse()
	{
		var navigator = Select(xml: "<root><child/></root>", path: "root/child");

		Assert.IsTrue(navigator.MoveToFirstNamespace(XPathNamespaceScope.All));
		Assert.AreEqual(expected: "xml", navigator.LocalName);
		Assert.IsFalse(navigator.MoveToNextNamespace(XPathNamespaceScope.All));
	}

	[TestMethod]
	public void NextOnNonNamespaceNodeReturnsFalse()
	{
		var navigator = Select(xml: "<root xmlns:p='urn:p'><child/></root>", path: "root/child");

		Assert.IsFalse(navigator.MoveToNextNamespace(XPathNamespaceScope.All));
		Assert.AreEqual(expected: "child", navigator.LocalName);
	}

	[TestMethod]
	public void SiblingDeclarationsDoNotLeak()
	{
		var navigator = Select(xml: "<root><left xmlns:l='urn:l'/><right xmlns:r='urn:r'/></root>", path: "root/right");

		CollectionAssert.AreEqual(new[] { ("r", "urn:r") }, Enumerate(navigator, XPathNamespaceScope.ExcludeXml));
	}

	[TestMethod]
	public void RootElementSeesOwnDeclarationsOnly()
	{
		var navigator = Select(xml: "<root xmlns:a='urn:a' xmlns:b='urn:b'/>", path: "root");

		CollectionAssert.AreEquivalent(new[] { ("a", "urn:a"), ("b", "urn:b") }, Enumerate(navigator, XPathNamespaceScope.ExcludeXml));
	}

	[TestMethod]
	public void DeepDescendantInheritsFromEveryLevel()
	{
		var navigator = Select(xml: "<a xmlns:p1='1'><b xmlns:p2='2'><c xmlns:p3='3'><d xmlns:p4='4'><e/></d></c></b></a>", path: "a/b/c/d/e");

		var result = Enumerate(navigator, XPathNamespaceScope.ExcludeXml);

		CollectionAssert.AreEquivalent(new[] { ("p1", "1"), ("p2", "2"), ("p3", "3"), ("p4", "4") }, result);
	}

	[TestMethod]
	public void NamespaceNodeOfNonElementContextIsAbsent()
	{
		var navigator = Select(xml: "<root xmlns:p='urn:p' attr='v'>text</root>", path: "root/@attr");

		Assert.IsFalse(navigator.MoveToFirstNamespace(XPathNamespaceScope.All));
		Assert.AreEqual(expected: "attr", navigator.LocalName);
	}

	[TestMethod]
	public void GetNamespaceReturnsInnermostBindingAndKeepsPosition()
	{
		var navigator = Select(xml: "<root xmlns:p='urn:outer' xmlns:q='urn:q'><child xmlns:p='urn:inner'/></root>", path: "root/child");

		Assert.AreEqual(expected: "urn:inner", navigator.GetNamespace("p"));
		Assert.AreEqual(expected: "urn:q", navigator.GetNamespace("q"));
		Assert.AreEqual(string.Empty, navigator.GetNamespace("missing"));
		Assert.AreEqual(expected: "child", navigator.LocalName);
	}

	[TestMethod]
	public void NamespacesInScopeHaveInnermostValues()
	{
		var navigator = Select(xml: "<root xmlns:p='urn:outer' xmlns:q='urn:q'><child xmlns:p='urn:inner'/></root>", path: "root/child");

		var scope = navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml)!;

		Assert.HasCount(expected: 2, scope);
		Assert.AreEqual(expected: "urn:inner", scope["p"]);
		Assert.AreEqual(expected: "urn:q", scope["q"]);
	}

	[TestMethod]
	public void NamespaceAxisYieldsOneNodePerPrefix()
	{
		var navigator = Select(xml: "<root xmlns:p='urn:outer'><child xmlns:p='urn:inner' xmlns:q='urn:q'/></root>", path: "root/child");

		Assert.AreEqual(expected: 3d, navigator.Evaluate("count(namespace::*)"));
		Assert.AreEqual(expected: 1d, navigator.Evaluate("count(namespace::p)"));
		Assert.AreEqual(expected: "urn:inner", navigator.Evaluate("string(namespace::p)"));
	}

	[TestMethod]
	public void ClonedNavigatorEnumeratesIndependently()
	{
		var navigator = Select(xml: "<root xmlns:a='urn:a'><child xmlns:b='urn:b'/></root>", path: "root/child");

		Assert.IsTrue(navigator.MoveToFirstNamespace(XPathNamespaceScope.ExcludeXml));
		var clone = navigator.Clone();
		var first = navigator.LocalName;

		Assert.IsTrue(navigator.MoveToNextNamespace(XPathNamespaceScope.ExcludeXml));
		Assert.AreEqual(first, clone.LocalName);
		Assert.AreNotEqual(first, navigator.LocalName);
		Assert.IsFalse(navigator.MoveToNextNamespace(XPathNamespaceScope.ExcludeXml));
	}
}
