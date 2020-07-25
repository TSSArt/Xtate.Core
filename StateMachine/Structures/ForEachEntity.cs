using System.Collections.Immutable;

namespace Xtate
{
	public struct ForEachEntity : IForEach, IVisitorEntity<ForEachEntity, IForEach>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IForEach

		public ImmutableArray<IExecutableEntity> Action { get; set; }
		public IValueExpression?                 Array  { get; set; }
		public ILocationExpression?              Index  { get; set; }
		public ILocationExpression?              Item   { get; set; }

	#endregion

	#region Interface IVisitorEntity<ForEachEntity,IForEach>

		void IVisitorEntity<ForEachEntity, IForEach>.Init(IForEach source)
		{
			Ancestor = source;
			Action = source.Action;
			Array = source.Array;
			Index = source.Index;
			Item = source.Item;
		}

		bool IVisitorEntity<ForEachEntity, IForEach>.RefEquals(ref ForEachEntity other) =>
				Action == other.Action &&
				ReferenceEquals(Array, other.Array) &&
				ReferenceEquals(Index, other.Index) &&
				ReferenceEquals(Item, other.Item);

	#endregion
	}
}