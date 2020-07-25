using System.Collections.Immutable;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class DataModelNode : IDataModel, IStoreSupport, IAncestorProvider, IDocumentId
	{
		private readonly DataModelEntity  _dataModel;
		private          DocumentIdRecord _documentIdNode;

		public DataModelNode(in DocumentIdRecord documentIdNode, in DataModelEntity dataModel)
		{
			_documentIdNode = documentIdNode;
			_dataModel = dataModel;
			Data = dataModel.Data.AsArrayOf<IData, DataNode>(true);
		}

		public ImmutableArray<DataNode> Data { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _dataModel.Ancestor;

	#endregion

	#region Interface IDataModel

		ImmutableArray<IData> IDataModel.Data => _dataModel.Data;

	#endregion

	#region Interface IDocumentId

		public int DocumentId => _documentIdNode.Value;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.DataModelNode);
			bucket.AddEntityList(Key.DataList, Data);
		}

	#endregion
	}
}