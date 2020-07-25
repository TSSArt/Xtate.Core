using Xtate.Persistence;

namespace Xtate
{
	internal sealed class ValueExpressionNode : IValueExpression, IStoreSupport, IAncestorProvider
	{
		private readonly ValueExpression _valueExpression;

		public ValueExpressionNode(in ValueExpression valueExpression) => _valueExpression = valueExpression;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _valueExpression.Ancestor;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ValueExpressionNode);
			bucket.Add(Key.Expression, Expression);
		}

	#endregion

	#region Interface IValueExpression

		public string? Expression => _valueExpression.Expression;

	#endregion
	}
}