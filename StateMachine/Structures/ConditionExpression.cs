namespace Xtate
{
	public struct ConditionExpression : IConditionExpression, IVisitorEntity<ConditionExpression, IConditionExpression>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IConditionExpression

		public string? Expression { get; set; }

	#endregion

	#region Interface IVisitorEntity<ConditionExpression,IConditionExpression>

		void IVisitorEntity<ConditionExpression, IConditionExpression>.Init(IConditionExpression source)
		{
			Ancestor = source;
			Expression = source.Expression;
		}

		bool IVisitorEntity<ConditionExpression, IConditionExpression>.RefEquals(ref ConditionExpression other) => ReferenceEquals(Expression, other.Expression);

	#endregion
	}
}