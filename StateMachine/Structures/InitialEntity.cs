namespace Xtate
{
	public struct InitialEntity : IInitial, IVisitorEntity<InitialEntity, IInitial>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IInitial

		public ITransition? Transition { get; set; }

	#endregion

	#region Interface IVisitorEntity<InitialEntity,IInitial>

		void IVisitorEntity<InitialEntity, IInitial>.Init(IInitial source)
		{
			Ancestor = source;

			Transition = source.Transition;
		}

		bool IVisitorEntity<InitialEntity, IInitial>.RefEquals(ref InitialEntity other) => ReferenceEquals(Transition, other.Transition);

	#endregion
	}
}