﻿#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultForEachEvaluator : IForEach, IExecEvaluator, IAncestorProvider
	{
		private readonly IForEach _forEach;

		public DefaultForEachEvaluator(IForEach forEach)
		{
			_forEach = forEach ?? throw new ArgumentNullException(nameof(forEach));

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

	#region Interface IAncestorProvider

		object IAncestorProvider.Ancestor => _forEach;

	#endregion

	#region Interface IExecEvaluator

		public virtual async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			var array = await ArrayEvaluator.EvaluateArray(executionContext, token).ConfigureAwait(false);

			for (var i = 0; i < array.Length; i ++)
			{
				var instance = array[i];

				await ItemEvaluator.SetValue(instance, executionContext, token).ConfigureAwait(false);

				if (IndexEvaluator is not null)
				{
					await IndexEvaluator.SetValue(new DefaultObject(i), executionContext, token).ConfigureAwait(false);
				}

				foreach (var execEvaluator in ActionEvaluatorList)
				{
					await execEvaluator.Execute(executionContext, token).ConfigureAwait(false);
				}
			}
		}

	#endregion

	#region Interface IForEach

		public IValueExpression                  Array  => _forEach.Array!;
		public ILocationExpression               Item   => _forEach.Item!;
		public ILocationExpression?              Index  => _forEach.Index;
		public ImmutableArray<IExecutableEntity> Action => _forEach.Action;

	#endregion
	}
}