using System.Collections.Immutable;

namespace Xtate
{
	public struct DoneDataEntity : IDoneData, IVisitorEntity<DoneDataEntity, IDoneData>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IDoneData

		public IContent?              Content    { get; set; }
		public ImmutableArray<IParam> Parameters { get; set; }

	#endregion

	#region Interface IVisitorEntity<DoneDataEntity,IDoneData>

		void IVisitorEntity<DoneDataEntity, IDoneData>.Init(IDoneData source)
		{
			Ancestor = source;
			Content = source.Content;
			Parameters = source.Parameters;
		}

		bool IVisitorEntity<DoneDataEntity, IDoneData>.RefEquals(ref DoneDataEntity other) =>
				ReferenceEquals(Content, other.Content) &&
				Parameters == other.Parameters;

	#endregion
	}
}