using System;
using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal abstract class NodeAdapter
	{
		public abstract XPathNodeType GetNodeType(in DataModelXPathNavigator.Node node);

		public virtual string GetValue(in DataModelXPathNavigator.Node node) => string.Empty;

		public virtual int GetBufferSizeForValue(in DataModelXPathNavigator.Node node) => 0;

		public virtual int WriteValueToSpan(in DataModelXPathNavigator.Node node, in Span<char> span) => 0;

		public virtual string GetLocalName(in DataModelXPathNavigator.Node node) => string.Empty;

		public virtual string GetName(in DataModelXPathNavigator.Node node)
		{
			var prefix = GetPrefix(node);

			return string.IsNullOrEmpty(prefix) ? GetLocalName(node) : prefix + ":" + GetLocalName(node);
		}

		public virtual string GetPrefix(in DataModelXPathNavigator.Node node) => string.Empty;

		public virtual string GetNamespaceUri(in DataModelXPathNavigator.Node node) => string.Empty;

		public virtual bool IsEmptyElement(in DataModelXPathNavigator.Node node) => true;

		public virtual bool GetFirstChild(in DataModelXPathNavigator.Node node, out DataModelXPathNavigator.Node childNode)
		{
			childNode = default;

			return false;
		}

		public virtual bool GetNextChild(in DataModelXPathNavigator.Node parentNode, ref DataModelXPathNavigator.Node node) => false;

		public virtual bool GetPreviousChild(in DataModelXPathNavigator.Node parentNode, ref DataModelXPathNavigator.Node node) => false;

		public virtual bool GetFirstAttribute(in DataModelXPathNavigator.Node node, out DataModelXPathNavigator.Node attributeNode)
		{
			attributeNode = default;

			return false;
		}

		public virtual bool GetNextAttribute(in DataModelXPathNavigator.Node parentNode, ref DataModelXPathNavigator.Node node) => false;

		public virtual bool GetFirstNamespace(in DataModelXPathNavigator.Node node, out DataModelXPathNavigator.Node namespaceNode)
		{
			namespaceNode = default;

			return false;
		}

		public virtual bool GetNextNamespace(in DataModelXPathNavigator.Node parentNode, ref DataModelXPathNavigator.Node node) => false;
	}
}