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

using System.Xml.XPath;

namespace Xtate.DataModel.XPath;

internal class XPathStripRootsIterator(XPathNodeIterator iterator) : XPathNodeIterator
{
	private readonly XPathNodeIterator _iterator = iterator.Clone();

	private XPathNavigator? _current;

	private int _position;

	public override XPathNavigator? Current => _current;

	public override int CurrentPosition => _position;

	public override XPathNodeIterator Clone() => new XPathStripRootsIterator(_iterator);

	public override bool MoveNext()
	{
		if (_current?.MoveToNext() == true)
		{
			_position ++;

			return true;
		}

		while (_iterator.MoveNext())
		{
			var navigator = _iterator.Current;

			if (navigator?.HasChildren == true)
			{
				_current = navigator.Clone();
				var moveToFirstChild = _current.MoveToFirstChild();

				Infra.Assert(moveToFirstChild);

				_position ++;

				return true;
			}
		}

		return false;
	}
}