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
using System.Collections.Immutable;
using Xtate.Core;

namespace Xtate.Builder
{
	public class FinalBuilder : BuilderBase, IFinalBuilder
	{
		private IDoneData?                        _doneData;
		private IIdentifier?                      _id;
		private ImmutableArray<IOnEntry>.Builder? _onEntryList;
		private ImmutableArray<IOnExit>.Builder?  _onExitList;

		public FinalBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IFinalBuilder

		public IFinal Build() =>
				new FinalEntity { Ancestor = Ancestor, Id = _id, OnEntry = _onEntryList?.ToImmutable() ?? default, OnExit = _onExitList?.ToImmutable() ?? default, DoneData = _doneData };

		public void SetId(IIdentifier id) => _id = id ?? throw new ArgumentNullException(nameof(id));

		public void AddOnEntry(IOnEntry onEntry)
		{
			if (onEntry is null) throw new ArgumentNullException(nameof(onEntry));

			(_onEntryList ??= ImmutableArray.CreateBuilder<IOnEntry>()).Add(onEntry);
		}

		public void AddOnExit(IOnExit onExit)
		{
			if (onExit is null) throw new ArgumentNullException(nameof(onExit));

			(_onExitList ??= ImmutableArray.CreateBuilder<IOnExit>()).Add(onExit);
		}

		public void SetDoneData(IDoneData doneData) => _doneData = doneData ?? throw new ArgumentNullException(nameof(doneData));

	#endregion
	}
}