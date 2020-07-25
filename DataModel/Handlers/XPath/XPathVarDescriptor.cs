using System.Xml.XPath;
using System.Xml.Xsl;

namespace Xtate.DataModel.XPath
{
	internal sealed class XPathVarDescriptor : IXsltContextVariable
	{
		private readonly string _name;

		public XPathVarDescriptor(string name) => _name = name;

	#region Interface IXsltContextVariable

		public object Evaluate(XsltContext xsltContext) => ((XPathExpressionContext) xsltContext).Resolver.GetVariable(_name);

		public bool IsLocal => false;
		public bool IsParam => false;

		public XPathResultType VariableType => XPathResultType.NodeSet;

	#endregion
	}
}