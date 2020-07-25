using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal class AttributeNodeAdapter : NodeAdapter
	{
		public override XPathNodeType GetNodeType(in DataModelXPathNavigator.Node node) => XPathNodeType.Attribute;

		public override string GetLocalName(in DataModelXPathNavigator.Node node) => node.ParentProperty ?? Infrastructure.Fail<string>();

		public override string GetNamespaceUri(in DataModelXPathNavigator.Node node) =>
				XPathMetadata.GetValue(node.Metadata, XPathMetadata.AttributeNamespaceOffset, node.ParentCursor, node.ParentProperty);

		public override string GetPrefix(in DataModelXPathNavigator.Node node) => XPathMetadata.GetValue(node.Metadata, XPathMetadata.AttributePrefixOffset, node.ParentCursor, node.ParentProperty);

		public override string GetValue(in DataModelXPathNavigator.Node node) => node.DataModelValue.AsString();
	}
}