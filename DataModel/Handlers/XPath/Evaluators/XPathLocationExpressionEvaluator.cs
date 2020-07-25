using System.Threading;
using System.Threading.Tasks;

namespace Xtate.DataModel.XPath
{
	internal class XPathLocationExpressionEvaluator : ILocationEvaluator, ILocationExpression, IAncestorProvider
	{
		private readonly XPathCompiledExpression _compiledExpression;
		private readonly LocationExpression      _locationExpression;

		public XPathLocationExpressionEvaluator(in LocationExpression locationExpression, XPathCompiledExpression compiledExpression)
		{
			_locationExpression = locationExpression;
			_compiledExpression = compiledExpression;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _locationExpression.Ancestor;

	#endregion

	#region Interface ILocationEvaluator

		public void DeclareLocalVariable(IExecutionContext executionContext) => executionContext.Engine().DeclareVariable(_compiledExpression);

		public ValueTask SetValue(IObject value, IExecutionContext executionContext, CancellationToken token)
		{
			executionContext.Engine().Assign(_compiledExpression, value);

			return default;
		}

		public ValueTask<IObject> GetValue(IExecutionContext executionContext, CancellationToken token) => new ValueTask<IObject>(executionContext.Engine().EvalObject(_compiledExpression));

		public string GetName(IExecutionContext executionContext) => executionContext.Engine().GetName(_compiledExpression);

	#endregion

	#region Interface ILocationExpression

		public string? Expression => _locationExpression.Expression;

	#endregion
	}
}