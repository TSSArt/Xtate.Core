namespace Xtate
{
	public struct CancelEntity : ICancel, IVisitorEntity<CancelEntity, ICancel>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface ICancel

		public string?           SendId           { get; set; }
		public IValueExpression? SendIdExpression { get; set; }

	#endregion

	#region Interface IVisitorEntity<CancelEntity,ICancel>

		void IVisitorEntity<CancelEntity, ICancel>.Init(ICancel source)
		{
			Ancestor = source;
			SendId = source.SendId;
			SendIdExpression = source.SendIdExpression;
		}

		bool IVisitorEntity<CancelEntity, ICancel>.RefEquals(ref CancelEntity other) =>
				ReferenceEquals(SendId, other.SendId) &&
				ReferenceEquals(SendIdExpression, other.SendIdExpression);

	#endregion
	}
}