using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.DataModel;

namespace Xtate
{
	public delegate void ExecutableAction(IExecutionContext executionContext);

	public delegate ValueTask ExecutableTask(IExecutionContext executionContext);

	public delegate ValueTask ExecutableCancellableTask(IExecutionContext executionContext, CancellationToken token);

	public class RuntimeAction : IExecutableEntity, IExecEvaluator
	{
		private readonly object _action;

		public RuntimeAction(ExecutableAction action) => _action = action ?? throw new ArgumentNullException(nameof(action));

		public RuntimeAction(ExecutableTask task) => _action = task ?? throw new ArgumentNullException(nameof(task));

		public RuntimeAction(ExecutableCancellableTask task) => _action = task ?? throw new ArgumentNullException(nameof(task));

	#region Interface IExecEvaluator

		public async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			switch (_action)
			{
				case ExecutableAction action:
					action(executionContext);
					break;

				case ExecutableTask task:
					await task(executionContext).ConfigureAwait(false);
					break;

				case ExecutableCancellableTask task:
					await task(executionContext, token).ConfigureAwait(false);
					break;
			}
		}

	#endregion
	}
}