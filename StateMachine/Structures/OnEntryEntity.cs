using System.Collections.Immutable;

namespace Xtate
{
	public struct OnEntryEntity : IOnEntry, IVisitorEntity<OnEntryEntity, IOnEntry>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IOnEntry

		public ImmutableArray<IExecutableEntity> Action { get; set; }

	#endregion

	#region Interface IVisitorEntity<OnEntryEntity,IOnEntry>

		void IVisitorEntity<OnEntryEntity, IOnEntry>.Init(IOnEntry source)
		{
			Ancestor = source;
			Action = source.Action;
		}

		bool IVisitorEntity<OnEntryEntity, IOnEntry>.RefEquals(ref OnEntryEntity other) => Action == other.Action;

	#endregion
	}
}