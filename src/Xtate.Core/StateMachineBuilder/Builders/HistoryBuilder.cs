#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System;

namespace Xtate.Builder
{
	public class HistoryBuilder : BuilderBase, IHistoryBuilder
	{
		private IIdentifier? _id;
		private ITransition? _transition;
		private HistoryType  _type;

		public HistoryBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IHistoryBuilder

		public IHistory Build() => new HistoryEntity { Ancestor = Ancestor, Id = _id, Type = _type, Transition = _transition };

		public void SetId(IIdentifier id) => _id = id ?? throw new ArgumentNullException(nameof(id));

		public void SetType(HistoryType type) => _type = type;

		public void SetTransition(ITransition transition) => _transition = transition ?? throw new ArgumentNullException(nameof(transition));

	#endregion
	}
}