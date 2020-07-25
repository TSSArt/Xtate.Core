namespace Xtate
{
	public struct ElseIfEntity : IElseIf, IVisitorEntity<ElseIfEntity, IElseIf>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IElseIf

		public IConditionExpression? Condition { get; set; }

	#endregion

	#region Interface IVisitorEntity<ElseIfEntity,IElseIf>

		void IVisitorEntity<ElseIfEntity, IElseIf>.Init(IElseIf source)
		{
			Ancestor = source;
			Condition = source.Condition;
		}

		bool IVisitorEntity<ElseIfEntity, IElseIf>.RefEquals(ref ElseIfEntity other) => ReferenceEquals(Condition, other.Condition);

	#endregion
	}
}