using Xtate.Persistence;

namespace Xtate
{
	internal sealed class ConditionExpressionNode : IConditionExpression, IStoreSupport, IAncestorProvider
	{
		private readonly ConditionExpression _conditionExpression;

		public ConditionExpressionNode(in ConditionExpression conditionExpression) => _conditionExpression = conditionExpression;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _conditionExpression.Ancestor;

	#endregion

	#region Interface IConditionExpression

		public string? Expression => _conditionExpression.Expression;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ConditionExpressionNode);
			bucket.Add(Key.Expression, Expression);
		}

	#endregion
	}
}