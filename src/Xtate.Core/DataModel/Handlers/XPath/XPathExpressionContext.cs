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

using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Xtate.DataModel.XPath
{
	internal class XPathExpressionContext : XsltContext
	{
		public XPathExpressionContext(XPathResolver resolver, XmlNameTable nameTable) : base((NameTable) nameTable) => Resolver = resolver;

		public XPathResolver Resolver { get; private set; }

		public override bool Whitespace => false;

		public override IXsltContextVariable ResolveVariable(string prefix, string name) => Resolver.ResolveVariable(LookupNamespace(prefix), name);

		public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] _) => Resolver.ResolveFunction(LookupNamespace(prefix), name);

		public override bool PreserveWhitespace(XPathNavigator node) => false;

		public override int CompareDocument(string baseUri, string nextbaseUri) => string.CompareOrdinal(baseUri, nextbaseUri);

		public void SetResolver(XPathResolver resolver) => Resolver = resolver;
	}
}