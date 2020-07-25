using System.Xml.XPath;
using System.Xml.Xsl;

namespace Xtate.DataModel.XPath
{
	internal abstract class XPathFunctionDescriptorBase : IXsltContextFunction
	{
		protected XPathFunctionDescriptorBase(string ns, string name, XPathResultType[] argTypes, XPathResultType returnType)
		{
			Namespace = ns;
			Name = name;
			ArgTypes = argTypes;
			ReturnType = returnType;
		}

		public virtual string Namespace { get; }

		public virtual string Name { get; }

	#region Interface IXsltContextFunction

		object IXsltContextFunction.Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext) => Invoke(((XPathExpressionContext) xsltContext).Resolver, args);

		public virtual XPathResultType[] ArgTypes { get; }

		public virtual XPathResultType ReturnType { get; }

		public virtual int Maxargs => ArgTypes.Length;

		public virtual int Minargs => ArgTypes.Length;

	#endregion

		protected abstract object Invoke(XPathResolver resolver, object[] args);
	}
}