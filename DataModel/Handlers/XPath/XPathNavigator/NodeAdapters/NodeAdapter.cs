#region Copyright © 2019-2023 Sergii Artemenko

// This file is part of the Xtate project. <https://xtate.net/>
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

#endregion

using System.Xml.XPath;

namespace Xtate.DataModel.XPath;

internal abstract class NodeAdapter
{
	public abstract XPathNodeType GetNodeType();

	public virtual string GetValue(in DataModelXPathNavigator.Node node) => string.Empty;

	public virtual int GetBufferSizeForValue(in DataModelXPathNavigator.Node node) => 0;

	public virtual int WriteValueToSpan(in DataModelXPathNavigator.Node node, in Span<char> span) => 0;

	public virtual string GetLocalName(in DataModelXPathNavigator.Node node) => string.Empty;

	public string GetName(in DataModelXPathNavigator.Node node)
	{
		var prefix = GetPrefix(node);

		return string.IsNullOrEmpty(prefix) ? GetLocalName(node) : prefix + @":" + GetLocalName(node);
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