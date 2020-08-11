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

namespace Xtate.Builder
{
	public class DoneDataBuilder : BuilderBase, IDoneDataBuilder
	{
		private IContent?                       _content;
		private ImmutableArray<IParam>.Builder? _parameters;

		public DoneDataBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IDoneDataBuilder

		public IDoneData Build() => new DoneDataEntity { Ancestor = Ancestor, Content = _content, Parameters = _parameters?.ToImmutable() ?? default };

		public void SetContent(IContent content) => _content = content ?? throw new ArgumentNullException(nameof(content));

		public void AddParameter(IParam parameter)
		{
			if (parameter is null) throw new ArgumentNullException(nameof(parameter));

			(_parameters ??= ImmutableArray.CreateBuilder<IParam>()).Add(parameter);
		}

	#endregion
	}
}