using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultCancelEvaluator : ICancel, IExecEvaluator, IAncestorProvider
	{
		private readonly CancelEntity _cancel;

		public DefaultCancelEvaluator(in CancelEntity cancel)
		{
			_cancel = cancel;
			SendIdExpressionEvaluator = cancel.SendIdExpression?.As<IStringEvaluator>();
		}

		public IStringEvaluator? SendIdExpressionEvaluator { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _cancel.Ancestor;

	#endregion

	#region Interface ICancel

		public string?           SendId           => _cancel.SendId;
		public IValueExpression? SendIdExpression => _cancel.SendIdExpression;

	#endregion

	#region Interface IExecEvaluator

		public virtual async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			var sendId = SendIdExpressionEvaluator != null ? await SendIdExpressionEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false) : SendId;

			if (string.IsNullOrEmpty(sendId))
			{
				throw new ExecutionException(Resources.Exception_SendIdIsEmpty);
			}

			await executionContext.Cancel(Xtate.SendId.FromString(sendId), token).ConfigureAwait(false);
		}

	#endregion
	}
}