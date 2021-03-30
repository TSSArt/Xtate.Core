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
	public delegate bool Predicate(IExecutionContext executionContext);

	public delegate ValueTask<bool> PredicateTask(IExecutionContext executionContext);

	public delegate ValueTask<bool> PredicateCancellableTask(IExecutionContext executionContext, CancellationToken token);

	[PublicAPI]
	public sealed class RuntimePredicate : IExecutableEntity, IBooleanEvaluator
	{
		private readonly object _predicate;

		public RuntimePredicate(Predicate predicate) => _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

		public RuntimePredicate(PredicateTask task) => _predicate = task ?? throw new ArgumentNullException(nameof(task));

		public RuntimePredicate(PredicateCancellableTask task) => _predicate = task ?? throw new ArgumentNullException(nameof(task));

	#region Interface IBooleanEvaluator

		public async ValueTask<bool> EvaluateBoolean(IExecutionContext executionContext, CancellationToken token) =>
			_predicate switch
			{
				Predicate predicate           => predicate(executionContext),
				PredicateTask task            => await task(executionContext).ConfigureAwait(false),
				PredicateCancellableTask task => await task(executionContext, token).ConfigureAwait(false),
				_                             => Infra.Unexpected<bool>(_predicate)
			};

	#endregion
	}
}