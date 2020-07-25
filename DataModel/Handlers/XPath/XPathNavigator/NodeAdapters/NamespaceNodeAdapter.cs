using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal class NamespaceNodeAdapter : NodeAdapter
	{
		public override XPathNodeType GetNodeType(in DataModelXPathNavigator.Node node) => XPathNodeType.Namespace;

		public override string GetLocalName(in DataModelXPathNavigator.Node node) => node.ParentProperty ?? Infrastructure.Fail<string>();

		public override string GetValue(in DataModelXPathNavigator.Node node) => node.DataModelValue.AsString();
	}
}