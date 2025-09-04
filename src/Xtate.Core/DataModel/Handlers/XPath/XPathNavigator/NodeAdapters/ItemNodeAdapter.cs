// Copyright © 2019-2025 Sergii Artemenko
// 
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

namespace Xtate.DataModel.XPath;

internal class ItemNodeAdapter : ElementNodeAdapter
{
    public override string GetLocalName(in DataModelXPathNavigator.Node node) => XmlConverter.KeyToLocalName(node.ParentProperty);

    public override string GetNamespaceUri(in DataModelXPathNavigator.Node node) =>
        XmlConverter.KeyToNamespaceOrDefault(node.ParentProperty) ??
        XPathMetadata.GetValue(node.Metadata, XPathMetadata.ElementIndex, XPathMetadata.ElementNamespaceOffset);

    public override string GetPrefix(in DataModelXPathNavigator.Node node) =>
        XmlConverter.KeyToPrefixOrDefault(node.ParentProperty) ??
        XPathMetadata.GetValue(node.Metadata, XPathMetadata.ElementIndex, XPathMetadata.ElementPrefixOffset);

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
        if (!ns && NextSysAttribute(parentNode, ref node))
        {
            return true;
        }

        if (parentNode.Metadata is not { } metadata)
        {
            node = default;

            return false;
        }

        var cursor = node.ParentCursor;

        while (metadata.NextEntry(ref cursor, out var entry))
        {
            if ((entry.Index - XPathMetadata.FirstAttributeOffset) % XPathMetadata.AttributeSegmentLength != 0)
            {
                continue;
            }

            var isNamespace = XPathMetadata.GetValue(metadata, entry.Index, XPathMetadata.AttributeNamespaceOffset) == XPathMetadata.XmlnsNamespace;

            if (isNamespace == ns)
            {
                var adapter = ns ? AdapterFactory.NamespaceNodeAdapter : AdapterFactory.AttributeNodeAdapter;
                var localName = XPathMetadata.GetValue(metadata, entry.Index, XPathMetadata.AttributeLocalNameOffset);
                var value = XPathMetadata.GetValue(metadata, entry.Index, XPathMetadata.AttributeValueOffset);
                node = new DataModelXPathNavigator.Node(value, adapter, cursor, entry.Index, localName, metadata);

                return true;
            }
        }

        node = default;

        return false;
    }

    private static bool NextSysAttribute(in DataModelXPathNavigator.Node parentNode, ref DataModelXPathNavigator.Node node)
    {
        if (UseTypeAttribute(parentNode) && node.ParentIndex == -1)
        {
            node = new DataModelXPathNavigator.Node(
                XmlConverter.GetTypeValue(parentNode.DataModelValue), AdapterFactory.TypeAttributeNodeAdapter,
                node.ParentCursor, parentIndex: -2, node.ParentProperty);

            return true;
        }

        return false;
    }

    private static bool UseTypeAttribute(in DataModelXPathNavigator.Node node) =>
        node.DataModelValue.Type switch
        {
            DataModelValueType.String    => false,
            DataModelValueType.Boolean   => true,
            DataModelValueType.DateTime  => true,
            DataModelValueType.Number    => true,
            DataModelValueType.Null      => true,
            DataModelValueType.Undefined => true,
            DataModelValueType.List      => false,
            _                            => throw Infra.Unmatched(node.DataModelValue.Type)
        };
}