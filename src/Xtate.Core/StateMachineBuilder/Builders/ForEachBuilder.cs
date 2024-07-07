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

namespace Xtate.Builder;

public class ForEachBuilder : BuilderBase, IForEachBuilder
{
	private ImmutableArray<IExecutableEntity>.Builder? _actions;
	private IValueExpression?                          _array;
	private ILocationExpression?                       _index;
	private ILocationExpression?                       _item;

#region Interface IForEachBuilder

	public IForEach Build() => new ForEachEntity { Ancestor = Ancestor, Array = _array, Item = _item, Index = _index, Action = _actions?.ToImmutable() ?? default };

	public void SetArray(IValueExpression array)
	{
		Infra.Requires(array);

		_array = array;
	}

	public void SetItem(ILocationExpression item)
	{
		Infra.Requires(item);

		_item = item;
	}

	public void SetIndex(ILocationExpression index)
	{
		Infra.Requires(index);

		_index = index;
	}

	public void AddAction(IExecutableEntity action)
	{
		Infra.Requires(action);

		(_actions ??= ImmutableArray.CreateBuilder<IExecutableEntity>()).Add(action);
	}

#endregion
}