using System;
using System.Xml.Xsl;

namespace Xtate.DataModel.XPath;

public abstract class XPathFunctionProviderBase<TXPathFunction> : IXPathFunctionProvider where TXPathFunction : class, IXsltContextFunction
{
	public required Func<TXPathFunction> XPathFunctionFactory { private get; init; }

#region Interface IXPathFunctionProvider

	public IXsltContextFunction? TryGetFunction(string ns, string name) => CanHandle(ns, name) ? XPathFunctionFactory() : default;

#endregion

	protected abstract bool CanHandle(string ns, string name);
}