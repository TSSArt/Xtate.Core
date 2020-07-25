using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultRaiseEvaluator : IRaise, IExecEvaluator, IAncestorProvider
	{
		private readonly RaiseEntity _raise;

		public DefaultRaiseEvaluator(in RaiseEntity raise)
		{
			Infrastructure.Assert(raise.OutgoingEvent != null);

			_raise = raise;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _raise.Ancestor;

	#endregion

	#region Interface IExecEvaluator

		public virtual ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			return executionContext.Send(_raise.OutgoingEvent!, token);
		}

	#endregion

	#region Interface IRaise

		public IOutgoingEvent OutgoingEvent => _raise.OutgoingEvent!;

	#endregion
	}
}