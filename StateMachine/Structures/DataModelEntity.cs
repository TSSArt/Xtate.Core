using System.Collections.Immutable;

namespace Xtate
{
	public struct DataModelEntity : IDataModel, IVisitorEntity<DataModelEntity, IDataModel>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IDataModel

		public ImmutableArray<IData> Data { get; set; }

	#endregion

	#region Interface IVisitorEntity<DataModelEntity,IDataModel>

		void IVisitorEntity<DataModelEntity, IDataModel>.Init(IDataModel source)
		{
			Ancestor = source;
			Data = source.Data;
		}

		bool IVisitorEntity<DataModelEntity, IDataModel>.RefEquals(ref DataModelEntity other) => Data == other.Data;

	#endregion
	}
}