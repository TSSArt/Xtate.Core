namespace Xtate.DataModel.XPath
{
	internal class ListNodeAdapter : ElementNodeAdapter
	{
		public override bool GetNextChild(in DataModelXPathNavigator.Node parentNode, ref DataModelXPathNavigator.Node node)
		{
			var list = parentNode.DataModelValue.AsList();

			var cursor = node.ParentCursor;

			var ok = list.NextEntry(ref cursor, out var entry);
			node = ok ? new DataModelXPathNavigator.Node(entry.Value, AdapterFactory.GetItemAdapter(entry.Value), cursor, entry.Index, entry.Key, entry.Metadata) : default;

			return ok;
		}

		public override bool GetPreviousChild(in DataModelXPathNavigator.Node parentNode, ref DataModelXPathNavigator.Node node)
		{
			var list = parentNode.DataModelValue.AsList();

			var cursor = node.ParentCursor;

			var ok = list.PreviousEntry(ref cursor, out var entry);
			node = ok ? new DataModelXPathNavigator.Node(entry.Value, AdapterFactory.GetItemAdapter(entry.Value), cursor, entry.Index, entry.Key, entry.Metadata) : default;

			return ok;
		}
	}
}