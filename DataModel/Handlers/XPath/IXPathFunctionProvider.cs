using System.Xml.Xsl;

namespace Xtate.DataModel.XPath;

public interface IXPathFunctionProvider
{
	IXsltContextFunction? TryGetFunction(string ns, string name);
}