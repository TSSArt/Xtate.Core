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

using System.Reflection;
using System.Xml;
using Xtate.Ancestor;
using Xtate.Scxml.Services;

namespace Xtate.Test.UnitTests.Scxml;

[TestClass]
public class ScxmlDirectorNestedCoverageTest
{
	[TestMethod]
	public void XmlLineInfoExposesLocationAndAncestor()
	{
		var ancestor = new object();
		var type = typeof(ScxmlDirector).GetNestedType("XmlLineInfo", BindingFlags.NonPublic)!;
		var instance = Activator.CreateInstance(type, [17, 23, ancestor])!;
		var lineInfo = (IXmlLineInfo) instance;

		Assert.IsTrue(lineInfo.HasLineInfo());
		Assert.AreEqual(expected: 17, lineInfo.LineNumber);
		Assert.AreEqual(expected: 23, lineInfo.LinePosition);
		Assert.AreSame(ancestor, ((IAncestorProvider) instance).Ancestor);
	}

	[TestMethod]
	public void XIncludeStringsInternsEveryAttributeAndNamespaceName()
	{
		var nameTable = new System.Xml.NameTable();
		var type = typeof(XIncludeReader).GetNestedType("Strings", BindingFlags.NonPublic)!;
		var strings = Activator.CreateInstance(type, [nameTable])!;
		var expected = new Dictionary<string, string>
					   {
						   ["Accept"] = "accept",
						   ["AcceptLanguage"] = "accept-language",
						   ["Encoding"] = "encoding",
						   ["Href"] = "href",
						   ["Include"] = "include",
						   ["Parse"] = "parse",
						   ["XInclude1Ns"] = "http://www.w3.org/2001/XInclude",
						   ["XInclude2Ns"] = "http://www.w3.org/2003/XInclude"
					   };

		foreach (var (propertyName, value) in expected)
		{
			var first = (string) type.GetProperty(propertyName)!.GetValue(strings)!;
			var second = (string) type.GetProperty(propertyName)!.GetValue(strings)!;
			Assert.AreEqual(value, first);
			Assert.AreSame(nameTable.Get(value), first);
			Assert.AreSame(first, second);
		}
	}
}
