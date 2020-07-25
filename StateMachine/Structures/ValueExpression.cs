namespace Xtate
{
	public struct ValueExpression : IValueExpression, IVisitorEntity<ValueExpression, IValueExpression>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IValueExpression

		public string? Expression { get; set; }

	#endregion

	#region Interface IVisitorEntity<ValueExpression,IValueExpression>

		void IVisitorEntity<ValueExpression, IValueExpression>.Init(IValueExpression source)
		{
			Ancestor = source;
			Expression = source.Expression;
		}

		bool IVisitorEntity<ValueExpression, IValueExpression>.RefEquals(ref ValueExpression other) => ReferenceEquals(Expression, other.Expression);

	#endregion
	}
}