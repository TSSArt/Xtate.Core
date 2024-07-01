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

internal class XPathSingleElementIterator(XPathNavigator navigator) : XPathNodeIterator
{
	private bool           _completed;

	public override XPathNavigator? Current => _completed ? navigator : default;

	public override int CurrentPosition => _completed ? 1 : 0;

	public override XPathNodeIterator Clone() => new XPathSingleElementIterator(navigator.Clone());

	public override bool MoveNext()
	{
		var completed = _completed;
		_completed = true;

<<<<<<< Updated upstream
		public XPathSingleElementIterator(XPathNavigator navigator) => _navigator = navigator;

		public override XPathNavigator? Current => _completed  ? _navigator : default;

		public override int CurrentPosition => _completed ? 1 : 0;

		public override XPathNodeIterator Clone() => new XPathSingleElementIterator(_navigator.Clone());

		public override bool MoveNext()
		{
			var completed = _completed;
			_completed = true;

			return !completed;
		}
=======
		return !completed;
>>>>>>> Stashed changes
	}
}