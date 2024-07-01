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

namespace Xtate.Core;

public readonly struct DocumentIdNode(LinkedList<int>? list)
{
<<<<<<< Updated upstream
	public readonly struct DocumentIdNode
	{
		private readonly LinkedListNode<int> _node;

		public DocumentIdNode(LinkedList<int> list)
		{
			Infra.Requires(list);

			_node = list.AddLast(-1);
		}

		public void Discard()
=======
	private readonly LinkedListNode<int>? _node = list?.AddLast(-1) ?? default;

	public void Discard()
	{
		if (_node?.List is { } list)
>>>>>>> Stashed changes
		{
			list.Remove(_node);
		}
<<<<<<< Updated upstream

		internal void SaveToSlot(out DocumentIdSlot slot) => slot = new DocumentIdSlot(_node);
=======
>>>>>>> Stashed changes
	}

	internal void SaveToSlot(out DocumentIdSlot slot) => slot = new DocumentIdSlot(_node);
}