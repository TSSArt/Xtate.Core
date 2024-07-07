// Copyright © 2019-2024 Sergii Artemenko
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

internal class ListItemNodeAdapter : ItemNodeAdapter
{
	public override bool GetNextChild(in DataModelXPathNavigator.Node parentNode, ref DataModelXPathNavigator.Node node)
	{
		var list = parentNode.DataModelValue.AsList();

		var cursor = node.ParentCursor;

		var ok = list.NextEntry(ref cursor, out var entry);
		node = ok ? new DataModelXPathNavigator.Node(entry.Value, AdapterFactory.GetItemAdapter(entry), cursor, entry.Index, entry.Key, entry.Metadata) : default;

		return ok;
	}

	public override bool GetPreviousChild(in DataModelXPathNavigator.Node parentNode, ref DataModelXPathNavigator.Node node)
	{
		var list = parentNode.DataModelValue.AsList();

		var cursor = node.ParentCursor;

		var ok = list.PreviousEntry(ref cursor, out var entry);
		node = ok ? new DataModelXPathNavigator.Node(entry.Value, AdapterFactory.GetItemAdapter(entry), cursor, entry.Index, entry.Key, entry.Metadata) : default;

		return ok;
	}
}