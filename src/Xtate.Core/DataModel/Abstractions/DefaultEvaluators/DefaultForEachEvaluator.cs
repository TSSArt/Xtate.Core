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

namespace Xtate.DataModel;

public class DefaultForEachEvaluator : ForEachEvaluator
{
	private static readonly IObject[] Indexes = new IObject[16];

	private readonly ImmutableArray<IExecEvaluator> _actionEvaluatorList;

	private readonly IArrayEvaluator _arrayEvaluator;

	private readonly ILocationEvaluator? _indexEvaluator;

	private readonly ILocationEvaluator _itemEvaluator;

	public DefaultForEachEvaluator(IForEach forEach) : base(forEach)
	{
		var arrayEvaluator = base.Array?.As<IArrayEvaluator>();
		Infra.NotNull(arrayEvaluator);
		_arrayEvaluator = arrayEvaluator;

		var itemEvaluator = base.Item?.As<ILocationEvaluator>();
		Infra.NotNull(itemEvaluator);
		_itemEvaluator = itemEvaluator;

		_actionEvaluatorList = base.Action.AsArrayOf<IExecutableEntity, IExecEvaluator>(true);
		_indexEvaluator = base.Index?.As<ILocationEvaluator>();
	}

	public override async ValueTask Execute()
	{
		var array = await _arrayEvaluator.EvaluateArray().ConfigureAwait(false);

		for (var i = 0; i < array.Length; i ++)
		{
			await ProcessItem(array[i], i).ConfigureAwait(false);
		}
	}

	protected virtual async ValueTask ProcessItem(IObject instance, int index)
	{
		await _itemEvaluator.SetValue(instance).ConfigureAwait(false);

		if (_indexEvaluator is not null)
		{
			var indexObject = index < Indexes.Length ? Indexes[index] ??= new DefaultObject(index) : new DefaultObject(index);

			await _indexEvaluator.SetValue(indexObject).ConfigureAwait(false);
		}

		await DoItemActions().ConfigureAwait(false);
	}

	protected virtual async ValueTask DoItemActions()
	{
		foreach (var execEvaluator in _actionEvaluatorList)
		{
			await execEvaluator.Execute().ConfigureAwait(false);
		}
	}
}