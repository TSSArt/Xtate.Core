using System.Xml;
using System.Xml.XPath;
using Xtate.Scxml;

namespace Xtate.DataModel.XPath
{
	internal class XPathCompiledExpression
	{
		private static readonly XPathResolver ParseExpressionResolver = new XPathResolver(XPathFunctionFactory.Instance);

		private readonly XPathExpressionContext _context;

		public XPathCompiledExpression(string expression, object? entity)
		{
			entity.Is<XmlNameTable>(out var nameTable);
			nameTable ??= new NameTable();

			_context = new XPathExpressionContext(ParseExpressionResolver, nameTable);

			if (entity.Is<IXmlNamespacesInfo>(out var xmlNamespacesInfo))
			{
				foreach (var prefixUri in xmlNamespacesInfo.Namespaces)
				{
					_context.AddNamespace(prefixUri.Prefix, prefixUri.Namespace);
				}
			}

			XPathExpression = XPathExpression.Compile(expression, _context);
		}

		public XPathResultType ReturnType => XPathExpression.ReturnType;

		public string Expression => XPathExpression.Expression;

		public XPathExpression XPathExpression { get; }

		public void SetResolver(XPathResolver resolver) => _context.SetResolver(resolver);
	}
}