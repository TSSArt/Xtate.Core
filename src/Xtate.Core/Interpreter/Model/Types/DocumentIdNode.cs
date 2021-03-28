#region Copyright © 2019-2021 Sergii Artemenko

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

using System.Collections.Generic;

namespace Xtate.Core
{
	internal readonly struct DocumentIdNode
	{
		private readonly LinkedListNode<int> _node;

		public DocumentIdNode(LinkedList<int> list) => _node = list.AddLast(-1);

		public void Discard()
		{
			if (_node.List is { } list)
			{
				list.Remove(_node);
			}
		}

		public void SaveToSlot(out DocumentIdSlot slot) => slot = new DocumentIdSlot(_node);
	}
}