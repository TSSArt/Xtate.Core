using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal class XmlnsXmlNodeAdapter : NodeAdapter
	{
		public override XPathNodeType GetNodeType(in DataModelXPathNavigator.Node node) => XPathNodeType.Namespace;

		public override string GetValue(in DataModelXPathNavigator.Node node) => "http://www.w3.org/XML/1998/namespace";

		public override string GetLocalName(in DataModelXPathNavigator.Node node) => "xml";
	}
}