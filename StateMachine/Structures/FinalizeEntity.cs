using System.Collections.Immutable;

namespace Xtate
{
	public struct FinalizeEntity : IFinalize, IVisitorEntity<FinalizeEntity, IFinalize>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IFinalize

		public ImmutableArray<IExecutableEntity> Action { get; set; }

	#endregion

	#region Interface IVisitorEntity<FinalizeEntity,IFinalize>

		void IVisitorEntity<FinalizeEntity, IFinalize>.Init(IFinalize source)
		{
			Ancestor = source;
			Action = source.Action;
		}

		bool IVisitorEntity<FinalizeEntity, IFinalize>.RefEquals(ref FinalizeEntity other) => Action == other.Action;

	#endregion
	}
}