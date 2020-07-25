namespace Xtate
{
	public struct LocationExpression : ILocationExpression, IVisitorEntity<LocationExpression, ILocationExpression>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface ILocationExpression

		public string? Expression { get; set; }

	#endregion

	#region Interface IVisitorEntity<LocationExpression,ILocationExpression>

		void IVisitorEntity<LocationExpression, ILocationExpression>.Init(ILocationExpression source)
		{
			Ancestor = source;
			Expression = source.Expression;
		}

		bool IVisitorEntity<LocationExpression, ILocationExpression>.RefEquals(ref LocationExpression other) => ReferenceEquals(Expression, other.Expression);

	#endregion
	}
}