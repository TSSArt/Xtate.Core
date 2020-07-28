#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 
#endregion

namespace Xtate.DataModel.XPath
{
	internal class ItemNodeAdapter : ElementNodeAdapter
	{
		public override string GetLocalName(in DataModelXPathNavigator.Node node) => node.ParentProperty ?? Infrastructure.Fail<string>();

		public override string GetNamespaceUri(in DataModelXPathNavigator.Node node) => XPathMetadata.GetValue(node.Metadata, XPathMetadata.ElementNamespaceOffset);

		public override string GetPrefix(in DataModelXPathNavigator.Node node) => XPathMetadata.GetValue(node.Metadata, XPathMetadata.ElementPrefixOffset);

		public override bool GetFirstAttribute(in DataModelXPathNavigator.Node node, out DataModelXPathNavigator.Node attributeNode)
		{
			attributeNode = new DataModelXPathNavigator.Node(value: default, default!);

			return GetNextAttribute(node, ref attributeNode);
		}

		public override bool GetNextAttribute(in DataModelXPathNavigator.Node parentNode, ref DataModelXPathNavigator.Node node) => GetNextAttribute(parentNode, ref node, ns: false);

		public override bool GetFirstNamespace(in DataModelXPathNavigator.Node node, out DataModelXPathNavigator.Node namespaceNode)
		{
			namespaceNode = new DataModelXPathNavigator.Node(value: default, default!);

			return GetNextNamespace(node, ref namespaceNode);
		}

		public override bool GetNextNamespace(in DataModelXPathNavigator.Node parentNode, ref DataModelXPathNavigator.Node node) => GetNextAttribute(parentNode, ref node, ns: true);

		private static bool GetNextAttribute(in DataModelXPathNavigator.Node parentNode, ref DataModelXPathNavigator.Node node, bool ns)
		{
			var metadata = parentNode.Metadata;

			if (metadata == null)
			{
				node = default;

				return false;
			}

			var cursor = node.ParentCursor;

			var currentKey = node.ParentProperty;

			while (metadata.NextEntry(ref cursor, out var entry))
			{
				if (entry.Key == currentKey)
				{
					continue;
				}

				currentKey = entry.Key;

				var isNamespace = XPathMetadata.GetValue(parentNode.Metadata, XPathMetadata.AttributeNamespaceOffset, cursor, entry.Key) == "http://www.w3.org/2000/xmlns/";
				if (isNamespace == ns)
				{
					var adapter = ns ? AdapterFactory.NamespaceNodeAdapter : AdapterFactory.AttributeNodeAdapter;
					node = new DataModelXPathNavigator.Node(entry.Value, adapter, cursor, entry.Index, entry.Key, metadata);

					return true;
				}
			}

			node = default;

			return false;
		}
	}
}