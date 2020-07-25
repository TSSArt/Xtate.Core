using System.Collections.Immutable;

namespace Xtate
{
	public struct OnExitEntity : IOnExit, IVisitorEntity<OnExitEntity, IOnExit>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IOnExit

		public ImmutableArray<IExecutableEntity> Action { get; set; }

	#endregion

	#region Interface IVisitorEntity<OnExitEntity,IOnExit>

		void IVisitorEntity<OnExitEntity, IOnExit>.Init(IOnExit source)
		{
			Ancestor = source;
			Action = source.Action;
		}

		bool IVisitorEntity<OnExitEntity, IOnExit>.RefEquals(ref OnExitEntity other) => Action == other.Action;

	#endregion
	}
}