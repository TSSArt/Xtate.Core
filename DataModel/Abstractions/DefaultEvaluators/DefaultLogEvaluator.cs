using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultLogEvaluator : ILog, IExecEvaluator, IAncestorProvider
	{
		private readonly LogEntity _log;

		public DefaultLogEvaluator(in LogEntity log)
		{
			_log = log;
			ExpressionEvaluator = log.Expression?.As<IObjectEvaluator>();
		}

		public IObjectEvaluator? ExpressionEvaluator { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _log.Ancestor;

	#endregion

	#region Interface IExecEvaluator

		public virtual async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			var data = default(DataModelValue);

			if (ExpressionEvaluator != null)
			{
				var obj = await ExpressionEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false);
				data = DataModelValue.FromObject(obj.ToObject()).AsConstant();
			}

			await executionContext.Log(_log.Label, data, token).ConfigureAwait(false);
		}

	#endregion

	#region Interface ILog

		public IValueExpression? Expression => _log.Expression;

		public string? Label => _log.Label;

	#endregion
	}
}