namespace Xtate
{
	public struct ContentEntity : IContent, IVisitorEntity<ContentEntity, IContent>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IContent

		public IValueExpression? Expression { get; set; }
		public IContentBody?     Body       { get; set; }

	#endregion

	#region Interface IVisitorEntity<ContentEntity,IContent>

		void IVisitorEntity<ContentEntity, IContent>.Init(IContent source)
		{
			Ancestor = source;
			Expression = source.Expression;
			Body = source.Body;
		}

		bool IVisitorEntity<ContentEntity, IContent>.RefEquals(ref ContentEntity other) =>
				ReferenceEquals(Expression, other.Expression) &&
				ReferenceEquals(Body, other.Body);

	#endregion
	}
}