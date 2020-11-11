#region Copyright © 2019-2020 Sergii Artemenko

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

using System;
using System.Buffers;
using System.Xml;
using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal class ElementNodeAdapter : NodeAdapter
	{
		public override XPathNodeType GetNodeType() => XPathNodeType.Element;

		public override bool IsEmptyElement(in DataModelXPathNavigator.Node node) => !GetFirstChild(node, out _);

		public override string GetLocalName(in DataModelXPathNavigator.Node node) => XmlConvert.EncodeLocalName(node.ParentProperty) ?? string.Empty;

		public override bool GetFirstChild(in DataModelXPathNavigator.Node node, out DataModelXPathNavigator.Node childNode)
		{
			childNode = new DataModelXPathNavigator.Node(value: default, default!);

			return GetNextChild(node, ref childNode);
		}

		public override string GetValue(in DataModelXPathNavigator.Node node)
		{
			var bufferSize = GetBufferSizeForValue(node);

			if (bufferSize == 0)
			{
				return string.Empty;
			}

			if (bufferSize <= 32768)
			{
				Span<char> buf = stackalloc char[bufferSize];

				var length = WriteValueToSpan(node, buf);

				return buf[..length].ToString();
			}

			var array = ArrayPool<char>.Shared.Rent(bufferSize);
			try
			{
				var length = WriteValueToSpan(node, array);

				return array.AsSpan(start: 0, length).ToString();
			}
			finally
			{
				ArrayPool<char>.Shared.Return(array);
			}
		}

		public sealed override int GetBufferSizeForValue(in DataModelXPathNavigator.Node node)
		{
			var count = 0;

			for (var ok = GetFirstChild(node, out var child); ok; ok = GetNextChild(node, ref child))
			{
				count += child.Adapter.GetBufferSizeForValue(child);
			}

			return count;
		}

		public sealed override int WriteValueToSpan(in DataModelXPathNavigator.Node node, in Span<char> span)
		{
			var count = 0;
			var buf = span;

			for (var ok = GetFirstChild(node, out var child); ok; ok = GetNextChild(node, ref child))
			{
				var length = child.Adapter.WriteValueToSpan(child, buf);
				buf = buf[length..];
				count += length;
			}

			return count;
		}
	}
}