#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;
using Xtate.DataModel;

namespace Xtate
{
	public delegate DataModelValue Evaluator(IExecutionContext executionContext);

	public delegate ValueTask<DataModelValue> EvaluatorTask(IExecutionContext executionContext);

	public delegate ValueTask<DataModelValue> EvaluatorCancellableTask(IExecutionContext executionContext, CancellationToken token);

	[PublicAPI]
	public sealed class RuntimeEvaluator : IValueExpression, IObjectEvaluator
	{
		private readonly object _evaluator;

		public RuntimeEvaluator(Evaluator evaluator) => _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));

		public RuntimeEvaluator(EvaluatorTask task) => _evaluator = task ?? throw new ArgumentNullException(nameof(task));

		public RuntimeEvaluator(EvaluatorCancellableTask task) => _evaluator = task ?? throw new ArgumentNullException(nameof(task));

	#region Interface IObjectEvaluator

		public async ValueTask<IObject> EvaluateObject(IExecutionContext executionContext, CancellationToken token) =>
			_evaluator switch
			{
				Evaluator evaluator           => new DefaultObject(evaluator(executionContext)),
				EvaluatorTask task            => new DefaultObject(await task(executionContext).ConfigureAwait(false)),
				EvaluatorCancellableTask task => new DefaultObject(await task(executionContext, token).ConfigureAwait(false)),
				_                             => Infrastructure.UnexpectedValue<IObject>(_evaluator)
			};

	#endregion

	#region Interface IValueExpression

		public string? Expression => null;

	#endregion
	}
}