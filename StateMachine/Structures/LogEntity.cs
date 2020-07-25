namespace Xtate
{
	public struct LogEntity : ILog, IVisitorEntity<LogEntity, ILog>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface ILog

		public IValueExpression? Expression { get; set; }
		public string?           Label      { get; set; }

	#endregion

	#region Interface IVisitorEntity<LogEntity,ILog>

		void IVisitorEntity<LogEntity, ILog>.Init(ILog source)
		{
			Ancestor = source;
			Expression = source.Expression;
			Label = source.Label;
		}

		bool IVisitorEntity<LogEntity, ILog>.RefEquals(ref LogEntity other) =>
				ReferenceEquals(Expression, other.Expression) &&
				ReferenceEquals(Label, other.Label);

	#endregion
	}
}