using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;
using Xtate.DataModel;

namespace Xtate
{
	public delegate bool Predicate(IExecutionContext executionContext);

	public delegate ValueTask<bool> PredicateTask(IExecutionContext executionContext);

	public delegate ValueTask<bool> PredicateCancellableTask(IExecutionContext executionContext, CancellationToken token);

	[PublicAPI]
	public class RuntimePredicate : IExecutableEntity, IBooleanEvaluator
	{
		private readonly object _predicate;

		public RuntimePredicate(Predicate predicate) => _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

		public RuntimePredicate(PredicateTask task) => _predicate = task ?? throw new ArgumentNullException(nameof(task));

		public RuntimePredicate(PredicateCancellableTask task) => _predicate = task ?? throw new ArgumentNullException(nameof(task));

	#region Interface IBooleanEvaluator

		public async ValueTask<bool> EvaluateBoolean(IExecutionContext executionContext, CancellationToken token) =>
				_predicate switch
				{
						Predicate predicate => predicate(executionContext),
						PredicateTask task => await task(executionContext).ConfigureAwait(false),
						PredicateCancellableTask task => await task(executionContext, token).ConfigureAwait(false),
						_ => Infrastructure.UnexpectedValue<bool>()
				};

	#endregion
	}
}