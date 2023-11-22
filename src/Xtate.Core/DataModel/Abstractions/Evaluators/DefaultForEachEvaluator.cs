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

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface IForEach

	public virtual IValueExpression?                 Array  => _forEach.Array;
	public virtual ILocationExpression?              Item   => _forEach.Item;
	public virtual ILocationExpression?              Index  => _forEach.Index;
	public virtual ImmutableArray<IExecutableEntity> Action => _forEach.Action;

#endregion
}

public class DefaultForEachEvaluator : ForEachEvaluator
{
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
		}
	}
}