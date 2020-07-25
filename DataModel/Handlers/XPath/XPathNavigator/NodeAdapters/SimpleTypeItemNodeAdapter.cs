namespace Xtate.DataModel.XPath
{
	internal class SimpleTypeItemNodeAdapter : ItemNodeAdapter
	{
		public override bool GetFirstChild(in DataModelXPathNavigator.Node node, out DataModelXPathNavigator.Node childNode)
		{
			childNode = new DataModelXPathNavigator.Node(node.DataModelValue, AdapterFactory.SimpleTypeNodeAdapter);

			return true;
		}

		public override bool GetNextChild(in DataModelXPathNavigator.Node parentNode, ref DataModelXPathNavigator.Node node) => false;
	}
}