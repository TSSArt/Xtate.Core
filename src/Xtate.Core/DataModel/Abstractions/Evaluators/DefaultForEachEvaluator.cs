<<<<<<< Updated upstream
﻿#region Copyright © 2019-2023 Sergii Artemenko

=======
﻿// Copyright © 2019-2024 Sergii Artemenko
// 
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
#endregion

using System.Collections.Immutable;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel;

public abstract class ForEachEvaluator : IForEach, IExecEvaluator, IAncestorProvider
{
	private readonly IForEach _forEach;

	protected ForEachEvaluator(IForEach forEach)
	{
		Infra.Requires(forEach);

		_forEach = forEach;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _forEach;
=======
namespace Xtate.DataModel;

public abstract class ForEachEvaluator(IForEach forEach) : IForEach, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => forEach;
>>>>>>> Stashed changes

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface IForEach

<<<<<<< Updated upstream
	public virtual IValueExpression?                 Array  => _forEach.Array;
	public virtual ILocationExpression?              Item   => _forEach.Item;
	public virtual ILocationExpression?              Index  => _forEach.Index;
	public virtual ImmutableArray<IExecutableEntity> Action => _forEach.Action;
=======
	public virtual IValueExpression?                 Array  => forEach.Array;
	public virtual ILocationExpression?              Item   => forEach.Item;
	public virtual ILocationExpression?              Index  => forEach.Index;
	public virtual ImmutableArray<IExecutableEntity> Action => forEach.Action;
>>>>>>> Stashed changes

#endregion
}

public class DefaultForEachEvaluator : ForEachEvaluator
{
<<<<<<< Updated upstream
	public DefaultForEachEvaluator(IForEach forEach) : base(forEach)
	{
		Infra.NotNull(forEach.Array);
		Infra.NotNull(forEach.Item);

		ArrayEvaluator = forEach.Array.As<IArrayEvaluator>();
		ItemEvaluator = forEach.Item.As<ILocationEvaluator>();
		IndexEvaluator = forEach.Index?.As<ILocationEvaluator>();
		ActionEvaluatorList = forEach.Action.AsArrayOf<IExecutableEntity, IExecEvaluator>();
	}

	public IArrayEvaluator                ArrayEvaluator      { get; }
	public ILocationEvaluator             ItemEvaluator       { get; }
	public ILocationEvaluator?            IndexEvaluator      { get; }
	public ImmutableArray<IExecEvaluator> ActionEvaluatorList { get; }

	public override async ValueTask Execute()
	{
		var array = await ArrayEvaluator.EvaluateArray().ConfigureAwait(false);

		for (var i = 0; i < array.Length; i ++)
		{
			var instance = array[i];

			await ItemEvaluator.SetValue(instance).ConfigureAwait(false);

			if (IndexEvaluator is not null)
			{
				await IndexEvaluator.SetValue(new DefaultObject(i)).ConfigureAwait(false);
			}

			foreach (var execEvaluator in ActionEvaluatorList)
			{
				await execEvaluator.Execute().ConfigureAwait(false);
			}
=======
	private static readonly IObject[] Indexes = new IObject[16];

	private readonly ImmutableArray<IExecEvaluator> _actionEvaluatorList;
	private readonly IArrayEvaluator                _arrayEvaluator;
	private readonly ILocationEvaluator?            _indexEvaluator;
	private readonly ILocationEvaluator             _itemEvaluator;

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
>>>>>>> Stashed changes
		}
	}
}