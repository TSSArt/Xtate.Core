namespace Xtate
{
	public struct ParamEntity : IParam, IVisitorEntity<ParamEntity, IParam>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IParam

		public IValueExpression?    Expression { get; set; }
		public ILocationExpression? Location   { get; set; }
		public string?              Name       { get; set; }

	#endregion

	#region Interface IVisitorEntity<ParamEntity,IParam>

		void IVisitorEntity<ParamEntity, IParam>.Init(IParam source)
		{
			Ancestor = source;
			Expression = source.Expression;
			Location = source.Location;
			Name = source.Name;
		}

		bool IVisitorEntity<ParamEntity, IParam>.RefEquals(ref ParamEntity other) =>
				ReferenceEquals(Expression, other.Expression) &&
				ReferenceEquals(Location, other.Location) &&
				ReferenceEquals(Name, other.Name);

	#endregion
	}
}