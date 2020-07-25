using System.Threading;
using System.Threading.Tasks;

namespace Xtate.DataModel.XPath
{
	internal class XPathConditionExpressionEvaluator : IConditionExpression, IBooleanEvaluator, IAncestorProvider
	{
		private readonly XPathCompiledExpression _compiledExpression;
		private readonly ConditionExpression     _conditionExpression;

		public XPathConditionExpressionEvaluator(ConditionExpression conditionExpression, XPathCompiledExpression compiledExpression)
		{
			_conditionExpression = conditionExpression;
			_compiledExpression = compiledExpression;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _conditionExpression.Ancestor;

	#endregion

	#region Interface IBooleanEvaluator

		ValueTask<bool> IBooleanEvaluator.EvaluateBoolean(IExecutionContext executionContext, CancellationToken token) =>
				new ValueTask<bool>(executionContext.Engine().EvalObject(_compiledExpression).AsBoolean());

	#endregion

	#region Interface IConditionExpression

		public string? Expression => _conditionExpression.Expression;

	#endregion
	}
}