using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.DataModel.None
{
	internal sealed class NoneConditionExpressionEvaluator : IConditionExpression, IBooleanEvaluator, IAncestorProvider, IDebugEntityId
	{
		private readonly ConditionExpression _conditionExpression;
		private readonly IIdentifier         _inState;

		public NoneConditionExpressionEvaluator(in ConditionExpression conditionExpression, IIdentifier inState)
		{
			_conditionExpression = conditionExpression;
			_inState = inState;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _conditionExpression.Ancestor;

	#endregion

	#region Interface IBooleanEvaluator

		ValueTask<bool> IBooleanEvaluator.EvaluateBoolean(IExecutionContext executionContext, CancellationToken token) => new ValueTask<bool>(executionContext.InState(_inState));

	#endregion

	#region Interface IConditionExpression

		public string? Expression => _conditionExpression.Expression;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{_inState}";

	#endregion
	}
}