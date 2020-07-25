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