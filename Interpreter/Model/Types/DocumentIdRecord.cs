using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Xtate
{
	internal struct DocumentIdRecord
	{
		private LinkedListNode<int>? _node;
		private int                  _value;

		public DocumentIdRecord(LinkedList<int> list)
		{
			_node = list.AddLast(-1);
			_value = -1;
		}

		public int Value
		{
			get
			{
				var node = _node;
				if (node == null)
				{
					return _value;
				}

				var value = node.Value;

				if (value >= 0)
				{
					_node = null;
					_value = value;
				}

				return value;
			}
		}

		[Pure]
		public DocumentIdRecord After()
		{
			Infrastructure.Assert(_node != null);

			return new DocumentIdRecord
				   {
						   _node = _node.List.AddAfter(_node, value: -1),
						   _value = -1
				   };
		}
	}
}