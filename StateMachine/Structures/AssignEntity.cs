namespace Xtate
{
	public struct AssignEntity : IAssign, IVisitorEntity<AssignEntity, IAssign>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IAssign

		public ILocationExpression? Location      { get; set; }
		public IValueExpression?    Expression    { get; set; }
		public IInlineContent?      InlineContent { get; set; }
		public string?              Type          { get; set; }
		public string?              Attribute     { get; set; }

	#endregion

	#region Interface IVisitorEntity<AssignEntity,IAssign>

		void IVisitorEntity<AssignEntity, IAssign>.Init(IAssign source)
		{
			Ancestor = source;
			Location = source.Location;
			InlineContent = source.InlineContent;
			Expression = source.Expression;
			Type = source.Type;
			Attribute = source.Attribute;
		}

		bool IVisitorEntity<AssignEntity, IAssign>.RefEquals(ref AssignEntity other) =>
				ReferenceEquals(Location, other.Location) &&
				ReferenceEquals(Expression, other.Expression) &&
				ReferenceEquals(InlineContent, other.InlineContent) &&
				ReferenceEquals(Type, other.Type) &&
				ReferenceEquals(Attribute, other.Attribute);

	#endregion
	}
}