using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal class ElementNodeAdapter : NodeAdapter
	{
		public override XPathNodeType GetNodeType(in DataModelXPathNavigator.Node node) => XPathNodeType.Element;

		public override bool IsEmptyElement(in DataModelXPathNavigator.Node node) => !GetFirstChild(node, out _);

		public override string GetLocalName(in DataModelXPathNavigator.Node node) => node.ParentProperty ?? string.Empty;

		public override bool GetFirstChild(in DataModelXPathNavigator.Node node, out DataModelXPathNavigator.Node childNode)
		{
			childNode = new DataModelXPathNavigator.Node(value: default, default!);

			return GetNextChild(node, ref childNode);
		}

		[SuppressMessage(category: "ReSharper", checkId: "SuggestVarOrType_Elsewhere")]
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

				return buf.Slice(start: 0, length).ToString();
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
				buf = buf.Slice(length);
				count += length;
			}

			return count;
		}
	}
}